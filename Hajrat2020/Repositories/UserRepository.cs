using Hajrat2020.Interfaces;
using Hajrat2020.Models;
using Hajrat2020.ViewModel;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Hajrat2020.Repositories
{
    public class UserRepository : IUser
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<ApplicationUser> GetUser(string userId)
        {
            return await _context.Users
                           .Include(x => x.City)
                           .Include(x => x.Gender)
                           .Where(x => x.Id == userId).FirstOrDefaultAsync();
        }
        public async Task<IdentityRole> GetRole()
        {
            return await _context.Roles.Where(x => x.Name == RoleName.SuperAdmin).FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<ApplicationUser>> GetUsers()
        {
            return await _context.Users.OrderBy(x => x.FirstName).ToListAsync();
        }
        public async Task<IEnumerable<Gender>> GetAllGenders()
        {
            return await _context.Genders.OrderBy(x => x.Name).ToListAsync();
        }
        public async Task<IEnumerable<City>> GetAllCities()
        {
            return await _context.Cities.OrderBy(x => x.Name).ToListAsync();
        }
        public async Task<IEnumerable<ApplicationUser>> GetNonSuperAdminUsers(string superAdminRoleId)
        {
            return await _context.Users.Where(x => x.Roles
                                    .Any(n => n.RoleId != superAdminRoleId))
                                    .OrderBy(x => x.FirstName).ToListAsync();
        }
        public async Task<IEnumerable<ApplicationUser>> GetNonSuperAdminActiveUsers(string superAdminRoleId)
        {
            return await _context.Users.Where(x => x.Roles
                        .Any(n => n.RoleId != superAdminRoleId) && x.Active)
                        .OrderBy(x => x.FirstName)
                        .ToListAsync();
        }
        public UserViewModel GetUsers(int? page)
        {
            throw new System.NotImplementedException();
        }
    }
}