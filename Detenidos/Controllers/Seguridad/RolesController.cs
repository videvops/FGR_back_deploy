using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NetTopologySuite.Utilities;
using Detenidos.Models;
using Detenidos.Utilidades;
using Detenidos.Utilidades.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Detenidos.Controllers
{
	[Route("api/roles")]
	[ApiController]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public class RolesController : ControllerBase
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RoleManager<ApplicationRole> _roleManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly IConfiguration _configuration;
		private readonly ApplicationDbContext _context;
		private readonly IMapper _mapper;

		public RolesController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration, ApplicationDbContext context, RoleManager<ApplicationRole> roleManager, IMapper mapper)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_configuration = configuration;
			_context = context;
			_roleManager = roleManager;
			_mapper = mapper;
		}

		[HttpGet]
		[Authorize(Roles = "Administrador")]
		public async Task<ActionResult<List<RoleDTO>>> Get([FromQuery] PaginacionDTO paginacionDTO)
		{
			var queryable = _roleManager.Roles.AsQueryable();
			await HttpContext.InsertarParametrosPaginacionEnCabecera(queryable);
			var _roles = await queryable.OrderBy(x => x.Name).Paginar(paginacionDTO).ToListAsync();
			return _mapper.Map<List<RoleDTO>>(_roles);
		}

		[HttpGet("listaRoles")]
		[Authorize(Roles = "Administrador")]
		public async Task<ActionResult<List<RoleDTO>>> ListaRoles()		{
			var queryable = _roleManager.Roles.AsQueryable();
		
			var _roles = await queryable.OrderBy(x => x.Name).ToListAsync();
			return _mapper.Map<List<RoleDTO>>(_roles);
		}

		[HttpGet("role/{id}")]
		[Authorize(Roles = "Administrador")]
		public async Task<ActionResult<RoleDTO>> Get(string id)
		{
			var role = await _context.Roles.FindAsync(id);
			if (role == null)
			{
				return NotFound();
			}
			return _mapper.Map<RoleDTO>(role);
		}

		[HttpGet("userRoles/{id}")]
		[Authorize(Roles = "Administrador")]
		public async Task<ActionResult<List<UserRoles>>> UserRoles(string id)
		{
			if (id != null && !id.Equals(""))
			{
				// Se verifica que la tabla [AspNetRoles] y [AspNetUserRolesAudit] esten emparejadas
				DateTime fechaOperacion = new Utilerias(_configuration).GetFechaServidor();
				string IP = HttpContext.Connection.RemoteIpAddress.ToString();
				var urol = await _context.UserRoles.AsNoTracking().Where(x => x.UserId == id).ToListAsync();
				List<string> urolaud = await _context.AspNetUserRolesAudit.AsNoTracking().Where(x => x.UserId == id && x.Vigente == true).Select(y => y.RoleId).ToListAsync();
				if (urol.Count > urolaud.Count)
				{
					if (urolaud.Count == 0)
					{
						foreach (IdentityUserRole<string> rolusr in urol)
						{
							string roleName = _context.Roles.Find(rolusr.RoleId).Name ?? "";
							_context.Add(new AspNetUserRolesAudit() { UserId = id, RoleId = rolusr.RoleId, RoleName = roleName, AsignacionInicial = fechaOperacion, IP = IP, Vigente = true });
							await _context.SaveChangesAsync();
						}
					}
					else
					{
						foreach (IdentityUserRole<string> rolusr in urol)
						{
							if (!urolaud.Contains(rolusr.RoleId))
							{
								string roleName = _context.Roles.Find(rolusr.RoleId).Name ?? "";
								_context.Add(new AspNetUserRolesAudit() { UserId = id, RoleId = rolusr.RoleId, RoleName = roleName, AsignacionInicial = fechaOperacion, IP = IP, Vigente = true });
								await _context.SaveChangesAsync();
							}
						}
					}
				}

				// Se obtienen los roles
				List<UserRoles> ur = new();
				List<ApplicationRole> roles = await _context.ApplicationRole.ToListAsync();
				List<string> userRoles = urol.Where(x => x.UserId == id).Select(x => x.RoleId).ToList();
				foreach (ApplicationRole r in roles)
				{
					ur.Add(new UserRoles() { Id = r.Id, Name = r.Name, Descripcion = r.Descripcion, Seleccionado = userRoles.Contains(r.Id) });
				}

				return ur;
			}
			return NoContent();
		}

		[HttpPut("updateUserRoles/{id}")]
		[Authorize(Roles = "Administrador")]
		public async Task<ActionResult> Put(string id, string[] ids)
		{
			if (ids != null && !id.Equals(""))
			{
				string IP = HttpContext.Connection.RemoteIpAddress.ToString();
				ApplicationUser user = await _context.ApplicationUser.FindAsync(id);
				DateTime fechaOperacion = new Utilerias(_configuration).GetFechaServidor();

				List<AspNetUserRolesAudit> rolesActuales = await _context.AspNetUserRolesAudit.Where(x => x.UserId == user.Id && x.Vigente == true).ToListAsync();

				if (ids.Length == 0) // Se quitan todos los roles al usuario y se desactiva la cuenta
				{
					// Se desactiva la cuenta
					if (user.StatusAccountId != (int)StatusAccount.Bloqueada && user.StatusAccountId != (int)StatusAccount.Cancelada) user.StatusAccountId = (int)StatusAccount.Inactiva;

					foreach (AspNetUserRolesAudit rolActual in rolesActuales)
					{
						rolActual.AsignacionTermino = fechaOperacion;
						rolActual.Vigente = false;

						var result = _userManager.RemoveFromRoleAsync(user, rolActual.RoleName);
						result.Wait();
					}
				}
				else
				{
					List<IdentityRole> rolesSeleccionados = await _context.Roles.Where(x => ids.Contains(x.Id)).ToListAsync();
					List<string> rolesSeleccionadosStr = rolesSeleccionados.Select(x => x.Id).ToList();
					List<string> rolesActualesStr = rolesActuales.Select(x => x.RoleId).ToList();

					// Se habilita la cuenta del usuario
					if (user.StatusAccountId != (int)StatusAccount.Bloqueada && user.StatusAccountId != (int)StatusAccount.Cancelada) user.StatusAccountId = (int)StatusAccount.Activa;

					// Se eliminan Roles al usuario
					foreach (AspNetUserRolesAudit rolActual in rolesActuales)
					{
						if (!rolesSeleccionadosStr.Contains(rolActual.RoleId))
						{
							rolActual.AsignacionTermino = fechaOperacion;
							rolActual.Vigente = false;

							var result = _userManager.RemoveFromRoleAsync(user, rolActual.RoleName);
							result.Wait();
						}
					}

					// Se agregan Roles al usuario
					foreach (IdentityRole rolSeleccionado in rolesSeleccionados)
					{
						if (!rolesActualesStr.Contains(rolSeleccionado.Id))
						{
							_context.AspNetUserRolesAudit.Add(new AspNetUserRolesAudit() { UserId = user.Id, RoleId = rolSeleccionado.Id, RoleName = rolSeleccionado.Name, AsignacionInicial = fechaOperacion, IP = IP, Vigente = true });

							var result = _userManager.AddToRoleAsync(user, rolSeleccionado.Name);
							result.Wait();
						}
					}
				}
			}

			return NoContent();
		}

		[HttpPut("update/{id}")]
		[Authorize(Roles = "Administrador")]
		public async Task<ActionResult> Put(string id, [FromBody] CreateUpdateRoleDTO _createUpdateRoleDTO)
		{
			var _role = await _context.Roles.FindAsync(id);
			if (_role == null)
			{
				return NotFound();
			}

			_role = _mapper.Map(_createUpdateRoleDTO, _role);

			var _result = await _roleManager.UpdateAsync(_role as ApplicationRole);

			if (_result.Succeeded) { return NoContent(); }
			else { return BadRequest(_result.Errors); }
		}

		[AllowAnonymous]
		[HttpPost("create")]
		[Authorize(Roles = "Administrador")]
		public async Task<ActionResult> Post([FromBody] CreateUpdateRoleDTO _createUpdateRoleDTO)
		{
			var _role = _mapper.Map<ApplicationRole>(_createUpdateRoleDTO);
			_role.FechaAlta = new Utilerias(_configuration).GetFechaServidor();

			var _result = await _roleManager.CreateAsync(_role);

			if (_result.Succeeded) { return NoContent(); }
			else { return BadRequest(_result.Errors); }
		}

		[HttpGet("productRoles/{id}/{menuId}")]
		[Authorize(Roles = "Administrador")]
		public async Task<ActionResult<List<ProductsDTO>>> Products(string id, int menuId)
		{
			if (id != null && !id.Equals("") && menuId > 0)
			{
				List<ProductsDTO> pr = new();
				var products = await _context.AspNetProducts.Where(x => x.Production == true && x.MenuID ==  menuId).Select(y => new { y.ProductID, y.MenuID, y.Name }).ToListAsync();
				List<int> productRoles = await _context.AspNetProductRoles.Where(x => x.RoleId == id && x.Vigente == true).Select(y => y.ProductID).ToListAsync();

				foreach (var product in products)
				{
					pr.Add(new ProductsDTO() { ProductID = product.ProductID, MenuID = product.MenuID, Name = product.Name, Selected = productRoles.Contains(product.ProductID) });
				}

				return pr;
			}
			return NoContent();
		}

		[HttpPut("updateProductRoles/{id}/{mid}")]
		[Authorize(Roles = "Administrador")]
		public async Task<ActionResult> UpdateProductRoles(string id, int mid, int[] ids)
		{
			if (ids != null && !id.Equals("") && mid > 0)
			{
				string IP = HttpContext.Connection.RemoteIpAddress.ToString();
				DateTime fechaOperacion = new Utilerias(_configuration).GetFechaServidor();

				int[] productsByMenu = await _context.AspNetProducts.Where(x => x.MenuID == mid).Select(y => y.ProductID).ToArrayAsync();
				List<AspNetProductRoles> productosActuales = await _context.AspNetProductRoles.Where(x => x.RoleId == id && productsByMenu.Contains(x.ProductID) && x.Vigente == true).ToListAsync();

				if (ids.Length == 0) // Se quitan todos los productos al Rol por menu
				{
					foreach (AspNetProductRoles productoActual in productosActuales)
					{
						productoActual.AsignacionTermino = fechaOperacion;
						productoActual.Vigente = false;
					}
					_ = await _context.SaveChangesAsync();
				}
				else
				{
					List<Products> productosSeleccionados = await _context.AspNetProducts.Where(x => ids.Contains(x.ProductID)).Select(y => new Products() { ProductID = y.ProductID, MenuID = y.MenuID }).ToListAsync();
					List<int> productsSeleccionadosInt = productosSeleccionados.Select(x => x.ProductID).ToList();
					List<int> productsActualesInt = productosActuales.Select(x => x.ProductID).ToList();

					// Se eliminan los productos al Rol por menu
					foreach (AspNetProductRoles productoActual in productosActuales)
					{
						if (!productsSeleccionadosInt.Contains(productoActual.ProductID))
						{
							productoActual.AsignacionTermino = fechaOperacion;
							productoActual.Vigente = false;
						}
					}
					_ = await _context.SaveChangesAsync();

					// Se agregan los productos al Rol por menu
					foreach (Products productoSeleccionado in productosSeleccionados)
					{
						if (!productsActualesInt.Contains(productoSeleccionado.ProductID))
						{
							_context.AspNetProductRoles.Add(new AspNetProductRoles()
							{
								RoleId = id,
								ProductID = productoSeleccionado.ProductID,
								AsignacionInicial = fechaOperacion,
								IP = IP,
								Vigente = true
							});
						}
					}
					_ = await _context.SaveChangesAsync();
				}
			}

			return NoContent();
		}
	}
}
