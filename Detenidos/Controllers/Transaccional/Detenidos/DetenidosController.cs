using Detenidos.Models;
using Detenidos.Models.Catalogos;
using Detenidos.Services;
using Detenidos.Utilidades;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Detenidos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class DetenidosController : ControllerBase
    {      
        private readonly DetenidoService _detenidoService;
        private readonly AuditoriaMongoService _auditoriaMongoService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly MailService _mail;
        private readonly IConfiguration _configuration;       
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string coleccion = "detenidos";
        public DetenidosController(UserManager<ApplicationUser> userManager, DetenidoService detendioService, ApplicationDbContext context, MailService mail,AuditoriaMongoService auditoriaMongoService, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
            _detenidoService = detendioService;
            _httpContextAccessor = httpContextAccessor;
            _mail = mail;
            _auditoriaMongoService = auditoriaMongoService;           
        }
        /*Operaciones para la auditoria en mongo
            * 1-Create
            * 2-Update
            * 3-Delete
            * 4-Search         
        */       
        [HttpGet]
        [Authorize(Roles = "Administrador,PFM_Administrador,PFM_Capturista,PFM_Consulta")]
        public async Task<ActionResult<List<Detenido>>> Get([FromQuery] PaginacionDTO paginacionDTO)
        {           
            List<Detenido> listaDetenidos = await _detenidoService.Get();
            string NameIdentifier = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
            ApplicationUser usuario = await _userManager.FindByIdAsync(NameIdentifier);
            List<int> listaSedes = await _context.UsuariosSeparos.Where(x => x.Borrado == false && x.Vigente == true && x.AspNetUsers_Id == usuario.Id).Select(x => x.SedeSubsedeID).ToListAsync();
           
            if (User.IsInRole("Administrador"))
            {
               
            }
            if (User.IsInRole("PFM_Administrador") || User.IsInRole("PFM_Capturista") || User.IsInRole("PFM_Consulta"))
            {
                listaDetenidos = listaDetenidos.Where(x => listaSedes.Contains(x.SedeSubsedeID) && x.Egreso==null).ToList();
            }

            double cantidad = listaDetenidos.Count;

            var headers = HttpContext.Response.Headers;
            if (headers == null) { throw new ArgumentNullException(nameof(headers));}
            headers.Add("cantidadTotalRegistros", cantidad.ToString());

            return Ok(listaDetenidos.AsQueryable().Paginar(paginacionDTO));          
        }  

        [HttpGet("GetModeloDetenido/{id:length(24)}")]
        [Authorize(Roles = "Administrador,CENAPI_Administrador,PFM_Administrador,PFM_Capturista,PFM_Consulta")]
        public async Task<ActionResult<Detenido>> GetModeloDetenido(string id)
        {
            var detenido = await _detenidoService.Get(id);
            if (detenido == null)
            {
                return NotFound();
            }            
            string NameIdentifier = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
            ApplicationUser usuario = await _userManager.FindByIdAsync(NameIdentifier);
           
            DatosUsuarioActualDTO datosUser = new();
            datosUser.Id = usuario.Id;
            datosUser.ListaSedes = null;
            datosUser.ListaRoles = null;
            datosUser.FechaEvento = new Utilerias(_configuration).GetFechaServidor();
            datosUser.Ip = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();

            detenido.Fichas = null;
            detenido.EstatusRegistro = null;
            detenido.Asignacion = null;
            detenido.Auditoria = null;
            detenido.RutaFirmaHuella = null;
            detenido.FechaActualizacionDelta = null;
            detenido.FechaAltaDelta = null;            

            DatosAuditoria datosAudit = new();
            datosAudit.Coleccion = "detenidos";
            datosAudit.IdColeccion =detenido.Id;
            datosAudit.OperacionID = 5;
            datosAudit.Operacion = "Action";
            datosAudit.Valores = "Clic en datos detenido";
            datosAudit.ValoresResultado = JsonConvert.SerializeObject(detenido);
            _auditoriaMongoService.GuardarAuditoria(datosUser, datosAudit);

            return detenido;
        }
        /*
         * Los estatus del registro son los siguientes:
         * 1-Nuevo Registro
         * 2-Visto
         * 3-Asignado
         * 4-Ficha
         */
        [HttpPost]
        [Authorize(Roles = "Administrador,PFM_Administrador,PFM_Capturista")]
        public async Task<ActionResult> Create(FormDetenido detenidoIn)
        {
            Utilerias utils = new(_configuration);
            DateTime fechaServidor = utils.GetFechaServidor();            
            string ip = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            string NameIdentifier = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
            ApplicationUser usuario = await _userManager.FindByIdAsync(NameIdentifier);

            int anio = fechaServidor.Year;
            var separo = _context.CatSeparo.Where(x => x.CatSeparoID == detenidoIn.CatSeparoID && x.Vigente == true && x.Borrado == false).FirstOrDefault();

            Detenido detenidoCreate = new();            
            detenidoCreate.AnioFolioIngreso = anio;            
            detenidoCreate.AnioFolioEstatal = anio;
            detenidoCreate.CatSeparoID = separo.CatSeparoID;
            detenidoCreate.NombreSeparo = separo.NombreSeparo;
            detenidoCreate.EsSede = separo.Sede == true ? 1 : 0;
            detenidoCreate.SedeSubsedeID = separo.SedeSubsedeID;
            detenidoCreate.NombreSedeSubsede = separo.NombreSedeSubsede;
            detenidoCreate.NombreSedeSubsedeLargo = separo.NombreSedeSubsedeLargo;
            detenidoCreate.CatEntidadFederativaID = separo.CatEntidadFederativaID;
            detenidoCreate.EntidadFederativa = separo.EntidadFederativa;
            detenidoCreate.MnemonicEdo = separo.MnemonicEdo;
            detenidoCreate.FehaHoraIngreso = detenidoIn.FechaHoraIngreso;
            detenidoCreate.NombreDetenido = detenidoIn.NombreDetenido;
            detenidoCreate.ApellidoPaternoDetenido = detenidoIn.ApellidoPaterno;
            detenidoCreate.ApellidoMaternoDetenido = detenidoIn.ApellidoMaterno;
            detenidoCreate.OtrosNombres = detenidoIn.OtrosNombres;
            detenidoCreate.Observaciones = detenidoIn.Observaciones;
            detenidoCreate.NacionalidadID = detenidoIn.NacionalidadDetenido;
            detenidoCreate.Nacionalidad = _context.CatNacionalidad.Where(x => x.CatNacionalidadID == detenidoIn.NacionalidadDetenido).Select(x => x.DescripcionNacionalidad).FirstOrDefault();
            detenidoCreate.OficioRetencion = detenidoIn.NumeroOficio;
            detenidoCreate.DependenciaDetencionID = detenidoIn.DependenciaDetencion;
            detenidoCreate.DependenciaDetencion = _context.CatAutoridadCargoDetencion.Where(x => x.CatAutoridadCargoDetencionID == detenidoIn.DependenciaDetencion).Select(x => x.NombreAutoridadCargoDetencion).FirstOrDefault(); ;
            detenidoCreate.RutaFirmaHuella = "C://";
            detenidoCreate.UsuarioID = usuario.Id;
            detenidoCreate.FechaAltaDelta = fechaServidor;
            detenidoCreate.FechaActualizacionDelta = null;

            List<Aliases> aliasCreate = new();

            if (detenidoIn.Aliases.Count > 0)
            {
                for (int i = 0; i < detenidoIn.Aliases.Count; i++)
                {
                    aliasCreate.Add(new Aliases
                    {
                        Alias = detenidoIn.Aliases[i],
                        FechaAltaDelta = fechaServidor,
                        FechaActualizacionDelta = null,
                        Borrado = 0
                    });
                }
            }
            else
            {
                aliasCreate = null;
                detenidoCreate.Aliases = aliasCreate;
            }
            detenidoCreate.Aliases = aliasCreate;         

            List<Delitos> delitosCreate = new();
            if (detenidoIn.Delitos.Count > 0)
            {
                for (int i = 0; i < detenidoIn.Delitos.Count; i++)
                {
                    //List<PFM_CatDelitoModalidadPrometheus> pfmCatDelito = utils.GetCatDelitoModalidad();
                    //int catDelitoID = detenidoIn.Delitos[i];
                    //var delito = _context.CatDelito.Where(x => x.CatDelitoID == catDelitoID).FirstOrDefault();
                    PFM_CatDelitoModalidadPrometheus catDelitoModalidad = utils.GetCatDelitoModalidadClasificacion(detenidoIn.Delitos[i]);
                    PFM_CatDelito delito = utils.GetPFMCatDelitoById(catDelitoModalidad.CatDelitoID);
                    delitosCreate.Add(new Delitos
                    {
                        CatDelitoID = catDelitoModalidad.CatDelitoID,
                        Delito = delito.Delito,
                        CatClasificacionDelitoID= catDelitoModalidad.CatClasificaDelitoID,
                        Clasificacion= catDelitoModalidad.Clasificacion,
                        FechaAltaDelta = fechaServidor,
                        FechaActualizacionDelta = null,
                        Borrado = 0
                    });;
                }
            }
            else
            {
                delitosCreate = null;
                detenidoCreate.Delitos = delitosCreate;
            }
            detenidoCreate.Delitos = delitosCreate;

            List<Asignacion> asignacionCreate = new();
            asignacionCreate = null;
            detenidoCreate.Asignacion = asignacionCreate;

            List<Egreso> egresoCreate = new();
            egresoCreate = null;
            detenidoCreate.Egreso = egresoCreate;

            List<Fichas> fichasCreate = new();
            fichasCreate = null;
            detenidoCreate.Fichas = fichasCreate;

            List<EstatusRegistro> registroEstatus = new();

            registroEstatus.Add(new EstatusRegistro
            {
                EstatusID = 1,
                Estatus = "Nuevo Registro",
                FechaEstatus = fechaServidor,
                FechaAltaDelta = fechaServidor,
                FechaActualizacionDelta = null,
                Borrado = 0
            });            
            detenidoCreate.EstatusRegistro = registroEstatus;

            List<AuditoriaDocumento> auditoriasList = new();
            string valores = JsonConvert.SerializeObject(detenidoIn);
            AuditoriaDocumento auditoria = new();
            auditoria.IdUsuario = usuario.Id;
            auditoria.Coleccion = coleccion;
            auditoria.FechaEvento = fechaServidor;
            auditoria.IdColeccion = "";
            auditoria.Valores = valores;
            auditoria.OperacionID = 1;
            auditoria.Operacion = "Create";
            auditoria.Ip =ip;

            detenidoCreate.Auditoria = Utilerias.GetAuditoriaMongoDocumento(auditoria, auditoriasList);

            detenidoCreate.Borrado = 0;
            detenidoCreate.FolioIngreso = utils.GetFolioDetenido(1);
            detenidoCreate.FolioEstatal = utils.GetFolioEstatalDetenido(separo.CatEntidadFederativaID);

            await _detenidoService.Create(detenidoCreate);
            if (detenidoCreate != null)
            {          
                string mensaje = "<!DOCTYPE html>";
                mensaje += "<html lang='es'>";
                mensaje += "<head>";
                mensaje += "<meta charset='utf-8'>";
                mensaje += "<title>REFIC</title>";
                mensaje += "</head>";
                mensaje += "<body>";
                mensaje += "<div style='border-color:#1D3969;border-style:solid;'>";                           
                mensaje += "<h3 style='background-color:#1D3969;color:#FFFFFF;text-align:center;margin-top: 0;'>Sistema REFIC</h3>";              
                mensaje += "<p>Datos del registro:</p> ";
                mensaje += "<ul>";
                mensaje += @$"<li><b>Folio Nacional:</b>{detenidoCreate.FolioIngreso + "/" + detenidoCreate.AnioFolioIngreso}</li>";
                mensaje += @$"<li><b>Folio Estatal:</b>{detenidoCreate.FolioEstatal + "/" + detenidoCreate.AnioFolioEstatal}</li>";
                mensaje += @$"<li><b>Entidad Federativa:</b>{detenidoCreate.EntidadFederativa}</li>";
                mensaje += @$"<li><b>Sede o Subsede:</b>{detenidoCreate.NombreSedeSubsede}</li>  ";
                mensaje += "</ul>";
                mensaje += "</div>";
                mensaje += "</body>";
                mensaje += "</html>";

                List<CatCorreo> correos = _context.CatCorreo.Where(x => x.Borrado == false && x.Vigente == true).ToList();
                string destinatario = correos.Where(x => x.CC == false && x.Borrado==false && x.Vigente==true).Select(x=>x.Correo).FirstOrDefault();
                List<string> conCopia = correos.Where(x =>x.CC == true && x.Borrado==false && x.Vigente==true).Select(x => x.Correo).ToList();

                if (correos.Count>0)
                {                   
                    _mail.SendEmailOutlook("Nuevo Registro", mensaje,destinatario,conCopia);
                }
                return Ok(detenidoCreate);
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPut("{id:length(24)}")]
        [Authorize(Roles = "Administrador,PFM_Administrador,PFM_Capturista")]
        public async Task<ActionResult> Update(string id, EditDetenido detenidoIn)
        {
            Dictionary<string, string[]> errores = await new Validador(_context,_detenidoService).ValidaEditDetenido(id);
            if (errores.Count>0)
            {
                return BadRequest(new { errors = errores });
            }

            Utilerias utils = new(_configuration);
            DateTime fechaServidor = utils.GetFechaServidor();            
            string ip = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            string NameIdentifier = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
            ApplicationUser usuario = await _userManager.FindByIdAsync(NameIdentifier);
            string valores = "";
            string coleccion = "detenidos";          

            Detenido detenido = await _detenidoService.Get(id);
            int result = DateTime.Compare(detenido.FehaHoraIngreso.Value, detenidoIn.FehaHoraIngreso.Value);
           
            if (detenido == null)
            {
                return NotFound();
            }
            if (detenido.FehaHoraIngreso != detenidoIn.FehaHoraIngreso)
            {
                detenido.FehaHoraIngreso = detenidoIn.FehaHoraIngreso;
                valores="FechaIngreso:"+ detenidoIn.FehaHoraIngreso;
            }
            if (detenido.OficioRetencion != detenidoIn.OficioRetencion)
            {
                detenido.OficioRetencion = detenidoIn.OficioRetencion;
                valores = valores != null ? valores + ";" + "OficioRetencion:" + detenidoIn.OficioRetencion : "OficioRetencion:" + detenidoIn.OficioRetencion;
            }
            if (detenido.DependenciaDetencionID != detenidoIn.DependenciaDetencionID)
            {
                detenido.DependenciaDetencionID = detenidoIn.DependenciaDetencionID;
                detenido.DependenciaDetencion= _context.CatAutoridadCargoDetencion.Where(x => x.CatAutoridadCargoDetencionID == detenidoIn.DependenciaDetencionID).Select(x=>x.NombreAutoridadCargoDetencion).FirstOrDefault();
                valores = valores != null ? valores + ";" + "DependenciaDetencionID:" + detenidoIn.DependenciaDetencionID : "DependenciaDetencionID:" + detenidoIn.DependenciaDetencionID;
            }
            if (detenido.CatSeparoID!=detenidoIn.CatSeparoID)
            {
                var separo = _context.CatSeparo.Where(x => x.CatSeparoID == detenidoIn.CatSeparoID && x.Vigente == true && x.Borrado == false).FirstOrDefault();
                detenido.CatSeparoID = detenidoIn.CatSeparoID;
                detenido.NombreSeparo = separo.NombreSeparo;
                detenido.EsSede= separo.Sede == true ? 1 : 0;
                detenido.SedeSubsedeID = separo.SedeSubsedeID;
                detenido.NombreSedeSubsede = separo.NombreSedeSubsede;
                detenido.NombreSedeSubsedeLargo = separo.NombreSedeSubsedeLargo;
                detenido.CatEntidadFederativaID = separo.CatEntidadFederativaID;
                detenido.EntidadFederativa = separo.EntidadFederativa;
                valores = valores != null ? valores + ";" + "CatSeparoID:" + detenidoIn.CatSeparoID : "CatSeparoID:" + detenidoIn.CatSeparoID;
            }

            if (detenido.Observaciones!= detenidoIn.Observaciones)
            {
                detenido.Observaciones = detenidoIn.Observaciones;
                valores = valores != null ? valores + ";" + "Observaciones:" + detenidoIn.Observaciones : "Observaciones:" + detenidoIn.Observaciones;
            }           

            if (detenido.NacionalidadID!=detenidoIn.NacionalidadID)
            {
                detenido.NacionalidadID = detenidoIn.NacionalidadID;
                detenido.Nacionalidad = _context.CatNacionalidad.Where(x => x.CatNacionalidadID == detenidoIn.NacionalidadID).Select(x => x.DescripcionNacionalidad).FirstOrDefault();
                valores = valores != null ? valores + ";" + "NacionalidadID:" + detenidoIn.NacionalidadID : "NacionalidadID:" + detenidoIn.NacionalidadID;
            }

            if (detenido.NombreDetenido!=detenidoIn.NombreDetenido)
            {
                detenido.NombreDetenido = detenidoIn.NombreDetenido;
                valores = valores != null ? valores + ";" + "NombreDetenido:" + detenidoIn.NombreDetenido : "NombreDetenido:" + detenidoIn.NombreDetenido;
            }
            if (detenido.ApellidoPaternoDetenido!=detenidoIn.ApellidoPaternoDetenido)
            {
                detenido.ApellidoPaternoDetenido = detenidoIn.ApellidoPaternoDetenido;
                valores = valores != null ? valores + ";" + "ApellidoPaternoDetenido:" + detenidoIn.ApellidoPaternoDetenido : "ApellidoPaternoDetenido:" + detenidoIn.ApellidoPaternoDetenido;
            }
            if (detenido.ApellidoMaternoDetenido!=detenidoIn.ApellidoMaternoDetenido)
            {
                detenido.ApellidoMaternoDetenido = detenidoIn.ApellidoMaternoDetenido;
                valores = valores != null ? valores + ";" + "ApellidoMaternoDetenido:" + detenidoIn.ApellidoMaternoDetenido : "ApellidoMaternoDetenido:" + detenidoIn.ApellidoMaternoDetenido;
            }
            detenido.OtrosNombres = detenidoIn.OtrosNombres;           
           
            /*---alias---*/
            List<Aliases> listaCompletaBD= detenido.Aliases;
            List<string> listaEditIn = detenidoIn.AliasesEdit; //-->los alias que vien del fron

            bool isEqualAlias=false;
            List<string> listaAliasEnBD = null;

            //---si la lista de la listaCompletaBD y la lista listaEditIn son nulas entonces isEqualAlias==true
            if (listaCompletaBD==null&& listaEditIn.Count==0)
            {
                isEqualAlias = true;
            }
            if (listaCompletaBD==null && listaEditIn.Count>0)
            {
                isEqualAlias = false;
                //listaEditIn.RemoveAll(s => string.IsNullOrWhiteSpace(s));
                //listaAliasEnBD = detenido.Aliases.Where(x => x.Borrado == 0).Select(x => x.Alias).ToList();
            }
            if(listaCompletaBD != null)
            {
                listaEditIn.RemoveAll(s => string.IsNullOrWhiteSpace(s));
                 listaAliasEnBD = detenido.Aliases.Where(x => x.Borrado == 0).Select(x => x.Alias).ToList();

                isEqualAlias = listaAliasEnBD.All(listaEditIn.Contains) && listaEditIn.All(listaAliasEnBD.Contains);
            }

                if (!isEqualAlias)
                {
                    List<string> firstNotSecond = listaAliasEnBD.Where(i => !listaEditIn.Contains(i)).ToList();
                    List<string> secondNotFirst = listaEditIn.Where(i => !listaAliasEnBD.Contains(i)).ToList();

                    string aliasEdit = JsonConvert.SerializeObject(detenidoIn.AliasesEdit);
                    valores = valores != null ? valores + ";" + "Aliases:" + aliasEdit : "Aliases:" + aliasEdit;
                    bool cumplido = false;
                    /* 1--si listaCompletaBD ==0 y listaEditIn >0 No hay alias hay que agregar los que vienten en la listaEditIn*/
                    if (listaCompletaBD.Count == 0 && listaEditIn.Count > 0)
                    {
                        List<Aliases> aliasCreate = new();
                        for (int i = 0; i < listaEditIn.Count; i++)
                        {
                            aliasCreate.Add(new Aliases
                            {
                                Alias = listaEditIn[i],
                                FechaAltaDelta = fechaServidor,
                                FechaActualizacionDelta = null,
                                Borrado = 0
                            });
                        }
                        detenido.Aliases = aliasCreate;
                        cumplido = true;
                    }

                    /* 2--si listaEditIn==0 y listaAliasEnBD >0 Hay que borrar la listaDeAliasActuales Borrado==1 y hacer una union de  listaDeAliasActuales con listaDeBorradosLogicosBD*/
                    if (!cumplido && listaEditIn.Count == 0 && listaAliasEnBD.Count > 0)
                    {
                        List<Aliases> listaDeAliasActuales = detenido.Aliases.Where(x => x.Borrado == 0).ToList();
                        foreach (var item in listaDeAliasActuales)
                        {
                            item.Borrado = 1;
                            item.FechaActualizacionDelta = fechaServidor;
                        }
                    List<Aliases> listaDeBorradosLogicosBD = detenido.Aliases.Where(x => x.Borrado == 1).ToList();
                    detenido.Aliases = listaDeAliasActuales.Concat(listaDeBorradosLogicosBD).ToList();
                        cumplido = true;
                    }

                    /* 3-- cuando quitan de la lista del front un elemento*/
                    if (!cumplido && firstNotSecond.Count > 0 && secondNotFirst.Count == 0)
                    {
                        List<Aliases> listaBorrar = detenido.Aliases.Where(x => x.Borrado == 0 && firstNotSecond.Contains(x.Alias)).ToList();
                        List<Aliases> listaAliasSinBorrar = detenido.Aliases.Where(x => x.Borrado == 0 && listaEditIn.Contains(x.Alias)).ToList();
                        foreach (var item in listaBorrar)
                        {
                            item.Borrado = 1;
                            item.FechaActualizacionDelta = fechaServidor;
                        }
                        List<Aliases> aliasActualizado = listaBorrar.Concat(listaAliasSinBorrar).ToList();
                    List<Aliases> listaDeBorradosLogicosBD = detenido.Aliases.Where(x => x.Borrado == 1).ToList();
                    detenido.Aliases = aliasActualizado.Concat(listaDeBorradosLogicosBD).ToList();
                        cumplido = true;
                    }

                    /* 4-- cuando en la lista del front eliminas elementos y agregas nuevos*/
                    if (!cumplido && firstNotSecond.Count > 0 && secondNotFirst.Count > 0)
                    {//borrar la lista firstNotSecond y agregar la lista secondNotFirst
                        List<Aliases> listaBorrar = detenido.Aliases.Where(x => x.Borrado == 0 && firstNotSecond.Contains(x.Alias)).ToList();
                        List<Aliases> listaSinBorrar = detenido.Aliases.Where(x => x.Borrado == 0 && !firstNotSecond.Contains(x.Alias)).ToList();

                        foreach (var item in listaBorrar)
                        {
                            item.Borrado = 1;
                            item.FechaActualizacionDelta = fechaServidor;
                        }

                        List<Aliases> aliasCreate = new();
                        for (int i = 0; i < secondNotFirst.Count; i++)
                        {
                            aliasCreate.Add(new Aliases
                            {
                                Alias = secondNotFirst[i],
                                FechaAltaDelta = fechaServidor,
                                FechaActualizacionDelta = null,
                                Borrado = 0
                            });
                        }
                        List<Aliases> aliasActualizado = listaBorrar.Concat(aliasCreate).ToList();
                        List<Aliases> aliasActualizadoDos = aliasActualizado.Concat(listaSinBorrar).ToList();

                    List<Aliases> listaDeBorradosLogicosBD = detenido.Aliases.Where(x => x.Borrado == 1).ToList();
                    detenido.Aliases = aliasActualizadoDos.Concat(listaDeBorradosLogicosBD).ToList();
                        cumplido = true;
                    }

                    /* 5-- cuando en el front agregas mas elementos a la lista alias*/
                    if (!cumplido && firstNotSecond.Count == 0 && secondNotFirst.Count > 0)
                    {//agregar la lista secondNotFirst y unir con la listaCompletaBD
                        List<Aliases> aliasCreate = new();
                        for (int i = 0; i < secondNotFirst.Count; i++)
                        {
                            aliasCreate.Add(new Aliases
                            {
                                Alias = secondNotFirst[i],
                                FechaAltaDelta = fechaServidor,
                                FechaActualizacionDelta = null,
                                Borrado = 0
                            });
                        }
                        detenido.Aliases = aliasCreate.Concat(listaCompletaBD).ToList();
                        cumplido = true;
                    }
                }                        
            /*--Fin-alias--*/

            /*--Delitos--*/
            List<Delitos> listDelitosCompletaDB = detenido.Delitos.ToList();
            List<Delitos> listDelitosBorradosLogicosDB = detenido.Delitos.Where(x=>x.Borrado==1).ToList();
            List<int> listaDelitosEditIn = detenidoIn.DelitosEdit;
            List<int> listaDelitosEnDB = detenido.Delitos.Where(x=>x.Borrado==0).Select(x=>x.CatClasificacionDelitoID).ToList();

            List<int> firstNotSecondDelito = listaDelitosEnDB.Where(i => !listaDelitosEditIn.Contains(i)).ToList();
            List<int> secondNotFirstDelito = listaDelitosEditIn.Where(i => !listaDelitosEnDB.Contains(i)).ToList();            

            //bool isEqualDelitos = listaDelitosEnDB.SequenceEqual(listaDelitosEditIn); // verificar si son iguales
            bool isEqualDelitos = listaDelitosEnDB.All(listaDelitosEditIn.Contains) && listaDelitosEditIn.All(listaDelitosEnDB.Contains);

            if (!isEqualDelitos)//hay cambios en delitos
            {
                string delitosEdit = JsonConvert.SerializeObject(detenidoIn.DelitosEdit);
                valores = valores != null ? valores + ";" + "Delitos:" + delitosEdit : "Delitos:" + delitosEdit;
                bool cumplido = false;
                /* 1-- si listDelitosCompletaDB==0 y listaDelitosEditIn >0 No hay delitos en DB, hay que agregar los vienen del front en listaDelitosEditIn*/
                if (listDelitosCompletaDB.Count == 0 && listaDelitosEditIn.Count > 0)
                {
                    List<Delitos> delitosCreate = new();
                    for (int i = 0; i < listaDelitosEditIn.Count; i++)
                    {
                        //int catDelitoID = listaDelitosEditIn[i];
                        //var delito = _context.CatDelito.Where(x => x.CatDelitoID == catDelitoID).FirstOrDefault();
                        PFM_CatDelitoModalidadPrometheus catDelitoModalidad = utils.GetCatDelitoModalidadClasificacion(listaDelitosEditIn[i]);
                        PFM_CatDelito delito = utils.GetPFMCatDelitoById(catDelitoModalidad.CatDelitoID);
                        delitosCreate.Add(new Delitos
                        {
                            CatDelitoID = catDelitoModalidad.CatDelitoID,
                            Delito = delito.Delito,
                            CatClasificacionDelitoID = catDelitoModalidad.CatClasificaDelitoID,
                            Clasificacion = catDelitoModalidad.Clasificacion,
                            FechaAltaDelta = fechaServidor,
                            FechaActualizacionDelta = null,
                            Borrado = 0
                        });
                    }
                    detenido.Delitos = delitosCreate;
                    cumplido = true;
                }

                /* 2-- si lalista listaDelitosEditIn ==0 y listaDelitosEnDB >0 hay que borrar la listaDelitosActuales Borrado==1 y hacer una union  de lista aliasactuales con listDelitosBorradosLogicosDB*/              
                if (!cumplido && listaDelitosEditIn.Count==0 && listaDelitosEnDB.Count>0)
                {
                    List<Delitos> listaDelitosActuales = detenido.Delitos.Where(x => x.Borrado == 0).ToList();
                    foreach (var item in listaDelitosActuales)
                    {
                        item.Borrado = 1;
                        item.FechaActualizacionDelta = fechaServidor;
                    }
                    detenido.Delitos = listaDelitosActuales.Concat(listDelitosBorradosLogicosDB).ToList();
                    cumplido = true;
                }

                /* 3-- cuando quitan de la lista del front un elemento*/
                if (!cumplido && firstNotSecondDelito.Count > 0 && secondNotFirstDelito.Count == 0)
                {
                    List<Delitos> listaBorrar = detenido.Delitos.Where(x => x.Borrado == 0 && firstNotSecondDelito.Contains(x.CatClasificacionDelitoID)).ToList();
                    List<Delitos> listaDelitosSinBorrar = detenido.Delitos.Where(x => x.Borrado == 0 && listaDelitosEditIn.Contains(x.CatClasificacionDelitoID)).ToList();

                    foreach (var item in listaBorrar)
                    {
                        item.Borrado = 1;
                        item.FechaActualizacionDelta = fechaServidor;
                    }

                    List<Delitos> delitosActualizado = listaBorrar.Concat(listaDelitosSinBorrar).ToList();
                    detenido.Delitos = delitosActualizado.Concat(listDelitosBorradosLogicosDB).ToList();
                    cumplido = true;
                }

                /* 4-- cuando en la lista del front eliminas elementos y agregas nuevos*/
                if (!cumplido && firstNotSecondDelito.Count > 0 && secondNotFirstDelito.Count > 0)
                {//borrar la lista firstNotSecondDelito y agregar la lista secondNotFirstDelito
                    List<Delitos> listaBorrar = detenido.Delitos.Where(x => x.Borrado == 0 && firstNotSecondDelito.Contains(x.CatClasificacionDelitoID)).ToList();
                    List<Delitos> listaSinBorrar = detenido.Delitos.Where(x => x.Borrado == 0 && !firstNotSecondDelito.Contains(x.CatClasificacionDelitoID)).ToList();

                    foreach (var item in listaBorrar)
                    {
                        item.Borrado = 1;
                        item.FechaActualizacionDelta = fechaServidor;
                    }

                    List<Delitos> delitosCreate = new();                   
                    for (int i = 0; i < secondNotFirstDelito.Count; i++)
                    {
                        int catDelitoModalidadClasificacionID = secondNotFirstDelito[i];
                        //var delito = _context.CatDelito.Where(x => x.CatDelitoID == catDelitoID).FirstOrDefault();
                        PFM_CatDelitoModalidadPrometheus CatDelitoModalidad = utils.GetCatDelitoModalidadClasificacion(catDelitoModalidadClasificacionID);
                        delitosCreate.Add(new Delitos
                        {
                            CatDelitoID = CatDelitoModalidad.CatDelitoID,
                            Delito = "",
                            CatClasificacionDelitoID = CatDelitoModalidad.CatClasificaDelitoID,
                            Clasificacion = CatDelitoModalidad.Clasificacion,
                            FechaAltaDelta = fechaServidor,
                            FechaActualizacionDelta = null,
                            Borrado = 0
                        });
                    }

                    List<Delitos> delitosActualizado = listaBorrar.Concat(delitosCreate).ToList();
                    List<Delitos> delitosActualizadoDos = delitosActualizado.Concat(listaSinBorrar).ToList();

                    detenido.Delitos = delitosActualizadoDos.Concat(listDelitosBorradosLogicosDB).ToList();
                    cumplido = true;
                }

                /* 5-- cuando en el front agregas mas elementos a la lista delitos*/
                if (!cumplido && firstNotSecondDelito.Count == 0 && secondNotFirstDelito.Count > 0)
                {//agregar la lista secondNotFirst y unir con la listaCompletaBD
                    List<Delitos> delitosCreate = new();
                    for (int i = 0; i < secondNotFirstDelito.Count; i++)
                    {
                        int catDelitoModadlidadClasificacionID = secondNotFirstDelito[i];
                        PFM_CatDelitoModalidadPrometheus CatDelitoModalidad = utils.GetCatDelitoModalidadClasificacion(catDelitoModadlidadClasificacionID);
                        //var delito = _context.CatDelito.Where(x => x.CatDelitoID == catDelitoID).FirstOrDefault();
                        delitosCreate.Add(new Delitos
                        {
                            CatDelitoID = CatDelitoModalidad.CatDelitoID,
                            Delito = "",
                            CatClasificacionDelitoID = CatDelitoModalidad.CatClasificaDelitoID,
                            Clasificacion = CatDelitoModalidad.Clasificacion,
                            FechaAltaDelta = fechaServidor,
                            FechaActualizacionDelta = null,
                            Borrado = 0
                        });
                    }
                    detenido.Delitos = delitosCreate.Concat(listDelitosCompletaDB).ToList();
                    cumplido = true;
                }
            }
            /*--Fin-Delitos--*/
            detenido.FechaActualizacionDelta = fechaServidor;

            AuditoriaDocumento auditoria = new();
            auditoria.IdUsuario = usuario.Id;
            auditoria.Coleccion = coleccion;
            auditoria.FechaEvento = fechaServidor;
            auditoria.IdColeccion = detenido.Id;
            auditoria.Valores = valores;
            auditoria.OperacionID = 2;
            auditoria.Operacion = "Update";
            auditoria.Ip = ip;

            var audit = detenido.Auditoria;
            if (audit != null)
            {
                detenido.Auditoria = Utilerias.GetAuditoriaMongoDocumento(auditoria, audit.ToList());
            }
            else {
                detenido.Auditoria = Utilerias.GetAuditoriaMongoDocumento(auditoria, null);
            }           


            try
            {
               await _detenidoService.Update(id, detenido);
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }        
        }

        [HttpDelete("{id:length(24)}")]
        [Authorize(Roles = "Administrador,PFM_Administrador")]
        public async Task<ActionResult> Delete(string id)
        {           
            string NameIdentifier = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
            ApplicationUser usuario = await _userManager.FindByIdAsync(NameIdentifier);
            var detenido = await _detenidoService.Get(id);            
            if (detenido == null)
            {
                return NotFound();
            }
            AuditoriaDocumento auditoria = new();
            auditoria.IdUsuario = usuario.Id;
            auditoria.Coleccion = coleccion;
            auditoria.FechaEvento = new Utilerias(_configuration).GetFechaServidor(); ;
            auditoria.IdColeccion = detenido.Id;
            auditoria.Valores = "";
            auditoria.OperacionID = 3;
            auditoria.Operacion = "Delete";
            auditoria.Ip = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(); ;

            var audit = detenido.Auditoria;
            if (audit != null)
            {
                detenido.Auditoria = Utilerias.GetAuditoriaMongoDocumento(auditoria, audit.ToList());
            }
            else
            {
                detenido.Auditoria = Utilerias.GetAuditoriaMongoDocumento(auditoria, null);
            }
            try
            {
                _detenidoService.DeleteLogic(id,detenido);
                return NoContent();
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("Buscador")]
        [Authorize(Roles = "Administrador,CENAPI_Administrador,PFM_Administrador,PFM_Capturista")]
        public async Task<ActionResult<List<Detenido>>> Buscardor([FromForm] FormBuscador datosBuscador, [FromQuery] PaginacionDTO paginacionDTO)
        {            
            string NameIdentifier = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
            ApplicationUser usuario = await _userManager.FindByIdAsync(NameIdentifier);
            List<int> listaSedes = await _context.UsuariosSeparos.Where(x => x.Borrado == false && x.Vigente == true && x.AspNetUsers_Id == usuario.Id).Select(x => x.SedeSubsedeID).ToListAsync();
            List<string> listaRoles = await _context.VW_UsuariosRoles.Where(x => x.UserId == usuario.Id).Select(x => x.RoleName).ToListAsync();

            DatosUsuarioActualDTO datosUser = new();
            datosUser.Id = usuario.Id;
            datosUser.ListaSedes = listaSedes;
            datosUser.ListaRoles = listaRoles;
            datosUser.FechaEvento = new Utilerias(_configuration).GetFechaServidor();
            datosUser.Ip = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
                  
            var listaBuscador = await _detenidoService.SearchDetenido(datosBuscador, datosUser);          

            double cantidad = listaBuscador.Count;

            var headers = HttpContext.Response.Headers;
            if (headers == null) 
            { 
                throw new ArgumentNullException(nameof(headers)); 
            }
            headers.Add("cantidadTotalRegistros", cantidad.ToString());           

            return Ok(listaBuscador.AsQueryable().Paginar(paginacionDTO));
        }

       [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<DatosUsuarioActual> ObtenDatos()
        {
            bool esAdamin = false;
            DateTime fechaServidor = new Utilerias(_configuration).GetFechaServidor();
            string ip = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            string NameIdentifier = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
            ApplicationUser usuario = await _userManager.FindByIdAsync(NameIdentifier);           

            List<int> listaSedes =await _context.UsuariosSeparos.Where(x => x.Borrado == false && x.Vigente == true && x.AspNetUsers_Id == usuario.Id).Select(x=>x.SedeSubsedeID).ToListAsync();
            if (User.IsInRole("Administrador"))
            {
                esAdamin = true;
            }
            DatosUsuarioActual datosUser = new();           
            datosUser.EsAdmin = esAdamin;
            datosUser.Id = usuario.Id;
            datosUser.ListaSedes = listaSedes;
            datosUser.FechaEvento = fechaServidor;
            datosUser.Ip = ip;
           
            return  datosUser;
        }        

        [HttpGet("getModeloEgreso/{id:length(24)}")]
        [Authorize(Roles = "Administrador,PFM_Administrador,PFM_Capturista")]
        public async Task<ActionResult<Egreso>> GetModeloEgreso(string id)
        {
            Detenido detenido = await _detenidoService.Get(id);
            if (detenido.Egreso == null)
            {
                return NotFound();
            }
            Egreso modeloEgreso = detenido.Egreso.Where(x => x.Borrado == 0).FirstOrDefault();
            return modeloEgreso;
        }

        [HttpPost("agregarEgreso/{id:length(24)}")]
        [Authorize(Roles = "Administrador,PFM_Administrador,PFM_Capturista")]
        public async Task<ActionResult> AgregarEgreso(string id, FormEgreso egresoIn)
        {
            Detenido detenido = await _detenidoService.Get(id);
            if (detenido == null)
            {
                return NotFound();
            }
            DateTime fechaServidor = new Utilerias(_configuration).GetFechaServidor();
            string ip = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            string NameIdentifier = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
            ApplicationUser usuario = await _userManager.FindByIdAsync(NameIdentifier);

            var catMotivoEgreso = _context.CatMotivoEgreso.Where(x => x.CatMotivoEgresoID == egresoIn.MotivoEgresoID && x.Vigente == true && x.Borrado == false).FirstOrDefault();

            List<Egreso> egresoCreate = new();
            egresoCreate.Add(new Egreso
            {
                OficioEgreso = egresoIn.OficioEgreso,
                Observaciones = egresoIn.Observaciones,
                FechaHoraEgreso = egresoIn.FechaHoraEgreso,
                FechaAltaDelta = fechaServidor,
                FechaActualizacionDelta = null,
                MotivoEgresoID = catMotivoEgreso.CatMotivoEgresoID,
                MotivoEgreso = catMotivoEgreso.MotivoEgreso,
                Borrado = 0
            });        

            detenido.Egreso = egresoCreate;

            AuditoriaDocumento auditoria = new();
            auditoria.IdUsuario = usuario.Id;
            auditoria.Coleccion = coleccion;
            auditoria.FechaEvento = fechaServidor;
            auditoria.IdColeccion = detenido.Id;
            auditoria.Valores = JsonConvert.SerializeObject(egresoIn)+";"+catMotivoEgreso.MotivoEgreso;
            auditoria.OperacionID = 2;
            auditoria.Operacion = "Update";
            auditoria.Ip = ip;

            var audit = detenido.Auditoria;
            if (audit != null)
            {
                detenido.Auditoria = Utilerias.GetAuditoriaMongoDocumento(auditoria, audit.ToList());
            }
            else
            {
                detenido.Auditoria = Utilerias.GetAuditoriaMongoDocumento(auditoria, null);
            }

            try
            {
                await _detenidoService.Update(id, detenido);
                return Ok(detenido);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }     
        }

        [HttpPut("updateEgreso/{id:length(24)}")]
        [Authorize(Roles = "Administrador,PFM_Administrador,PFM_Capturista")]
        public async Task<ActionResult> UpdateEgreso(string id, FormEgreso cambiosEgresoIn)
        {
            Detenido detenido = await _detenidoService.Get(id);           
            if (detenido == null)
            {
                return NotFound();
            }
            DateTime fechaServidor = new Utilerias(_configuration).GetFechaServidor();
            var catMotivoEgreso = _context.CatMotivoEgreso.Where(x => x.CatMotivoEgresoID == cambiosEgresoIn.MotivoEgresoID && x.Vigente == true && x.Borrado == false).FirstOrDefault();
            string NameIdentifier = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
            ApplicationUser usuario = await _userManager.FindByIdAsync(NameIdentifier);
            
            List<Egreso> egresos = detenido.Egreso;
            //--por el momento solo habra un egreso -----------solo funciona para un egreso!!!
            foreach (var item in egresos)
            {                
                item.OficioEgreso = cambiosEgresoIn.OficioEgreso;
                item.Observaciones = cambiosEgresoIn.Observaciones;
                item.FechaHoraEgreso = cambiosEgresoIn.FechaHoraEgreso;
                item.FechaActualizacionDelta = fechaServidor;
                item.MotivoEgresoID = catMotivoEgreso.CatMotivoEgresoID;
                item.MotivoEgreso = catMotivoEgreso.MotivoEgreso;
            }
            detenido.Egreso = egresos;

            AuditoriaDocumento auditoria = new();
            auditoria.IdUsuario = usuario.Id;
            auditoria.Coleccion = coleccion;
            auditoria.FechaEvento = fechaServidor;
            auditoria.IdColeccion = detenido.Id;
            auditoria.Valores = JsonConvert.SerializeObject(cambiosEgresoIn) + ";" + catMotivoEgreso.MotivoEgreso;
            auditoria.OperacionID = 2;
            auditoria.Operacion = "Update";
            auditoria.Ip = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(); ;

            var audit = detenido.Auditoria;
            if (audit != null)
            {
                detenido.Auditoria = Utilerias.GetAuditoriaMongoDocumento(auditoria, audit.ToList());
            }
            else
            {
                detenido.Auditoria = Utilerias.GetAuditoriaMongoDocumento(auditoria, null);
            }

            try
            {
               await _detenidoService.Update(id, detenido);
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }      
       
        [HttpPost("agregarUbicacionesSeparos")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult> AgregarUbicacionesSeparos(string id, [FromForm] string ubicacionesSedesIn)
        {
           var usuario = _userManager.Users.Where(x => x.Id == id).FirstOrDefault();

            int[] idsUbicaciones = Array.Empty<int>();
            if (ubicacionesSedesIn != null)
            {
                idsUbicaciones = ubicacionesSedesIn.Split(',').Select(int.Parse).ToArray();
            }
            List<int> listaUbicacionesIds = new(idsUbicaciones);

            using var transaction = _context.Database.BeginTransaction();
            try
            {               
                foreach (var item in listaUbicacionesIds)
                {
                    UsuariosSeparos userSeparosCreate = new ();
                    CatSeparo dato= _context.CatSeparo.Where(x => x.SedeSubsedeID == item).FirstOrDefault();
                    userSeparosCreate.AspNetUsers_Id = usuario.Id;
                    userSeparosCreate.AspNetUsers_UserId = usuario.UserId;
                    userSeparosCreate.SedeSubsedeID = item;
                    userSeparosCreate.CatEntidadFederativaID = dato.CatEntidadFederativaID;
                    userSeparosCreate.EntidadFederativa = dato.EntidadFederativa;
                    userSeparosCreate.Vigente = dato.Vigente;
                    userSeparosCreate.Borrado = dato.Borrado;

                    _context.Add(userSeparosCreate);
                    await _context.SaveChangesAsync();
                }
                transaction.Commit();
                return NoContent();
            }
            catch (Exception e)
            {
                transaction.Rollback();
                return BadRequest(e.Message);
            }
        }
        [AllowAnonymous]
        [HttpPut("updateUbicacionesSeparos")]
        [Authorize(Roles = "Administrador")]
        public ActionResult UpdateUbicacionesSeparos(string id, [FromForm] string ubicacionesSedesIn)
        {          
            int[] idsUbicaciones = Array.Empty<int>();
            if (ubicacionesSedesIn != null)
            {
                idsUbicaciones = ubicacionesSedesIn.Split(',').Select(int.Parse).ToArray();
            }
            List<int> listaUbicacionesIdsIn = new(idsUbicaciones);
            List<int> ubicacionesBdUsuario =  _context.UsuariosSeparos.Where(x => x.AspNetUsers_Id == id && x.Borrado == false && x.Vigente == true).Select(x => x.SedeSubsedeID).ToList();

            bool isEqualUbicaciones = ubicacionesBdUsuario.All(listaUbicacionesIdsIn.Contains) && listaUbicacionesIdsIn.All(ubicacionesBdUsuario.Contains);
            
            if (!isEqualUbicaciones)
            {
                List<int> firstNotSecondUbicacion = ubicacionesBdUsuario.Where(i => !listaUbicacionesIdsIn.Contains(i)).ToList();
                List<int> secondNotFirstUbicacion = listaUbicacionesIdsIn.Where(i => !ubicacionesBdUsuario.Contains(i)).ToList();

                bool cumplido = false;
                /*1-- si lalista listaUbicacionesIdsIn == 0 y ubicacionesBdUsuario > 0 hay que borrar la ubicacionesBdUsuario Borrado== 1*/
                if (!cumplido && listaUbicacionesIdsIn.Count==0 && ubicacionesBdUsuario.Count>0)
                {
                    bool ok=BorrarUbicaciones(id, ubicacionesBdUsuario);
                    if (!ok)
                    {
                        return BadRequest();
                    }
                    cumplido = true;
                }
                /* 2-- cuando quitan de la lista del front un elemento*/
                if (!cumplido && firstNotSecondUbicacion.Count > 0 && secondNotFirstUbicacion.Count == 0)
                {
                  // List<int> listaBorrar = _context.UsuariosSeparos.Where(x => x.Borrado ==false && x.Vigente==true && firstNotSecondUbicacion.Contains(x.SedeSubsedeID)).Select(x=>x.SedeSubsedeID).ToList();
                     bool ok= BorrarUbicaciones(id, firstNotSecondUbicacion);
                    if (!ok)
                    {
                        return BadRequest();
                    }
                    cumplido = true;
                }

                /* 3-- cuando en la lista del front eliminas elementos y agregas nuevos*/
                if (!cumplido && firstNotSecondUbicacion.Count > 0 && secondNotFirstUbicacion.Count > 0)
                {//borrar la lista firstNotSecondUbicacion y agregar la lista secondNotFirstUbicacion                    
                       bool ok= BorrarUbicaciones(id, firstNotSecondUbicacion);
                       bool okAgregar= AgregarUbicaciones(id, secondNotFirstUbicacion);
                    if (!ok)
                    {
                        return BadRequest();
                    }
                    if (!okAgregar)
                    {
                        return BadRequest();
                    }
                    cumplido = true;
                }
                /* 4-- cuando en el front agregas mas elementos a la lista */
                if (!cumplido && firstNotSecondUbicacion.Count == 0 && secondNotFirstUbicacion.Count > 0)
                { //agregar la lista secondNotFirst
                    bool ok=AgregarUbicaciones(id, secondNotFirstUbicacion);
                    if (!ok)
                    {
                        return BadRequest();
                    }
                    cumplido = true;
                }
            }
            return NoContent();          
        }      
        [HttpGet("getEntidadesSeparosUser")]        
        public async Task<ActionResult> GetEntidadesSeparosUser()
        {
            string NameIdentifier = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
            ApplicationUser usuario = await _userManager.FindByIdAsync(NameIdentifier);

            if(User.IsInRole("Administrador"))
            {
                var entidadesFedUsers = _context.CatSeparo.Where(x => !x.Borrado && x.Vigente == true).Select(x => new
                {
                    x.CatEntidadFederativaID,
                    x.EntidadFederativa
                }).Distinct().ToList();
                return Ok(entidadesFedUsers);
            }
            else
            {
                var entidadesFedUsers = _context.UsuariosSeparos.Where(x => !x.Borrado && x.Vigente == true && x.AspNetUsers_Id == usuario.Id).
                Select(x => new
                {
                    x.CatEntidadFederativaID,
                    x.EntidadFederativa
                }).Distinct().ToList();
                return Ok(entidadesFedUsers);
            }            
        }      
        [HttpGet("getEntidadesSeparosUserActuales")]
        public ActionResult GetEntidadesSeparosUserActuales(string Id)
        {
            List<Separos> listaSeparos = new();
            var entidadesFedUsersActules = _context.UsuariosSeparos.Where(x => !x.Borrado && x.Vigente == true && x.AspNetUsers_Id == Id).
                Select(x => new
                {
                    x.CatEntidadFederativaID,
                    x.EntidadFederativa,
                    x.SedeSubsedeID,
                }).Distinct().ToList();

            for (int i = 0; i < entidadesFedUsersActules.Count; i++)
            {
                var datoSeparo = _context.CatSeparo.Where(x => x.CatEntidadFederativaID == entidadesFedUsersActules[i].CatEntidadFederativaID).Select(x => x.EntidadFederativa).FirstOrDefault();

                List<SedesSeparos> sedesSeparos = _context.CatSeparo.Where(x => x.SedeSubsedeID == entidadesFedUsersActules[i].SedeSubsedeID).
                  Select(x => new SedesSeparos
                  {
                      SedeSubsedeID = x.SedeSubsedeID,
                      NombreSedeSubsede = x.NombreSedeSubsede
                  }).Distinct().ToList();

                listaSeparos.Add(new Separos
                {
                    CatEntidadFederativaID = entidadesFedUsersActules[i].CatEntidadFederativaID,
                    EntidadFederativa = datoSeparo,
                    Sedes = sedesSeparos
                });
            }
            return Ok(listaSeparos);
        }
        private bool AgregarUbicaciones(string id, List<int> ubicacionesIn)
        {
            var usuario = _userManager.Users.Where(x => x.Id == id).FirstOrDefault();
            Utilerias utils = new Utilerias();
            string valorUrlApi = new Utilerias(_configuration).GetValorConstante("webapi_url_base");


            HttpResponseMessage responsePersonalAIC = utils.Call_WebApi(new HttpResponseMessage(),
                  "/api/SL_Usuario/GetPersonalUbicacionByPersonalID" + "?PersonalID=" + usuario.PersonalID, valorUrlApi);

            UsersAIC datosPersonalAIC = GetDatosPersonalAIC(responsePersonalAIC);

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                foreach (var item in ubicacionesIn)
                {
                    UsuariosSeparos userSeparosCreate = new();
                    CatSeparo dato = _context.CatSeparo.Where(x => x.SedeSubsedeID == item).FirstOrDefault();
                    userSeparosCreate.AspNetUsers_Id = usuario.Id;
                    userSeparosCreate.AspNetUsers_UserId = usuario.UserId;
                    userSeparosCreate.SedeSubsedeID = item;
                    userSeparosCreate.CatEntidadFederativaID = dato.CatEntidadFederativaID;
                    userSeparosCreate.EntidadFederativa = dato.EntidadFederativa;
                    userSeparosCreate.PersonalID = usuario.PersonalID;
                    userSeparosCreate.UbicacionID =datosPersonalAIC.UbicacionID;
                    userSeparosCreate.Vigente = dato.Vigente;
                    userSeparosCreate.Borrado = dato.Borrado;

                    _context.Add(userSeparosCreate);
                    _context.SaveChanges();
                }
                transaction.Commit();
                return true;
            }
            catch (Exception e)
            {
                transaction.Rollback();
                return false;
            }               
        }       

        private bool BorrarUbicaciones(string id,List<int> ubicacionesIn)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                foreach (var item in ubicacionesIn)
                {
                    UsuariosSeparos ubicacion = _context.UsuariosSeparos.Where(x => x.AspNetUsers_Id == id && x.SedeSubsedeID == item && x.Borrado == false && x.Vigente == true).FirstOrDefault();
                    ubicacion.Borrado = true;
                    _context.Entry(ubicacion).State = EntityState.Modified;
                    _context.SaveChanges();
                }
                transaction.Commit();
                return true;
            }
            catch(Exception e)
            {
                transaction.Rollback();
                return false;
            }               
        }

        private static UsersAIC GetDatosPersonalAIC(HttpResponseMessage response)
        {
            UsersAIC datosPersonal;
            if (response.IsSuccessStatusCode)
            {
                string cadenaRespuesta = response.Content.ReadAsStringAsync().Result;
                datosPersonal = JsonConvert.DeserializeObject<UsersAIC>(cadenaRespuesta);
            }
            else { throw new Exception("Error al obtener datos"); }

            return datosPersonal;
        }
       

    }
}
