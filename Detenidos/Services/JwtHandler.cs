using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Detenidos.Models;
using Detenidos.Utilidades.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Detenidos.Services
{
	public class JwtHandler : IJwtHandler
	{
		private readonly JwtOptions _jwtOptions;
		private readonly SecurityKey _securityKey;
		private readonly SigningCredentials _signingCredentials;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly ApplicationDbContext _context;

		public JwtHandler(IOptions<JwtOptions> jwtOptions, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
		{
			_jwtOptions = jwtOptions.Value;
			_securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
			_signingCredentials = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256);
			_userManager = userManager;
			_context = context;
		}

		public async Task<JsonWebToken> GetToken(ApplicationUser user)
		{
			IList<string> roles = await _userManager.GetRolesAsync(user);
			//string fiscalia = _context.CatFiscalias.Find(user.CatFiscaliaID).NombreFiscalia ?? "";

			List<Claim> _claims = new()
			{
				new Claim(ClaimTypes.NameIdentifier, user.Id),
				new Claim(ClaimTypes.Name, user.UserName),
				new Claim("CatFiscaliaID", user.CatFiscaliaID.ToString()),
				new Claim("username", user.UserName),
				new Claim("friendlyname", user.FriendlyName),
				//new Claim("fiscalia", fiscalia)//,
											   //new Claim("SId", SId)
			};

			foreach (string role in roles)
			{
				_claims.Add(new Claim(ClaimTypes.Role, role));
			}

			IList<Claim> _claimsdb = await _userManager.GetClaimsAsync(user);

			_claims.AddRange(_claimsdb);

			DateTime _expiration = DateTime.UtcNow.AddDays(1);
			JwtSecurityToken _token = new(issuer: null, audience: null, claims: _claims, expires: _expiration, signingCredentials: _signingCredentials);

			return new JsonWebToken()
			{
				AccessToken = new JwtSecurityTokenHandler().WriteToken(_token),
				RefreshToken = "",
				Expiration = _expiration
			};
		}
	}
}
