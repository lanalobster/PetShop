using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PetShop.Models.ViewModels
{
    public class PurchaseViewModel
    {
        public Purchase Purchase { get; set; }
        public List<ItemInStore> ItemsInStore { get; set; } = new List<ItemInStore>();
        public List<Customer> Customers { get; set; } = new List<Customer>();
        public List<ItemInPurchase> ItemsInPurchase { get; set; } = new List<ItemInPurchase>();
        public List<string> ItemErrorMessages { get; set; } = new List<string>();
        public List<string> CustomerErrorMessages { get; set; } = new List<string>();
        public Customer ChosenCustomer { get; set; }

        public PurchaseViewModel(List<ItemInStore> itemsInStore, List<Customer> customers)
        {
            ItemsInStore = itemsInStore;
            Customers = customers;
        }

        public double GetTotalSum()
        {
            double total = 0;
            foreach(var itemInPurchase in this.ItemsInPurchase)
            {
                double subtotal = (double?)(itemInPurchase.Quantity * itemInPurchase.ItemInStore.Item.Price) ?? 0;
                var sale = itemInPurchase.ItemInStore.Item.GetCurrentSale();
                total += sale == null ? subtotal : (subtotal - (subtotal * sale.Percentage / 100.0));
            }
            if(this.ChosenCustomer != null)
            {
                total -= total * this.ChosenCustomer.GetCustomerBonusPercentage() / 100.0;
            }
            return total;
        }
    }
}