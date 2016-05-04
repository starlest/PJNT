namespace PutraJayaNT.ViewModels.Analysis
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using Item;
    using MVVMFramework;
    using Utilities;

    internal class SalesAnalysisMonthlyVM : ViewModelBase
    {
        private ushort _selectedYear;
        private CategoryVM _selectedCategory;

        public SalesAnalysisMonthlyVM()
        {
            DisplayedLines = new ObservableCollection<SalesAnalysisMonthlyLineVM>();
            Years = new ObservableCollection<ushort>();
            Categories = new ObservableCollection<CategoryVM>();
            ListedItems = new ObservableCollection<ItemVM>();
            UpdateYears();
            UpdateCategories();
            UpdateListedItems();
        }

        #region Collections
        public ObservableCollection<SalesAnalysisMonthlyLineVM> DisplayedLines { get; } 

        public ObservableCollection<ushort> Years { get; }

        public  ObservableCollection<CategoryVM> Categories { get; } 

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
            set { SetProperty(ref _selectedCategory, value, () => SelectedCategory); }
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
            using (var context = new ERPContext(UtilityMethods.GetDBName()))
            {
                var categoriesFromDatabase = context.ItemCategories.OrderBy(category => category.Name);
                foreach (var category in categoriesFromDatabase)
                    Categories.Add(new CategoryVM {Model = category});
            }
        }

        private void UpdateListedItems()
        {
            ListedItems.Clear();
            using (var context = new ERPContext(UtilityMethods.GetDBName()))
            {
               // var itemsFromDatabase = context.Inventory.Where(item =>)
            }
        }
        #endregion
    }
}
