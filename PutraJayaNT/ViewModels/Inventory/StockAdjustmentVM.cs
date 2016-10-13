namespace ECRP.ViewModels.Inventory
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using Models.StockCorrection;
    using MVVMFramework;
    using Utilities;
    using Utilities.ModelHelpers;

    internal class StockAdjustmentVM : ViewModelBase<StockAdjustmentTransaction>
    {
        #region Backing Fields
        private bool _isNotEditMode;
        private string _transactionID;
        private DateTime _transactionDate;
        private string _transactionDescription;
        private StockAdjustmentTransactionLineVM _selectedLine;
        private ICommand _deleteLineCommand;
        private ICommand _newTransactionCommand;
        private ICommand _saveTransactionCommand;
        #endregion

        public StockAdjustmentVM()
        {
            _isNotEditMode = true;
            NewEntryVM = new StockAdjustmentNewEntryVM(this);
            Model = new StockAdjustmentTransaction();
            DisplayedLines = new ObservableCollection<StockAdjustmentTransactionLineVM>();
            TransactionDate = UtilityMethods.GetCurrentDate().Date;
            SetTransactionID();
        }

        public StockAdjustmentNewEntryVM NewEntryVM { get; }

        #region Collections
        public ObservableCollection<StockAdjustmentTransactionLineVM> DisplayedLines { get; }
        #endregion

        #region Properties 
        public bool IsNotEditMode
        {
            get { return _isNotEditMode; }
            set { SetProperty(ref _isNotEditMode, value, () => IsNotEditMode); }
        }

        public string TransactionID
        {
            get { return _transactionID; }
            set
            {
                if (!CheckIDExists(value))
                {
                    MessageBox.Show("Transaction does not exists.", "Invalid ID", MessageBoxButton.OK);
                    return;
                }
                SetProperty(ref _transactionID, value, () => TransactionID);
                SetEditMode();
            }
        }

        public DateTime TransactionDate
        {
            get { return _transactionDate; }
            set { SetProperty(ref _transactionDate, value, () => TransactionDate); }
        }

        public string TransactionDescription
        {
            get { return _transactionDescription; }
            set { SetProperty(ref _transactionDescription, value, () => TransactionDescription); }
        }

        public StockAdjustmentTransactionLineVM SelectedLine
        {
            get { return _selectedLine; }
            set { SetProperty(ref _selectedLine, value, () => SelectedLine); }
        }
        #endregion

        #region Commands
        public ICommand DeleteLineCommand
        {
            get
            {
                return _deleteLineCommand ?? (_deleteLineCommand = new RelayCommand(() =>
                {
                    if (!IsThereLineSelected() && !IsLineDeletionConfirmationYes()) return;
                    DisplayedLines.Remove(_selectedLine);
                }));
            }
        }

        public ICommand NewTransactionCommand => _newTransactionCommand ?? (_newTransactionCommand = new RelayCommand(ResetTransaction));

        public ICommand SaveTransactionCommand
        {
            get
            {
                return _saveTransactionCommand ?? (_saveTransactionCommand = new RelayCommand(() =>
                {
                    if (DisplayedLines.Count == 0 || !IsDescriptionFilled() 
                    || !IsSavingConfirmationYes() || !UtilityMethods.GetMasterAdminVerification()) return;
                    AssignTransactionPropertiesToModel();
                    StockAdjustmentHelper.AddStockAdjustmentTransactionToDatabase(Model);
                    MessageBox.Show("Transaction sucessfully is saved.", "Success", MessageBoxButton.OK);
                    ResetTransaction();
                }));
            }
        }
        #endregion

        #region Helper Methods
        private static bool CheckIDExists(string id)
        {
            using (var context = UtilityMethods.createContext())
            {
                var transaction = context.StockAdjustmentTransactions.SingleOrDefault(e => e.StockAdjustmentTransactionID.Equals(id));
                return transaction != null;
            }
        }

        private void SetEditMode()
        {
            IsNotEditMode = false;
            NewEntryVM.ResetEntryFields();

            using (var context = UtilityMethods.createContext())
            {
                var transactionFromDatabase = context.StockAdjustmentTransactions.Single(
                    transaction => transaction.StockAdjustmentTransactionID.Equals(_transactionID));
                TransactionDescription = transactionFromDatabase.Description;
                TransactionDate = transactionFromDatabase.Date;

                var lines = context.StockAdjustmentTransactionLines
                    .Include("Item")
                    .Include("Warehouse")
                    .Where(line => line.StockAdjustmentTransactionID.Equals(_transactionID))
                    .OrderBy(line => line.Warehouse.Name)
                    .ThenBy(line => line.Item.Name);

                DisplayedLines.Clear();
                foreach (var line in lines)
                    DisplayedLines.Add(new StockAdjustmentTransactionLineVM { Model = line });
            }
        }

        private void SetTransactionID()
        {
            var month = _transactionDate.Month;
            var year = _transactionDate.Year;
            var leadingIDString = "SA" + (long)((year - 2000) * 100 + month) + "-";
            var endingIDString = 0.ToString().PadLeft(4, '0');
            _transactionID = leadingIDString + endingIDString;

            string lastTransactionID = null;
            using (var context = UtilityMethods.createContext())
            {
                var IDs = from StockAdjustmentTransaction in context.StockAdjustmentTransactions
                          where StockAdjustmentTransaction.StockAdjustmentTransactionID.Substring(0, 7).Equals(leadingIDString)
                          && string.Compare(StockAdjustmentTransaction.StockAdjustmentTransactionID, _transactionID, StringComparison.Ordinal) >= 0
                          orderby StockAdjustmentTransaction.StockAdjustmentTransactionID descending
                          select StockAdjustmentTransaction.StockAdjustmentTransactionID;
                if (IDs.Count() != 0) lastTransactionID = IDs.First();
            }

            if (lastTransactionID != null)
            {
                var newIDIndex = Convert.ToInt64(lastTransactionID.Substring(7, 4)) + 1;
                endingIDString = newIDIndex.ToString().PadLeft(4, '0');
                _transactionID = leadingIDString + endingIDString;
            }

            Model.StockAdjustmentTransactionID = _transactionID;
            OnPropertyChanged("TransactionID");
        }

        private bool IsDescriptionFilled()
        {
            if (_transactionDescription != null && !_transactionDescription.Equals("") && !_transactionDescription.Equals(" ")) return true;
            MessageBox.Show("Please enter a description.", "Missing Field", MessageBoxButton.OK);
            return false;
        }

        private void ResetTransaction()
        {
            IsNotEditMode = true;
            TransactionDescription = null;
            TransactionDate = UtilityMethods.GetCurrentDate();
            NewEntryVM.NewEntryWarehouse = null;
            NewEntryVM.ResetEntryFields();
            DisplayedLines.Clear();
            Model = new StockAdjustmentTransaction
            {
                AdjustStockTransactionLines = new ObservableCollection<StockAdjustmentTransactionLine>()
            };
            SetTransactionID();
            NewEntryVM.UpdateWarehouses();
        }
        #endregion

        #region Transaction Helper Methods
        private static bool IsSavingConfirmationYes()
        {
            return MessageBox.Show("Confirm transaction?", "Confirmation", MessageBoxButton.YesNo) ==
                   MessageBoxResult.Yes;
        }

        private void AssignTransactionPropertiesToModel()
        {
            Model.Description = _transactionDescription;
            Model.Date = UtilityMethods.GetCurrentDate().Date;
            foreach (var line in DisplayedLines)
                Model.AdjustStockTransactionLines.Add(line.Model);
        }
        #endregion

        #region Line Helper Methods
        private bool IsThereLineSelected()
        {
            if (_selectedLine != null) return true;
            MessageBox.Show("Please select a line.", "Invalid Command", MessageBoxButton.OK);
            return false;
        }

        private static bool IsLineDeletionConfirmationYes()
        {
            return
                MessageBox.Show("Confirm deletion?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) ==
                MessageBoxResult.Yes;
        }
        #endregion
    }
}
