namespace PUJASM.ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update10 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SalesReturnTransactionLines", "ReturnPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PurchaseReturnTransactionLines", "ReturnPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropColumn("dbo.SalesReturnTransactionLines", "NetDiscount");
            DropColumn("dbo.PurchaseReturnTransactionLines", "NetDiscount");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PurchaseReturnTransactionLines", "NetDiscount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.SalesReturnTransactionLines", "NetDiscount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropColumn("dbo.PurchaseReturnTransactionLines", "ReturnPrice");
            DropColumn("dbo.SalesReturnTransactionLines", "ReturnPrice");
        }
    }
}
