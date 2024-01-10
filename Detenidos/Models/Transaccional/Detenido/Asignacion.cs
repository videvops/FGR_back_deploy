using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Models
{
    public class Asignacion
    {
        public int PersonalID { get; set; }
        public string Nombre { get; set; }
        public int AdscripcionID { get; set; }
        public string Adscripcion { get; set; }
        public DateTime? FechaAltaDelta { get; set; }
        public DateTime? FechaActualizacionDelta { get; set; }
        public int Borrado { get; set; }
    }
}
