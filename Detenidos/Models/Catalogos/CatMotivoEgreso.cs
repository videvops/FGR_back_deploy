using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Models.Catalogos
{
    [Table("CatMotivoEgreso")]
    public class CatMotivoEgreso
    {
        [Key]
        public int CatMotivoEgresoID { get; set; }
        public string MotivoEgreso { get; set; }
        public bool Vigente { get; set; }
        public bool Borrado { get; set; }
    }
}
