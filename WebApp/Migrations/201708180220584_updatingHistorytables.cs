namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatingHistorytables : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProductionHistory", "Status", c => c.String());
            AddColumn("dbo.ProductionHistory", "State", c => c.String());
            AddColumn("dbo.ProductionHistory", "Storage", c => c.String());
            AddColumn("dbo.PurchaseHistory", "Vendor", c => c.String());
            AddColumn("dbo.PurchaseHistory", "State", c => c.String());
            AddColumn("dbo.PurchaseHistory", "Status", c => c.String());
            AddColumn("dbo.PurchaseHistory", "Storage", c => c.String());
            AlterColumn("dbo.ProductionHistory", "UpdateDate", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
            AlterColumn("dbo.PurchaseHistory", "UpdateDate", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
            DropColumn("dbo.ProductionHistory", "StatusID");
            DropColumn("dbo.ProductionHistory", "StateID");
            DropColumn("dbo.PurchaseHistory", "VendorID");
            DropColumn("dbo.PurchaseHistory", "StateID");
            DropColumn("dbo.PurchaseHistory", "StatusID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PurchaseHistory", "StatusID", c => c.Int(nullable: false));
            AddColumn("dbo.PurchaseHistory", "StateID", c => c.Int(nullable: false));
            AddColumn("dbo.PurchaseHistory", "VendorID", c => c.Int(nullable: false));
            AddColumn("dbo.ProductionHistory", "StateID", c => c.Int(nullable: false));
            AddColumn("dbo.ProductionHistory", "StatusID", c => c.Int(nullable: false));
            AlterColumn("dbo.PurchaseHistory", "UpdateDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.ProductionHistory", "UpdateDate", c => c.DateTime(nullable: false));
            DropColumn("dbo.PurchaseHistory", "Storage");
            DropColumn("dbo.PurchaseHistory", "Status");
            DropColumn("dbo.PurchaseHistory", "State");
            DropColumn("dbo.PurchaseHistory", "Vendor");
            DropColumn("dbo.ProductionHistory", "Storage");
            DropColumn("dbo.ProductionHistory", "State");
            DropColumn("dbo.ProductionHistory", "Status");
        }
    }
}
