namespace PUJASM.ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update23 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SalesTransactions", "Discount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SalesTransactions", "Discount");
        }
    }
}
