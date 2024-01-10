using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Models
{
	[Table("VW_Products")]
	public class VW_Products
	{
		public int Total { get; set; }
		public string UserId { get; set; }
		[Key]
		public int MenuID { get; set; }
		public string Name { get; set; }
		public string Icon { get; set; }
		public string RouteOrFunction { get; set; }
		public int Position { get; set; }
		public bool PathMatchExact { get; set; }
		public string Badge { get; set; }
		public string BadgeColor { get; set; }
		public string Type { get; set; }
		public string CustomClass { get; set; }
	}
}
