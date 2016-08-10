using PutraJayaNT.Utilities.ModelHelpers;

namespace PJMixTests.Master
{
    using System.Linq;
    using Database.Supplier;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using PutraJayaNT.Utilities;
    using PutraJayaNT.ViewModels.Master.Suppliers;
    using PutraJayaNT.Models;

    [TestClass]
    public class MasterSuppliersTests
    {
        [TestMethod]
        public void TestAddSupplier()
        {
            var supplier = CreateTestSupplier();
            SupplierHelper.AddSupplierAlongWithItsLedgerToDatabase(supplier);
            var result1 = CheckIfSupplierAndItsLedgerExistsInDatabase(supplier);
            Assert.AreEqual(true, result1);
            RemoveSupplierAndItsLedgerFromDatabase(supplier);
            var result2 = CheckIfSupplierAndItsLedgerExistsInDatabase(supplier);
            Assert.AreEqual(false, result2);
        }

        [TestMethod]
        public void TestEditSupplier()
        {
            var originalSupplier = DatabaseSupplierHelper.FirstOrDefault(supplier => supplier.ID >= 1);
            var editedSupplier = CreateTestSupplier();
            editedSupplier.ID = originalSupplier.ID;

            SupplierHelper.SaveSupplierEditsToDatabase(originalSupplier, editedSupplier);
            var editedSupplierFromDatabase = DatabaseSupplierHelper.FirstOrDefault(supplier => supplier.ID >= 1);
            var result1 = CompareSuppliers(editedSupplierFromDatabase, editedSupplier);
            var result2 = CheckIfSupplierAndItsLedgerExistsInDatabase(editedSupplierFromDatabase);
            var result3 = CheckIfSupplierAndItsLedgerExistsInDatabase(originalSupplier);

            // Revert editedSupplier back to original values
            SupplierHelper.SaveSupplierEditsToDatabase(editedSupplier, originalSupplier);
            editedSupplierFromDatabase = DatabaseSupplierHelper.FirstOrDefault(supplier => supplier.ID >= 1);
            var result4 = CompareSuppliers(editedSupplierFromDatabase, originalSupplier);
            var result5 = CheckIfSupplierAndItsLedgerExistsInDatabase(editedSupplierFromDatabase);
            var result6 = CheckIfSupplierAndItsLedgerExistsInDatabase(editedSupplier);

            Assert.AreEqual(result1, result4);
            Assert.AreNotEqual(result2, result3);
            Assert.AreNotEqual(result5, result6);
        }

        private static bool CompareSuppliers(Supplier supplier1, Supplier supplier2)
        {
            return supplier1.ID.Equals(supplier2.ID) && supplier1.Name.Equals(supplier2.Name)
                   && supplier1.Address.Equals(supplier2.Address) && supplier1.GSTID.Equals(supplier2.GSTID);
        }

        private static Supplier CreateTestSupplier()
        {
            return new Supplier
            {
                ID = -10000,
                Active = true,
                Address = "ala land",
                GSTID = 02345,
                Name = "Test Supplier"
            };
        }

        private static bool CheckIfSupplierAndItsLedgerExistsInDatabase(Supplier supplier)
        {
            using (var context = new ERPContext(UtilityMethods.GetDBName(), UtilityMethods.GetIpAddress()))
            {
                var supplierReturnedFromDatabase = context.Suppliers.FirstOrDefault(e => e.ID.Equals(supplier.ID));
                if (supplierReturnedFromDatabase == null) return false;

                var supplierLedgerAccountName = supplier.Name + " Accounts Payable";
                var supplierLedgerAccountReturnedFromDatabase = context.Ledger_Accounts.FirstOrDefault(e => e.Name.Equals(supplierLedgerAccountName));
                if (supplierLedgerAccountReturnedFromDatabase == null) return false;

                var supplierLedgerGeneralReturnedFromDatabase = context.Ledger_General.FirstOrDefault(e => e.ID.Equals(supplierLedgerAccountReturnedFromDatabase.ID));
                if (supplierLedgerGeneralReturnedFromDatabase == null) return false;

                var customerLedgerAccountBalanceReturnedFromDatabase = context.Ledger_Account_Balances.FirstOrDefault(e => e.ID.Equals(supplierLedgerAccountReturnedFromDatabase.ID));
                return customerLedgerAccountBalanceReturnedFromDatabase != null;
            }
        }

        private static void RemoveSupplierLedgerFromDatabaseContext(ERPContext context, Supplier supplier)
        {
            var supplierLedgerAccountName = supplier.Name + " Accounts Payable";
            var supplierLedgerAccount = context.Ledger_Accounts.First(e => e.Name.Equals(supplierLedgerAccountName));
            var supplierLedgerGeneral = context.Ledger_General.First(e => e.ID.Equals(supplierLedgerAccount.ID));
            var supplierLedgerAccountBalance = context.Ledger_Account_Balances.First(e => e.ID.Equals(supplierLedgerAccount.ID));
            context.Ledger_Account_Balances.Remove(supplierLedgerAccountBalance);
            context.Ledger_General.Remove(supplierLedgerGeneral);
            context.Ledger_Accounts.Remove(supplierLedgerAccount);
        }

        private static void RemoveSupplierAndItsLedgerFromDatabase(Supplier supplier)
        {
            using (var context = new ERPContext(UtilityMethods.GetDBName(), UtilityMethods.GetIpAddress()))
            {
                var supplierToBeRemoved = context.Suppliers.First(s => s.Name.Equals(supplier.Name));
                context.Suppliers.Remove(supplierToBeRemoved);
                RemoveSupplierLedgerFromDatabaseContext(context, supplier);
                context.SaveChanges();
            }
        }
    }
}
