namespace PUJASM.ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update1 : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("SalesReturnTransactionLines");
            DropPrimaryKey("PurchaseReturnTransactionLines");
            AddPrimaryKey("SalesReturnTransactionLines", new[] { "SalesReturnTransactionID", "ItemID", "WarehouseID", "SalesPrice", "Discount", "ReturnPrice" });
            AddPrimaryKey("PurchaseReturnTransactionLines", new[] { "PurchaseReturnTransactionID", "ItemID", "WarehouseID", "ReturnWarehouseID", "PurchasePrice", "Discount", "ReturnPrice" });
        }
        
        public override void Down()
        {
            DropPrimaryKey("PurchaseReturnTransactionLines");
            DropPrimaryKey("SalesReturnTransactionLines");
            AddPrimaryKey("PurchaseReturnTransactionLines", new[] { "PurchaseReturnTransactionID", "ItemID", "WarehouseID", "ReturnWarehouseID", "PurchasePrice", "Discount" });
            AddPrimaryKey("SalesReturnTransactionLines", new[] { "SalesReturnTransactionID", "ItemID", "WarehouseID", "SalesPrice", "Discount" });
        }
    }
}
