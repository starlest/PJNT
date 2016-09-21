namespace ECRP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update2 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Ledger_Accounts", "Class");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Ledger_Accounts", "Class", c => c.String(nullable: false, unicode: false));
        }
    }
}
