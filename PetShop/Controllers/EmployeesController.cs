using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using PetShop.Models;

namespace PetShop.Controllers
{
    public class EmployeesController : Controller
    {
        private PetShopContext db = new PetShopContext();

        // GET: Employees
        public ActionResult Index()
        {
            var employee = db.Employees.Include(e => e.Title);
            return View(employee.ToList());
        }

        // GET: Employees/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Employee employee = db.Employees.Find(id);
            if (employee == null)
            {
                return HttpNotFound();
            }
            return View(employee);
        }

        // GET: Employees/Create
        public ActionResult Create()
        {
            ViewBag.TitleId = new SelectList(db.Titles, "TitleId", "Name");
            return View();
        }

        // POST: Employees/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "EmployeeId,Name,Surname,DateOfBirth,EmailAddress,PhoneNumber,TitleId,Salary")] Employee employee)
        {
            if (ModelState.IsValid)
            {
                employee.DateOfBirth = DateTime.Parse(employee.DateOfBirth.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                employee.CreatedOn = DateTime.UtcNow;
                db.Employees.Add(employee);
                db.SaveChanges();
                return RedirectToAction("Register", "Account", new { email = employee.EmailAddress});
            }

            ViewBag.TitleId = new SelectList(db.Titles, "TitleId", "Name", employee.TitleId);
            return View(employee);
        }

        // GET: Employees/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Employee employee = db.Employees.Find(id);
            if (employee == null)
            {
                return HttpNotFound();
            }
            ViewBag.TitleId = new SelectList(db.Titles, "TitleId", "Name", employee.TitleId);
            return View(employee);
        }

        // POST: Employees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "EmployeeId,Name,Surname,DateOfBirth,EmailAddress,PhoneNumber,CreatedOn,ModifiedOn,TitleId,Salary")] Employee employee)
        {
            if (ModelState.IsValid)
            {
                db.Entry(employee).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.TitleId = new SelectList(db.Titles, "TitleId", "Name", employee.TitleId);
            return View(employee);
        }

        // GET: Employees/Delete/5
        public async System.Threading.Tasks.Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Employee employee = db.Employees.Find(id);
            if (employee == null)
            {
                return HttpNotFound();
            }
            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async System.Threading.Tasks.Task<ActionResult> DeleteConfirmed(int id)
        {
            Employee employee = db.Employees.Find(id);
            db.Employees.Remove(employee);
            db.SaveChanges();
            ApplicationUserManager userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var user = await userManager.FindByNameAsync(employee.EmailAddress);
            if(user != null)
                await userManager.DeleteAsync(user);
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
