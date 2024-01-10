using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Models.Catalogos
{
    [Table("CatAutoridadCargoDetencion")]
    public class CatAutoridadCargoDetencion
    {
        [Key]
        public int CatAutoridadCargoDetencionID { get; set; }
        public string NombreAutoridadCargoDetencion { get; set; }
        public bool Vigente { get; set; }
        public bool Borrado { get; set; }
    }
}
