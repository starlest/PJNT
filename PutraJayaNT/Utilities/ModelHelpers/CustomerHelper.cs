namespace ECERP.Utilities.ModelHelpers
{
    using System.Linq;
    using System.Transactions;
    using System.Windows;
    using Models.Accounting;
    using Models.Customer;
    using Services;

    public static class CustomerHelper
    {
        public static void AddCustomerAlongWithItsLedgerToDatabase(Customer customer)
        {
            var context = UtilityMethods.createContext();
            var success = true;
            try
            {
                AddCustomerToDatabaseContext(context, customer);
                CreateAndAddCustomerLedgerToDatabaseContext(context, customer);
                context.SaveChanges();
            }
            catch
            {
                MessageBox.Show("The customer's name is already being used.", "Invalid ID", MessageBoxButton.OK);
                success = false;
            }
            finally
            {
                if (success)
                    MessageBox.Show("Successfully added customer!", "Success", MessageBoxButton.OK);
                context.Dispose();
            }
        }

        public static void SaveCustomerEditsToDatabase(Customer editingCustomer, Customer editedCustomer)
        {
            using (var ts = new TransactionScope())
            {
                var context = UtilityMethods.createContext();
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
            customer.Group = context.CustomerGroups.First(group => group.ID.Equals(customer.Group.ID));
            context.Customers.Add(customer);
        }

        private static void CreateAndAddCustomerLedgerToDatabaseContext(ERPContext context, Customer customer)
        {
            var accountName = customer.Name + " Accounts Receivable";
            AccountingService.CreateNewAccount(context, accountName, Constants.ACCOUNTS_RECEIVABLE);
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
            editingCustomer = context.Customers.Include("Group").Single(customer => customer.ID.Equals(editingCustomer.ID));
            DeepCopyCustomerProperties(editedCustomer, ref editingCustomer);

            editingCustomer.Group = context.CustomerGroups.First(group => group.ID.Equals(editingCustomer.Group.ID));

            context.SaveChanges();
        }
        #endregion
    }
}
