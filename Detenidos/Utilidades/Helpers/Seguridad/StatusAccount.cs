using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Utilidades.Helpers
{
	public enum StatusAccount
	{
		Inactiva = 0,
		Bloqueada = 1,
		Activa = 2,
		Cancelada = 3
	}

	public enum StatusSession
	{
		User_Login = 1,
		User_Logout = 2,
		Another_User_Logout = 3
	}
}
