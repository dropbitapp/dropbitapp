namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RecordItemToUnitOfMeasurementIdFix1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RecordItemToUnitOfMeasurement", "UnitOfMeasurementId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.RecordItemToUnitOfMeasurement", "UnitOfMeasurementId");
        }
    }
}
