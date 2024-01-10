using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Models.Catalogos
{
    [Table("CatDelito")]
    public class CatDelito
    {
        [Key]
        public int CatDelitoID { get; set; }
        public string Delito { get; set; }
        public bool Vigente { get; set; }
        public bool Borrado { get; set; }
    }
}
