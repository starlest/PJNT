namespace ECRP.ViewModels.Analysis
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
    using Utilities.Analysis;

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
            using (var context = UtilityMethods.createContext())
            {
                var categoriesFromDatabase = context.ItemCategories.OrderBy(category => category.Name);
                foreach (var category in categoriesFromDatabase)
                    Categories.Add(new CategoryVM { Model = category });
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
            using (var context = UtilityMethods.createContext())
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
                        var janTotalSales = SalesAnalysisHelper.GetSalesForecastInMonth(context, item, _selectedYear, 1);
                        line.Jan = ConvertSalesQuantityToString(item, janTotalSales);
                        if (SalesAnalysisHelper.GetItemTotalSalesInMonth(context, item, _selectedYear, 1) < janTotalSales)
                            line.IsJanTargetNotMet = true;
                        break;
                    case 2:
                        var febTotalSales = SalesAnalysisHelper.GetSalesForecastInMonth(context, item, _selectedYear, 2);
                        line.Feb = ConvertSalesQuantityToString(item, febTotalSales);
                        if (SalesAnalysisHelper.GetItemTotalSalesInMonth(context, item, _selectedYear, 2) < febTotalSales)
                            line.IsFebTargetNotMet = true;
                        break;
                    case 3:
                        var marTotalSales = SalesAnalysisHelper.GetSalesForecastInMonth(context, item, _selectedYear, 3);
                        line.Mar = ConvertSalesQuantityToString(item, marTotalSales);
                        if (SalesAnalysisHelper.GetItemTotalSalesInMonth(context, item, _selectedYear, 3) < marTotalSales)
                            line.IsMarTargetNotMet = true;
                        break;
                    case 4:
                        var aprTotalSales = SalesAnalysisHelper.GetSalesForecastInMonth(context, item, _selectedYear, 4);
                        line.Apr = ConvertSalesQuantityToString(item, aprTotalSales);
                        if (SalesAnalysisHelper.GetItemTotalSalesInMonth(context, item, _selectedYear, 4) < aprTotalSales)
                            line.IsAprTargetNotMet = true;
                        break;
                    case 5:
                        var mayTotalSales = SalesAnalysisHelper.GetSalesForecastInMonth(context, item, _selectedYear, 5);
                        line.May = ConvertSalesQuantityToString(item, mayTotalSales);
                        if (SalesAnalysisHelper.GetItemTotalSalesInMonth(context, item, _selectedYear, 5) < mayTotalSales)
                            line.IsMayTargetNotMet = true;
                        break;
                    case 6:
                        var junTotalSales = SalesAnalysisHelper.GetSalesForecastInMonth(context, item, _selectedYear, 6);
                        line.Jun = ConvertSalesQuantityToString(item, junTotalSales);
                        if (SalesAnalysisHelper.GetItemTotalSalesInMonth(context, item, _selectedYear, 6) < junTotalSales)
                            line.IsJunTargetNotMet = true;
                        break;
                    case 7:
                        var julTotalSales = SalesAnalysisHelper.GetSalesForecastInMonth(context, item, _selectedYear, 7);
                        line.Jul = ConvertSalesQuantityToString(item, julTotalSales);
                        if (SalesAnalysisHelper.GetItemTotalSalesInMonth(context, item, _selectedYear, 7) < julTotalSales)
                            line.IsJulTargetNotMet = true;
                        break;
                    case 8:
                        var augTotalSales = SalesAnalysisHelper.GetSalesForecastInMonth(context, item, _selectedYear, 8);
                        line.Aug = ConvertSalesQuantityToString(item, augTotalSales);
                        if (SalesAnalysisHelper.GetItemTotalSalesInMonth(context, item, _selectedYear, 8) < augTotalSales)
                            line.IsAugTargetNotMet = true;
                        break;
                    case 9:
                        var sepTotalSales = SalesAnalysisHelper.GetSalesForecastInMonth(context, item, _selectedYear, 9);
                        line.Sep = ConvertSalesQuantityToString(item, sepTotalSales);
                        if (SalesAnalysisHelper.GetItemTotalSalesInMonth(context, item, _selectedYear, 9) < sepTotalSales)
                            line.IsSepTargetNotMet = true;
                        break;
                    case 10:
                        var octTotalSales = SalesAnalysisHelper.GetSalesForecastInMonth(context, item, _selectedYear, 10);
                        line.Oct = ConvertSalesQuantityToString(item, octTotalSales);
                        if (SalesAnalysisHelper.GetItemTotalSalesInMonth(context, item, _selectedYear, 10) < octTotalSales)
                            line.IsOctTargetNotMet = true;
                        break;
                    case 11:
                        var novTotalSales = SalesAnalysisHelper.GetSalesForecastInMonth(context, item, _selectedYear, 11);
                        line.Nov = ConvertSalesQuantityToString(item, novTotalSales);
                        if (SalesAnalysisHelper.GetItemTotalSalesInMonth(context, item, _selectedYear, 11) < novTotalSales)
                            line.IsNovTargetNotMet = true;
                        break;
                    default:
                        var decTotalSales = SalesAnalysisHelper.GetSalesForecastInMonth(context, item, _selectedYear, 12);
                        line.Dec = ConvertSalesQuantityToString(item, decTotalSales);
                        if (SalesAnalysisHelper.GetItemTotalSalesInMonth(context, item, _selectedYear, 12) < decTotalSales)
                            line.IsDecTargetNotMet = true;
                        break;
                }
            }

            DisplayedLines.Add(line);
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
