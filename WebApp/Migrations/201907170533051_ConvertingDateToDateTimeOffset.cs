namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ConvertingDateToDateTimeOffset : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Production", "ProductionDateOffset", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.Production", "ProductionStartTimeOffset", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.Production", "ProductionEndTimeOffset", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.BlendedComponentHist", "UpdateDateOffset", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.BottlingInfoHist", "UpdateDateOffset", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.Destruction", "StartTimeOffset", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.Destruction", "EndTimeOffset", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.DistillerDetail", "TimeZoneOffset", c => c.Int(nullable: false));
            AddColumn("dbo.FillTest", "FillTestDateOffset", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.GainLoss", "DateRecordedOffset", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.PersistentReport", "DateOffset", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.ProductionHist", "UpdateDateOffset", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.ProductionHistory", "UpdateDateOffset", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.ProductionHistory", "ProductionStartTimeOffset", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.ProductionHistory", "ProductionEndTimeOffset", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.ProductionHistory", "WithdrawalDateOffset", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.Purchase", "PurchaseDateOffset", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.PurchaseHist", "UpdateDateOffset", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.PurchaseHistory", "PurchaseDateOffset", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.PurchaseHistory", "UpdateDateOffset", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.TaxWithdrawn", "DateOfSaleOffset", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.TaxWithdrawn", "DateRecordedOffset", c => c.DateTimeOffset(nullable: false, precision: 7));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TaxWithdrawn", "DateRecordedOffset");
            DropColumn("dbo.TaxWithdrawn", "DateOfSaleOffset");
            DropColumn("dbo.PurchaseHistory", "UpdateDateOffset");
            DropColumn("dbo.PurchaseHistory", "PurchaseDateOffset");
            DropColumn("dbo.PurchaseHist", "UpdateDateOffset");
            DropColumn("dbo.Purchase", "PurchaseDateOffset");
            DropColumn("dbo.ProductionHistory", "WithdrawalDateOffset");
            DropColumn("dbo.ProductionHistory", "ProductionEndTimeOffset");
            DropColumn("dbo.ProductionHistory", "ProductionStartTimeOffset");
            DropColumn("dbo.ProductionHistory", "UpdateDateOffset");
            DropColumn("dbo.ProductionHist", "UpdateDateOffset");
            DropColumn("dbo.PersistentReport", "DateOffset");
            DropColumn("dbo.GainLoss", "DateRecordedOffset");
            DropColumn("dbo.FillTest", "FillTestDateOffset");
            DropColumn("dbo.DistillerDetail", "TimeZoneOffset");
            DropColumn("dbo.Destruction", "EndTimeOffset");
            DropColumn("dbo.Destruction", "StartTimeOffset");
            DropColumn("dbo.BottlingInfoHist", "UpdateDateOffset");
            DropColumn("dbo.BlendedComponentHist", "UpdateDateOffset");
            DropColumn("dbo.Production", "ProductionEndTimeOffset");
            DropColumn("dbo.Production", "ProductionStartTimeOffset");
            DropColumn("dbo.Production", "ProductionDateOffset");
        }
    }
}
