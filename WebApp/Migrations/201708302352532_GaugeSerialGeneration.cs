namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GaugeSerialGeneration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GaugeSerial",
                c => new
                    {
                        GaugeSerialID = c.Int(nullable: false, identity: true),
                        Serial = c.String(nullable: false),
                        RecordID = c.Int(nullable: false),
                        RecordType = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.GaugeSerialID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.GaugeSerial");
        }
    }
}
