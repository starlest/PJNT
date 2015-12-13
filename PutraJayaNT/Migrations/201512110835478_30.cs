namespace PUJASM.ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _30 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PurchaseTransactionLines", "Discount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PurchaseTransactionLines", "Discount");
        }
    }
}
