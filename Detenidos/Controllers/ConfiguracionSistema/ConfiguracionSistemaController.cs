using Detenidos.Models;
using Detenidos.Utilidades;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Controllers.ConfiguracionSistema
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ConfiguracionSistemaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ConfiguracionSistemaController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("agregarConfiguracionCorreo")]
        public async Task<ActionResult> AgregarConfiguracionCorreo([FromBody] ConfigCorreoDTO datosIn)
        {
            ConfigCorreo createCorreo = new();
            createCorreo.NombreServidorCorreo = datosIn.NombreServidorCorreo;
            createCorreo.Usuario = datosIn.Usuario;
            createCorreo.Password =Security.Encrypt(datosIn.Password);
            createCorreo.Host = datosIn.Host;
            createCorreo.Port = datosIn.Port;
            createCorreo.Ssl = datosIn.Ssl;
            createCorreo.Defaultcredentials = datosIn.Defaultcredentials;
            createCorreo.Vigente = true;
            createCorreo.Borrado = false;
            _context.Add(createCorreo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("agregarConfiguracionFTP")]
        public async Task<ActionResult> AgregarConfiguracionFTP([FromBody] ConfigFtpDTO datosIn)
        {
            ConfigFtp createFtp = new();
            createFtp.Usuario = datosIn.Usuario;
            createFtp.Password = Security.Encrypt(datosIn.Password);
            createFtp.Ruta = datosIn.Ruta;
            createFtp.Carpeta = datosIn.Carpeta;
            createFtp.Vigente = true;
            createFtp.Borrado = false;        
            _context.Add(createFtp);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
