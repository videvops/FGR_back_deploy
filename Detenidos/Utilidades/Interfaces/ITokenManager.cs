using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Utilidades.Interfaces
{
	public interface ITokenManager
	{
		Task<bool> IsCurrentActiveToken();
		Task DeactivateCurrentAsync();
		Task ActivateCurrentAsync(string userName, string jwt);
		//Task<bool> IsActiveAsync(string token);
		Task<bool> IsActiveAsync();
		Task DeactivateAsync(string token);
	}
}
