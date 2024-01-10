using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Detenidos.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Detenidos.Models.Catalogos;
using Detenidos.Utilidades;
using System.Net.Http;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace Detenidos.Controllers
{
    [Route("api/catalogos")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    
    public class CatalogosController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IConfiguration _configuration;

        public CatalogosController(ApplicationDbContext context, IMapper mapper, IConfiguration configuration)
        {
            this.context = context;
            this.mapper = mapper;
            _configuration = configuration;
        }

        [HttpGet("catAutoridadCargoDetencion")]
        public async Task<ActionResult<List<CatAutoridadCargoDetencion>>> GetAutoridadCargoDetencion()
        {
            List<CatAutoridadCargoDetencion> catalogo = await context.CatAutoridadCargoDetencion.Where(a => !a.Borrado && a.Vigente==true).ToListAsync();
            return mapper.Map<List<CatAutoridadCargoDetencion>>(catalogo);
        }

        [HttpGet("catNacionalidad")]
        public async Task<ActionResult<List<CatNacionalidad>>> GetNacionalidad()
        {
            List<CatNacionalidad> catalogo = await context.CatNacionalidad.Where(a => !a.Borrado).OrderBy(a => a.DescripcionNacionalidad).ToListAsync();
            return mapper.Map<List<CatNacionalidad>>(catalogo);
        }
        
        [HttpGet("catGenero")]
        public async Task<ActionResult<List<CatGenero>>> GetSexo()
        {
            List<CatGenero> catalogo = await context.CatGenero.Where(a => !a.Borrado).ToListAsync();
            return mapper.Map<List<CatGenero>>(catalogo);
        }

        [HttpGet("catDelito")]
        public async Task<ActionResult<List<CatDelito>>> GetDelito()
        {
            List<CatDelito> catalogo = await context.CatDelito.Where(a => !a.Borrado && a.Vigente==true).ToListAsync();
            return mapper.Map<List<CatDelito>>(catalogo);
        }

        [HttpGet("catMotivoEgreso")]
        public async Task<ActionResult<List<CatMotivoEgreso>>> GetMotivoEgreso()
        {
            List<CatMotivoEgreso> catalogo = await context.CatMotivoEgreso.Where(a => !a.Borrado && a.Vigente == true).ToListAsync();
            return mapper.Map<List<CatMotivoEgreso>>(catalogo);
        }       
       
        [HttpGet("catArbolSeparos")]
        public async Task<ActionResult<List<Separos>>> GetArbolSeparos()
        {
            List<Separos> listaSeparos = new();                

            List<int> IdsEntidadFederativa =await context.CatSeparo.Where(a => !a.Borrado && a.Vigente == true).Select(x => x.CatEntidadFederativaID).Distinct().ToListAsync();

            for (int i=0;i<IdsEntidadFederativa.Count;i++)
            {
                var datoSeparo = context.CatSeparo.Where(x => x.CatEntidadFederativaID == IdsEntidadFederativa[i]).Select(x=>x.EntidadFederativa).FirstOrDefault();

                List<SedesSeparos> sedesSeparos = context.CatSeparo.Where(x => x.CatEntidadFederativaID == IdsEntidadFederativa[i]).
                Select(x=>new SedesSeparos
                {SedeSubsedeID=x.SedeSubsedeID,
                NombreSedeSubsede=x.NombreSedeSubsede
                }).Distinct().ToList();

                listaSeparos.Add(new Separos
                {
                    CatEntidadFederativaID = IdsEntidadFederativa[i],
                    EntidadFederativa =datoSeparo,
                    Sedes=sedesSeparos                    
                });
            }           
            return Ok(listaSeparos);
        }
      
        [HttpGet("getEntidadFedSeparos")]
        public async Task<ActionResult<List<EntidadFedSeparos>>> GetEntidadFedSeparos()
        {
            List<int> IdsEntidadFederativa = await context.CatSeparo.Where(a => !a.Borrado && a.Vigente == true).Select(x => x.CatEntidadFederativaID).Distinct().ToListAsync();

            List<EntidadFedSeparos> catEntidadesFedSeparos =await context.CatSeparo.Where(a => !a.Borrado && a.Vigente == true).
              Select(x => new EntidadFedSeparos
              {
                  CatEntidadFederativaID = x.CatEntidadFederativaID,
                  EntidadFederativa = x.EntidadFederativa
              }).Distinct().ToListAsync();

            return Ok(catEntidadesFedSeparos);
        }

        [HttpGet("getSedesSubsedesSeparos/{Id}")]
        public async Task<ActionResult<List<SedesSeparos>>> GetSedesSubsedesSeparos(int Id)
        {
            List<SedesSeparos> sedesSeparos = await context.CatSeparo.Where(x => x.CatEntidadFederativaID == Id && !x.Borrado && x.Vigente == true).
               Select(x => new SedesSeparos
               {
                   SedeSubsedeID = x.SedeSubsedeID,
                   NombreSedeSubsede = x.NombreSedeSubsede
               }).Distinct().ToListAsync();

            return Ok(sedesSeparos);
        }

        [HttpGet("getCatSeparos/{Id}")]
        public async Task<ActionResult> GetCatSeparos(int Id)
        {
           var catSeparoSedes =await context.CatSeparo.Where(x => x.SedeSubsedeID == Id && !x.Borrado && x.Vigente == true).
                Select(x => new 
            {
              x.CatSeparoID,
              x.NombreSeparo
            }).ToListAsync();

            return Ok(catSeparoSedes);
        }

        // Seguridad
        [HttpGet("catStatusAccount")]
        public async Task<ActionResult<List<CatStatusAccountDTO>>> GetStatusAccount()
        {
            List<CatStatusAccount> status = await context.CatStatusAccount.Where(x=>x.StatusAccountId>0).ToListAsync();
            return mapper.Map<List<CatStatusAccountDTO>>(status);
        }

        [HttpGet("getCatUsuariosAIC/{terminoBusqueda}")]
        public ActionResult getCatUsuariosAIC(string terminoBusqueda)
        {
            try
            {
                Utilerias utils = new();
                string valorUrlApi = new Utilerias(_configuration).GetValorConstante("webapi_url_base");

                HttpResponseMessage responsePersonalAIC = utils.Call_WebApi(new HttpResponseMessage(),
                      "api/SL_Catalogos/BuscarPersonal" + "?nombreCompleto=" + terminoBusqueda, valorUrlApi);

               List<CatUSersAIC> catPersonalAIC = GetCatPersonalAIC(responsePersonalAIC);
                return Ok(catPersonalAIC);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        private static List<CatUSersAIC> GetCatPersonalAIC(HttpResponseMessage response)
        {
           List<CatUSersAIC> datosPersonal;
            if (response.IsSuccessStatusCode)
            {
                string cadenaRespuesta = response.Content.ReadAsStringAsync().Result;
                datosPersonal = JsonConvert.DeserializeObject<List<CatUSersAIC>>(cadenaRespuesta);
            }
            else { throw new Exception("Error al obtener datos"); }

            return datosPersonal;
        }
     [HttpGet("getPFMCatDelito")]
        [AllowAnonymous]
        public ActionResult GetPFMCatDelito()
        {
            try
            {
                Utilerias utils = new(_configuration);
                List<PFM_CatDelito> pfmCatDelito = utils.GetPFMCatDelito();
                return Ok(pfmCatDelito);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("getPFMCatDelitoByID/{Id}")]
        [AllowAnonymous]
        public ActionResult GetPFMCatDelitoById(int Id)
        {
            try
            {
                Utilerias utils = new(_configuration);
                PFM_CatDelito pfmCatDelito = utils.GetPFMCatDelitoById(Id);
                return Ok(pfmCatDelito);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("getCatDelitoModalidad/{Id}")]
        [AllowAnonymous]
        public ActionResult GetCatDelitoModalidad(int Id)
        {
            try
            {
                Utilerias utils = new(_configuration);               
                List<PFM_CatDelitoModalidadPrometheus> catDelitoModalidad = utils.GetCatDelitoModalidad(Id);
                return Ok(catDelitoModalidad);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("getCatDelitoModalidadByClasificacion/{Id}")]
        [AllowAnonymous]
        public ActionResult GetCatDelitoModalidadByClasificacion(int Id)
        {
            try
            {
                Utilerias utils = new(_configuration);              
                PFM_CatDelitoModalidadPrometheus catDelitoModalidad = utils.GetCatDelitoModalidadClasificacion(Id);
                return Ok(catDelitoModalidad);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }



    }
}
