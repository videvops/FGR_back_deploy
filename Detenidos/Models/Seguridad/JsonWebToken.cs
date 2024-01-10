using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Models
{
	public class JsonWebToken
	{
		public string AccessToken { get; set; }
		public string RefreshToken { get; set; }
		public DateTime Expiration { get; set; }
		//public long Expires { get; set; }
		public string SessionID { get; set; }
	}
}
