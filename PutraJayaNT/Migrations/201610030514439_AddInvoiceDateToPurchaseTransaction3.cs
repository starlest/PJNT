namespace ECRP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddInvoiceDateToPurchaseTransaction3 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.PurchaseTransactions", "InvoiceDate", c => c.DateTime(nullable: false, precision: 0));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.PurchaseTransactions", "InvoiceDate", c => c.DateTime(precision: 0));
        }
    }
}
