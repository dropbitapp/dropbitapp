namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Destruction_V31 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Destruction_V3", "BatchType");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Destruction_V3", "BatchType", c => c.String(nullable: false));
        }
    }
}
