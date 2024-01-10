using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Models
{
    public class PFM_CatDelito
    {
        public int CatDelitoID { get; set; }
        public string Delito { get; set; }
        public bool PericialesTransito { get; set; }
    }

    public class PFM_CatDelitoModalidadPrometheus
    {
        public int CatClasificaDelitoID { get; set; }
        public int CatDelitoID { get; set; }
        public string Clasificacion { get; set; }
        public string Descripcion { get; set; }
        public string Instrumento { get; set; }
        public string Categoria { get; set; }
        public Nullable<int> Peso { get; set; }
        public bool PericialesTransito { get; set; }
    }
}
