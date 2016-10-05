namespace ECRP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddInvoiceDateToPurchaseTransaction : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PurchaseTransactions", "InvoiceDate", c => c.DateTime(nullable: false, precision: 0));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PurchaseTransactions", "InvoiceDate");
        }
    }
}
