namespace PUJASM.ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedUserToSalesTransaction : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SalesTransactions", "User_Username", c => c.String(maxLength: 128, storeType: "nvarchar"));
            CreateIndex("dbo.SalesTransactions", "User_Username");
            AddForeignKey("dbo.SalesTransactions", "User_Username", "dbo.Users", "Username");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SalesTransactions", "User_Username", "dbo.Users");
            DropIndex("dbo.SalesTransactions", new[] { "User_Username" });
            DropColumn("dbo.SalesTransactions", "User_Username");
        }
    }
}
