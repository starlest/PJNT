namespace PUJASM.ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update9 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SalesTransactions", "CollectionSalesman_ID", c => c.Int());
            CreateIndex("dbo.SalesTransactions", "CollectionSalesman_ID");
            AddForeignKey("dbo.SalesTransactions", "CollectionSalesman_ID", "dbo.Salesmen", "ID");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SalesTransactions", "CollectionSalesman_ID", "dbo.Salesmen");
            DropIndex("dbo.SalesTransactions", new[] { "CollectionSalesman_ID" });
            DropColumn("dbo.SalesTransactions", "CollectionSalesman_ID");
        }
    }
}
