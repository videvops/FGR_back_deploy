using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Detenidos.Models
{
	[Table("CatStatusAccount")]
	public class CatStatusAccount
	{
		[Key]
		public int StatusAccountId { get; set; }
		public string StatusAccount { get; set; }
	}

	public class CatStatusAccountDTO
	{
		public int StatusAccountId { get; set; }
		public string StatusAccount { get; set; }
	}
}
