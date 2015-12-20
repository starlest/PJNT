namespace PUJASM.ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update0001 : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("PurchaseTransactionLines");
            AddPrimaryKey("PurchaseTransactionLines", new[] { "PurchaseTransactionID", "ItemID", "WarehouseID", "PurchasePrice", "Discount" });
        }
        
        public override void Down()
        {
            DropPrimaryKey("PurchaseTransactionLines");
            AddPrimaryKey("PurchaseTransactionLines", new[] { "PurchaseTransactionID", "ItemID", "WarehouseID" });
        }
    }
}
