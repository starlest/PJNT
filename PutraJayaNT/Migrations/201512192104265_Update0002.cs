namespace PUJASM.ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update0002 : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("PurchaseReturnTransactionLines");
            AddColumn("PurchaseReturnTransactionLines", "Discount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddPrimaryKey("PurchaseReturnTransactionLines", new[] { "PurchaseReturnTransactionID", "ItemID", "WarehouseID", "PurchasePrice", "Discount" });
        }
        
        public override void Down()
        {
            DropPrimaryKey("PurchaseReturnTransactionLines");
            DropColumn("PurchaseReturnTransactionLines", "Discount");
            AddPrimaryKey("PurchaseReturnTransactionLines", new[] { "PurchaseReturnTransactionID", "ItemID", "WarehouseID" });
        }
    }
}
