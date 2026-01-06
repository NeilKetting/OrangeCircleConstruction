using System.Threading.Tasks;
using OCC.Shared.Models;

namespace OCC.Client.Services
{
    public interface IPermissionService
    {
        bool CanAccess(string route);
    }
}
