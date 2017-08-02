namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GainLossDistillerIdRemoval : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.GainLoss", "DistillerID", "dbo.Distiller");
            DropIndex("dbo.GainLoss", new[] { "DistillerID" });
            DropColumn("dbo.GainLoss", "DistillerID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.GainLoss", "DistillerID", c => c.Int(nullable: false));
            CreateIndex("dbo.GainLoss", "DistillerID");
            AddForeignKey("dbo.GainLoss", "DistillerID", "dbo.Distiller", "DistillerID", cascadeDelete: true);
        }
    }
}
