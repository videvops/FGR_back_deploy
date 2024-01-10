using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Detenidos.Models;
using Detenidos.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Detenidos.Utilidades.Interfaces;
using Detenidos.Services;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Detenidos.Utilidades.Helpers;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace Detenidos.Controllers
{
	[Route("api/accounts")]
	[ApiController]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] 
	public class AccountController : ControllerBase
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RoleManager<ApplicationRole> _roleManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly IConfiguration _configuration;
		private readonly ApplicationDbContext _context;
		private readonly IMapper _mapper;
		private readonly IFileStorage _fileStore;
		private readonly ITokenManager _tokenManager;
		//private readonly JwtHandler _jwtHandler;
		private readonly JwtOptions _jwtOptions;
		private readonly SecurityKey _securityKey;
		private readonly SigningCredentials _signingCredentials;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public AccountController(
			UserManager<ApplicationUser> userManager, 
			SignInManager<ApplicationUser> signInManager, 
			IConfiguration configuration, 
			ApplicationDbContext context, 
			RoleManager<ApplicationRole> roleManager, 
			IMapper mapper, 
			IFileStorage fileStore, 
			ITokenManager tokenManager,
			//JwtHandler jwtHandler,
			IOptions<JwtOptions> jwtOptions,
			IHttpContextAccessor httpContextAccessor)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_configuration = configuration;
			_roleManager = roleManager;
			_context = context;
			_mapper = mapper;
			_fileStore = fileStore;
			_tokenManager = tokenManager;
			//_jwtHandler = jwtHandler;
			_jwtOptions = jwtOptions.Value;
			_securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
			_signingCredentials = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256);
			_httpContextAccessor = httpContextAccessor;
		}

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<List<UserDTO>>> Get([FromQuery] PaginacionDTO paginacionDTO)
        {
            var queryable = _userManager.Users.AsQueryable();                    
            await HttpContext.InsertarParametrosPaginacionEnCabecera(queryable);
            //var _users = await queryable.OrderBy(x => x.UserName).Paginar(paginacionDTO).ToListAsync();
            //return _mapper.Map<List<UserDTO>>(_users);
            return Ok(await queryable.OrderBy(x => x.UserName).Paginar(paginacionDTO).ToListAsync());
        }

        [HttpGet("user/{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<UserDTO>> Get(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return _mapper.Map<UserDTO>(user);
        }


        [HttpPut("update/{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult> Put(string id, [FromBody] UpdateUserDTO _updateUserDTO)
        {
            var user = await _context.Users.FindAsync(id);            
            if (user == null) { return NotFound(); }

            // Reestablecemos la contraseña a 123456
            if (_updateUserDTO.ResetPassword)
            {
                var _resultRemovePassword = _userManager.RemovePasswordAsync(user as ApplicationUser);
                if (_resultRemovePassword.Result.Succeeded)
                {
                    var _result = _userManager.AddPasswordAsync(user as ApplicationUser, "123456");
                    if (_result.Result.Succeeded)
                    {
                        // Contraseña reestablecida
                    }
                }
            }

            if (_updateUserDTO.StatusAccountId == 2) { user.AccessFailedCount = 0; }

            _ = _mapper.Map(_updateUserDTO, user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("updatePassword/{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult> UpdatePassword(string id, [FromBody] ManageUserViewModel model)
        {
            var user = await _context.Users.FindAsync(id);              
            if (user == null)
            {
                return NotFound();
            }
            // Reestablecemos la contraseña a 123456
            IdentityResult result = await _userManager.ChangePasswordAsync(user as ApplicationUser, model.CurrentPassowrd, model.NewPassword);
            if (result.Succeeded)
            {
                return NoContent();
            }
            else
            {
                return BadRequest();
            }
        }
      
        [HttpPost("register")]
        //[Authorize(Roles = "Administrador")]
        public async Task<ActionResult> Post([FromBody] CreateUserDTO _createUserDTO)
        {
            /*
             * Cada que se crea una cuenta del sistema
             * se genera un registro en la tabla UsuariosSistemasExternosAIC de la base de datos AIC_Develop, mediente el PrometheusWebApi
             */
            string sistema = new Utilerias(_configuration).GetNombreSistema("nombreSistema");

            DatosRgistro datosRegistro = new();
            datosRegistro.PersonalID = _createUserDTO.PersonalID;
            datosRegistro.Sistema =sistema;

            var _user = _mapper.Map<ApplicationUser>(_createUserDTO);
            _user.UserId = new Utilerias(_configuration).GetSequence("Users");           
            _user.FechaAlta = new Utilerias(_configuration).GetFechaServidor();
            _user.PersonalID = _createUserDTO.PersonalID;
            var _result = await _userManager.CreateAsync(_user, "123456");
            string valorUrlApi = new Utilerias(_configuration).GetValorConstante("webapi_url_base");

            HttpClient client = new HttpClient();
            if (client.BaseAddress == null)
                client.BaseAddress = new Uri(valorUrlApi);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage resp = client.PostAsJsonAsync(valorUrlApi + "/api/SL_RegistroCuentasSistemasAIC/RegistroUsuario", datosRegistro).Result;

            if (_result.Succeeded) {            
                if (resp.IsSuccessStatusCode)
                {
                    return Ok();
                }
                return NoContent(); 
            }
            else { return BadRequest(_result.Errors); }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<JsonWebToken>> Login([FromBody] LoginViewModel loginViewModel)
        {
            ApplicationUser user = await _userManager.FindByNameAsync(loginViewModel.UserName); // Obtenemos el usuario que quiere iniciar sesión
            if (user != null)
            {
                if (user.StatusAccountId==1)
                {
                    return Unauthorized("Su cuenta está inactiva, consulte al administrador del sistema!");
                }
                if (user.StatusAccountId ==2)
                {
                    return Unauthorized("Su cuenta está bloqueada, consulte al administrador del sistema!");
                }
                if (user.StatusAccountId ==4)
                {
                    return Unauthorized("Su cuenta está cancelada, consulte al administrador del sistema!");
                }
                if (user.AccessFailedCount == 5) // El usuario escribió mal su contraseña 5 veces
                {
                    user.StatusAccountId = 2;
                    await _context.SaveChangesAsync();
                    return Unauthorized("Su cuenta está bloqueada, consulte al administrador del sistema!");
                }

                var _resultSignIn = await _signInManager.PasswordSignInAsync(loginViewModel.UserName, loginViewModel.Password, isPersistent: false, lockoutOnFailure: true);

                if (_resultSignIn.Succeeded) // Inicio de sesión exitoso
                {                  
                    user.AccessFailedCount = 0;
                    AspNetUserSessions session = new()
                    {
                        UserId = user.Id,
                        IP = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(),
                        LoginDate = new Utilerias(_configuration).GetFechaServidor(),
                        SessionStatusId = (int)StatusSession.User_Login
                    };
                    _context.Add(session);
                    await _context.SaveChangesAsync();
                    int SessionID = session.Id;

                    return await GetToken(user, SessionID.ToString());
                }
                else 
                {
                    return Unauthorized("El usuario o la contraseña son incorrectos!");
                }
            }
            else
            {
                return Unauthorized("El usuario o la contraseña son incorrectos!");
            }
        }

        [HttpPost("logoff")]
        public async Task<ActionResult> LogOff([FromBody] Sessions s)
        {
            int SessionId = new Utilerias().GetInt(Security.Decrypt(s.SessionID));
            AspNetUserSessions session = await _context.Sessions.FindAsync(SessionId);
            if (session != null)
            {
                session.LogoffDate = new Utilerias(_configuration).GetFechaServidor();
                session.SessionStatusId = (int)StatusSession.User_Logout;
                await _context.SaveChangesAsync();
            }
            return NoContent();
        }

        private async Task<JsonWebToken> GetToken(ApplicationUser _user, string SessionID)
        {
            IList<string> roles = await _userManager.GetRolesAsync(_user);
            // string fiscalia = _context.CatFiscalias.Find(_user.CatFiscaliaID).NombreFiscalia ?? "";

            List<Claim> _claims = new()
            {
                new Claim(ClaimTypes.NameIdentifier, _user.Id),
                new Claim(ClaimTypes.Name, _user.UserName),
                new Claim("CatFiscaliaID", _user.CatFiscaliaID.ToString()),
                new Claim("UserName", _user.UserName),
                new Claim("friendlyname", _user.FriendlyName),
                //new Claim("fiscalia", fiscalia)
            };

            foreach (string role in roles)
            {
                _claims.Add(new Claim(ClaimTypes.Role, role));
            }

            IList<Claim> _claimsdb = await _userManager.GetClaimsAsync(_user);

            _claims.AddRange(_claimsdb);

            DateTime _expiration = DateTime.UtcNow.AddDays(1);
            JwtSecurityToken _token = new(issuer: null, audience: null, claims: _claims, expires: _expiration, signingCredentials: _signingCredentials);

            var jwt = new JsonWebToken()
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(_token),
                RefreshToken = "",
                Expiration = _expiration,
                SessionID = Security.Encrypt(SessionID)
            };

            //await _tokenManager.ActivateCurrentAsync(_user.UserName, jwt.AccessToken);

            return jwt;
        }

        [HttpPost("avatar")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult> Post([FromForm] UserAvatar userAvatar)
        {
            if (userAvatar.Avatar != null)
            {
                await _fileStore.SaveFileWWWRoot("Avatars", userAvatar.Avatar);
            }
            return NoContent();
        }

        [HttpGet("version")]
        [AllowAnonymous]
        public ActionResult<string> Version()
        {
            return "1.0";
        }

        [AllowAnonymous]
        [HttpPut("actulizarStatusCuenta/{PersonalID:int}")]
        [Authorize(Roles = "Administrador")]       
        public async Task<ActionResult> ActulizarStatusCuenta(int PersonalID)
        {
         var users = await _userManager.Users.Where(x => x.PersonalID==PersonalID).ToListAsync();
            if (users == null)
            {
                return NotFound();
            }
            try
            {
                foreach (var item in users)
                {
                   ApplicationUser cuentaUser = await _userManager.Users.Where(x=>x.Id==item.Id).FirstOrDefaultAsync();
                    cuentaUser.StatusAccountId = 2;                   
                    _context.Entry(cuentaUser).State = EntityState.Modified;
                    _context.SaveChanges();
                }               
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

    }
}
