namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class grisha : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Spirit", "ProcessingReportTypeID", "dbo.ProcessingReportType");
            DropIndex("dbo.Spirit", new[] { "ProcessingReportTypeID" });
            DropTable("dbo.ProcessingReportType");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.ProcessingReportType",
                c => new
                    {
                        ProcessingReportTypeID = c.Int(nullable: false, identity: true),
                        ProcessingReportTypeName = c.String(),
                    })
                .PrimaryKey(t => t.ProcessingReportTypeID);
            
            CreateIndex("dbo.Spirit", "ProcessingReportTypeID");
            AddForeignKey("dbo.Spirit", "ProcessingReportTypeID", "dbo.ProcessingReportType", "ProcessingReportTypeID", cascadeDelete: true);
        }
    }
}
