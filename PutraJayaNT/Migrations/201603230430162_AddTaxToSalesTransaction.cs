namespace PutraJayaNT.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTaxToSalesTransaction : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SalesTransactions", "Tax", c => c.Decimal(nullable: false, precision: 50, scale: 30));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SalesTransactions", "Tax");
        }
    }
}
