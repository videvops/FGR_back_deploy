using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Detenidos.Models
{
	[Table("AspNetMenu")]
	public class AspNetMenu
	{
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

	public class ItemsMenuDTO
	{
		public int MenuID { get; set; }
		public string Name { get; set; }
	}
}
