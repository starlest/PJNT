namespace PUJASM.ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update0003 : DbMigration
    {
        public override void Up()
        {
            AddColumn("SalesReturnTransactions", "SalesTransactionDiscountIncluded", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("PurchaseReturnTransactions", "PurchaseTransactionDiscountIncluded", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("PurchaseReturnTransactions", "PurchaseTransactionDiscountIncluded");
            DropColumn("SalesReturnTransactions", "SalesTransactionDiscountIncluded");
        }
    }
}
