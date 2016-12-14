namespace ECRP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update21 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.SystemParameters", "Key", c => c.String(nullable: false, unicode: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.SystemParameters", "Key", c => c.String(unicode: false));
        }
    }
}
