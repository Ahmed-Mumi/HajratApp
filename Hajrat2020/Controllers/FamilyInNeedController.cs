using Hajrat2020.Interfaces;
using Hajrat2020.Models;
using Hajrat2020.ViewModel;
using Microsoft.AspNet.Identity;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Hajrat2020.Controllers
{
    public class FamilyInNeedController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public FamilyInNeedController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<ActionResult> GetFamilies(int? page)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            ViewBag.StartRowIndex = ((pageNumber - 1) * pageSize) + 1;
            var isAdmin = !User.IsInRole(RoleName.User);
            var families = await _unitOfWork.FamilyInNeedRepository.GetFamiliesDependingOnRole(User.Identity.GetUserId(), isAdmin);
            families = await ManageDateOfLastHelp(families);
            var familyViewModel = AutoMapper.Mapper.Map<IEnumerable<FamilyViewModel>>(families);
            var familiesViewModel = new FamilyViewModel
            {
                FamiliesInNeed = familyViewModel.ToPagedList(page ?? 1, pageSize),
                LoggedInUser = User.Identity.GetUserId(),
                IsAdmin = isAdmin,
            };
            return View(familiesViewModel);
        }
        public async Task<IEnumerable<FamilyInNeed>> ManageDateOfLastHelp(IEnumerable<FamilyInNeed> families)
        {
            var donations = await _unitOfWork.DonationRepository.GetDonationsOrderBy();
            foreach (var item in families)
            {
                var donation = donations.Where(x => x.FamilyInNeedId == item.Id).FirstOrDefault() ?? null;
                item.DateOfLastHelp = donation == null ? (DateTime?)null : donation.DateOfDonation;
            }
            return families;
        }
        public async Task<ActionResult> AddFamily()
        {
            var family = new FamilyViewModel()
            {
                ApplicationUserId = User.Identity.GetUserId(),
                Cities = await _unitOfWork.UserRepository.GetAllCities(),
            };
            return View("AddEditFamily", family);
        }
        public async Task<ActionResult> DeActivateFamily(int id)
        {
            if (id > 0)
            {
                var family = await _unitOfWork.FamilyInNeedRepository.GetFamily(id);
                family.IsActive = !family.IsActive;
                await _unitOfWork.Complete();
            }
            return RedirectToAction("GetFamilies");
        }
        public async Task<ActionResult> EditFamily(int id)
        {
            var familyViewModel = new FamilyViewModel();
            if (id > 0)
            {
                var family = await _unitOfWork.FamilyInNeedRepository.GetFamily(id);
                familyViewModel = AutoMapper.Mapper.Map<FamilyViewModel>(family);
                familyViewModel.Cities = await _unitOfWork.UserRepository.GetAllCities();
                familyViewModel.DateOfLastUpdate = DateTime.Now;
            }
            return View("AddEditFamily", familyViewModel);
        }
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(FamilyViewModel familyViewModel)
        {
            if (!ModelState.IsValid)
            {
                var familyVM = new FamilyViewModel()
                {
                    ApplicationUserId = User.Identity.GetUserId(),
                    Cities = await _unitOfWork.UserRepository.GetAllCities(),
                };
                return View("AddEditFamily", familyVM);
            }
            var isContactInfoEmpty = String.IsNullOrWhiteSpace(familyViewModel.ContactPersonName) ||
                String.IsNullOrWhiteSpace(familyViewModel.ContactPersonPhone);

            if (isContactInfoEmpty)
            {
                familyViewModel = await SetUsersContactInfo(familyViewModel);
            }
            var family = new FamilyInNeed();
            if (familyViewModel.Id > 0)
            {
                family = await _unitOfWork.FamilyInNeedRepository.GetFamily(familyViewModel.Id);
            }
            else
            {
                _unitOfWork.FamilyInNeedRepository.AddFamily(family);
            }
            AutoMapper.Mapper.Map(familyViewModel, family);
            await _unitOfWork.Complete();
            return RedirectToAction("GetFamilies");
        }
        public async Task<FamilyViewModel> SetUsersContactInfo(FamilyViewModel familyViewModel)
        {
            var user = await _unitOfWork.UserRepository.GetUser(familyViewModel.ApplicationUserId);
            familyViewModel.ContactPersonPhone = user.Phone;
            familyViewModel.ContactPersonName = user.FullName;
            return familyViewModel;
        }
        public async Task<ActionResult> GetFamily(int id)
        {
            var familyViewModel = new FamilyViewModel();
            if (id > 0)
            {
                familyViewModel = AutoMapper.Mapper.Map<FamilyViewModel>(await _unitOfWork.FamilyInNeedRepository.GetFamily(id));
                var donation = await _unitOfWork.DonationRepository.GetFamilyLastDonation(familyViewModel.Id) ?? null;
                familyViewModel.DateOfLastHelp = donation == null ? (DateTime?)null : donation.DateOfDonation;
            }
            return PartialView("_GetFamily", familyViewModel);
        }
        public async Task<ActionResult> AddFamilyToPrint(int id)
        {
            if (id > 0)
            {
                var family = new FamilyUser()
                {
                    UserId = User.Identity.GetUserId(),
                    FamilyInNeedId = id
                };
                _unitOfWork.FamilyInNeedRepository.AddFamilyToPrint(family);
                await _unitOfWork.Complete();
            }
            return RedirectToAction("GetFamilies");
        }
        public async Task<ActionResult> RemoveAllFamilyToPrint()
        {
            await _unitOfWork.FamilyInNeedRepository.RemoveAllFamilyToPrint(User.Identity.GetUserId());
            await _unitOfWork.Complete();
            return RedirectToAction("EditUser", "User", new { id = User.Identity.GetUserId() });
        }
        public async Task<ActionResult> RemoveFamilyToPrint(int id)
        {
            if (id > 0)
            {
                await _unitOfWork.FamilyInNeedRepository.RemoveFamilyToPrint(id, User.Identity.GetUserId());
                await _unitOfWork.Complete();
            }
            return RedirectToAction("EditUser", "User", new { id = User.Identity.GetUserId() });
        }
    }
}