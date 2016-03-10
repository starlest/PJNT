namespace PutraJayaNT.Utilities.ModelHelpers
{
    using System.Linq;
    using System.Transactions;
    using Database.Item;
    using Database.Supplier;
    using Models.Inventory;

    public static class InventoryHelper
    {
        public static void AddItemToDatabase(Item item)
        {
            using (var context = new ERPContext())
            {
                AddItemToDatabaseContext(context, item);
                context.SaveChanges();
            }
        }

        public static void SaveItemEditsToDatabase(Item editingItem, Item editedItem)
        {
            using (var ts = new TransactionScope())
            {
                var context = new ERPContext();
                SaveItemEditsToDatabaseContext(context, editingItem, editedItem);
                ts.Complete();
            }
        }

        public static void DeepCopyItemProperties(Item fromItem, ref Item toItem)
        {
            toItem.Name = fromItem.Name;
            toItem.Category = fromItem.Category;
            toItem.PurchasePrice = fromItem.PurchasePrice;
            toItem.SalesPrice = fromItem.SalesPrice;
            toItem.SalesExpense = fromItem.SalesExpense;
            toItem.UnitName = fromItem.UnitName;
            toItem.PiecesPerUnit = fromItem.PiecesPerUnit;
        }

        #region Add Item Helper Methods
        private static void AddItemToDatabaseContext(ERPContext context, Item item)
        {
            AttachItemSupplierToDatabaseContext(context, item);
            AttachItemCategoryToDatabaseContext(context, item);
            context.Inventory.Add(item);
        }

        private static void AttachItemCategoryToDatabaseContext(ERPContext context, Item item)
        {
            item.Category = context.ItemCategories.First(category => category.ID.Equals(item.Category.ID));
        }

        private static void AttachItemSupplierToDatabaseContext(ERPContext context, Item item)
        {
            var supplierToBeAttached = item.Suppliers.First();
            item.Suppliers.RemoveAt(0);
            supplierToBeAttached = context.Suppliers.FirstOrDefault(e => e.ID.Equals(supplierToBeAttached.ID));
            item.Suppliers.Add(supplierToBeAttached);
        }
        #endregion

        #region Edit Item Helper Methods
        private static void SaveItemEditsToDatabaseContext(ERPContext context, Item editingItem, Item editedItem)
        {
            DatabaseItemHelper.AttachToObjectFromDatabaseContext(context, ref editingItem);

            DeepCopyItemProperties(editedItem, ref editingItem);
            var categoryToBeAttached = editingItem.Category;
            DatabaseItemCategoryHelper.AttachToObjectFromDatabaseContext(context, ref categoryToBeAttached);
            editingItem.Category = categoryToBeAttached;

            // Assign and attach to Database Context editedItem's suppliers to editingItem's 
            var editingSuppliers = editingItem.Suppliers;
            editingItem.Suppliers.ToList().ForEach(supplier => editingSuppliers.Remove(supplier));
            foreach (var supplier in editedItem.Suppliers.ToList())
            {
                var supplierToBeAttached = supplier;
                DatabaseSupplierHelper.AttachToDatabaseContext(context, ref supplierToBeAttached);
                editingItem.Suppliers.Add(supplierToBeAttached);
            }

            // Assign and attach to Database Context editedItem's alternativeSalesPrices to editingItem's 
            var editingSalesPrices = editingItem.AlternativeSalesPrices;
            editingItem.AlternativeSalesPrices.ToList().ForEach(altSalesPrice => editingSalesPrices.Remove(altSalesPrice));
            foreach (var altSalesPrice in editedItem.AlternativeSalesPrices.ToList())
            {
                altSalesPrice.Item = editingItem;
                context.AlternativeSalesPrices.Add(altSalesPrice);
            }

            context.SaveChanges();
        }
        #endregion
    }
}
