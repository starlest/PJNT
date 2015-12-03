namespace PUJASM.ERP.Utilities
{
    using System.Data.Entity;
    using Models;
    using Models.Accounting;

    public partial class ERPContext : DbContext
    {
        public ERPContext()
            : base("name=ERPContext")
        {
            //Database.SetInitializer<ERPContext>(new MigrateDatabaseToLatestVersion<ERPContext, Migrations.Configuration>());
        }

        public virtual DbSet<Item> Inventory { get; set; }
        public virtual DbSet<SalesTransactionLine> TransactionLines { get; set; }
        public virtual DbSet<SalesTransaction> Transactions { get; set; }
        public virtual DbSet<PurchaseTransactionLine> PurchaseTransactionLines { get; set; }
        public virtual DbSet<PurchaseTransaction> PurchaseTransactions { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Supplier> Suppliers { get; set; }

        public virtual DbSet<LedgerAccount> Ledger_Accounts { get; set; }
        public virtual DbSet<LedgerTransaction> Ledger_Transactions { get; set; }
        public virtual DbSet<LedgerGeneral> Ledger_General { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Item>()
                .HasMany(e => e.TransactionLines)
                .WithRequired(e => e.Item)
                .HasForeignKey(e => e.ItemID);

            modelBuilder.Entity<User>()
                .HasMany(e => e.Transactions)
                .WithOptional(e => e.User)
                .HasForeignKey(e => e.CashierName);
        }
    }
}
