namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuantityToUnitFix : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuantityToUnit", "UnitOfMeasurementId", c => c.Int(nullable: false));
            DropColumn("dbo.QuantityToUnit", "MeasurementUnitId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.QuantityToUnit", "MeasurementUnitId", c => c.Int(nullable: false));
            DropColumn("dbo.QuantityToUnit", "UnitOfMeasurementId");
        }
    }
}
