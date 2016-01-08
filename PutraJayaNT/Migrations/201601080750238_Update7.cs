namespace PUJASM.ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update7 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SalesCommissions", "Percentage", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropColumn("dbo.SalesCommissions", "CommissionPercentage");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SalesCommissions", "CommissionPercentage", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            DropColumn("dbo.SalesCommissions", "Percentage");
        }
    }
}
