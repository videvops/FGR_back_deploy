using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Models
{
	public class AspNetUserRolesAudit
	{
		[Key]
		public int Id { get; set; }
		public string UserId { get; set; }
		public string RoleId { get; set; }
		public string RoleName { get; set; }
		public DateTime AsignacionInicial { get; set; }
		public DateTime? AsignacionTermino { get; set; }
		public string IP { get; set; }
		public bool Vigente { get; set; }
	}
}
