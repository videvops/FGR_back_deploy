using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Models
{
    public class FormBuscador
    {
        public DateTime? FechaIngresoInicial { get; set; }
        public DateTime? FechaIngresoFinal { get; set; }
        public int? NumeroDetenido { get; set; }
        public int? Anio { get; set; }
        public string Nombre { get; set; }
        public string APaterno { get; set; }
        public string AMaterno { get; set; }
        public string Rfc { get; set; }
        public string Alias { get; set; }
        public int? NacionalidadID { get; set; }
        public string OficioRetencion { get; set; }
        public int? DependenciaDetencionID { get; set; }
        public int? CatDelitoID { get; set; }
        public string OficioEgreso { get; set; }
        public DateTime? FechaEgresoInicial { get; set; }
        public DateTime? FechaEgresoFinal { get; set; }
        public int? MotivoEgresoID { get; set; }
        public bool Ficha { get; set; }

    }
}
