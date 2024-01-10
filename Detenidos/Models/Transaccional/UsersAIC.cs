using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Models
{
    public class UsersAIC
    {
        [Key]
        public int Id { get; set; }
        public int PersonalID { get; set; }
        public int UbicacionID { get; set; }
        public int CatDivisionID { get; set; }
        public string Nombre { get; set; }
    }
    public class CatUSersAIC
    {
        public int PersonalID { get; set; }
        public string NCompleto { get; set; }
    }
}
