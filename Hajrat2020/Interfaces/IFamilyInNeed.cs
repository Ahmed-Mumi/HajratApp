using Hajrat2020.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hajrat2020.Interfaces
{
    public interface IFamilyInNeed
    {
        FamilyInNeed AddFamily(FamilyInNeed familyInNeed);
        FamilyUser AddFamilyToPrint(FamilyUser familyUser);
        void EditFamily(FamilyInNeed familyInNeed);
        Task<IEnumerable<FamilyInNeed>> GetFamiliesDependingOnRole(string userId, bool isAdmin);
        Task<FamilyInNeed> GetFamily(int id);
        Task<IEnumerable<FamilyUser>> GetFamilyUsersByUserId(string userId);
        Task<FamilyUser> GetFamilyUserByFamilyIdUserId(string userId, int familyInNeedId);
        Task RemoveAllFamilyToPrint(string userId);
        Task RemoveFamilyToPrint(int familyInNeedId, string userId);
        Task<IEnumerable<FamilyInNeed>> GetFamiliesOrderBy();
        Task<IEnumerable<FamilyInNeed>> GetFamiliesOrderByActive();

    }
}
