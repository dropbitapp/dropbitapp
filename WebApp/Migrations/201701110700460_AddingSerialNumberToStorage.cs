namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddingSerialNumberToStorage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Storage", "SerialNumber", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Storage", "SerialNumber");
        }
    }
}
