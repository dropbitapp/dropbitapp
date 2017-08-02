namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RecordItemToUnitOfMeasurementIdFix : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.RecordItemToUnitOfMeasurement", "UnitOfMeasurementId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.RecordItemToUnitOfMeasurement", "UnitOfMeasurementId", c => c.String(nullable: false));
        }
    }
}
