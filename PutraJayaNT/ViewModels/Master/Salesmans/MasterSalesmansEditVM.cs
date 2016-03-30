using System.Transactions;
using System.Windows;
using System.Windows.Input;
using MVVMFramework;
using PutraJayaNT.Models.Salesman;
using PutraJayaNT.Utilities;
using PutraJayaNT.ViewModels.Salesman;

namespace PutraJayaNT.ViewModels.Master.Salesmans
{
    using System.Linq;

    public class MasterSalesmansEditVM : ViewModelBase
    {
        private decimal _editPercentage;
        private ICommand _confirmEditCommand;

        public MasterSalesmansEditVM(SalesCommissionVM editingSalescommission)
        {
            EditingSalesCommission = editingSalescommission;
            SetDefaultEditProperties();
        }

        #region Edit Properties
        public SalesCommissionVM EditingSalesCommission { get; }

        public decimal EditPercentage
        {
            get { return _editPercentage; }
            set { SetProperty(ref _editPercentage, value, "EditPercentage"); }
        }
        #endregion

        public ICommand ConfirmEditCommand
        {
            get
            {
                return _confirmEditCommand ?? (_confirmEditCommand = new RelayCommand(() =>
                {
                    if (!IsEditConfirmationYes() && !AreEditFieldsValid()) return;
                    var editingSalesCommissionCopy = EditingSalesCommission.Model;
                    var editedSalesCommission = MakeEditedSalesCommission();
                    SaveSalesCommissionEditsToDatabase(editingSalesCommissionCopy, editedSalesCommission);
                    UpdateEditingSalesCommissionUIValues();
                    UtilityMethods.CloseForemostWindow();
                }));
            }
        }

        #region Helper Methods
        private static bool IsEditConfirmationYes()
        {
            return MessageBox.Show("Confirm edit?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes;
        }

        private bool AreEditFieldsValid()
        {
            return _editPercentage < 0 || _editPercentage > 100;
        }

        private void SetDefaultEditProperties()
        {
            _editPercentage = EditingSalesCommission.Percentage;
        }

        private SalesCommission MakeEditedSalesCommission()
        {
            return new SalesCommission
            {
                Salesman_ID = EditingSalesCommission.Salesman.ID,
                Category_ID = EditingSalesCommission.Category.ID,
                Percentage =  _editPercentage
            };
        }

        private static void DeepCopySalesCommissionProperties(SalesCommission fromSalesCommission, SalesCommission toSalesCommission)
        {
            toSalesCommission.Percentage = fromSalesCommission.Percentage;
        }

        private static void SaveSalesCommissionEditsToDatabaseContext(ERPContext context, SalesCommission editingSalesCommission, SalesCommission editedSalesCommission)
        {
            editingSalesCommission =
                context.SalesCommissions.SingleOrDefault(
                    commission =>
                        commission.Salesman_ID.Equals(editingSalesCommission.Salesman.ID) &&
                        commission.Category_ID.Equals(editingSalesCommission.Category.ID));
            if (editingSalesCommission != null) DeepCopySalesCommissionProperties(editedSalesCommission, editingSalesCommission);
            else context.SalesCommissions.Add(editedSalesCommission);
            context.SaveChanges();
        }

        public static void SaveSalesCommissionEditsToDatabase(SalesCommission editingSalesCommission, SalesCommission editedSalesCommission)
        {
            using (var ts = new TransactionScope())
            {
                var context = new ERPContext(UtilityMethods.GetDBName());
                SaveSalesCommissionEditsToDatabaseContext(context, editingSalesCommission, editedSalesCommission);
                ts.Complete();
            }
        }

        private void UpdateEditingSalesCommissionUIValues()
        {
            var editedSalesCommission = MakeEditedSalesCommission();
            var salesCommissionTo = EditingSalesCommission.Model;
            DeepCopySalesCommissionProperties(editedSalesCommission, salesCommissionTo);
            EditingSalesCommission.UpdatePropertiesToUI();
        }
        #endregion
    }
}
