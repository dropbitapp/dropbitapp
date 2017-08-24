namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class creatingFillTestTableUpdate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FillTest",
                c => new
                    {
                        ProductionID = c.Int(nullable: false),
                        FillTestID = c.Int(nullable: false, identity: true),
                        ProofGallons = c.Single(nullable: false),
                        FillTestDate = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.FillTestID)
                .ForeignKey("dbo.Production", t => t.ProductionID, cascadeDelete: true)
                .Index(t => t.ProductionID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FillTest", "ProductionID", "dbo.Production");
            DropIndex("dbo.FillTest", new[] { "ProductionID" });
            DropTable("dbo.FillTest");
        }
    }
}
