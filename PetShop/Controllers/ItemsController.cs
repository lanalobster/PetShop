using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PetShop.App_Start;
using PetShop.Models;

namespace PetShop.Controllers
{
    public class ItemsController : Controller
    {
        private PetShopContext db = new PetShopContext();

        // GET: Items
        public ActionResult Index(int page = 1)
        {
            //foreach(var item in db.Items.ToList())
            //{
            //    item.Name = "Animal Item " + Convert.ToInt32(item.Price + item.ItemId/10);
            //    db.Entry(item).State = EntityState.Modified;
            //    db.SaveChanges();
            //}

            var foundItems = db.Items.ToList();
            var itemsPerPage = PageConfig.ItemsPerPage;
            ViewBag.CurrentPage = page;
            ViewBag.NumberOfPages = foundItems.Count / itemsPerPage + 1;
            if (foundItems.Count <= 0)
            {
                return PartialView("NoMatchFound");
            }
            var itemsToShow = foundItems.OrderBy(i => i.Name).Skip((page - 1) * itemsPerPage).Take(itemsPerPage).ToList();
            return View(itemsToShow);
        }

        public ActionResult Search(string name = "", int page = 1)
        {
            var foundItems = db.Items.Where(i => i.Name.Contains(name)).ToList();
            var itemsPerPage = PageConfig.ItemsPerPage;
            ViewBag.CurrentPage = page;
            ViewBag.SearchString = name;
            ViewBag.NumberOfPages = foundItems.Count / itemsPerPage + 1;
            if (foundItems.Count <= 0)
            {
                return PartialView("NoMatchFound");
            }
            var itemsToShow = foundItems.OrderBy(i => i.Name).Skip((page - 1) * itemsPerPage).Take(itemsPerPage).ToList();
            return PartialView(itemsToShow);
        }

        // GET: Items/Create
        public ActionResult Create()
        {
            ViewBag.Animals = db.Animals.Select(a => a.Name).ToList();
            ViewBag.Brands = db.Brands.Select(a => a.Name).ToList();
            ViewBag.ItemSubcategories = db.ItemSubcategories.Select(a => a.Name).ToList();
            ViewBag.Itemcategories = db.ItemCategories.Select(a => a.Name).ToList();
            return View();
        }

        // POST: Items/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Name,Price,Description,Volume")] Item item, string Animal, string Brand, string Subcategory, string Category)
        {
            if (String.IsNullOrEmpty(Animal) || String.IsNullOrEmpty(Brand) ||
                String.IsNullOrEmpty(Subcategory) || String.IsNullOrEmpty(Animal)
                || String.IsNullOrEmpty(item.Name) || String.IsNullOrEmpty(item.Description)
                || (item.Price == null) || String.IsNullOrEmpty(item.Volume))
                ModelState.AddModelError(String.Empty, "Values can't be null");
            if (ModelState.IsValid)
            {
                var animal = db.Animals.Where(a => a.Name == Animal).FirstOrDefault();
                if (animal == null)
                {
                    animal = db.Animals.Add(new Models.Animal() { Name = Animal, CreatedOn = DateTime.UtcNow });
                    db.SaveChanges();
                }
                var brand = db.Brands.Where(a => a.Name == Brand).FirstOrDefault();
                if (brand == null)
                {
                    brand = db.Brands.Add(new Models.Brand() { Name = Brand, CreatedOn = DateTime.UtcNow });
                    db.SaveChanges();
                }
                var category = db.ItemCategories.Where(a => a.Name == Category).FirstOrDefault();
                if (category == null)
                {
                    category = db.ItemCategories.Add(new Models.ItemCategory() { Name = Category, CreatedOn = DateTime.UtcNow });
                    db.SaveChanges();
                }
                var subcategory = db.ItemSubcategories.Where(a => a.Name == Subcategory).FirstOrDefault();
                if (subcategory == null)
                {
                    subcategory = db.ItemSubcategories.Add(new Models.ItemSubcategory() { Name = Category, CreatedOn = DateTime.UtcNow, ItemCategory = category });
                    db.SaveChanges();
                }
                item.Animal = animal;
                item.Brand = brand;
                item.ItemSubcategory = subcategory;
                item.CreatedOn = DateTime.UtcNow;
                db.Items.Add(item);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Animals = db.Animals.Select(a => a.Name).ToList();
            ViewBag.Brands = db.Brands.Select(a => a.Name).ToList();
            ViewBag.ItemSubcategories = db.ItemSubcategories.Select(a => a.Name).ToList();
            ViewBag.Itemcategories = db.ItemCategories.Select(a => a.Name).ToList();
            return View(item);
        }

        // GET: Items/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Item item = db.Items.Find(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            ViewBag.Animals = db.Animals.Select(a => a.Name).ToList();
            ViewBag.Brands = db.Brands.Select(a => a.Name).ToList();
            ViewBag.ItemSubcategories = db.ItemSubcategories.Select(a => a.Name).ToList();
            ViewBag.Itemcategories = db.ItemCategories.Select(a => a.Name).ToList();
            return View(item);
        }

        // POST: Items/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ItemId,Name,Price,Description,Volume")] Item item, string Animal, string Brand, string Subcategory, string Category)
        {
            if (ModelState.IsValid)
            {
                var animal = db.Animals.Where(a => a.Name.GetHashCode().ToString() == Animal).FirstOrDefault();
                if (animal == null)
                {
                    animal = db.Animals.Add(new Models.Animal() { Name = Animal, CreatedOn = DateTime.UtcNow });
                    db.SaveChanges();
                }
                var brand = db.Brands.Where(a => a.Name == Brand).FirstOrDefault();
                if (brand == null)
                {
                    brand = db.Brands.Add(new Models.Brand() { Name = Brand, CreatedOn = DateTime.UtcNow });
                    db.SaveChanges();
                }
                var category = db.ItemCategories.Where(a => a.Name == Category).FirstOrDefault();
                if (category == null)
                {
                    category = db.ItemCategories.Add(new Models.ItemCategory() { Name = Category, CreatedOn = DateTime.UtcNow });
                    db.SaveChanges();
                }
                var subcategory = db.ItemSubcategories.Where(a => a.Name == Subcategory).FirstOrDefault();
                if (subcategory == null)
                {
                    subcategory = db.ItemSubcategories.Add(new Models.ItemSubcategory() { Name = Category, CreatedOn = DateTime.UtcNow, ItemCategory = category });
                    db.SaveChanges();
                }
                db.Entry(item).State = EntityState.Modified;
                item.BrandId = brand.BrandId;
                item.ItemSubcategory = subcategory;
                item.ModifiedOn = DateTime.UtcNow;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Animals = db.Animals.Select(a => a.Name).ToList();
            ViewBag.Brands = db.Brands.Select(a => a.Name).ToList();
            ViewBag.ItemSubcategories = db.ItemSubcategories.Select(a => a.Name).ToList();
            ViewBag.Itemcategories = db.ItemCategories.Select(a => a.Name).ToList();
            return View(item);
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
