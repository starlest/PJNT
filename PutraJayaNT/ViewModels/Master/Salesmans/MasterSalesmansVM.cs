namespace ECRP.ViewModels.Master.Salesmans 
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using Models.Salesman;
    using MVVMFramework;
    using Salesman;
    using Utilities;
    using Views.Master.Salesmans;

    public class MasterSalesmansVM : ViewModelBase
    {
        private SalesmanVM _selectedSalesman;
        private SalesCommissionVM _selectedLine;

        ICommand _editCommand;

        private ICommand _searchCommand;

        public MasterSalesmansVM()
        {
            NewEntryVM = new MasterSalesmansNewEntryVM(this);

            Salesmans = new ObservableCollection<SalesmanVM>();
            DisplayedSalesCommissions = new ObservableCollection<SalesCommissionVM>();

            UpdateSalesmans();
        }

        public MasterSalesmansNewEntryVM NewEntryVM { get; }

        #region Collections
        public ObservableCollection<SalesmanVM> Salesmans { get; }

        public ObservableCollection<SalesCommissionVM> DisplayedSalesCommissions { get; }
        #endregion

        #region Properties
        public SalesmanVM SelectedSalesman
        {
            get { return _selectedSalesman; }
            set { SetProperty(ref _selectedSalesman, value, "SelectedSalesman"); }
        }

        public SalesCommissionVM SelectedLine
        {
            get { return _selectedLine; }
            set { SetProperty(ref _selectedLine, value, "SelectedLine"); }
        }
        #endregion

        #region Commands
        public ICommand SearchCommand
        {
            get
            {
                return _searchCommand ?? (_searchCommand = new RelayCommand(() =>
                {
                    UpdateDisplayedSalesCommissions();
                    UpdateSalesmans();
                }));
            }
        }

        public ICommand EditCommand
        {
            get
            {
                return _editCommand ?? (_editCommand = new RelayCommand(() =>
                {
                    if (!IsEditSalesmanSelected()) return;
                    ShowEditWindow();
                    _selectedLine.UpdatePropertiesToUI();
                }));
            }
        }
        #endregion

        #region Helper Method
        private void ShowEditWindow()
        {
            var vm = new MasterSalesmansEditVM(_selectedLine);
            var editWindow = new MasterSalesmansEditView(vm)
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            editWindow.ShowDialog();
        }

        private bool IsEditSalesmanSelected()
        {
            if (_selectedLine != null) return true;
            MessageBox.Show("Please select an item to edit.", "No Selection", MessageBoxButton.OK);
            return false;
        }

        public void UpdateSalesmans()
        {
            var oldSelectedSalesman = _selectedSalesman;

            Salesmans.Clear();
            using (var context = UtilityMethods.createContext())
            {
                var salesmansFromDatabase =
                    context.Salesmans.Where(salesman => !salesman.Name.Equals(" ")).OrderBy(salesman => salesman.Name);
                foreach (var salesman in salesmansFromDatabase)
                    Salesmans.Add(new SalesmanVM {Model = salesman});
            }

            UpdateSelectedSalesman(oldSelectedSalesman);
        }

        private void UpdateSelectedSalesman(SalesmanVM oldSelectedSalesman)
        {
            if (oldSelectedSalesman == null) return;
            SelectedSalesman = Salesmans.FirstOrDefault(salesman => salesman.ID.Equals(oldSelectedSalesman.ID));
        }

        private void UpdateDisplayedSalesCommissions()
        {
            DisplayedSalesCommissions.Clear();

            using (var context = UtilityMethods.createContext())
            {
                var categories = context.ItemCategories.OrderBy(category => category.Name);

                foreach (var category in categories.ToList())
                {
                    var commission = context.SalesCommissions
                        .Include("Category")
                        .Include("Salesman")
                        .FirstOrDefault(
                            salesman =>
                            salesman.Salesman_ID.Equals(_selectedSalesman.ID) &&
                            salesman.Category_ID.Equals(category.ID));

                    if (commission != null)
                        DisplayedSalesCommissions.Add(new SalesCommissionVM { Model = commission });

                    else
                    {
                        var newCommission = new SalesCommission
                        {
                            Salesman = _selectedSalesman.Model,
                            Category = category,
                            Percentage = 0m
                        };
                        DisplayedSalesCommissions.Add(new SalesCommissionVM {Model = newCommission});
                    }
                }
            }

        }
        #endregion
    }
}
