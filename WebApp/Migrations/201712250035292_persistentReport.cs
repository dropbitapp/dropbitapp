namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class persistentReport : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PersistentReport",
                c => new
                    {
                        PersistentReportID = c.Int(nullable: false, identity: true),
                        IdentifierID = c.Int(nullable: false),
                        PartID = c.Int(nullable: false),
                        RowID = c.Int(nullable: false),
                        ColumnID = c.Int(nullable: false),
                        Value = c.Single(nullable: false),
                        Date = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        DistillerID = c.Int(nullable: false),
                        Custom = c.String(),
                    })
                .PrimaryKey(t => t.PersistentReportID);
            
            CreateTable(
                "dbo.ReportColumn",
                c => new
                    {
                        ReportColumnID = c.Int(nullable: false, identity: true),
                        ColumnName = c.String(),
                        ColumnID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ReportColumnID);
            
            CreateTable(
                "dbo.ReportIdentifier",
                c => new
                    {
                        ReportIdentifierID = c.Int(nullable: false, identity: true),
                        ReportName = c.String(),
                        IdentifierID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ReportIdentifierID);
            
            CreateTable(
                "dbo.ReportPart",
                c => new
                    {
                        ReportPartID = c.Int(nullable: false, identity: true),
                        PartName = c.String(),
                        PartID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ReportPartID);
            
            CreateTable(
                "dbo.ReportRow",
                c => new
                    {
                        ReportRowID = c.Int(nullable: false, identity: true),
                        RowName = c.String(),
                        RowID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ReportRowID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ReportRow");
            DropTable("dbo.ReportPart");
            DropTable("dbo.ReportIdentifier");
            DropTable("dbo.ReportColumn");
            DropTable("dbo.PersistentReport");
        }
    }
}
