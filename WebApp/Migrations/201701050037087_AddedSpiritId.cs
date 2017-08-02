namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedSpiritId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Blending", "SpiritId", c => c.Int(nullable: false));
            AddColumn("dbo.Bottling", "SpiritId", c => c.Int(nullable: false));
            DropColumn("dbo.Bottling", "SpiritTypeId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Bottling", "SpiritTypeId", c => c.Int(nullable: false));
            DropColumn("dbo.Bottling", "SpiritId");
            DropColumn("dbo.Blending", "SpiritId");
        }
    }
}
