namespace PUJASM.ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update1 : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("PurchaseReturnTransactionLines");
            AddColumn("SalesReturnTransactionLines", "NetDiscount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("PurchaseReturnTransactionLines", "ReturnWarehouseID", c => c.Int(nullable: false));
            AddPrimaryKey("PurchaseReturnTransactionLines", new[] { "PurchaseReturnTransactionID", "ItemID", "WarehouseID", "ReturnWarehouseID", "PurchasePrice", "Discount" });
            CreateIndex("PurchaseReturnTransactionLines", "ReturnWarehouseID");
            AddForeignKey("PurchaseReturnTransactionLines", "ReturnWarehouseID", "Warehouses", "ID", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("PurchaseReturnTransactionLines", "ReturnWarehouseID", "Warehouses");
            DropIndex("PurchaseReturnTransactionLines", new[] { "ReturnWarehouseID" });
            DropPrimaryKey("PurchaseReturnTransactionLines");
            DropColumn("PurchaseReturnTransactionLines", "ReturnWarehouseID");
            DropColumn("SalesReturnTransactionLines", "NetDiscount");
            AddPrimaryKey("PurchaseReturnTransactionLines", new[] { "PurchaseReturnTransactionID", "ItemID", "WarehouseID", "PurchasePrice", "Discount" });
        }
    }
}
