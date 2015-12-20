namespace PUJASM.ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update0007 : DbMigration
    {
        public override void Up()
        {
            AddColumn("PurchaseReturnTransactionLines", "Total", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("PurchaseReturnTransactions", "GrossTotal", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("PurchaseReturnTransactions", "NetTotal", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("PurchaseReturnTransactions", "NetTotal");
            DropColumn("PurchaseReturnTransactions", "GrossTotal");
            DropColumn("PurchaseReturnTransactionLines", "Total");
        }
    }
}
