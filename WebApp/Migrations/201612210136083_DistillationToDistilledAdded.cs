namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DistillationToDistilledAdded : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DistillationToDistilled",
                c => new
                    {
                        DistillationToDistilledId = c.Int(nullable: false, identity: true),
                        DistillationId = c.Int(nullable: false),
                        DistilledId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.DistillationToDistilledId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.DistillationToDistilled");
        }
    }
}
