namespace PUJASM.ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _205 : DbMigration
    {
        public override void Up()
        {
            AddColumn("Inventory", "SalesExpense", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("Inventory", "SalesExpense");
        }
    }
}
