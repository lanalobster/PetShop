using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using PetShop.Models;
using PetShop.Models.ViewModels;

namespace PetShop.Controllers
{
    public class PurchasesController : Controller
    {
        private PetShopContext db = new PetShopContext();
        private static PurchaseViewModel currentPurchase;

        // GET: Purchases
        public async System.Threading.Tasks.Task<ActionResult> Index()
        {
            ApplicationUserManager userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var user = await userManager.FindByNameAsync(User.Identity.Name);
            var isUserCashier = user != null ? await userManager.IsInRoleAsync(user.Id, "cashier") : false;

            var purchases = db.Purchases.Include(p => p.Customer).Include(p => p.Employee);
            if (isUserCashier)
            {
                purchases = purchases.Where(p => p.Employee.EmailAddress == user.Email);
            }
            currentPurchase = null;
            return View(purchases.ToList());
        }

        [HttpGet]
        public ActionResult AddItem()
        {
            return PartialView(currentPurchase);
        }

        [HttpPost]
        public ActionResult AddItem(string item, int? quantity)
        {
            currentPurchase.ItemErrorMessages.Clear();
            if (string.IsNullOrEmpty(item))
            {
                currentPurchase.ItemErrorMessages.Add("Please enter name of an item");
                return View("Create", currentPurchase);
            }

            var itemInPurchase = currentPurchase.ItemsInPurchase.
                Where(ip => ip.ItemInStore.Item.Name.ToLower() == item.ToLower()).FirstOrDefault();
            if (itemInPurchase != null)
            {
                if (quantity == null || quantity == 0)
                {
                    currentPurchase.ItemErrorMessages.Add("Please enter quantity of the item. Quantity cannot eq 0.");
                    return View("Create", currentPurchase);
                }
                if (quantity + itemInPurchase.Quantity > itemInPurchase.ItemInStore.Quantity)
                {
                    currentPurchase.ItemErrorMessages.Add("Not enough items in store: " + item + ". Number of items left: " + (itemInPurchase.ItemInStore.Quantity - itemInPurchase.Quantity));
                    return View("Create", currentPurchase);
                }
                itemInPurchase.Quantity += quantity ?? 0;
                return View("Create", currentPurchase);
            }

            var itemInStore = currentPurchase.ItemsInStore.Where(i => i.Item.Name.ToLower() == item.ToLower()).FirstOrDefault();
            itemInPurchase = new ItemInPurchase() { ItemInStore = itemInStore, Quantity = quantity ?? 0, CreatedOn = DateTime.UtcNow };
            if (quantity == null || quantity == 0)
            {
                currentPurchase.ItemErrorMessages.Add("Please enter quantity of the item. Quantity cannot eq 0.");
                return View("Create", currentPurchase);
            }
            if (itemInStore == null)
            {
                currentPurchase.ItemErrorMessages.Add("No such item in store: " + item);
                return View("Create", currentPurchase);
            }
            if (quantity > itemInStore.Quantity)
            {
                currentPurchase.ItemErrorMessages.Add("Not enough items in store: " + item +". Current amount of the item in store: " + itemInStore.Quantity);
                return View("Create", currentPurchase);
            }
            currentPurchase.ItemsInPurchase.Add(itemInPurchase);
            return View("Create", currentPurchase);
        }

        public ActionResult RemoveItem(string itemName)
        {
            currentPurchase.ItemsInPurchase.RemoveAll(ip => ip.ItemInStore.Item.Name.ToLower() == itemName.ToLower());
            return PartialView("Create", currentPurchase);
        }

        public ActionResult RemoveCustomer()
        {
            currentPurchase.ChosenCustomer = null;
            return PartialView("Create", currentPurchase);
        }

        [HttpGet]
        public ActionResult AddCustomer()
        {
            return PartialView(currentPurchase);
        }

        [HttpPost]
        public ActionResult AddCustomer(string phoneNumber)
        {
            currentPurchase.CustomerErrorMessages.Clear();
            var customer = db.Customers.Where(c => c.PhoneNumber.ToLower() == phoneNumber.ToLower()).FirstOrDefault();
            if(customer == null)
            {
                currentPurchase.CustomerErrorMessages.Add("No such customer. Please register a new customer.");
                return View("Create", currentPurchase);
            }
            currentPurchase.ChosenCustomer = customer;
            return View("Create", currentPurchase);
        }

        public ActionResult RegisterCustomer()
        {
            ViewBag.CityId = new SelectList(db.Cities, "CityId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterCustomer([Bind(Include = "Name,Surname,DateOfBirth,EmailAddress,PhoneNumber, Street,CityId")] Customer customer)
        {
            customer.CreatedOn = DateTime.UtcNow;
            if (ModelState.IsValid)
            {
                currentPurchase.ChosenCustomer = customer;
                return View("Create", currentPurchase);
            }

            ViewBag.CityId = new SelectList(db.Cities, "CityId", "Name", customer.CityId);
            return View(customer);
        }

        // GET: Purchases/Create

        [HttpGet]
        public ActionResult Create()
        {
            var itemsInStore = db.ItemInStores.Where(i => i.Quantity != 0).Include(ist => ist.Item).Include(ist => ist.Item.Sale).ToList();
            var customers = db.Customers.ToList();
            currentPurchase = currentPurchase ?? new PurchaseViewModel(itemsInStore, customers);
            currentPurchase.CustomerErrorMessages.Clear();
            currentPurchase.ItemErrorMessages.Clear();
            return View(currentPurchase);
        }

        [HttpPost]
        public ActionResult Create(Purchase purchase)
        {
            var purchaseToAdd = new Purchase();
            var date = DateTime.UtcNow.Date;
            purchaseToAdd.CheckNumber = (db.Purchases.Where(p => DbFunctions.TruncateTime(p.CreatedOn) == date).Count() + 1).ToString();
            purchaseToAdd.CreatedOn = DateTime.UtcNow;
            purchaseToAdd.Employee = db.Employees.Where(e => e.EmailAddress == User.Identity.Name).FirstOrDefault();
            if(currentPurchase.ChosenCustomer?.CustomerId == 0)
            {
                currentPurchase.ChosenCustomer =  db.Customers.Add(currentPurchase.ChosenCustomer);
            }
            purchaseToAdd.Customer = currentPurchase.ChosenCustomer;
            purchaseToAdd.TotalSum = (decimal?)currentPurchase.GetTotalSum();
            if(currentPurchase.ChosenCustomer != null)
                purchase.Customer.Bonuses += Convert.ToInt32(purchase.TotalSum);
            var addedPurchase = db.Purchases.Add(purchaseToAdd);
            foreach (var itemInPurchase in currentPurchase.ItemsInPurchase)
            {
                itemInPurchase.Purchase = addedPurchase;
                db.ItemInPurchases.Add(itemInPurchase);
                var itemInStore = db.ItemInStores.Where(ist => ist.ItemInStoreId == itemInPurchase.ItemInStore.ItemInStoreId).FirstOrDefault();
                itemInStore.Quantity -= itemInPurchase.Quantity;
                db.SaveChanges();
            }
            db.SaveChanges();
            currentPurchase = null;
            return View("Details", addedPurchase);
        }

        // GET: Purchases/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Purchase purchase = db.Purchases.Find(id);
            if (purchase == null)
            {
                return HttpNotFound();
            }
            return View(purchase);
        }

        // GET: Purchases/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Purchase purchase = db.Purchases.Find(id);
            if (purchase == null)
            {
                return HttpNotFound();
            }
            ViewBag.CustomerId = new SelectList(db.Customers, "CustomerId", "Name", purchase.CustomerId);
            ViewBag.EmployeeId = new SelectList(db.Employees, "EmployeeId", "Name", purchase.EmployeeId);
            return View(purchase);
        }

        // POST: Purchases/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "PurchaseId,CreatedOn,ModifiedOn,TotalSum,EmployeeId,CheckNumber,CustomerId")] Purchase purchase)
        {
            if (ModelState.IsValid)
            {
                db.Entry(purchase).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CustomerId = new SelectList(db.Customers, "CustomerId", "Name", purchase.CustomerId);
            ViewBag.EmployeeId = new SelectList(db.Employees, "EmployeeId", "Name", purchase.EmployeeId);
            return View(purchase);
        }

        // GET: Purchases/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Purchase purchase = db.Purchases.Find(id);
            if (purchase == null)
            {
                return HttpNotFound();
            }
            return View(purchase);
        }

        // POST: Purchases/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Purchase purchase = db.Purchases.Find(id);
            db.Purchases.Remove(purchase);
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
