using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Models
{
    public class FormDetenido
    {
        public string NombreDetenido { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public int NacionalidadDetenido { get; set; }
        public List<string> Aliases { get; set; }
        public string NumeroOficio { get; set; }
        public int DependenciaDetencion { get; set; }
        public DateTime? FechaHoraIngreso { get; set; }
        public string Observaciones { get; set; }
        public List<int> Delitos { get; set; }//llegan los id de las modalidades
        public int CatSeparoID { get; set; }
       public string  OtrosNombres { get; set; }

    }
}
