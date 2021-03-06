﻿using System;
using System.Collections.Generic;
using System.Linq;
using WebApp.Helpers;
using WebApp.Models;
using WebApp.Persistence.BusinessLogicEnums;

namespace WebApp.Reports
{
    public class ProductionReport
    {
        private readonly DistilDBContext _db;
        private readonly DataLayer _dl;

        // integer month representation
        private const int january = 1;
        private const int february = 2;
        private const int march = 3;
        private const int april = 4;
        private const int may = 5;
        private const int june = 6;
        private const int july = 7;
        private const int august = 8;
        private const int september = 9;
        private const int october = 10;
        private const int november = 11;
        private const int december = 12;

        public ProductionReport(DistilDBContext db, DataLayer dl)
        {
            _db = db;
            _dl = dl;
        }

        /// <summary>
        /// GetProductionReportData methods pull production report data for an user in a given month
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public ProductionReportingObject GetProductionReportData(DateTimeOffset start, DateTimeOffset end, int userId)
        {
            ProductionReportingObject prodRepObj = new ProductionReportingObject();
            List<ProdReportPart1> part1List = new List<ProdReportPart1>();
            List<ProdReportParts2Through4> part2Thru4List = new List<ProdReportParts2Through4>();
            List<ProdReportPart5> part5 = new List<ProdReportPart5>();
            List<ProdReportPart6> prodReportPart6List = new List<ProdReportPart6>();
            List<ProductionReportHelper> tempRepObjList = new List<ProductionReportHelper>();

            // get distiller information for header report
            int distillerID = _dl.GetDistillerId(userId);

            prodRepObj.Header = _dl.GetDistillerInfoForReportHeader(distillerID, start);

            // get initial set of records produced in a given reporting period. It is used in 
            // mutliple methods
            GetAllProductionReportRecordsForGivenPeriod(userId, start, end, ref tempRepObjList);

            // get Part 1 of the Production Report
            GetProductionReportPart1Records(start, end, ref part1List, tempRepObjList);

            // we need this for line 17(b) of Part 1
            // we only need to get Unfinished Spirit during the quartely returns so doing the check here. Months could change from year to year. Need to check with TTB every year
            if (start.Month == january && end.Month == january || start.Month == april && end.Month == april || start.Month == july && end.Month == july || start.Month == october && end.Month == october)
            {
                GetUnfinishedSpiritsForProductionReport(userId, start, end, ref tempRepObjList);
            }

            // get data for line 15 of Part 1
            GetReceivedForRedistillationForLine15(userId, start, end, ref part1List, ref part5);

            // parts 2 through 4
            GetRecordsForParts2Through4(ref part1List, ref part2Thru4List, tempRepObjList);

            // get part 6 data
            ProductionReportPart6Data(ref prodReportPart6List, ref tempRepObjList);

            prodRepObj.Part1List = part1List;
            prodRepObj.Part2Through4List = part2Thru4List;
            prodRepObj.part5List = part5;
            prodRepObj.ProdReportPart6List = prodReportPart6List;

            CalcuateTotals(ref prodRepObj);

            return prodRepObj;
        }

        /// <summary>
        /// Computes totals for each row in Production report
        /// TODO: 
        /// Technical debt: 
        /// 1. Factor out CalculateTotals methods from Production and Storage Reports. This can be fixed during another stage of OOP implementation on Reporting workflows
        /// 2. We need to add ennums for SpiritTypeCategory as there is only 9 entries there so we can avoid quering the db every time we Calculate Totals
        /// </summary>
        /// <param name="prodRepObj"></param>
        private void CalcuateTotals(ref ProductionReportingObject prodRepObj)
        {
            if (prodRepObj.Part1List.Count > 0)
            {
                try
                {
                    // Get total column in the SpiritTypeReporting table
                    var spiritTypeCategory = (from type in _db.SpiritTypeReporting
                                              where type.SpiritTypeReportingID == (int)ReportSpiritTypes.Total
                                              select new
                                              {
                                                  type.SpiritTypeReportingID,
                                                  type.ProductTypeName
                                              }).FirstOrDefault();

                    if (spiritTypeCategory != null)
                    {
                        ProdReportPart1 part1 = new ProdReportPart1();

                        foreach (var i in prodRepObj.Part1List)
                        {
                            part1.SpiritTypeReportingID = spiritTypeCategory.SpiritTypeReportingID;
                            part1.SpiritCatName = spiritTypeCategory.ProductTypeName;
                            part1.ProccessingAcct += i.ProccessingAcct;
                            part1.ProducedTotal += i.ProducedTotal;
                            part1.Recd4RedistilaltionL15 += i.Recd4RedistilaltionL15;
                            part1.Recd4RedistilL17 += i.Recd4RedistilL17;
                            part1.StorageAcct += i.StorageAcct;
                            part1.UnfinishedSpiritsEndOfQuarterL17 += i.UnfinishedSpiritsEndOfQuarterL17;
                        }

                        // Round totals
                        part1.ProccessingAcct = (float)Math.Round(part1.ProccessingAcct, 3);
                        part1.ProducedTotal = (float)Math.Round(part1.ProducedTotal, 3);
                        part1.Recd4RedistilaltionL15 = (float)Math.Round(part1.Recd4RedistilaltionL15, 3);
                        part1.Recd4RedistilL17 = (float)Math.Round(part1.Recd4RedistilL17, 3);
                        part1.StorageAcct = (float)Math.Round(part1.StorageAcct, 3);
                        part1.UnfinishedSpiritsEndOfQuarterL17 = (float)Math.Round(part1.UnfinishedSpiritsEndOfQuarterL17, 3);

                        // Add total category to existing report body
                        prodRepObj.Part1List.Add(part1);
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        /// <summary>
        /// GetAllProductionReportRecordsForGivenPeriod method gets the data for production report part 1
        /// </summary>
        /// <param name="userId">Distiller user id</param>
        /// <param name="start">reporting start period</param>
        /// <param name="end">reporting end period</param>
        /// <param name="tempRepObjList">out parameter object list that needs to be populated</param>
        private void GetAllProductionReportRecordsForGivenPeriod(int userId, DateTimeOffset start, DateTimeOffset end, ref List<ProductionReportHelper> tempRepObjList)
        {
            try
            {
                var ress =
                (from prod in _db.Production
                 join distillers in _db.AspNetUserToDistiller on prod.DistillerID equals distillers.DistillerID into distillers_join
                 from distillers in distillers_join.DefaultIfEmpty()
                 join prod2SpiritType in _db.ProductionToSpiritTypeReporting on prod.ProductionID equals prod2SpiritType.ProductionID into prod2SpiritType_join
                 from prod2SpiritType in prod2SpiritType_join.DefaultIfEmpty()
                 join matKindRep in _db.MaterialKindReporting on prod2SpiritType.MaterialKindReportingID equals matKindRep.MaterialKindReportingID into matKindRep_join
                 from matKindRep in matKindRep_join.DefaultIfEmpty()
                 join spiritTypeRep in _db.SpiritTypeReporting on prod2SpiritType.SpiritTypeReportingID equals spiritTypeRep.SpiritTypeReportingID into spiritTypeRep_join
                 from spiritTypeRep in spiritTypeRep_join.DefaultIfEmpty()
                 join prodRepMatCat2MatKind in _db.ProdRepMatCat2MaterialKind on matKindRep.MaterialKindReportingID equals prodRepMatCat2MatKind.MaterialKindReportingID into prodRepMatCat2MatKind_join
                 from prodRepMatCat2MatKind in prodRepMatCat2MatKind_join.DefaultIfEmpty()
                 join prodRepMatCat in _db.ProductionReportMaterialCategory on prodRepMatCat2MatKind.ProductionReportMaterialCategoryID equals prodRepMatCat.ProductionReportMaterialCategoryID into prodRepMatCat_join
                 from prodRepMatCat in prodRepMatCat_join.DefaultIfEmpty()
                 where
                   distillers.UserId == userId &&
                   prod.Gauged == true &&
                   (new int[] { (int)Persistence.BusinessLogicEnums.State.Distilled }).Contains(prod.StateID) &&
                   prod.ProductionEndTimeOffset >= start &&
                   prod.ProductionEndTimeOffset <= end
                 select new
                 {
                     prod.StateID,
                     ProductionID = (int?)prod.ProductionID,
                     SpiritTypeName = spiritTypeRep.ProductTypeName ?? string.Empty,
                     MaterialKindName = matKindRep.MaterialKindName ?? string.Empty,
                     SpiritTypeReportingID = (int?)spiritTypeRep.SpiritTypeReportingID ?? (int?)0,
                     MaterialKindReportingID = (int?)matKindRep.MaterialKindReportingID ?? (int?)0,
                     MaterialCategoryName = prodRepMatCat.MaterialCategoryName ?? string.Empty,
                     ProductionReportMaterialCategoryID = (int?)prodRepMatCat.ProductionReportMaterialCategoryID ?? (int?)0,
                     Gauged = (bool?)prod.Gauged ?? false
                 }).Distinct();

                // fill in temporary query results object so we can massage the data for further processing
                foreach (var l in ress)
                {
                    ProductionReportHelper tempRepObj = new ProductionReportHelper();
                    tempRepObj.StateID = l.StateID;
                    tempRepObj.ProductionID = (int)l.ProductionID;
                    tempRepObj.SpiritTypeReportName = l.SpiritTypeName;
                    tempRepObj.MaterialKindReportingName = l.MaterialKindName;
                    tempRepObj.SpiritTypeReportingID = (int)l.SpiritTypeReportingID;
                    tempRepObj.MaterialKindReportingID = (int)l.MaterialKindReportingID;
                    tempRepObj.Gauged = l.Gauged;

                    tempRepObjList.Add(tempRepObj);
                }

                if (tempRepObjList != null)
                {
                    // first, we want to make sure that every record received from the _db, has SpiritType and Material Kinds associated with it
                    // because there are cases when Blending and Bottling may not have SpiritType in a reporting sense. Our current design allows it
                    // but maybe worth changing it
                    foreach (var i in tempRepObjList)
                    {
                        if (i.SpiritTypeReportingID == 0)
                        {
                            var k =
                                    (from prodContent in _db.ProductionContent
                                     join prod2SpiritType in _db.ProductionToSpiritTypeReporting on prodContent.RecordID equals prod2SpiritType.ProductionID into prod2SpiritType_join
                                     from prod2SpiritType in prod2SpiritType_join.DefaultIfEmpty()
                                     join matKindRep in _db.MaterialKindReporting on prod2SpiritType.MaterialKindReportingID equals matKindRep.MaterialKindReportingID into matKindRep_join
                                     from matKindRep in matKindRep_join.DefaultIfEmpty()
                                     join spiritTRep in _db.SpiritTypeReporting on prod2SpiritType.SpiritTypeReportingID equals spiritTRep.SpiritTypeReportingID into spiritTRep_join
                                     from spiritTRep in spiritTRep_join.DefaultIfEmpty()
                                     where
                                         prodContent.isProductionComponent == true &&
                                         prodContent.ProductionID == i.ProductionID
                                     select new
                                     {
                                         RecordID = (int?)prodContent.RecordID,
                                         SpiritTypeReportingID = (int?)spiritTRep.SpiritTypeReportingID ?? (int?)0,
                                         SpiritTypeReportingName = spiritTRep.ProductTypeName ?? String.Empty,
                                         MaterialKindReportingID = (int?)matKindRep.MaterialKindReportingID ?? (int?)0,
                                         MaterialKindReportingName = matKindRep.MaterialKindName ?? String.Empty
                                     }).FirstOrDefault();
                            if (k != null)
                            {
                                if ((int)k.SpiritTypeReportingID == 0)
                                {
                                    bool spiritTypeFound = false;
                                    var tempRecordID = k.RecordID;
                                    while (!spiritTypeFound)
                                    {
                                        var t =
                                            (from prodContent in _db.ProductionContent
                                             join prod2SpiritType in _db.ProductionToSpiritTypeReporting on prodContent.RecordID equals prod2SpiritType.ProductionID into prod2SpiritType_join
                                             from prod2SpiritType in prod2SpiritType_join.DefaultIfEmpty()
                                             join matKindRep in _db.MaterialKindReporting on prod2SpiritType.MaterialKindReportingID equals matKindRep.MaterialKindReportingID into matKindRep_join
                                             from matKindRep in matKindRep_join.DefaultIfEmpty()
                                             join spiritTRep in _db.SpiritTypeReporting on prod2SpiritType.SpiritTypeReportingID equals spiritTRep.SpiritTypeReportingID into spiritTRep_join
                                             from spiritTRep in spiritTRep_join.DefaultIfEmpty()
                                             where
                                             prodContent.isProductionComponent == true &&
                                             prodContent.ProductionID == tempRecordID
                                             select new
                                             {
                                                 RecordID = (int?)prodContent.RecordID,
                                                 SpiritTypeReportingID = (int?)spiritTRep.SpiritTypeReportingID ?? (int?)0,
                                                 SpiritTypeReportingName = spiritTRep.ProductTypeName ?? String.Empty,
                                                 MaterialKindReportingID = (int?)matKindRep.MaterialKindReportingID ?? (int?)0,
                                                 MaterialKindReportingName = matKindRep.MaterialKindName ?? String.Empty
                                             }).FirstOrDefault();
                                        if (t != null)
                                        {
                                            if (t.SpiritTypeReportingID != null || t.SpiritTypeReportingID != 0)
                                            {
                                                var record = tempRepObjList.Find(x => x.ProductionID == i.ProductionID);
                                                record.SpiritTypeReportingID = (int)t.SpiritTypeReportingID;
                                                record.SpiritTypeReportName = t.SpiritTypeReportingName;
                                                record.MaterialKindReportingID = (int)t.MaterialKindReportingID;
                                                record.MaterialKindReportingName = t.MaterialKindReportingName;
                                                spiritTypeFound = true;
                                            }
                                            else
                                            {
                                                tempRecordID = t.RecordID;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    var record = tempRepObjList.Find(x => x.ProductionID == i.ProductionID);
                                    record.SpiritTypeReportingID = (int)k.SpiritTypeReportingID;
                                    record.SpiritTypeReportName = k.SpiritTypeReportingName;
                                    record.MaterialKindReportingID = (int)k.MaterialKindReportingID;
                                    record.MaterialKindReportingName = k.MaterialKindReportingName;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// GetProductionReportPart1Records method calls Rows in the report iteratively
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="part1List"></param>
        /// <param name="tempRepObjList"></param>
        private void GetProductionReportPart1Records(DateTimeOffset start, DateTimeOffset end, ref List<ProdReportPart1> part1List, List<ProductionReportHelper> tempRepObjList)
        {
            foreach (var pRec in tempRepObjList)
            {
                // Entered into Storage and Processing account case
                GetEnteredInStorageAndProcessingAccount(start, end, pRec, ref part1List);
            }
        }

        /// <summary>
        /// This method gets PFGals for line 11 on Production report
        /// given the initial list of production Ids for that month.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="record"></param>
        /// <param name="part1List"></param>
        private void GetEnteredInStorageAndProcessingAccount(DateTimeOffset start, DateTimeOffset end, ProductionReportHelper record, ref List<ProdReportPart1> part1List)
        {
            float blendedProof = 0f;
            float distilledProof = 0f;

            // Entered into Storage account case 
            var blended =
                (from productionContent in _db.ProductionContent
                 join production in _db.Production on productionContent.ProductionID equals production.ProductionID into production_join
                 from production in production_join.DefaultIfEmpty()
                 join gainLoss in _db.GainLoss on production.ProductionID equals gainLoss.ProductionId into gainLoss_join
                 from gainLoss in gainLoss_join.DefaultIfEmpty()
                 join prod4Rep in _db.Production4Reporting on productionContent.ProductionID equals prod4Rep.ProductionID
                 where productionContent.RecordID == record.ProductionID
                 && gainLoss.Type == false
                 && production.ProductionEndTimeOffset >= start
                 && production.ProductionEndTimeOffset <= end
                 && prod4Rep.Redistilled == false
                 && production.StateID == (int)Persistence.BusinessLogicEnums.State.Blended
                 select new
                 {
                     id = prod4Rep.ProductionID,
                     proof = (float?)prod4Rep.Proof ?? (float?)0,
                     loss = (float?)gainLoss.Quantity
                 }).Distinct().ToList();

            if (blended != null)
            {
                foreach (var i in blended)
                {
                    blendedProof += (float)Math.Round((float)i.proof + (float)i.loss, 3);
                }
            }
            else
            {
                blendedProof = 0f;
            }

            var distilled =
                (from prod4Reporting in _db.Production4Reporting
                 where prod4Reporting.ProductionID == record.ProductionID
                 select new
                 {
                     Proof = (float?)prod4Reporting.Proof ?? (float?)0
                 }).FirstOrDefault();

            if (distilled != null)
            {
                distilledProof = (float)Math.Round((float)distilled.Proof, 3);
            }
            else
            {
                distilledProof = 0f;
            }

            var spiritType = part1List.Find(x => x.SpiritTypeReportingID == record.SpiritTypeReportingID);

            if (spiritType == null)
            {
                ProdReportPart1 part1Obj = new ProdReportPart1();
                part1Obj.SpiritCatName = record.SpiritTypeReportName;
                part1Obj.SpiritTypeReportingID = (int)record.SpiritTypeReportingID;
                part1Obj.ProccessingAcct = (float)Math.Round(blendedProof, 3);
                float storageProof = (float)Math.Round((distilledProof - blendedProof) >= 0 ? (distilledProof - blendedProof) : 0, 3);
                part1Obj.StorageAcct = (float)Math.Round(storageProof, 3);
                part1Obj.ProducedTotal = (float)Math.Round((blendedProof + storageProof), 3);

                part1List.Add(part1Obj);
            }
            else
            {
                float storageProof = (float)Math.Round((distilledProof - blendedProof) >= 0 ? (distilledProof - blendedProof) : 0, 3);
                spiritType.StorageAcct += (float)Math.Round(storageProof, 3);
                spiritType.ProducedTotal += (float)Math.Round((blendedProof + storageProof), 3);
            }
        }

        /// <summary>
        /// GetUnfinishedSpiritsForProductionReport method gets quarterly unfinished spirits data
        /// </summary>
        /// <param name="userId">Distiller user id</param>
        /// <param name="start">reporting start period</param>
        /// <param name="end">reporting end period</param>
        /// <param name="tempRepObjList">out parameter object list that needs to be populated</param>
        public void GetUnfinishedSpiritsForProductionReport(int userId, DateTimeOffset start, DateTimeOffset end, ref List<ProductionReportHelper> tempRepObjList)
        {
            // this section of code checks the reporting month to determine whether we need to report
            // line 17 of production report
            // for February reporting
            DateTimeOffset[] startAndEndDates = new DateTimeOffset[2]; // [startDate, endDate]

            if (end.Month == february)
            {
                start = new DateTimeOffset(end.Year - 1, december, 1, 0, 0, 0, TimeSpan.Zero);
            }
            else if (end.Month == may)
            {
                start = new DateTimeOffset(end.Year, march, 1, 0, 0, 0, TimeSpan.Zero);
            }
            else if (end.Month == august)
            {
                start = new DateTimeOffset(end.Year, june, 1, 0, 0, 0, TimeSpan.Zero);
            }
            else if (end.Month == november)
            {
                start = new DateTimeOffset(end.Year, september, 1, 0, 0, 0, TimeSpan.Zero);
            }

            try
            {
                var ress =
                (from prod in _db.Production
                 join prodReport in _db.Production4Reporting on prod.ProductionID equals prodReport.ProductionID into prodReport_join
                 from prodReport in prodReport_join.DefaultIfEmpty()
                 join distillers in _db.AspNetUserToDistiller on prod.DistillerID equals distillers.DistillerID into distillers_join
                 from distillers in distillers_join.DefaultIfEmpty()
                 join prod2SpiritType in _db.ProductionToSpiritTypeReporting on prod.ProductionID equals prod2SpiritType.ProductionID into prod2SpiritType_join
                 from prod2SpiritType in prod2SpiritType_join.DefaultIfEmpty()
                 join matKindRep in _db.MaterialKindReporting on prod2SpiritType.MaterialKindReportingID equals matKindRep.MaterialKindReportingID into matKindRep_join
                 from matKindRep in matKindRep_join.DefaultIfEmpty()
                 join spiritTypeRep in _db.SpiritTypeReporting on prod2SpiritType.SpiritTypeReportingID equals spiritTypeRep.SpiritTypeReportingID into spiritTypeRep_join
                 from spiritTypeRep in spiritTypeRep_join.DefaultIfEmpty()
                 join prod2Purch in _db.ProductionToPurchase on prod.ProductionID equals prod2Purch.ProductionID into prod2Purch_join
                 from prod2Purch in prod2Purch_join.DefaultIfEmpty()
                 join purch4Reprt in _db.Purchase4Reporting on prod2Purch.PurchaseID equals purch4Reprt.PurchaseID into purch4Reprt_join
                 from purch4Reprt in purch4Reprt_join.DefaultIfEmpty()
                 join prodRepMatCat2MatKind in _db.ProdRepMatCat2MaterialKind on matKindRep.MaterialKindReportingID equals prodRepMatCat2MatKind.MaterialKindReportingID into prodRepMatCat2MatKind_join
                 from prodRepMatCat2MatKind in prodRepMatCat2MatKind_join.DefaultIfEmpty()
                 join prodRepMatCat in _db.ProductionReportMaterialCategory on prodRepMatCat2MatKind.ProductionReportMaterialCategoryID equals prodRepMatCat.ProductionReportMaterialCategoryID into prodRepMatCat_join
                 from prodRepMatCat in prodRepMatCat_join.DefaultIfEmpty()
                 where
                   distillers.UserId == userId &&
                   prod.Gauged == false &&
                   (prod.StatusID == (int)Persistence.BusinessLogicEnums.Status.Active ||
                   prod.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processing) &&
                   (new int[] { (int)Persistence.BusinessLogicEnums.State.Distilled, (int)Persistence.BusinessLogicEnums.State.Blended, (int)Persistence.BusinessLogicEnums.State.Bottled }).Contains(prod.StateID) &&
                   prod.ProductionEndTimeOffset >= start &&
                   prod.ProductionEndTimeOffset <= end
                 select new
                 {
                     prod.StateID,
                     ProductionID = (int?)prod.ProductionID ?? 0,
                     SpiritTypeName = spiritTypeRep.ProductTypeName ?? string.Empty,
                     Redistilled = (bool?)prodReport.Redistilled ?? false,
                     MaterialKindName = matKindRep.MaterialKindName ?? string.Empty,
                     Weight = (System.Single?)prodReport.Weight ?? (System.Single?)0,
                     Volume = (System.Single?)prodReport.Volume ?? (System.Single?)0,
                     Alcohol = (System.Single?)prodReport.Alcohol ?? (System.Single?)0,
                     Proof = (System.Single?)prodReport.Proof ?? (System.Single?)0,
                     SpiritTypeReportingID = (int?)spiritTypeRep.SpiritTypeReportingID ?? (int?)0,
                     MaterialKindReportingID = (int?)matKindRep.MaterialKindReportingID ?? (int?)0,
                     MaterialCategoryName = prodRepMatCat.MaterialCategoryName ?? string.Empty,
                     ProductionReportMaterialCategoryID = (int?)prodRepMatCat.ProductionReportMaterialCategoryID ?? (int?)0,
                     Gauged = (bool?)prod.Gauged ?? false
                 }).Distinct();

                // fill in temporary query results object so we can massage the data for further processing
                foreach (var l in ress)
                {
                    ProductionReportHelper tempRepObj = new ProductionReportHelper();
                    tempRepObj.StateID = l.StateID;
                    tempRepObj.ProductionID = (int)l.ProductionID;
                    tempRepObj.SpiritTypeReportName = l.SpiritTypeName;
                    tempRepObj.Redistilled = (bool)l.Redistilled;
                    tempRepObj.MaterialKindReportingName = l.MaterialKindName;
                    tempRepObj.Weight = (float)Math.Round((float)l.Weight, 3);
                    tempRepObj.Volume = (float)Math.Round((float)l.Volume, 3);
                    tempRepObj.Alcohol = (float)Math.Round((float)l.Alcohol, 3);
                    tempRepObj.Proof = (float)Math.Round((float)l.Proof, 3);
                    tempRepObj.SpiritTypeReportingID = (int)l.SpiritTypeReportingID;
                    tempRepObj.MaterialKindReportingID = (int)l.MaterialKindReportingID;
                    tempRepObj.Gauged = l.Gauged;

                    tempRepObjList.Add(tempRepObj);
                }

                if (tempRepObjList != null)
                {
                    // first, we want to make sure that every record received from the _db, has SpiritType and Material Kinds associated with it
                    // because there are cases when Blending and Bottling may not have SpiritType in a reporting sense. Our current design allows it
                    // but maybe worth changing it
                    foreach (var i in tempRepObjList)
                    {
                        if (i.SpiritTypeReportingID == 0)
                        {
                            try
                            {
                                var k =
                                      (from prodContent in _db.ProductionContent
                                       join prod2SpiritType in _db.ProductionToSpiritTypeReporting on prodContent.RecordID equals prod2SpiritType.ProductionID into prod2SpiritType_join
                                       from prod2SpiritType in prod2SpiritType_join.DefaultIfEmpty()
                                       join matKindRep in _db.MaterialKindReporting on prod2SpiritType.MaterialKindReportingID equals matKindRep.MaterialKindReportingID into matKindRep_join
                                       from matKindRep in matKindRep_join.DefaultIfEmpty()
                                       join spiritTRep in _db.SpiritTypeReporting on prod2SpiritType.SpiritTypeReportingID equals spiritTRep.SpiritTypeReportingID into spiritTRep_join
                                       from spiritTRep in spiritTRep_join.DefaultIfEmpty()
                                       where
                                          prodContent.isProductionComponent == true &&
                                          prodContent.ProductionID == i.ProductionID
                                       select new
                                       {
                                           RecordID = (int?)prodContent.RecordID,
                                           SpiritTypeReportingID = (int?)spiritTRep.SpiritTypeReportingID ?? (int?)0,
                                           SpiritTypeReportingName = spiritTRep.ProductTypeName,
                                           MaterialKindReportingID = (int?)matKindRep.MaterialKindReportingID,
                                           MaterialKindReportingName = matKindRep.MaterialKindName
                                       }).FirstOrDefault();
                                if (k != null)
                                {
                                    if ((int)k.SpiritTypeReportingID == null || (int)k.SpiritTypeReportingID == 0)
                                    {
                                        bool spiritTypeFound = false;
                                        var tempRecordID = k.RecordID;
                                        while (!spiritTypeFound)
                                        {
                                            var t =
                                                (from prodContent in _db.ProductionContent
                                                 join prod2SpiritType in _db.ProductionToSpiritTypeReporting on prodContent.RecordID equals prod2SpiritType.ProductionID into prod2SpiritType_join
                                                 from prod2SpiritType in prod2SpiritType_join.DefaultIfEmpty()
                                                 join matKindRep in _db.MaterialKindReporting on prod2SpiritType.MaterialKindReportingID equals matKindRep.MaterialKindReportingID into matKindRep_join
                                                 from matKindRep in matKindRep_join.DefaultIfEmpty()
                                                 join spiritTRep in _db.SpiritTypeReporting on prod2SpiritType.SpiritTypeReportingID equals spiritTRep.SpiritTypeReportingID into spiritTRep_join
                                                 from spiritTRep in spiritTRep_join.DefaultIfEmpty()
                                                 where
                                                    prodContent.isProductionComponent == true &&
                                                    prodContent.ProductionID == tempRecordID
                                                 select new
                                                 {
                                                     RecordID = (int?)prodContent.RecordID,
                                                     SpiritTypeReportingID = (int?)spiritTRep.SpiritTypeReportingID ?? (int?)0,
                                                     SpiritTypeReportingName = spiritTRep.ProductTypeName,
                                                     MaterialKindReportingID = (int?)matKindRep.MaterialKindReportingID,
                                                     MaterialKindReportingName = matKindRep.MaterialKindName
                                                 }).FirstOrDefault();
                                            if (t != null)
                                            {
                                                if (t.SpiritTypeReportingID != null || t.SpiritTypeReportingID != 0)
                                                {
                                                    var record = tempRepObjList.Find(x => x.ProductionID == i.ProductionID);
                                                    record.SpiritTypeReportingID = (int)t.SpiritTypeReportingID;
                                                    record.SpiritTypeReportName = t.SpiritTypeReportingName;
                                                    record.MaterialKindReportingID = (int)t.MaterialKindReportingID;
                                                    record.MaterialKindReportingName = t.MaterialKindReportingName;
                                                    spiritTypeFound = true;
                                                }
                                                else
                                                {
                                                    tempRecordID = t.RecordID;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var record = tempRepObjList.Find(x => x.ProductionID == i.ProductionID);
                                        record.SpiritTypeReportingID = (int)k.SpiritTypeReportingID;
                                        record.SpiritTypeReportName = k.SpiritTypeReportingName;
                                        record.MaterialKindReportingID = (int)k.MaterialKindReportingID;
                                        record.MaterialKindReportingName = k.MaterialKindReportingName;
                                    }
                                }
                                else
                                {
                                    // if there are no records in ProductionComponent with value isProductionComponent == 1 then we know that this material was made out of purchased material
                                }
                            }
                            catch (Exception e)
                            {
                                throw e;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Gets records that are marked as received for redistillation on Line 15 of Production report
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="tempRepObjList"></param>
        /// <param name="prodRPart5L"></param>
        private void GetReceivedForRedistillationForLine15(int userId, DateTimeOffset start, DateTimeOffset end, ref List<ProdReportPart1> part1List, ref List<ProdReportPart5> prodRPart5L)
        {
            var ress =
                (from prod in _db.Production
                 join distillers in _db.AspNetUserToDistiller on prod.DistillerID equals distillers.DistillerID into distillers_join
                 from distillers in distillers_join.DefaultIfEmpty()
                 join prod2SpiritType in _db.ProductionToSpiritTypeReporting on prod.ProductionID equals prod2SpiritType.ProductionID into prod2SpiritType_join
                 from prod2SpiritType in prod2SpiritType_join.DefaultIfEmpty()
                 join matKindRep in _db.MaterialKindReporting on prod2SpiritType.MaterialKindReportingID equals matKindRep.MaterialKindReportingID into matKindRep_join
                 from matKindRep in matKindRep_join.DefaultIfEmpty()
                 join spiritTypeRep in _db.SpiritTypeReporting on prod2SpiritType.SpiritTypeReportingID equals spiritTypeRep.SpiritTypeReportingID into spiritTypeRep_join
                 from spiritTypeRep in spiritTypeRep_join.DefaultIfEmpty()
                 join prodRepMatCat2MatKind in _db.ProdRepMatCat2MaterialKind on matKindRep.MaterialKindReportingID equals prodRepMatCat2MatKind.MaterialKindReportingID into prodRepMatCat2MatKind_join
                 from prodRepMatCat2MatKind in prodRepMatCat2MatKind_join.DefaultIfEmpty()
                 join prodRepMatCat in _db.ProductionReportMaterialCategory on prodRepMatCat2MatKind.ProductionReportMaterialCategoryID equals prodRepMatCat.ProductionReportMaterialCategoryID into prodRepMatCat_join
                 from prodRepMatCat in prodRepMatCat_join.DefaultIfEmpty()
                 where
                   distillers.UserId == userId &&
                   (new int[] { (int)Persistence.BusinessLogicEnums.State.Distilled }).Contains(prod.StateID) &&
                   prod.ProductionEndTimeOffset >= start &&
                   prod.ProductionEndTimeOffset <= end
                 select new
                 {
                     prod.StateID,
                     ProductionID = (int?)prod.ProductionID,
                     SpiritTypeName = spiritTypeRep.ProductTypeName ?? string.Empty,
                     MaterialKindName = matKindRep.MaterialKindName ?? string.Empty,
                     SpiritTypeReportingID = (int?)spiritTypeRep.SpiritTypeReportingID ?? (int?)0,
                     MaterialKindReportingID = (int?)matKindRep.MaterialKindReportingID ?? (int?)0,
                     MaterialCategoryName = prodRepMatCat.MaterialCategoryName ?? string.Empty,
                     ProductionReportMaterialCategoryID = (int?)prodRepMatCat.ProductionReportMaterialCategoryID ?? (int?)0,
                     Gauged = (bool?)prod.Gauged ?? false
                 }).Distinct().ToList();

            if (ress != null)
            {
                try
                {
                    List<ProductionContentIterator> prodContentIteratorList = new List<ProductionContentIterator>();

                    foreach (var i in ress)
                    {
                        GetRedistilledProductionContentRecordsForProcessing(start, end, ref prodContentIteratorList, (int)i.ProductionID /*productionID or recordID*/);
                    }

                    foreach (var k in prodContentIteratorList)
                    {
                        if (k.IsProductionComponent == true)
                        {
                            GetProductionRedistilledRecords(k.ProductionContentId, k.Proof, ref part1List, ref prodRPart5L);
                        }
                        else if (k.IsProductionComponent == false)
                        {
                            GetPurchasedRedistilledRecords(k.ProductionContentId, k.Proof, ref part1List, ref prodRPart5L);
                        }
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        /// <summary>
        /// GetRedistilledProductionContentRecordsForProcessing method is used 
        /// in GetReceivedForRedistillationForLine15. This method recursively 
        /// getting the data needed to compute Received For Redistillation on line 15
        /// of Production Report.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="prodContentIteratorList"></param>
        /// <param name="currRecordId"></param>
        private void GetRedistilledProductionContentRecordsForProcessing(DateTimeOffset start, DateTimeOffset end, ref List<ProductionContentIterator> prodContentIteratorList, int currRecordId/*if it's a first iteration then it is productionID else it is recordID*/)
        {
            bool parentGauged = false;
            int parentState = 0;
            int parentProductionId = 0;
            try
            {
                var baseCaseRecords =
                   (from prodContent in _db.ProductionContent
                    join CF in _db.ContentField on prodContent.ContentFieldID equals CF.ContentFieldID into CF_join
                    from CF in CF_join.DefaultIfEmpty()
                    join prod in _db.Production on prodContent.ProductionID equals prod.ProductionID into prod_join
                    from prod in prod_join.DefaultIfEmpty()
                    where
                       prodContent.ProductionID == currRecordId
                       && (new int[]
                       {
                           (int)Persistence.BusinessLogicEnums.ContenField.PurFermentedProofGal,
                           (int)Persistence.BusinessLogicEnums.ContenField.PurDistilledProofGal,
                           (int)Persistence.BusinessLogicEnums.ContenField.ProdDistilledProofGal,
                           (int)Persistence.BusinessLogicEnums.ContenField.ProdBlendedProofGal,
                           (int)Persistence.BusinessLogicEnums.ContenField.ProdFermentedProofGal
                       }).Contains(prodContent.ContentFieldID)
                       && prod.ProductionStartTimeOffset >= start
                       && prod.ProductionEndTimeOffset <= end
                    select new
                    {
                        prodContent.isProductionComponent, // the record that went into making this production record is from either Purchase or Prod
                        ContentFieldID = CF.ContentFieldID,
                        RecordID = prodContent.RecordID,
                        Proof = (float?)prodContent.ContentValue ?? (float?)0,
                        prodContent.ProductionContentID,
                        ProductionID = prodContent.ProductionID,
                        Gauged = prod.Gauged,
                        StateID = prod.StateID
                    }).ToList();

                if (baseCaseRecords != null)
                {
                    foreach (var current in baseCaseRecords)
                    {
                        var prodContentParentProductionRecordInfo =
                        (from prod in _db.Production
                         where prod.ProductionID == current.RecordID
                         select prod).FirstOrDefault();

                        if (prodContentParentProductionRecordInfo != null)
                        {
                            parentProductionId = prodContentParentProductionRecordInfo.ProductionID;
                            parentGauged = prodContentParentProductionRecordInfo.Gauged;
                            parentState = prodContentParentProductionRecordInfo.StateID;
                        }
                        else
                        {
                            parentGauged = false;
                            parentState = 0;
                            parentProductionId = 0;
                        }

                        // todo: looks like the two conditionals below are doing the same thing
                        // someone needs to try to combine "if" with "if else" and test to see if
                        // it doesn't break anything.
                        if (current.isProductionComponent == false)
                        {
                            ProductionContentIterator pCI = new ProductionContentIterator();

                            pCI.ProductionId = (int)current.ProductionID;
                            pCI.ProductionContentId = (int)current.ProductionContentID;
                            pCI.RecordId = (int)current.RecordID;
                            pCI.IsProductionComponent = (bool)current.isProductionComponent;
                            pCI.Proof = (float)Math.Round((float)current.Proof, 3);

                            prodContentIteratorList.Add(pCI);
                        }
                        else if (current.Gauged == true && current.StateID == (int)Persistence.BusinessLogicEnums.State.Distilled && parentGauged == true)
                        {
                            ProductionContentIterator pCI = new ProductionContentIterator();

                            pCI.ProductionId = (int)current.ProductionID;
                            pCI.ProductionContentId = (int)current.ProductionContentID;
                            pCI.RecordId = (int)current.RecordID;
                            pCI.IsProductionComponent = (bool)current.isProductionComponent;
                            pCI.Proof = (float)Math.Round((float)current.Proof, 3);

                            prodContentIteratorList.Add(pCI);
                        }

                        if (current.isProductionComponent != false)
                        {
                            var skipNextRecrusiveCall = prodContentIteratorList.Find(x => x.ProductionId == parentProductionId);
                            if (!(skipNextRecrusiveCall != null))
                            {
                                GetRedistilledProductionContentRecordsForProcessing(start, end, ref prodContentIteratorList, current.RecordID);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// GetProductionRedistilledRecords retrieves
        /// data for produced records that were redistilled
        /// from ProductionContent table
        /// </summary>
        /// <param name="prodContentId"></param>
        /// <param name="proof"></param>
        /// <param name="part1List"></param>
        /// <param name="prodRPart5L"></param>
        private void GetProductionRedistilledRecords(int prodContentId, float proof, ref List<ProdReportPart1> part1List, ref List<ProdReportPart5> prodRPart5L)
        {
            try
            {
                var productionSpiritType =
                (from prodContent in _db.ProductionContent
                 join prod2SpiritType in _db.ProductionToSpiritTypeReporting on prodContent.RecordID equals prod2SpiritType.ProductionID into pur2SpiritType_join
                 from pur2SpiritType in pur2SpiritType_join.DefaultIfEmpty()
                 join spiritTypeRep in _db.SpiritTypeReporting on pur2SpiritType.SpiritTypeReportingID equals spiritTypeRep.SpiritTypeReportingID into spiritTypeRep_join
                 from spiritTypeRep in spiritTypeRep_join.DefaultIfEmpty()
                 where
                 prodContent.ProductionContentID == prodContentId
                 select new
                 {
                     SpiritShortName = spiritTypeRep.ProductTypeName ?? String.Empty,
                     SpiritTypeReportingID = (int?)spiritTypeRep.SpiritTypeReportingID ?? (int?)0
                 }).FirstOrDefault();

                if (productionSpiritType != null)
                {
                    if ((int)productionSpiritType.SpiritTypeReportingID != 0)
                    {
                        var spRec = part1List.Find(x => x.SpiritTypeReportingID == (int)productionSpiritType.SpiritTypeReportingID);
                        if (spRec != null)
                        {
                            spRec.Recd4RedistilaltionL15 += (float)Math.Round(proof, 3);

                            var prod5Rec = prodRPart5L.Find(x => x.KindofSpirits == productionSpiritType.SpiritShortName);

                            if (prod5Rec != null)
                            {
                                prod5Rec.Proof += (float)Math.Round(proof, 3);
                            }
                            else
                            {
                                ProdReportPart5 prod5Inst = new ProdReportPart5();
                                prod5Inst.KindofSpirits = productionSpiritType.SpiritShortName;
                                prod5Inst.Proof = (float)Math.Round(proof, 3);

                                prodRPart5L.Add(prod5Inst);
                            }
                        }
                        else
                        {
                            ProdReportPart1 part1Obj = new ProdReportPart1();
                            part1Obj.Recd4RedistilaltionL15 = (float)Math.Round(proof, 3);
                            part1Obj.SpiritTypeReportingID = (int)productionSpiritType.SpiritTypeReportingID;
                            part1Obj.SpiritCatName = productionSpiritType.SpiritShortName;

                            part1List.Add(part1Obj);

                            ProdReportPart5 prod5Inst = new ProdReportPart5();
                            prod5Inst.KindofSpirits = part1Obj.SpiritCatName;
                            prod5Inst.Proof = (float)Math.Round(part1Obj.Recd4RedistilaltionL15, 3);

                            prodRPart5L.Add(prod5Inst);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// GetProductionRedistilledRecords retrieves
        /// data for purchased records that were redistilled
        /// from ProductionContent table
        /// </summary>
        /// <param name="productionContentId"></param>
        /// <param name="proof"></param>
        /// <param name="part1List"></param>
        /// <param name="prodRPart5L"></param>
        public void GetPurchasedRedistilledRecords(int productionContentId, float proof, ref List<ProdReportPart1> part1List, ref List<ProdReportPart5> prodRPart5L)
        {
            try
            {
                var purchaseSpiritType =
                (from prodContent in _db.ProductionContent
                 join pur2SpiritType in _db.PurchaseToSpiritTypeReporting on prodContent.RecordID equals pur2SpiritType.PurchaseID into pur2SpiritType_join
                 from pur2SpiritType in pur2SpiritType_join.DefaultIfEmpty()
                 join spiritTypeRep in _db.SpiritTypeReporting on pur2SpiritType.SpiritTypeReportingID equals spiritTypeRep.SpiritTypeReportingID into spiritTypeRep_join
                 from spiritTypeRep in spiritTypeRep_join.DefaultIfEmpty()
                 where
                    prodContent.ProductionContentID == productionContentId
                 select new
                 {
                     SpiritShortName = spiritTypeRep.ProductTypeName ?? String.Empty,
                     SpiritTypeReportingID = (int?)spiritTypeRep.SpiritTypeReportingID ?? (int?)0
                 }).FirstOrDefault();

                if (purchaseSpiritType != null)
                {
                    if ((int)purchaseSpiritType.SpiritTypeReportingID != 0)
                    {
                        var spRec = part1List.Find(x => x.SpiritTypeReportingID == (int)purchaseSpiritType.SpiritTypeReportingID);
                        if (spRec != null)
                        {
                            spRec.Recd4RedistilaltionL15 += (float)Math.Round(proof, 3);

                            var prod5Rec = prodRPart5L.Find(x => x.KindofSpirits == purchaseSpiritType.SpiritShortName);

                            if (prod5Rec != null)
                            {
                                prod5Rec.Proof += (float)Math.Round(proof, 3);
                            }
                            else
                            {
                                ProdReportPart5 prod5Inst = new ProdReportPart5();
                                prod5Inst.KindofSpirits = purchaseSpiritType.SpiritShortName;
                                prod5Inst.Proof = (float)Math.Round(proof, 3);

                                prodRPart5L.Add(prod5Inst);
                            }
                        }
                        else
                        {
                            ProdReportPart1 part1Obj = new ProdReportPart1();
                            part1Obj.Recd4RedistilaltionL15 = (float)Math.Round(proof, 3);
                            part1Obj.SpiritTypeReportingID = (int)purchaseSpiritType.SpiritTypeReportingID;
                            part1Obj.SpiritCatName = (string)purchaseSpiritType.SpiritShortName;

                            part1List.Add(part1Obj);

                            ProdReportPart5 prod5Inst = new ProdReportPart5();
                            prod5Inst.KindofSpirits = part1Obj.SpiritCatName;
                            prod5Inst.Proof = (float)Math.Round(part1Obj.Recd4RedistilaltionL15, 3);

                            prodRPart5L.Add(prod5Inst);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// GetRecordsForParts2Through4 retrieves records from Production report
        /// for part 2 - 4.
        /// </summary>
        /// <param name="part1List"></param>
        /// <param name="part2Thru4List"></param>
        /// <param name="tempRepObjList"></param>
        private void GetRecordsForParts2Through4(ref List<ProdReportPart1> part1List, ref List<ProdReportParts2Through4> part2Thru4List, List<ProductionReportHelper> tempRepObjList)
        {
            foreach (var rec in tempRepObjList)
            {
                // Deal with parts 2 through 4 Start
                if (rec.MaterialKindReportingID > 0)
                {
                    var materialKind = part2Thru4List.Find(x => x.MaterialKindReportingID == rec.MaterialKindReportingID);

                    if (materialKind == null)
                    {
                        ProdReportParts2Through4 prodRP2T4 = new ProdReportParts2Through4();
                        prodRP2T4.KindOfMaterial = rec.MaterialKindReportingName;
                        prodRP2T4.MaterialKindReportingID = (int)rec.MaterialKindReportingID;

                        var part4 =
                            (from prodForRep in _db.Production4Reporting
                             where prodForRep.ProductionID == rec.ProductionID
                             select new
                             {
                                 Proof = (float?)prodForRep.Proof ?? (float?)0
                             }).FirstOrDefault();

                        prodRP2T4.ProofGallons = (float)Math.Round((float)part4.Proof, 3);

                        prodRP2T4.SpiritTypeReportingID = (int)rec.SpiritTypeReportingID;

                        part2Thru4List.Add(prodRP2T4);
                    }
                    else
                    {
                        var part4 =
                            (from prodForRep in _db.Production4Reporting
                             where prodForRep.ProductionID == rec.ProductionID
                             select new
                             {
                                 Proof = (float?)prodForRep.Proof ?? (float?)0
                             }).FirstOrDefault();

                        materialKind.ProofGallons += (float)Math.Round((float)part4.Proof, 3);
                    }
                }
            }
        }

        /// <summary>
        /// ProductionReportPart6Data method retrieves
        /// data from part 6 of Production report.
        /// </summary>
        /// <param name="prodReportPart6List"></param>
        /// <param name="tempRepObjList"></param>
        private void ProductionReportPart6Data(ref List<ProdReportPart6> prodReportPart6List, ref List<ProductionReportHelper> tempRepObjList)
        {
            List<ProductionPart6DataForParsing> productionContentList = new List<ProductionPart6DataForParsing>();
            float updatedRawMaterialAmmt = 0f;

            foreach (var i in tempRepObjList) // iterate through the set of all active production records for this month and get Part 6 information
            {
                try
                {
                    var prodFromPurchaseSet = (from prodCont in _db.ProductionContent
                                               join prod2Purch in _db.ProductionToPurchase on prodCont.RecordID equals prod2Purch.PurchaseID into prod2Purch_join
                                               from prod2Purch in prod2Purch_join.DefaultIfEmpty()
                                               join purch in _db.Purchase on prod2Purch.PurchaseID equals purch.PurchaseID into purch_join
                                               from purch in purch_join.DefaultIfEmpty()
                                               join matDict2MatCat in _db.MaterialDict2MaterialCategory on purch.MaterialDictID equals matDict2MatCat.MaterialDictID into matDict2MatCat_join
                                               from matDict2MatCat in matDict2MatCat_join.DefaultIfEmpty()
                                               join prodRepMatCat in _db.ProductionReportMaterialCategory on matDict2MatCat.ProductionReportMaterialCategoryID equals prodRepMatCat.ProductionReportMaterialCategoryID into prodRepMatCat_join
                                               from prodRepMatCat in prodRepMatCat_join.DefaultIfEmpty()
                                               join matDict in _db.MaterialDict on purch.MaterialDictID equals matDict.MaterialDictID into matDict_join
                                               from matDict in matDict_join.DefaultIfEmpty()
                                               where prod2Purch.ProductionID == i.ProductionID
                                               && prodCont.isProductionComponent == false
                                               select  new
                                               {
                                                   ProductionID = prodCont.ProductionID,
                                                   ContentValue = prodCont.ContentValue,
                                                   ContentFieldID = prodCont.ContentFieldID,
                                                   RecordID = prodCont.RecordID,
                                                   PurchaseID = prod2Purch.PurchaseID,
                                                   ProductionReportMaterialCategoryId = (int?)prodRepMatCat.ProductionReportMaterialCategoryID ?? (int?)0,
                                                   MaterialCategoryName = matDict.Name
                                               }).ToList();

                    if (prodFromPurchaseSet != null && prodFromPurchaseSet.Count() > 0)
                    {
                        foreach (var prodFromPurchase in prodFromPurchaseSet) // iterate throught the list of all Purchase records that were used in producing this month's distills
                        {
                            var productions = (from prodCont in _db.ProductionContent
                                               where prodCont.RecordID == prodFromPurchase.ProductionID
                                               && prodCont.ProductionID == i.ProductionID
                                               select prodCont)
                                               .ToList();

                            if (productions != null && productions.Count() > 0)
                            {
                                foreach (var prod in productions)
                                {
                                    if (prod.ContentFieldID == (int)Persistence.BusinessLogicEnums.ContenField.ProdFermentedVolume ||
                                        prod.ContentFieldID == (int)Persistence.BusinessLogicEnums.ContenField.ProdFermentedWeight)
                                    {
                                        var originalFermentedAmount = (from prodForReporting in _db.Production4Reporting
                                                                       where prodForReporting.ProductionID == prod.RecordID
                                                                       select new
                                                                       {
                                                                           originalFermentedAmmt = prodForReporting.Volume > 0 ? prodForReporting.Volume : prodForReporting.Weight,
                                                                       }).FirstOrDefault();

                                        ProductionPart6DataForParsing prt6TempData = new ProductionPart6DataForParsing();
                                        prt6TempData.OriginalRawMaterialAmount = (float)Math.Round(prodFromPurchase.ContentValue, 3);
                                        prt6TempData.ContentFieldId = prodFromPurchase.ContentFieldID;
                                        prt6TempData.FermentedAmountWentIntoCurrentProduction = (float)Math.Round(prod.ContentValue, 3);
                                        prt6TempData.OriginalFermentedAmount = (float)Math.Round(originalFermentedAmount != null ? originalFermentedAmount.originalFermentedAmmt : 0, 3);
                                        prt6TempData.ReportingPeriodProductionId = i.ProductionID;
                                        prt6TempData.NeedsMappingFromFermentedToPurchase = true;
                                        prt6TempData.PurchaseId = prodFromPurchase.RecordID;
                                        prt6TempData.ProductionReportMaterialCategoryId = (int)prodFromPurchase.ProductionReportMaterialCategoryId;
                                        prt6TempData.MaterialCategoryName = prodFromPurchase.MaterialCategoryName;

                                        productionContentList.Add(prt6TempData);
                                    }
                                }
                            }
                            else // this is the case with Pomace where we distill fermented product like Pomace directly without fermenting it first
                            {
                                var productionInList = productionContentList.Where(x => x.ReportingPeriodProductionId == i.ProductionID).FirstOrDefault();

                                if (productionInList != null && productionInList.PurchaseId != prodFromPurchase.RecordID)
                                {
                                    if (prodFromPurchase.ContentFieldID == (int)Persistence.BusinessLogicEnums.ContenField.PurFermentedVolume ||
                                        prodFromPurchase.ContentFieldID == (int)Persistence.BusinessLogicEnums.ContenField.PurFermentedWeight)
                                    {
                                        var originalFermentedAmount = (from purchForReporting in _db.Purchase4Reporting
                                                                       where purchForReporting.PurchaseID == prodFromPurchase.RecordID
                                                                       select new
                                                                       {
                                                                           originalFermentedAmmt = purchForReporting.Volume > 0 ? purchForReporting.Volume : purchForReporting.Weight
                                                                       }).FirstOrDefault();

                                        productionInList.FermentedAmountWentIntoCurrentProduction += (float)Math.Round(prodFromPurchase.ContentValue, 3);
                                    }
                                }
                                else if (productionInList == null && i.ProductionID == prodFromPurchase.ProductionID) // to avoid double counting we need to ensure that productionID from this month matches productionID that came from Production2Purchase table
                                {
                                    if (prodFromPurchase.ContentFieldID == (int)Persistence.BusinessLogicEnums.ContenField.PurFermentedVolume ||
                                        prodFromPurchase.ContentFieldID == (int)Persistence.BusinessLogicEnums.ContenField.PurFermentedWeight)
                                    {
                                        var originalFermentedAmount = (from purchForReporting in _db.Purchase4Reporting
                                                                       where purchForReporting.PurchaseID == prodFromPurchase.RecordID
                                                                       select new
                                                                       {
                                                                           originalFermentedAmmt = purchForReporting.Volume > 0 ? purchForReporting.Volume : purchForReporting.Weight
                                                                       }).FirstOrDefault();

                                        ProductionPart6DataForParsing prt6TempData = new ProductionPart6DataForParsing();
                                        prt6TempData.OriginalRawMaterialAmount = (float)Math.Round(originalFermentedAmount != null ? originalFermentedAmount.originalFermentedAmmt : 0, 3);
                                        prt6TempData.ContentFieldId = prodFromPurchase.ContentFieldID;
                                        prt6TempData.FermentedAmountWentIntoCurrentProduction = (float)Math.Round(prodFromPurchase.ContentValue, 3);
                                        prt6TempData.OriginalFermentedAmount = (float)Math.Round(prodFromPurchase.ContentValue, 3);
                                        prt6TempData.ReportingPeriodProductionId = i.ProductionID;
                                        prt6TempData.NeedsMappingFromFermentedToPurchase = false;
                                        prt6TempData.PurchaseId = prodFromPurchase.RecordID;
                                        prt6TempData.ProductionReportMaterialCategoryId = (int)prodFromPurchase.ProductionReportMaterialCategoryId;
                                        prt6TempData.MaterialCategoryName = prodFromPurchase.MaterialCategoryName;

                                        productionContentList.Add(prt6TempData);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }

            foreach (var k in productionContentList)
            {
                try
                {
                    if (k.NeedsMappingFromFermentedToPurchase == false)
                    {
                        updatedRawMaterialAmmt = (float)Math.Round(k.FermentedAmountWentIntoCurrentProduction, 3);
                    }
                    else
                    {
                        updatedRawMaterialAmmt = (k.OriginalRawMaterialAmount / k.OriginalFermentedAmount) * k.FermentedAmountWentIntoCurrentProduction; // the idea here is to try to infere the amount of raw material used in wine production which in turn was used in a given distillation production.
                    }
                    var mater = prodReportPart6List.Find(x => x.KindOfMaterial == (string)k.MaterialCategoryName);

                    // case where Material doesn't exist
                    if (mater == null)
                    {
                        ProdReportPart6 prt6 = new ProdReportPart6();
                        prt6.ProductionID = k.ReportingPeriodProductionId;
                        prt6.KindOfMaterial = (string)k.MaterialCategoryName;
                        prt6.ProdReportMaterialCategoryID = (int)k.ProductionReportMaterialCategoryId;

                        if (k.ContentFieldId == (int)Persistence.BusinessLogicEnums.ContenField.PurFermentableVolume)
                        {
                            prt6.Volume = (float)Math.Round(updatedRawMaterialAmmt, 3);
                        }
                        else if (k.ContentFieldId == (int)Persistence.BusinessLogicEnums.ContenField.PurFermentableWeight || k.ContentFieldId == (int)Persistence.BusinessLogicEnums.ContenField.PurFermentedWeight)
                        {
                            prt6.Weight = (float)Math.Round(updatedRawMaterialAmmt, 3);
                        }

                        prodReportPart6List.Add(prt6);
                    }
                    else // case where Material already exists
                    {
                        if (k.ContentFieldId == (int)Persistence.BusinessLogicEnums.ContenField.PurFermentableVolume)
                        {
                            mater.Volume += (float)Math.Round(updatedRawMaterialAmmt, 3);
                        }
                        else if (k.ContentFieldId == (int)Persistence.BusinessLogicEnums.ContenField.PurFermentableWeight || k.ContentFieldId == (int)Persistence.BusinessLogicEnums.ContenField.PurFermentedWeight)
                        {
                            mater.Weight += (float)Math.Round(updatedRawMaterialAmmt, 3);
                        }
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
    }
}