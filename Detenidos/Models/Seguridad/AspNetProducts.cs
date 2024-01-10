using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Detenidos.Models
{
	[Table("AspNetProducts")]
	public class AspNetProducts
	{
		[Key]
		public int ProductID { get; set; }
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
		public bool Production { get; set; }
	}

	public class ProductsDTO
	{
		[Key]
		public int ProductID { get; set; }
		public int MenuID { get; set; }
		public string Name { get; set; }
		public bool Selected { get; set; }
	}

	public class Products
	{
		[Key]
		public int ProductID { get; set; }
		public int MenuID { get; set; }
	}
}
