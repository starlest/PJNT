namespace PUJASM.ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TransactionsUser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Ledger_Transactions", "User_Username", c => c.String(maxLength: 128, storeType: "nvarchar"));
            CreateIndex("dbo.Ledger_Transactions", "User_Username");
            AddForeignKey("dbo.Ledger_Transactions", "User_Username", "dbo.Users", "Username");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Ledger_Transactions", "User_Username", "dbo.Users");
            DropIndex("dbo.Ledger_Transactions", new[] { "User_Username" });
            DropColumn("dbo.Ledger_Transactions", "User_Username");
        }
    }
}
