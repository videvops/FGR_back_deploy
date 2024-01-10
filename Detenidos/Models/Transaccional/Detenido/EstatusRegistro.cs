using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Models
{
    public class EstatusRegistro
    {
        public int EstatusID { get; set; }
        public string Estatus { get; set; }
        public DateTime? FechaEstatus { get; set; }
        public DateTime? FechaAltaDelta { get; set; }
        public DateTime? FechaActualizacionDelta { get; set; }
        public int Borrado { get; set; }
    }
}
