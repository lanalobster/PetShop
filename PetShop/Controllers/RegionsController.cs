using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PetShop.Models;

namespace PetShop.Controllers
{
    public class RegionsController : Controller
    {
        private PetShopContext db = new PetShopContext();

        // GET: Regions
        public ActionResult Index()
        {
            return View(db.Regions.ToList());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Name")] Region region)
        {
            if (ModelState.IsValid)
            {
                region.CreatedOn = DateTime.UtcNow;
                db.Regions.Add(region);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(region);
        }

         protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
