namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Destruction_V3 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Destruction_V3", "WorkflowType", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Destruction_V3", "WorkflowType");
        }
    }
}
