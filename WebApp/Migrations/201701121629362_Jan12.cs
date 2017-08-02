namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Jan12 : DbMigration
    {
        public override void Up()
        {
            //DropPrimaryKey("dbo.Bottling");
            CreateTable(
                "dbo.BottleCapacity",
                c => new
                    {
                        BottleCapacityId = c.Int(nullable: false, identity: true),
                        RecordId = c.Int(nullable: false),
                        IdentifierId = c.Int(nullable: false),
                        BottleCapacityValue = c.Single(nullable: false),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.BottleCapacityId);
            
            CreateTable(
                "dbo.BottleQuantity",
                c => new
                    {
                        BottleQuantityId = c.Int(nullable: false, identity: true),
                        RecordId = c.Int(nullable: false),
                        IdentifierId = c.Int(nullable: false),
                        BottleQuantityValue = c.Int(nullable: false),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.BottleQuantityId);
            
            CreateTable(
                "dbo.CaseCapacity",
                c => new
                    {
                        CaseCapacityId = c.Int(nullable: false, identity: true),
                        RecordId = c.Int(nullable: false),
                        IdentifierId = c.Int(nullable: false),
                        CaseCapacityValue = c.Int(nullable: false),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.CaseCapacityId);
            
            CreateTable(
                "dbo.CaseQuantity",
                c => new
                    {
                        CaseQuantityId = c.Int(nullable: false, identity: true),
                        RecordId = c.Int(nullable: false),
                        IdentifierId = c.Int(nullable: false),
                        CaseQuantityValue = c.Single(nullable: false),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.CaseQuantityId);
            
            CreateTable(
                "dbo.DistillationToDistilled",
                c => new
                    {
                        DistillationToDistilledId = c.Int(nullable: false, identity: true),
                        DistillationId = c.Int(nullable: false),
                        DistilledId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.DistillationToDistilledId);
            
            CreateTable(
                "dbo.InvBlendable",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RecordId = c.Int(nullable: false),
                        DistillableOrigin = c.Int(nullable: false),
                        BatchName = c.String(),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.InvDistillable",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RecordId = c.Int(nullable: false),
                        DistillableOrigin = c.String(),
                        DistillMethod = c.String(),
                        BatchName = c.String(),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.RecordItemToUnitOfMeasurement",
                c => new
                    {
                        RecordItemToUnitOfMeasurementId = c.Int(nullable: false, identity: true),
                        RecordItemName = c.String(nullable: false),
                        RecordId = c.Int(nullable: false),
                        UnitOfMeasurementId = c.Int(nullable: false),
                        TableIdentifier = c.Int(nullable: false),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.RecordItemToUnitOfMeasurementId);
            
            CreateTable(
                "dbo.RecordItemToUnitOfMeasurementHist",
                c => new
                    {
                        RecordItemToUnitOfMeasurementHistId = c.Int(nullable: false, identity: true),
                        RecordItemToUnitOfMeasurementId = c.Int(nullable: false),
                        RecordId = c.Int(nullable: false),
                        UnitOfMeasurementId = c.Int(nullable: false),
                        TableIdentifier = c.Int(nullable: false),
                        ChangedDate = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.RecordItemToUnitOfMeasurementHistId);
            
            CreateTable(
                "dbo.Spirit",
                c => new
                    {
                        SpiritId = c.Int(nullable: false, identity: true),
                        SpiritName = c.String(nullable: false),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.SpiritId);
            
            AddColumn("dbo.Blending", "BatchName", c => c.String());
            AddColumn("dbo.Blending", "BlendedId", c => c.Int(nullable: false));
            AddColumn("dbo.Blending", "SpiritId", c => c.Int(nullable: false));
           // AddColumn("dbo.Bottling", "BottlingId", c => c.Int(nullable: false, identity: true));
            //AddColumn("dbo.Bottling", "BottledId", c => c.Int(nullable: false));
            //AddColumn("dbo.Bottling", "BatchName", c => c.String());
            //AddColumn("dbo.Bottling", "SpiritId", c => c.Int(nullable: false));
            AddColumn("dbo.Distillation", "BatchName", c => c.String());
            AddColumn("dbo.Distillation", "DistilledId", c => c.Int(nullable: false));
            AddColumn("dbo.Fermentation", "BatchName", c => c.String());
            AddColumn("dbo.Fermentation", "FermentedId", c => c.Int(nullable: false));
            AddColumn("dbo.Purchase", "PurBatchName", c => c.String());
            AddColumn("dbo.Status", "StatusShortName", c => c.String(nullable: false));
            AddColumn("dbo.Status", "StatusFullName", c => c.String(nullable: false));
            AddColumn("dbo.Storage", "SerialNumber", c => c.String());
            AddColumn("dbo.Storage", "IsFull", c => c.Boolean(nullable: false));
            AlterColumn("dbo.AlcoholContent", "AlcContValue", c => c.Single(nullable: false));
            AlterColumn("dbo.Note", "NoteValue", c => c.String());
            AlterColumn("dbo.ProofGallon", "ProofGallonValue", c => c.Single(nullable: false));
            //DropColumn("dbo.Bottling", "BlendingId");
            //AddPrimaryKey("dbo.Bottling", "BottlingId");
            DropColumn("dbo.Blended", "BlendingId");
            DropColumn("dbo.Bottled", "BottlingId");
            DropColumn("dbo.Status", "StatusName");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Status", "StatusName", c => c.String());
            //AddColumn("dbo.Bottling", "BlendingId", c => c.Int(nullable: false, identity: true));
            AddColumn("dbo.Bottled", "BottlingId", c => c.Int(nullable: false));
            AddColumn("dbo.Blended", "BlendingId", c => c.Int(nullable: false));
            //DropPrimaryKey("dbo.Bottling");
            AlterColumn("dbo.ProofGallon", "ProofGallonValue", c => c.String());
            AlterColumn("dbo.Note", "NoteValue", c => c.String(nullable: false));
            AlterColumn("dbo.AlcoholContent", "AlcContValue", c => c.String());
            DropColumn("dbo.Storage", "IsFull");
            DropColumn("dbo.Storage", "SerialNumber");
            DropColumn("dbo.Status", "StatusFullName");
            DropColumn("dbo.Status", "StatusShortName");
            DropColumn("dbo.Purchase", "PurBatchName");
            DropColumn("dbo.Fermentation", "FermentedId");
            DropColumn("dbo.Fermentation", "BatchName");
            DropColumn("dbo.Distillation", "DistilledId");
            DropColumn("dbo.Distillation", "BatchName");
            //DropColumn("dbo.Bottling", "SpiritId");
            //DropColumn("dbo.Bottling", "BatchName");
            //DropColumn("dbo.Bottling", "BottledId");
            //DropColumn("dbo.Bottling", "BottlingId");
            DropColumn("dbo.Blending", "SpiritId");
            DropColumn("dbo.Blending", "BlendedId");
            DropColumn("dbo.Blending", "BatchName");
            DropTable("dbo.Spirit");
            DropTable("dbo.RecordItemToUnitOfMeasurementHist");
            DropTable("dbo.RecordItemToUnitOfMeasurement");
            DropTable("dbo.InvDistillable");
            DropTable("dbo.InvBlendable");
            DropTable("dbo.DistillationToDistilled");
            DropTable("dbo.CaseQuantity");
            DropTable("dbo.CaseCapacity");
            DropTable("dbo.BottleQuantity");
            DropTable("dbo.BottleCapacity");
            //AddPrimaryKey("dbo.Bottling", "BlendingId");
        }
    }
}
