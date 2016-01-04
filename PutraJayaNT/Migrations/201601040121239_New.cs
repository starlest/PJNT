namespace PUJASM.ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class New : DbMigration
    {
        public override void Up()
        {

            CreateTable(
                "dbo.DecreaseStockTransactionLines",
                c => new
                    {
                        DecreaseStockTransactionID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        ItemID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        WarehouseID = c.Int(nullable: false),
                        Quantity = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.DecreaseStockTransactionID, t.ItemID, t.WarehouseID })
                .ForeignKey("dbo.DecreaseStockTransactions", t => t.DecreaseStockTransactionID, cascadeDelete: true)
                .ForeignKey("dbo.Inventory", t => t.ItemID, cascadeDelete: true)
                .ForeignKey("dbo.Warehouses", t => t.WarehouseID, cascadeDelete: true)
                .Index(t => t.DecreaseStockTransactionID)
                .Index(t => t.ItemID)
                .Index(t => t.WarehouseID);
            
            CreateTable(
                "dbo.DecreaseStockTransactions",
                c => new
                    {
                        DecreaseStrockTransactionID = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                        Date = c.DateTime(nullable: false, precision: 0),
                        User_Username = c.String(maxLength: 128, storeType: "nvarchar"),
                    })
                .PrimaryKey(t => t.DecreaseStrockTransactionID)
                .ForeignKey("dbo.Users", t => t.User_Username)
                .Index(t => t.User_Username);
            

        }
        
        public override void Down()
        {
           
        }
    }
}
