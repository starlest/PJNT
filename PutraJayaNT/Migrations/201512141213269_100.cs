namespace PUJASM.ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _100 : DbMigration
    {
        public override void Up()
        {
            AddColumn("PurchaseTransactionLines", "SoldOrReturned", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("PurchaseTransactionLines", "SoldOrReturned");
        }
    }
}
