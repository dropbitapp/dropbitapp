namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProcessingReportTypesTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProcessingReportType",
                c => new
                    {
                        ProcessingReportTypeID = c.Int(nullable: false, identity: true),
                        ProcessingReportTypeName = c.String(),
                    })
                .PrimaryKey(t => t.ProcessingReportTypeID);
            
            AddColumn("dbo.Spirit", "ProcessingReportTypeID", c => c.Int(nullable: false));
            CreateIndex("dbo.Spirit", "ProcessingReportTypeID");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Spirit", new[] { "ProcessingReportTypeID" });
            DropColumn("dbo.Spirit", "ProcessingReportTypeID");
            DropTable("dbo.ProcessingReportType");
        }
    }
}
