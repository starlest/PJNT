namespace PutraJayaNT.ViewModels.Analysis
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using Customer;
    using Item;
    using Models.Customer;
    using Models.Inventory;
    using Models.Sales;
    using MVVMFramework;
    using Utilities;

    internal class SalesAnalysisMonthlyVM : ViewModelBase
    {
        #region Backing Fields
        private ushort _selectedYear;
        private CategoryVM _selectedCategory;
        private ItemVM _selectedItem;
        private CustomerVM _selectedCustomer;
        private ICommand _clearCommand;
        private ICommand _displayCommand;
        #endregion

        public SalesAnalysisMonthlyVM()
        {
            DisplayedLines = new ObservableCollection<SalesAnalysisMonthlyLineVM>();
            Years = new ObservableCollection<ushort>();
            Categories = new ObservableCollection<CategoryVM>();
            ListedItems = new ObservableCollection<ItemVM>();
            Customers = new ObservableCollection<CustomerVM>();
            UpdateYears();
            UpdateCategories();
            UpdateCustomers();
            _selectedCustomer = Customers.First();
            _selectedYear = Years.First();
        }

        #region Collections
        public ObservableCollection<SalesAnalysisMonthlyLineVM> DisplayedLines { get; } 

        public ObservableCollection<ushort> Years { get; }

        public ObservableCollection<CategoryVM> Categories { get; } 

        public ObservableCollection<ItemVM> ListedItems { get; }

        public ObservableCollection<CustomerVM> Customers { get; }
        #endregion

        #region Properties
        public ushort SelectedYear
        {
            get { return _selectedYear; }
            set { SetProperty(ref _selectedYear, value, () => SelectedYear); }
        }

        public CategoryVM SelectedCategory
        {
            get { return _selectedCategory; }
            set
            {
                SetProperty(ref _selectedCategory, value, () => SelectedCategory);
                if (_selectedCategory == null) return;
                SelectedItem = null;
                UpdateListedItems();
            }
        }

        public ItemVM SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value, () => SelectedItem); }
        }

        public CustomerVM SelectedCustomer
        {
            get { return _selectedCustomer; }
            set { SetProperty(ref _selectedCustomer, value, () => SelectedCustomer); }
        }
        #endregion

        #region Commands
        public ICommand ClearCommand => _clearCommand ?? (_clearCommand = new RelayCommand(ClearScreen));

        public ICommand DisplayCommand
        {
            get
            {
                return _displayCommand ?? (_displayCommand = new RelayCommand(() =>
                {
                    if (!IsThereSelectedItem() || !IsThereSelectedCustomer()) return;
                    UpdateLines();
                }));
            }
        }
        #endregion

        #region Helper Methods
        private void UpdateYears()
        {
            Years.Clear();
            var currentYear = (ushort) UtilityMethods.GetCurrentDate().Year;
            for (var year = currentYear; year >= 2016; year--)
                Years.Add(year);
        }

        private void UpdateCategories()
        {
            Categories.Clear();
            var allCategory = new Category {ID = -1, Name = "All"};
            Categories.Add(new CategoryVM {Model = allCategory});
            using (var context = UtilityMethods.createContext())
            {
                var categoriesFromDatabase = context.ItemCategories.OrderBy(category => category.Name);
                foreach (var category in categoriesFromDatabase)
                    Categories.Add(new CategoryVM {Model = category});
            }
        }

        private void UpdateListedItems()
        {
            ListedItems.Clear();
            using (var context = UtilityMethods.createContext())
            {
                IEnumerable<Item> itemsFromDatabase;

                if (_selectedCategory.Name.Equals("All"))
                    itemsFromDatabase = context.Inventory.Where(item => item.Active).OrderBy(item => item.Name);
                else
                    itemsFromDatabase =
                        context.Inventory.Where(item => item.Category.ID.Equals(_selectedCategory.ID) && item.Active)
                            .OrderBy(item => item.Name);

                var allItem = new Item {ItemID = "-1", Name = "All"};
                ListedItems.Add(new ItemVM {Model = allItem});
                foreach (var item in itemsFromDatabase)
                    ListedItems.Add(new ItemVM {Model = item});
            }
        }

        private void UpdateCustomers()
        {
            Customers.Clear();
            using (var context = UtilityMethods.createContext())
            {
                var allCustomer = new Customer {ID = -1, Name = "All"};
                Customers.Add(new CustomerVM {Model = allCustomer});
                var customersFromDatabase = context.Customers.Where(customer => customer.Active).OrderBy(customer => customer.Name);
                foreach (var customer in customersFromDatabase)
                    Customers.Add(new CustomerVM {Model = customer});
            }
        }

        private void ClearScreen()
        {
            SelectedYear = Years.First();
            SelectedCustomer = Customers.First();
            SelectedCategory = null;
            SelectedItem = null;
            UpdateCategories();
            DisplayedLines.Clear();
        }

        private bool IsThereSelectedItem()
        {
            if (_selectedItem != null) return true;
            MessageBox.Show("Please select an item.", "Missing Selection", MessageBoxButton.OK);
            return false;
        }

        private bool IsThereSelectedCustomer()
        {
            if (_selectedCustomer != null) return true;
            MessageBox.Show("Please select a customer.", "Missing Selection", MessageBoxButton.OK);
            return false;
        }

        private void UpdateLines()
        {
            DisplayedLines.Clear();
            using (var context = UtilityMethods.createContext())
            {
                if (_selectedItem.Name.Equals("All"))
                    LoadAllItemsSalesAnalysis(context);
                else
                    LoadItemSalesAnalysis(context, _selectedItem);
            }
        }

        private void LoadAllItemsSalesAnalysis(ERPContext context)
        {
            foreach (var item in ListedItems.Where(item => !item.Name.Equals("All")))
                LoadItemSalesAnalysis(context, item);    
        }

        private void LoadItemSalesAnalysis(ERPContext context, ItemVM item)
        {
            var line = new SalesAnalysisMonthlyLineVM { Item = item };
            for (var i = 1; i <= 12; i++)
            {
                int previousMonthSales;
                switch (i)
                {
                    case 1:
                        var janTotalSales = GetItemTotalSalesInMonth(context, item, _selectedYear, 1);
                        previousMonthSales = GetItemTotalSalesInMonth(context, item, _selectedYear - 1, 12);
                        line.Jan = ConvertSalesQuantityToString(item, janTotalSales);
                        line.IsJanSalesDown = janTotalSales < previousMonthSales;
                        break;
                    case 2:
                        var febTotalSales = GetItemTotalSalesInMonth(context, item, _selectedYear, 2);
                        previousMonthSales = GetItemTotalSalesInMonth(context, item, _selectedYear, 1);
                        line.Feb = ConvertSalesQuantityToString(item, febTotalSales);
                        line.IsFebSalesDown = febTotalSales < previousMonthSales;
                        break;
                    case 3:
                        var marTotalSales = GetItemTotalSalesInMonth(context, item, _selectedYear, 3);
                        previousMonthSales = GetItemTotalSalesInMonth(context, item, _selectedYear, 2);
                        line.Mar = ConvertSalesQuantityToString(item, marTotalSales);
                        line.IsMarSalesDown = marTotalSales < previousMonthSales;
                        break;
                    case 4:
                        var aprTotalSales = GetItemTotalSalesInMonth(context, item, _selectedYear, 4);
                        previousMonthSales = GetItemTotalSalesInMonth(context, item, _selectedYear, 3);
                        line.Apr = ConvertSalesQuantityToString(item, aprTotalSales);
                        line.IsAprSalesDown = aprTotalSales < previousMonthSales;
                        break;
                    case 5:
                        var mayTotalSales = GetItemTotalSalesInMonth(context, item, _selectedYear, 5);
                        previousMonthSales = GetItemTotalSalesInMonth(context, item, _selectedYear, 4);
                        line.May = ConvertSalesQuantityToString(item, mayTotalSales);
                        line.IsMaySalesDown = mayTotalSales < previousMonthSales;
                        break;
                    case 6:
                        var junTotalSales = GetItemTotalSalesInMonth(context, item, _selectedYear, 6);
                        previousMonthSales = GetItemTotalSalesInMonth(context, item, _selectedYear, 5);
                        line.Jun = ConvertSalesQuantityToString(item, junTotalSales);
                        line.IsJunSalesDown = junTotalSales < previousMonthSales;
                        break;
                    case 7:
                        var julTotalSales = GetItemTotalSalesInMonth(context, item, _selectedYear, 7);
                        previousMonthSales = GetItemTotalSalesInMonth(context, item, _selectedYear, 6);
                        line.Jul = ConvertSalesQuantityToString(item, julTotalSales);
                        line.IsJulSalesDown = julTotalSales < previousMonthSales;
                        break;
                    case 8:
                        var augTotalSales = GetItemTotalSalesInMonth(context, item, _selectedYear, 8);
                        previousMonthSales = GetItemTotalSalesInMonth(context, item, _selectedYear, 7);
                        line.Aug = ConvertSalesQuantityToString(item, augTotalSales);
                        line.IsAugSalesDown = augTotalSales < previousMonthSales;
                        break;
                    case 9:
                        var sepTotalSales = GetItemTotalSalesInMonth(context, item, _selectedYear, 9);
                        previousMonthSales = GetItemTotalSalesInMonth(context, item, _selectedYear, 8);
                        line.Sep = ConvertSalesQuantityToString(item, sepTotalSales);
                        line.IsSepSalesDown = sepTotalSales < previousMonthSales;
                        break;
                    case 10:
                        var octTotalSales = GetItemTotalSalesInMonth(context, item, _selectedYear, 10);
                        previousMonthSales = GetItemTotalSalesInMonth(context, item, _selectedYear, 9);
                        line.Oct = ConvertSalesQuantityToString(item, octTotalSales);
                        line.IsOctSalesDown = octTotalSales < previousMonthSales;
                        break;
                    case 11:
                        var novTotalSales = GetItemTotalSalesInMonth(context, item, _selectedYear, 11);
                        previousMonthSales = GetItemTotalSalesInMonth(context, item, _selectedYear, 10);
                        line.Nov = ConvertSalesQuantityToString(item, novTotalSales);
                        line.IsNovSalesDown = novTotalSales < previousMonthSales;
                        break;
                    default:
                        var decTotalSales = GetItemTotalSalesInMonth(context, item, _selectedYear, 12);
                        previousMonthSales = GetItemTotalSalesInMonth(context, item, _selectedYear, 11);
                        line.Dec = ConvertSalesQuantityToString(item, decTotalSales);
                        line.IsDecSalesDown = decTotalSales < previousMonthSales;
                        break;
                }
            }

            DisplayedLines.Add(line);
        }

        private int GetItemTotalSalesInMonth(ERPContext context, ItemVM item, int year, int month)
        {
            var totalSalesQuantity = 0;
            IEnumerable<SalesTransactionLine> monthSalesTransactionLines;
            IEnumerable<SalesReturnTransactionLine> monthSalesReturnTransactionLines;

            if (_selectedCustomer.Name.Equals("All"))
            {
                monthSalesTransactionLines =
                    context.SalesTransactionLines.Where(
                        line => line.SalesTransaction.Date.Year.Equals(year) &&
                                line.SalesTransaction.Date.Month.Equals(month) &&
                                line.ItemID.Equals(item.ID));

                monthSalesReturnTransactionLines =
                    context.SalesReturnTransactionLines.Where(
                        line => line.SalesReturnTransaction.Date.Year.Equals(year) &&
                                line.SalesReturnTransaction.Date.Month.Equals(month) &&
                                line.ItemID.Equals(item.ID));
            }

            else
            {
                monthSalesTransactionLines =
                    context.SalesTransactionLines.Where(
                        line => line.SalesTransaction.Customer.ID.Equals(_selectedCustomer.ID) &&
                                line.SalesTransaction.Date.Year.Equals(year) &&
                                line.SalesTransaction.Date.Month.Equals(month) &&
                                line.ItemID.Equals(item.ID));

                monthSalesReturnTransactionLines =
                    context.SalesReturnTransactionLines.Where(
                        line => line.SalesReturnTransaction.SalesTransaction.Customer.ID.Equals(_selectedCustomer.ID) &&
                                line.SalesReturnTransaction.Date.Year.Equals(year) &&
                                line.SalesReturnTransaction.Date.Month.Equals(month) &&
                                line.ItemID.Equals(item.ID));
            }

            foreach (var line in monthSalesTransactionLines)
                totalSalesQuantity += line.Quantity;

            foreach (var line in monthSalesReturnTransactionLines)
                totalSalesQuantity -= line.Quantity;

            return totalSalesQuantity;
        }

        private static string ConvertSalesQuantityToString(ItemVM item, int quantity)
        {
            if (quantity == 0) return "0/0/0";
            var units = quantity/item.PiecesPerUnit;
            var secondaryUnits = item.PiecesPerSecondaryUnit == 0
                ? 0
                : quantity%item.PiecesPerUnit/item.PiecesPerSecondaryUnit;
            var pieces = item.PiecesPerSecondaryUnit == 0
                ? quantity%item.PiecesPerUnit
                : quantity%item.PiecesPerUnit%item.PiecesPerSecondaryUnit;
            return units + "/" + secondaryUnits + "/" + pieces;
        }
        #endregion
    }
}
