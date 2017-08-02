namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Authentication : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AspNetUsersToDistillers_v3",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false),
                        DistillerId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
            AddColumn("dbo.SpiritCut_v3", "DistillerId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SpiritCut_v3", "DistillerId");
            DropTable("dbo.AspNetUsersToDistillers_v3");
        }
    }
}
