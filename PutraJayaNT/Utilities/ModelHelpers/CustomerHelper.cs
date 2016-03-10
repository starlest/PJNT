using System.Linq;
using System.Transactions;
using PutraJayaNT.Models.Accounting;
using PutraJayaNT.Models.Customer;
using PutraJayaNT.Utilities.Database.Customer;

namespace PutraJayaNT.Utilities.ModelHelpers
{
    public static class CustomerHelper
    {
        public static void AddCustomerAlongWithItsLedgerToDatabase(Customer customer)
        {
            using (var context = new ERPContext())
            {
                AddCustomerToDatabaseContext(context, customer);
                CreateAndAddCustomerLedgerToDatabaseContext(context, customer);
                context.SaveChanges();
            }
        }

        public static void SaveCustomerEditsToDatabase(Customer editingCustomer, Customer editedCustomer)
        {
            using (var ts = new TransactionScope())
            {
                var context = new ERPContext();
                if (!IsCustomerNameChanged(editingCustomer, editedCustomer))
                    ChangeCustomerLedgerAccountNameInDatabaseContext(context, editingCustomer, editedCustomer);
                SaveCustomerEditsToDatabaseContext(context, editingCustomer, editedCustomer);
                ts.Complete();
            }
        }

        public static void DeepCopyCustomerProperties(Customer fromCustomer, ref Customer toCustomer)
        {
            toCustomer.Name = fromCustomer.Name;
            toCustomer.City = fromCustomer.City;
            toCustomer.Address = fromCustomer.Address;
            toCustomer.Telephone = fromCustomer.Telephone;
            toCustomer.NPWP = fromCustomer.NPWP;
            toCustomer.CreditTerms = fromCustomer.CreditTerms;
            toCustomer.MaxInvoices = fromCustomer.MaxInvoices;
            toCustomer.Group = fromCustomer.Group;
        }

        #region Add Customer Helper Methods
        private static void AddCustomerToDatabaseContext(ERPContext context, Customer customer)
        {
            var customerGroupToBeAttached = customer.Group;
            DatabaseCustomerGroupHelper.AttachToObjectFromDatabaseContext(context, ref customerGroupToBeAttached);
            customer.Group = customerGroupToBeAttached;
            context.Customers.Add(customer);
        }

        private static void CreateAndAddCustomerLedgerToDatabaseContext(ERPContext context, Customer customer)
        {
            var ledgerAccount = CreateCustomerLedgerAccount(customer);
            context.Ledger_Accounts.Add(ledgerAccount);

            var ledgerGeneral = CreateCustomerLedgerGeneral(ledgerAccount);
            context.Ledger_General.Add(ledgerGeneral);

            var ledgerAccountBalance = CreateCustomerLedgerAccountBalance(ledgerAccount);
            context.Ledger_Account_Balances.Add(ledgerAccountBalance);
        }

        private static LedgerAccountBalance CreateCustomerLedgerAccountBalance(LedgerAccount ledgerAccount)
        {
            return new LedgerAccountBalance
            {
                LedgerAccount = ledgerAccount,
                PeriodYear = UtilityMethods.GetCurrentDate().Year,
            };
        }

        private static LedgerAccount CreateCustomerLedgerAccount(Customer customer)
        {
            return new LedgerAccount
            {
                Name = customer.Name + " Accounts Receivable",
                Notes = "Accounts Receivable",
                Class = "Asset"
            };
        }

        private static LedgerGeneral CreateCustomerLedgerGeneral(LedgerAccount ledgerAccount)
        {
            return new LedgerGeneral
            {
                LedgerAccount = ledgerAccount,
                PeriodYear = UtilityMethods.GetCurrentDate().Year,
                Period = UtilityMethods.GetCurrentDate().Month,
            };
        }
        #endregion

        #region Edit Customer Helper Methods
        private static bool IsCustomerNameChanged(Customer editingCustomer, Customer editedCustomer)
        {
            return editingCustomer.Name == editedCustomer.Name;
        }

        private static void ChangeCustomerLedgerAccountNameInDatabaseContext(ERPContext context, Customer editingCustomer, Customer editedCustomer)
        {
            var ledgerAccountFromDatabase = GetCustomerLedgerAccountFromDatabaseContext(context, editingCustomer);
            ledgerAccountFromDatabase.Name = $"{editedCustomer.Name} Accounts Receivable";
            context.SaveChanges();
        }

        private static LedgerAccount GetCustomerLedgerAccountFromDatabaseContext(ERPContext context, Customer customer)
        {
            var searchName = customer.Name + " Accounts Receivable";
            return context.Ledger_Accounts.First(e => e.Name.Equals(searchName));
        }

        private static void SaveCustomerEditsToDatabaseContext(ERPContext context, Customer editingCustomer, Customer editedCustomer)
        {
            DatabaseCustomerHelper.AttachToObjectFromDatabaseContext(context, ref editingCustomer);
            DeepCopyCustomerProperties(editedCustomer, ref editingCustomer);

            var customerGroupToBeAttachedToDatabaseContext = editingCustomer.Group;
            DatabaseCustomerGroupHelper.AttachToObjectFromDatabaseContext(context, ref customerGroupToBeAttachedToDatabaseContext);
            editingCustomer.Group = customerGroupToBeAttachedToDatabaseContext;

            context.SaveChanges();
        }
        #endregion
    }
}
