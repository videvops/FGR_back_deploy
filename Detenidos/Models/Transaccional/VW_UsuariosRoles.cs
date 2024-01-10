using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Models
{
    [Table("VW_UsuariosRoles")]
    public class VW_UsuariosRoles
    {
        [Key]
        public string UserId { get; set; }
        public string  RoleName { get; set; }
    }
}
