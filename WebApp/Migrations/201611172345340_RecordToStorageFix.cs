namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RecordToStorageFix : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.RecordToStorage");
            DropPrimaryKey("dbo.RecordToStorageHistory");
            DropColumn("dbo.RecordToStorage", "RecToContId");
            DropColumn("dbo.RecordToStorageHistory", "RecToContHistId");
            DropColumn("dbo.RecordToStorageHistory", "RecordToContChangedDate");
            AddColumn("dbo.RecordToStorage", "RecordToStorageId", c => c.Int(nullable: false, identity: true));
            AddColumn("dbo.RecordToStorageHistory", "RecordToStorageHistoryId", c => c.Int(nullable: false, identity: true));
            AddColumn("dbo.RecordToStorageHistory", "RecordToStorageChangedDate", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
            AddPrimaryKey("dbo.RecordToStorage", "RecordToStorageId");
            AddPrimaryKey("dbo.RecordToStorageHistory", "RecordToStorageHistoryId");

        }
        
        public override void Down()
        {
            AddColumn("dbo.RecordToStorageHistory", "RecordToContChangedDate", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
            AddColumn("dbo.RecordToStorageHistory", "RecToContHistId", c => c.Int(nullable: false, identity: true));
            AddColumn("dbo.RecordToStorage", "RecToContId", c => c.Int(nullable: false, identity: true));
            DropPrimaryKey("dbo.RecordToStorageHistory");
            DropPrimaryKey("dbo.RecordToStorage");
            DropColumn("dbo.RecordToStorageHistory", "RecordToStorageChangedDate");
            DropColumn("dbo.RecordToStorageHistory", "RecordToStorageHistoryId");
            DropColumn("dbo.RecordToStorage", "RecordToStorageId");
            AddPrimaryKey("dbo.RecordToStorageHistory", "RecToContHistId");
            AddPrimaryKey("dbo.RecordToStorage", "RecToContId");
        }
    }
}
