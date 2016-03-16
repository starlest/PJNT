using System.Transactions;
using PutraJayaNT.Models;
using PutraJayaNT.Models.Accounting;
using System.Linq;

namespace PutraJayaNT.Utilities.ModelHelpers
{
    using System.Data.Entity;
    using System.Windows;

    public static class SupplierHelper
    {
        public static void AddSupplierAlongWithItsLedgerToDatabase(Supplier supplier)
        {
            var context = new ERPContext();
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
                var context = new ERPContext();
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
            var ledgerAccount = CreateSupplierLedgerAccount(suppplier);
            context.Ledger_Accounts.Add(ledgerAccount);

            var ledgerGeneral = CreateSupplierLedgerGeneral(ledgerAccount);
            context.Ledger_General.Add(ledgerGeneral);

            var ledgerAccountBalance = CreateSupplierLedgerAccountBalance(ledgerAccount);
            context.Ledger_Account_Balances.Add(ledgerAccountBalance);
        }

        private static LedgerAccountBalance CreateSupplierLedgerAccountBalance(LedgerAccount ledgerAccount)
        {
            return new LedgerAccountBalance
            {
                LedgerAccount = ledgerAccount,
                PeriodYear = UtilityMethods.GetCurrentDate().Year,
            };
        }

        private static LedgerGeneral CreateSupplierLedgerGeneral(LedgerAccount ledgerAccount)
        {
            return new LedgerGeneral
            {
                LedgerAccount = ledgerAccount,
                PeriodYear = UtilityMethods.GetCurrentDate().Year,
                Period = UtilityMethods.GetCurrentDate().Month,
            };
        }

        private static LedgerAccount CreateSupplierLedgerAccount(Supplier supplier)
        {
            return new LedgerAccount
            {
                Name = supplier.Name + " Accounts Payable",
                Notes = "Accounts Payable",
                Class = "Liability"
            };
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
