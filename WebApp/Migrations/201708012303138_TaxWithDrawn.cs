namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TaxWithDrawn : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TaxWithdrawn",
                c => new
                    {
                        ProductionID = c.Int(nullable: false),
                        TaxWithdrawnID = c.Int(nullable: false, identity: true),
                        Value = c.Single(nullable: false),
                        DateOfSale = c.DateTime(nullable: false),
                        DateRecorded = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.TaxWithdrawnID)
                .ForeignKey("dbo.Production", t => t.ProductionID, cascadeDelete: true)
                .Index(t => t.ProductionID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TaxWithdrawn", "ProductionID", "dbo.Production");
            DropIndex("dbo.TaxWithdrawn", new[] { "ProductionID" });
            DropTable("dbo.TaxWithdrawn");
        }
    }
}
