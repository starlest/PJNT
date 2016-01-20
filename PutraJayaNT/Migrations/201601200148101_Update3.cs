namespace PUJASM.ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update3 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PurchaseTransactions", "DOID", c => c.String(unicode: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PurchaseTransactions", "DOID");
        }
    }
}
