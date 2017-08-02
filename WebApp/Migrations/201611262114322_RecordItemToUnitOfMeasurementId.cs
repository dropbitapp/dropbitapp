namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RecordItemToUnitOfMeasurementId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RecordItemToUnitOfMeasurement", "UnitOfMeasurementId", c => c.String(nullable: false));
            DropColumn("dbo.RecordItemToUnitOfMeasurement", "UnitOfMeasurementAbbr");
        }
        
        public override void Down()
        {
            AddColumn("dbo.RecordItemToUnitOfMeasurement", "UnitOfMeasurementAbbr", c => c.String(nullable: false));
            DropColumn("dbo.RecordItemToUnitOfMeasurement", "UnitOfMeasurementId");
        }
    }
}
