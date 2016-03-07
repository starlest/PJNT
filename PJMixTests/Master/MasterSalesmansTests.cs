using PutraJayaNT.Models.Salesman;
using PutraJayaNT.ViewModels.Master.Salesmans;

namespace PJMixTests.Master
{
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using PutraJayaNT.Utilities;
    using PutraJayaNT.Models;

    [TestClass]
    public class MasterSalesmansTests
    {
        [TestMethod]
        public void TestAddSalesman()
        {
            var salesman = CreateTestSalesman();
            MasterSalesmansNewEntryVM.AddSalesmanToDatabase(salesman);
            var result1 = CheckIfSalesmanExistsInDatabase(salesman);
            Assert.AreEqual(true, result1);
            RemoveSalesmanFromDatabase(salesman);
            var result2 = CheckIfSalesmanExistsInDatabase(salesman);
            Assert.AreEqual(false, result2);
        }

        [TestMethod]
        public void TestEditSalesman()
        {
            //var originalSupplier = DatabaseSupplierHelper.FirstOrDefault(supplier => supplier.ID == 1);
            //var editedSupplier = CreateTestSalesman();
            //editedSupplier.ID = originalSupplier.ID;

            //MasterSuppliersEditVM.SaveSupplierEditsToDatabase(originalSupplier, editedSupplier);
            //var editedSupplierFromDatabase = DatabaseSupplierHelper.FirstOrDefault(supplier => supplier.ID == 1);
            //var result1 = CompareSuppliers(editedSupplierFromDatabase, editedSupplier);
            //var result2 = CheckIfSalesmanExistsInDatabase(editedSupplierFromDatabase);
            //var result3 = CheckIfSalesmanExistsInDatabase(originalSupplier);

            //// Revert editedSupplier back to original values
            //MasterSuppliersEditVM.SaveSupplierEditsToDatabase(editedSupplier, originalSupplier);
            //editedSupplierFromDatabase = DatabaseSupplierHelper.FirstOrDefault(supplier => supplier.ID == 1);
            //var result4 = CompareSuppliers(editedSupplierFromDatabase, originalSupplier);
            //var result5 = CheckIfSalesmanExistsInDatabase(editedSupplierFromDatabase);
            //var result6 = CheckIfSalesmanExistsInDatabase(editedSupplier);

            //Assert.AreEqual(result1, result4);
            //Assert.AreNotEqual(result2, result3);
            //Assert.AreNotEqual(result5, result6);
        }

        private static bool CompareSuppliers(Supplier supplier1, Supplier supplier2)
        {
            return supplier1.ID.Equals(supplier2.ID) && supplier1.Name.Equals(supplier2.Name)
                   && supplier1.Address.Equals(supplier2.Address) && supplier1.GSTID.Equals(supplier2.GSTID);
        }

        private static Salesman CreateTestSalesman()
        {
            return new Salesman
            {
                ID = -10000,
                Name = "testsalesman"
            };
        }

        private static bool CheckIfSalesmanExistsInDatabase(Salesman salesman)
        {
            using (var context = new ERPContext())
            {
                var salesmanReturnedFromDatabase = context.Salesmans.FirstOrDefault(e => e.Name.Equals(salesman.Name));
                return salesmanReturnedFromDatabase != null;
            }
        }

        private static void RemoveSalesmanFromDatabase(Salesman salesman)
        {
            using (var context = new ERPContext())
            {
                var salesmanToBeRemoved = context.Salesmans.First(s => s.Name.Equals(salesman.Name));
                context.Salesmans.Remove(salesmanToBeRemoved);
                context.SaveChanges();
            }
        }
    }
}
