namespace PUJASM.ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DueDateNoLongerOptional : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.SalesTransactions", "DueDate", c => c.DateTime(nullable: false, precision: 0));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.SalesTransactions", "DueDate", c => c.DateTime(precision: 0));
        }
    }
}
