namespace PutraJayaNT.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update3 : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.SalesTransactions", new[] { "When" });
            AddColumn("dbo.SalesTransactions", "Date", c => c.DateTime(nullable: false, precision: 0));
            CreateIndex("dbo.SalesTransactions", "Date");
            DropColumn("dbo.SalesTransactions", "When");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SalesTransactions", "When", c => c.DateTime(nullable: false, precision: 0));
            DropIndex("dbo.SalesTransactions", new[] { "Date" });
            DropColumn("dbo.SalesTransactions", "Date");
            CreateIndex("dbo.SalesTransactions", "When");
        }
    }
}
