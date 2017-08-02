namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class removingForeignKeyPurchaseSpirReport : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.PurchaseToSpiritTypeReporting", "MaterialKindReportingID", "dbo.MaterialKindReporting");
            DropIndex("dbo.PurchaseToSpiritTypeReporting", new[] { "MaterialKindReportingID" });
            DropPrimaryKey("dbo.PurchaseToSpiritTypeReporting");
            AddPrimaryKey("dbo.PurchaseToSpiritTypeReporting", new[] { "PurchaseID", "SpiritTypeReportingID" });
            DropColumn("dbo.PurchaseToSpiritTypeReporting", "MaterialKindReportingID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PurchaseToSpiritTypeReporting", "MaterialKindReportingID", c => c.Int(nullable: false));
            DropPrimaryKey("dbo.PurchaseToSpiritTypeReporting");
            AddPrimaryKey("dbo.PurchaseToSpiritTypeReporting", new[] { "PurchaseID", "SpiritTypeReportingID", "MaterialKindReportingID" });
            CreateIndex("dbo.PurchaseToSpiritTypeReporting", "MaterialKindReportingID");
            AddForeignKey("dbo.PurchaseToSpiritTypeReporting", "MaterialKindReportingID", "dbo.MaterialKindReporting", "MaterialKindReportingID", cascadeDelete: true);
        }
    }
}
