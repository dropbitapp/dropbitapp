namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatingGainLoss : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GainLoss", "ProductionId", c => c.Int(nullable: false));
            DropColumn("dbo.GainLoss", "BottledRecordId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.GainLoss", "BottledRecordId", c => c.Int(nullable: false));
            DropColumn("dbo.GainLoss", "ProductionId");
        }
    }
}
