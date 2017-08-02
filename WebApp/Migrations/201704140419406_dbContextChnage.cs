namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class dbContextChnage : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Production_v3", "DistillerID");
            AddColumn("dbo.Production_v3", "DistillerID", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Production_v3", "DistillerID");
        }
    }
}
