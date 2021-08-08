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
    public class UserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ActionResult> GetUsers(int? page)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            ViewBag.StartRowIndex = ((pageNumber - 1) * pageSize) + 1;
            var userViewModel = AutoMapper.Mapper.Map<IEnumerable<UserViewModel>>(await PopulateUserList());
            var usersViewModel = new UserViewModel
            {
                Users = userViewModel.ToPagedList(page ?? 1, pageSize),
                IsRoleAdmin = !User.IsInRole(RoleName.User),
                LoggedInUser = User.Identity.GetUserId()
            };
            return View(usersViewModel);
        }
        [Authorize(Roles = RoleName.SuperAdminOrAdmin)]
        public async Task<ActionResult> DeActivateUser(string id)
        {
            if (!String.IsNullOrWhiteSpace(id))
            {
                var user = await _unitOfWork.UserRepository.GetUser(id);
                user.Active = !user.Active;
                await _unitOfWork.Complete();
            }
            return RedirectToAction("GetUsers");
        }
        [Authorize(Roles = RoleName.SuperAdminOrAdmin)]
        public async Task<ActionResult> AddUser()
        {
            var userViewModel = new UserViewModel()
            {
                PasswordHash = GenerateRandomPassword()
            };
            userViewModel = await PopulateUsersInfo(userViewModel);
            return View("AddEditUser", userViewModel);
        }
        public async Task<ActionResult> EditUser(string id)
        {
            var userViewModel = new UserViewModel();
            if (!String.IsNullOrWhiteSpace(id))
            {
                var user = await _unitOfWork.UserRepository.GetUser(id);
                userViewModel = AutoMapper.Mapper.Map<UserViewModel>(user);
                userViewModel = await PopulateUsersInfo(userViewModel);
                userViewModel.IsRoleSuperAdmin = User.IsInRole(RoleName.SuperAdmin);
                userViewModel.IsChosenRoleAdmin = userViewModel.RoleName != RoleName.User;
                userViewModel.LoggedInUser = User.Identity.GetUserId();
                userViewModel.FamilyUsers = await _unitOfWork.FamilyInNeedRepository.GetFamilyUsersByUserId(User.Identity.GetUserId());
                userViewModel = await ReceivedMoney(userViewModel);
            }
            return View("AddEditUser", userViewModel);
        }
        public async Task<UserViewModel> PopulateUsersInfo(UserViewModel user)
        {
            user.Genders = await _unitOfWork.UserRepository.GetAllGenders();
            user.Cities = await _unitOfWork.UserRepository.GetAllCities();
            user.IsRoleAdmin = !User.IsInRole(RoleName.User);
            return user;
        }
        public async Task<ActionResult> GetUser(string id)
        {
            var userViewModel = new UserViewModel();
            if (!String.IsNullOrWhiteSpace(id))
            {
                userViewModel = AutoMapper.Mapper.Map<UserViewModel>(await _unitOfWork.UserRepository.GetUser(id));
            }
            return PartialView("_GetUser", userViewModel);
        }
        public async Task<IEnumerable<ApplicationUser>> PopulateUserList()
        {
            IEnumerable<ApplicationUser> users = new List<ApplicationUser>();
            if (User.IsInRole(RoleName.SuperAdmin))
            {
                users = await _unitOfWork.UserRepository.GetUsers();
            }
            else
            {
                var superAdminRoleId = await _unitOfWork.UserRepository.GetRole();
                if (User.IsInRole(RoleName.Admin))
                {
                    users = await _unitOfWork.UserRepository.GetNonSuperAdminUsers(superAdminRoleId.Id);
                }
                else if (User.IsInRole(RoleName.User))
                {

                    users = await _unitOfWork.UserRepository.GetNonSuperAdminActiveUsers(superAdminRoleId.Id);
                }
            }
            return users;
        }
        public async Task<UserViewModel> ReceivedMoney(UserViewModel model)
        {
            var hajratDonations = await _unitOfWork.DonationRepository.GetHajratDonations();
            model.ReceivedMoneySumKM = hajratDonations.Where(x => x.CurrencyId == MagicNumbers.BAM).Select(x => (decimal)x.AmountOfMoney).DefaultIfEmpty().Sum();
            model.ReceivedMoneySumEuro = hajratDonations.Where(x => x.CurrencyId == MagicNumbers.Euro).Select(x => (decimal)x.AmountOfMoney).DefaultIfEmpty().Sum();
            model.ReceivedMoneySumDolar = hajratDonations.Where(x => x.CurrencyId == MagicNumbers.Dolar).Select(x => (decimal)x.AmountOfMoney).DefaultIfEmpty().Sum();
            return model;
        }
        public static string GenerateRandomPassword()
        {
            var requiredLength = 12;
            var requiredUniqueChars = 4;
            var requireDigit = true;
            var requireLowercase = true;
            var requireNonAlphanumeric = true;
            var requireUppercase = true;
            string[] randomChars = new[] {
            "ABCDEFGHJKLMNOPQRSTUVWXYZ",    // uppercase 
            "abcdefghijkmnopqrstuvwxyz",    // lowercase
            "0123456789",                   // digits
            "#%@$?_-"                        // non-alphanumeric
        };

            Random rand = new Random(Environment.TickCount);
            List<char> chars = new List<char>();

            if (requireUppercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[0][rand.Next(0, randomChars[0].Length)]);

            if (requireLowercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[1][rand.Next(0, randomChars[1].Length)]);

            if (requireDigit)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[2][rand.Next(0, randomChars[2].Length)]);

            if (requireNonAlphanumeric)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[3][rand.Next(0, randomChars[3].Length)]);

            for (int i = chars.Count; i < requiredLength
                || chars.Distinct().Count() < requiredUniqueChars; i++)
            {
                string rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count),
                    rcs[rand.Next(0, rcs.Length)]);
            }

            return new string(chars.ToArray());
        }
    }
}