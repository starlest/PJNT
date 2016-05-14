using PutraJayaNT.Models.Customer;

namespace PutraJayaNT.Utilities
{
    using System.Configuration;
    using Models;
    using Models.Accounting;
    using Models.Inventory;
    using System.Data.Entity;
    using Models.Sales;
    using Models.Purchase;
    using Models.StockCorrection;
    using Models.Salesman;
    using System.Data.Entity.ModelConfiguration.Conventions;

    public class ERPContext : DbContext
    {
        public ERPContext(string dbName)
            : base(GetConnectionString(dbName))
        {
            //Database.SetInitializer<ERPContext>(new MigrateDatabaseToLatestVersion<ERPContext, Migrations.Configuration>());
            //Database.SetInitializer<ERPContext>(new DropCreateDatabaseAlways<ERPContext>());
        }

        //public ERPContext()
        //    : base("name=PJMIX")
        //{
        //    //Database.SetInitializer<ERPContext>(new MigrateDatabaseToLatestVersion<ERPContext, Migrations.Configuration>());
        //    //Database.SetInitializer<ERPContext>(new DropCreateDatabaseAlways<ERPContext>());
        //}

        public static string GetConnectionString(string dbName)
        {
            // Server=localhost;Database={0};Uid=username;Pwd=password
            var connString =
                ConfigurationManager.ConnectionStrings["ERPContext"].ConnectionString;
            return string.Format(connString, dbName);
        }

        public virtual DbSet<Stock> Stocks { get; set; }
        public virtual DbSet<Item> Inventory { get; set; }
        public virtual DbSet<Category> ItemCategories { get; set; }
        public virtual DbSet<Warehouse> Warehouses { get; set; }
        public virtual DbSet<StockBalance> StockBalances { get; set; }

        public virtual DbSet<Salesman> Salesmans { get; set; }
        public virtual DbSet<SalesCommission> SalesCommissions { get; set; }

        public virtual DbSet<StockAdjustmentTransaction> StockAdjustmentTransactions {get; set; }
        public virtual DbSet<StockAdjustmentTransactionLine> StockAdjustmentTransactionLines { get; set; }
        public virtual DbSet<StockMovementTransaction> StockMovementTransactions { get; set; }
        public virtual DbSet<StockMovementTransactionLine> StockMovementTransactionLines { get; set; }

        public virtual DbSet<SalesTransactionLine> SalesTransactionLines { get; set; }
        public virtual DbSet<SalesTransaction> SalesTransactions { get; set; }
        public virtual DbSet<SalesReturnTransaction> SalesReturnTransactions { get; set; }
        public virtual DbSet<SalesReturnTransactionLine> SalesReturnTransactionLines { get; set; }
        public virtual DbSet<AlternativeSalesPrice> AlternativeSalesPrices { get; set; }

        public virtual DbSet<PurchaseReturnTransaction> PurchaseReturnTransactions { get; set; }
        public virtual DbSet<PurchaseReturnTransactionLine> PurchaseReturnTransactionLines { get; set; }
        public virtual DbSet<PurchaseTransactionLine> PurchaseTransactionLines { get; set; }
        public virtual DbSet<PurchaseTransaction> PurchaseTransactions { get; set; }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Supplier> Suppliers { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<CustomerGroup> CustomerGroups { get; set; }

        public virtual DbSet<LedgerAccount> Ledger_Accounts { get; set; }
        public virtual DbSet<LedgerTransaction> Ledger_Transactions { get; set; }
        public virtual DbSet<LedgerTransactionLine> Ledger_Transaction_Lines { get; set; }
        public virtual DbSet<LedgerGeneral> Ledger_General { get; set; }
        public virtual DbSet<LedgerAccountBalance> Ledger_Account_Balances { get; set; }

        public virtual DbSet<Date> Dates { get; set; }

        public virtual DbSet<TelegramBotNotification> TelegramBotNotifications { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<DecimalPropertyConvention>();
            modelBuilder.Conventions.Add(new DecimalPropertyConvention(50, 30));
        }
    }
}
