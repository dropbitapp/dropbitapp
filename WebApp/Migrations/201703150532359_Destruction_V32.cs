namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Destruction_V32 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Destruction_V3", "RepresentativeName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Destruction_V3", "RepresentativeName");
        }
    }
}
