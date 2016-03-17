﻿namespace PutraJayaNT.Utilities.ModelHelpers
{
    using System.Linq;
    using System.Transactions;
    using System.Windows;
    using Models.Inventory;

    public static class InventoryHelper
    {
        public static void AddItemToDatabase(Item item)
        {
            var success = true;
            var context = new ERPContext();
            try
            {
                AddItemToDatabaseContext(context, item);
                context.SaveChanges();
            }
            catch
            {
                success = false;
                MessageBox.Show("The item ID is already being used.", "Invalid ID", MessageBoxButton.OK);
            }
            finally
            {
                if (success)
                    MessageBox.Show("Successfully added item!", "Success", MessageBoxButton.OK);
                context.Dispose();
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

        public static void DeepCopyItemProperties(Item fromItem, Item toItem)
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
            editingItem = context.Inventory.First(item => item.ItemID.Equals(editingItem.ItemID));
            DeepCopyItemProperties(editedItem, editingItem);
            editingItem.Category = context.ItemCategories.First(category => category.ID.Equals(editingItem.Category.ID));
            AssignEditedItemSuppliersToEditingSupplier(context, editingItem, editedItem);         

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

        private static void AssignEditedItemSuppliersToEditingSupplier(ERPContext context, Item editingItem, Item editedItem)
        {
            // Assign editedItem's suppliers to editingItem's 
            var editingSuppliers = editingItem.Suppliers;
            editingItem.Suppliers.ToList().ForEach(supplier => editingSuppliers.Remove(supplier));
            foreach (var attachedSupplier in editedItem.Suppliers.ToList()
                .Select(supplierToBeAdded => context.Suppliers.First(supplier => supplier.ID.Equals(supplierToBeAdded.ID))))
                editingItem.Suppliers.Add(attachedSupplier);
        }

        #endregion
    }
}