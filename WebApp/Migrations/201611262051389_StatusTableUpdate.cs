namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class StatusTableUpdate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Status", "StatusShortName", c => c.String(nullable: false));
            AddColumn("dbo.Status", "StatusFullName", c => c.String(nullable: false));
            DropColumn("dbo.Status", "StatusName");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Status", "StatusName", c => c.String());
            DropColumn("dbo.Status", "StatusFullName");
            DropColumn("dbo.Status", "StatusShortName");
        }
    }
}
