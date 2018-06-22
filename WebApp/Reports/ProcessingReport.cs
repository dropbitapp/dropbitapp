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
            }

            line8RunningSum += (float)procRepP1.OnHandFirstofMonth;

            // 2(c) current month received bulk
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

            line8RunningSum += (float)procRepP1.Recd4Process;

            // 9 (c) Bottled or Packaged
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

            line26RunningSum = (float)procRepP1.AmtBottledPackaged;

            // 24 (c) Losses
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

            line26RunningSum += procRepP1.Losses;

            // Round to three decimals
            line26RunningSum = (float)Math.Round(line26RunningSum, 3);

            if ((line8RunningSum - line26RunningSum) < 0)
            {
                throw new InvalidOperationException();
            }

            // 25(c) On hand end of month
            procRepP1.OnHandEndofMonth = (float)Math.Round(Convert.ToDouble(line8RunningSum - line26RunningSum), 3);

            // Processing Report Part 2 Section
            // Bottled Column(b)
            procRepP2.FinishedProduct = "bottled";

            // 27(c) previous month
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

            // 28(b) Bottled or Packaged
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

            // 46 (b) On hand End of Month
            var onHandEndOfMonthP2 =
             (from prod in
                    (from prod in _db.Production
                     join proof in _db.Proof on prod.ProofID equals proof.ProofID into proof_join
                     from proof in proof_join.DefaultIfEmpty()
                     join distillers in _db.AspNetUserToDistiller on prod.DistillerID equals distillers.DistillerID into distillers_join
                     from distillers in distillers_join.DefaultIfEmpty()
                     where
                       distillers.UserId == userId &&
                       prod.ProductionTypeID == (int)Persistence.BusinessLogicEnums.ProductionType.Bottling &&
                       prod.ProductionEndTime <= endOfReporting &&
                       prod.Gauged == true &&
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
                  OnHandEndOfMonth = g.Sum(p => p.Value)
              }).FirstOrDefault();

            if (onHandEndOfMonthP2 != null)
            {
                procRepP2.OnHandEndofMonth = (float)onHandEndOfMonthP2.OnHandEndOfMonth;
            }

            // line 33 - Withdrawn for Tax Determined
            var taxWithdrawn =
            from tax in _db.TaxWithdrawn
            where tax.DateOfSale >= startOfReporting && tax.DateOfSale <= endOfReporting
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

            // Processing Report Part 4
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

            procRepObj.Part1 = procRepP1;
            procRepObj.Part2 = procRepP2;
            procRepObj.Part4List = procRepP4L;
            return procRepObj;
        }
    }
}