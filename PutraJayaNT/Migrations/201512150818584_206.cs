namespace PUJASM.ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _206 : DbMigration
    {
        public override void Up()
        {
            AddColumn("SalesTransactions", "GrossTotal", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("SalesTransactions", "SalesExpense", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("SalesTransactions", "SalesExpense");
            DropColumn("SalesTransactions", "GrossTotal");
        }
    }
}
