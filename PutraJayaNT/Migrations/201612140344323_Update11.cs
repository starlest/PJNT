namespace ECRP.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update11 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SystemParameters",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Key = c.String(unicode: false),
                        Value = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.SystemParameters");
        }
    }
}
