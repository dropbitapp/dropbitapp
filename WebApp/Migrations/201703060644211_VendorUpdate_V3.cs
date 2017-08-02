namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class VendorUpdate_V3 : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.VendorDetail_v3");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.VendorDetail_v3",
                c => new
                    {
                        VendorID = c.Int(nullable: false, identity: true),
                        StreetAddress = c.String(),
                        City = c.String(),
                        Zip = c.String(),
                        State = c.String(),
                        Phone = c.String(),
                        Email = c.String(),
                        Note = c.String(maxLength: 512),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.VendorID);
            
        }
    }
}
