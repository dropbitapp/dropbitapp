
namespace WebApp.Persistence.BusinessLogicEnums
{
    public enum ProductinWorkflowType
    {
        Default = 0,
        Fermentation,
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
        Default =0,
        Grain,
        Fruit,
        Cane,
        Other
    }
}