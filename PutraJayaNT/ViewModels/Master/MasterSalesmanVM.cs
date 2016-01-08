namespace PutraJayaNT.ViewModels.Master 
{
    using MVVMFramework;
    using System.Collections.ObjectModel;
    using System.Windows;
    using PutraJayaNT.Models.Salesman;
    using Utilities;
    using System.Linq;
    using Salesman;
    using System.Windows.Input;
    class MasterSalesmanVM : ViewModelBase
    {
        ObservableCollection<Salesman> _salesmen;
        ObservableCollection<SalesCommissionVM> _salesCommissions;

        Salesman _selectedSalesman;
        SalesCommissionVM _selectedLine;

        bool _isEditWindowNotOpen;
        Visibility _editWindowVisisbility;
        ICommand _editCommand;
        ICommand _cancelEditCommand;
        ICommand _confirmEditCommand;

        public MasterSalesmanVM()
        {
            _salesmen = new ObservableCollection<Salesman>();
            _salesCommissions = new ObservableCollection<SalesCommissionVM>();

            _isEditWindowNotOpen = true;
            _editWindowVisisbility = Visibility.Hidden;

            UpdateSalesmen();
        }

        public ObservableCollection<Salesman> Salesmen
        {
            get { return _salesmen; }
        }

        public ObservableCollection<SalesCommissionVM> SalesCommissions
        {
            get { return _salesCommissions; }
        }

        public Salesman SelectedSalesman
        {
            get { return _selectedSalesman; }
            set
            {
                SetProperty(ref _selectedSalesman, value, "SelectedSalesman");

                if (_selectedSalesman == null) return;

                UpdateSalesCommissions();
            }
        }

        public SalesCommissionVM SelectedLine
        {
            get { return _selectedLine; }
            set { SetProperty(ref _selectedLine, value, "SelectedLine"); }
        }

        #region Edit Properties
        public bool IsEditWindowNotOpen
        {
            get { return _isEditWindowNotOpen; }
            set { SetProperty(ref _isEditWindowNotOpen, value, "IsEditWindowNotOpen"); }
        }

        public Visibility EditWindowVisibility
        {
            get { return _editWindowVisisbility; }
            set { SetProperty(ref _editWindowVisisbility, value, "EditWindowVisibility"); }
        }

        public ICommand EditCommand
        {
            get
            {
                return _editCommand ?? (_editCommand = new RelayCommand(() =>
                {
                    if (_selectedLine == null)
                    {
                        MessageBox.Show("Please select a line to edit.", "No Selection", MessageBoxButton.OK);
                        return;
                    }
                    else
                    {
                        EditWindowVisibility = Visibility.Visible;
                        IsEditWindowNotOpen = false;
                    }
                }));
            }
        }

        public ICommand CancelEditCommand
        {
            get
            {
                return _cancelEditCommand ?? (_cancelEditCommand = new RelayCommand(() =>
                {
                    EditWindowVisibility = Visibility.Collapsed;
                    IsEditWindowNotOpen = true;
                }));
            }
        }

        public ICommand ConfirmEditCommand
        {
            get
            {
                return _confirmEditCommand ?? (_confirmEditCommand = new RelayCommand(() =>
                {
                    if (_selectedLine.Percentage < 0 || _selectedLine.Percentage > 100)
                    {
                        MessageBox.Show("Please enter a percetange between 0-100.", "Invalid Value", MessageBoxButton.OK);
                        return;
                    }

                    if (MessageBox.Show("Confirm edit?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        // Update the commission in the database
                        using (var context = new ERPContext())
                        {
                            var commission = context.SalesCommissions
                                      .Include("Salesman")
                                      .Include("Category")
                                      .Where(e => e.Salesman_ID.Equals(_selectedSalesman.ID) && e.Category_ID.Equals(_selectedLine.Category.ID))
                                      .FirstOrDefault();

                            if (commission != null) commission.Percentage = _selectedLine.Percentage;
                            else
                            {
                                var newCommission = new SalesCommission
                                {
                                    Salesman = context.Salesmans.Where(e => e.ID.Equals(_selectedSalesman.ID)).FirstOrDefault(),
                                    Category = context.Categories.Where(e => e.ID.Equals(_selectedLine.Category.ID)).FirstOrDefault(),
                                    Percentage = _selectedLine.Percentage
                                };
                                context.SalesCommissions.Add(newCommission);
                            }

                            context.SaveChanges();
                        }

                        EditWindowVisibility = Visibility.Collapsed;
                        IsEditWindowNotOpen = true;

                        MessageBox.Show("Successfully saved.", "Success", MessageBoxButton.OK);
                    }
                }));
            }
        }
        #endregion

        #region Helper Method
        private void UpdateSalesmen()
        {
            _salesmen.Clear();

            using (var context = new ERPContext())
            {
                var salesmen = context.Salesmans.ToList();

                foreach (var salesman in salesmen)
                    _salesmen.Add(salesman);
            }
        }

        private void UpdateSalesCommissions()
        {
            _salesCommissions.Clear();

            using (var context = new ERPContext())
            {
                var categories = context.Categories.OrderBy(e => e.Name).ToList();

                foreach (var category in categories)
                {
                    var commission = context.SalesCommissions
                        .Include("Salesman")
                        .Include("Category")
                        .Where(e => e.Salesman_ID.Equals(_selectedSalesman.ID) && e.Category_ID.Equals(category.ID))
                        .FirstOrDefault();

                    if (commission != null)
                        _salesCommissions.Add(new SalesCommissionVM { Model = commission });

                    else
                    {
                        var newCommission = new SalesCommission { Salesman = _selectedSalesman, Category = category, Percentage = 0 };
                        _salesCommissions.Add(new SalesCommissionVM { Model = newCommission });
                    }
                }
            }
        }
        #endregion
    }
}
