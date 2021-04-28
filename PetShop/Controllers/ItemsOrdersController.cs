using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PetShop.Models;
using PetShop.Models.ViewModels;

namespace PetShop.Controllers
{
    public class ItemsOrdersController : Controller
    {
        private PetShopContext db = new PetShopContext();
        private static OrderViewModel currentOrder;

        // GET: ItemsOrders
        public ActionResult Index()
        {
            var itemsOrder = db.ItemsOrders.Include(i => i.Employee).Include(i => i.Supplier);
            var itemsOrdersModels = new List<OrderViewModel>();
            foreach(var order in itemsOrder)
            {
                var stockedItems = new List<StockedItem>();
                foreach(var itemInOrder in order.ItemInOrder)
                {
                    var stockedItem = itemInOrder.StockedItem.FirstOrDefault();
                    if(stockedItem != null)
                        stockedItems.Add(stockedItem);
                }
                itemsOrdersModels.Add(new OrderViewModel { ItemsOrder = order, StockedItems = stockedItems});
            }
            currentOrder = null;
            return View(itemsOrdersModels.ToList());
        }

        [HttpGet]
        public ActionResult AddStore()
        {
            return PartialView(currentOrder);
        }

        [HttpPost]
        public ActionResult AddStore(int storeId)
        {
            var store = db.Stores.Where(st => st.StoreId == storeId).FirstOrDefault();
            currentOrder.ChosenStore = store;
            return View("Create", currentOrder);
        }


        [HttpGet]
        public ActionResult AddItem()
        {
            return PartialView(currentOrder);
        }

        [HttpPost]
        public ActionResult AddItem(string itemName, int? quantity, double? price)
        {
            currentOrder.ItemErrorMessages.Clear();
            if (string.IsNullOrEmpty(itemName))
                currentOrder.ItemErrorMessages.Add("Please enter name of an item");
            if (quantity == null || quantity == 0)
                currentOrder.ItemErrorMessages.Add("Please enter quantity of the item. Quantity cannot eq 0.");
            if (price == null)
                currentOrder.ItemErrorMessages.Add("Please enter price of the item.");
            if(currentOrder.ItemErrorMessages.Count() > 0)
                return View("Create", currentOrder);
            var itemInOrder = currentOrder.ItemsInOrder.
                Where(ist => ist.Item.Name.ToLower() == itemName.ToLower()).FirstOrDefault();
            if (itemInOrder != null)
            {
                itemInOrder.Quantity += quantity ?? 0;
                return View("Create", currentOrder);
            }
            var item = db.Items.Where(i => i.Name.ToLower() == itemName.ToLower()).FirstOrDefault();
            if(item == null)
            {
                currentOrder.ItemErrorMessages.Add("No such item in catalogue: " + item + ".");
                currentOrder.ItemErrorMessages.Add("No such item in catalogue: " + item + ".");
                return View("Create", currentOrder);
            }

            itemInOrder = new ItemInOrder() { Item = item, Quantity = quantity ?? 0, CreatedOn = DateTime.Now, Price = (decimal?)price };
            currentOrder.ItemsInOrder.Add(itemInOrder);
            return View("Create", currentOrder);
        }

        public ActionResult RemoveItem(string itemName)
        {
            currentOrder.ItemsInOrder.RemoveAll(io => io.Item.Name.ToLower() == itemName.ToLower());
            return PartialView("Create", currentOrder);
        }

        public ActionResult RegisterItem()
        {
            return View("~/Views/Items/Create.cshtml");
        }

        public ActionResult RemoveSupplier()
        {
            currentOrder.ChosenSupplier = null;
            return PartialView("Create", currentOrder);
        }

        [HttpGet]
        public ActionResult AddSupplier()
        {
            return PartialView(currentOrder);
        }

        [HttpPost]
        public ActionResult AddSupplier(string phoneNumber)
        {
            currentOrder.SupplierErrorMessages.Clear();
            var supplier = db.Suppliers.Where(c => c.PhoneNumber.ToLower() == phoneNumber.ToLower()).FirstOrDefault();
            if (supplier == null)
            {
                currentOrder.SupplierErrorMessages.Add("No such supplier. Please register a new supplier.");
                return View("Create", currentOrder);
            }
            currentOrder.ChosenSupplier = supplier;
            return View("Create", currentOrder);
        }

        public ActionResult RegisterSupplier()
        {
            ViewBag.CityId = new SelectList(db.Cities, "CityId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterSupplier([Bind(Include = "Name,Surname,DateOfBirth,EmailAddress,PhoneNumber, Street,CityId")] Supplier supplier)
        {         
            if (ModelState.IsValid)
            {
                supplier.CreatedOn = DateTime.UtcNow;
                currentOrder.ChosenSupplier = supplier;
                return View("Create", currentOrder);
            }

            ViewBag.CityId = new SelectList(db.Cities, "CityId", "Name", supplier.CityId);
            return View(supplier);
        }

        public ActionResult Create()
        {
            var suppliers = db.Suppliers.ToList();
            var items = db.Items.ToList();
            var stores = db.Stores.ToList();
            currentOrder = currentOrder ?? new OrderViewModel() { Suppliers = suppliers, Items = items, Stores = stores };
            currentOrder.ItemErrorMessages.Clear();
            currentOrder.SupplierErrorMessages.Clear();
            return View(currentOrder);
        }

        [HttpPost]
        public ActionResult Create(ItemsOrder itemsOrder)
        {
            var orderToAdd = new ItemsOrder();
            var date = DateTime.UtcNow.Date;
            orderToAdd.Store = currentOrder.ChosenStore;
            orderToAdd.CreatedOn = DateTime.UtcNow;
            orderToAdd.Employee = db.Employees.Where(e => e.EmailAddress == User.Identity.Name).FirstOrDefault();
            orderToAdd.TotalSum = (decimal?)currentOrder.GetTotalSum();
            if (currentOrder.ChosenSupplier?.SupplierId == 0)
            {
                currentOrder.ChosenSupplier = db.Suppliers.Add(currentOrder.ChosenSupplier);
            }
            orderToAdd.Supplier = currentOrder.ChosenSupplier;
            var addedOrder = db.ItemsOrders.Add(orderToAdd);
            foreach (var itemInOrder in currentOrder.ItemsInOrder)
            {
                itemInOrder.ItemsOrder = addedOrder;
                db.ItemInOrders.Add(itemInOrder);
                db.SaveChanges();
            }
            db.SaveChanges();
            currentOrder = null;
            ViewBag.Status = "Not completed";
            return RedirectToAction("Details", new {id = addedOrder.ItemsOrderId });
        }

        [HttpPost]
        public ActionResult Stock()
        {
            var employee = db.Employees.Where(e => e.EmailAddress == User.Identity.Name).FirstOrDefault();
            var order = currentOrder.ItemsOrder;
            foreach (var itemInOrder in order.ItemInOrder)
            {
                var stockedItem = new StockedItem()
                {
                    ItemInOrder = itemInOrder,
                    Employee = employee,
                    Quantity = itemInOrder.Quantity,
                };
                currentOrder.StockedItems.Add(stockedItem);
            }
            ViewBag.Status = "Finishing up";
            return View("Details", currentOrder);
        }

        public ActionResult Complete()
        {
            var employee = db.Employees.Where(e => e.EmailAddress == User.Identity.Name).FirstOrDefault();
            var order = currentOrder.ItemsOrder;
            foreach (var stockedItem in currentOrder.StockedItems)
            {
                var stockedItemToAdd = new StockedItem()
                {
                    CreatedOn = DateTime.UtcNow,
                    ItemInOrder = stockedItem.ItemInOrder,
                    Employee = employee,
                    Quantity = stockedItem.ItemInOrder.Quantity,
                };
                var addedStockedItem = db.StockedItems.Add(stockedItemToAdd);
                db.SaveChanges();
                var itemInStore = new ItemInStore()
                {
                    CreatedOn = DateTime.UtcNow,
                    Item = stockedItem.ItemInOrder.Item,
                    Quantity = addedStockedItem.Quantity,
                    StockedItem = addedStockedItem,
                    Store = order.Store,
                };
                db.ItemInStores.Add(itemInStore);
                db.SaveChanges();
            }
            ViewBag.Status = "Complete";
            return RedirectToAction("Details", new { id = currentOrder.ItemsOrder.ItemsOrderId } );
        }

        [HttpGet]
         public ActionResult EditQuantity(int itemInOrderId)
        {
            var stockedItem = currentOrder.StockedItems.Where(si => si.ItemInOrder.ItemInOrderId == itemInOrderId).FirstOrDefault();
            if (stockedItem == null)
            {
                return HttpNotFound();
            }
            return View(stockedItem);
         }

        [HttpPost]
        public ActionResult EditQuantity(int itemInOrderId, int quantity)
        {
            var stockedItem = currentOrder.StockedItems.Where(si => si.ItemInOrder.ItemInOrderId == itemInOrderId).FirstOrDefault();
            if (stockedItem == null)
            {
                return HttpNotFound();
            }
            stockedItem.Quantity = quantity;
            ViewBag.Status = "Finishing up";
            //var order = db.ItemsOrders.Where(io => io.ItemsOrderId == stockedItem.ItemInOrder.ItemsOrderId).FirstOrDefault();
            return View("Details", currentOrder);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ItemsOrder itemsOrder = db.ItemsOrders.Find(id);
            var stockedItems = db.StockedItems.Where(si => si.ItemInOrder.ItemsOrderId == itemsOrder.ItemsOrderId);
            currentOrder = new OrderViewModel()
            {
                ChosenSupplier = itemsOrder.Supplier,
                ChosenStore = itemsOrder.Store,
                ItemsInOrder = itemsOrder.ItemInOrder.ToList(),
                ItemsOrder = itemsOrder,
                StockedItems = stockedItems.ToList()
            };

            if (itemsOrder == null)
            {
                return HttpNotFound();
            }
            var completed = itemsOrder.ItemInOrder.All(io => io.StockedItem?.Count() > 0);
            ViewBag.Status = completed ? "Completed" : "Not completed";
            return View(currentOrder);
        }
        // GET: ItemsOrders/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ItemsOrder itemsOrder = db.ItemsOrders.Find(id);
            if (itemsOrder == null)
            {
                return HttpNotFound();
            }
            ViewBag.EmployeeId = new SelectList(db.Employees, "EmployeeId", "Name", itemsOrder.EmployeeId);
            ViewBag.SupplierId = new SelectList(db.Suppliers, "SupplierId", "Name", itemsOrder.SupplierId);
            return View(itemsOrder);
        }

        // POST: ItemsOrders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "TotalSum,CreatedOn,ModifiedOn,ItemsOrderId,SupplierId,EmployeeId")] ItemsOrder itemsOrder)
        {
            if (ModelState.IsValid)
            {
                db.Entry(itemsOrder).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.EmployeeId = new SelectList(db.Employees, "EmployeeId", "Name", itemsOrder.EmployeeId);
            ViewBag.SupplierId = new SelectList(db.Suppliers, "SupplierId", "Name", itemsOrder.SupplierId);
            return View(itemsOrder);
        }

        // GET: ItemsOrders/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ItemsOrder itemsOrder = db.ItemsOrders.Find(id);
            if (itemsOrder == null)
            {
                return HttpNotFound();
            }
            return View(itemsOrder);
        }

        // POST: ItemsOrders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ItemsOrder itemsOrder = db.ItemsOrders.Find(id);
            db.ItemsOrders.Remove(itemsOrder);
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
