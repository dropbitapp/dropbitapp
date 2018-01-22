using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp.Helpers;
using WebApp.Models;
using WebApp.Persistence.BusinessLogicEnums;

namespace WebApp.Persistence.Repositories
{ 
    public class ReportRepository
    {
        private readonly DistilDBContext _context;
        private List<ReportDto> _reportDtoList = new List<ReportDto>();

        // <summary>
        /// GetDistillerID retrieves DistillerId for given UserId
        /// </summary>
        public int GetDistillerId(int userId)
        {
            int distillerId = (from rec in _context.AspNetUserToDistiller
                               where rec.UserId == userId
                               select rec.DistillerID).FirstOrDefault();
            return distillerId;
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

        public ReportRepository()
        {
           _context = new DistilDBContext();
        }

        /// <summary>
        /// Method queries for a set of reporting records from persistent report table
        /// </summary>
        /// <param name="startOfReporting"></param>
        /// <param name="endOfReporting"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IEnumerable<ReportDto> GetReportData(DateTime startOfReporting, DateTime endOfReporting, int userId, PersistReportType reportType)
        {
            throw new NotImplementedException();
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

            // run actual db update
            foreach (var i in _reportDtoList)
            {

            }
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

                    if (rowId == (int)PersistReportRow.OnHandFirstOfMonth && firstOfMonthRec != null) {
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
                        cellValue.DistillerID = GetDistillerId(userId);

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
    }
}