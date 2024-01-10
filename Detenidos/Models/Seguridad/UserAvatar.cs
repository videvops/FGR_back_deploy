using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Models
{
	public class UserAvatar
	{
		public int CatFiscaliaID { get; set; }
		public IFormFile Avatar { get; set; }
	}
}
