using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Detenidos.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Detenidos.Controllers
{
	[Route("api/menu")]
	[ApiController]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public class MenuController : ControllerBase
	{
		private readonly ApplicationDbContext context;

		public MenuController(ApplicationDbContext context)
		{
			this.context = context;
		}

        [HttpGet("items")]
        public async Task<ActionResult<List<SidenavItem>>> Get()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            List<SidenavItem> items = new List<SidenavItem>();
            List<VW_Products> products = await context.VW_Products.Where(x => x.UserId == userId).AsNoTracking().ToListAsync();
            List<AspNetMenu> menus = await context.AspNetMenu.OrderBy(x => x.Position).AsNoTracking().ToListAsync();

            foreach (AspNetMenu menu in menus)
            {
                if (menu.Type != null && menu.Type.Equals("subheading"))
                {
                    items.Add(new SidenavItem()
                    {
                        Name = menu.Name,
                        Icon = menu.Icon,
                        RouteOrFunction = menu.RouteOrFunction,
                        Position = menu.Position,
                        PathMatchExact = menu.PathMatchExact,
                        Badge = menu.Badge,
                        BadgeColor = menu.BadgeColor,
                        Type = menu.Type,
                        CustomClass = menu.CustomClass
                    });
                }
                else
                {
                    List<VW_Products> productsPerMenu = products.Where(x => x.MenuID == menu.MenuID).ToList();
                    if (productsPerMenu.Count > 0)
                    {
                        List<SidenavItem> itemsPerMenu = new();
                        foreach (VW_Products product in productsPerMenu)
                        {
                            itemsPerMenu.Add(new SidenavItem()
                            {
                                Name = product.Name,
                                Icon = product.Icon,
                                RouteOrFunction = product.RouteOrFunction,
                                Position = product.Position,
                                PathMatchExact = product.PathMatchExact,
                                Badge = product.Badge,
                                BadgeColor = product.BadgeColor,
                                Type = product.Type,
                                CustomClass = product.CustomClass
                            });
                        }

                        items.Add(new SidenavItem()
                        {
                            Name = menu.Name,
                            Icon = menu.Icon,
                            RouteOrFunction = menu.RouteOrFunction,
                            SubItems = itemsPerMenu,
                            Position = menu.Position,
                            PathMatchExact = menu.PathMatchExact,
                            Badge = menu.Badge,
                            BadgeColor = menu.BadgeColor,
                            Type = menu.Type,
                            CustomClass = menu.CustomClass
                        });
                    }
                }
            }

            return items;
        }

        [HttpGet("itemsMenu")]
        public async Task<ActionResult<List<ItemsMenuDTO>>> ItemsMenu()
        {
            return await context.AspNetMenu.Where(x => !x.Type.Equals("subheading")).Select(y => new ItemsMenuDTO() { MenuID = y.MenuID, Name = y.Name }).AsNoTracking().ToListAsync();
        }
    }
}
