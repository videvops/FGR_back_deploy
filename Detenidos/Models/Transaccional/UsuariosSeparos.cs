using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Models
{
    [Table("UsuariosSeparos")]
    public class UsuariosSeparos
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string AspNetUsers_Id { get; set; }
        public int AspNetUsers_UserId { get; set; }    
        public int SedeSubsedeID { get; set; }
        public int CatEntidadFederativaID { get; set; }
        public string EntidadFederativa { get; set; }
        public int PersonalID { get; set; }
        public int UbicacionID { get; set; }
        public bool Vigente { get; set; }
        public bool Borrado { get; set; }
    }
}
