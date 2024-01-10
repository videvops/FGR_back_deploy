using Detenidos.Models;
using Detenidos.Services;
using Detenidos.Utilidades;
using LinqKit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Detenidos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class GestionCenapiController : ControllerBase
    {
        private readonly GestionCenapiService _gestionCenapiService;
        private readonly DetenidoService _detenidoService;
        private readonly AuditoriaMongoService _auditoriaMongoService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly MailService _mail;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string coleccion = "detenidos";
        public GestionCenapiController(UserManager<ApplicationUser> userManager,GestionCenapiService gestionCenapiService, ApplicationDbContext context, MailService mail,AuditoriaMongoService auditoriaMongoService,DetenidoService detenidoService, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;         
            _mail = mail;
            _gestionCenapiService = gestionCenapiService;
            _httpContextAccessor = httpContextAccessor;
            _auditoriaMongoService = auditoriaMongoService;
            _detenidoService = detenidoService;
        }
        /*Operaciones para la auditoria en mongo
           * 1-Create
           * 2-Update
           * 3-Delete
           * 4-Search  
           * 5-Action
       */
        [HttpGet]
        [Authorize(Roles = "Administrador,CENAPI_Administrador")]
        public async Task<ActionResult<List<Detenido>>> Get([FromQuery] PaginacionDTO paginacionDTO)
        {        
            var listasolicitudes = await _gestionCenapiService.Get();

            string NameIdentifier = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
            ApplicationUser usuario = await _userManager.FindByIdAsync(NameIdentifier);

            if (User.IsInRole("Administrador") || User.IsInRole("CENAPI_Administrador"))
            {
                listasolicitudes = listasolicitudes.Where(x => x.Fichas == null).ToList();
            }
            else
            {
                listasolicitudes = listasolicitudes.Where(x =>x.Asignacion.Any(x=>x.Borrado==0 && x.PersonalID==usuario.PersonalID) && x.EstatusRegistro.Any(x=>x.Borrado==0 && x.EstatusID!=4)).ToList();
            }

            double cantidad = listasolicitudes.Count;
            var headers = HttpContext.Response.Headers;
            if (headers == null) { throw new ArgumentNullException(nameof(headers)); }
            headers.Add("cantidadTotalRegistros", cantidad.ToString());

            return Ok(listasolicitudes.AsQueryable().Paginar(paginacionDTO));        
        }

        [HttpGet("GetModeloDetenido/{id:length(24)}")]       
        [Authorize(Roles = "Administrador,CENAPI_Administrador")]
        public async Task<ActionResult<Detenido>> GetModeloDetenido(string id)
        {
            var detenido = await _detenidoService.Get(id);
            if (detenido == null)
            {
                return NotFound();
            }            
            string NameIdentifier = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
            ApplicationUser usuario = await _userManager.FindByIdAsync(NameIdentifier);

            var listaEstatus = detenido.EstatusRegistro.Where(x => x.Borrado == 0 && x.EstatusID == 2).ToList();
            if (listaEstatus.Count == 0)
            {
                DateTime fechaServidor = new Utilerias(_configuration).GetFechaServidor();
                detenido.EstatusRegistro.Add(new EstatusRegistro
                {
                    EstatusID = 2,
                    Estatus = "Visto",
                    FechaEstatus = fechaServidor,
                    FechaAltaDelta = fechaServidor,
                    FechaActualizacionDelta = null,
                    Borrado = 0
                });

               await _detenidoService.Update(id, detenido);
            }
            DatosAuditoria datosAudit = new();
            datosAudit.Coleccion = "detenidos";
            datosAudit.IdColeccion = detenido.Id;
            datosAudit.OperacionID = 5;
            datosAudit.Operacion = "Action";
            datosAudit.Valores = "Clic en datos detenido";
            datosAudit.ValoresResultado = JsonConvert.SerializeObject(detenido);

            DatosUsuarioActualDTO datosUser = new();
            datosUser.Id = usuario.Id;
            datosUser.ListaSedes = null;
            datosUser.ListaRoles = null;
            datosUser.FechaEvento = new Utilerias(_configuration).GetFechaServidor();
            datosUser.Ip = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();

            _auditoriaMongoService.GuardarAuditoria( datosUser, datosAudit);
            return detenido;
        }

        [HttpPost("agregarFicha")]       
        public async Task<ActionResult> AgregarFicha([FromForm] ArchivoFichaDTO archivoFichaIn)
        {
          string NameIdentifier = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
          ApplicationUser usuario = await _userManager.FindByIdAsync(NameIdentifier);

            var detenido = await _detenidoService.Get(archivoFichaIn.Id);
            if (detenido == null)
            {
                return NotFound();
            }

            IFormFile archivoIn = archivoFichaIn.Archivo;      

            if (archivoIn!=null)
            {
                ConfigFtp datosFtp =  _context.ConfigFtp.Where(x => x.Borrado == false && x.Vigente == true).FirstOrDefault();
                datosFtp.Carpeta = datosFtp.Carpeta+ detenido.MnemonicEdo+"/"+detenido.AnioFolioIngreso+"/";
                
                Fichas Archivo = new Utilerias(_configuration).SubirArchivoFTP(archivoIn,usuario,datosFtp,detenido.Id);
                Archivo.Ip= _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();

                if (Archivo==null) //hubo una excepcion al subir el archivo!!
                {
                    return BadRequest();
                }
                List<Fichas> listaFichas = detenido.Fichas;
                if (listaFichas!=null)
                {
                    listaFichas.Add(Archivo);
                    detenido.Fichas = listaFichas;
                }
                else
                {
                    List<Fichas> listaF = new();
                    listaF.Add(Archivo);
                    detenido.Fichas = listaF;
                }

                await _detenidoService.Update(archivoFichaIn.Id, detenido);
            }            
            return NoContent();
        }

        [HttpGet("verArchivo")]
        public async Task<ActionResult<FichaDTO>> VerArchivo(string id, string nombreUnico)
        {
            string NameIdentifier = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
            ApplicationUser usuario = await _userManager.FindByIdAsync(NameIdentifier);
            var detenido = await _detenidoService.Get(id);
            if (detenido == null)
            {
                return NotFound();
            }
            var ficha = detenido.Fichas.Where(x => x.NombreUnico == nombreUnico && x.Borrado==0).FirstOrDefault();
            ConfigFtp datosFtp = _context.ConfigFtp.Where(x => x.Borrado == false && x.Vigente == true).FirstOrDefault();

            FichaDTO fichaDto = new();
            fichaDto.NombreArchivo = ficha.NombreArchivo;
            fichaDto.RutaFicha = ficha.RutaFicha;
            fichaDto.TipoArchivo = ficha.TipoArchivo;
            fichaDto.Cadena64 = "";

            Utilerias utils = new(_configuration);
            utils.DownloadFTP(fichaDto,datosFtp);

            DatosAuditoria datosAudit = new();
            datosAudit.Coleccion =coleccion;
            datosAudit.IdColeccion = detenido.Id;
            datosAudit.OperacionID = 5;
            datosAudit.Operacion = "Action";
            datosAudit.Valores = "Clic en ver ficha";
            datosAudit.ValoresResultado = ficha.NombreArchivo+";"+ficha.NombreUnico+";"+ficha.TipoArchivo+";"+ficha.RutaFicha;

            DatosUsuarioActualDTO datosUser = new();
            datosUser.Id = usuario.Id;
            datosUser.ListaSedes = null;
            datosUser.ListaRoles = null;
            datosUser.FechaEvento = new Utilerias(_configuration).GetFechaServidor();
            datosUser.Ip = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            _auditoriaMongoService.GuardarAuditoria(datosUser, datosAudit);

            return fichaDto;
        }

    }
}
