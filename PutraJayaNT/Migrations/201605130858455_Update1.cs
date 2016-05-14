namespace PutraJayaNT.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TelegramBotNotifications",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        When = c.DateTime(nullable: false, precision: 0),
                        Message = c.String(unicode: false),
                        Sent = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.TelegramBotNotifications");
        }
    }
}
