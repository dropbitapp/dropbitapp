namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GainLossFixes : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.GainLoss", "BlendedRecordId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.GainLoss", "BlendedRecordId", c => c.Int(nullable: false));
        }
    }
}
