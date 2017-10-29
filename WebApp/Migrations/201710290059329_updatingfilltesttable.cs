namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatingfilltesttable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FillTest", "AlcoholContent", c => c.Single(nullable: false));
            AddColumn("dbo.FillTest", "FillVariation", c => c.Single(nullable: false));
            AddColumn("dbo.FillTest", "CorrectiveAction", c => c.String());
            DropColumn("dbo.FillTest", "ProofGallons");
        }
        
        public override void Down()
        {
            AddColumn("dbo.FillTest", "ProofGallons", c => c.Single(nullable: false));
            DropColumn("dbo.FillTest", "CorrectiveAction");
            DropColumn("dbo.FillTest", "FillVariation");
            DropColumn("dbo.FillTest", "AlcoholContent");
        }
    }
}
