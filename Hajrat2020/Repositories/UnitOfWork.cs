using Hajrat2020.Interfaces;
using Hajrat2020.Models;
using System.Threading.Tasks;

namespace Hajrat2020.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }
        public IDonation DonationRepository => new DonationRepository(_context);
        public IUser UserRepository => new UserRepository(_context);
        public IFamilyInNeed FamilyInNeedRepository => new FamilyInNeedRepository(_context);
        public async Task<bool> Complete()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}