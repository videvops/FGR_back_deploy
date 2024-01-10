using Detenidos.Models;
using System.Threading.Tasks;

namespace Detenidos.Utilidades.Interfaces
{
    public interface IJwtHandler
    {
        Task<JsonWebToken> GetToken(ApplicationUser user);
    }
}
