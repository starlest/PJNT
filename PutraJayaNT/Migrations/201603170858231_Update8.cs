namespace PutraJayaNT.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update8 : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "AdjustStockTransactionLines", newName: "StockAdjustmentTransactionLines");
        }
        
        public override void Down()
        {
            RenameTable(name: "StockAdjustmentTransactionLines", newName: "AdjustStockTransactionLines");
        }
    }
}
