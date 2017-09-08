namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProductionHistoryTableFix : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ProductionHistory", "WithdrawalDate", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ProductionHistory", "WithdrawalDate", c => c.DateTime(nullable: false));
        }
    }
}
