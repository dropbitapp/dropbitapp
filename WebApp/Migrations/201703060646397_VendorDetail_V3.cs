namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class VendorDetail_V3 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.VendorDetail_v3",
                c => new
                    {
                        VendorDetailID = c.Int(nullable: false, identity: true),
                        VendorID = c.Int(nullable: false),
                        StreetAddress = c.String(),
                        City = c.String(),
                        Zip = c.String(),
                        State = c.String(),
                        Phone = c.String(),
                        Email = c.String(),
                        Note = c.String(maxLength: 512),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.VendorDetailID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.VendorDetail_v3");
        }
    }
}
