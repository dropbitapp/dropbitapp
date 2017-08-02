namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatingPurchase2Production : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ProductionToPurchase", "ProductionID", "dbo.Production");
            DropForeignKey("dbo.ProductionToPurchase", "PurchaseID", "dbo.Purchase");
            DropIndex("dbo.ProductionToPurchase", new[] { "PurchaseID" });
            DropIndex("dbo.ProductionToPurchase", new[] { "ProductionID" });
        }
        
        public override void Down()
        {
            CreateIndex("dbo.ProductionToPurchase", "ProductionID");
            CreateIndex("dbo.ProductionToPurchase", "PurchaseID");
            AddForeignKey("dbo.ProductionToPurchase", "PurchaseID", "dbo.Purchase", "PurchaseID", cascadeDelete: true);
            AddForeignKey("dbo.ProductionToPurchase", "ProductionID", "dbo.Production", "ProductionID", cascadeDelete: true);
        }
    }
}
