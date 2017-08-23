namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class recoveringMigrationsDoneInPreviousCommit : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProductionHistory", "BurningDownMethod", c => c.String());
            AddColumn("dbo.ProductionHistory", "UsedMats", c => c.String());
            AddColumn("dbo.ProductionHistory", "SpiritCutName", c => c.String());
            AddColumn("dbo.ProductionHistory", "BlendingAdditives", c => c.String());
            AddColumn("dbo.ProductionHistory", "BottlingInfo", c => c.String());
            AddColumn("dbo.ProductionHistory", "SpiritTypeReportingID", c => c.Int(nullable: false));
            AddColumn("dbo.ProductionHistory", "MaterialKindReportingID", c => c.Int(nullable: false));
            AddColumn("dbo.ProductionHistory", "TaxedProof", c => c.Single(nullable: false));
            AddColumn("dbo.ProductionHistory", "WithdrawalDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.PurchaseHistory", "RecordName", c => c.String());
            AddColumn("dbo.PurchaseHistory", "SpiritTypeReportingID", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PurchaseHistory", "SpiritTypeReportingID");
            DropColumn("dbo.PurchaseHistory", "RecordName");
            DropColumn("dbo.ProductionHistory", "WithdrawalDate");
            DropColumn("dbo.ProductionHistory", "TaxedProof");
            DropColumn("dbo.ProductionHistory", "MaterialKindReportingID");
            DropColumn("dbo.ProductionHistory", "SpiritTypeReportingID");
            DropColumn("dbo.ProductionHistory", "BottlingInfo");
            DropColumn("dbo.ProductionHistory", "BlendingAdditives");
            DropColumn("dbo.ProductionHistory", "SpiritCutName");
            DropColumn("dbo.ProductionHistory", "UsedMats");
            DropColumn("dbo.ProductionHistory", "BurningDownMethod");
        }
    }
}
