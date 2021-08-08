using Hajrat2020.Interfaces;
using Hajrat2020.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Hajrat2020.Repositories
{
    public class FamilyInNeedRepository : IFamilyInNeed
    {
        private readonly ApplicationDbContext _context;

        public FamilyInNeedRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public FamilyInNeed AddFamily(FamilyInNeed familyInNeed)
        {
            _context.FamilyInNeeds.Add(familyInNeed);
            return familyInNeed;
        }
        public FamilyUser AddFamilyToPrint(FamilyUser familyUser)
        {
            _context.FamilyUsers.Add(familyUser);
            return familyUser;
        }
        public void EditFamily(FamilyInNeed familyInNeed)
        {
            _context.Entry(familyInNeed).State = EntityState.Modified;
        }
        public async Task<IEnumerable<FamilyInNeed>> GetFamiliesDependingOnRole(string userId, bool isAdmin)
        {
            return await _context.FamilyInNeeds.Include(c => c.City)
                                .Include(u => u.ApplicationUser)
                                .Where(x => ((x.ApplicationUserId == userId || isAdmin) ? true : x.IsActive)
                                        && !x.IsHajrat)
                                .OrderByDescending(x => x.IsUrgent)
                                .ThenBy(x => x.DateOfLastHelp)
                                .ToListAsync();
        }
        public async Task<FamilyInNeed> GetFamily(int id)
        {
            return await _context.FamilyInNeeds
                                 .Include(c => c.City)
                                 .Include(u => u.ApplicationUser)
                                 .Where(x => x.Id == id).FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<FamilyUser>> GetFamilyUsersByUserId(string userId)
        {
            return await _context.FamilyUsers.Include(x => x.FamilyInNeed)
                    .Where(x => x.UserId == userId).ToListAsync();
        }
        public async Task<IEnumerable<FamilyInNeed>> GetFamiliesOrderBy()
        {
            return await _context.FamilyInNeeds.OrderBy(x => x.FullName).ToListAsync();
        }
        public async Task<IEnumerable<FamilyInNeed>> GetFamiliesOrderByActive()
        {
            return await _context.FamilyInNeeds.OrderBy(x => x.FirstName).Where(x => x.IsActive).ToListAsync();
        }
        public async Task<FamilyUser> GetFamilyUserByFamilyIdUserId(string userId, int familyInNeedId)
        {
            return await _context.FamilyUsers
                .Where(x => x.FamilyInNeedId == familyInNeedId && x.UserId == userId)
                .FirstOrDefaultAsync();
        }
        public async Task RemoveAllFamilyToPrint(string userId)
        {
            var familyUsers = await _context.FamilyUsers.Where(x => x.UserId == userId).ToListAsync();
            _context.FamilyUsers.RemoveRange(familyUsers);
        }
        public async Task RemoveFamilyToPrint(int familyInNeedId, string userId)
        {
            var familyUser = await _context.FamilyUsers
                    .Where(x => x.FamilyInNeedId == familyInNeedId && x.UserId == userId).FirstOrDefaultAsync();
            _context.FamilyUsers.Remove(familyUser);
        }
    }
}