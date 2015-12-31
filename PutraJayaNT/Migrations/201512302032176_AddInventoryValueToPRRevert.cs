namespace PUJASM.ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddInventoryValueToPRRevert : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.PurchaseReturnTransactionLines", "InventoryValue");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PurchaseReturnTransactionLines", "InventoryValue", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}
