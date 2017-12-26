using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp.Helpers;
using WebApp.Models;
using WebApp.Persistence.BusinessLogicEnums;

namespace WebApp.Repositories
{ 
    public class ReportRepository
    {
        private readonly ApplicationDbContext _context;
        private List<ReportDto> _reportDtoList = new List<ReportDto>(); 

        private void GetDataToBeInsertedInPersistentTableFormat(RecordType recordType, ref List<ReportDto> _reportDtoList, PurchaseObject purchaseData = null, ProductionObject productionData = null)
        {
            throw new NotImplementedException();
        }

        public ReportRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Method queries for a set of reporting records from persistent report table
        /// </summary>
        /// <param name="startOfReporting"></param>
        /// <param name="endOfReporting"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IEnumerable<ReportDto> GetReportData(DateTime startOfReporting, DateTime endOfReporting, int userId, ReportType reportType)
        {
            throw new NotImplementedException();
        }

        public bool UpdateReportDataDuringPurchase(PurchaseObject purchaseData)
        {
            // call private method here that determines what reports should be updated
            GetDataToBeInsertedInPersistentTableFormat(RecordType.Purchase, ref _reportDtoList, purchaseData: purchaseData);

            // run actual db update
            foreach (var i in _reportDtoList)
            {

            }
            throw new NotImplementedException();
        }

        public bool UpdateReportDataDuringProduction(ProductionObject productionData)
        {
            // call private method here that determines what reports should be updated
            GetDataToBeInsertedInPersistentTableFormat(RecordType.Purchase, ref _reportDtoList, productionData: productionData);

            // run actual db update
            foreach (var i in _reportDtoList)
            {

            }
            throw new NotImplementedException();
        }
    }
}