namespace ECERP.Utilities
{
    using System.Configuration;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using Models;
    using Models.Accounting;
    using Models.Customer;
    using Models.Inventory;
    using Models.Purchase;
    using Models.Sales;
    using Models.Salesman;
    using Models.StockCorrection;
    using Models.Supplier;

    public class ERPContext : DbContext
    {
        public ERPContext()
        {
        }

        public ERPContext(string dbName, string ipAddress)
            : base(GetConnectionString(dbName, ipAddress))
        {
            var adapter = (IObjectContextAdapter)this;
            var objectContext = adapter.ObjectContext;
            objectContext.CommandTimeout = 3 * 60;
        }

        public static string GetConnectionString(string dbName, string ipAddress)
        {
            var connString =
                ConfigurationManager.ConnectionStrings["ERPContext"].ConnectionString;
            return string.Format(connString, dbName, ipAddress);
        }

        #region Inventory
        public virtual DbSet<Stock> Stocks { get; set; }
        public virtual DbSet<Item> Inventory { get; set; }
        public virtual DbSet<Category> ItemCategories { get; set; }
        public virtual DbSet<Warehouse> Warehouses { get; set; }
        public virtual DbSet<StockBalance> StockBalances { get; set; }
        #endregion

        public virtual DbSet<Salesman> Salesmans { get; set; }
        public virtual DbSet<SalesCommission> SalesCommissions { get; set; }

        #region Stock Changes
        public virtual DbSet<StockAdjustmentTransaction> StockAdjustmentTransactions {get; set; }
        public virtual DbSet<StockAdjustmentTransactionLine> StockAdjustmentTransactionLines { get; set; }
        public virtual DbSet<StockMovementTransaction> StockMovementTransactions { get; set; }
        public virtual DbSet<StockMovementTransactionLine> StockMovementTransactionLines { get; set; }
        #endregion

        public virtual DbSet<SalesTransactionLine> SalesTransactionLines { get; set; }
        public virtual DbSet<SalesTransaction> SalesTransactions { get; set; }
        public virtual DbSet<SalesReturnTransaction> SalesReturnTransactions { get; set; }
        public virtual DbSet<SalesReturnTransactionLine> SalesReturnTransactionLines { get; set; }
        public virtual DbSet<AlternativeSalesPrice> AlternativeSalesPrices { get; set; }

        #region Purchase
        public virtual DbSet<PurchaseReturnTransaction> PurchaseReturnTransactions { get; set; }
        public virtual DbSet<PurchaseReturnTransactionLine> PurchaseReturnTransactionLines { get; set; }
        public virtual DbSet<PurchaseTransactionLine> PurchaseTransactionLines { get; set; }
        public virtual DbSet<PurchaseTransaction> PurchaseTransactions { get; set; }
        #endregion

        #region Entities
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Supplier> Suppliers { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }

        public virtual DbSet<City> Cities { get; set; }

        public virtual DbSet<CustomerGroup> CustomerGroups { get; set; }
        #endregion

        #region Accounting 
        public virtual DbSet<LedgerAccount> Ledger_Accounts { get; set; }
        public virtual DbSet<LedgerAccountClass> Ledger_Account_Classes { get; set; }
        public virtual DbSet<LedgerAccountGroup> Ledger_Account_Groups { get; set; }
        public virtual DbSet<LedgerTransaction> Ledger_Transactions { get; set; }
        public virtual DbSet<LedgerTransactionLine> Ledger_Transaction_Lines { get; set; }
        public virtual DbSet<LedgerGeneral> Ledger_General { get; set; }
        public virtual DbSet<LedgerAccountBalance> Ledger_Account_Balances { get; set; }
        #endregion

        public virtual DbSet<TelegramBotNotification> TelegramBotNotifications { get; set; }

        public virtual DbSet<SystemParameter> SystemParameters { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<DecimalPropertyConvention>();
            modelBuilder.Conventions.Add(new DecimalPropertyConvention(50, 30));
        }
    }
}
