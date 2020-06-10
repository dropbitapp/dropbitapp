namespace WebApp.Migrations
{
    using Models;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<WebApp.Models.DistilDBContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(WebApp.Models.DistilDBContext context)
        {
            //  This method will be called after migrating to the latest version.

            // Pre-populate lookup tables
            context.SpiritTypeReporting.AddOrUpdate(
                i => i.ProductTypeName,
                new SpiritTypeReporting { ProductTypeName = "WhiskyUnder160", ProductTypeFullName = "WhiskyUnder160" },
                new SpiritTypeReporting { ProductTypeName = "WhiskyOver160", ProductTypeFullName = "WhiskyOver160" },
                new SpiritTypeReporting { ProductTypeName = "BrandyUnder170", ProductTypeFullName = "BrandyUnder170" },
                new SpiritTypeReporting { ProductTypeName = "BrandyOver170", ProductTypeFullName = "BrandyOver170" },
                new SpiritTypeReporting { ProductTypeName = "Rum", ProductTypeFullName = "Rum" },
                new SpiritTypeReporting { ProductTypeName = "Gin", ProductTypeFullName = "Gin" },
                new SpiritTypeReporting { ProductTypeName = "Vodka", ProductTypeFullName = "Vodka" },
                new SpiritTypeReporting { ProductTypeName = "AlcoholUnder190", ProductTypeFullName = "AlcoholUnder190" },
                new SpiritTypeReporting { ProductTypeName = "AlcoholOver190", ProductTypeFullName = "AlcoholOver190" },
                new SpiritTypeReporting { ProductTypeName = "Other", ProductTypeFullName = "Other" },
                new SpiritTypeReporting { ProductTypeName = "Wine", ProductTypeFullName = "Wine" },
                new SpiritTypeReporting { ProductTypeName = "Total", ProductTypeFullName = "Total" }
                );

            context.ProductionReportMaterialCategory.AddOrUpdate(
                i => i.MaterialCategoryName,
                new ProductionReportMaterialCategory { MaterialCategoryName = "Grain" },
                new ProductionReportMaterialCategory { MaterialCategoryName = "Fruit" },
                new ProductionReportMaterialCategory { MaterialCategoryName = "Cane" },
                new ProductionReportMaterialCategory { MaterialCategoryName = "Other" }
                );

            context.State.AddOrUpdate(
                i => i.Name,
                new State { Name = "Fermentable" },
                new State { Name = "Fermented" },
                new State { Name = "Distilled" },
                new State { Name = "Blended" },
                new State { Name = "Bottled" },
                new State { Name = "Supply" },
                new State { Name = "Additive" }
                );

            // Some of the queries depend on deterministic ids in Status table
            context.Status.AddOrUpdate(
                i => i.Name,
                new Status { Name = "Active" },
                new Status { Name = "Processing" },
                new Status { Name = "Processed" },
                new Status { Name = "Active" }, // To be deleted, needs to match existing Name to avoid duplicate insertion 
                new Status { Name = "Active" }, // To be deleted, needs to match existing Name to avoid duplicate insertion 
                new Status { Name = "Deleted" },
                new Status { Name = "Archived" },
                new Status { Name = "Active" }, // To be deleted, needs to match existing Name to avoid duplicate insertion 
                new Status { Name = "Destroyed" }
                );

            // Write out to the db before deleting StatusID 4, 5, 8
            context.SaveChanges();

            // Work around to setting INSERT_IDENTITY on/off
            context.Database.ExecuteSqlCommand("DELETE FROM dbo.Status WHERE StatusID IN (4, 5, 8)");

            context.PurchaseType.AddOrUpdate(
                i => i.Name,
                new PurchaseType { Name = "Fermentable" },
                new PurchaseType { Name = "Fermented" },
                new PurchaseType { Name = "Distilled" },
                new PurchaseType { Name = "Supply" },
                new PurchaseType { Name = "Additive" }
                );

            context.ProductionType.AddOrUpdate(
                i => i.Name,
                new ProductionType { Name = "Fermentation" },
                new ProductionType { Name = "Distillation" },
                new ProductionType { Name = "Blending" },
                new ProductionType { Name = "Bottling" }
                );

            context.ContentField.AddOrUpdate(
                i => i.ContentFieldName,        
                new ContentField { ContentFieldName = "PurFermentableVolume" },
                new ContentField { ContentFieldName = "PurFermentableWeight" },
                new ContentField { ContentFieldName = "PurFermentedVolume" },
                new ContentField { ContentFieldName = "PurFermentedWeight" },
                new ContentField { ContentFieldName = "ProdFermentedVolume" },
                new ContentField { ContentFieldName = "ProdFermentedWeight" },
                new ContentField { ContentFieldName = "PurAdditiveVolume" },
                new ContentField { ContentFieldName = "PurAdditiveWeight" },
                new ContentField { ContentFieldName = "PurDistilledVolume" },
                new ContentField { ContentFieldName = "PurDistilledWeight" },
                new ContentField { ContentFieldName = "ProdDistilledVolume" },
                new ContentField { ContentFieldName = "ProdDistilledWeight" },
                new ContentField { ContentFieldName = "ProdBlendedVolume" },
                new ContentField { ContentFieldName = "ProdBlendedWeight" },
                new ContentField { ContentFieldName = "PurFermentedAlcohol" },
                new ContentField { ContentFieldName = "PurFermentedProofGal" },
                new ContentField { ContentFieldName = "PurDistilledAlcohol" },
                new ContentField { ContentFieldName = "PurDistilledProofGal" },
                new ContentField { ContentFieldName = "ProdFermentedAlcohol" },
                new ContentField { ContentFieldName = "ProdDistilledProofGal" },
                new ContentField { ContentFieldName = "ProdBlendedAlcohol" },
                new ContentField { ContentFieldName = "ProdBlendedProofGal" },
                new ContentField { ContentFieldName = "ProdFermentedProofGal" },
                new ContentField { ContentFieldName = "ProdDistilledAlcohol" }
                );

            context.UnitOfMeasurement.AddOrUpdate(
                i => i.Name,
                new UnitOfMeasurement { Name = "gal" },
                new UnitOfMeasurement { Name = "lb" },
                new UnitOfMeasurement { Name = "oz" },
                new UnitOfMeasurement { Name = "kg" },
                new UnitOfMeasurement { Name = "gm" },
                new UnitOfMeasurement { Name = "lt" },
                new UnitOfMeasurement { Name = "pc" }
                );

            context.ProcessingReportType.AddOrUpdate(
                i => i.ProcessingReportTypeName,
                new ProcessingReportType { ProcessingReportTypeName = "AlcoholAndNeutralSpiritsOtherThanVodka" },
                new ProcessingReportType { ProcessingReportTypeName = "BlendedStraightWhiskey" },
                new ProcessingReportType { ProcessingReportTypeName = "BlendedWhiskeyWithNeutralSpirits" },
                new ProcessingReportType { ProcessingReportTypeName = "BlendedWhiskeyWithLightWhiskey" },
                new ProcessingReportType { ProcessingReportTypeName = "BlendedLightWhiskey" },
                new ProcessingReportType { ProcessingReportTypeName = "AnyOtherBlendsOf100Whiskey" },
                new ProcessingReportType { ProcessingReportTypeName = "ImportedWhiskeyScotch" },
                new ProcessingReportType { ProcessingReportTypeName = "ImportedWhiskeyCanadian" },
                new ProcessingReportType { ProcessingReportTypeName = "ImportedWhiskeyIrishAndOthers" },
                new ProcessingReportType { ProcessingReportTypeName = "DomesticWhiskeyDistilledAt160AndUnder" },
                new ProcessingReportType { ProcessingReportTypeName = "DomesticWhiskeyDistilledAtOver160" },
                new ProcessingReportType { ProcessingReportTypeName = "BrandyDistilledAt170AndUnder" },
                new ProcessingReportType { ProcessingReportTypeName = "BrandyDistilledAtOver170" },
                new ProcessingReportType { ProcessingReportTypeName = "SpiritsRumPuertoRican" },
                new ProcessingReportType { ProcessingReportTypeName = "SpiritsRumVirginIslands" },
                new ProcessingReportType { ProcessingReportTypeName = "RumDomestic" },
                new ProcessingReportType { ProcessingReportTypeName = "RumOtherImported" },
                new ProcessingReportType { ProcessingReportTypeName = "Gin" },
                new ProcessingReportType { ProcessingReportTypeName = "Vodka" },
                new ProcessingReportType { ProcessingReportTypeName = "CordialsLiqueursAndSpecialties" },
                new ProcessingReportType { ProcessingReportTypeName = "CocktailsAndMixedDrinks" },
                new ProcessingReportType { ProcessingReportTypeName = "Tequila" }
                );

            // Seed default distillery
            context.Distiller.AddOrUpdate(
                i => i.Name,
                new Distiller { Name = "Dropbit" }
                );

            context.SaveChanges();

            context.DistillerDetail.AddOrUpdate(
            i => i.EIN,
            new DistillerDetail
            {
                DistillerID = 1,
                EIN = "12-3456789",
                DSP = "DSP-WA-10000",
                StreetAddress = "123 Bourbon Street",
                City = "Seattle",
                Zip = "98101",
                State = "WA",
                Phone = "206-123-4567",
                Email = "admin@dropbit.io",
                TimeZoneOffset = -8,
                Note = "Distillery template"
            });

            context.SpiritCut.AddOrUpdate(
                i => i.Name,
                new SpiritCut { Name = "Head", DistillerId = 1 },
                new SpiritCut { Name = "Heart", DistillerId = 1 },
                new SpiritCut { Name = "Tail", DistillerId = 1 },
                new SpiritCut { Name = "Mixed", DistillerId = 1}
                );

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
        }
    }
}
