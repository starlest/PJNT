namespace PutraJayaNT.ViewModels.Analysis
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using Item;
    using Models.Inventory;
    using MVVMFramework;
    using Utilities;

    internal class SalesForecastMonthlyVM : ViewModelBase
    {
        #region Backing Fields
        private ushort _selectedYear;
        private CategoryVM _selectedCategory;
        private ItemVM _selectedItem;
        private ICommand _clearCommand;
        private ICommand _displayCommand;
        #endregion

        public SalesForecastMonthlyVM()
        {
            DisplayedLines = new ObservableCollection<SalesForecastMonthlyLineVM>();
            Years = new ObservableCollection<ushort>();
            Categories = new ObservableCollection<CategoryVM>();
            ListedItems = new ObservableCollection<ItemVM>();
            UpdateYears();
            UpdateCategories();
            _selectedYear = Years.First();
        }

        #region Collections
        public ObservableCollection<SalesForecastMonthlyLineVM> DisplayedLines { get; }

        public ObservableCollection<ushort> Years { get; }

        public ObservableCollection<CategoryVM> Categories { get; }

        public ObservableCollection<ItemVM> ListedItems { get; }
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
        #endregion

        #region Commands
        public ICommand ClearCommand => _clearCommand ?? (_clearCommand = new RelayCommand(ClearScreen));

        public ICommand DisplayCommand
        {
            get
            {
                return _displayCommand ?? (_displayCommand = new RelayCommand(() =>
                {
                    if (!IsThereSelectedItem()) return;
                    UpdateLines();
                }));
            }
        }
        #endregion

        #region Helper Methods
        private void UpdateYears()
        {
            Years.Clear();
            var currentYear = (ushort)UtilityMethods.GetCurrentDate().Year;
            for (var year = currentYear; year >= 2016; year--)
                Years.Add(year);
        }

        private void UpdateCategories()
        {
            Categories.Clear();
            var allCategory = new Category { ID = -1, Name = "All" };
            Categories.Add(new CategoryVM { Model = allCategory });
            using (var context = new ERPContext(UtilityMethods.GetDBName(), UtilityMethods.GetIpAddress()))
            {
                var categoriesFromDatabase = context.ItemCategories.OrderBy(category => category.Name);
                foreach (var category in categoriesFromDatabase)
                    Categories.Add(new CategoryVM { Model = category });
            }
        }

        private void UpdateListedItems()
        {
            ListedItems.Clear();
            using (var context = new ERPContext(UtilityMethods.GetDBName(), UtilityMethods.GetIpAddress()))
            {
                IEnumerable<Item> itemsFromDatabase;

                if (_selectedCategory.Name.Equals("All"))
                    itemsFromDatabase = context.Inventory.Where(item => item.Active).OrderBy(item => item.Name);
                else
                    itemsFromDatabase =
                        context.Inventory.Where(item => item.Category.ID.Equals(_selectedCategory.ID) && item.Active)
                            .OrderBy(item => item.Name);

                var allItem = new Item { ItemID = "-1", Name = "All" };
                ListedItems.Add(new ItemVM { Model = allItem });
                foreach (var item in itemsFromDatabase)
                    ListedItems.Add(new ItemVM { Model = item });
            }
        }

        private void ClearScreen()
        {
            SelectedYear = Years.First();
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

        private void UpdateLines()
        {
            DisplayedLines.Clear();
            using (var context = new ERPContext(UtilityMethods.GetDBName(), UtilityMethods.GetIpAddress()))
            {
                if (_selectedItem.Name.Equals("All"))
                    LoadAllItemsSalesForecast(context);
                else
                    LoadItemSalesForecast(context, _selectedItem);
            }
        }

        private void LoadAllItemsSalesForecast(ERPContext context)
        {
            foreach (var item in ListedItems.Where(item => !item.Name.Equals("All")))
                LoadItemSalesForecast(context, item);
        }

        private void LoadItemSalesForecast(ERPContext context, ItemVM item)
        {
            var line = new SalesForecastMonthlyLineVM { Item = item };
            for (var i = 1; i <= 12; i++)
            {
                switch (i)
                {
                    case 1:
                        var janTotalSales = GetSalesForecastInMonth(context, item, 1);
                        line.Jan = ConvertSalesQuantityToString(item, janTotalSales);
                        if (GetItemTotalSalesInMonth(context, item, _selectedYear, 1) < janTotalSales)
                            line.IsJanTargetNotMet = true;
                        break;
                    case 2:
                        var febTotalSales = GetSalesForecastInMonth(context, item, 2);
                        line.Feb = ConvertSalesQuantityToString(item, febTotalSales);
                        if (GetItemTotalSalesInMonth(context, item, _selectedYear, 2) < febTotalSales)
                            line.IsFebTargetNotMet = true;
                        break;
                    case 3:
                        var marTotalSales = GetSalesForecastInMonth(context, item, 3);
                        line.Mar = ConvertSalesQuantityToString(item, marTotalSales);
                        if (GetItemTotalSalesInMonth(context, item, _selectedYear, 3) < marTotalSales)
                            line.IsMarTargetNotMet = true;
                        break;
                    case 4:
                        var aprTotalSales = GetSalesForecastInMonth(context, item, 4);
                        line.Apr = ConvertSalesQuantityToString(item, aprTotalSales);
                        if (GetItemTotalSalesInMonth(context, item, _selectedYear, 4) < aprTotalSales)
                            line.IsAprTargetNotMet = true;
                        break;
                    case 5:
                        var mayTotalSales = GetSalesForecastInMonth(context, item, 5);
                        line.May = ConvertSalesQuantityToString(item, mayTotalSales);
                        if (GetItemTotalSalesInMonth(context, item, _selectedYear, 5) < mayTotalSales)
                            line.IsMayTargetNotMet = true;
                        break;
                    case 6:
                        var junTotalSales = GetSalesForecastInMonth(context, item, 6);
                        line.Jun = ConvertSalesQuantityToString(item, junTotalSales);
                        if (GetItemTotalSalesInMonth(context, item, _selectedYear, 6) < junTotalSales)
                            line.IsJunTargetNotMet = true;
                        break;
                    case 7:
                        var julTotalSales = GetSalesForecastInMonth(context, item, 7);
                        line.Jul = ConvertSalesQuantityToString(item, julTotalSales);
                        if (GetItemTotalSalesInMonth(context, item, _selectedYear, 7) < julTotalSales)
                            line.IsJulTargetNotMet = true;
                        break;
                    case 8:
                        var augTotalSales = GetSalesForecastInMonth(context, item, 8);
                        line.Aug = ConvertSalesQuantityToString(item, augTotalSales);
                        if (GetItemTotalSalesInMonth(context, item, _selectedYear, 8) < augTotalSales)
                            line.IsAugTargetNotMet = true;
                        break;
                    case 9:
                        var sepTotalSales = GetSalesForecastInMonth(context, item, 9);
                        line.Sep = ConvertSalesQuantityToString(item, sepTotalSales);
                        if (GetItemTotalSalesInMonth(context, item, _selectedYear, 9) < sepTotalSales)
                            line.IsSepTargetNotMet = true;
                        break;
                    case 10:
                        var octTotalSales = GetSalesForecastInMonth(context, item, 10);
                        line.Oct = ConvertSalesQuantityToString(item, octTotalSales);
                        if (GetItemTotalSalesInMonth(context, item, _selectedYear, 10) < octTotalSales)
                            line.IsOctTargetNotMet = true;
                        break;
                    case 11:
                        var novTotalSales = GetSalesForecastInMonth(context, item, 11);
                        line.Nov = ConvertSalesQuantityToString(item, novTotalSales);
                        if (GetItemTotalSalesInMonth(context, item, _selectedYear, 11) < novTotalSales)
                            line.IsNovTargetNotMet = true;
                        break;
                    default:
                        var decTotalSales = GetSalesForecastInMonth(context, item, 12);
                        line.Dec = ConvertSalesQuantityToString(item, decTotalSales);
                        if (GetItemTotalSalesInMonth(context, item, _selectedYear, 12) < decTotalSales)
                            line.IsDecTargetNotMet = true;
                        break;
                }
            }

            DisplayedLines.Add(line);
        }

        private int GetSalesForecastInMonth(ERPContext context, ItemVM item, int month)
        {
            var year = _selectedYear;
            if (month == 1) year--;
            var previousMonthTotalSales = GetItemTotalSalesInMonth(context, item, year, month - 1);
            if (month == 2) year--;
            var secondPreviousMonthTotalSales = GetItemTotalSalesInMonth(context, item, year, month - 2);
            if (month == 3) year--;
            var thirdPreviousMonthTotalSales = GetItemTotalSalesInMonth(context, item, year, month - 3);

            if (previousMonthTotalSales == 0) return 0;
            if (secondPreviousMonthTotalSales == 0) return previousMonthTotalSales;
            return thirdPreviousMonthTotalSales == 0
                ? (previousMonthTotalSales + secondPreviousMonthTotalSales) /2
                : (previousMonthTotalSales + secondPreviousMonthTotalSales + thirdPreviousMonthTotalSales)/3;
        }

        private static int GetItemTotalSalesInMonth(ERPContext context, ItemVM item, int year, int month)
        {
            var totalSalesQuantity = 0;

            var monthSalesTransactionLines =
                context.SalesTransactionLines.Where(
                    line => line.SalesTransaction.Date.Year.Equals(year) && line.SalesTransaction.Date.Month.Equals(month) && line.ItemID.Equals(item.ID));

            var monthSalesReturnTransactionLines =
                context.SalesReturnTransactionLines.Where(
                    line => line.SalesReturnTransaction.Date.Year.Equals(year) && line.SalesReturnTransaction.Date.Month.Equals(month) && line.ItemID.Equals(item.ID));

            foreach (var line in monthSalesTransactionLines)
                totalSalesQuantity += line.Quantity;

            foreach (var line in monthSalesReturnTransactionLines)
                totalSalesQuantity -= line.Quantity;

            return totalSalesQuantity;
        }

        private static string ConvertSalesQuantityToString(ItemVM item, int quantity)
        {
            if (quantity == 0) return "0/0/0";
            var units = quantity / item.PiecesPerUnit;
            var secondaryUnits = item.PiecesPerSecondaryUnit == 0
                ? 0
                : quantity % item.PiecesPerUnit / item.PiecesPerSecondaryUnit;
            var pieces = item.PiecesPerSecondaryUnit == 0
                ? quantity % item.PiecesPerUnit
                : quantity % item.PiecesPerUnit % item.PiecesPerSecondaryUnit;
            return units + "/" + secondaryUnits + "/" + pieces;
        }
        #endregion
    }
}
