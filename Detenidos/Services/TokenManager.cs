using Detenidos.Utilidades.Interfaces;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Detenidos.Models;
using Microsoft.Extensions.Primitives;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Detenidos.Utilidades
{
	public class TokenManager : ITokenManager
	{
		private readonly IDistributedCache _cache;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly IOptions<JwtOptions> _jwtOptions;

		public TokenManager(IDistributedCache cache, IHttpContextAccessor httpContextAccessor, IOptions<JwtOptions> jwtOptions)
		{
			_cache = cache;
			_httpContextAccessor = httpContextAccessor;
			_jwtOptions = jwtOptions;
		}

		public async Task<bool> IsCurrentActiveToken() /**/
			=> await IsActiveAsync(/*GetCurrentAsync()*/);

		public async Task DeactivateCurrentAsync() /**/
			=> await DeactivateAsync(GetCurrentAsync());

		public async Task ActivateCurrentAsync(string userName, string jwt)
		{
			await _cache.SetStringAsync(userName, jwt,
				new DistributedCacheEntryOptions
				{
					AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_jwtOptions.Value.ExpiryMinutes)
				});
		}

		/*public async Task<bool> IsActiveAsync(string token) / ** /
			=> await _cache.GetStringAsync(GetKey(token)) == null;*/
		public async Task<bool> IsActiveAsync() /**/
		{
			bool statusJwt = true;
			var authorizationHeader = _httpContextAccessor.HttpContext.Request.Headers["authorization"];
			if (authorizationHeader != StringValues.Empty)
			{
				string redisJwt = "", requestJwt = "";
				try
				{
					requestJwt = authorizationHeader.Single().Split(" ").Last();
					JwtSecurityToken token = new JwtSecurityTokenHandler().ReadJwtToken(requestJwt);
					Claim user = token.Claims.Where(x => x.Type == ClaimTypes.Name).FirstOrDefault();
					string UserName = user.Value;
					redisJwt = await _cache.GetStringAsync(UserName);
				}
				catch { }

				return redisJwt == requestJwt;
			}

			return statusJwt;
		}

		public async Task DeactivateAsync(string token) /**/
			=> await _cache.SetStringAsync(GetKey(token), " ", 
				new DistributedCacheEntryOptions
				{
					AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_jwtOptions.Value.ExpiryMinutes)
				});

		private string GetCurrentAsync()
		{
			var authorizationHeader = _httpContextAccessor.HttpContext.Request.Headers["authorization"];
			return authorizationHeader == StringValues.Empty ? string.Empty : authorizationHeader.Single().Split(" ").Last();
		}

		private static string GetKey(string token)
			=> $"tokens:{token}:deactivated";
	}
}
