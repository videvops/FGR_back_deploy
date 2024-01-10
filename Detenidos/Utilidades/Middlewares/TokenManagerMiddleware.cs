﻿using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Detenidos.Utilidades.Interfaces;

namespace Detenidos.Utilidades.Middlewares
{
	public class TokenManagerMiddleware : IMiddleware
	{
		private readonly ITokenManager _tokenManager;

		public TokenManagerMiddleware(ITokenManager tokenManager)
		{
			_tokenManager = tokenManager;
		}

		public async Task InvokeAsync(HttpContext context, RequestDelegate next)
		{
			//if (await _tokenManager.IsCurrentActiveToken())
			{
				await next(context);

				return;
			}

			//context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
		}
	}
}
