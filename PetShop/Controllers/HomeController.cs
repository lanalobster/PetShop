using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using PetShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PetShop.Controllers
{
    public class HomeController : Controller
    {
        public async System.Threading.Tasks.Task<ActionResult> Index()
        {
            ApplicationUserManager userManager = HttpContext.GetOwinContext()
                                           .GetUserManager<ApplicationUserManager>();
            var user = await userManager.FindByNameAsync(User.Identity.Name);
            if(user == null)
                return View(@"~/Views/Account/Login.cshtml");
            var isUserCashier = await userManager.IsInRoleAsync(user.Id, "cashier");
            if (isUserCashier)
                return RedirectToAction("Index", "Items");
            var isUserManager = await userManager.IsInRoleAsync(user.Id, "manager");
            if (isUserManager)
                return RedirectToAction("Index", "ItemsOrders");
            return RedirectToAction("Index", "Items");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}