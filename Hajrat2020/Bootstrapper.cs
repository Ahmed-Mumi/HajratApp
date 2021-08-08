using Hajrat2020.Controllers;
using Hajrat2020.Interfaces;
using Hajrat2020.Repositories;
using Hajrat2020.Services;
using Microsoft.Practices.Unity;
using System.Web.Mvc;
using Unity.Mvc3;

namespace Hajrat2020
{
    public static class Bootstrapper
    {
        public static void Initialise()
        {
            var container = BuildUnityContainer();

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }

        private static IUnityContainer BuildUnityContainer()
        {
            var container = new UnityContainer();

            //container.RegisterType<IDonation, DonationService>();
            container.RegisterType<IController, DonationsController>("Donations");
            //container.RegisterType<IUser, UserService>();
            container.RegisterType<IController, UserController>("User");
            //container.RegisterType<IFamilyInNeed, FamilyInNeedService>();
            container.RegisterType<IController, FamilyInNeedController>("FamilyInNeed");
            container.RegisterType<IUnitOfWork, UnitOfWork>();
            container.RegisterType<AccountController>(new InjectionConstructor());
            //container.RegisterType<DonationsController>(new InjectionConstructor());


            return container;
        }
    }
}