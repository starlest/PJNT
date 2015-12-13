namespace PUJASM.ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update1 : DbMigration
    {
        public override void Up()
        {

        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Salesmen",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.ID);
            
            AddColumn("dbo.SalesTransactions", "Salesman_ID", c => c.Int());
            CreateIndex("dbo.SalesTransactions", "Salesman_ID");
            AddForeignKey("dbo.SalesTransactions", "Salesman_ID", "dbo.Salesmen", "ID");
        }
    }
}
