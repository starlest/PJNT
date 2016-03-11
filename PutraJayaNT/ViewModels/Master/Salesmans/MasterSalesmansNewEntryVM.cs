namespace PutraJayaNT.ViewModels.Master.Salesmans
{
    using System.Windows;
    using System.Windows.Input;
    using MVVMFramework;
    using Models.Salesman;
    using Utilities.ModelHelpers;

    public class MasterSalesmansNewEntryVM : ViewModelBase
    {
        private readonly MasterSalesmansVM _parentVM;

        private string _newEntryName;
        private ICommand _newEntryCommand;

        public MasterSalesmansNewEntryVM(MasterSalesmansVM parentVM)
        {
            _parentVM = parentVM;
        }

        public string NewEntryName
        {
            get { return _newEntryName; }
            set { SetProperty(ref _newEntryName, value, "NewEntryName"); }
        }

        public ICommand NewEntryCommand
        {
            get
            {
                return _newEntryCommand ?? (_newEntryCommand = new RelayCommand(() =>
                {
                    if (!IsNewEntryCommandChecksSuccessful()) return;
                    var newSalesman = CreateNewEntrySalesman();
                    SalesmanHelper.AddSalesmanToDatabase(newSalesman);
                    ResetEntryFields();
                    _parentVM.UpdateSalesmans();
                    MessageBox.Show("Successfully added salesman!", "Success", MessageBoxButton.OK);
                }));
            }
        }

        #region Helper Methods
        private bool IsNewEntryCommandChecksSuccessful()
        {
            return AreAllEntryFieldsFilled() && IsNewEntryCommandConfirmationYes();
        }

        private bool AreAllEntryFieldsFilled()
        {
            if (_newEntryName != null) return true;
            MessageBox.Show("Please enter salesman's name", "Missing Fields", MessageBoxButton.OK);
            return false;
        }

        private static bool IsNewEntryCommandConfirmationYes()
        {
            return MessageBox.Show("Confirm adding this salesman?", "Confirmation", MessageBoxButton.YesNo) != MessageBoxResult.No;
        }

        private Salesman CreateNewEntrySalesman()
        {
            return new Salesman
            {
                Name = _newEntryName
            };
        }

        private void ResetEntryFields()
        {
            NewEntryName = null;
        }
        #endregion
    }
}
