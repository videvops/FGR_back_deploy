using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Models
{
    public class FormEgreso
    {
        public int MotivoEgresoID { get; set; }
        public string OficioEgreso { get; set;}
        public DateTime FechaHoraEgreso { get; set; }        
        public string Observaciones { get; set; }
    }
}
