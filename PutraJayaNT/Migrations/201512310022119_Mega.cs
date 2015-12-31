namespace PUJASM.ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mega : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PurchaseReturnTransactions", "User_Username", c => c.String(maxLength: 128, storeType: "nvarchar"));
            CreateIndex("dbo.PurchaseReturnTransactions", "User_Username");
            AddForeignKey("dbo.PurchaseReturnTransactions", "User_Username", "dbo.Users", "Username");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PurchaseReturnTransactions", "User_Username", "dbo.Users");
            DropIndex("dbo.PurchaseReturnTransactions", new[] { "User_Username" });
            DropColumn("dbo.PurchaseReturnTransactions", "User_Username");
        }
    }
}
