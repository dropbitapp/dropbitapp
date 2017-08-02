namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedTableToDbContext : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DictionaryItemToUnitOfMeasurement",
                c => new
                    {
                        DictionaryItemToUnitOfMeasurementId = c.Int(nullable: false, identity: true),
                        DictItemName = c.String(nullable: false),
                        RecordId = c.Int(nullable: false),
                        UnitOfMeasurementId = c.Int(nullable: false),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.DictionaryItemToUnitOfMeasurementId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.DictionaryItemToUnitOfMeasurement");
        }
    }
}
