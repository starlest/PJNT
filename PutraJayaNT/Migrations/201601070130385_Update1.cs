namespace PUJASM.ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AdjustStockTransactionLines",
                c => new
                {
                    AdjustStockTransactionID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                    ItemID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                    WarehouseID = c.Int(nullable: false),
                    Quantity = c.Int(nullable: false),
                })
                .PrimaryKey(t => new { t.AdjustStockTransactionID, t.ItemID, t.WarehouseID })
                .ForeignKey("dbo.AdjustStockTransactions", t => t.AdjustStockTransactionID, cascadeDelete: true)
                .ForeignKey("dbo.Inventory", t => t.ItemID, cascadeDelete: true)
                .ForeignKey("dbo.Warehouses", t => t.WarehouseID, cascadeDelete: true)
                .Index(t => t.AdjustStockTransactionID)
                .Index(t => t.ItemID)
                .Index(t => t.WarehouseID);

            CreateTable(
                "dbo.AdjustStockTransactions",
                c => new
                {
                    AdjustStrockTransactionID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                    Date = c.DateTime(nullable: false, precision: 0),
                    User_Username = c.String(maxLength: 128, storeType: "nvarchar"),
                })
                .PrimaryKey(t => t.AdjustStrockTransactionID)
                .ForeignKey("dbo.Users", t => t.User_Username)
                .Index(t => t.User_Username);

        }

        public override void Down()
        {
            
        }
    }
}
