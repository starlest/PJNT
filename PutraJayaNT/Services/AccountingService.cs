namespace PutraJayaNT.Services
{
    using System.Collections.Generic;

    internal static class AccountingService
    {
        private static readonly List<string> _protectedAccounts = new List<string>
        {
            "Sales Returns And Allowances",
            "Cost of Goods Sold",
            "- Accounts Payable",
            "Inventory"
        };

        public static List<string> GetProtectedAccounts() => _protectedAccounts;
    }
}
