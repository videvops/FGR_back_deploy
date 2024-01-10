using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Models
{
    public class Egreso
    {
        public string OficioEgreso { get; set; }
        public string Observaciones { get; set; }
        public DateTime? FechaHoraEgreso { get; set; }
        public DateTime? FechaAltaDelta { get; set; }
        public DateTime? FechaActualizacionDelta { get; set; }
        public int MotivoEgresoID { get; set; }
        public string MotivoEgreso { get; set; }
        public int Borrado { get; set; }
    }
}
