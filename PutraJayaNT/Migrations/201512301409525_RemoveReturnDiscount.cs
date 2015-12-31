namespace PUJASM.ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveReturnDiscount : DbMigration
    {
        public override void Up()
        {
            DropColumn("SalesReturnTransactions", "GrossTotal");
            DropColumn("SalesReturnTransactions", "SalesTransactionDiscountIncluded");
            DropColumn("PurchaseReturnTransactions", "GrossTotal");
            DropColumn("PurchaseReturnTransactions", "PurchaseTransactionDiscountIncluded");
        }
        
        public override void Down()
        {
            AddColumn("PurchaseReturnTransactions", "PurchaseTransactionDiscountIncluded", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("PurchaseReturnTransactions", "GrossTotal", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("SalesReturnTransactions", "SalesTransactionDiscountIncluded", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("SalesReturnTransactions", "GrossTotal", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}
