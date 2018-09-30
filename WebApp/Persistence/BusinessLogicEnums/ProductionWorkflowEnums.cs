
namespace WebApp.Persistence.BusinessLogicEnums
{
    public enum ProductinWorkflowType
    {
        Fermentation = 1,
        Distillation,
        Blending,
        Bottling
    }

    /// <summary>
    /// Enum represents values from ProductionReportMaterialCategory table.
    /// It is used to associate Purchase values with Production Report Part 6
    /// </summary>
    public enum ProductionReportMaterialCategory
    {
        Grain = 1,
        Fruit,
        Cane,
        Other
    }

    public enum Status
    {
        Active = 1,
        Processing,
        Processed,
        Deleted = 6,
        Archived = 7,
        Destroyed = 9
    }

    public enum State
    {
        Fermentable = 1,
        Fermented   = 2,
        Distilled   = 3,
        Blended     = 4,
        Bottled     = 5,
        Supply      = 6,
        Additive    = 7
    }

    // Purchase type should be replaced by States. Having Purchase types is redundant.
    // task tracking the work - Task 1416: [FEATURE] It seems that ProductionType, PurchaseType and State are all doing the same thing. Which is redundant. I propose to just keep State and get rid of Production/Purchase types
    public enum PurchaseType
    {
        Fermentable = 1,
        Fermented = 2,
        Distilled = 3,
        Supply = 4,
        Additive = 5
    }

    // Production type should be replaced by States. Having Production types is redundant.
    // task tracking the work - Task 1416: [FEATURE] It seems that ProductionType, PurchaseType and State are all doing the same thing. Which is redundant. I propose to just keep State and get rid of Production/Purchase types
    public enum ProductionType
    {
        Fermentation = 1,
        Distillation = 2,
        Blending = 3,
        Bottling = 4
    }

    public enum ContenField
    {
        PurFermentableVolume    = 1
        ,PurFermentableWeight   = 2
        ,PurFermentedVolume     = 3
        ,PurFermentedWeight     = 4
        ,ProdFermentedVolume    = 5
        ,ProdFermentedWeight    = 6
        ,PurAdditiveVolume      = 7
        ,PurAdditiveWeight      = 8
        ,PurDistilledVolume     = 9
        ,PurDistilledWeight     = 10
        ,ProdDistilledVolume    = 11
        ,ProdDistilledWeight    = 12
        ,ProdBlendedVolume      = 13
        ,ProdBlendedWeight      = 14
        ,PurFermentedAlcohol    = 15
        ,PurFermentedProofGal   = 16
        ,PurDistilledAlcohol    = 17
        ,PurDistilledProofGal   = 18
        ,ProdFermentedAlcohol   = 19
        ,ProdDistilledProofGal  = 20
        ,ProdBlendedAlcohol     = 21
        ,ProdBlendedProofGal    = 22
        ,ProdFermentedProofGal  = 23
        ,ProdDistilledAlcohol   = 24
    }

    public enum RecordType
    {
        Purchase = 1,
        Production
    }

    public enum UnitOfMeasurement
    {
        gal = 1
        ,lb = 2
        ,oz = 3
        ,kg = 4
        ,gm = 5
        ,lt = 6
        ,pc = 7
    }

    public enum ProcessingReportType
    {
        AlcoholAndNeutralSpiritsOtherThanVodka = 1
        ,BlendedStraightWhiskey
        ,BlendedWhiskeyWithNeutralSpirits
        ,BlendedWhiskeyWithLightWhiskey
        ,BlendedLightWhiskey
        ,AnyOtherBlendsOf100Whiskey
        ,ImportedWhiskeyScotch
        ,ImportedWhiskeyCanadian
        ,ImportedWhiskeyIrishAndOthers
        ,DomesticWhiskeyDistilledAt160AndUnder
        ,DomesticWhiskeyDistilledAtOver160
        ,BrandyDistilledAt170AndUnder
        ,BrandyDistilledAtOver170
        ,SpiritsRumPuertoRican
        ,SpiritsRumVirginIslands
        ,RumDomestic
        ,RumOtherImported
        ,Gin
        ,Vodka
        ,CordialsLiqueursAndSpecialties
        ,CocktailsAndMixedDrinks
        ,Tequila
    }
}