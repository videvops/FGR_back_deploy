using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Models
{
    public class DatosUsuarioActual
    {      
        public string Id { get; set; }
        public List<int> ListaSedes  { get; set; }
        public bool EsAdmin { get; set; }
        public DateTime FechaEvento { get; set; }
        public string Ip { get; set; }
    } 

    public class DatosUsuarioActualDTO
    {
        public string Id { get; set; }
        public List<int> ListaSedes { get; set; }
        public List<string> ListaRoles { get; set; }        
        public DateTime FechaEvento { get; set; }
        public string Ip { get; set; }
    }


}
