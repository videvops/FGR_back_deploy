using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Detenidos.Models
{
	[Table("AspNetProductRoles")]
	public class AspNetProductRoles
	{
		[Key]
		public int ProductRoleID { get; set; }
		public string RoleId { get; set; }
		public int ProductID { get; set; }
		public DateTime AsignacionInicial { get; set; }
		public DateTime? AsignacionTermino { get; set; }
		public string IP { get; set; }
		public bool Vigente { get; set; }
	}
}
