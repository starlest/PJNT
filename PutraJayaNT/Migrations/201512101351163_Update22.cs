namespace PUJASM.ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update22 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Suppliers", "PurchaseReturnCredits", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Suppliers", "PurchaseReturnCredits");
        }
    }
}
