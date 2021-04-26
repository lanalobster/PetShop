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
            return RedirectToAction("Search");
        }

        public ActionResult Search(string name="", int page = 1)
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
            if(string.IsNullOrEmpty(name))
                return View("Index", itemsToShow);
            return PartialView(itemsToShow);
        }

        // GET: Items/Details/5
        public ActionResult Details(int? id)
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
            return View(item);
        }

        // GET: Items/Create
        public ActionResult Create()
        {
            ViewBag.AnimalId = new SelectList(db.Animals, "AnimalId", "Name");
            ViewBag.BrandId = new SelectList(db.Brands, "BrandId", "Name");
            ViewBag.ItemSubcategoryId = new SelectList(db.ItemSubcategories, "ItemSubcategoryId", "Name");
            return View();
        }

        // POST: Items/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ItemId,Name,CreatedOn,ModifiedOn,BrandId,Price,AnimalId,Description,ItemSubcategoryId,Volume")] Item item)
        {
            if (ModelState.IsValid)
            {
                db.Items.Add(item);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AnimalId = new SelectList(db.Animals, "AnimalId", "Name", item.AnimalId);
            ViewBag.BrandId = new SelectList(db.Brands, "BrandId", "Name", item.BrandId);
            ViewBag.ItemSubcategoryId = new SelectList(db.ItemSubcategories, "ItemSubcategoryId", "Name", item.ItemSubcategoryId);
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
            ViewBag.AnimalId = new SelectList(db.Animals, "AnimalId", "Name", item.AnimalId);
            ViewBag.BrandId = new SelectList(db.Brands, "BrandId", "Name", item.BrandId);
            ViewBag.ItemSubcategoryId = new SelectList(db.ItemSubcategories, "ItemSubcategoryId", "Name", item.ItemSubcategoryId);
            return View(item);
        }

        // POST: Items/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ItemId,Name,CreatedOn,ModifiedOn,BrandId,Price,AnimalId,Description,ItemSubcategoryId,Volume")] Item item)
        {
            if (ModelState.IsValid)
            {
                db.Entry(item).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AnimalId = new SelectList(db.Animals, "AnimalId", "Name", item.AnimalId);
            ViewBag.BrandId = new SelectList(db.Brands, "BrandId", "Name", item.BrandId);
            ViewBag.ItemSubcategoryId = new SelectList(db.ItemSubcategories, "ItemSubcategoryId", "Name", item.ItemSubcategoryId);
            return View(item);
        }

        // GET: Items/Delete/5
        public ActionResult Delete(int? id)
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
            return View(item);
        }

        // POST: Items/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Item item = db.Items.Find(id);
            db.Items.Remove(item);
            db.SaveChanges();
            return RedirectToAction("Index");
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
