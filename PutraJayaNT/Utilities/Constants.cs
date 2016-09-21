namespace ECRP.Utilities
{
    public static class Constants
    {
        public const string ALL = "All";

        public const string CURRENTUSER = "CurrentUser";
        public const string ISVERIFIED = "IsVerified";
        public const string SELECTEDSERVER = "SelectedServer";
        public const string IPADDRESS = "ipAddress";
        public const string NESTLE = "Nestle";
        public const string MIX = "Mix";

        #region Accounting
        public const string DEBIT = "Debit";
        public const string CREDIT = "Credit";

        public struct LedgerAccountClasses
        {
            public const string ASSET = "Asset";
            public const string LIABILITY = "Liability";
            public const string EQUITY = "Equity";
            public const string EXPENSE = "Expense";
            public const string REVENUE = "Revenue";
        }

        public const string CURRENT_ASSET = "Current Asset";
        public const string ACCOUNTS_RECEIVABLE = "Accounts Receivable";
        public const string ACCOUNTS_PAYABLE = "Accounts Payable";
        public const string OPERATING_EXPENSE = "Operating Expense";
        public const string BANK = "Bank";
        public const string CAPITAL = "Capital";
        public const string RETAINED_EARNINGS = "Retained Earnings";
        public const string COST_OF_GOODS_SOLD = "Cost of Goods Sold";

        #endregion
    }
}