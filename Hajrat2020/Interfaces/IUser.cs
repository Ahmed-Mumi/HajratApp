using Hajrat2020.Models;
using Hajrat2020.ViewModel;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hajrat2020.Interfaces
{
    public interface IUser
    {
        Task<ApplicationUser> GetUser(string userId);
        Task<IEnumerable<ApplicationUser>> GetUsers();
        Task<IdentityRole> GetRole();
        Task<IEnumerable<Gender>> GetAllGenders();
        Task<IEnumerable<City>> GetAllCities();
        Task<IEnumerable<ApplicationUser>> GetNonSuperAdminUsers(string superAdminRoleId);
        Task<IEnumerable<ApplicationUser>> GetNonSuperAdminActiveUsers(string superAdminRoleId);
        UserViewModel GetUsers(int? page);
    }
}
