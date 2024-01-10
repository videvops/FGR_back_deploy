using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Models.Catalogos
{
    [Table("CatSeparo")]
    public class CatSeparo
    {
        [Key]
        public int CatSeparoID { get; set; }
        public int SedeSubsedeID { get; set; }
        public string NombreSedeSubsede { get; set; }
        public string NombreSedeSubsedeLargo { get; set; }
        public bool Sede { get; set; }
        public int AreaID { get; set; }
        public string NombreSeparo { get; set; }
        public int CatEntidadFederativaID { get; set; }
        public string EntidadFederativa { get; set; }
        public string MnemonicEdo { get; set; }        
        public string EstadoID { get; set; }
        public string SeparoID { get; set; }
        public bool Vigente { get; set; }
        public bool Borrado { get; set; }
    }
}
