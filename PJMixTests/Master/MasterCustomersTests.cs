using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PutraJayaNT.Models.Customer;
using PutraJayaNT.Utilities;
using PutraJayaNT.Utilities.Database.Customer;
using PutraJayaNT.Utilities.ModelHelpers;
using PutraJayaNT.ViewModels.Master.Customers;

namespace PJMixTests.Master
{
    [TestClass]
    public class MasterCustomersTests
    {
        [TestMethod]
        public void TestAddCustomer()
        {
            var customer = CreateTestCustomer();
            CustomerHelper.AddCustomerAlongWithItsLedgerToDatabase(customer);
            var result1 = CheckIfCustomerAndItsLedgerExistsInDatabase(customer);
            Assert.AreEqual(true, result1);
            RemoveCustomerAndItsLedgerFromDatabase(customer);
            var result2 = CheckIfCustomerAndItsLedgerExistsInDatabase(customer);
            Assert.AreEqual(false, result2);
        }

        [TestMethod]
        public void TestEditCustomer()
        {
            var originalCustomer = DatabaseCustomerHelper.FirstOrDefault(customer => customer.ID >= 1);
            var editedCustomer = CreateTestCustomer();
            editedCustomer.ID = originalCustomer.ID;

            CustomerHelper.SaveCustomerEditsToDatabase(originalCustomer, editedCustomer);
            var editedCustomerFromDatabase = DatabaseCustomerHelper.FirstOrDefault(customer => customer.ID >= 1);
            var result1 = CompareCustomers(editedCustomerFromDatabase, editedCustomer);
            var result2 = CheckIfCustomerAndItsLedgerExistsInDatabase(editedCustomerFromDatabase);
            var result3 = CheckIfCustomerAndItsLedgerExistsInDatabase(originalCustomer);

            // Revert editedCustomer back to original values
            CustomerHelper.SaveCustomerEditsToDatabase(editedCustomer, originalCustomer);
            editedCustomerFromDatabase = DatabaseCustomerHelper.FirstOrDefault(customer => customer.ID >= 1);
            var result4 = CompareCustomers(editedCustomerFromDatabase, originalCustomer);
            var result5 = CheckIfCustomerAndItsLedgerExistsInDatabase(editedCustomerFromDatabase);
            var result6 = CheckIfCustomerAndItsLedgerExistsInDatabase(editedCustomer);

            Assert.AreEqual(result1, result4);
            Assert.AreNotEqual(result2, result3);
            Assert.AreNotEqual(result5, result6);
        }

        [TestMethod]
        public void TestActivateCustomer()
        {
            var testCustomer = DatabaseCustomerHelper.FirstOrDefault(customer => customer.ID == 1);
            MasterCustomersVM.DeactivateCustomerInDatabase(testCustomer);
            testCustomer = DatabaseCustomerHelper.FirstOrDefault(customer => customer.ID == 1);
            Assert.AreEqual(testCustomer.Active, false);

            MasterCustomersVM.ActivateCustomerInDatabase(testCustomer);
            testCustomer = DatabaseCustomerHelper.FirstOrDefault(customer => customer.ID == 1);
            Assert.AreEqual(testCustomer.Active, true);
        }

        private static bool CompareCustomers(Customer customer1, Customer customer2)
        {
            return customer1.Name.Equals(customer2.Name) && customer1.Group.ID.Equals(customer2.Group.ID)
                   && customer1.Address.Equals(customer2.Address) && customer1.City.Equals(customer2.City)
                   && customer1.CreditTerms == customer2.CreditTerms && customer1.MaxInvoices == customer2.MaxInvoices
                   && customer1.NPWP.Equals(customer2.NPWP) && customer1.Telephone.Equals(customer2.Telephone);
        }

        private static Customer CreateTestCustomer()
        {
            using (var context = new ERPContext())
            {
                return new Customer
                {
                    ID = -1000,
                    Name = "Test",
                    Group = context.CustomerGroups.First(),
                    Active = true,
                    Address = "lala land",
                    City = "lala",
                    CreditTerms = 7,
                    MaxInvoices = 10,
                    Telephone = "4242",
                    NPWP = "23"   
                };
            }
        }

        private static bool CheckIfCustomerAndItsLedgerExistsInDatabase(Customer customer)
        {
            using (var context = new ERPContext())
            {
                var customerReturnedFromDatabase = context.Customers.FirstOrDefault(e => e.ID.Equals(customer.ID));
                if (customerReturnedFromDatabase == null) return false;

                var customerLedgerAccountName = customer.Name + " Accounts Receivable";
                var customerLedgerAccountReturnedFromDatabase = context.Ledger_Accounts.FirstOrDefault(e => e.Name.Equals(customerLedgerAccountName));
                if (customerLedgerAccountReturnedFromDatabase == null) return false;

                var customerLedgerGeneralReturnedFromDatabase = context.Ledger_General.FirstOrDefault(e => e.ID.Equals(customerLedgerAccountReturnedFromDatabase.ID));
                if (customerLedgerGeneralReturnedFromDatabase == null) return false;

                var customerLedgerAccountBalanceReturnedFromDatabase = context.Ledger_Account_Balances.FirstOrDefault(e => e.ID.Equals(customerLedgerAccountReturnedFromDatabase.ID));
                return customerLedgerAccountBalanceReturnedFromDatabase != null;
            }
        }

        private static void RemoveCustomerLedgerFromDatabaseContext(ERPContext context, Customer customer)
        {
            var customerLedgerAccountName = customer.Name + " Accounts Receivable";
            var customerLedgerAccount = context.Ledger_Accounts.First(e => e.Name.Equals(customerLedgerAccountName));
            var customerLedgerGeneral = context.Ledger_General.First(e => e.ID.Equals(customerLedgerAccount.ID));
            var customerLedgerAccountBalance = context.Ledger_Account_Balances.First(e => e.ID.Equals(customerLedgerAccount.ID));
            context.Ledger_Account_Balances.Remove(customerLedgerAccountBalance);
            context.Ledger_General.Remove(customerLedgerGeneral);
            context.Ledger_Accounts.Remove(customerLedgerAccount);
        }

        private static void RemoveCustomerAndItsLedgerFromDatabase(Customer customer)
        {
            using (var context = new ERPContext())
            {
                var customerToBeRemoved = context.Customers.First(c => c.Name.Equals(customer.Name));
                context.Customers.Remove(customerToBeRemoved);
                RemoveCustomerLedgerFromDatabaseContext(context, customer);
                context.SaveChanges();
            }
        }
    }
}
