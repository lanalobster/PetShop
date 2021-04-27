using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PetShop.Models.ViewModels
{
    public class OrderViewModel
    {
        public ItemsOrder ItemsOrder { get; set; }
        public List<ItemInOrder> ItemsInOrder { get; set; } = new List<ItemInOrder>();
        public List<Item> Items { get; set; } = new List<Item>();
        public List<StockedItem> StockedItems { get; set; } = new List<StockedItem>();
        public List<Supplier> Suppliers { get; set; } = new List<Supplier>();
        public Supplier ChosenSupplier { get; set; }
        public List<Store> Stores { get; set; } = new List<Store>();
        public Store ChosenStore { get; set; }
        public List<string> ItemErrorMessages { get; set; } = new List<string>();
        public List<string> SupplierErrorMessages { get; set; } = new List<string>();

        public double GetTotalSum()
        {
            double total = 0;
            foreach (var itemInOrder in this.ItemsInOrder)
            {
                double subtotal = (double)(itemInOrder.Quantity * itemInOrder.Price ?? 0);
                total += subtotal;
            }
            return total;
        }
    }
}