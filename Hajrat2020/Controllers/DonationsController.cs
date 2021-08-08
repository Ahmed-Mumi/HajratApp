using Hajrat2020.Interfaces;
using Hajrat2020.Models;
using Hajrat2020.ViewModel;
using Microsoft.AspNet.Identity;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Hajrat2020.Controllers
{
    public class DonationsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public DonationsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<ActionResult> GetDonations(int? TypeOfHelpId, int? FamilyInNeedId, int? page, bool all = false)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            ViewBag.StartRowIndex = ((pageNumber - 1) * pageSize) + 1;
            var donations = await _unitOfWork.DonationRepository.GetAllDonations();
            var donationViewModel = AutoMapper.Mapper.Map<IEnumerable<DonationViewModel>>(donations);
            var pagedDonations = donationViewModel
                .Where(x => (TypeOfHelpId != null ? x.TypeOfHelpId == TypeOfHelpId : true)
                        && (FamilyInNeedId != null ? x.FamilyInNeedId == FamilyInNeedId : true));
            pageSize = all == false ? 10 : donationViewModel.Count();
            var donationsViewModel = new DonationViewModel
            {
                Donations = all == false ? pagedDonations.ToPagedList(page ?? 1, pageSize)
                    : pagedDonations.ToPagedList(1, pageSize),
                LoggedInUser = User.Identity.GetUserId(),
                IsAdmin = !User.IsInRole(RoleName.User),
                TypesOfHelp = await _unitOfWork.DonationRepository.GetAllTypesOfHelp(),
                FamiliesInNeed = await _unitOfWork.FamilyInNeedRepository.GetFamiliesOrderBy(),
                SearchTypeId = TypeOfHelpId != null ? TypeOfHelpId : null,
                SearchFamilyId = FamilyInNeedId != null ? FamilyInNeedId : null
            }; return View(donationsViewModel);
        }
        public async Task<ActionResult> AddDonation()
        {
            var donation = new DonationViewModel();
            donation = await PopulateDonationInformations(donation);
            return View("AddEditDonation", donation);
        }
        public async Task<DonationViewModel> PopulateDonationInformations(DonationViewModel donation)
        {
            donation.ApplicationUserId = User.Identity.GetUserId();
            donation.TypesOfHelp = await _unitOfWork.DonationRepository.GetAllTypesOfHelp();
            donation.FamiliesInNeed = await _unitOfWork.FamilyInNeedRepository.GetFamiliesOrderByActive();
            donation.Currencies = await _unitOfWork.DonationRepository.GetAllCurrencies();
            return donation;
        }
        public async Task<ActionResult> EditDonation(int id)
        {
            var donationViewModel = new DonationViewModel();
            if (id > 0)
            {
                var donation = await _unitOfWork.DonationRepository.GetDonation(id);
                if (donation != null)
                {
                    donationViewModel = AutoMapper.Mapper.Map<DonationViewModel>(donation);
                    donationViewModel = await PopulateDonationInformations(donationViewModel);
                    donationViewModel.DateOfLastUpdate = DateTime.Now;
                }
            }
            return View("AddEditDonation", donationViewModel);
        }

        public async Task<ActionResult> DeleteDonation(int id)
        {
            if (id > 0)
            {
                var donation = await _unitOfWork.DonationRepository.GetDonation(id);
                await DecreaseFamilyNumberOfHelps(donation.FamilyInNeedId);
                await _unitOfWork.DonationRepository.DeleteDonation(donation.Id);
                await _unitOfWork.Complete();
            }
            return RedirectToAction("GetDonations");
        }
        public async Task DecreaseFamilyNumberOfHelps(int familyInNeedId)
        {
            var family = await _unitOfWork.FamilyInNeedRepository.GetFamily(familyInNeedId);
            if (family != null && family.NumberOfHelpsSoFar > 0)
            {
                family.NumberOfHelpsSoFar--;
            }
        }
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(DonationViewModel donationViewModel)
        {
            if (!ModelState.IsValid)
            {
                var donationVM = new DonationViewModel();
                donationVM = await PopulateDonationInformations(donationVM);
                return View("AddEditDonation", donationVM);
                //return View("AddEditDonation", donationViewModel);
            }
            if (donationViewModel.ImageUpload != null)
            {
                donationViewModel = AddImage(donationViewModel);
            }
            var donation = new Donation();
            if (donationViewModel.Id > 0)
            {
                donation = await _unitOfWork.DonationRepository.GetDonation(donationViewModel.Id);
                await ChangeNumberOfHelps(donation, donationViewModel);
            }
            else
            {
                await UpdateFamily(donationViewModel.FamilyInNeedId);
                _unitOfWork.DonationRepository.AddDonation(donation);
            }
            AutoMapper.Mapper.Map(donationViewModel, donation);
            await _unitOfWork.Complete();
            return RedirectToAction("GetDonations");
        }
        public async Task<FamilyInNeed> UpdateFamily(int familyInNeedId)
        {
            var family = await _unitOfWork.FamilyInNeedRepository.GetFamily(familyInNeedId);
            family.NumberOfHelpsSoFar++;
            family.DateOfLastHelp = DateTime.Now;
            return family;
        }
        public DonationViewModel AddImage(DonationViewModel donationViewModel)
        {
            string fileName = Path.GetFileNameWithoutExtension(donationViewModel.ImageUpload.FileName);
            string extenstion = Path.GetExtension(donationViewModel.ImageUpload.FileName);
            fileName = fileName + DateTime.Now.ToString("yymmssfff") + extenstion;
            donationViewModel.Image = System.Web.HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) +
             (MagicNumbers.ImagePath + fileName);
            donationViewModel.ImageUpload.SaveAs(Path.Combine(System.Web.HttpContext.Current.
                Server.MapPath("~" + MagicNumbers.ImagePath), fileName));
            return donationViewModel;
        }
        public async Task ChangeNumberOfHelps(Donation donation, DonationViewModel donationViewModel)
        {
            if (donation.FamilyInNeedId != donationViewModel.FamilyInNeedId)
            {
                if (donation.FamilyInNeed.NumberOfHelpsSoFar > 0)
                {
                    var familyToDecreaseHelps = await _unitOfWork.FamilyInNeedRepository.GetFamily(donation.FamilyInNeedId);
                    familyToDecreaseHelps.NumberOfHelpsSoFar--;
                }
                var familyToIncreaseHelps = await _unitOfWork.FamilyInNeedRepository.GetFamily(donationViewModel.FamilyInNeedId);
                familyToIncreaseHelps.NumberOfHelpsSoFar++;
            }
        }
        public async Task<ActionResult> GetDonation(int id)
        {
            var donationViewModel = new DonationViewModel();
            if (id > 0)
            {
                donationViewModel = AutoMapper.Mapper.Map<DonationViewModel>
                    (await _unitOfWork.DonationRepository.GetDonationIncludes(id));
            }
            return PartialView("_GetDonation", donationViewModel);
        }
    }
}