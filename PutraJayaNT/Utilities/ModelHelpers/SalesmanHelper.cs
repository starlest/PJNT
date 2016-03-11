namespace PutraJayaNT.Utilities.ModelHelpers
{
    using Models.Salesman;

    public static class SalesmanHelper
    {
        public static void AddSalesmanToDatabase(Salesman salesman)
        {
            using (var context = new ERPContext())
            {
                context.Salesmans.Add(salesman);
                context.SaveChanges();
            }
        }
    }
}
