using System;
using System.Collections.Generic;
using System.Linq;
using WebApp.Helpers;
using WebApp.Models;

namespace WebApp.Reports
{
    public class ProcessingReport
    {
        private readonly DistilDBContext _db;
        private readonly DataLayer _dl;

        public ProcessingReport(DistilDBContext db, DataLayer dl)
        {
            _db = db;
            _dl = dl;
        }

        /// <summary>
        /// GetProcessingReportData retrieves processing report data for a given month
        /// </summary>
        /// <param name="startOfReporting"> start date</param>
        /// <param name="endOfReporting">end date</param>
        /// <param name="userId"> user id of person who queries it</param>
        /// <returns></returns>
        public ProcessingReportingObject GetProcessingReportData(DateTime startOfReporting, DateTime endOfReporting, int userId)
        {
            ProcessingReportingObject procRepObj = new ProcessingReportingObject();
            ProcessReportingPart1 procRepP1 = new ProcessReportingPart1();
            ProcessReportingPart2 procRepP2 = new ProcessReportingPart2();
            List<ProcessReportingPart4> procRepP4L = new List<ProcessReportingPart4>();

            List<ProductionReportHelper> tempRepObjList = new List<ProductionReportHelper>();

            var line8RunningSum = 0F;
            var line26RunningSum = 0F;

            // get distiller information for header report
            int distillerID = _dl.GetDistillerId(userId);
            procRepObj.Header = _dl.GetDistillerInfoForReportHeader(distillerID, startOfReporting);

            // Processing Report Part 1 Section
            procRepP1.BulkIngredients = "spirit";

            // 1(c) previous month
            OnHandFirstOfMonth(startOfReporting, userId, ref procRepP1);

            line8RunningSum += procRepP1.OnHandFirstofMonth;

            // 2(c) current month received bulk
            Received(startOfReporting, endOfReporting, userId, ref procRepP1);

            line8RunningSum += procRepP1.Recd4Process;

            // 9 (c) Bottled or Packaged
            BottledOrPackaged(startOfReporting, endOfReporting, userId, ref procRepP1);

            line26RunningSum = procRepP1.AmtBottledPackaged;

            // 24 (c) Losses
            Losses(startOfReporting, endOfReporting, userId, ref procRepP1);

            line26RunningSum += (float)Math.Round(procRepP1.Losses, 3);

            // Round to three decimals
            line26RunningSum = (float)Math.Round(line26RunningSum, 3);

            if ((line8RunningSum - line26RunningSum) < 0)
            {
                throw new InvalidOperationException();
            }

            // 25(c) On hand end of month
            OnHandEndOfMonth(ref procRepP1, line8RunningSum, line26RunningSum);

            // Processing Report Part 2 Section
            // Bottled Column(b)
            procRepP2.FinishedProduct = "bottled";

            // 27(c) previous month
            OnHandFirstOfMonthBottled(startOfReporting, userId, ref procRepP2);

            // 28(b) Bottled or Packaged
            BottledOrPackagedpart2(startOfReporting, endOfReporting, userId, ref procRepP2);

            // 31 (b) 
            TotalLines27Through30(ref procRepP2);

            // line 33 - Withdrawn for Tax Determined
            WithdrawnTaxDetermined(startOfReporting, endOfReporting, ref procRepP2, userId);

            // 46 (b) On hand End of Month
            OnHandEndOfMonthPart2(endOfReporting, userId, ref procRepP2);

            // 47 (b)
            TotalLines32Through46(ref procRepP2);

            // Processing Report Part 4
            Part4ProcessingReport(startOfReporting, endOfReporting, userId, ref procRepP4L);

            procRepObj.Part1 = procRepP1;
            procRepObj.Part2 = procRepP2;
            procRepObj.Part4List = procRepP4L;

            // todo: figure out proper flow of roundings in this class.
            // Calling Round method now seems redundand since we are doing
            // rounding in each method
            Round(ref procRepObj);

            return procRepObj;
        }

        /// <summary>
        /// TotalLines27Through30 method simply adds up Lines 27 through 31
        /// </summary>
        /// <param name="procRepP2"></param>
        private void TotalLines27Through30(ref ProcessReportingPart2 procRepP2)
        {
            procRepP2.TotalLine31 = procRepP2.OnHandFirstofMonth + procRepP2.AmtBottledPackaged; // continue on adding extra rows as we add support for them
            Math.Round(procRepP2.TotalLine31, 3);
        }

        /// <summary>
        /// TotalLines32Through46 method simply adds up Lines 32 through 47
        /// </summary>
        /// <param name="procRepP2"></param>
        private void TotalLines32Through46(ref ProcessReportingPart2 procRepP2)
        {
            procRepP2.TotalLine47 = procRepP2.TaxWithdrawn + procRepP2.OnHandEndofMonth; // continue on adding extra rows as we add support for them
            Math.Round(procRepP2.TotalLine47, 3);
        }

        /// <summary>
        /// Get Part 4 data for Processing report
        /// TODO: it could use some optimization. Need to
        /// figure out how not to do string comparisons here.
        /// Perhaps enums would be useful here since ProcessingReportType
        /// is a small table.
        /// </summary>
        /// <param name="startOfReporting"></param>
        /// <param name="endOfReporting"></param>
        /// <param name="userId">userId is used to get DistillerID to get relevant report</param>
        /// <param name="procRepP4L"></param>
        private void Part4ProcessingReport(DateTime startOfReporting, DateTime endOfReporting, int userId, ref List<ProcessReportingPart4> procRepP4L)
        {
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
                 join prod2Purch in _db.ProductionToPurchase on prod.ProductionID equals prod2Purch.ProductionID into prod2Purch_join
                 from prod2Purch in prod2Purch_join.DefaultIfEmpty()
                 join purch4Reprt in _db.Purchase4Reporting on prod2Purch.PurchaseID equals purch4Reprt.PurchaseID into purch4Reprt_join
                 from purch4Reprt in purch4Reprt_join.DefaultIfEmpty()
                 join prod2Spirit in _db.ProductionToSpirit on prod.ProductionID equals prod2Spirit.ProductionID into prod2Spirit_join
                 from prod2Spirit in prod2Spirit_join.DefaultIfEmpty()
                 join spirit in _db.Spirit on prod2Spirit.SpiritID equals spirit.SpiritID into spirit_join
                 from spirit in spirit_join.DefaultIfEmpty()
                 join procRepType in _db.ProcessingReportType on spirit.ProcessingReportTypeID equals procRepType.ProcessingReportTypeID into procRepType_join
                 from procRepType in procRepType_join.DefaultIfEmpty()
                 where
                    distillers.UserId == userId &&
                    prod.Gauged == true &&
                    (prod.StatusID == (int)Persistence.BusinessLogicEnums.Status.Active ||
                    prod.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processing ||
                    prod.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processed) &&
                    (new int[] { (int)Persistence.BusinessLogicEnums.State.Blended, (int)Persistence.BusinessLogicEnums.State.Bottled }).Contains(prod.StateID) &&
                    prod.ProductionEndTime >= startOfReporting &&
                    prod.ProductionEndTime <= endOfReporting
                 select new
                 {
                     prod.StateID,
                     ProductionID = (int?)prod.ProductionID,
                     SpiritTypeName = spirit.Name ?? string.Empty,
                     Redistilled = (bool?)prodReport.Redistilled,
                     Weight = (System.Single?)prodReport.Weight ?? (System.Single?)0,
                     Volume = (System.Single?)prodReport.Volume ?? (System.Single?)0,
                     Alcohol = (System.Single?)prodReport.Alcohol ?? (System.Single?)0,
                     Proof = (System.Single?)prodReport.Proof ?? (System.Single?)0,
                     ProcessingType = procRepType.ProcessingReportTypeName ?? string.Empty,
                     ProcessingTypeID = (int?)procRepType.ProcessingReportTypeID,
                 }).Distinct();

                foreach (var rec in ress)
                {
                    bool isInProcRepList = false;
                    ProcessReportingPart4 part4Obj = new ProcessReportingPart4();
                    part4Obj.ProcessingReportTypeName = rec.ProcessingType;

                    if (rec.StateID == (int)Persistence.BusinessLogicEnums.State.Blended)
                    {
                        if (rec.ProcessingType == "ALCOHOL AND NEUTRAL SPIRITS (Other than vodka)")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.AlcoholNeutral += (float)rec.Proof;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bulkSpiritDumped";
                                part4Obj.AlcoholNeutral = (float)rec.Proof;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                        else if (rec.ProcessingType == "BLENDED STRAIGHT WHISKEY")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.BlendedStraightWhiskey += (float)rec.Proof;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bulkSpiritDumped";
                                part4Obj.BlendedStraightWhiskey = (float)rec.Proof;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                        else if (rec.ProcessingType == "BLENDED WHISKEY: With neutral spirits")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.BlendedWhiskeyWithNeutral += (float)rec.Proof;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bulkSpiritDumped";
                                part4Obj.BlendedWhiskeyWithNeutral = (float)rec.Proof;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                        else if (rec.ProcessingType == "BLENDED WHISKEY: With light whiskey")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.BlendedWhiskeyWithLight += (float)rec.Proof;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bulkSpiritDumped";
                                part4Obj.BlendedWhiskeyWithLight = (float)rec.Proof;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                        else if (rec.ProcessingType == "BLENDED LIGHT WHISKEY")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.BlendedLightWhiskey += (float)rec.Proof;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bulkSpiritDumped";
                                part4Obj.BlendedLightWhiskey = (float)rec.Proof;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                        else if (rec.ProcessingType == "ANY OTHER BLENDS OF 100% WHISKEY")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.BlendedOtherWhiskey += (float)rec.Proof;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bulkSpiritDumped";
                                part4Obj.BlendedOtherWhiskey = (float)rec.Proof;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                        else if (rec.ProcessingType == "IMPORTED WHISKEY: Scotch")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.ImportedWhiskeyScotch += (float)rec.Proof;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bulkSpiritDumped";
                                part4Obj.ImportedWhiskeyScotch = (float)rec.Proof;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                        else if (rec.ProcessingType == "IMPORTED WHISKEY: Canadian")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.ImportedWhiskeyCanadian += (float)rec.Proof;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bulkSpiritDumped";
                                part4Obj.ImportedWhiskeyCanadian = (float)rec.Proof;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                        else if (rec.ProcessingType == "IMPORTED WHISKEY: Irish and Others")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.ImportedWhiskeyIrish += (float)rec.Proof;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bulkSpiritDumped";
                                part4Obj.ImportedWhiskeyIrish = (float)rec.Proof;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                        else if (rec.ProcessingType == "DOMESTIC WHISKEY DISTILLED AT 160 AND UNDER")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.DomesticWhiskey160Under += (float)rec.Proof;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bulkSpiritDumped";
                                part4Obj.DomesticWhiskey160Under = (float)rec.Proof;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                        else if (rec.ProcessingType == " DOMESTIC WHISKEY DISTILLED AT OVER 160")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.DomesticWhiskeyOver160 += (float)rec.Proof;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bulkSpiritDumped";
                                part4Obj.DomesticWhiskeyOver160 = (float)rec.Proof;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                        else if (rec.ProcessingType == "BRANDY DISTILLED AT 170 AND UNDER")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.Brandy170Under += (float)rec.Proof;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bulkSpiritDumped";
                                part4Obj.Brandy170Under = (float)rec.Proof;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                        else if (rec.ProcessingType == "BRANDY DISTILLED AT OVER 170")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.BrandyOver170 += (float)rec.Proof;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bulkSpiritDumped";
                                part4Obj.BrandyOver170 = (float)rec.Proof;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                        else if (rec.ProcessingType == "SPIRITS (Rum):  Puerto Rican")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.RumPuertoRican += (float)rec.Proof;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bulkSpiritDumped";
                                part4Obj.RumPuertoRican = (float)rec.Proof;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                        else if (rec.ProcessingType == "SPIRITS (Rum): Virgin Islands")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.RumVirginIslands += (float)rec.Proof;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bulkSpiritDumped";
                                part4Obj.RumVirginIslands = (float)rec.Proof;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                        else if (rec.ProcessingType == "RUM: Domestic")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.RumDomestic += (float)rec.Proof;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bulkSpiritDumped";
                                part4Obj.RumDomestic = (float)rec.Proof;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                        else if (rec.ProcessingType == "RUM: \"Other\" Imported")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.RumOtherImported += (float)rec.Proof;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bulkSpiritDumped";
                                part4Obj.RumOtherImported = (float)rec.Proof;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                        else if (rec.ProcessingType == "GIN")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.Gin += (float)rec.Proof;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bulkSpiritDumped";
                                part4Obj.Gin = (float)rec.Proof;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                        else if (rec.ProcessingType == "VODKA")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.Vodka += (float)rec.Proof;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bulkSpiritDumped";
                                part4Obj.Vodka = (float)rec.Proof;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                        else if (rec.ProcessingType == "CORDIALS, LIQUEURS, AND SPECIALTIES")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.Liqueur += (float)rec.Proof;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bulkSpiritDumped";
                                part4Obj.Liqueur = (float)rec.Proof;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                        else if (rec.ProcessingType == "COCKTAILS AND MIXED DRINKS")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.Cocktail += (float)rec.Proof;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bulkSpiritDumped";
                                part4Obj.Cocktail = (float)rec.Proof;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                        else if (rec.ProcessingType == "TEQUILA")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.Tequila += (float)rec.Proof;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bulkSpiritDumped";
                                part4Obj.Tequila = (float)rec.Proof;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                    }
                    else if (rec.StateID == (int)Persistence.BusinessLogicEnums.State.Bottled) // processing bottling record
                    {
                        if (rec.ProcessingType == "ALCOHOL AND NEUTRAL SPIRITS (Other than vodka)")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.AlcoholNeutral += (float)rec.Volume;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bottled";
                                part4Obj.AlcoholNeutral = (float)rec.Volume;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                        else if (rec.ProcessingType == "BLENDED STRAIGHT WHISKEY")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.BlendedStraightWhiskey += (float)rec.Volume;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bottled";
                                part4Obj.BlendedStraightWhiskey = (float)rec.Proof;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                        else if (rec.ProcessingType == "BLENDED WHISKEY: With neutral spirits")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.BlendedWhiskeyWithNeutral += (float)rec.Volume;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bottled";
                                part4Obj.BlendedWhiskeyWithNeutral = (float)rec.Volume;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                        else if (rec.ProcessingType == "BLENDED WHISKEY: With light whiskey")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.BlendedWhiskeyWithLight += (float)rec.Volume;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bottled";
                                part4Obj.BlendedWhiskeyWithLight = (float)rec.Volume;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                        else if (rec.ProcessingType == "BLENDED LIGHT WHISKEY")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.BlendedLightWhiskey += (float)rec.Volume;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bottled";
                                part4Obj.BlendedLightWhiskey = (float)rec.Volume;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                        else if (rec.ProcessingType == "ANY OTHER BLENDS OF 100% WHISKEY")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.BlendedOtherWhiskey += (float)rec.Volume;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bottled";
                                part4Obj.BlendedOtherWhiskey = (float)rec.Volume;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                        else if (rec.ProcessingType == "IMPORTED WHISKEY: Scotch")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.ImportedWhiskeyScotch += (float)rec.Volume;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bottled";
                                part4Obj.ImportedWhiskeyScotch = (float)rec.Volume;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                        else if (rec.ProcessingType == "IMPORTED WHISKEY: Canadian")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.ImportedWhiskeyCanadian += (float)rec.Volume;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bottled";
                                part4Obj.ImportedWhiskeyCanadian = (float)rec.Volume;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                        else if (rec.ProcessingType == "IMPORTED WHISKEY: Irish and Others")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.ImportedWhiskeyIrish += (float)rec.Volume;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bottled";
                                part4Obj.ImportedWhiskeyIrish = (float)rec.Volume;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                        else if (rec.ProcessingType == "DOMESTIC WHISKEY DISTILLED AT 160 AND UNDER")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.DomesticWhiskey160Under += (float)rec.Volume;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bottled";
                                part4Obj.DomesticWhiskey160Under = (float)rec.Volume;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                        else if (rec.ProcessingType == " DOMESTIC WHISKEY DISTILLED AT OVER 160")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.DomesticWhiskeyOver160 += (float)rec.Volume;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bottled";
                                part4Obj.DomesticWhiskeyOver160 = (float)rec.Volume;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                        else if (rec.ProcessingType == "BRANDY DISTILLED AT 170 AND UNDER")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.Brandy170Under += (float)rec.Volume;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bottled";
                                part4Obj.Brandy170Under = (float)rec.Volume;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                        else if (rec.ProcessingType == "BRANDY DISTILLED AT OVER 170")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.BrandyOver170 += (float)rec.Volume;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bottled";
                                part4Obj.BrandyOver170 = (float)rec.Volume;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                        else if (rec.ProcessingType == "SPIRITS (Rum):  Puerto Rican")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.RumPuertoRican += (float)rec.Volume;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bottled";
                                part4Obj.RumPuertoRican = (float)rec.Volume;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                        else if (rec.ProcessingType == "SPIRITS (Rum): Virgin Islands")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.RumVirginIslands += (float)rec.Volume;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bottled";
                                part4Obj.RumVirginIslands = (float)rec.Volume;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                        else if (rec.ProcessingType == "RUM: Domestic")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.RumDomestic += (float)rec.Volume;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bottled";
                                part4Obj.RumDomestic = (float)rec.Volume;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                        else if (rec.ProcessingType == "RUM: \"Other\" Imported")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.RumOtherImported += (float)rec.Volume;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bottled";
                                part4Obj.RumOtherImported = (float)rec.Volume;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                        else if (rec.ProcessingType == "GIN")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.Gin += (float)rec.Volume;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bottled";
                                part4Obj.Gin = (float)rec.Volume;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                        else if (rec.ProcessingType == "VODKA")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.Vodka += (float)rec.Volume;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bottled";
                                part4Obj.Vodka = (float)rec.Volume;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                        else if (rec.ProcessingType == "CORDIALS, LIQUEURS, AND SPECIALTIES")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.Liqueur += (float)rec.Volume;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bottled";
                                part4Obj.Liqueur = (float)rec.Volume;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                        else if (rec.ProcessingType == "COCKTAILS AND MIXED DRINKS")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.Cocktail += (float)rec.Volume;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bottled";
                                part4Obj.Cocktail = (float)rec.Volume;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                        else if (rec.ProcessingType == "TEQUILA")
                        {
                            var record = procRepP4L.Find(x => x.ProcessingTypeID == rec.ProcessingTypeID);
                            if (record != null && record.StateID == rec.StateID)
                            {
                                record.Tequila += (float)rec.Volume;
                                isInProcRepList = true;
                            }
                            else
                            {
                                part4Obj.ProcessingSpirits = "bottled";
                                part4Obj.Tequila = (float)rec.Volume;
                                part4Obj.ProcessingTypeID = (int)rec.ProcessingTypeID;
                            }
                        }
                    }

                    if (!isInProcRepList)
                    {
                        part4Obj.StateID = rec.StateID;
                        procRepP4L.Add(part4Obj);
                    }
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        /// <summary>
        /// Line 33 gets value for Bottled/Packaged spirits sold.
        /// Used for tax calcualtions.
        /// </summary>
        /// <param name="startOfReporting"></param>
        /// <param name="endOfReporting"></param>
        /// <param name="procRepP2"></param>
        private void WithdrawnTaxDetermined(DateTime startOfReporting, DateTime endOfReporting, ref ProcessReportingPart2 procRepP2, int userId)
        {
            var taxWithdrawn =
                from tax in _db.TaxWithdrawn
                join productions in _db.Production on tax.ProductionID equals productions.ProductionID into productions_join
                from productions in productions_join.DefaultIfEmpty()
                join distillers in _db.AspNetUserToDistiller on productions.DistillerID equals distillers.DistillerID into distillers_join
                from distillers in distillers_join.DefaultIfEmpty()
                where tax.DateOfSale >= startOfReporting
                && tax.DateOfSale <= endOfReporting
                && distillers.UserId == userId
                select new
                {
                    TaxPaid = (System.Single?)tax.Value ?? (System.Single?)0
                };

            if (taxWithdrawn != null)
            {
                foreach (var i in taxWithdrawn)
                {
                    procRepP2.TaxWithdrawn += (float)i.TaxPaid;
                }
            }
            // round to 3 decimals
            Math.Round(procRepP2.TaxWithdrawn, 3);
        }

        /// <summary>
        /// Gets OnHandEndOfMonth in Part 2 of processing report.
        /// TODO: we can combine it with Part 1 method perhaps, by providing extra
        /// parameter called StateID which will filter Bottled vs Bulk spirits.
        /// </summary>
        /// <param name="endOfReporting"></param>
        /// <param name="userId"></param>
        /// <param name="procRepP2"></param>
        private void OnHandEndOfMonthPart2(DateTime endOfReporting, int userId, ref ProcessReportingPart2 procRepP2)
        {
            procRepP2.OnHandEndofMonth = procRepP2.TotalLine31 - procRepP2.TaxWithdrawn; // grisha:todo: as we continue on adding new cases for rows 32 through 45 we need to keep subtracting it
            // round to 3 decimals
            Math.Round(procRepP2.OnHandEndofMonth, 3);
        }

        /// <summary>
        /// Part 2 of Processing report which gives out the same output as BottledOrPackaged method but 
        /// the difference is in the 3rd argument. ProcessingReportPart2 in this case versus ProcessingReportPart1 in 
        /// another case. Keeping two separate methods for now and will revisit once we will access existing architecture 
        /// of data types
        /// </summary>
        /// <param name="startOfReporting"></param>
        /// <param name="endOfReporting"></param>
        /// <param name="userId">userId is used to get DistillerID so we can pull correct data</param>
        /// <param name="procRepP2"></param>
        private void BottledOrPackagedpart2(DateTime startOfReporting, DateTime endOfReporting, int userId, ref ProcessReportingPart2 procRepP2)
        {
            var bottledPackagedp2 =
                            (from prod in
                                (from prod in _db.Production
                                 join prod4Rep in _db.Production4Reporting on prod.ProductionID equals prod4Rep.ProductionID into prod4Rep_join
                                 from prod4Rep in prod4Rep_join.DefaultIfEmpty()
                                 join distillers in _db.AspNetUserToDistiller on prod.DistillerID equals distillers.DistillerID into distillers_join
                                 from distillers in distillers_join.DefaultIfEmpty()
                                 where
                                    distillers.UserId == userId &&
                                    prod.Gauged == true &&
                                    prod.ProductionEndTime >= startOfReporting &&
                                    prod.ProductionEndTime <= endOfReporting &&
                                    (prod.StatusID == (int)Persistence.BusinessLogicEnums.Status.Active ||
                                    prod.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processing) &&
                                    prod.StateID == (int)Persistence.BusinessLogicEnums.State.Bottled
                                 select new
                                 {
                                     Value = (System.Single?)prod4Rep.Proof ?? (System.Single?)0,
                                     Dummy = "x"
                                 })
                             group prod by new { prod.Dummy } into g
                             select new
                             {
                                 BottledPackagedBottled = g.Sum(p => p.Value)
                             }).FirstOrDefault();

            if (bottledPackagedp2 != null)
            {
                procRepP2.AmtBottledPackaged = (float)bottledPackagedp2.BottledPackagedBottled;
            }
            // round to 3 decimals
            Math.Round(procRepP2.AmtBottledPackaged, 3);
        }

        /// <summary>
        /// Get OnHandFirstOfMonth Bottled or Packaged goods. Line 27 of Processing report
        /// </summary>
        /// <param name="startOfReporting"></param>
        /// <param name="userId"> userID is used to get DistillerID for which we are quering the reports</param>
        /// <param name="procRepP2"></param>
        private void OnHandFirstOfMonthBottled(DateTime startOfReporting, int userId, ref ProcessReportingPart2 procRepP2)
        {
            var p2OnHand2stMo =
                            (from prod in
                                (from prod in _db.Production
                                 join proof in _db.Proof on prod.ProofID equals proof.ProofID into proof_join
                                 from proof in proof_join.DefaultIfEmpty()
                                 join distillers in _db.AspNetUserToDistiller on prod.DistillerID equals distillers.DistillerID into distillers_join
                                 from distillers in distillers_join.DefaultIfEmpty()
                                 where
                                   distillers.UserId == userId &&
                                   prod.ProductionTypeID == (int)Persistence.BusinessLogicEnums.ProductionType.Bottling &&
                                   prod.Gauged == true &&
                                   prod.ProductionEndTime < startOfReporting &&
                                   (prod.StatusID == (int)Persistence.BusinessLogicEnums.Status.Active ||
                                   prod.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processing ||
                                   prod.StateID == (int)Persistence.BusinessLogicEnums.State.Bottled)
                                 select new
                                 {
                                     Value = (System.Single?)proof.Value ?? (System.Single?)0,
                                     Dummy = "x"
                                 })
                             group prod by new { prod.Dummy } into g
                             select new
                             {
                                 OnHandFirstOfMonthBottled = g.Sum(p => p.Value)
                             }).FirstOrDefault();

            if (p2OnHand2stMo != null)
            {
                procRepP2.OnHandFirstofMonth = (float)p2OnHand2stMo.OnHandFirstOfMonthBottled;
            }
            // round to 3 decimals
            Math.Round(procRepP2.OnHandFirstofMonth, 3);
        }

        /// <summary>
        /// Line 25 of Processing Report
        /// </summary>
        /// <param name="procRepP1"></param>
        /// <param name="line8RunningSum"></param>
        /// <param name="line26RunningSum"></param>
        private void OnHandEndOfMonth(ref ProcessReportingPart1 procRepP1, float line8RunningSum, float line26RunningSum)
        {
            procRepP1.OnHandEndofMonth = (float)Math.Round(Convert.ToDouble(line8RunningSum - line26RunningSum), 3);
        }

        /// <summary>
        /// Line 24 of Processing Report
        /// </summary>
        /// <param name="startOfReporting"></param>
        /// <param name="endOfReporting"></param>
        /// <param name="userId"></param>
        /// <param name="procRepP1"></param>
        private void Losses(DateTime startOfReporting, DateTime endOfReporting, int userId, ref ProcessReportingPart1 procRepP1)
        {
            var accumulatedLoss =
            (from prod in
            (from prod in _db.Production
                join gl in _db.GainLoss on new { ProductionID = prod.ProductionID } equals new { ProductionID = gl.BottledRecordId } into gl_join
                from gl in gl_join.DefaultIfEmpty()
                join distillers in _db.AspNetUserToDistiller on prod.DistillerID equals distillers.DistillerID into distillers_join
                from distillers in distillers_join.DefaultIfEmpty()
                where
                distillers.UserId == userId &&
                prod.Gauged == true &&
                prod.StateID == (int)Persistence.BusinessLogicEnums.State.Bottled
                && prod.ProductionEndTime >= startOfReporting
                && prod.ProductionEndTime <= endOfReporting
                select new
                {
                    Quantity = (System.Single?)gl.Quantity ?? (System.Single?)0,
                    Dummy = "x"
                })
                group prod by new { prod.Dummy } into g
                select new
                {
                    Losses = g.Sum(p => p.Quantity) ?? 0
                }).FirstOrDefault();

            if (accumulatedLoss != null)
            {
                procRepP1.Losses = accumulatedLoss.Losses;
            }
            // round to 3 decimals
            Math.Round(procRepP1.Losses, 3);
        }

        /// <summary>
        /// Lines 9 and 28 of Processing Report
        /// </summary>
        /// <param name="startOfReporting"></param>
        /// <param name="endOfReporting"></param>
        /// <param name="userId"></param>
        /// <param name="procRepP1"></param>
        private void BottledOrPackaged(DateTime startOfReporting, DateTime endOfReporting, int userId, ref ProcessReportingPart1 procRepP1)
        {
            var bottledPackaged =
                (from prod in
                (from prod in _db.Production
                 join prod4Rep in _db.Production4Reporting on prod.ProductionID equals prod4Rep.ProductionID into prod4Rep_join
                 from prod4Rep in prod4Rep_join.DefaultIfEmpty()
                 join distillers in _db.AspNetUserToDistiller on prod.DistillerID equals distillers.DistillerID into distillers_join
                 from distillers in distillers_join.DefaultIfEmpty()
                 where
                   distillers.UserId == userId &&
                   prod.Gauged == true &&
                   prod.StateID == (int)Persistence.BusinessLogicEnums.State.Bottled &&
                   (prod.StatusID == (int)Persistence.BusinessLogicEnums.Status.Active ||
                   prod.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processing) &&
                   prod.ProductionEndTime >= startOfReporting &&
                   prod.ProductionEndTime <= endOfReporting
                 select new
                 {
                     Value = (System.Single?)prod4Rep.Proof ?? (System.Single?)0,
                     Dummy = "x"
                 })
                 group prod by new { prod.Dummy } into g
                 select new
                 {
                     BottledPackagedBulk = g.Sum(p => p.Value)
                 }).FirstOrDefault();

            if (bottledPackaged != null)
            {
                procRepP1.AmtBottledPackaged = (float)bottledPackaged.BottledPackagedBulk;
            }
            // round to 3 decimals
            Math.Round(procRepP1.AmtBottledPackaged, 3);
        }

        /// <summary>
        /// Received Line 2 of Processing Report
        /// </summary>
        /// <param name="startOfReporting"></param>
        /// <param name="endOfReporting"></param>
        /// <param name="userId"></param>
        /// <param name="procRepP1"></param>
        private void Received(DateTime startOfReporting, DateTime endOfReporting, int userId, ref ProcessReportingPart1 procRepP1)
        {
            var recBulk =
            (from prod in
                (from prod in _db.Production
                 join prod4Rep in _db.Production4Reporting on prod.ProductionID equals prod4Rep.ProductionID into prod4Rep_join
                 from prod4Rep in prod4Rep_join.DefaultIfEmpty()
                 join distillers in _db.AspNetUserToDistiller on prod.DistillerID equals distillers.DistillerID into distillers_join
                 from distillers in distillers_join.DefaultIfEmpty()
                 where
                   distillers.UserId == userId &&
                   prod.Gauged == true &&
                   prod.StateID == (int)Persistence.BusinessLogicEnums.State.Blended &&
                   prod.ProductionEndTime >= startOfReporting &&
                   prod.ProductionEndTime <= endOfReporting
                 select new
                 {
                     Value = (System.Single?)prod4Rep.Proof ?? (System.Single?)0,
                     Dummy = "x"
                 })
             group prod by new { prod.Dummy } into g
             select new
             {
                 ReceivedBulk = g.Sum(p => p.Value)
             }).FirstOrDefault();

            if (recBulk != null)
            {
                procRepP1.Recd4Process = (float)recBulk.ReceivedBulk;
            }

            // round to 3 decimals
            Math.Round(procRepP1.Recd4Process, 3);
        }

        /// <summary>
        /// Line 1 of Processing Report
        /// row of Processing report
        /// </summary>
        /// <param name="startOfReporting"></param>
        /// <param name="userId"></param>
        /// <param name="procRepP1"></param>
        private void OnHandFirstOfMonth(DateTime startOfReporting, int userId, ref ProcessReportingPart1 procRepP1)
        {
            var onHands1stMoC =
                (from prod in _db.Production
                 join productionContent in _db.ProductionContent on prod.ProductionID equals productionContent.RecordID into productionContent_join
                 from productionContent in productionContent_join.DefaultIfEmpty()
                 join outputProduction in _db.Production on productionContent.ProductionID equals outputProduction.ProductionID into outputProduction_join
                 from outputProduction in outputProduction_join.DefaultIfEmpty()
                 join proof in _db.Proof on prod.ProofID equals proof.ProofID into proof_join
                 from proof in proof_join.DefaultIfEmpty()
                 join prod4Rep in _db.Production4Reporting on prod.ProductionID equals prod4Rep.ProductionID into prod4Rep_join
                 from prod4Rep in prod4Rep_join.DefaultIfEmpty()
                 join distillers in _db.AspNetUserToDistiller on prod.DistillerID equals distillers.DistillerID into distillers_join
                 from distillers in distillers_join.DefaultIfEmpty()
                 where
                 distillers.UserId == userId &&
                 prod.Gauged == true &&
                 prod.ProductionEndTime < startOfReporting &&
                 prod.StateID == (int)Persistence.BusinessLogicEnums.State.Blended
                 && (prod.StatusID == (int)Persistence.BusinessLogicEnums.Status.Active
                 || prod.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processing
                 || (prod.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processed && outputProduction.ProductionEndTime > startOfReporting && productionContent.ContentFieldID == (int)Persistence.BusinessLogicEnums.ContenField.ProdBlendedProofGal))
                 select new
                 {
                     Value = (System.Single?)prod4Rep.Proof ?? (System.Single?)0
                 }).ToList();

            if (onHands1stMoC != null)
            {
                foreach (var i in onHands1stMoC)
                {
                    procRepP1.OnHandFirstofMonth += (float)i.Value;
                }

                // round to 3 decimals
                Math.Round(procRepP1.OnHandFirstofMonth, 3);
            }
        }

        private void Round(ref ProcessingReportingObject report)
        {
            try
            {
                // Sum up values for part of the processing report
                report.Part1.OnHandEndofMonth = (float)Math.Round(report.Part1.OnHandEndofMonth, 3);
                report.Part1.Recd4Process = (float)Math.Round(report.Part1.Recd4Process, 3);
                report.Part1.WineMixedWithSpirit = (float)Math.Round(report.Part1.WineMixedWithSpirit, 3);
                report.Part1.Dumped4Processing = (float)Math.Round(report.Part1.Dumped4Processing, 3);
                report.Part1.Gains = (float)Math.Round(report.Part1.Gains, 3);
                report.Part1.AmtBottledPackaged = (float)Math.Round(report.Part1.AmtBottledPackaged, 3);
                report.Part1.Transf2Prod4Redistil = (float)Math.Round(report.Part1.Transf2Prod4Redistil, 3);
                report.Part1.Destroyed = (float)Math.Round(report.Part1.Destroyed, 3);
                report.Part1.Used4Redistil = (float)Math.Round(report.Part1.Used4Redistil, 3);
                report.Part1.Losses = (float)Math.Round(report.Part1.Losses, 3);
                report.Part1.OnHandEndofMonth = (float)Math.Round(report.Part1.OnHandEndofMonth, 3);

                report.Part2.OnHandFirstofMonth = (float)Math.Round(report.Part2.OnHandFirstofMonth, 3);
                report.Part2.AmtBottledPackaged = (float)Math.Round(report.Part2.AmtBottledPackaged, 3);
                report.Part2.Recd4Process = (float)Math.Round(report.Part2.Recd4Process, 3);
                report.Part2.InventoryOverage = (float)Math.Round(report.Part2.InventoryOverage, 3);
                report.Part2.Transf2Prod4Redistil = (float)Math.Round(report.Part2.Transf2Prod4Redistil, 3);
                report.Part2.Destroyed = (float)Math.Round(report.Part2.Destroyed, 3);
                report.Part2.Dumped4Processing = (float)Math.Round(report.Part2.Dumped4Processing, 3);
                report.Part2.RecordedLosses = (float)Math.Round(report.Part2.RecordedLosses, 3);
                report.Part2.InventoryShortage = (float)Math.Round(report.Part2.InventoryShortage, 3);
                report.Part2.OnHandEndofMonth = (float)Math.Round(report.Part2.OnHandEndofMonth, 3);
                report.Part2.TaxWithdrawn = (float)Math.Round(report.Part2.TaxWithdrawn, 3);

                foreach(var i in report.Part4List)
                {
                    i.AlcoholNeutral = (float)Math.Round(i.AlcoholNeutral, 3);
                    i.BlendedStraightWhiskey = (float)Math.Round(i.BlendedStraightWhiskey, 3);
                    i.BlendedWhiskeyWithNeutral = (float)Math.Round(i.BlendedWhiskeyWithNeutral, 3);
                    i.BlendedWhiskeyWithLight = (float)Math.Round(i.BlendedWhiskeyWithLight, 3);
                    i.BlendedLightWhiskey = (float)Math.Round(i.BlendedLightWhiskey, 3);
                    i.BlendedOtherWhiskey = (float)Math.Round(i.BlendedOtherWhiskey, 3);
                    i.ImportedWhiskeyScotch = (float)Math.Round(i.ImportedWhiskeyScotch, 3);
                    i.ImportedWhiskeyCanadian = (float)Math.Round(i.ImportedWhiskeyCanadian, 3);
                    i.ImportedWhiskeyIrish = (float)Math.Round(i.ImportedWhiskeyIrish, 3);
                    i.DomesticWhiskey160Under = (float)Math.Round(i.DomesticWhiskey160Under, 3);
                    i.DomesticWhiskeyOver160 = (float)Math.Round(i.DomesticWhiskeyOver160, 3);
                    i.Brandy170Under = (float)Math.Round(i.Brandy170Under, 3);
                    i.BrandyOver170 = (float)Math.Round(i.BrandyOver170, 3);
                    i.RumPuertoRican = (float)Math.Round(i.RumPuertoRican, 3);
                    i.RumVirginIslands = (float)Math.Round(i.RumVirginIslands, 3);
                    i.RumDomestic = (float)Math.Round(i.RumDomestic, 3);
                    i.RumOtherImported = (float)Math.Round(i.RumOtherImported, 3);
                    i.Gin = (float)Math.Round(i.Gin, 3);
                    i.Vodka = (float)Math.Round(i.Vodka, 3);
                    i.Liqueur = (float)Math.Round(i.Liqueur, 3);
                    i.Cocktail = (float)Math.Round(i.Cocktail, 3);
                    i.Tequila = (float)Math.Round(i.Tequila, 3);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}