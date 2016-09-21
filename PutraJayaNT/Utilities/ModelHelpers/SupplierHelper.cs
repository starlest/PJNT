namespace ECRP.Utilities.ModelHelpers
{
    using System.Data.Entity;
    using System.Linq;
    using System.Transactions;
    using System.Windows;
    using Models.Accounting;
    using Models.Supplier;
    using Services;

    public static class SupplierHelper
    {
        public static void AddSupplierAlongWithItsLedgerToDatabase(Supplier supplier)
        {
            var context = UtilityMethods.createContext();
            var success = true;
            try
            {
                context.Suppliers.Add(supplier);
                CreateAndAddSupplierLedgerToDatabaseContext(context, supplier);
                context.SaveChanges();
            }
            catch
            {
                MessageBox.Show("The supplier's name is already being used.", "Invalid ID", MessageBoxButton.OK);
                success = false;
            }
            finally
            {
                if (success)
                    MessageBox.Show("Successfully added supplier!", "Success", MessageBoxButton.OK);
                context.Dispose();
            }
        }

        public static void SaveSupplierEditsToDatabase(Supplier editingSupplier, Supplier editedSupplier)
        {
            using (var ts = new TransactionScope())
            {
                var context = UtilityMethods.createContext();
                if (!IsSupplierNameChange(editingSupplier, editedSupplier))
                    ChangeSupplierLedgerAccountNameInDatabaseContext(context, editingSupplier, editedSupplier);
                SaveSupplierEditsToDatabaseContext(context, editingSupplier, editedSupplier);
                ts.Complete();
            }
        }

        public static void DeepCopySupplierProperties(Supplier fromSupplier, Supplier toSupplier)
        {
            toSupplier.Active = fromSupplier.Active;
            toSupplier.Name = fromSupplier.Name;
            toSupplier.Address = fromSupplier.Address;
            toSupplier.GSTID = fromSupplier.GSTID;
        }

        #region Add Supplier Helper Methods
        private static void CreateAndAddSupplierLedgerToDatabaseContext(ERPContext context, Supplier suppplier)
        {
            var accountName = suppplier.Name + " Accounts Payable";
            AccountingService.CreateNewAccount(context, accountName, Constants.ACCOUNTS_PAYABLE);
        }
        #endregion

        #region Edit Supplier Helper Methods
        private static bool IsSupplierNameChange(Supplier editingSupplier, Supplier editedSupplier)
        {
            return editingSupplier.Name == editedSupplier.Name;
        }

        private static void ChangeSupplierLedgerAccountNameInDatabaseContext(ERPContext context, Supplier editingSupplier, Supplier editedSupplier)
        {
            var ledgerAccountFromDatabase = GetSupplierLedgerAccountFromDatabaseContext(context, editingSupplier);
            ledgerAccountFromDatabase.Name = $"{editedSupplier.Name} Accounts Payable";
            context.SaveChanges();
        }

        private static LedgerAccount GetSupplierLedgerAccountFromDatabaseContext(ERPContext context, Supplier supplier)
        {
            var searchName = supplier.Name + " Accounts Payable";
            return context.Ledger_Accounts.First(e => e.Name.Equals(searchName));
        }

        private static void SaveSupplierEditsToDatabaseContext(ERPContext context, Supplier editingSupplier,
            Supplier editedSupplier)
        {
            context.Entry(editingSupplier).State = EntityState.Modified;
            DeepCopySupplierProperties(editedSupplier, editingSupplier);
            context.SaveChanges();
        }
        #endregion
    }
}
