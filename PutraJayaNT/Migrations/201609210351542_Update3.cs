namespace ECRP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update3 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Ledger_Account_Groups",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.ID);
            
            AddColumn("dbo.Ledger_Accounts", "LedgerAccountGroup_ID", c => c.Int());
            CreateIndex("dbo.Ledger_Accounts", "LedgerAccountGroup_ID");
            AddForeignKey("dbo.Ledger_Accounts", "LedgerAccountGroup_ID", "dbo.Ledger_Account_Groups", "ID");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Ledger_Accounts", "LedgerAccountGroup_ID", "dbo.Ledger_Account_Groups");
            DropIndex("dbo.Ledger_Accounts", new[] { "LedgerAccountGroup_ID" });
            DropColumn("dbo.Ledger_Accounts", "LedgerAccountGroup_ID");
            DropTable("dbo.Ledger_Account_Groups");
        }
    }
}
