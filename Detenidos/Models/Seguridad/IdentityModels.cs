using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Detenidos.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FriendlyName { get; set; }
        public int StatusAccountId { get; set; }
        public int UserId { get; set; }
        public int CatFiscaliaID { get; set; }
        public int PersonalID { get; set; }
        public DateTime FechaAlta { get; set; }    

        public ApplicationUser() : base() { }
    }

    public class UserDTO
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string NormalizedUserName { get; set; }
        public string Email { get; set; }
        public string NormalizedEmail { get; set; }
        public int AccessFailedCount { get; set; }
        public string FriendlyName { get; set; }
        public int StatusAccountId { get; set; }
        public int UserId { get; set; }
        public int CatFiscaliaID { get; set; }
        public int PersonalID { get; set; }
        public DateTime FechaAlta { get; set; }
        public bool ResetPassword { get; set; }       
    }

    public class CreateUserDTO
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FriendlyName { get; set; }
        public int StatusAccountId { get; set; }
        public int CatFiscaliaID { get; set; }
        public int PersonalID { get; set; }
        public bool ResetPassword { get; set; }      
    }

    public class UpdateUserDTO
    {
        public string Email { get; set; }
        public string FriendlyName { get; set; }
        public int StatusAccountId { get; set; }
        public int CatFiscaliaID { get; set; }
        public int PersonalID { get; set; }
        public bool ResetPassword { get; set; }        
    }

    public class ApplicationRole : IdentityRole
    {
        public string Descripcion { get; set; }
        public DateTime FechaAlta { get; set; }

        public ApplicationRole() : base() { }

        public ApplicationRole(string name, string descripcion) : base(name)
        {
            Name = name;
            Descripcion = descripcion;
        }
    }

    public class RoleDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaAlta { get; set; }
    }

    public class CreateUpdateRoleDTO
    {
        public string Name { get; set; }
        public string Descripcion { get; set; }
    }

    public class UserRoles
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Descripcion { get; set; }
        public bool Seleccionado { get; set; }
    }
}
