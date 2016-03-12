namespace PJMixTests.Database.Item
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using PutraJayaNT.Models.Inventory;
    using PutraJayaNT.Utilities;

    public static class DatabaseItemCategoryHelper
    {
        public static IEnumerable<Category> GetAll()
        {
            using (var context = new ERPContext())
                return context.ItemCategories.OrderBy(category => category.Name).ToList();
        }

        public static Category FirstOrDefault(Func<Category, bool> condition)
        {
            using (var context = new ERPContext())
                return context.ItemCategories.Where(condition).OrderBy(category => category.Name).FirstOrDefault();
        }

        public static void AttachToObjectFromDatabaseContext(ERPContext context, ref Category categoryToBeAttached)
        {
            var categoryID = categoryToBeAttached.ID;
            categoryToBeAttached = context.ItemCategories.First(category => category.ID.Equals(categoryID));
        }
    }
}
