using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Models.Catalogos
{
    [Table("CatGenero")]
    public class CatGenero
    {
        [Key]
        public int CatGeneroID { get; set; }
        public string Genero { get; set; }
        public bool Borrado { get; set; }
    }
}
