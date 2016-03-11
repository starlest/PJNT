namespace PJMixTests.Master
{
    using PutraJayaNT.Utilities.ModelHelpers;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Database.Item;
    using Database.Supplier;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using PutraJayaNT.Models;
    using PutraJayaNT.Models.Inventory;
    using PutraJayaNT.Utilities;
    using PutraJayaNT.ViewModels.Master.Inventory;

    [TestClass]
    public class MasterInventoryTests
    {
        [TestMethod]
        public void TestAddItem()
        {
            var item = CreateTestItem();
            InventoryHelper.AddItemToDatabase(item);
            var result1 = CheckIfItemExistsInDatabase(item);
            RemoveItemFromDatabase(item);
            var result2 = CheckIfItemExistsInDatabase(item);
            Assert.AreNotEqual(result1, result2);
        }

        [TestMethod] 
        public void TestEditItem()
        {
            var originalItem = DatabaseItemHelper.FirstOrDefault(item => item.ItemID.Equals("BH001"));
            var editedItem = CreateTestItem();
            editedItem.ItemID = originalItem.ItemID;

            InventoryHelper.SaveItemEditsToDatabase(originalItem, editedItem);
            var editedItemFromDatabase = DatabaseItemHelper.FirstOrDefault(item => item.ItemID.Equals("BH001"));
            var result1 = CompareItems(editedItemFromDatabase, editedItem);

            // Revert editedCustomer back to original values
            InventoryHelper.SaveItemEditsToDatabase(editedItem, originalItem);
            editedItemFromDatabase = DatabaseItemHelper.FirstOrDefault(item => item.ItemID.Equals("BH001"));
            var result2 = CompareItems(editedItemFromDatabase, originalItem);

            Assert.AreEqual(result1, result2);
        }

        [TestMethod]
        public void TestActivateItem()
        {
            var testItem = DatabaseItemHelper.FirstOrDefault(item =>  item.ItemID.Equals("PLP001"));
            MasterInventoryVM.DeactivateItemInDatabase(testItem);
            testItem = DatabaseItemHelper.FirstOrDefault(item => item.ItemID.Equals("PLP001"));
            Assert.AreEqual(testItem.Active, false);

            MasterInventoryVM.ActivateItemInDatabase(testItem);
            testItem = DatabaseItemHelper.FirstOrDefault(item => item.ItemID.Equals("PLP001"));
            Assert.AreEqual(testItem.Active, true);
        }

        private static bool CompareItems(Item item1, Item item2)
        {
            return CompareItemProperties(item1, item2) && CompareItemCollections(item1, item2);
        }

        private static bool CompareItemProperties(Item item1, Item item2)
        {
            return item1.Active.Equals(item2.Active) && item1.ItemID.Equals(item2.ItemID)
                   && item1.Category.ID.Equals(item2.Category.ID) && item1.Name.Equals(item2.Name)
                   && item1.PiecesPerUnit.Equals(item2.PiecesPerUnit) && item1.PurchasePrice.Equals(item2.PurchasePrice)
                   && item1.SalesExpense.Equals(item2.SalesExpense) && item1.SalesPrice.Equals(item2.SalesPrice) &&
                   item1.UnitName.Equals(item2.UnitName);
        }

        private static bool CompareItemCollections(Item item1, Item item2)
        {
            if (item1.Suppliers.Any(supplier1 => !item2.Suppliers.Contains(supplier1)))
            {
                return false;
            }

            if (
                item1.AlternativeSalesPrices.Any(
                    altSalesPrice1 => !item2.AlternativeSalesPrices.Contains(altSalesPrice1)))
            {
                return false;
            }

            return item1.Suppliers.Count == item2.Suppliers.Count && item1.AlternativeSalesPrices.Count == item2.AlternativeSalesPrices.Count;
        }

        private static Item CreateTestItem()
        {
            var testItem = new Item
            {
                ItemID = "test123",
                Active = true,
                Category = DatabaseItemCategoryHelper.FirstOrDefault(category => category.ID.Equals(1)),
                Name = "testitem",
                PiecesPerUnit = 12,
                PurchasePrice = 12000m/12,
                SalesPrice = 13000m/12,
                SalesExpense = 1000m/12,
                UnitName = "ctn",
                Suppliers = new ObservableCollection<Supplier>(),
                AlternativeSalesPrices = new ObservableCollection<AlternativeSalesPrice>()
            };

            testItem.Suppliers.Add(DatabaseSupplierHelper.FirstOrDefault(supplier => supplier.ID.Equals(1)));
            testItem.AlternativeSalesPrices.Add(new AlternativeSalesPrice
            {
                Item = testItem,
                Name = "testprice",
                SalesPrice = 20000m
            });

            return testItem;
        }

        private static bool CheckIfItemExistsInDatabase(Item itemInCheck)
        {
            return DatabaseItemHelper.FirstOrDefault(item => item.ItemID.Equals(itemInCheck.ItemID)) != null;
        }

        private static void RemoveItemFromDatabase(Item itemToBeRemoved)
        {
            using (var context = new ERPContext())
            {
                itemToBeRemoved = context.Inventory.First(item => item.ItemID.Equals(itemToBeRemoved.ItemID));
                context.Inventory.Remove(itemToBeRemoved);
                context.SaveChanges();
            }
        }
    }
}
