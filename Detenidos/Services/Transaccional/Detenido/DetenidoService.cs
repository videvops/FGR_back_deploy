using Detenidos.Models;
using Detenidos.Utilidades;
using LinqKit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Services
{
    public class DetenidoService
    {
        private readonly IMongoCollection<Detenido> _detenidos;
        private readonly IMongoDatabaseSettings _settings;
        private readonly AuditoriaMongoService _auditoriaMongoService;
        private readonly ApplicationDbContext _context;

        public DetenidoService(IMongoDatabaseSettings settings, AuditoriaMongoService auditoriaMongoService, ApplicationDbContext context)
        {
            var client = new MongoClient(settings.MongoConectionString);
            var database = client.GetDatabase(settings.MongoDatabaseName);

            _detenidos = database.GetCollection<Detenido>("detenidos");
            _auditoriaMongoService = auditoriaMongoService;
            _settings = settings;
            _context = context;
        }

        public async Task<List<Detenido>> Get()
        {            
            List<Detenido> listaDetenido= await _detenidos.Find(x => x.Borrado == 0).SortByDescending(x=>x.Id).ToListAsync();
            return listaDetenido;
        }
        public async Task<Detenido> Get(string id)
        {
            var detenido = await _detenidos.Find(x => x.Borrado == 0 && x.Id == id).FirstOrDefaultAsync();
            return detenido;
        }

        public async Task<Detenido> Create(Detenido detenidoCreate)
        {
            //var client = new MongoClient(_settings.MongoConectionString);

            //using (var session = await client.StartSessionAsync())
            //{
            //    // Begin transaction
            //    session.StartTransaction();
            //    try
            //    {
            //        await _detenidos.InsertOneAsync(session, detenidoCreate);
            //        // Made it here without error? Let's commit the transaction
            //        await session.CommitTransactionAsync();
            //        return detenidoCreate;
            //    }
            //    catch (Exception e)
            //    {
            //        await session.AbortTransactionAsync();
            //        detenidoCreate = null;
            //        return detenidoCreate;
            //    }           
            //}

            try
            {
                await _detenidos.InsertOneAsync(detenidoCreate);
                return detenidoCreate;
            }
            catch (Exception e)
            {
                detenidoCreate = null;
                return detenidoCreate;
            }
        }


        public async Task<Detenido> Update(string id, Detenido detenidoIn)
        {
           await _detenidos.ReplaceOneAsync(detenido => detenido.Id == id, detenidoIn);
            return detenidoIn;
        }       
        public void DeleteLogic(string id,Detenido detenidoIn)
        {
            detenidoIn.Borrado = 1;
            _detenidos.ReplaceOne(detenido => detenido.Id == id, detenidoIn);
            //   var filter = Builders<Detenido>.Filter.Eq("Id", id);
            //   var update = Builders<Detenido>.Update.Set("Borrado", 1);
            //_detenidos.UpdateOne(filter, update);
        }

        public async Task< List<Detenido>> SearchDetenido(FormBuscador formDetenido, DatosUsuarioActualDTO datosUser)
        {             
            string terminosBusqueda = null;

            var predicate = PredicateBuilder.New<Detenido>();

            if (datosUser.ListaRoles.Contains("Administrador")|| datosUser.ListaRoles.Contains("CENAPI_Administrador"))
            {
                predicate = predicate.And(x => x.Borrado == 0);
            }
            else
            {
                predicate = predicate.And(x => x.Borrado == 0 && datosUser.ListaSedes.Contains(x.SedeSubsedeID));
            }

            if (formDetenido.FechaIngresoInicial !=null && formDetenido.FechaIngresoFinal !=null)
            {
                predicate = predicate.And(x => x.FehaHoraIngreso >=formDetenido.FechaIngresoInicial && x.FehaHoraIngreso <=formDetenido.FechaIngresoFinal);                
                terminosBusqueda = "FechaIngresoInicial:"+ formDetenido.FechaIngresoInicial+";"+"FechaIngresoFinal:"+formDetenido.FechaIngresoFinal;
            }
            if (formDetenido.NumeroDetenido !=null)
            {
                predicate = predicate.And(x=>x.FolioIngreso== formDetenido.NumeroDetenido);
                terminosBusqueda = terminosBusqueda!=null ? terminosBusqueda+";"+ "FolioIngreso:" + formDetenido.NumeroDetenido : "FolioIngreso:" + formDetenido.NumeroDetenido;
            }
            if (formDetenido.Anio != null)
            {
                predicate = predicate.And(x => x.AnioFolioIngreso == formDetenido.Anio);
                terminosBusqueda = terminosBusqueda != null ? terminosBusqueda + ";" + "AnioFolioIngreso:" + formDetenido.Anio : "AnioFolioIngreso:" + formDetenido.Anio;
            }
            if (!string.IsNullOrEmpty(formDetenido.Nombre))
            {
                predicate = predicate.And(x => x.NombreDetenido.ToLower().Contains(formDetenido.Nombre.ToLower()));
                terminosBusqueda = terminosBusqueda != null ? terminosBusqueda + ";" + "NombreDetenido:" + formDetenido.Nombre : "NombreDetenido:" + formDetenido.Nombre;
            }
            if (!string.IsNullOrEmpty(formDetenido.APaterno))
            {
                predicate = predicate.And(x => x.ApellidoPaternoDetenido.ToLower().Contains(formDetenido.APaterno.ToLower()));
                terminosBusqueda = terminosBusqueda != null ? terminosBusqueda + ";" + "ApellidoPaternoDetenido:" + formDetenido.APaterno : "ApellidoPaternoDetenido:" + formDetenido.APaterno;
            }
            if (!string.IsNullOrEmpty(formDetenido.AMaterno))
            {
                predicate = predicate.And(x => x.ApellidoMaternoDetenido.ToLower().Contains(formDetenido.AMaterno.ToLower()));
                terminosBusqueda = terminosBusqueda != null ? terminosBusqueda + ";" + "ApellidoMaternoDetenido:" + formDetenido.AMaterno : "ApellidoMaternoDetenido:" + formDetenido.AMaterno;
            }            
            if (formDetenido.NacionalidadID !=null)
            {
                predicate = predicate.And(x => x.NacionalidadID== formDetenido.NacionalidadID);
                var nacionalidad = _context.CatNacionalidad.Where(x => x.CatNacionalidadID == formDetenido.NacionalidadID).Select(x => x.DescripcionNacionalidad).FirstOrDefault();

                terminosBusqueda = terminosBusqueda != null ? terminosBusqueda + ";" + "NacionalidadID:" + formDetenido.NacionalidadID+";"+"Nacionalidad:"+nacionalidad : "NacionalidadID:" + formDetenido.NacionalidadID+";"+"Nacionalidad:"+nacionalidad;
            }
            if (!string.IsNullOrEmpty(formDetenido.OficioRetencion))
            {
                predicate = predicate.And(x => x.OficioRetencion.ToLower().Contains(formDetenido.OficioRetencion.ToLower()));
                terminosBusqueda = terminosBusqueda != null ? terminosBusqueda + ";" + "OficioRetencion:" + formDetenido.OficioRetencion : "OficioRetencion:" + formDetenido.OficioRetencion;
            }
            if (formDetenido.DependenciaDetencionID !=null)
            {
                predicate = predicate.And(x => x.DependenciaDetencionID == formDetenido.DependenciaDetencionID);
                var dependencia = _context.CatAutoridadCargoDetencion.Where(x => x.CatAutoridadCargoDetencionID == formDetenido.DependenciaDetencionID).Select(x => x.NombreAutoridadCargoDetencion).FirstOrDefault();
                terminosBusqueda = terminosBusqueda != null ? terminosBusqueda + ";" + "DependenciaDetencionID:" + formDetenido.DependenciaDetencionID+";"+ "NombreAutoridadCargoDetencion:"+dependencia : "DependenciaDetencionID:" + formDetenido.DependenciaDetencionID+";"+ "NombreAutoridadCargoDetencion:"+dependencia;
            }
            if (!string.IsNullOrEmpty(formDetenido.Alias))
            {
                predicate = predicate.And(x => x.Aliases.Any(x => x.Alias.ToLower().Contains(formDetenido.Alias.ToLower()) && x.Borrado==0));
                terminosBusqueda = terminosBusqueda != null ? terminosBusqueda + ";" + "Aliases:" + formDetenido.Alias : "Aliases:" + formDetenido.Alias;
            }
            if (formDetenido.CatDelitoID != null)
            {
                predicate = predicate.And(x=>x.Delitos.Any(x=>x.CatDelitoID==formDetenido.CatDelitoID && x.Borrado==0));
                var delito = _context.CatDelito.Where(x => x.CatDelitoID == formDetenido.CatDelitoID).Select(x => x.Delito).FirstOrDefault();
                terminosBusqueda = terminosBusqueda != null ? terminosBusqueda + ";" + "DelitoID:" + formDetenido.CatDelitoID+";"+"Delito:"+delito : "DelitoID:" + formDetenido.CatDelitoID+";"+"Delito:"+delito;
            }
            if (formDetenido.FechaEgresoInicial !=null && formDetenido.FechaEgresoFinal !=null)
            {
                predicate = predicate.And(x=>x.Egreso.Any(x=>x.Borrado==0 && x.FechaHoraEgreso >= formDetenido.FechaEgresoInicial && x.FechaHoraEgreso <=formDetenido.FechaEgresoFinal));
                terminosBusqueda = terminosBusqueda != null ? terminosBusqueda + ";" + "FechaEgresoInicial:" + formDetenido.FechaEgresoInicial+";" +"FechaEgresoFinal:"+formDetenido.FechaEgresoFinal : "FechaEgresoInicial:"+formDetenido.FechaEgresoInicial+";" +"FechaEgresoFinal:"+formDetenido.FechaEgresoFinal;
            }
            if (formDetenido.MotivoEgresoID !=null)
            {
                predicate = predicate.And(x => x.Egreso.Any(x => x.Borrado == 0 && x.MotivoEgresoID==formDetenido.MotivoEgresoID));
                var motivoEgreso = _context.CatMotivoEgreso.Where(x => x.CatMotivoEgresoID == formDetenido.MotivoEgresoID).Select(x => x.MotivoEgreso).FirstOrDefault();
                terminosBusqueda = terminosBusqueda != null ? terminosBusqueda + ";" + "MotivoEgresoID:" + formDetenido.MotivoEgresoID+";" +"MotivoEgreso:"+motivoEgreso : "MotivoEgresoID:" + formDetenido.MotivoEgresoID+";" +"MotivoEgreso:"+motivoEgreso;
            }           
            if (formDetenido.Ficha==true)
            {
                predicate = predicate.And(x=>x.Fichas.Any(x=>x.Borrado==0));
                terminosBusqueda = terminosBusqueda != null ? terminosBusqueda + ";" + "Fichas:" + formDetenido.Ficha : "Fichas:" + formDetenido.Ficha;
            }
            List<Detenido> detenidos = await _detenidos.Find(predicate).ToListAsync();

            DatosAuditoria datosAudit = new();
            datosAudit.Coleccion = "detenidos";
            datosAudit.IdColeccion = "";
            datosAudit.OperacionID = 4;
            datosAudit.Operacion = "Search";
            datosAudit.Valores = terminosBusqueda;
            datosAudit.ValoresResultado = detenidos.Count.ToString();
            _auditoriaMongoService.GuardarAuditoria(datosUser, datosAudit);

            return detenidos;
        }

    }
}
