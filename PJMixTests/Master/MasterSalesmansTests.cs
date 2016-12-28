namespace PJMixTests.Master
{
    using System.Linq;
    using ECERP.Models.Salesman;
    using ECERP.Utilities;
    using ECERP.Utilities.ModelHelpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MasterSalesmansTests
    {
        [TestMethod]
        public void TestAddSalesman()
        {
            var salesman = CreateTestSalesman();
            SalesmanHelper.AddSalesmanToDatabase(salesman);
            var result1 = CheckIfSalesmanExistsInDatabase(salesman);
            Assert.AreEqual(true, result1);
            RemoveSalesmanFromDatabase(salesman);
            var result2 = CheckIfSalesmanExistsInDatabase(salesman);
            Assert.AreEqual(false, result2);
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
            using (var context = UtilityMethods.createContext())
            {
                var salesmanReturnedFromDatabase = context.Salesmans.FirstOrDefault(e => e.Name.Equals(salesman.Name));
                return salesmanReturnedFromDatabase != null;
            }
        }

        private static void RemoveSalesmanFromDatabase(Salesman salesman)
        {
            using (var context = UtilityMethods.createContext())
            {
                var salesmanToBeRemoved = context.Salesmans.First(s => s.Name.Equals(salesman.Name));
                context.Salesmans.Remove(salesmanToBeRemoved);
                context.SaveChanges();
            }
        }
    }
}
