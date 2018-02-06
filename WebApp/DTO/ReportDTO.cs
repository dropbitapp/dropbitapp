using System.Collections.Generic;
using WebApp.Helpers;

namespace WebApp.ReportDTO
{
    /// Objects used for transporting data back and forth back-end and front-end

    /// </summary>
    /// Class containing report data {header, type, part, column, row, value}
    /// </summary>
    public class ReportData
    {
        public ReportHeader Header { get; set; }
        public List<PersistRepType> ReportTypeList { get; set; }
    }
    /// <summary>
    /// PersistRepType class containing Report Type : {Storage, Production, Processing}
    /// </summary>
    public class PersistRepType
    {
        public int ReportTypeId { get; set; }
        public List<PersistRepPart> ReportPartList { get; set; }
    }
    /// <summary>
    /// PersistRepPart class containing Part information of the report
    /// </summary>
    public class PersistRepPart
    {
        public int PartId { get; set; }
        public List<PersistRepColumn> ColumnSpaceList { get; set; }
    }
    /// <summary>
    /// PersistRepColumn class containing Column data which is used in ReportData class
    /// </summary>
    public class PersistRepColumn
    {
        public int ColumnId { get; set; }
        public List<PersistRepRow> RowSpaceList { get; set; }
    }
    /// <summary>
    /// PersistRepRow class contains values for each row
    /// </summary>
    public class PersistRepRow
    {
        public int RowId { get; set; }
        public float Value { get; set; }
    }
}