using System;
using System.Collections.Generic;
using System.Linq;
using WebApp.Helpers;
using WebApp.Models;
using WebApp.ReportDTO;
using WebApp.Persistence.BusinessLogicEnums;

namespace WebApp.Persistence.Repositories
{
    public class ReportRepository
    {
        private readonly DistilDBContext _context;
        private readonly DataLayer _dl;

        public ReportRepository(DistilDBContext context, DataLayer dl)
        {
            _context = context;
            _dl = dl;
        }

        /// <summary>
        /// Method queries for a set of reporting records from persistent report table
        /// </summary>
        /// <param name="startOfReporting"></param>
        /// <param name="endOfReporting"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public ReportData GetReportData(DateTime endOfReporting, int userId, PersistReportType reportType)
        {
            ReportData reportData = GetPersistentStorageReportData(endOfReporting, userId);
            return reportData;
        }

        private void GetDataToBeInsertedInPersistentTableFormat(RecordType recordType, int userId, PurchaseObject purchaseData = null, ProductionObject productionData = null)
        {
            //update storage report
            if (purchaseData != null)
            {
                try
                {
                    // update line 2
                    UpdateStorageReportCellValue(purchaseData, userId, (int)PersistReportRow.DepositedInBulkStorage);
                    // update line 2 row total
                    UpdateStorageReportCellValue(purchaseData, userId, (int)PersistReportRow.DepositedInBulkStorage, (int)PersistReportColumn.Total);
                    // update line 6
                    UpdateStorageReportCellValue(purchaseData, userId, (int)PersistReportRow.TotalLines_1_Through_5);
                    // update line 6 row total
                    UpdateStorageReportCellValue(purchaseData, userId, (int)PersistReportRow.TotalLines_1_Through_5, (int)PersistReportColumn.Total);
                    // update line 23 (= line 6 - line 17 - line 18)

                    // update line 23 (= line 6 - line 17 - line 18) row total

                    // update line 24
                    UpdateStorageReportCellValue(purchaseData, userId, (int)PersistReportRow.TotalLines_7_Through_23);
                    // update line 24 row total
                    UpdateStorageReportCellValue(purchaseData, userId, (int)PersistReportRow.TotalLines_7_Through_23, (int)PersistReportColumn.Total);
                    // update line 1 for next month
                    UpdateStorageReportCellValue(purchaseData, userId, (int)PersistReportRow.OnHandFirstOfMonth);
                    // update line 1 row total for next month
                    UpdateStorageReportCellValue(purchaseData, userId, (int)PersistReportRow.OnHandFirstOfMonth, (int)PersistReportColumn.Total);
                    CompleteDbTransaction();
                }
                catch (Exception e)
                {
                    throw e;
                }

            }
        }

        public bool UpdateReportDataDuringPurchase(PurchaseObject purchaseData, int userId)
        {
            bool retMthdExecResult = false;
            // call private method here that determines what reports should be updated
            try
            {
                GetDataToBeInsertedInPersistentTableFormat(RecordType.Purchase, userId, purchaseData: purchaseData);
                retMthdExecResult = true;
            }
            catch (Exception e)
            {
                retMthdExecResult = false;
                throw e;
            }

            return retMthdExecResult;
        }

        public bool UpdateReportDataDuringProduction(ProductionObject productionData, int userId)
        {
            // call private method here that determines what reports should be updated
            GetDataToBeInsertedInPersistentTableFormat(RecordType.Purchase, userId, productionData: productionData);

            throw new NotImplementedException();
        }


        private void UpdateStorageReportCellValue(PurchaseObject purchaseData, int userId, int reportRowId, int reportColId = 0 /*reportColId is optional*/)
        {
            int IdentifierId = (int)PersistReportType.Storage;
            int PartId = (int)PersistReportPart.Part1;
            int rowId = reportRowId;
            int colId = reportColId == 0 ? purchaseData.SpiritTypeReportingID : reportColId;
            if (rowId == 0 || colId == 0)
            {
                throw new ArgumentOutOfRangeException("Reporting row ID and column ID must be greater than 0");
            }
            // converting to PST
            DateTime purchDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(purchaseData.PurchaseDate, "Pacific Standard Time");
            // adding 1 month to update OnHandFirstOfMonth for next month
            DateTime nextMonth = purchDate.AddMonths(1);
            float proofGal = purchaseData.ProofGallon;

            if (purchaseData != null)
            {
                try
                {
                    // checking for existing row with these values
                    var reportRec =
                        (from rec in _context.PersistentReport
                         where (rec.IdentifierID == IdentifierId &&
                                rec.PartID == PartId &&
                                rec.RowID == rowId &&
                                rec.ColumnID == colId &&
                                rec.Date.Year == purchDate.Year &&
                                rec.Date.Month == purchDate.Month)
                         select rec).FirstOrDefault();

                    var firstOfMonthRec =
                                (from rec in _context.PersistentReport
                                 where (rec.IdentifierID == IdentifierId &&
                                 rec.PartID == PartId &&
                                 rec.RowID == rowId &&
                                 rec.ColumnID == colId &&
                                 rec.Date.Year == nextMonth.Year &&
                                 rec.Date.Month == nextMonth.Month)
                                 select rec).FirstOrDefault();

                    if (rowId == (int)PersistReportRow.OnHandFirstOfMonth && firstOfMonthRec != null)
                    {
                        firstOfMonthRec.Value += proofGal;
                    }
                    else if (reportRec == null)
                    {
                        PersistentReport cellValue = new PersistentReport();
                        cellValue.IdentifierID = IdentifierId;
                        cellValue.PartID = PartId;
                        cellValue.RowID = rowId;
                        cellValue.ColumnID = colId;
                        cellValue.Value = purchaseData.ProofGallon;
                        if (rowId == (int)PersistReportRow.OnHandFirstOfMonth)
                        {
                            cellValue.Date = nextMonth;
                        }
                        else
                        {
                            cellValue.Date = purchDate;
                        }
                        cellValue.DistillerID = _dl.GetDistillerId(userId);

                        _context.PersistentReport.Add(cellValue);
                    }
                    else
                    {
                        reportRec.Value += proofGal;
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        private void CompleteDbTransaction()
        {
            _context.SaveChanges();
        }


        #region Data Retrieval

        /// <summary>
        /// Method queries for a set of storage reporting records from persistent report table
        /// </summary>
        /// <param name="startOfReporting"></param>
        /// <param name="endOfReporting"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public ReportData GetPersistentStorageReportData(DateTime endDate, int userId)
        {

            int identifierId = (int)PersistReportType.Storage;
            int partId = (int)PersistReportPart.Part1;
            int endYear = endDate.Year;
            int endMonth = endDate.Month;
            int distillerID = _dl.GetDistillerId(userId);
            ReportData reportData = new ReportData();

            try
            {
                var records =
                        (from storageRec in _context.PersistentReport
                         where (storageRec.DistillerID == distillerID &&
                         storageRec.IdentifierID == identifierId &&
                         storageRec.PartID == partId &&
                         storageRec.Date.Year == endYear &&
                         storageRec.Date.Month == endMonth)
                         select storageRec).DefaultIfEmpty();

                if (records != null)
                {
                    // fill header
                    reportData.Header = _dl.GetDistillerInfoForReportHeader(distillerID, endDate);

                    PersistRepType reportType = new PersistRepType();
                    PersistRepPart reportPart = new PersistRepPart();
                    List<PersistRepType> reportTypeList = new List<PersistRepType>();
                    List<PersistRepPart> reportPartList = new List<PersistRepPart>();
                    List<PersistRepColumn> reportColumnList = new List<PersistRepColumn>();

                    // these values are static for storage report
                    reportType.ReportTypeId = identifierId;
                    reportPart.PartId = partId;

                    foreach (var item in records)
                    {
                        PersistRepColumn reportColumn = new PersistRepColumn();
                        PersistRepRow reportRow = new PersistRepRow();
                        List<PersistRepRow> reportRowList = new List<PersistRepRow>();

                        var match = reportColumnList.Find(x => x.ColumnId == item.ColumnID);
                        if (match == null)
                        {
                            // fill reportRow object
                            reportRow.RowId = item.RowID;
                            reportRow.Value = item.Value;

                            // fill reportRowList with reportRow objects
                            reportRowList.Add(reportRow);

                            // fill reportColumn object 
                            reportColumn.ColumnId = item.ColumnID;
                            reportColumn.RowSpaceList = reportRowList;

                            // fill reportColumnList with reportColumn objects
                            reportColumnList.Add(reportColumn);
                        }
                        else
                        {
                            // fill reportRow object
                            reportRow.RowId = item.RowID;
                            reportRow.Value = item.Value;

                            // add to existing columns row list
                            match.RowSpaceList.Add(reportRow);
                        }
                    }
                    // fill reportPart object
                    reportPart.ColumnSpaceList = reportColumnList;
                    // fill reportPartList with reportPart objects
                    reportPartList.Add(reportPart);
                    // fill reportType object
                    reportType.ReportPartList = reportPartList;
                    // fill reportTypeList with reportType objects
                    reportTypeList.Add(reportType);
                    // fill reportData object
                    reportData.ReportTypeList = reportTypeList;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        return reportData;
        }

    #endregion
    }
}