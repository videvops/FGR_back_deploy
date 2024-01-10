using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Models
{
    [Table("ConfigFtp")]
    public class ConfigFtp
    {
        [Key]
        public int Id { get; set; }
        public string Usuario { get; set; }
        public string Password { get; set; }
        public string Ruta { get; set; }
        public string Carpeta { get; set; }   
        public bool Vigente { get; set; }
        public bool Borrado { get; set; }
    }

    public class ConfigFtpDTO
    {
        public string Usuario { get; set; }
        public string Password { get; set; }
        public string Ruta { get; set; }
        public string Carpeta { get; set; }
    }
}
