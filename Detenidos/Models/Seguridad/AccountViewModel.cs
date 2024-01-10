using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Models
{
	public class LoginViewModel
	{
		[Required]
		public string UserName { get; set; }
		[Required]
		public string Password { get; set; }
		public bool RememberMe { get; set; }
	}

	/*public class AuthenticationResponse
	{
		public string Token { get; set; }
		public DateTime Expiration { get; set; }
	}*/

    public class ManageUserViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string CurrentPassowrd { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "La {0} debe ser de al menos {2} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "La nueva contraseña y su confirmación no coinciden.")]
        public string ConfirmPassword { get; set; }
    }
}
