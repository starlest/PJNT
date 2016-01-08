namespace PUJASM.ERP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update2 : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("Ledger_Account_Balances");
            AddPrimaryKey("Ledger_Account_Balances", new[] { "ID", "PeriodYear" });
            AddForeignKey("Ledger_Account_Balances", "ID", "Ledger_Accounts", "ID", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropPrimaryKey("Ledger_Account_Balances");
            AddPrimaryKey("Ledger_Account_Balances", "ID");
            AddForeignKey("Ledger_Account_Balances", "ID", "Ledger_Accounts", "ID");
        }
    }
}
