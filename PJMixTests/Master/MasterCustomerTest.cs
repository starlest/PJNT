using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PutraJayaNT.Models;
using PutraJayaNT.Utilities;
using PutraJayaNT.ViewModels.Master.Customer;

namespace PJMixTests.Master
{
    [TestClass]
    public class MasterCustomerTest
    {
        [TestMethod]
        public void TestAddCustomer()
        {
            var customer = CreateTestCustomer();
            MasterCustomersNewEntryVM.AddCustomerAlongWithItsLedgerToDatabase(customer);
            var result = CheckIfCustomerAndItsLedgerExistsInDatabase(customer);
            Assert.AreEqual(true, result);
            RemoveCustomerAndItsLedgerFromDatabase(customer);
        }

        public void TestRemoveCustomer()
        {
            var customer = CreateTestCustomer();
            RemoveCustomerAndItsLedgerFromDatabase(customer);
            var result = CheckIfCustomerAndItsLedgerExistsInDatabase(customer);
            Assert.AreEqual(false, result);
        }

        private static Customer CreateTestCustomer()
        {
            using (var context = new ERPContext())
            {
                var customer = new Customer
                {
                    Name = "Test",
                    Group = context.CustomerGroups.First()
                };
                return customer;
            }
        }

        private static bool CheckIfCustomerAndItsLedgerExistsInDatabase(Customer customer)
        {
            using (var context = new ERPContext())
            {
                var customerReturnedFromDatabase = context.Customers.FirstOrDefault(e => e.Name.Equals(customer.Name));
                if (customerReturnedFromDatabase == null) return false;

                var customerLedgerAccountName = customer.Name + " Accounts Receivable";
                var customerLedgerAccountReturnedFromDatabase = context.Ledger_Accounts.FirstOrDefault(e => e.Name.Equals(customerLedgerAccountName));
                if (customerLedgerAccountReturnedFromDatabase == null) return false;

                var customerLedgerGeneralReturnedFromDatabase = context.Ledger_General.FirstOrDefault(e => e.ID.Equals(customerLedgerAccountReturnedFromDatabase.ID));
                if (customerLedgerGeneralReturnedFromDatabase == null) return false;

                var customerLedgerAccountBalanceReturnedFromDatabase = context.Ledger_Account_Balances.FirstOrDefault(e => e.ID.Equals(customerLedgerAccountReturnedFromDatabase.ID));
                if (customerLedgerAccountBalanceReturnedFromDatabase == null) return false;

                return true;
            }
        }

        private static void RemoveCustomerFromDatabaseContext(ERPContext context, Customer customer)
        {
            var customerReturnedFromDatabase = context.Customers.First(e => e.ID.Equals(customer.ID));
            context.Customers.Remove(customerReturnedFromDatabase);
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
                RemoveCustomerFromDatabaseContext(context, customer);
                RemoveCustomerLedgerFromDatabaseContext(context, customer);
                context.SaveChanges();
            }
        }
    }
}
