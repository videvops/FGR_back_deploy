using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Models
{
	public class SidenavItem
	{
		public string Name { get; set; }
		public string Icon { get; set; }
		public string RouteOrFunction { get; set; }
		public SidenavItem Parent { get; set; }
		public List<SidenavItem> SubItems { get; set; }
		public int Position { get; set; }
		public bool PathMatchExact { get; set; }
		public string Badge { get; set; }
		public string BadgeColor { get; set; }
		public string Type { get; set; }
		public string CustomClass { get; set; }
		public int Nivel { get; set; }
	}
}
