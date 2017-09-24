namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class recreatingTable : DbMigration
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
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ProcessingReportType");
        }
    }
}
