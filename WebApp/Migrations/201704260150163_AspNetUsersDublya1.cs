namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AspNetUsersDublya1 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.AspNetUsersToDistillers_v3", "UserId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.AspNetUsersToDistillers_v3", "UserId", c => c.String(nullable: false));
        }
    }
}
