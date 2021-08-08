using System.Threading.Tasks;

namespace Hajrat2020.Interfaces
{
    public interface IUnitOfWork
    {
        IDonation DonationRepository { get; }
        IFamilyInNeed FamilyInNeedRepository { get; }
        IUser UserRepository { get; }
        Task<bool> Complete();
    }
}
