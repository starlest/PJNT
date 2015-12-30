namespace PUJASM.ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddInventoryValueToPR : DbMigration
    {
        public override void Up()
        {
            AddColumn("PurchaseReturnTransactionLines", "InventoryValue", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("PurchaseReturnTransactionLines", "InventoryValue");
        }
    }
}
