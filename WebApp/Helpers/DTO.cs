namespace WebApp.Helpers
{
    /// Objects used for transporting data back and forth back-end and front-end

    /// <summary>
    /// Used in Reporting workflows
    /// </summary>
    public class ReportDto
    {
        public int IdentifierID { get; set; }
        public int PartID { get; set; }
        public int RowID { get; set; }
        public int ColumnID { get; set; }
        public float Value { get; set; }
        public string Custom { get; set; }
    }
}