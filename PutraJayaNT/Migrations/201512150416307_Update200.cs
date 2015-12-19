namespace PUJASM.ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update200 : DbMigration
    {
        public override void Up()
        {
            AddColumn("SalesTransactions", "DueDate", c => c.DateTime(precision: 0));
        }
        
        public override void Down()
        {
            DropColumn("SalesTransactions", "DueDate");
        }
    }
}
