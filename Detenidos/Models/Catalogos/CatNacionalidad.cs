using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Models.Catalogos
{
    [Table("CatNacionalidad")]
    public class CatNacionalidad
    {
        [Key]
        public int CatNacionalidadID { get; set; }
        public string DescripcionNacionalidad { get; set; }
        public bool Borrado { get; set; }
    }
}
