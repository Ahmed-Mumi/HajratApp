using Hajrat2020.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hajrat2020.Interfaces
{
    public interface IDonation
    {
        Donation AddDonation(Donation donation);
        Task DeleteDonation(int id);
        void EditDonation(Donation donation);
        Task<Donation> GetDonation(int id);
        Task<Donation> GetFamilyLastDonation(int id);
        Task<IEnumerable<TypeOfHelp>> GetAllTypesOfHelp();
        Task<IEnumerable<Donation>> GetAllDonations();
        Task<IEnumerable<Donation>> GetDonationsOrderBy();
        Task<IEnumerable<Donation>> GetHajratDonations();
        Task<IEnumerable<Currency>> GetAllCurrencies();
        Task<Donation> GetDonationIncludes(int id);
    }
}
