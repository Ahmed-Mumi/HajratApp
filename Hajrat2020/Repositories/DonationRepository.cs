using Hajrat2020.Interfaces;
using Hajrat2020.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Hajrat2020.Repositories
{
    public class DonationRepository : IDonation
    {
        private readonly ApplicationDbContext _context;

        public DonationRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public Donation AddDonation(Donation donation)
        {
            _context.Donations.Add(donation);
            return donation;
        }
        public async Task DeleteDonation(int id)
        {
            var donation = await _context.Donations.SingleOrDefaultAsync(t => t.Id == id);
            _context.Donations.Remove(donation);
        }
        public void EditDonation(Donation donation)
        {
            _context.Entry(donation).State = EntityState.Modified;
        }
        public async Task<Donation> GetDonation(int id)
        {
            return await _context.Donations.
                Include(x => x.FamilyInNeed)
                .FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<Donation> GetDonationIncludes(int id)
        {
            return await _context.Donations.Where(x => x.Id == id)
                             .Include(t => t.TypeOfHelp)
                             .Include(u => u.ApplicationUser)
                             .Include(f => f.FamilyInNeed)
                             .Include(c => c.FamilyInNeed.City)
                             .Include(cu => cu.Currency)
                             .FirstOrDefaultAsync();
        }
        public async Task<Donation> GetFamilyLastDonation(int id)
        {
            return await _context.Donations.Where(x => x.FamilyInNeedId == id)
                    .OrderByDescending(x => x.DateOfDonation).FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<TypeOfHelp>> GetAllTypesOfHelp()
        {
            return await _context.TypeOfHelps.OrderBy(x => x.Name).ToListAsync();
        }
        public async Task<IEnumerable<Donation>> GetAllDonations()
        {
            return await _context.Donations
             .Include(t => t.TypeOfHelp)
             .Include(u => u.ApplicationUser)
             .Include(f => f.FamilyInNeed)
             .OrderByDescending(x => x.DateOfDonation)
             .ToListAsync();
        }
        public async Task<IEnumerable<Donation>> GetDonationsOrderBy()
        {
            return await _context.Donations.OrderByDescending(x => x.DateOfDonation).ToListAsync();
        }
        public async Task<IEnumerable<Donation>> GetHajratDonations()
        {
            return await _context.Donations.Where(x => x.FamilyInNeed.IsHajrat).ToListAsync();
        }
        public async Task<IEnumerable<Currency>> GetAllCurrencies()
        {
            return await _context.Currencies.ToListAsync();
        }
    }
}