namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DisableForeignKey : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ProductionToSpiritTypeReporting", "MaterialKindReportingID", "dbo.MaterialKindReporting");
            DropIndex("dbo.ProductionToSpiritTypeReporting", new[] { "MaterialKindReportingID" });
            DropPrimaryKey("dbo.ProductionToSpiritTypeReporting");
            AddPrimaryKey("dbo.ProductionToSpiritTypeReporting", new[] { "ProductionID", "SpiritTypeReportingID" });
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.ProductionToSpiritTypeReporting");
            AddPrimaryKey("dbo.ProductionToSpiritTypeReporting", new[] { "ProductionID", "SpiritTypeReportingID", "MaterialKindReportingID" });
            CreateIndex("dbo.ProductionToSpiritTypeReporting", "MaterialKindReportingID");
            AddForeignKey("dbo.ProductionToSpiritTypeReporting", "MaterialKindReportingID", "dbo.MaterialKindReporting", "MaterialKindReportingID", cascadeDelete: true);
        }
    }
}
