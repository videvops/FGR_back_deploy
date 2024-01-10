using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Models
{
    public class Delitos
    {
        public int CatDelitoID { get; set; }
        public string Delito { get; set; }
        public int CatClasificacionDelitoID { get; set; }
        public string Clasificacion { get; set; }
        public DateTime? FechaAltaDelta { get; set; }
        public DateTime? FechaActualizacionDelta { get; set; }
        public int Borrado { get; set; }
    }
}
