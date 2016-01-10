namespace PUJASM.ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update8 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SalesTransactions", "DOPrinted", c => c.Boolean(nullable: false));
            AddColumn("dbo.SalesTransactions", "InvoicePrinted", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SalesTransactions", "InvoicePrinted");
            DropColumn("dbo.SalesTransactions", "DOPrinted");
        }
    }
}
