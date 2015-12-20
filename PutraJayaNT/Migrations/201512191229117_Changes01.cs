namespace PUJASM.ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Changes01 : DbMigration
    {
        public override void Up()
        {
            AddColumn("PurchaseTransactions", "GrossTotal", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("PurchaseTransactions", "Discount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("PurchaseTransactions", "Discount");
            DropColumn("PurchaseTransactions", "GrossTotal");
        }
    }
}
