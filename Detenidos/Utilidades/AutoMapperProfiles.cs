using AutoMapper;
using NetTopologySuite.Geometries;
using Detenidos.Models;
//using Detenidos.Models.Transaccional;
using System.Collections.Generic;

namespace Detenidos.Utilidades
{
	public class AutoMapperProfiles: Profile
	{
		public AutoMapperProfiles(GeometryFactory geometryFactory)
		{
            // Seguridad
            CreateMap<ApplicationUser, UserDTO>().ReverseMap();
            CreateMap<CreateUserDTO, ApplicationUser>();
            CreateMap<UpdateUserDTO, ApplicationUser>();
            CreateMap<ApplicationRole, RoleDTO>().ReverseMap();
            CreateMap<CreateUpdateRoleDTO, ApplicationRole>();
            CreateMap<CatStatusAccount, CatStatusAccountDTO>().ReverseMap();     
        }  
 
    }
}
