namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class creatingHistoryTables4PurchaseAndProduction : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProductionHistory",
                c => new
                    {
                        ProductionHistoryID = c.Int(nullable: false, identity: true),
                        ProductionID = c.Int(nullable: false),
                        UpdateDate = c.DateTime(nullable: false),
                        ProductionName = c.String(),
                        ProductionStartTime = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        ProductionEndTime = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        Volume = c.Single(nullable: false),
                        Weight = c.Single(nullable: false),
                        Alcohol = c.Single(nullable: false),
                        Proof = c.Single(nullable: false),
                        StatusID = c.Int(nullable: false),
                        StateID = c.Int(nullable: false),
                        Note = c.String(maxLength: 1024),
                        UserID = c.Int(nullable: false),
                        Gauged = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ProductionHistoryID);
            
            CreateTable(
                "dbo.PurchaseHistory",
                c => new
                    {
                        PurchaseHistoryID = c.Int(nullable: false, identity: true),
                        PurchaseID = c.Int(nullable: false),
                        PurchaseName = c.String(),
                        Price = c.Single(nullable: false),
                        VendorID = c.Int(nullable: false),
                        Volume = c.Single(nullable: false),
                        Weight = c.Single(nullable: false),
                        Alcohol = c.Single(nullable: false),
                        Proof = c.Single(nullable: false),
                        PurchaseDate = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        Note = c.String(maxLength: 1024),
                        StateID = c.Int(nullable: false),
                        StatusID = c.Int(nullable: false),
                        Gauged = c.Boolean(nullable: false),
                        UserID = c.Int(nullable: false),
                        UpdateDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.PurchaseHistoryID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.PurchaseHistory");
            DropTable("dbo.ProductionHistory");
        }
    }
}
