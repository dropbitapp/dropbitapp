
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
        Fermented,
        Distilled,
        Blended,
        Bottled,
        Supply,
        Additive
    }
}