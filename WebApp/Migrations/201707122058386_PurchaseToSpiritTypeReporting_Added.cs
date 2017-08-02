namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PurchaseToSpiritTypeReporting_Added : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PurchaseToSpiritTypeReporting",
                c => new
                    {
                        PurchaseID = c.Int(nullable: false),
                        SpiritTypeReportingID = c.Int(nullable: false),
                        MaterialKindReportingID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.PurchaseID, t.SpiritTypeReportingID, t.MaterialKindReportingID })
                .ForeignKey("dbo.MaterialKindReporting", t => t.MaterialKindReportingID, cascadeDelete: true)
                .ForeignKey("dbo.Purchase", t => t.PurchaseID, cascadeDelete: true)
                .ForeignKey("dbo.SpiritTypeReporting", t => t.SpiritTypeReportingID, cascadeDelete: true)
                .Index(t => t.PurchaseID)
                .Index(t => t.SpiritTypeReportingID)
                .Index(t => t.MaterialKindReportingID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PurchaseToSpiritTypeReporting", "SpiritTypeReportingID", "dbo.SpiritTypeReporting");
            DropForeignKey("dbo.PurchaseToSpiritTypeReporting", "PurchaseID", "dbo.Purchase");
            DropForeignKey("dbo.PurchaseToSpiritTypeReporting", "MaterialKindReportingID", "dbo.MaterialKindReporting");
            DropIndex("dbo.PurchaseToSpiritTypeReporting", new[] { "MaterialKindReportingID" });
            DropIndex("dbo.PurchaseToSpiritTypeReporting", new[] { "SpiritTypeReportingID" });
            DropIndex("dbo.PurchaseToSpiritTypeReporting", new[] { "PurchaseID" });
            DropTable("dbo.PurchaseToSpiritTypeReporting");
        }
    }
}
