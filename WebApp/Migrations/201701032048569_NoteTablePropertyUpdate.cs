namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NoteTablePropertyUpdate : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Note", "NoteValue", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Note", "NoteValue", c => c.String(nullable: false));
        }
    }
}
