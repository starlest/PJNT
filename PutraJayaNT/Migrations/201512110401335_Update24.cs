namespace PUJASM.ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update24 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SalesTransactions", "Notes", c => c.String(unicode: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SalesTransactions", "Notes");
        }
    }
}
