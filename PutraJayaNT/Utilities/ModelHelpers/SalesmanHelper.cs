namespace PutraJayaNT.Utilities.ModelHelpers
{
    using System.Windows;
    using Models.Salesman;

    public static class SalesmanHelper
    {
        public static void AddSalesmanToDatabase(Salesman salesman)
        {
            var context = new ERPContext(UtilityMethods.GetDBName(), UtilityMethods.GetIpAddress());
            var success = true;
            try
            {
                context.Salesmans.Add(salesman);
                context.SaveChanges();
            }
            catch
            {
                MessageBox.Show("The salesman's name is already being used.", "Invalid ID", MessageBoxButton.OK);
                success = false;
            }
            finally
            {
                if (success)
                    MessageBox.Show("Successfully added salesman!", "Success", MessageBoxButton.OK);
                context.Dispose();
            }

        }
    }
}
