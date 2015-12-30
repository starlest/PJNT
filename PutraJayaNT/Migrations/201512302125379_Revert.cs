namespace PUJASM.ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Revert : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PurchaseReturnTransactionLines", "NetDiscount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropColumn("dbo.PurchaseTransactionLines", "NetDiscount");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PurchaseTransactionLines", "NetDiscount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropColumn("dbo.PurchaseReturnTransactionLines", "NetDiscount");
        }
    }
}
