using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Models
{
    public class Separos
    {
        public int CatEntidadFederativaID { get; set;}
        public string EntidadFederativa { get; set; }
        public List<SedesSeparos>Sedes { get; set; }
    }

  
}
