namespace PUJASM.ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Test : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("SalesTransactionLines");
            AddPrimaryKey("SalesTransactionLines", new[] { "SalesTransactionID", "ItemID", "WarehouseID", "SalesPrice", "Discount" });
        }
        
        public override void Down()
        {
            DropPrimaryKey("SalesTransactionLines");
            AddPrimaryKey("SalesTransactionLines", new[] { "SalesTransactionID", "ItemID", "WarehouseID" });
        }
    }
}
