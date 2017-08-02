namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Destruction_V3 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Destruction_V3",
                c => new
                    {
                        DestructionID = c.Int(nullable: false, identity: true),
                        RecordID = c.Int(nullable: false),
                        BatchType = c.String(nullable: false),
                        StartTime = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        EndTime = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        DestructionMethod = c.String(nullable: false),
                        Withdrawn = c.Boolean(nullable: false),
                        Quantity = c.Single(nullable: false),
                        VolumeByWeight = c.Single(nullable: false),
                        AlcoholContent = c.Single(nullable: false),
                        ProofGallons = c.Single(nullable: false),
                        Note = c.String(maxLength: 512),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.DestructionID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Destruction_V3");
        }
    }
}
