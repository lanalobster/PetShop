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
    public class StoresController : Controller
    {
        private PetShopContext db = new PetShopContext();

        // GET: Stores
        public ActionResult Index()
        {
            var store = db.Stores.Include(s => s.City).Include(r => r.City.Region);
            return View(store.ToList());
        }

        // GET: Stores/Create
        public ActionResult Create()
        {
            ViewBag.Cities = db.Cities;
            ViewBag.Regions = db.Regions;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Street")] Store store, string city, string region)
        {
            var cityToAdd = db.Cities.Where(c => c.Name == city).FirstOrDefault();
            var regionToAdd = db.Regions.Where(c => c.Name == region).FirstOrDefault();
            if (ModelState.IsValid)
            {
                if (regionToAdd == null)
                {
                    regionToAdd = db.Regions.Add(new Region { CreatedOn = DateTime.UtcNow, Name = region });
                }
                if (cityToAdd == null)
                {
                    cityToAdd = db.Cities.Add(new City { CreatedOn = DateTime.UtcNow, Region = regionToAdd });
                }
                store.CreatedOn = DateTime.UtcNow;
                store.City = cityToAdd;
                db.Stores.Add(store);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(store);
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
