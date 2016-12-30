namespace ECERP.Migrations.ERPContext
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update3 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Customers", "City");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Customers", "City", c => c.String(unicode: false));
        }
    }
}
