using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Models
{
    [Table("ConfigCorreo")]
    public class ConfigCorreo
    {
        [Key]
        public int Id { get; set; }
        public string NombreServidorCorreo { get; set; }
        public string Usuario { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public bool Ssl { get; set; }
        public bool Defaultcredentials { get; set; }
        public bool Vigente { get; set; }
        public bool Borrado { get; set; }
    }

    public class ConfigCorreoDTO
    {
        public string NombreServidorCorreo { get; set; }
        public string Usuario { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public bool Ssl { get; set; }
        public bool Defaultcredentials { get; set; }
    }


}
