//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PetShop.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class PetShopContext : DbContext
    {
        public PetShopContext()
            : base("name=PetShopContext")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Animal> Animals { get; set; }
        public virtual DbSet<Brand> Brands { get; set; }
        public virtual DbSet<City> Cities { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<Item> Items { get; set; }
        public virtual DbSet<ItemCategory> ItemCategories { get; set; }
        public virtual DbSet<ItemInOrder> ItemInOrders { get; set; }
        public virtual DbSet<ItemInPurchase> ItemInPurchases { get; set; }
        public virtual DbSet<ItemInStore> ItemInStores { get; set; }
        public virtual DbSet<ItemsOrder> ItemsOrders { get; set; }
        public virtual DbSet<ItemSubcategory> ItemSubcategories { get; set; }
        public virtual DbSet<Purchase> Purchases { get; set; }
        public virtual DbSet<Region> Regions { get; set; }
        public virtual DbSet<Sale> Sales { get; set; }
        public virtual DbSet<StockedItem> StockedItems { get; set; }
        public virtual DbSet<Store> Stores { get; set; }
        public virtual DbSet<Supplier> Suppliers { get; set; }
        public virtual DbSet<Title> Titles { get; set; }
    }
}
