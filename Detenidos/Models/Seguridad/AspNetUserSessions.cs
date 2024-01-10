using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Detenidos.Models
{
	[Table("AspNetUserSessions")]
	public class AspNetUserSessions
	{
		[Key]
		public int Id { get; set; }
		public string UserId { get; set; }
		public string IP { get; set; }
		public DateTime LoginDate { get; set; }
		public DateTime? LogoffDate { get; set; }
		public int SessionStatusId { get; set; }
	}

	public class Sessions
	{
		public string SessionID { get; set; }
	}
}
