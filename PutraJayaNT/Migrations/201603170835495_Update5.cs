namespace PutraJayaNT.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update5 : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "MoveStockTransactions", newName: "StockMovementTransactions");
        }
        
        public override void Down()
        {
            RenameTable(name: "StockMovementTransactions", newName: "MoveStockTransactions");
        }
    }
}
