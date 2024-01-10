using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Models
{
    [Table("AspNetUsers")]
    public class AspNetUsers
    {
        [Key]
        public string Id { get; set; }
        public int UserId { get; set; }
        public int StatusAccountId { get; set; }       
    }
}
