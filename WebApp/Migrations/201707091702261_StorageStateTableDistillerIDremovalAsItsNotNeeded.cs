namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class StorageStateTableDistillerIDremovalAsItsNotNeeded : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.StorageState", "DistillerID", "dbo.Distiller");
            DropIndex("dbo.StorageState", new[] { "DistillerID" });
            DropColumn("dbo.StorageState", "DistillerID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.StorageState", "DistillerID", c => c.Int(nullable: false));
            CreateIndex("dbo.StorageState", "DistillerID");
            AddForeignKey("dbo.StorageState", "DistillerID", "dbo.Distiller", "DistillerID", cascadeDelete: true);
        }
    }
}
