namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class gaugesWorkflowAndMaterialCategories : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MaterialDict2MaterialCategory",
                c => new
                    {
                        MaterialDictID = c.Int(nullable: false),
                        ProductionReportMaterialCategoryID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.MaterialDictID)
                .ForeignKey("dbo.MaterialDict", t => t.MaterialDictID)
                .ForeignKey("dbo.ProductionReportMaterialCategory", t => t.ProductionReportMaterialCategoryID, cascadeDelete: true)
                .Index(t => t.MaterialDictID)
                .Index(t => t.ProductionReportMaterialCategoryID);
            
            AddColumn("dbo.Production", "Gauged", c => c.Boolean(nullable: false));
            AddColumn("dbo.Purchase", "Gauged", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MaterialDict2MaterialCategory", "ProductionReportMaterialCategoryID", "dbo.ProductionReportMaterialCategory");
            DropForeignKey("dbo.MaterialDict2MaterialCategory", "MaterialDictID", "dbo.MaterialDict");
            DropIndex("dbo.MaterialDict2MaterialCategory", new[] { "ProductionReportMaterialCategoryID" });
            DropIndex("dbo.MaterialDict2MaterialCategory", new[] { "MaterialDictID" });
            DropColumn("dbo.Purchase", "Gauged");
            DropColumn("dbo.Production", "Gauged");
            DropTable("dbo.MaterialDict2MaterialCategory");
        }
    }
}
