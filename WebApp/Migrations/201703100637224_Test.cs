namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Test : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Production_v3", "StatusID", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Production_v3", "StatusID");
        }
    }
}
