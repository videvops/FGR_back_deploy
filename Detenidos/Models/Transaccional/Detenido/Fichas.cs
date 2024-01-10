using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Models
{
    public class Fichas
    {
        public string UsuarioAlta { get; set; }
        public string NombreUsuarioAlta { get; set; }
        public string RutaFicha { get; set; }
        public string NombreTrabajoFicha { get; set; }
        public int PersonalID { get; set; }
        public string NombreArchivo { get; set; }
        public string NombreUnico { get; set; }      
        public string Hash { get; set; }
        public string Observaciones { get; set; }
        public string Ip { get; set; }
        public DateTime? FechaAltaDelta { get; set; }
        public DateTime? FechaActualizacionDelta { get; set; }
        public int Borrado { get; set; }        
        public string TipoArchivo { get; set; }    
    }

    public class ArchivoFichaDTO
    {
        public string Id { get; set; }
        public IFormFile Archivo { get; set; }
        public string NombreTrabajoFicha { get; set; }
        public int PersonalID { get; set; }
        public string Observaciones { get; set; }
    }
    public class FichaDTO
    {
        public string NombreArchivo { get; set; }
        public string TipoArchivo { get; set; }
        public string RutaFicha { get; set; }
        public string Cadena64 { get; set; }

    }
}
