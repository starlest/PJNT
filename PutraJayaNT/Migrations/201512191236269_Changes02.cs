namespace PUJASM.ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Changes02 : DbMigration
    {
        public override void Up()
        {
            AddColumn("PurchaseTransactions", "Note", c => c.String(unicode: false));
        }
        
        public override void Down()
        {
            DropColumn("PurchaseTransactions", "Note");
        }
    }
}
