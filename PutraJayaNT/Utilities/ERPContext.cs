namespace PutraJayaNT.Utilities
{
    using Models;
    using Models.Accounting;
    using MySql.Data.Entity;
    using System.Data.Entity;

    public partial class ERPContext : DbContext
    {
        public ERPContext()
            : base("name=ERPContext")
        {
            //Database.SetInitializer<ERPContext>(new MigrateDatabaseToLatestVersion<ERPContext, Migrations.Configuration>());
            //Database.SetInitializer<ERPContext>(new DropCreateDatabaseAlways<ERPContext>());
        }

        public virtual DbSet<Item> Inventory { get; set; }
        public virtual DbSet<Category> Category { get; set; }
        public virtual DbSet<SalesTransactionLine> SalesTransactionLines { get; set; }
        public virtual DbSet<SalesTransaction> SalesTransactions { get; set; }
        public virtual DbSet<SalesReturnTransaction> SalesReturnTransactions { get; set; }
        public virtual DbSet<SalesReturnTransactionLine> SalesReturnTransactionLines { get; set; }
        public virtual DbSet<PurchaseReturnTransaction> PurchaseReturnTransactions { get; set; }
        public virtual DbSet<PurchaseReturnTransactionLine> PurchaseReturnTransactionLines { get; set; }
        public virtual DbSet<PurchaseTransactionLine> PurchaseTransactionLines { get; set; }
        public virtual DbSet<PurchaseTransaction> PurchaseTransactions { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Supplier> Suppliers { get; set; }

        public virtual DbSet<LedgerAccount> Ledger_Accounts { get; set; }
        public virtual DbSet<LedgerTransaction> Ledger_Transactions { get; set; }
        public virtual DbSet<LedgerTransactionLine> Ledger_Transaction_Lines { get; set; }
        public virtual DbSet<LedgerGeneral> Ledger_General { get; set; }
        public virtual DbSet<LedgerAccountBalance> Ledger_Account_Balances { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasMany(e => e.Transactions)
                .WithOptional(e => e.User)
                .HasForeignKey(e => e.CashierName);
        }
    }
}
