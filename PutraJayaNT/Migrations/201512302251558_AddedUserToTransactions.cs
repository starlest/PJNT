namespace PUJASM.ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedUserToTransactions : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SalesReturnTransactions", "User_Username", c => c.String(maxLength: 128, storeType: "nvarchar"));
            AddColumn("dbo.PurchaseTransactions", "User_Username", c => c.String(maxLength: 128, storeType: "nvarchar"));
            CreateIndex("dbo.SalesReturnTransactions", "User_Username");
            CreateIndex("dbo.PurchaseTransactions", "User_Username");
            AddForeignKey("dbo.SalesReturnTransactions", "User_Username", "dbo.Users", "Username");
            AddForeignKey("dbo.PurchaseTransactions", "User_Username", "dbo.Users", "Username");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PurchaseTransactions", "User_Username", "dbo.Users");
            DropForeignKey("dbo.SalesReturnTransactions", "User_Username", "dbo.Users");
            DropIndex("dbo.PurchaseTransactions", new[] { "User_Username" });
            DropIndex("dbo.SalesReturnTransactions", new[] { "User_Username" });
            DropColumn("dbo.PurchaseTransactions", "User_Username");
            DropColumn("dbo.SalesReturnTransactions", "User_Username");
        }
    }
}
