namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Feb15 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Purchase4Reporting", "ProductionID", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Purchase4Reporting", "ProductionID");
        }
    }
}
