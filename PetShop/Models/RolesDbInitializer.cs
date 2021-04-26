using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

namespace PetShop.Models
{
    public class RolesDbInitializer : CreateDatabaseIfNotExists<ApplicationDbContext>
    {
        protected override void Seed(ApplicationDbContext context)
        {
            var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(context));

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            var admin = new IdentityRole { Name = "admin" };
            var manager = new IdentityRole { Name = "manager" };
            var cashier = new IdentityRole { Name = "cashier" };

            roleManager.Create(admin);
            roleManager.Create(manager);
            roleManager.Create(cashier);

            var adminUser = new ApplicationUser { Email = "admin@gmail.com", UserName = "admin@gmail.com" };
            string password = "Jessya_58";
            var result = userManager.Create(adminUser, password);

            if (result.Succeeded)
            {
                userManager.AddToRole(adminUser.Id, admin.Name);
            }

            base.Seed(context);
        }
    }
}