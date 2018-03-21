using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using WebApp.Models;
using WebApp.Persistence.BusinessLogicEnums;
using WebApp.Persistence.Repositories;
using WebApp.ReportDTO;


namespace WebApp.Helpers
{
    /// <summary>
    /// TypeIdentifer keeps mapping of TableType ids and associated Table Types,
    /// FYI - when updating this enm please be sure to update TableTypeIdentifier table in the db
    /// </summary>
    public enum TypeIdentifier
    {
        Default, // 0
        /*Dictionary Types*/
        RawMaterial,
        Supply,
        Vendor,
        Additive,
        Storage,
        Spirit,
        /*Purchase Types*/
        FermentablePur,
        Distillable,
        FermentedPur,
        DistilledPur,
        SupplyPur,
        AdditivePur,
        /*Production Types*/
        FermentableProd,
        FermentedProd,
        DistilledProd,
        BlendingProd,
        BlendedComponentProd,
        BottlingProd
    }

    public enum TableTypeIdentifer
    {
        Default,
        RawMaterial,
    }

    public enum RecordType
    {
        Purchase = 1,
        Production
    }

    /// <summary>
    /// ProductionContentIterator class is used for tracking when we iterate through 
    /// ProductionContent table to get all relevant records for given ProductionID
    /// </summary>
    public class ProductionContentIterator
    {
        public int ProductionContentId { get; set; } // ProductionContentID
        public int ProductionId { get; set; } // ProductionID
        public int RecordId { get; set; } // RecordID 
        public bool IsProductionComponent { get; set; } // IsProduction
        public float Proof { get; set; }
    }

    /// <summary>
    /// This object is needed as a temporary storage to keep initial query results for 
    /// production report. Not ideal but since I can't(not can't - don't want to waste my time on this) come up with a decent query that 
    /// will gather all necessary information in a single pass, I have to do that.
    /// </summary>
    public class ProductionReportHelper
    {
        public int StateID { get; set; }
        public int ProductionID { get; set; }
        public string SpiritTypeReportName { get; set; }
        public bool Redistilled { get; set; }
        public string MaterialKindReportingName { get; set; }
        public float Weight { get; set; }
        public float Volume { get; set; }
        public float Alcohol { get; set; }
        public float Proof { get; set; }
        public int SpiritTypeReportingID { get; set; }
        public int MaterialKindReportingID { get; set; }
        public bool Gauged { get; internal set; }
        public float Recd4RedistilaltionL15 { get; set; }
    }

    public class DataLayer
    {
        // Initialize Model Object so we can interact with it
        private DistilDBContext db = new DistilDBContext();

        #region Shared Methods

        /// <summary>
        /// Generate gauge serial number starting with 1 at the beginning of the year.
        /// </summary>
        /// <param name="recordId"></param>
        /// <param name="recordType"></param>
        public void GenerateGaugeSerial(int recordId, int recordType)
        {
            string newSerial = string.Empty;
            var currentYear = DateTime.Now.Year;

            // Find last GaugeSerial record and get it's serial string
            // Sample serial string format: 12017, 23452017(last four digits represent a year)
            var last = (from rec in db.GaugeSerial
                        orderby rec.GaugeSerialID descending
                        select rec.Serial).FirstOrDefault();

            if (last != null)
            {
                // Parse serial and year from serial string
                int serial;
                short year;
                bool isSerial = int.TryParse((last.Substring(0, last.Length - 4)), out serial);
                bool isYear = short.TryParse((last.Substring(last.Length - 4, 4)), out year);

                if (isSerial && isYear)
                {
                    // If the last record in table was created in the last year, reset new serial to 1
                    if (year < currentYear)
                    {
                        newSerial = "1" + currentYear.ToString();
                    }
                    // If the last record in table was created in the same year, increment new serial by 1
                    else if (year == currentYear)
                    {
                        // Increment last serial before generating new serial string
                        newSerial = (++serial) + currentYear.ToString();
                    }
                }
                else
                {
                    throw new Exception("TryParse failed to parse serial.");
                }
            }
            else // Create a new serial record if table is empty, start serial at 1
            {
                newSerial = "1" + currentYear.ToString();
            }

            // Create new gauge serial database record
            GaugeSerial serialRec = new GaugeSerial();
            serialRec.RecordID = recordId;
            serialRec.RecordType = recordType;
            serialRec.Serial = newSerial;
            db.GaugeSerial.Add(serialRec);
            db.SaveChanges();
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
            // Persistent Reporting: get persistent report storage data.
            ReportRepository reportRepository = new ReportRepository();
            ReportData reportData = new ReportData();
            reportData = reportRepository.GetPersistentStorageReportData(endOfReporting, userId);
            return reportData;
        }

        /// <summary>
        /// Get gauge serial number records falling within specified time period.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public List<GaugeSerial> GetGaugeSerial(DateTime start, DateTime end)
        {
            List<GaugeSerial> records;

            // Find all serial records that are associated with purchase/production record whose date falls within supplied range
            var res = (from rec in db.GaugeSerial
                       join production in db.Production on rec.RecordID equals production.ProductionID into production_join
                       from production in production_join.DefaultIfEmpty()
                       join purchase in db.Purchase on rec.RecordID equals purchase.PurchaseID into purchase_join
                       from purchase in purchase_join.DefaultIfEmpty()
                       where (rec.RecordType == 1 &&
                             purchase.PurchaseDate > start &&
                             purchase.PurchaseDate < end)
                             || (rec.RecordType == 2 &&
                             production.ProductionDate > start &&
                             production.ProductionDate < end)
                       select rec).DefaultIfEmpty();

            if (res != null)
            {
                records = new List<GaugeSerial>(res.Count());
                records.AddRange(res);
            }
            else
            {
                records = null;
            }
            return records;
        }

        /// <summary>
        /// Get gauge serial number record matching supplied recordId and recordType.
        /// </summary>
        /// <param name="recordId"></param>
        /// <param name="recordType"></param>
        public int GetGaugeSerial(int recordId, int recordType)
        {
            var res = (from rec in db.GaugeSerial
                       where rec.RecordID == recordId &&
                             rec.RecordType == recordType
                       select rec.Serial).FirstOrDefault();

            if (res != null)
            {
                return int.Parse(res);
            }
            else
            {
                return -1;
            }
        }

        // here we are keeping methods that are used through out the application workflows. 
        public List<PurMatObject> GetRawMaterialList4Fermentation(int userId)
        {
            List<PurMatObject> rawMList = new List<PurMatObject>();

            var fermentables =
                from purch in db.Purchase
                join quant in db.Volume on purch.VolumeID equals quant.VolumeID into quant_join
                from quant in quant_join.DefaultIfEmpty()
                join vbw in db.Weight on purch.WeightID equals vbw.WeightID into vbw_join
                from vbw in vbw_join.DefaultIfEmpty()
                join matDic in db.MaterialDict on purch.MaterialDictID equals matDic.MaterialDictID into matDic_join
                from matDic in matDic_join.DefaultIfEmpty()
                join distiller in db.AspNetUserToDistiller on new { DistillerID = purch.DistillerID } equals new { DistillerID = distiller.DistillerID } into distiller_join
                from distiller in distiller_join.DefaultIfEmpty()
                where
                  (purch.StatusID == (int)Persistence.BusinessLogicEnums.Status.Active ||
                  purch.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processing) &&
                  purch.StateID == (int)Persistence.BusinessLogicEnums.State.Fermentable &&
                  distiller.UserId == userId
                select new
                {
                    PurchaseID = ((System.Int32?)purch.PurchaseID ?? (System.Int32?)0),
                    PurchaseBatchName = purch.PurchaseName ?? string.Empty,
                    PurchaseDate = purch.PurchaseDate,
                    StatusID = ((System.Int32?)purch.StatusID ?? (System.Int32?)0),
                    RawMaterialName = matDic.Name ?? string.Empty,
                    MaterialDictID = ((System.Int32?)matDic.MaterialDictID ?? (System.Int32?)0),
                    Quantity = ((System.Single?)quant.Value ?? (System.Single?)0),
                    VolumeByWeight = ((System.Single?)vbw.Value ?? (System.Single?)0),
                    BurningDownMethod = purch.BurningDownMethod ?? null
                };

            if (fermentables != null)
            {
                foreach (var i in fermentables)
                {
                    PurMatObject obj = new PurMatObject();
                    obj.PurchaseId = (int)i.PurchaseID;
                    obj.PurchaseBatchName = i.PurchaseBatchName;
                    obj.PurchaseDate = i.PurchaseDate;
                    obj.RawMaterialId = (int)i.MaterialDictID;
                    obj.MaterialName = i.RawMaterialName;
                    obj.QtyGal = (float)i.Quantity;
                    obj.VBW = (float)i.VolumeByWeight;
                    obj.BurningDownMethod = i.BurningDownMethod;
                    obj.DistillableOrigin = "pur";
                    rawMList.Add(obj);
                }
            }

            return rawMList;
        }

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
            int distillerID = GetDistillerId(userId);
            procRepObj.Header = GetDistillerInfoForReportHeader(distillerID, startOfReporting);

            // Processing Report Part 1 Section
            procRepP1.BulkIngredients = "spirit";

            // 1(c) previous month
            var onHands1stMoC =
                (from prod in db.Production
                 join productionContent in db.ProductionContent on prod.ProductionID equals productionContent.RecordID into productionContent_join
                 from productionContent in productionContent_join.DefaultIfEmpty()
                 join outputProduction in db.Production on productionContent.ProductionID equals outputProduction.ProductionID into outputProduction_join
                 from outputProduction in outputProduction_join.DefaultIfEmpty()
                 join proof in db.Proof on prod.ProofID equals proof.ProofID into proof_join
                 from proof in proof_join.DefaultIfEmpty()
                 join prod4Rep in db.Production4Reporting on prod.ProductionID equals prod4Rep.ProductionID into prod4Rep_join
                 from prod4Rep in prod4Rep_join.DefaultIfEmpty()
                 join distillers in db.AspNetUserToDistiller on prod.DistillerID equals distillers.DistillerID into distillers_join
                 from distillers in distillers_join.DefaultIfEmpty()
                 where
                 distillers.UserId == userId &&
                 prod.Gauged == true &&
                 prod.ProductionEndTime < startOfReporting &&
                 prod.StateID == (int)Persistence.BusinessLogicEnums.State.Blended
                 && (prod.StatusID == (int)Persistence.BusinessLogicEnums.Status.Active
                 || prod.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processing
                 || (prod.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processed && outputProduction.ProductionEndTime > startOfReporting && productionContent.ContentFieldID == 22))
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
                (from prod in db.Production
                 join prod4Rep in db.Production4Reporting on prod.ProductionID equals prod4Rep.ProductionID into prod4Rep_join
                 from prod4Rep in prod4Rep_join.DefaultIfEmpty()
                 join distillers in db.AspNetUserToDistiller on prod.DistillerID equals distillers.DistillerID into distillers_join
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
                (from prod in db.Production
                 join prod4Rep in db.Production4Reporting on prod.ProductionID equals prod4Rep.ProductionID into prod4Rep_join
                 from prod4Rep in prod4Rep_join.DefaultIfEmpty()
                 join distillers in db.AspNetUserToDistiller on prod.DistillerID equals distillers.DistillerID into distillers_join
                 from distillers in distillers_join.DefaultIfEmpty()
                 where
                   distillers.UserId == userId &&
                   prod.Gauged == true &&
                   prod.StateID == 5 &&
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
                (from prod in db.Production
                 join gl in db.GainLoss on new { ProductionID = prod.ProductionID } equals new { ProductionID = gl.BottledRecordId } into gl_join
                 from gl in gl_join.DefaultIfEmpty()
                 join distillers in db.AspNetUserToDistiller on prod.DistillerID equals distillers.DistillerID into distillers_join
                 from distillers in distillers_join.DefaultIfEmpty()
                 where
                 distillers.UserId == userId &&
                 prod.Gauged == true &&
                 prod.StateID == 5
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
                    (from prod in db.Production
                     join proof in db.Proof on prod.ProofID equals proof.ProofID into proof_join
                     from proof in proof_join.DefaultIfEmpty()
                     join distillers in db.AspNetUserToDistiller on prod.DistillerID equals distillers.DistillerID into distillers_join
                     from distillers in distillers_join.DefaultIfEmpty()
                     where
                       distillers.UserId == userId &&
                       prod.ProductionTypeID == 4 &&
                       prod.Gauged == true &&
                       prod.ProductionEndTime < startOfReporting &&
                       (prod.StatusID == (int)Persistence.BusinessLogicEnums.Status.Active ||
                       prod.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processing ||
                       prod.StateID == 5)
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
                    (from prod in db.Production
                     join prod4Rep in db.Production4Reporting on prod.ProductionID equals prod4Rep.ProductionID into prod4Rep_join
                     from prod4Rep in prod4Rep_join.DefaultIfEmpty()
                     join distillers in db.AspNetUserToDistiller on prod.DistillerID equals distillers.DistillerID into distillers_join
                     from distillers in distillers_join.DefaultIfEmpty()
                     where
                        distillers.UserId == userId &&
                        prod.Gauged == true &&
                        prod.ProductionEndTime >= startOfReporting &&
                        prod.ProductionEndTime <= endOfReporting &&
                        (prod.StatusID == (int)Persistence.BusinessLogicEnums.Status.Active ||
                        prod.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processing) &&
                        prod.StateID == 5
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
                    (from prod in db.Production
                     join proof in db.Proof on prod.ProofID equals proof.ProofID into proof_join
                     from proof in proof_join.DefaultIfEmpty()
                     join distillers in db.AspNetUserToDistiller on prod.DistillerID equals distillers.DistillerID into distillers_join
                     from distillers in distillers_join.DefaultIfEmpty()
                     where
                       distillers.UserId == userId &&
                       prod.ProductionTypeID == 4 &&
                       prod.ProductionEndTime <= endOfReporting &&
                       prod.Gauged == true &&
                       (prod.StatusID == (int)Persistence.BusinessLogicEnums.Status.Active ||
                       prod.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processing ||
                       prod.StateID == 5)
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
            from tax in db.TaxWithdrawn
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
                (from prod in db.Production
                 join prodReport in db.Production4Reporting on prod.ProductionID equals prodReport.ProductionID into prodReport_join
                 from prodReport in prodReport_join.DefaultIfEmpty()
                 join distillers in db.AspNetUserToDistiller on prod.DistillerID equals distillers.DistillerID into distillers_join
                 from distillers in distillers_join.DefaultIfEmpty()
                 join prod2SpiritType in db.ProductionToSpiritTypeReporting on prod.ProductionID equals prod2SpiritType.ProductionID into prod2SpiritType_join
                 from prod2SpiritType in prod2SpiritType_join.DefaultIfEmpty()
                 join prod2Purch in db.ProductionToPurchase on prod.ProductionID equals prod2Purch.ProductionID into prod2Purch_join
                 from prod2Purch in prod2Purch_join.DefaultIfEmpty()
                 join purch4Reprt in db.Purchase4Reporting on prod2Purch.PurchaseID equals purch4Reprt.PurchaseID into purch4Reprt_join
                 from purch4Reprt in purch4Reprt_join.DefaultIfEmpty()
                 join prod2Spirit in db.ProductionToSpirit on prod.ProductionID equals prod2Spirit.ProductionID into prod2Spirit_join
                 from prod2Spirit in prod2Spirit_join.DefaultIfEmpty()
                 join spirit in db.Spirit on prod2Spirit.SpiritID equals spirit.SpiritID into spirit_join
                 from spirit in spirit_join.DefaultIfEmpty()
                 join procRepType in db.ProcessingReportType on spirit.ProcessingReportTypeID equals procRepType.ProcessingReportTypeID into procRepType_join
                 from procRepType in procRepType_join.DefaultIfEmpty()
                 where
                    distillers.UserId == userId &&
                    prod.Gauged == true &&
                    (prod.StatusID == (int)Persistence.BusinessLogicEnums.Status.Active ||
                    prod.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processing ||
                    prod.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processed) &&
                    (new int[] { 4, 5 }).Contains(prod.StateID) &&
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
                    else if (rec.StateID == 5) // processing bottling record
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

        internal bool UpdateProduction(ProductionObject pObj, int userId)
        {
            bool retMthdExecResult = false;
            try
            {
                var prodT =
                    (from rec in db.Production
                     join distillers in db.AspNetUserToDistiller on rec.DistillerID equals distillers.DistillerID into distillers_join
                     from distillers in distillers_join.DefaultIfEmpty()
                     where rec.ProductionID == pObj.ProductionId &&
                        distillers.UserId == userId
                     select rec).FirstOrDefault();

                if (prodT != null)
                {
                    if (prodT.ProductionName != pObj.BatchName && pObj?.BatchName != null)
                    {
                        prodT.ProductionName = pObj.BatchName;
                    }

                    if (prodT.ProductionDate != pObj.ProductionDate && pObj?.ProductionDate != null)
                    {
                        prodT.ProductionDate = pObj.ProductionDate;
                    }

                    if (prodT.ProductionStartTime != pObj.ProductionStart && pObj?.ProductionStart != null)
                    {
                        prodT.ProductionStartTime = pObj.ProductionStart;
                    }

                    if (prodT.ProductionEndTime != pObj.ProductionEnd && pObj?.ProductionEnd != null)
                    {
                        prodT.ProductionEndTime = pObj.ProductionEnd;
                    }

                    if (prodT.Note != pObj.Note && pObj?.Note != null)
                    {
                        prodT.Note = pObj.Note;
                    }
                    db.SaveChanges();
                    //todo: need to be able to add update for Material Type(even though, updating material type might be difficult)

                    // update Spirit type for production if applicable
                    if (pObj?.SpiritId != null && pObj.SpiritId != 0)
                    {
                        var p2S =
                            (from rec in db.ProductionToSpirit
                             where rec.ProductionID == prodT.ProductionID
                             select rec).FirstOrDefault();

                        if (p2S != null)
                        {
                            if (pObj.SpiritId != p2S.SpiritID)
                            {
                                p2S.SpiritID = pObj.SpiritId;
                            }
                        }
                        db.SaveChanges();
                    }

                    // update Spirit Cut if applicable
                    if (pObj?.SpiritCutId != null && pObj.SpiritCutId != 0)
                    {
                        var p2SC =
                         (from rec in db.ProductionToSpiritCut
                          where rec.ProductionID == prodT.ProductionID
                          select rec).FirstOrDefault();

                        if (p2SC != null)
                        {
                            if (pObj.SpiritId != p2SC.SpiritCutID)
                            {
                                p2SC.SpiritCutID = pObj.SpiritCutId;
                            }
                        }
                        db.SaveChanges();
                    }

                    //Quantity
                    if (prodT.VolumeID != 0 && pObj.Quantity != null)
                    {
                        //update quantity record
                        var qtyRec =
                            (from rec in db.Volume
                             where rec.VolumeID == prodT.VolumeID
                             select rec).FirstOrDefault();
                        if (qtyRec != null && qtyRec.Value != pObj.Quantity)
                        {
                            qtyRec.Value = pObj.Quantity;
                            db.SaveChanges();
                        }
                    }
                    else if (prodT.VolumeID == 0 && pObj.Quantity != null)
                    {
                        //create quantity record
                        Volume newQtyRec = new Volume();
                        newQtyRec.Value = pObj.Quantity;
                        db.Volume.Add(newQtyRec);
                        db.SaveChanges();
                        prodT.VolumeID = newQtyRec.VolumeID;
                    }

                    if (pObj.ProductionType != "Bottling")
                    {
                        //Volume By Weight
                        if (prodT.WeightID != 0 && pObj.VolumeByWeight != null)
                        {
                            //update volume by weight record
                            var vbwRec =
                                (from rec in db.Weight
                                 where rec.WeightID == prodT.WeightID
                                 select rec).FirstOrDefault();
                            if (vbwRec != null & vbwRec.Value != pObj.VolumeByWeight)
                            {
                                vbwRec.Value = pObj.VolumeByWeight;
                                db.SaveChanges();
                            }
                        }
                        else if (prodT.WeightID == 0 && pObj.VolumeByWeight != null)
                        {
                            //create new volume by weight record
                            Weight newVbwRec = new Weight();
                            newVbwRec.Value = pObj.VolumeByWeight;
                            db.Weight.Add(newVbwRec);
                            db.SaveChanges();
                            prodT.WeightID = newVbwRec.WeightID;
                        }
                    }
                    else
                    {
                        // Widrawn For Tax update:
                        TaxWithdrawn taxes = new TaxWithdrawn();
                        taxes.DateOfSale = pObj.WithdrawalDate;
                        taxes.DateRecorded = DateTime.UtcNow;
                        taxes.ProductionID = pObj.ProductionId;
                        taxes.Value = pObj.TaxedProof;

                        db.TaxWithdrawn.Add(taxes);
                        db.SaveChanges();
                    }

                    //Alcohol Content
                    if (prodT.AlcoholID != 0 && pObj.AlcoholContent != null)
                    {
                        //update alcohol content record
                        var alcRec =
                            (from rec in db.Alcohol
                             where rec.AlcoholID == prodT.AlcoholID
                             select rec).FirstOrDefault();
                        if (alcRec != null && alcRec.Value != pObj.AlcoholContent)
                        {
                            alcRec.Value = pObj.AlcoholContent;
                            db.SaveChanges();
                        }
                    }
                    else if (prodT.AlcoholID == 0 && pObj.AlcoholContent != null)
                    {
                        //create alcohol content record
                        Alcohol newAlcRec = new Alcohol();
                        newAlcRec.Value = pObj.AlcoholContent;
                        db.Alcohol.Add(newAlcRec);
                        db.SaveChanges();
                        prodT.AlcoholID = newAlcRec.AlcoholID;
                    }

                    //Proof
                    if (prodT.ProofID != 0 && pObj.ProofGallon != null)
                    {
                        //update proof record
                        var prfRec =
                            (from rec in db.Proof
                             where rec.ProofID == prodT.ProofID
                             select rec).FirstOrDefault();
                        if (prfRec != null && prfRec.Value != pObj.ProofGallon)
                        {
                            prfRec.Value = pObj.ProofGallon;
                            db.SaveChanges();
                        }
                    }
                    else if (prodT.ProofID == 0 && pObj.ProofGallon != null)
                    {
                        //create proof record
                        Proof newPrfRec = new Proof();
                        newPrfRec.Value = pObj.ProofGallon;
                        db.Proof.Add(newPrfRec);
                        db.SaveChanges();
                        prodT.ProofID = newPrfRec.ProofID;
                    }

                    // storage update
                    var storages =
                        from rec in db.StorageToRecord
                        where rec.RecordId == prodT.ProductionID && rec.TableIdentifier == "prod"
                        select rec;

                    // empty StorageToRecord table records first
                    if (storages != null)
                    {
                        foreach (var i in storages)
                        {
                            db.StorageToRecord.Remove(i);
                        }
                        db.SaveChanges();
                    }

                    if (pObj.Storage != null)
                    {
                        // write new records to StorageToRecord table
                        foreach (var k in pObj.Storage)
                        {
                            StorageToRecord stoR = new StorageToRecord();
                            stoR.StorageID = k.StorageId;
                            stoR.RecordId = prodT.ProductionID;
                            stoR.TableIdentifier = "prod";
                            db.StorageToRecord.Add(stoR);
                        }
                        db.SaveChanges();
                    }

                    // update Blended Component If applicable 
                    if (pObj.BlendingAdditives != null)
                    {
                        var blenComp =
                            (from rec in db.BlendedComponent
                             where rec.ProductionID == prodT.ProductionID
                             select rec);

                        if (blenComp != null)
                        {
                            foreach (var bc in blenComp)
                            {
                                db.BlendedComponent.Remove(bc);
                            }
                        }

                        foreach (var bA in pObj.BlendingAdditives)
                        {
                            BlendedComponent blendCT = new BlendedComponent();
                            blendCT.RecordId = bA.RawMaterialId;
                            blendCT.ProductionID = prodT.ProductionID;
                            blendCT.Quantity = bA.RawMaterialQuantity;
                            blendCT.UnitOfMeasurement = bA.UnitOfMeasurement;
                            db.BlendedComponent.Add(blendCT);
                        }
                        db.SaveChanges();
                    }

                    // update Bottling info if applicable
                    if (pObj.BottlingInfo != null)
                    {
                        var botlR =
                            (from rec in db.BottlingInfo
                             where rec.ProductionID == prodT.ProductionID
                             select rec).FirstOrDefault();

                        if (botlR != null)
                        {
                            if (botlR.BottleQuantity != pObj.BottlingInfo.BottleQuantity)
                            {
                                botlR.BottleQuantity = pObj.BottlingInfo.BottleQuantity;
                            }

                            if (botlR.CaseCapacity != pObj.BottlingInfo.CaseCapacity)
                            {
                                botlR.CaseCapacity = pObj.BottlingInfo.CaseCapacity;
                            }

                            if (botlR.BottleVolume != pObj.BottlingInfo.BottleCapacity)
                            {
                                botlR.BottleVolume = pObj.BottlingInfo.BottleCapacity;
                            }

                            if (botlR.CaseQuantity != pObj.BottlingInfo.CaseQuantity)
                            {
                                botlR.CaseQuantity = pObj.BottlingInfo.CaseQuantity;
                            }
                        }
                        db.SaveChanges();
                    }

                    retMthdExecResult = true;
                }
                else
                {
                    retMthdExecResult = false;
                }
            }
            catch (Exception e)
            {
                throw;
            }
            return retMthdExecResult;
        }

        internal List<ProdObjectConcise> GetBlendingList(string prodType, int userId)
        {
            List<ProdObjectConcise> bList = new List<ProdObjectConcise>();
            try
            {
                var res =
                   from prod in db.Production
                   join quants in db.Volume on prod.VolumeID equals quants.VolumeID into quants_join
                   from quants in quants_join.DefaultIfEmpty()
                   join VBW in db.Weight on prod.WeightID equals VBW.WeightID into VBW_join
                   from VBW in VBW_join.DefaultIfEmpty()
                   join alc in db.Alcohol on prod.AlcoholID equals alc.AlcoholID into alc_join
                   from alc in alc_join.DefaultIfEmpty()
                   join proof in db.Proof on prod.ProofID equals proof.ProofID into proof_join
                   from proof in proof_join.DefaultIfEmpty()
                   join p2Spi in db.ProductionToSpirit on prod.ProductionID equals p2Spi.ProductionID into p2Spi_join
                   from p2Spi in p2Spi_join.DefaultIfEmpty()
                   join spi in db.Spirit on p2Spi.SpiritID equals spi.SpiritID into spi_join
                   from spi in spi_join.DefaultIfEmpty()
                   join status in db.Status on prod.StatusID equals status.StatusID into status_join
                   from status in status_join.DefaultIfEmpty()
                   join state in db.State on prod.StateID equals state.StateID into state_join
                   from state in state_join.DefaultIfEmpty()
                   join distiller in db.AspNetUserToDistiller on new { DistillerID = prod.DistillerID } equals new { DistillerID = distiller.DistillerID } into distiller_join
                   from distiller in distiller_join.DefaultIfEmpty()
                   where
                     (prod.StatusID == (int)Persistence.BusinessLogicEnums.Status.Active ||
                     prod.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processing) &&
                     prod.StateID == (int)Persistence.BusinessLogicEnums.State.Blended &&
                     distiller.UserId == userId
                   select new
                   {
                       ProductionName = prod.ProductionName,
                       ProductionID = ((System.Int32?)prod.ProductionID ?? (System.Int32?)0),
                       ProductionEndDate = prod.ProductionEndTime,
                       StatusName = status.Name,
                       StateName = state.Name,
                       Quantity = ((System.Single?)quants.Value ?? (System.Single?)0),
                       VolumeByWeight = ((System.Single?)VBW.Value ?? (System.Single?)0),
                       Alcohol = ((System.Single?)alc.Value ?? (System.Single?)0),
                       Proof = ((System.Single?)proof.Value ?? (System.Single?)0),
                       SpiritName = (spi.Name ?? string.Empty),
                       SpiritID = ((System.Int32?)p2Spi.SpiritID ?? (System.Int32?)0)
                   };

                if (res != null)
                {
                    foreach (var rec in res)
                    {
                        ProdObjectConcise pobj = new ProdObjectConcise();
                        pobj.DistillableOrigin = "prod";
                        pobj.BatchName = rec.ProductionName;
                        pobj.ProductionId = (int)rec.ProductionID;
                        pobj.ProductionEndDate = rec.ProductionEndDate;
                        pobj.Quantity = (float)rec.Quantity;
                        pobj.VolumeByWeight = (float)rec.VolumeByWeight;
                        pobj.AlcoholContent = (float)rec.Alcohol;
                        pobj.ProofGallon = (float)rec.Proof;
                        pobj.SpiritId = (int)rec.SpiritID;
                        pobj.SpiritName = rec.SpiritName;

                        bList.Add(pobj);
                    }
                }
            }
            catch (Exception e)
            {
                throw (e);
            }

            return bList;
        }

        internal bool UpdatePurchase(PurchaseObject purchaseObject, int userId)
        {
            bool retMthdExecResult = false;

            try
            {
                var purchT =
                    (from rec in db.Purchase
                     join dslrs in db.AspNetUserToDistiller on rec.DistillerID equals dslrs.DistillerID into dslrs_join
                     from dslrs in dslrs_join.DefaultIfEmpty()
                     where rec.PurchaseID == purchaseObject.PurchaseId &&
                     dslrs.UserId == userId
                     select rec).FirstOrDefault();

                if (purchT != null)
                {
                    if (purchT.PurchaseName != purchaseObject.PurBatchName && purchaseObject.PurBatchName != null)
                    {
                        purchT.PurchaseName = purchaseObject.PurBatchName;
                    }

                    if (purchT.PurchaseDate != purchaseObject.PurchaseDate && purchaseObject.PurchaseDate != null)
                    {
                        purchT.PurchaseDate = purchaseObject.PurchaseDate;
                    }

                    if (purchT.VendorID != purchaseObject.VendorId && purchaseObject?.VendorId != null)
                    {
                        purchT.VendorID = purchaseObject.VendorId;
                    }

                    if (purchT.Price != purchaseObject.Price && purchaseObject?.Price != null)
                    {
                        purchT.Price = purchaseObject.Price;
                    }

                    if (purchT.Note != purchaseObject.Note && purchaseObject.Note != null)
                    {
                        purchT.Note = purchaseObject.Note;
                    }

                    //todo: need to be able to add update for storages and Material Type(even though, updating material type might be difficult)

                    db.SaveChanges();

                    // Quantity
                    if (purchT.VolumeID > 0 && purchaseObject.Quantity != null)
                    {
                        //update quantity record
                        var qtyRec =
                            (from rec in db.Volume
                             where rec.VolumeID == purchT.VolumeID
                             select rec).FirstOrDefault();
                        if (qtyRec != null && qtyRec.Value != purchaseObject.Quantity)
                        {
                            qtyRec.Value = purchaseObject.Quantity;
                            db.SaveChanges();
                        }
                    }
                    else if (purchT.VolumeID == 0 && purchaseObject.Quantity != null)
                    {
                        //create quantity record
                        Volume newQtyRec = new Volume();
                        newQtyRec.Value = purchaseObject.Quantity;
                        db.Volume.Add(newQtyRec);
                        db.SaveChanges();
                        purchT.VolumeID = newQtyRec.VolumeID;
                    }

                    if (purchaseObject.PurchaseType != "Supply" || purchaseObject.PurchaseType != "Additive")
                    {
                        // Volume By Weight
                        if (purchT.WeightID != 0 && purchaseObject.VolumeByWeight != null)
                        {
                            //update volume by weight record
                            var vbwRec =
                                (from rec in db.Weight
                                 where rec.WeightID == purchT.WeightID
                                 select rec).FirstOrDefault();

                            if (vbwRec != null && vbwRec.Value != purchaseObject.VolumeByWeight)
                            {
                                vbwRec.Value = purchaseObject.VolumeByWeight;
                                db.SaveChanges();
                            }
                        }
                        else if (purchT.WeightID == 0 && purchaseObject.VolumeByWeight != null)
                        {
                            //create volume by weight record
                            Weight newVbwRec = new Weight();
                            newVbwRec.Value = purchaseObject.VolumeByWeight;
                            db.Weight.Add(newVbwRec);
                            db.SaveChanges();
                            purchT.WeightID = newVbwRec.WeightID;
                        }
                    }

                    if (purchaseObject.PurchaseType == "Distilled" || purchaseObject.PurchaseType == "Fermented")
                    {
                        // Alcohol Content
                        if (purchT.AlcoholID != 0 && purchaseObject.AlcoholContent != null)
                        {
                            //update alcohol content record
                            var alcRec =
                                (from rec in db.Alcohol
                                 where rec.AlcoholID == purchT.AlcoholID
                                 select rec).FirstOrDefault();
                            if (alcRec != null && alcRec.Value != purchaseObject.AlcoholContent)
                            {
                                alcRec.Value = purchaseObject.AlcoholContent;
                                db.SaveChanges();
                            }
                        }
                        else if (purchT.AlcoholID == 0 && purchaseObject.AlcoholContent != null)
                        {
                            //create alcohol content record
                            Alcohol newAlcRec = new Alcohol();
                            newAlcRec.Value = purchaseObject.AlcoholContent;
                            db.Alcohol.Add(newAlcRec);
                            db.SaveChanges();
                            purchT.AlcoholID = newAlcRec.AlcoholID;
                        }

                        // Proof
                        if (purchT.ProofID != 0 && purchaseObject.ProofGallon != null)
                        {
                            //update proof record
                            var prfRec =
                                (from rec in db.Proof
                                 where rec.ProofID == purchT.ProofID
                                 select rec).FirstOrDefault();
                            if (prfRec != null && prfRec.Value != purchaseObject.ProofGallon)
                            {
                                prfRec.Value = purchaseObject.ProofGallon;
                                db.SaveChanges();
                            }
                        }
                        else if (purchT.ProofID == 0 && purchaseObject.ProofGallon != null)
                        {
                            //create proof record
                            Proof newPrfRec = new Proof();
                            newPrfRec.Value = purchaseObject.ProofGallon;
                            db.Proof.Add(newPrfRec);
                            db.SaveChanges();
                            purchT.ProofID = newPrfRec.ProofID;
                        }

                        // call Update Storage report table here
                    }

                    // update storages
                    var storages =
                        from rec in db.StorageToRecord
                        where rec.RecordId == purchT.PurchaseID && rec.TableIdentifier == "pur"
                        select rec;

                    // empty StorageToRecord table records first
                    if (storages != null)
                    {
                        foreach (var i in storages)
                        {
                            db.StorageToRecord.Remove(i);
                        }
                        db.SaveChanges();
                    }

                    if (purchaseObject.Storage != null)
                    {
                        string storagesString = string.Empty;
                        // write new records to StorageToRecord table
                        foreach (var k in purchaseObject.Storage)
                        {
                            StorageToRecord stoR = new StorageToRecord();
                            stoR.StorageID = k.StorageId;
                            stoR.RecordId = purchT.PurchaseID;
                            stoR.TableIdentifier = "pur";
                            db.StorageToRecord.Add(stoR);
                            db.SaveChanges();
                            storagesString += k.StorageName + "; ";
                        }
                        purchaseObject.StorageName = storagesString;
                    }
                }
                else
                {
                    return false;
                }

                retMthdExecResult = true;

                SavePurchaseHistory(purchaseObject, userId);
            }
            catch (Exception e)
            {
                throw;
            }

            return retMthdExecResult;
        }

        /// <summary>
        /// This method removes all relevant records in the DB in all associated tables
        /// </summary>
        /// <param name="purchaseObject"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool DeletePurchase(PurchaseObject purchaseObject, int userId)
        {
            bool retMthdExecResult = false;
            if (purchaseObject.PurchaseId >= 0)
            {
                try
                {
                    var purRec =
                        (from rec in db.Purchase
                         join dslrs in db.AspNetUserToDistiller on rec.DistillerID equals dslrs.DistillerID into dslrs_join
                         from dslrs in dslrs_join.DefaultIfEmpty()
                         where rec.PurchaseID == purchaseObject.PurchaseId &&
                            dslrs.UserId == userId
                         select rec).FirstOrDefault();

                    if (purRec != null)
                    {
                        var purch4Rep =
                           (from rec in db.Purchase4Reporting
                            where rec.PurchaseID == purRec.PurchaseID
                            select rec).FirstOrDefault();

                        if (purch4Rep != null)
                        {
                            db.Purchase4Reporting.Remove(purch4Rep);

                        }

                        db.Purchase.Remove(purRec);

                        var qtyRec =
                            (from rec in db.Volume
                             where rec.VolumeID == purRec.VolumeID
                             select rec).FirstOrDefault();

                        if (qtyRec != null)
                        {
                            db.Volume.Remove(qtyRec);
                        }

                        var vbwRec =
                            (from rec in db.Weight
                             where rec.WeightID == purRec.WeightID
                             select rec).FirstOrDefault();

                        if (vbwRec != null)
                        {
                            db.Weight.Remove(vbwRec);
                        }

                        var alcRec =
                            (from rec in db.Alcohol
                             where rec.AlcoholID == purRec.AlcoholID
                             select rec).FirstOrDefault();

                        if (alcRec != null)
                        {
                            db.Alcohol.Remove(alcRec);
                        }

                        var prfRec =
                            (from rec in db.Proof
                             where rec.ProofID == purRec.ProofID
                             select rec).FirstOrDefault();

                        if (prfRec != null)
                        {
                            db.Proof.Remove(prfRec);
                        }

                        var strRec =
                            (from rec in db.StorageToRecord
                             where rec.RecordId == purRec.PurchaseID
                             select rec).FirstOrDefault();

                        if (strRec != null)
                        {
                            db.StorageToRecord.Remove(strRec);
                        }

                        var purToSpiritTypeReporting =
                            (from rec in db.PurchaseToSpiritTypeReporting
                             where rec.PurchaseID == purRec.PurchaseID
                             select rec);

                        if (purToSpiritTypeReporting != null)
                        {
                            foreach (var i in purToSpiritTypeReporting)
                            {
                                db.PurchaseToSpiritTypeReporting.Remove(i);
                            }
                        }

                        var prodContent =
                            (from rec in db.ProductionContent
                             where rec.RecordID == purRec.PurchaseID
                             && rec.isProductionComponent == false
                             select rec);

                        if (prodContent != null)
                        {
                            foreach (var k in prodContent)
                            {
                                db.ProductionContent.Remove(k);
                            }
                        }

                        db.Purchase.Remove(purRec);

                        db.SaveChanges();
                    }

                    retMthdExecResult = true;
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Failed to delete Purchase: " + e);
                    retMthdExecResult = false;
                }
            }
            else
            {
                retMthdExecResult = false;
            }
            return retMthdExecResult;
        }

        /// <summary>
        /// GetRawMaterialListForDistilling raw distillable items for Distilling workflow
        /// </summary>
        /// <param name="distillMethod"></param>
        /// <returns></returns>
        public List<ProdObjectConcise> GetMaterialListForProduction(string productionType, int userId)
        {
            List<ProdObjectConcise> list = new List<ProdObjectConcise>();

            if (productionType == "distillation")
            {
                int combinedId = 1; // we need this so we have a unique set of ids that listbox in Distillation and Blending workflows needs

                var purchaseQueryResult =
                    from pur in db.Purchase
                    join qty in db.Volume on pur.VolumeID equals qty.VolumeID into qty_join
                    from qty in qty_join.DefaultIfEmpty()
                    join vbw in db.Weight on pur.WeightID equals vbw.WeightID into vbw_join
                    from vbw in vbw_join.DefaultIfEmpty()
                    join alc in db.Alcohol on pur.AlcoholID equals alc.AlcoholID into alc_join
                    from alc in alc_join.DefaultIfEmpty()
                    join distiller in db.AspNetUserToDistiller on pur.DistillerID equals distiller.DistillerID into distiller_join
                    from distiller in distiller_join.DefaultIfEmpty()
                    where
                        (pur.StatusID == (int)Persistence.BusinessLogicEnums.Status.Active || pur.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processing) &&
                        (pur.StateID == (int)Persistence.BusinessLogicEnums.State.Fermented || pur.StateID == (int)Persistence.BusinessLogicEnums.State.Distilled) &&
                        distiller.UserId == userId
                    select new
                    {
                        PurchaseID = ((System.Int32?)pur.PurchaseID ?? (System.Int32?)0),
                        PurchaseBatchName = pur.PurchaseName ?? string.Empty,
                        StatusID = ((System.Int32?)pur.StatusID ?? (System.Int32?)0),
                        Quantity = ((System.Single?)qty.Value ?? (System.Single?)0),
                        AlcoholContent = ((System.Single?)alc.Value ?? (System.Single?)0),
                        VolumeByWeight = ((System.Single?)vbw.Value ?? (System.Single?)0),
                        BurningDownMethod = pur.BurningDownMethod ?? null,
                        PurchaseDate = pur.PurchaseDate
                    };

                if (purchaseQueryResult != null)
                {
                    foreach (var i in purchaseQueryResult)
                    {
                        ProdObjectConcise prodObj = new ProdObjectConcise();
                        prodObj.BatchName = i.PurchaseBatchName;
                        prodObj.RecordId = combinedId;
                        prodObj.PurchaseId = (int)i.PurchaseID;
                        prodObj.DistillableOrigin = "pur";
                        prodObj.Quantity = (float)i.Quantity;
                        prodObj.VolumeByWeight = (float)i.VolumeByWeight;
                        prodObj.BurningDownMethod = i.BurningDownMethod;
                        prodObj.AlcoholContent = (float)i.AlcoholContent;
                        prodObj.PurchaseDate = i.PurchaseDate;
                        list.Add(prodObj);
                        combinedId++;
                    }
                }

                var productionQueryResult =
                    from prod in db.Production
                    join qty in db.Volume on prod.VolumeID equals qty.VolumeID into qty_join
                    from qty in qty_join.DefaultIfEmpty()
                    join vbw in db.Weight on prod.WeightID equals vbw.WeightID into vbw_join
                    from vbw in vbw_join.DefaultIfEmpty()
                    join alc in db.Alcohol on prod.AlcoholID equals alc.AlcoholID into alc_join
                    from alc in alc_join.DefaultIfEmpty()
                    join distiller in db.AspNetUserToDistiller on prod.DistillerID equals distiller.DistillerID into distiller_join
                    from distiller in distiller_join.DefaultIfEmpty()
                    where
                        (prod.StatusID == (int)Persistence.BusinessLogicEnums.Status.Active || prod.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processing) &&
                        (prod.StateID == (int)Persistence.BusinessLogicEnums.State.Fermented || prod.StateID == (int)Persistence.BusinessLogicEnums.State.Distilled) &&
                        distiller.UserId == userId
                    select new
                    {
                        ProductionID = ((System.Int32?)prod.ProductionID ?? (System.Int32?)0),
                        ProductionBatchName = prod.ProductionName ?? string.Empty,
                        StatusID = ((System.Int32?)prod.StatusID ?? (System.Int32?)0),
                        Quantity = ((System.Single?)qty.Value ?? (System.Single?)0),
                        VolumeByWeight = ((System.Single?)vbw.Value ?? (System.Single?)0),
                        AlcoholContent = ((System.Single?)alc.Value ?? (System.Single?)0),
                        BurningDownMethod = prod.BurningDownMethod ?? null,
                        ProductionEndDate = prod.ProductionEndTime
                    };

                if (productionQueryResult != null)
                {
                    foreach (var i in productionQueryResult)
                    {
                        ProdObjectConcise prodObj = new ProdObjectConcise();
                        prodObj.BatchName = i.ProductionBatchName;
                        prodObj.RecordId = combinedId;
                        prodObj.ProductionId = (int)i.ProductionID;
                        prodObj.DistillableOrigin = "prod";
                        prodObj.Quantity = (float)i.Quantity;
                        prodObj.VolumeByWeight = (float)i.VolumeByWeight;
                        prodObj.AlcoholContent = (float)i.AlcoholContent;
                        prodObj.BurningDownMethod = i.BurningDownMethod;
                        prodObj.ProductionEndDate = i.ProductionEndDate;
                        list.Add(prodObj);
                        combinedId++;
                    }
                }
            }
            else if (productionType == "blending")
            {
                int combinedId = 1; // we need this so we have a unique set of ids that listbox in Distillation and Blending workflows needs

                var res =
                 (from purch in db.Purchase
                  join distillers in db.AspNetUserToDistiller on purch.DistillerID equals distillers.DistillerID into distillers_join
                  from distillers in distillers_join.DefaultIfEmpty()
                  join quant in db.Volume on purch.VolumeID equals quant.VolumeID into quant_join
                  from quant in quant_join.DefaultIfEmpty()
                  join vbw in db.Weight on purch.WeightID equals vbw.WeightID into vbw_join
                  from vbw in vbw_join.DefaultIfEmpty()
                  where
                   (purch.StatusID == (int)Persistence.BusinessLogicEnums.Status.Active ||
                   purch.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processing) &&
                   purch.StateID == (int)Persistence.BusinessLogicEnums.State.Distilled &&
                   distillers.UserId == userId
                  select new
                  {
                      PurchaseID = ((System.Int32?)purch.PurchaseID ?? (System.Int32?)0),
                      PurchaseBatchName = purch.PurchaseName ?? string.Empty,
                      PurchaseDate = purch.PurchaseDate,
                      StatusID = ((System.Int32?)purch.StatusID ?? (System.Int32?)0),
                      StateID = ((System.Int32?)purch.StateID ?? (System.Int32?)0),
                      Quantity = ((System.Single?)quant.Value ?? (System.Single?)0),
                      VolumeByWeight = ((System.Single?)vbw.Value ?? (System.Single?)0),
                      BurningDownMethod = purch.BurningDownMethod ?? null
                  }).Distinct();

                if (res != null)
                {
                    foreach (var i in res)
                    {
                        ProdObjectConcise prodO = new ProdObjectConcise();
                        prodO.BatchName = i.PurchaseBatchName;
                        prodO.RecordId = combinedId;
                        prodO.PurchaseId = (int)i.PurchaseID;
                        prodO.PurchaseDate = i.PurchaseDate;
                        prodO.DistillableOrigin = "pur";
                        prodO.BurningDownMethod = i.BurningDownMethod;
                        prodO.Quantity = (float)i.Quantity;
                        prodO.VolumeByWeight = (float)i.VolumeByWeight;
                        list.Add(prodO);
                        combinedId++;
                    }
                }

                var res1 =
                   (from prod in db.Production
                    join distillers in db.AspNetUserToDistiller on new { DistillerID = prod.DistillerID } equals new { DistillerID = distillers.DistillerID } into distillers_join
                    from distillers in distillers_join.DefaultIfEmpty()
                    join quant in db.Volume on prod.VolumeID equals quant.VolumeID into quant_join
                    from quant in quant_join.DefaultIfEmpty()
                    join vbw in db.Weight on prod.WeightID equals vbw.WeightID into vbw_join
                    from vbw in vbw_join.DefaultIfEmpty()
                    where
                      (prod.StatusID == (int)Persistence.BusinessLogicEnums.Status.Active ||
                      prod.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processing) &&
                      prod.StateID == (int)Persistence.BusinessLogicEnums.State.Distilled &&
                      distillers.UserId == userId
                    select new
                    {
                        ProductionID = ((System.Int32?)prod.ProductionID ?? (System.Int32?)0),
                        ProductionName = prod.ProductionName ?? string.Empty,
                        ProductonEndDate = prod.ProductionEndTime,
                        StatusID = ((System.Int32?)prod.StatusID ?? (System.Int32?)0),
                        StateID = ((System.Int32?)prod.StateID ?? (System.Int32?)0),
                        Quantity = ((System.Single?)quant.Value ?? (System.Single?)0),
                        VolumeByWeight = ((System.Single?)vbw.Value ?? (System.Single?)0),
                        BurningDownMethod = prod.BurningDownMethod ?? null
                    }).Distinct();

                if (res1 != null)
                {
                    foreach (var i in res1)
                    {
                        ProdObjectConcise prodO = new ProdObjectConcise();
                        prodO.BatchName = i.ProductionName;
                        prodO.DistillableOrigin = "prod";
                        prodO.RecordId = combinedId;
                        prodO.ProductionId = (int)i.ProductionID;
                        prodO.ProductionEndDate = i.ProductonEndDate;
                        prodO.BurningDownMethod = i.BurningDownMethod;
                        prodO.Quantity = (float)i.Quantity;
                        prodO.VolumeByWeight = (float)i.VolumeByWeight;
                        list.Add(prodO);
                        combinedId++;
                    }
                }
            }

            return list;
        }

        public List<RawMaterialObject> GetAdditivesListForProduction(string matType, int userId)
        {
            List<RawMaterialObject> rawMaterialList = new List<RawMaterialObject>();

            var ress =
            from Mats in db.MaterialDict
            join distillers in db.AspNetUserToDistiller on Mats.DistillerID equals distillers.DistillerID into distillers_join
            from distillers in distillers_join
            join MatsType in db.MaterialType on Mats.MaterialDictID equals MatsType.MaterialDictID into MatsType_join
            from MatsType in MatsType_join.DefaultIfEmpty()
            join units in db.UnitOfMeasurement on Mats.UnitOfMeasurementID equals units.UnitOfMeasurementID into units_join
            from units in units_join.DefaultIfEmpty()
            where
                MatsType.Name == matType &&
                distillers.UserId == userId
            select new
            {
                MaterialDictID = (System.Int32?)Mats.MaterialDictID ?? (System.Int32?)0,
                Name = Mats.Name ?? string.Empty,
                UnitOfMeasurementID = (System.Int32?)Mats.UnitOfMeasurementID ?? (System.Int32?)0,
                Note = Mats.Note ?? string.Empty,
                UnitName = units.Name ?? string.Empty
            };

            if (ress != null)
            {
                foreach (var i in ress)
                {
                    RawMaterialObject rObj = new RawMaterialObject();
                    rObj.RawMaterialId = (int)i.MaterialDictID;
                    rObj.RawMaterialName = i.Name;
                    rObj.Note = i.Note;
                    rObj.UnitType = i.UnitName;
                    rObj.UnitTypeId = (int)i.UnitOfMeasurementID;
                    rawMaterialList.Add(rObj);
                }
            }

            return rawMaterialList;
        }

        /// <summary>
        /// GetRawMaterialListForPurchase method is used to view RawMaterials list in Purchase workflows
        /// </summary>
        /// <param name="optimized"></param>
        /// <param name="identifierId"></param>
        /// <param name="purchaseMatType"></param>
        /// <returns></returns>
        public List<RawMaterialObject> GetRawMaterialListForPurchase(int identifierId, int userId, string purchaseMatType = "")
        {
            List<RawMaterialObject> rawMaterialList = new List<RawMaterialObject>();

            var ress =
            from Mats in db.MaterialDict
            join dslrs in db.AspNetUserToDistiller on Mats.DistillerID equals dslrs.DistillerID into dslrs_join
            from dslrs in dslrs_join.DefaultIfEmpty()
            join MatsType in db.MaterialType on Mats.MaterialDictID equals MatsType.MaterialDictID into MatsType_join
            from MatsType in MatsType_join.DefaultIfEmpty()
            join units in db.UnitOfMeasurement on Mats.UnitOfMeasurementID equals units.UnitOfMeasurementID into units_join
            from units in units_join.DefaultIfEmpty()
            where
                MatsType.Name == purchaseMatType &&
                dslrs.UserId == userId
            select new
            {
                Mats.MaterialDictID,
                Mats.Name,
                Mats.UnitOfMeasurementID,
                Mats.Note,
                UnitName = units.Name
            };

            if (ress != null)
            {
                foreach (var i in ress)
                {
                    RawMaterialObject rObj = new RawMaterialObject();
                    rObj.RawMaterialId = i.MaterialDictID;
                    rObj.RawMaterialName = i.Name;
                    rObj.Note = i.Note;
                    rObj.UnitType = i.UnitName;
                    rObj.UnitTypeId = i.UnitOfMeasurementID;
                    rawMaterialList.Add(rObj);
                }
            }

            return rawMaterialList;
        }

        /// <summary>
        /// GetRawMaterialListDict method that we use in dictionary workflows to view All Raw Materials record in Raw Material view workflow
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<RawMaterialObject> GetRawMaterialListDict(int userId)
        {
            List<RawMaterialObject> rawMaterialList = new List<RawMaterialObject>();
            var res =
                from matDict in db.MaterialDict
                join unit in db.UnitOfMeasurement on matDict.UnitOfMeasurementID equals unit.UnitOfMeasurementID into unit_join
                from unit in unit_join.DefaultIfEmpty()
                join us2Distills in db.AspNetUserToDistiller on new { DistillerID = matDict.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where
                  us2Distills.UserId == userId
                select new
                {
                    MaterialDictID = (System.Int32?)matDict.MaterialDictID ?? (System.Int32?)0,
                    Name = matDict.Name ?? string.Empty,
                    UnitOfMeasurementID = (System.Int32?)matDict.UnitOfMeasurementID ?? (System.Int32?)0,
                    Note = matDict.Note ?? string.Empty,
                    UnitName = unit.Name ?? string.Empty
                };

            if (res != null)
            {
                foreach (var i in res)
                {
                    RawMaterialObject rawMatObj = new RawMaterialObject();
                    rawMatObj.RawMaterialId = (int)i.MaterialDictID;
                    rawMatObj.RawMaterialName = i.Name;
                    rawMatObj.Note = i.Note;
                    rawMatObj.UnitType = i.UnitName;
                    rawMatObj.UnitTypeId = (int)i.UnitOfMeasurementID;
                    rawMaterialList.Add(rawMatObj);
                }
            }

            foreach (var iter in rawMaterialList)
            {
                var matTypes =
                        from mattype in db.MaterialType
                        where mattype.MaterialDictID == iter.RawMaterialId
                        select mattype.Name;
                if (matTypes != null)
                {
                    PurchaseMaterialBooleanTypes types = new PurchaseMaterialBooleanTypes();
                    foreach (var it in matTypes)
                    {
                        if (it == "Fermentable")
                        {
                            types.Fermentable = true;
                        }
                        if (it == "Fermented")
                        {
                            types.Fermented = true;
                        }
                        if (it == "Distilled")
                        {
                            types.Distilled = true;
                        }
                        if (it == "Supply")
                        {
                            types.Supply = true;
                        }
                        if (it == "Additive")
                        {
                            types.Additive = true;
                        }
                    }
                    iter.PurchaseMaterialTypes = types;
                }
            }
            return rawMaterialList;
        }

        /// <summary>
        /// GetPurMaterialType method maps current PurchaseMaterialBooleanTypes types
        /// </summary>
        /// <param name="purMatBooleanTypes"></param>
        /// <param name="purchaseMaterialTypeName"></param>
        /// <returns></returns>
        private void GetPurMaterialType(ref PurchaseMaterialBooleanTypes purMatBooleanTypes, string purchaseMaterialTypeName)
        {
            if (purchaseMaterialTypeName == "Additive")
            {
                purMatBooleanTypes.Additive = true;
            }
            if (purchaseMaterialTypeName == "Supply")
            {
                purMatBooleanTypes.Supply = true;
            }
            if (purchaseMaterialTypeName == "Distilled")
            {
                purMatBooleanTypes.Distilled = true;
            }
            if (purchaseMaterialTypeName == "Fermented")
            {
                purMatBooleanTypes.Fermented = true;
            }
            if (purchaseMaterialTypeName == "Fermentable")
            {
                purMatBooleanTypes.Fermentable = true;
            }
        }

        /// <summary>
        /// GetSpiritTypeList method gets the list of available spirits
        /// </summary>
        /// <returns></returns>
        public List<SpiritObject> GetSpiritTypeList(int userId)
        {
            List<SpiritObject> spiritList = new List<SpiritObject>();

            try
            {
                var res =
                    (from i in db.Spirit
                     join distillers in db.AspNetUserToDistiller on i.DistillerID equals distillers.DistillerID into distillers_join
                     from distillers in distillers_join
                     where distillers.UserId == userId
                     select new
                     {
                         i.SpiritID,
                         i.Name,
                         i.Note
                     });
                foreach (var i in res)
                {
                    SpiritObject spirit = new SpiritObject();
                    spirit.SpiritId = i.SpiritID;
                    spirit.SpiritName = i.Name;
                    spirit.Note = i.Note;
                    spiritList.Add(spirit);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error Getting Spirit Type List: " + e);
                return null;
            }

            return spiritList;
        }

        /// <summary>
        /// IsOfPurMaterialType method returns value (true/false) for a given set of purchaseMaterialTypes and purchaseMaterialType in question.
        /// For example I am trying to see what bool value a current purchaseMaterial type of "Fermented" has.
        /// </summary>
        /// <param name="purchaseMaterialTypes"></param>
        /// <param name="purchaseMatType"></param>
        /// <returns></returns>
        private bool IsOfPurMaterialType(PurchaseMaterialBooleanTypes purchaseMaterialTypes, string purchaseMatType)
        {
            if (purchaseMatType == "Additive")
            {
                return purchaseMaterialTypes.Additive;
            }
            else if (purchaseMatType == "Supply")
            {
                return purchaseMaterialTypes.Supply;
            }
            else if (purchaseMatType == "Distilled")
            {
                return purchaseMaterialTypes.Distilled;
            }
            else if (purchaseMatType == "Fermented")
            {
                return purchaseMaterialTypes.Fermented;
            }
            else if (purchaseMatType == "Fermentable")
            {
                return purchaseMaterialTypes.Fermentable;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// GetUnitList gets the list of units available from the UnitsOfMeasurement table
        /// </summary>
        /// <returns>List<UnitObject></returns>
        public List<UnitObject> GetUnitList()
        {
            List<UnitObject> unitList = new List<UnitObject>();
            try
            {
                var recs = db.UnitOfMeasurement.ToList();
                var recsFinalResult = (from rec in recs
                                       select new
                                       {
                                           rec.UnitOfMeasurementID,
                                           rec.Name
                                       });
                foreach (var res in recsFinalResult)
                {
                    UnitObject unit = new UnitObject();
                    unit.UnitOfMeasurementId = res.UnitOfMeasurementID;
                    unit.UnitName = res.Name;
                    unitList.Add(unit);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error getting the list of Units: " + e);
                throw;
            }
            return unitList;
        }

        /// <summary>
        /// GetVendorData queries db for list of Vendors and their associated id's
        /// </summary>
        /// <returns>List<VendorObject></returns>
        public List<VendorObject> GetVendorData(int userId)
        {
            var vendorList = new List<VendorObject>();

            try
            {
                var VendorFinalResults =
                    from ven in db.Vendor
                    join dslrs in db.AspNetUserToDistiller on ven.DistillerID equals dslrs.DistillerID into dslrs_join
                    from dslrs in dslrs_join.DefaultIfEmpty()
                    join venD in db.VendorDetail on ven.VendorID equals venD.VendorID into venD_join
                    from venD in venD_join.DefaultIfEmpty()
                    where dslrs.UserId == userId
                    select new
                    {
                        ven.Name,
                        ven.VendorID
                    };
                foreach (var vendorRes in VendorFinalResults)
                {
                    var curVendor = new VendorObject();
                    curVendor.VendorId = vendorRes.VendorID;
                    curVendor.VendorName = vendorRes.Name;
                    vendorList.Add(curVendor);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error getting Vendor list : " + e);
                throw;
            }

            return vendorList;
        }

        /// <summary>
        /// GetStorageData queries db for list of Storages and their associated id's
        /// </summary>
        /// <returns>List<StorageObject></returns>
        public List<StorageObject> GetStorageData(int userId)
        {
            var storageList = new List<StorageObject>();

            try
            {
                var storageTable = db.Storage.ToList();
                var storages =
                    (from storS in db.Storage
                     join dslrs in db.AspNetUserToDistiller on storS.DistillerID equals dslrs.DistillerID into dslrs_join
                     from dslrs in dslrs_join.DefaultIfEmpty()
                         // where  storS.DistillerID == DistillerID
                     where dslrs.UserId == userId
                     select new
                     {
                         storS.StorageID,
                         storS.Name
                     });
                foreach (var storage in storages)
                {
                    var currentObject = new StorageObject();
                    currentObject.StorageId = storage.StorageID;
                    currentObject.StorageName = storage.Name;
                    storageList.Add(currentObject);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error getting storage data: " + e);
                throw;
            }

            return storageList;
        }

        /// <summary>
        /// GetSpiritCutData queries db for list of spirit cuts and their associated id's
        /// </summary>
        /// <returns>List<StorageObject></returns>
        public List<SpiritCutObject> GetSpiritCutData()
        {
            var spiritCutList = new List<SpiritCutObject>();
            try
            {
                var spiritCuts =
                    (from spiritCut in db.SpiritCut
                     select new
                     {
                         spiritCut.SpiritCutID,
                         spiritCut.Name
                     });
                foreach (var spiritCut in spiritCuts)
                {
                    var currentObject = new SpiritCutObject();
                    currentObject.SpiritCutId = spiritCut.SpiritCutID;
                    currentObject.SpiritCutName = spiritCut.Name;
                    spiritCutList.Add(currentObject);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error getting Spirit Cut data : " + e);
            }
            return spiritCutList;
        }

        public List<SpiritToKindListObject> GetReportingSpiritTypes()
        {
            List<SpiritToKindListObject> stList = new List<SpiritToKindListObject>();
            try
            {
                var str =
                    from st in db.SpiritTypeReporting
                    select st;

                if (str.Any())
                {
                    foreach (var st in str)
                    {
                        SpiritToKindListObject stklObj = new SpiritToKindListObject();
                        stklObj.SpiritTypeReportingID = st.SpiritTypeReportingID;
                        stklObj.ProductTypeName = st.ProductTypeFullName;
                        stList.Add(stklObj);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Unable to get reporting spirit types: {0}", e);
            }
            return stList;
        }
        #endregion

        #region Dictionaries Methods

        /// <summary>
        /// CreateSpirit Method inserts new record in Spirit table
        /// </summary>
        /// <param name="spiritObject"></param>
        /// <returns>int</returns>
        public int CreateSpirit(int userId, SpiritObject spiritObject)
        {
            //define method execution return value to be false by default
            var retMthdExecResult = 0;

            if (spiritObject != null)
            {
                try
                {
                    Spirit tbl = new Spirit();
                    tbl.Name = spiritObject.SpiritName;
                    tbl.ProcessingReportTypeID = spiritObject.ProcessingReportTypeID;
                    tbl.DistillerID = GetDistillerId(userId);
                    if (spiritObject.Note != string.Empty && spiritObject.Note != null)
                    {
                        tbl.Note = spiritObject.Note;
                    }
                    db.Spirit.Add(tbl);
                    db.SaveChanges();
                    retMthdExecResult = tbl.SpiritID;
                }
                catch (Exception e)
                {
                    retMthdExecResult = 0;
                }
            }
            else
            {
                retMthdExecResult = 0;
            }

            return retMthdExecResult;
        }

        /// <summary>
        /// CreateVendor Method inserts new record in Vendor table and also updates Note table if there is a note
        /// </summary>
        /// <param name="vendorObject"></param>
        /// <param name="userId"></param>
        /// <returns>int</returns>
        public int CreateVendor(int userId, VendorObject vendorObject)
        {
            //define method execution return value to be false by default
            int retMthdExecResult = 0;
            int distillerID = GetDistillerId(userId);
            if (vendorObject != null)
            {
                try
                {
                    Vendor tbl = new Vendor();
                    tbl.Name = vendorObject.VendorName;
                    tbl.DistillerID = distillerID;
                    db.Vendor.Add(tbl);
                    db.SaveChanges();

                    VendorDetail tbl1 = new VendorDetail();
                    if (vendorObject.Note != string.Empty && vendorObject.Note != null)
                    {
                        tbl1.Note = vendorObject.Note;
                    }
                    tbl1.VendorID = tbl.VendorID;
                    db.VendorDetail.Add(tbl1);
                    db.SaveChanges();
                    retMthdExecResult = tbl.VendorID;
                }
                catch (Exception e)
                {
                    retMthdExecResult = 0;
                    throw e;
                }
            }
            else
            {
                retMthdExecResult = 0;
            }

            return retMthdExecResult;
        }

        /// <summary>
        /// CreateStorage method inserts a new record in Storage table and a note if such exists
        /// </summary>
        /// <param name="storageObject"></param>
        /// <param name="userId"></param>
        /// <returns>int</returns>
        public int CreateStorage(int userId, StorageObject storageObject)
        {
            //define method execution return value to be false by default
            int retMthdExecResult = 0;
            int distillerId = GetDistillerId(userId);

            if (storageObject != null)
            {
                try
                {
                    Storage storRec = new Storage();
                    storRec.Name = storageObject.StorageName;
                    storRec.SerialNumber = storageObject.SerialNumber;
                    storRec.Capacity = storageObject.Capacity;
                    storRec.DistillerID = distillerId;
                    if (storageObject.Note != string.Empty && storageObject.Note != null)
                    {
                        storRec.Note = storageObject.Note;
                    }
                    db.Storage.Add(storRec);
                    db.SaveChanges();

                    StorageState storState = new StorageState();
                    storState.StorageID = storRec.StorageID;
                    storState.Available = true;
                    db.StorageState.Add(storState);
                    db.SaveChanges();
                    retMthdExecResult = storRec.StorageID;
                }
                catch (Exception e)
                {
                    retMthdExecResult = 0;
                }
            }

            return retMthdExecResult;
        }

        /// <summary>
        /// CreateRawMaterial creates new record in Raw Materials table, inserts/updates Note table and inserts/updates 
        /// </summary>
        /// <param name="rawMObject"></param>
        /// <param name="userId"></param>
        /// <returns>int</returns>
        public int CreateRawMaterial(int userId, RawMaterialObject rawMObject)
        {
            //define method execution return value to be false by default
            int retMthdExecResult = 0;
            int materialDictID = 0;
            int distillerId = GetDistillerId(userId);

            if (rawMObject != null)
            {
                try
                {
                    MaterialDict matDict = new MaterialDict();
                    matDict.Name = rawMObject.RawMaterialName;
                    matDict.UnitOfMeasurementID = rawMObject.UnitTypeId;
                    matDict.DistillerID = distillerId;

                    if (rawMObject.Note != string.Empty && rawMObject.Note != null)
                    {
                        matDict.Note = rawMObject.Note;
                    }

                    db.MaterialDict.Add(matDict);
                    db.SaveChanges();

                    materialDictID = matDict.MaterialDictID;

                    // build relationships between given raw material and purchase material types
                    if (rawMObject.PurchaseMaterialTypes.Additive)
                    {
                        MaterialType matType = new MaterialType();
                        matType.MaterialDictID = materialDictID;
                        matType.Name = "Additive";
                        db.MaterialType.Add(matType);
                        db.SaveChanges();
                    }

                    if (rawMObject.PurchaseMaterialTypes.Distilled)
                    {
                        MaterialType matType = new MaterialType();
                        matType.MaterialDictID = materialDictID;
                        matType.Name = "Distilled";
                        db.MaterialType.Add(matType);
                        db.SaveChanges();
                    }

                    if (rawMObject.PurchaseMaterialTypes.Fermentable)
                    {
                        MaterialType matType = new MaterialType();
                        matType.MaterialDictID = materialDictID;
                        matType.Name = "Fermentable";
                        db.MaterialType.Add(matType);

                        MaterialDict2MaterialCategory md2mc = new MaterialDict2MaterialCategory();
                        md2mc.MaterialDictID = materialDictID;
                        md2mc.ProductionReportMaterialCategoryID = rawMObject.MaterialCategoryID;
                        db.MaterialDict2MaterialCategory.Add(md2mc);
                        db.SaveChanges();
                    }

                    if (rawMObject.PurchaseMaterialTypes.Fermented)
                    {
                        MaterialType matType = new MaterialType();
                        matType.MaterialDictID = materialDictID;
                        matType.Name = "Fermented";
                        db.MaterialType.Add(matType);

                        MaterialDict2MaterialCategory md2mc = new MaterialDict2MaterialCategory();
                        md2mc.MaterialDictID = materialDictID;
                        md2mc.ProductionReportMaterialCategoryID = rawMObject.MaterialCategoryID;
                        db.MaterialDict2MaterialCategory.Add(md2mc);
                        db.SaveChanges();
                    }

                    if (rawMObject.PurchaseMaterialTypes.Supply)
                    {
                        MaterialType matType = new MaterialType();
                        matType.MaterialDictID = materialDictID;
                        matType.Name = "Supply";
                        db.MaterialType.Add(matType);
                        db.SaveChanges();
                    }

                    retMthdExecResult = materialDictID;
                }
                catch
                {
                    retMthdExecResult = 0;
                    throw;
                }
            }
            return retMthdExecResult;
        }

        /// <summary>
        /// UpdateSpirit method updates Spirit table and a note value in Notes table if Note hasn't been changed
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="spiritObject"></param>
        /// <returns>bool</returns>
        public bool UpdateSpirit(int userId, SpiritObject spiritObject)
        {
            //define method execution return value to be false by default
            var retMthdExecResult = false;
            int distillerId = GetDistillerId(userId);

            if (spiritObject != null)
            {
                try
                {
                    var recs =
                        from rec in db.Spirit
                        where rec.SpiritID == spiritObject.SpiritId && rec.DistillerID == distillerId
                        select rec;
                    var item = recs.FirstOrDefault();

                    if (item.Name != spiritObject.SpiritName)
                    {
                        item.Name = spiritObject.SpiritName;
                    }

                    if (item.Note != spiritObject.Note)
                    {
                        item.Note = spiritObject.Note;
                    }

                    db.SaveChanges();
                    retMthdExecResult = true;
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Failed to update spirit record : " + e);
                    retMthdExecResult = false;
                }
            }
            return retMthdExecResult;
        }

        /// <summary>
        /// UpdateVendor method updates Vendor table and a note value in Notes table if Note hasn't been changed
        /// </summary>
        /// <param name="vendorObject"></param>
        /// <param name="userId"></param>
        /// <returns>bool</returns>
        public bool UpdateVendor(int userId, VendorObject vendorObject)
        {
            //define method execution return value to be false by default
            var retMthdExecResult = false;
            int distillerId = GetDistillerId(userId);

            if (vendorObject != null)
            {
                try
                {
                    var recs =
                        from rec in db.Vendor
                        where rec.VendorID == vendorObject.VendorId && rec.DistillerID == distillerId
                        select rec;

                    var vendorItem = recs.FirstOrDefault();

                    if (vendorItem.Name != vendorObject.VendorName || vendorObject.Note != string.Empty)
                    {
                        vendorItem.Name = vendorObject.VendorName;
                        db.SaveChanges();
                    }

                    var recs1 =
                        from rec1 in db.VendorDetail
                        where rec1.VendorID == vendorObject.VendorId
                        select rec1;

                    var vendorItem1 = recs1.FirstOrDefault();

                    if (vendorItem1.Note != vendorObject.Note || vendorObject.Note != null)
                    {
                        vendorItem1.Note = vendorObject.Note;
                        db.SaveChanges();
                    }

                    retMthdExecResult = true;
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Failed updating vendor record: " + e);
                    retMthdExecResult = false;
                }
            }

            return retMthdExecResult;
        }

        /// <summary>
        /// UpdateStorage method updates Storage table and a note value in Notes table if Note hasn't been changed
        /// </summary>
        /// <param name="storageObject"></param>
        /// <param name="userId"></param>
        /// <returns>bool</returns>
        public bool UpdateStorage(int userId, StorageObject storageObject)
        {
            //define method execution return value to be false by default
            var retMthdExecResult = false;
            int distillerId = GetDistillerId(userId);

            if (storageObject != null)
            {
                try
                {
                    var storRes =
                        from storRecord in db.Storage
                        where storRecord.StorageID == storageObject.StorageId
                        && storRecord.DistillerID == distillerId
                        select storRecord;

                    var storItem = storRes.FirstOrDefault();

                    if (storItem.Name != storageObject.StorageName || storageObject.Note != null)
                    {
                        storItem.Name = storageObject.StorageName;
                    }

                    if (storItem.SerialNumber != storageObject.SerialNumber || storageObject.Note != null)
                    {
                        storItem.SerialNumber = storageObject.SerialNumber;
                    }

                    if (storItem.Note != storageObject.Note || storageObject.Note != null)
                    {
                        storItem.Note = storageObject.Note;
                    }

                    db.SaveChanges();
                    retMthdExecResult = true;
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Failed to update Storage Record : " + e);
                    retMthdExecResult = false;
                }
            }

            return retMthdExecResult;
        }

        /// <summary>
        /// UpdateRawMaterial method updates RawMaterial table
        /// <param name="rawMaterialObject"></param>
        /// <param name="userId"></param>
        /// <returns>bool</returns>
        public bool UpdateRawMaterial(int userId, RawMaterialObject rawMObject)
        {
            //define method execution return value to be false by default
            var retMthdExecResult = false;
            int materialDictID = 0;
            int distillerId = GetDistillerId(userId);

            if (rawMObject != null)
            {
                try
                {
                    materialDictID = rawMObject.RawMaterialId;

                    var ress =
                        (from rec in db.MaterialDict
                         where rec.MaterialDictID == materialDictID && rec.DistillerID == distillerId
                         select rec).FirstOrDefault();

                    if (ress != null)
                    {
                        if (ress.Name != rawMObject.RawMaterialName)
                        {
                            ress.Name = rawMObject.RawMaterialName;
                        }

                        if (ress.Note != rawMObject.Note)
                        {
                            ress.Note = rawMObject.Note;
                        }

                        if (ress.UnitOfMeasurementID != rawMObject.UnitTypeId)
                        {
                            ress.UnitOfMeasurementID = rawMObject.UnitTypeId;
                        }
                    }
                    db.SaveChanges();

                    // re-build relationships between given raw material and purchase material types
                    var res =
                        (from rec in db.MaterialType
                         where rec.MaterialDictID == materialDictID
                         select rec);
                    if (res != null)
                    {
                        foreach (var i in res)
                        {
                            db.MaterialType.Remove(i);
                        }
                        db.SaveChanges();
                    }

                    if (rawMObject.PurchaseMaterialTypes.Additive)
                    {
                        try
                        {
                            MaterialType matType = new MaterialType();
                            matType.MaterialDictID = materialDictID;
                            matType.Name = "Additive";
                            db.MaterialType.Add(matType);
                            db.SaveChanges();
                            retMthdExecResult = true;
                        }
                        catch
                        {
                            return false;
                        }
                    }

                    if (rawMObject.PurchaseMaterialTypes.Distilled)
                    {
                        try
                        {
                            MaterialType matType = new MaterialType();
                            matType.MaterialDictID = materialDictID;
                            matType.Name = "Distilled";
                            db.MaterialType.Add(matType);
                            db.SaveChanges();
                            retMthdExecResult = true;
                        }
                        catch
                        {
                            return false;
                        }
                    }

                    if (rawMObject.PurchaseMaterialTypes.Fermentable)
                    {
                        try
                        {
                            MaterialType matType = new MaterialType();
                            matType.MaterialDictID = materialDictID;
                            matType.Name = "Fermentable";
                            db.MaterialType.Add(matType);
                            db.SaveChanges();
                            retMthdExecResult = true;
                        }
                        catch
                        {
                            return false;
                        }
                    }

                    if (rawMObject.PurchaseMaterialTypes.Fermented)
                    {
                        try
                        {
                            MaterialType matType = new MaterialType();
                            matType.MaterialDictID = materialDictID;
                            matType.Name = "Fermented";
                            db.MaterialType.Add(matType);
                            db.SaveChanges();
                            retMthdExecResult = true;
                        }
                        catch
                        {
                            return false;
                        }
                    }

                    if (rawMObject.PurchaseMaterialTypes.Supply)
                    {
                        try
                        {
                            MaterialType matType = new MaterialType();
                            matType.MaterialDictID = materialDictID;
                            matType.Name = "Supply";
                            db.MaterialType.Add(matType);
                            db.SaveChanges();
                            retMthdExecResult = true;
                        }
                        catch
                        {
                            return false;
                        }
                    }
                    retMthdExecResult = true;
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Failed to Update Raw Material Record: " + e);
                    return retMthdExecResult;
                }
            }

            return retMthdExecResult;
        }

        /// <summary>
        /// This method gets the list of Spirits
        /// </summary>
        ///  <param name="userId"></param>
        /// <returns>List<SpiritObject></returns>
        public List<SpiritObject> GetSpiritList(int userId)
        {
            List<SpiritObject> spiritList = new List<SpiritObject>();
            try
            {
                var recs =
                    from spirit in db.Spirit
                    join us2Distills in db.AspNetUserToDistiller on new { DistillerID = spirit.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                    from us2Distills in us2Distills_join.DefaultIfEmpty()
                    where
                      us2Distills.UserId == userId
                    select new
                    {
                        SpiritID = (System.Int32?)spirit.SpiritID ?? (System.Int32?)0,
                        Name = spirit.Name ?? string.Empty,
                        Note = spirit.Note ?? string.Empty
                    };

                foreach (var iter in recs)
                {
                    var curSpirit = new SpiritObject();
                    curSpirit.SpiritId = (int)iter.SpiritID;
                    curSpirit.SpiritName = iter.Name;
                    curSpirit.Note = iter.Note;
                    spiritList.Add(curSpirit);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error retrieving Spirit List: " + e);
            }
            return spiritList;
        }

        /// <summary>
        /// This method gets the list of Vendors
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>List<VendorObject></returns>
        public List<VendorObject> GetVendorList(int userId)
        {
            List<VendorObject> vendorList = new List<VendorObject>();
            int distillerId = GetDistillerId(userId);

            try
            {
                var VendorFinalResults =
                    from vendRes in db.Vendor
                    join vendDetails in db.VendorDetail on vendRes.VendorID equals vendDetails.VendorID into vendDetails_join
                    from vendDetails in vendDetails_join.DefaultIfEmpty()
                    where vendRes.DistillerID == distillerId
                    select new
                    {
                        vendRes.VendorID,
                        vendRes.Name,
                        Note = (vendDetails.Note ?? string.Empty)
                    };
                foreach (var vendorRes in VendorFinalResults)
                {
                    var curVendor = new VendorObject();
                    curVendor.VendorId = vendorRes.VendorID;
                    curVendor.VendorName = vendorRes.Name;
                    curVendor.Note = vendorRes.Note;
                    vendorList.Add(curVendor);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error getting Vendor list : " + e);
            }

            return vendorList;
        }

        /// <summary>
        /// GetStorageList queries db for Storage List
        /// </summary>
        /// <returns>List<StorageObject></returns>
        public List<StorageObject> GetStorageList(int userId)
        {
            var storageList = new List<StorageObject>();
            try
            {
                var storFinalResult =
                    from storage in db.Storage
                    join us2Distills in db.AspNetUserToDistiller on new { DistillerID = storage.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                    from us2Distills in us2Distills_join.DefaultIfEmpty()
                    where
                      us2Distills.UserId == userId
                    select new
                    {
                        StorageID = (System.Int32?)storage.StorageID ?? (System.Int32?)0,
                        Name = storage.Name ?? string.Empty,
                        Capacity = (System.Single?)storage.Capacity ?? (System.Single?)0,
                        SerialNumber = storage.SerialNumber ?? string.Empty,
                        Note = storage.Note ?? string.Empty
                    };

                foreach (var storRes in storFinalResult)
                {
                    var curStor = new StorageObject();
                    curStor.StorageId = (int)storRes.StorageID;
                    curStor.StorageName = storRes.Name;
                    curStor.SerialNumber = storRes.SerialNumber;
                    curStor.Note = storRes.Note;
                    storageList.Add(curStor);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error getting storage list : " + e);
            }

            return storageList;
        }

        internal bool DeleteDictionaryRecord(int userId, DeleteRecordObject deleteObject)
        {
            int RecordID = deleteObject.DeleteRecordID;
            string RecordType = deleteObject.DeleteRecordType;
            bool retMthdExecResult = false;
            if (RecordID > 0)
            {
                try
                {
                    if (RecordType == "RawMaterial")
                    {
                        var res = from rec in db.Purchase
                                  where rec.MaterialDictID == RecordID
                                  select rec;

                        var recCount = res.Count();

                        if (recCount == 0)
                            retMthdExecResult = DeleteRawMaterial(userId, RecordID);
                        else
                            retMthdExecResult = false;
                    }
                    else if (RecordType == "Spirit")
                    {
                        var res = from rec in db.ProductionToSpirit
                                  where rec.SpiritID == RecordID
                                  select rec;
                        var recCount = res.Count();

                        if (recCount == 0)
                            retMthdExecResult = DeleteSpirit(userId, RecordID);
                        else
                            retMthdExecResult = false;
                    }
                    else if (RecordType == "Storage")
                    {
                        var res = from rec in db.StorageToRecord
                                  where rec.StorageID == RecordID
                                  select rec;
                        var recCount = res.Count();

                        if (recCount == 0)
                            retMthdExecResult = DeleteStorage(userId, RecordID);
                        else
                            retMthdExecResult = false;
                    }
                    else if (RecordType == "Vendor")
                    {
                        var res = from rec in db.Purchase
                                  where rec.VendorID == RecordID
                                  select rec;

                        var recCount = res.Count();

                        if (recCount == 0)
                            retMthdExecResult = DeleteVendor(userId, RecordID);
                        else
                            retMthdExecResult = false;
                    }
                    else
                    {
                        retMthdExecResult = false;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Failed to delete " + RecordType + ": " + e);
                    retMthdExecResult = false;
                }
            }
            else
            {
                retMthdExecResult = false;
            }
            return retMthdExecResult;
        }

        private bool DeleteVendor(int userId, int vendorID)
        {
            bool retMthdExecResult = false;
            int distillerId = GetDistillerId(userId);
            if (vendorID > 0)
            {
                try
                {
                    var recs =
                        (from rec in db.VendorDetail
                         where rec.VendorID == vendorID
                         select rec).FirstOrDefault();

                    if (recs != null)
                    {
                        db.VendorDetail.Remove(recs);
                        db.SaveChanges();
                    }

                    var recs1 =
                        (from rec1 in db.Vendor
                         where rec1.VendorID == vendorID && rec1.DistillerID == distillerId
                         select rec1).FirstOrDefault();

                    if (recs1 != null)
                    {
                        db.Vendor.Remove(recs1);
                        db.SaveChanges();
                    }

                    retMthdExecResult = true;
                }
                catch (Exception e)
                {
                    // Spirit?
                    Debug.WriteLine("Failed to delete Spirit: " + e);
                    retMthdExecResult = false;
                }
            }
            else
            {
                retMthdExecResult = false;
            }
            return retMthdExecResult;
        }

        private bool DeleteSpirit(int userId, int spiritID)
        {
            bool retMthdExecResult = false;
            int distillerId = GetDistillerId(userId);
            if (spiritID > 0)
            {
                try
                {
                    var recs =
                        (from rec in db.Spirit
                         where rec.SpiritID == spiritID && rec.DistillerID == distillerId
                         select rec).FirstOrDefault();
                    if (recs != null)
                    {
                        db.Spirit.Remove(recs);
                        db.SaveChanges();
                    }
                    retMthdExecResult = true;
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Failed to delete Spirit: " + e);
                    retMthdExecResult = false;
                }
            }
            else
            {
                retMthdExecResult = false;
            }
            return retMthdExecResult;
        }

        private bool DeleteStorage(int userId, int storageID)
        {
            bool retMthdExecResult = false;
            int distillerId = GetDistillerId(userId);
            if (storageID >= 0)
            {
                try
                {
                    var recs =
                        (from rec in db.Storage
                         where rec.StorageID == storageID && rec.DistillerID == distillerId
                         select rec).FirstOrDefault();
                    if (recs != null)
                    {
                        db.Storage.Remove(recs);
                        db.SaveChanges();
                    }
                    retMthdExecResult = true;
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Failed to delete Storage: " + e);
                    retMthdExecResult = false;
                }
            }
            else
            {
                retMthdExecResult = false;
            }
            return retMthdExecResult;
        }

        private bool DeleteRawMaterial(int userId, int rawMaterialID)
        {
            bool retMthdExecResult = false;
            int distillerId = GetDistillerId(userId);
            if (rawMaterialID >= 0)
            {
                try
                {
                    var recs1 =
                        (from rec in db.MaterialDict
                         where rec.MaterialDictID == rawMaterialID && rec.DistillerID == distillerId
                         select rec).FirstOrDefault();
                    if (recs1 != null)
                    {
                        db.MaterialDict.Remove(recs1);
                        db.SaveChanges();
                    }
                    var recs2 =
                        (from rec in db.MaterialType
                         where rec.MaterialDictID == rawMaterialID
                         select rec).FirstOrDefault();
                    if (recs2 != null)
                    {
                        db.MaterialType.Remove(recs2);
                        db.SaveChanges();
                    }
                    retMthdExecResult = true;
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Failed to delete RawMaterial: " + e);
                    retMthdExecResult = false;
                }
            }
            else
            {
                retMthdExecResult = false;
            }
            return retMthdExecResult;
        }

        /// <summary>
        /// Retrieves a list of processing report types
        /// </summary>
        ///  <param name="userId"></param>
        /// <returns>List<SpiritObject></returns>
        public List<ProcessingReportTypeObject> GetProcessingReportTypes()
        {
            List<ProcessingReportTypeObject> types = new List<ProcessingReportTypeObject>();

            try
            {
                var res = from rec in db.ProcessingReportType
                          select rec;

                foreach (var r in res)
                {
                    var type = new ProcessingReportTypeObject();
                    type.Id = r.ProcessingReportTypeID;
                    type.Name = r.ProcessingReportTypeName;
                    types.Add(type);
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine("Error retrieving a list of processing report types: " + e);
            }

            return types;
        }
        #endregion

        #region Purchase Methods

        /// <summary>
        /// GetDistillerID retrieves DistillerId for given UserId
        /// </summary>
        public int GetDistillerId(int userId)
        {
            int distillerId = (from rec in db.AspNetUserToDistiller
                               where rec.UserId == userId
                               select rec.DistillerID).FirstOrDefault();
            return distillerId;
        }

        /// <summary>
        /// CreatePurchase Method creates a new Purchase Record
        /// </summary>
        /// <param name="purchaseObject"></param>
        /// <returns>int</returns>
        public int CreatePurchase(PurchaseObject purchaseObject, int userId)
        {
            int retMthdExecResult = 0;

            try
            {
                Purchase purchT = new Purchase();
                purchT.PurchaseName = purchaseObject.PurBatchName;
                purchT.PurchaseDate = purchaseObject.PurchaseDate;
                purchT.MaterialDictID = purchaseObject.RecordId;
                purchT.Note = purchaseObject.Note;
                purchT.Price = purchaseObject.Price;
                purchT.VendorID = purchaseObject.VendorId;
                purchT.DistillerID = GetDistillerId(userId);

                var pTypes =
                    (from rec in db.PurchaseType
                     where rec.Name == purchaseObject.PurchaseType
                     select rec).FirstOrDefault();

                if (pTypes != null)
                {
                    purchT.PurchaseTypeID = pTypes.PurchaseTypeID;
                }

                if (purchaseObject.Quantity > 0 && purchaseObject?.Quantity != null)
                {
                    Volume quantG = new Volume();
                    quantG.Value = purchaseObject.Quantity;
                    db.Volume.Add(quantG);
                    db.SaveChanges();

                    purchT.VolumeID = quantG.VolumeID;
                }
                else
                {
                    purchT.VolumeID = 0;
                }

                if (purchaseObject.VolumeByWeight > 0 && purchaseObject?.VolumeByWeight != null)
                {
                    Weight vBW = new Weight();
                    vBW.Value = purchaseObject.VolumeByWeight;
                    db.Weight.Add(vBW);
                    db.SaveChanges();

                    purchT.WeightID = vBW.WeightID;
                }
                else
                {
                    purchT.WeightID = 0;
                }

                if (purchaseObject.AlcoholContent > 0 && purchaseObject?.AlcoholContent != null)
                {
                    Alcohol alc = new Alcohol();
                    alc.Value = purchaseObject.AlcoholContent;
                    db.Alcohol.Add(alc);
                    db.SaveChanges();

                    purchT.AlcoholID = alc.AlcoholID;
                }
                else
                {
                    purchT.AlcoholID = 0;
                }

                if (purchaseObject.ProofGallon > 0 && purchaseObject?.ProofGallon != null)
                {
                    Proof proof = new Proof();
                    proof.Value = purchaseObject.ProofGallon;
                    db.Proof.Add(proof);
                    db.SaveChanges();

                    purchT.ProofID = proof.ProofID;
                }
                else
                {
                    purchT.ProofID = 0;
                }

                purchT.StatusID =
                    (from rec in db.Status
                     where rec.Name == "Active"
                     select rec.StatusID).FirstOrDefault();

                purchT.StateID =
                    (from rec in db.State
                     where rec.Name == purchaseObject.PurchaseType
                     select rec.StateID).FirstOrDefault();

                db.Purchase.Add(purchT);
                db.SaveChanges();

                // Only fermented and distilled purchase records can be reported on storage report
                if ((pTypes.PurchaseTypeID == 2 || pTypes.PurchaseTypeID == 3) && purchaseObject?.SpiritTypeReportingID != null && purchaseObject?.SpiritTypeReportingID != 0)
                {
                    PurchaseToSpiritTypeReporting pstr = new PurchaseToSpiritTypeReporting();
                    pstr.PurchaseID = purchT.PurchaseID;
                    pstr.SpiritTypeReportingID = purchaseObject.SpiritTypeReportingID;
                    db.PurchaseToSpiritTypeReporting.Add(pstr);
                    db.SaveChanges();

                    // Persistent Reporting: call Update Storage Report method here
                    ReportRepository reportRepository = new ReportRepository();
                    reportRepository.UpdateReportDataDuringPurchase(purchaseObject, userId);
                }

                //update StorageToRecord
                if (purchaseObject.Storage != null)
                {
                    foreach (var iter in purchaseObject.Storage)
                    {
                        StorageToRecord storToRec = new StorageToRecord();
                        storToRec.StorageID = iter.StorageId;
                        storToRec.RecordId = purchT.PurchaseID;
                        storToRec.TableIdentifier = "pur";
                        db.StorageToRecord.Add(storToRec);
                        db.SaveChanges();
                    }
                }

                if (purchT.PurchaseTypeID == 1 /*fermentable*/ || purchT.PurchaseTypeID == 2 /*fermented*/ || purchT.PurchaseTypeID == 3 /*distilled*/|| purchT.PurchaseTypeID == 5/*additive*/)
                {
                    try
                    {
                        // save purchase fermentable, fermented and distil data and quantities into Purchase4Reporting table which is used for reporting
                        Purchase4Reporting purch4RepT = new Purchase4Reporting();
                        purch4RepT.PurchaseID = purchT.PurchaseID;
                        purch4RepT.Weight = purchaseObject.VolumeByWeight;
                        purch4RepT.Volume = purchaseObject.Quantity;
                        purch4RepT.Proof = purchaseObject.ProofGallon;
                        purch4RepT.Alcohol = purchaseObject.AlcoholContent;
                        purch4RepT.Redistilled = false;

                        db.Purchase4Reporting.Add(purch4RepT);
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        throw;
                    }
                }

                retMthdExecResult = purchT.PurchaseID;

                // now, lets' try to save to history table
                purchaseObject.PurchaseId = purchT.PurchaseID;
                purchaseObject.Status = "Active";
                SavePurchaseHistory(purchaseObject, userId);
            }
            catch (Exception e)
            {
                retMthdExecResult = 0;
                throw e;
            }

            return retMthdExecResult;
        }

        /// <summary>
        /// SavePurchaseHistory method executes an insertion into PurchaseHistorty table for audit purposes
        /// </summary>
        /// <param name="purObject"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        internal bool SavePurchaseHistory(PurchaseObject purObject, int userId)
        {
            bool retMthdExecResult = false;
            try
            {
                PurchaseHistory purH = new PurchaseHistory();
                purH.PurchaseID = purObject.PurchaseId;
                purH.PurchaseName = purObject.PurBatchName;
                purH.Price = purObject.Price;
                purH.Vendor = purObject.VendorName;
                purH.Volume = purObject.Quantity;
                purH.Weight = purObject.VolumeByWeight;
                purH.Alcohol = purObject.AlcoholContent;
                purH.Proof = purObject.ProofGallon;

                if (purObject.PurchaseDate != DateTime.MinValue)
                {
                    purH.PurchaseDate = purObject.PurchaseDate;
                }
                purH.Note = purObject.Note;
                purH.State = purObject.PurchaseType;
                purH.Status = purObject.Status;
                purH.Gauged = purObject.Gauged;
                purH.UserID = userId;
                purH.UpdateDate = DateTime.UtcNow;


                if (purObject.Storage != null)
                {
                    StringBuilder storageStr = new StringBuilder();
                    foreach (var k in purObject.Storage)
                    {
                        storageStr.Append(k.StorageName)
                            .Append(";");
                    }
                    purH.Storage = storageStr.ToString();
                }

                purH.RecordName = purObject.RecordName;
                purH.SpiritTypeReportingID = purH.SpiritTypeReportingID;

                db.PurchaseHistory.Add(purH);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                retMthdExecResult = false;
                throw e;
            }

            return retMthdExecResult;
        }

        /// <summary>
        /// GetPurchasesList accumulates purchase information to be sent ot front end for view
        /// *Algorithm*:
        /// get list of pruchaseIds, purchase date and record id from Purchase table table for a particular purchase type.Ex. "Fermentable"
        /// get Price for a given purchaseId
        /// get RawMaterial name for a given purchaseId (this is applicable only for Fermentable and Supply purchases, in all other cases, names should be generic distilled or fermented)
        /// get Quantity for a given purchaseId
        /// get Storage for a given  purchaseId
        /// get Vendor for a given purchaseId
        /// get Note for a give purchaseId
        ///
        /// </summary>
        /// <param name="purchaseType"></param>
        /// <returns>List<PurchaseObject></returns>
        public List<PurchaseObject> GetPurchasesList(string purchaseType, int userId)
        {

            List<PurchaseObject> purchaseList = new List<PurchaseObject>();

            var res =
                from purchT in db.Purchase
                join purType in db.PurchaseType on purchT.PurchaseTypeID equals purType.PurchaseTypeID into purType_join
                from purType in purType_join.DefaultIfEmpty()
                join distiller in db.AspNetUserToDistiller on purchT.DistillerID equals distiller.DistillerID into distiller_join
                from distiller in distiller_join.DefaultIfEmpty()
                join material in db.MaterialDict on purchT.MaterialDictID equals material.MaterialDictID into material_join
                from material in material_join.DefaultIfEmpty()
                join vendor in db.Vendor on purchT.VendorID equals vendor.VendorID into vendor_join
                from vendor in vendor_join.DefaultIfEmpty()
                join galQuant in db.Volume on purchT.VolumeID equals galQuant.VolumeID into galQuant_join
                from galQuant in galQuant_join.DefaultIfEmpty()
                join VBW in db.Weight on purchT.WeightID equals VBW.WeightID into VBW_join
                from VBW in VBW_join.DefaultIfEmpty()
                join alc in db.Alcohol on purchT.AlcoholID equals alc.AlcoholID into alc_join
                from alc in alc_join.DefaultIfEmpty()
                join proof in db.Proof on purchT.ProofID equals proof.ProofID into proof_join
                from proof in proof_join.DefaultIfEmpty()
                join states in db.State on purchT.StateID equals states.StateID into states_join
                from states in states_join.DefaultIfEmpty()
                join statuses in db.Status on purchT.StatusID equals statuses.StatusID into statuses_join
                from statuses in statuses_join.DefaultIfEmpty()
                where
                    distiller.UserId == userId &&
                    purType.Name == purchaseType &&
                    statuses.Name != "Deleted" &&
                    statuses.Name != "Destroyed" &&
                    statuses.Name != "Closed"
                select new
                {
                    purchT.PurchaseID,
                    purchT.PurchaseName,
                    purchT.Price,
                    purchT.PurchaseDate,
                    PurchaseNote = purchT.Note,
                    PurchaseType = purType.Name,
                    MaterialName = (material.Name ?? string.Empty),
                    VendorName = vendor.Name,
                    Gallons = ((System.Single?)galQuant.Value ?? (System.Single?)0),
                    VolumeByWeight = ((System.Single?)VBW.Value ?? (System.Single?)0),
                    Alcohol = ((System.Single?)alc.Value ?? (System.Single?)0),
                    Proof = ((System.Single?)proof.Value ?? (System.Single?)0),
                    State = (states.Name ?? string.Empty),
                    Status = (statuses.Name ?? string.Empty)
                };

            foreach (var iterator in res)
            {
                PurchaseObject purchase = new PurchaseObject();
                purchase.PurchaseId = iterator.PurchaseID;
                purchase.RecordName = iterator.MaterialName;
                purchase.PurchaseType = iterator.PurchaseType;
                purchase.Note = iterator.PurchaseNote;
                purchase.PurchaseDate = iterator.PurchaseDate;
                purchase.Price = iterator.Price;
                purchase.Quantity = (float)iterator.Gallons;
                purchase.VendorName = iterator.VendorName;
                purchase.VolumeByWeight = (float)iterator.VolumeByWeight;
                purchase.AlcoholContent = (float)iterator.Alcohol;
                purchase.ProofGallon = (float)iterator.Proof;
                purchase.PurBatchName = iterator.PurchaseName;
                purchase.State = iterator.State;
                purchase.Status = iterator.Status;

                purchaseList.Add(purchase);
            }

            // now, let's get multiple storages
            foreach (var i in purchaseList)
            {
                List<StorageObject> storageL = new List<StorageObject>();
                var storages =
                    from rec in db.StorageToRecord
                    join stoName in db.Storage on rec.StorageID equals stoName.StorageID
                    where rec.RecordId == i.PurchaseId && rec.TableIdentifier == "pur"
                    select new
                    {
                        stoName.Name,
                        rec.StorageID
                    };
                if (storages != null)
                {
                    foreach (var it in storages)
                    {
                        StorageObject stor = new StorageObject();
                        stor.StorageId = it.StorageID;
                        stor.StorageName = it.Name;
                        storageL.Add(stor);
                    }
                }
                i.Storage = storageL;
            }

            return purchaseList;
        }

        #endregion

        #region Production Methods
        /// <summary>
        /// CreateProduction Method creates a new Production Record
        /// </summary>
        /// <param name="prodObject"></param>
        /// <returns>int</returns> 
        public int CreateProduction(ProductionObject prodObject, int userId)
        {
            //define method execution return value to be false by default
            int retMthdExecResult = 0;

            var distillerId = GetDistillerId(userId);

            prodObject.StatusName = "Active";

            Production prod = new Production();
            prod.ProductionName = prodObject.BatchName;
            prod.DistillerID = distillerId;
            prod.ProductionDate = prodObject.ProductionDate;
            prod.ProductionStartTime = prodObject.ProductionStart;
            prod.ProductionEndTime = prodObject.ProductionEnd;
            prod.Note = prodObject.Note;

            if (prodObject.Gauged)
            {
                prod.Gauged = prodObject.Gauged;
            }
            else if (!prodObject.Gauged)
            {
                prod.Gauged = false;
            }

            var pTypes =
                    (from rec in db.ProductionType
                     where rec.Name == prodObject.ProductionType
                     select rec).FirstOrDefault();

            if (pTypes != null)
            {
                prod.ProductionTypeID = pTypes.ProductionTypeID;
            }

            if (prodObject.Quantity > 0 && prodObject?.Quantity != null)
            {
                Volume quantG = new Volume();
                quantG.Value = prodObject.Quantity;
                db.Volume.Add(quantG);
                db.SaveChanges();

                prod.VolumeID = quantG.VolumeID;
            }
            else
            {
                prod.VolumeID = 0;
            }

            if (prodObject.VolumeByWeight > 0 && prodObject?.VolumeByWeight != null)
            {
                Weight vBW = new Weight();
                vBW.Value = prodObject.VolumeByWeight;
                db.Weight.Add(vBW);
                db.SaveChanges();

                prod.WeightID = vBW.WeightID;
            }
            else
            {
                prod.WeightID = 0;
            }

            if (prodObject.AlcoholContent > 0 && prodObject?.AlcoholContent != null)
            {
                Alcohol alc = new Alcohol();
                alc.Value = prodObject.AlcoholContent;
                db.Alcohol.Add(alc);
                db.SaveChanges();

                prod.AlcoholID = alc.AlcoholID;
            }
            else
            {
                prod.AlcoholID = 0;
            }

            if (prodObject.ProofGallon > 0 && prodObject?.ProofGallon != null)
            {
                Proof proof = new Proof();
                proof.Value = prodObject.ProofGallon;
                db.Proof.Add(proof);
                db.SaveChanges();

                prod.ProofID = proof.ProofID;
            }
            else
            {
                prod.ProofID = 0;
            }

            //this part here is where we assign a state to the 
            if (prodObject.ProductionType == "Fermentation")
            {
                prod.StateID =
                    (from rec in db.State
                     where rec.Name == "Fermented"
                     select rec.StateID).FirstOrDefault();
            }
            else if (prodObject.ProductionType == "Distillation")
            {
                prod.StateID =
                    (from rec in db.State
                     where rec.Name == "Distilled"
                     select rec.StateID).FirstOrDefault();
            }
            else if (prodObject.ProductionType == "Blending")
            {
                prod.StateID =
                    (from rec in db.State
                     where rec.Name == "Blended"
                     select rec.StateID).FirstOrDefault();
            }
            else if (prodObject.ProductionType == "Bottling")
            {
                prod.StateID =
                    (from rec in db.State
                     where rec.Name == "Bottled"
                     select rec.StateID).FirstOrDefault();
            }

            prod.StatusID =
                (from rec in db.Status
                 where rec.Name == "Active"
                 select rec.StatusID).FirstOrDefault();

            // save new records in Production table
            db.Production.Add(prod);
            db.SaveChanges();

            if (prodObject.Storage != null)
            {
                //update StorageToRecord
                foreach (var iter in prodObject.Storage)
                {
                    StorageToRecord storToRec = new StorageToRecord();
                    storToRec.StorageID = iter.StorageId;
                    storToRec.RecordId = prod.ProductionID;
                    storToRec.TableIdentifier = "prod";
                    db.StorageToRecord.Add(storToRec);
                    db.SaveChanges();
                }
            }

            if (prodObject?.SpiritTypeReportingID > 0)
            {
                ProductionToSpiritTypeReporting prodToSpirType = new ProductionToSpiritTypeReporting();
                prodToSpirType.SpiritTypeReportingID = prodObject.SpiritTypeReportingID;
                if (prodObject?.MaterialKindReportingID != 0)
                {
                    prodToSpirType.MaterialKindReportingID = prodObject.MaterialKindReportingID;
                }
                prodToSpirType.ProductionID = prod.ProductionID;
                db.ProductionToSpiritTypeReporting.Add(prodToSpirType);
                db.SaveChanges();
            }

            if (prodObject.ProductionType == "Fermentation")
            {
                // in this section, we need to handle used materials. update its statuses, values, states
                if (prodObject.UsedMats != null)
                {
                    // handle updating records that are being used for creating this production record
                    UpdateRecordsUsedInProductionWorkflow(prodObject.UsedMats, prod.ProductionID, userId);

                    // call Update Storage report table method here
                }
            }

            else if (prodObject.ProductionType == "Distillation")
            {
                try
                {
                    // verify list of batches received from the front-end and used in distillation is not empty
                    if (prodObject.UsedMats != null)
                    {
                        // handle updating records that are being used for creating this production record
                        UpdateRecordsUsedInProductionWorkflow(prodObject.UsedMats, prod.ProductionID, userId);

                        if (prodObject?.SpiritCutId != null)
                        {
                            ProductionToSpiritCut prodToSCut = new ProductionToSpiritCut();
                            prodToSCut.SpiritCutID = prodObject.SpiritCutId;
                            prodToSCut.ProductionID = prod.ProductionID;
                            db.ProductionToSpiritCut.Add(prodToSCut);
                            db.SaveChanges();
                        }

                        // analyze prodObject.UsedMats contents here (Perhaps, method returning waht reports should be udpated with some extra information?)
                        // extract Spirit Information from prodObject.UsedMats so we know what we need to update in Storage Report and Production reports
                        // some of the questions to be answered:
                        // 1. Is the material being used Wine that was produced in-house? then we need to update wine column in Storage report, Line 25 in Production report, Part 5 of Production Report, Part 6 of Production Report
                        // 2. Is this gauged distil and the quarter end reporting month? If so, weneed to aslo update Line 17 (a and b) 
                        // call Update Storage and Production reports table method here
                    }
                }
                catch (Exception e)
                {
                    retMthdExecResult = 0;
                    throw e;
                }
            }

            else if (prodObject.ProductionType == "Blending")
            {
                if (prodObject.UsedMats != null) // todo: we need to makre sure that in Production workflow front-end we assign either raw materials or distil IDs to it
                {
                    // handle updating records that are being used for creating this production record
                    UpdateRecordsUsedInProductionWorkflow(prodObject.UsedMats, prod.ProductionID, userId);

                    if (prodObject?.SpiritId != null)
                    {
                        ProductionToSpirit prodToSpirit = new ProductionToSpirit();
                        prodToSpirit.SpiritID = prodObject.SpiritId;
                        prodToSpirit.ProductionID = prod.ProductionID;
                        db.ProductionToSpirit.Add(prodToSpirit);
                        db.SaveChanges();
                    }

                    // update Blended Components related information
                    if (prodObject.BlendingAdditives != null)
                    {
                        foreach (var i in prodObject.BlendingAdditives)
                        {
                            BlendedComponent bC = new BlendedComponent();
                            bC.ProductionID = prod.ProductionID;
                            bC.RecordId = i.RawMaterialId;
                            bC.Quantity = i.RawMaterialQuantity;
                            bC.UnitOfMeasurement = i.UnitOfMeasurement;
                            db.BlendedComponent.Add(bC);
                            db.SaveChanges();
                        }
                    }

                    // analyze prodObject.UsedMats contents here. (Perhaps, method returning waht reports should be udpated with some extra information?)
                    // extract Spirit Information from prodObject.UsedMats so we know what we need to update in Storage Report and Production reports
                    // some of the questions to be answered:
                    // 1. Is the material being used was entered in his reporting period or previous? If it's from current reporting period, then we update Line 9 of Production report. If from previous reporting period, then we need to update line 17 of Storage report.
                    // 2. 
                    // call Update Storage, Production and Processing reports table method here
                }
            }

            else if (prodObject.ProductionType == "Bottling")
            {
                string statusString = string.Empty;

                List<int> purIdL = new List<int>(); // this is used as a temp holder for purchase ids

                if (prodObject.UsedMats != null) // we need to makre sure that in Production workflow front-end we assign either raw materials or distil IDs to it
                {
                    // handle updating records that are being used for creating this production record
                    UpdateRecordsUsedInProductionWorkflow(prodObject.UsedMats, prod.ProductionID, userId);

                    if (prodObject?.SpiritId != null)
                    {
                        ProductionToSpirit prodToSpirit = new ProductionToSpirit();
                        prodToSpirit.SpiritID = prodObject.SpiritId;
                        prodToSpirit.ProductionID = prod.ProductionID;
                        db.ProductionToSpirit.Add(prodToSpirit);
                    }

                    // now, lets register gains/losses
                    if (prodObject.GainLoss > 0)
                    {
                        // gain
                        GainLoss glt = new GainLoss();
                        glt.Type = true;
                        glt.Quantity = prodObject.GainLoss;
                        glt.DateRecorded = DateTime.UtcNow;
                        glt.BottledRecordId = prod.ProductionID;
                        db.GainLoss.Add(glt);
                        db.SaveChanges();
                    }
                    else if (prodObject.GainLoss < 0)
                    {
                        // loss
                        GainLoss glt = new GainLoss();
                        glt.Type = false;
                        glt.Quantity = Math.Abs(prodObject.GainLoss); // since cumulativeGainLoss is negative, making it to be positive
                        glt.DateRecorded = DateTime.UtcNow;
                        glt.BottledRecordId = prod.ProductionID;
                        db.GainLoss.Add(glt);
                        db.SaveChanges();
                    }

                    // update Bottling Info related information
                    if (prodObject.BottlingInfo != null)
                    {
                        BottlingInfo bottI = new BottlingInfo();
                        bottI.ProductionID = prod.ProductionID;
                        bottI.CaseCapacity = prodObject.BottlingInfo.CaseCapacity;
                        bottI.CaseQuantity = prodObject.BottlingInfo.CaseQuantity;
                        bottI.BottleVolume = prodObject.BottlingInfo.BottleCapacity;
                        bottI.BottleQuantity = prodObject.BottlingInfo.BottleQuantity;

                        db.BottlingInfo.Add(bottI);
                        db.SaveChanges();
                    }

                    // update fillTest information.
                    if (prodObject.FillTestList != null)
                    {
                        foreach (var i in prodObject.FillTestList)
                        {
                            FillTest fillTest = new FillTest();
                            fillTest.ProductionID = prod.ProductionID;
                            fillTest.AlcoholContent = i.FillAlcoholContent;
                            fillTest.FillTestDate = i.FillDate;
                            fillTest.FillVariation = i.FillVariation;
                            fillTest.CorrectiveAction = i.CorrectiveAction;
                            db.FillTest.Add(fillTest);
                            db.SaveChanges();
                        }
                    }

                    // analyze prodObject.UsedMats contents here
                    // extract Spirit Information from prodObject.UsedMats so we know what we need to update Processing report
                    // some of the questions to be answered:
                    // 1. 
                    // 2. 
                    // call Update Processing report table method here
                }
            }
            try
            {
                // save production data and quantities into Production4Reporting table which is used for reporting
                Production4Reporting prod4RepT = new Production4Reporting();
                prod4RepT.ProductionID = prod.ProductionID;
                prod4RepT.Weight = prodObject.VolumeByWeight;
                prod4RepT.Volume = prodObject.Quantity;
                prod4RepT.Proof = prodObject.ProofGallon;
                prod4RepT.Alcohol = prodObject.AlcoholContent;
                prod4RepT.Redistilled = false;

                db.Production4Reporting.Add(prod4RepT);
                db.SaveChanges();
                retMthdExecResult = 0;
            }
            catch (Exception e)
            {
                retMthdExecResult = 0;
                throw;
            }

            // insert a record in the history table
            prodObject.ProductionId = prod.ProductionID;
            if (!SaveProductionHistory(prodObject, userId))
            {
                retMthdExecResult = 0;
            }

            return retMthdExecResult = prod.ProductionID;
        }

        internal void UpdateRecordsUsedInProductionWorkflow(List<ObjInfo4Burndwn> usedMats, int productionIDBeingCreated, int userId)
        {
            // list of purchase ids of purchase records and ids associated with production records
            // that are used in distillation process
            List<int> purIdL = new List<int>();

            try
            {
                // iterate through purchased and produced batches used in distillation
                foreach (var k in usedMats)
                {
                    var status = string.Empty;

                    // purchase batch used in the distillation
                    if (k.DistillableOrigin == "pur")
                    {
                        // update PurchaseHistory table
                        PurchaseObject purObj = new PurchaseObject();

                        purIdL.Add(k.ID);

                        var purch =
                            (from rec in db.Purchase
                             where rec.PurchaseID == k.ID
                             select rec).FirstOrDefault();

                        // all of the batch volume/weight used in the distillation
                        if (k.OldVal <= 0)
                        {
                            status = "Processed";
                            var statusId =
                                (from rec in db.Status
                                 where rec.Name == status
                                 select rec.StatusID).FirstOrDefault();

                            purch.StatusID = statusId;
                            purObj.Status = status;
                        }

                        // set burning down method for the batch used in distillation,
                        // if it hasn't been done yet, to "volume" or "weight"
                        if (purch.BurningDownMethod == null && k.BurningDownMethod != null)
                        {
                            purch.BurningDownMethod = k.BurningDownMethod;
                        }

                        // we need to make sure that if the purchased used material is being partially distilled, we need to create 
                        // a new distilate record in Purchase4Reporting table with the same Purchase ID but with different Proof/Volume/Weight value.
                        // else, the same record in Purchase4Reporting needs to be marked as redistilled for reporting purposes.
                        var p =
                            (from rec in db.Purchase4Reporting
                             where purch.PurchaseID == rec.PurchaseID
                             select rec).FirstOrDefault();

                        if (p != null)
                        {
                            if (p.Proof == k.Proof && p.PurchaseID == k.ID)
                            {
                                p.Redistilled = true;
                                p.ProductionID = productionIDBeingCreated;

                                db.SaveChanges();
                            }
                            else if (p.Proof != k.Proof && p.PurchaseID == k.ID)
                            {
                                Purchase4Reporting purch4Rep = new Purchase4Reporting();
                                purch4Rep.PurchaseID = k.ID;
                                purch4Rep.Proof = k.Proof;

                                if (k.BurningDownMethod == "weight" && purch.WeightID > 0)
                                {
                                    purch4Rep.Weight = k.OldVal;
                                }
                                if (k.BurningDownMethod == "volume" && purch.VolumeID > 0)
                                {
                                    purch4Rep.Volume = k.OldVal;
                                }
                                purch4Rep.Redistilled = true;
                                purch4Rep.ProductionID = productionIDBeingCreated;

                                db.Purchase4Reporting.Add(purch4Rep);
                                db.SaveChanges();
                            }
                        }

                        // update proof value after it has been recalculated
                        // on front-end using the new volume quantity and also
                        // store left over Proof into materials that are being burnt down
                        float tempProofGHolder = 0f;

                        if (purch.ProofID > 0 && k.Proof >= 0)
                        {
                            var proof =
                                (from rec in db.Proof
                                 where rec.ProofID == purch.ProofID
                                 select rec).FirstOrDefault();

                            if (proof != null)
                            {
                                tempProofGHolder = proof.Value - k.Proof;
                                proof.Value = k.Proof;
                            }

                            db.SaveChanges();
                        }

                        //todo: perhaps, we can re-use Production content workflow below to record Blending additives as well
                        // save to the ProductionContent table
                        List<ProductionContent> prodContentL = new List<ProductionContent>();

                        if (k.BurningDownMethod == "volume" && purch.VolumeID > 0)
                        {
                            if (purch.PurchaseTypeID == 1)
                            {
                                // PurFermentableVolume
                                ProductionContent prodContent = PrepareProductionContentTableInfo4Saving(productionIDBeingCreated, k.ID, 1, false, k.NewVal);
                                prodContentL.Add(prodContent);
                            }

                            if (purch.PurchaseTypeID == 2)
                            {
                                // PurFermentedVolume
                                ProductionContent prodContent = PrepareProductionContentTableInfo4Saving(productionIDBeingCreated, k.ID, 3, false, k.NewVal);
                                prodContentL.Add(prodContent);

                                ProductionContent prodContent4ProofG = PrepareProductionContentTableInfo4Saving(productionIDBeingCreated, k.ID, 16, false, tempProofGHolder);
                                prodContentL.Add(prodContent4ProofG);
                            }

                            if (purch.PurchaseTypeID == 3)
                            {
                                // PurDistilledVolume
                                ProductionContent prodContent = PrepareProductionContentTableInfo4Saving(productionIDBeingCreated, k.ID, 9, false, k.NewVal);
                                prodContentL.Add(prodContent);

                                ProductionContent prodContent4ProofG = PrepareProductionContentTableInfo4Saving(productionIDBeingCreated, k.ID, 18, false, tempProofGHolder);
                                prodContentL.Add(prodContent4ProofG);
                            }

                            var q =
                                (from rec in db.Volume
                                 where purch.VolumeID == rec.VolumeID
                                 select rec).FirstOrDefault();

                            if (q != null)
                            {
                                q.Value = k.OldVal;
                            }

                            purObj.Quantity = q.Value;
                        }

                        if (k.BurningDownMethod == "weight" && purch.WeightID > 0)
                        {
                            if (purch.PurchaseTypeID == 1)
                            {
                                // PurFermentableWeight
                                ProductionContent prodContent = PrepareProductionContentTableInfo4Saving(productionIDBeingCreated, k.ID, 2, false, k.NewVal);
                                prodContentL.Add(prodContent);
                            }

                            if (purch.PurchaseTypeID == 2)
                            {
                                // PurFermentedWeight
                                ProductionContent prodContent = PrepareProductionContentTableInfo4Saving(productionIDBeingCreated, k.ID, 4, false, k.NewVal);
                                prodContentL.Add(prodContent);
                            }

                            if (purch.PurchaseTypeID == 3)
                            {
                                // PurDistilledWeight
                                ProductionContent prodContent = PrepareProductionContentTableInfo4Saving(productionIDBeingCreated, k.ID, 10, false, k.NewVal);
                                prodContentL.Add(prodContent);
                            }

                            var vBW =
                                (from rec in db.Weight
                                 where rec.WeightID == purch.WeightID
                                 select rec).FirstOrDefault();

                            if (vBW != null)
                            {
                                vBW.Value = k.OldVal;
                            }

                            purObj.VolumeByWeight = k.OldVal;
                        }

                        db.ProductionContent.AddRange(prodContentL);
                        db.SaveChanges();

                        SavePurchaseHistory(purObj, userId);
                    }
                    // production batch used in the distillation
                    else if (k.DistillableOrigin == "prod")
                    {
                        ProductionObject prodObj = new ProductionObject();

                        prodObj.ProductionId = k.ID;

                        // query for purchaseIds associated with production record
                        // that is being used in the distillation
                        var prod2PurIds =
                            (from rec in db.ProductionToPurchase
                             where rec.ProductionID == k.ID
                             select rec.PurchaseID);

                        // add these purchaseIds to the list
                        if (prod2PurIds != null)
                        {
                            foreach (var i in prod2PurIds)
                            {
                                purIdL.Add(i);
                            }
                        }

                        var prodRec =
                            (from rec in db.Production
                             where rec.ProductionID == k.ID
                             select rec).FirstOrDefault();

                        // all of the batch volume/weight used in the distillation
                        if (k.OldVal <= 0)
                        {
                            status = "Processed";
                            var statusId =
                                (from rec in db.Status
                                 where rec.Name == status
                                 select rec.StatusID).FirstOrDefault();

                            prodRec.StatusID = statusId;

                            prodObj.StatusName = status;
                        }

                        // we need to make sure that if the used material that was produced by us is a distilate and being re-distiled again,
                        // it needs to be marked as redistilled for reporting purposes if all of the proof gallons are used. Else, we need to insert
                        // another record into Production4Reporting with the same ProductionID but with different Proof and volume/weight values.
                        var p =
                            (from rec in db.Production4Reporting
                             where prodRec.ProductionID == rec.ProductionID
                             select rec).FirstOrDefault();
                        if (p != null)
                        {
                            if (p.Proof == k.Proof && p.ProductionID == k.ID)
                            {
                                p.Redistilled = true;
                                db.SaveChanges();
                            }
                            else if (p.Proof != k.Proof && p.ProductionID == k.ID)
                            {
                                Production4Reporting prod4Rep = new Production4Reporting();
                                prod4Rep.ProductionID = k.ID;
                                prod4Rep.Proof = k.Proof;

                                if (k.BurningDownMethod == "weight" && prodRec.WeightID > 0)
                                {
                                    prod4Rep.Weight = k.OldVal;
                                }
                                if (k.BurningDownMethod == "volume" && prodRec.VolumeID > 0)
                                {
                                    prod4Rep.Volume = k.OldVal;
                                }
                                prod4Rep.Redistilled = true;
                                db.Production4Reporting.Add(prod4Rep);
                                db.SaveChanges();
                            }
                        }

                        // update proof value after it has been recalculated
                        // on front-end using the new volume quantity and also
                        // store left over Proof into materials that are being burnt down
                        float tempProofGHolder = 0f;

                        if (prodRec.ProofID > 0 && k.Proof >= 0)
                        {
                            var proof =
                                (from rec in db.Proof
                                 where rec.ProofID == prodRec.ProofID
                                 select rec).FirstOrDefault();

                            if (proof != null)
                            {
                                tempProofGHolder = proof.Value - k.Proof;
                                proof.Value = k.Proof;
                            }

                            db.SaveChanges();
                        }

                        // set burning down method for the batch used in distillation,
                        // if it hasn't been done yet, to "volume" or "weight"
                        if (prodRec.BurningDownMethod == null && k.BurningDownMethod != null)
                        {
                            prodRec.BurningDownMethod = k.BurningDownMethod;
                        }

                        // save to the ProductionContent table
                        List<ProductionContent> prodContentL = new List<ProductionContent>();

                        if (k.BurningDownMethod == "volume" && prodRec.VolumeID > 0)
                        {
                            if (prodRec.ProductionTypeID == 1)
                            {
                                // ProdFermentedVolume
                                ProductionContent prodContent = PrepareProductionContentTableInfo4Saving(productionIDBeingCreated, k.ID, 5, true, k.NewVal);
                                prodContentL.Add(prodContent);

                                ProductionContent prodContent4ProofG = PrepareProductionContentTableInfo4Saving(productionIDBeingCreated, k.ID, 23, true, tempProofGHolder);
                                prodContentL.Add(prodContent4ProofG);
                            }

                            if (prodRec.ProductionTypeID == 2)
                            {
                                // ProdDistilledVolume
                                ProductionContent prodContent = PrepareProductionContentTableInfo4Saving(productionIDBeingCreated, k.ID, 11, true, k.NewVal);
                                prodContentL.Add(prodContent);

                                ProductionContent prodContent4ProofG = PrepareProductionContentTableInfo4Saving(productionIDBeingCreated, k.ID, 20, true, tempProofGHolder);
                                prodContentL.Add(prodContent4ProofG);
                            }

                            if (prodRec.ProductionTypeID == 3)
                            {
                                // ProdBlendedVolume
                                ProductionContent prodContent = PrepareProductionContentTableInfo4Saving(productionIDBeingCreated, k.ID, 13, true, k.NewVal);
                                prodContentL.Add(prodContent);

                                ProductionContent prodContent4ProofG = PrepareProductionContentTableInfo4Saving(productionIDBeingCreated, k.ID, 22, true, tempProofGHolder);
                                prodContentL.Add(prodContent4ProofG);
                            }

                            db.ProductionContent.AddRange(prodContentL);

                            var q =
                                (from rec in db.Volume
                                 where prodRec.VolumeID == rec.VolumeID
                                 select rec).FirstOrDefault();

                            if (q != null)
                            {
                                q.Value = k.OldVal;
                            }

                            prodObj.Quantity = k.OldVal;

                            db.SaveChanges();
                        }

                        if (k.BurningDownMethod == "weight" && prodRec.WeightID > 0)
                        {
                            if (prodRec.ProductionTypeID == 1)
                            {
                                // ProdFermentedWeight
                                ProductionContent prodContent = PrepareProductionContentTableInfo4Saving(productionIDBeingCreated, k.ID, 6, true, k.NewVal);
                                prodContentL.Add(prodContent);

                                ProductionContent prodContent4ProofG = PrepareProductionContentTableInfo4Saving(productionIDBeingCreated, k.ID, 23, true, tempProofGHolder);
                                prodContentL.Add(prodContent4ProofG);
                            }

                            if (prodRec.ProductionTypeID == 2)
                            {
                                // ProdDistilledWeight
                                ProductionContent prodContent = PrepareProductionContentTableInfo4Saving(productionIDBeingCreated, k.ID, 12, true, k.NewVal);
                                prodContentL.Add(prodContent);

                                ProductionContent prodContent4ProofG = PrepareProductionContentTableInfo4Saving(productionIDBeingCreated, k.ID, 20, true, tempProofGHolder);
                                prodContentL.Add(prodContent4ProofG);
                            }

                            if (prodRec.ProductionTypeID == 3)
                            {
                                // ProdBlendedWeight
                                ProductionContent prodContent = PrepareProductionContentTableInfo4Saving(productionIDBeingCreated, k.ID, 14, true, k.NewVal);
                                prodContentL.Add(prodContent);

                                ProductionContent prodContent4ProofG = PrepareProductionContentTableInfo4Saving(productionIDBeingCreated, k.ID, 22, true, tempProofGHolder);
                                prodContentL.Add(prodContent4ProofG);
                            }

                            db.ProductionContent.AddRange(prodContentL);

                            var vBW =
                            (from rec in db.Weight
                             where prodRec.WeightID == rec.WeightID
                             select rec).FirstOrDefault();

                            if (vBW != null)
                            {
                                vBW.Value = k.OldVal;
                            }

                            prodObj.VolumeByWeight = k.OldVal;
                        }

                        db.SaveChanges();

                        SaveProductionHistory(prodObj, userId);
                    }

                    if (purIdL != null)
                    {
                        // iterate through list of purchaseIds of purchase records
                        // and purchase records associated with production records
                        // used in the distillation
                        foreach (var i in purIdL)
                        {
                            ProductionToPurchase prodToPur = new ProductionToPurchase();
                            prodToPur.ProductionID = productionIDBeingCreated;
                            prodToPur.PurchaseID = i;
                            db.ProductionToPurchase.Add(prodToPur);
                            db.SaveChanges();
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// PrepareProductionContentTableInfo4Saving method is used to prepare record contents for saving into ProductionContent table
        /// </summary>
        /// <param name="productionID">ProductionID with which used record values are being associated</param>
        /// <param name="usedRecID">ID of the record being used for creation of productionID</param>
        /// <param name="contentFieldID">ID of the type of a quantity</param>
        /// <param name="isProductionComponent"> boolean flag indicatting whether a record being used was produced in-house</param>
        /// <param name="contentValue"> quantity value of the material being used</param>
        /// <returns></returns>
        public ProductionContent PrepareProductionContentTableInfo4Saving(int productionID, int usedRecID, int contentFieldID, bool isProductionComponent, float contentValue)
        {
            ProductionContent productionContentInstance = new ProductionContent();
            productionContentInstance.ProductionID = productionID;
            productionContentInstance.RecordID = usedRecID;
            productionContentInstance.ContentFieldID = contentFieldID;
            productionContentInstance.isProductionComponent = isProductionComponent;
            productionContentInstance.ContentValue = contentValue;

            return productionContentInstance;
        }

        internal bool SaveProductionHistory(ProductionObject prodObject, int userId)
        {
            bool retMthdExecResult = false;

            try
            {
                ProductionHistory histTable = new ProductionHistory();
                histTable.ProductionID = prodObject.ProductionId;
                histTable.UpdateDate = DateTime.UtcNow;
                histTable.ProductionName = prodObject.BatchName;

                if (prodObject.ProductionStart != DateTime.MinValue)
                {
                    histTable.ProductionStartTime = prodObject.ProductionStart;
                }

                if (prodObject.ProductionEnd != DateTime.MinValue)
                {
                    histTable.ProductionEndTime = prodObject.ProductionEnd;
                }

                histTable.Volume = prodObject.Quantity;
                histTable.Weight = prodObject.VolumeByWeight;
                histTable.Alcohol = prodObject.AlcoholContent;
                histTable.Proof = prodObject.ProofGallon;
                histTable.Status = prodObject.StatusName;
                histTable.State = prodObject.ProductionType;
                histTable.Note = prodObject.Note;
                histTable.UserID = userId;
                histTable.Gauged = prodObject.Gauged;

                if (prodObject.Storage != null)
                {
                    StringBuilder storageString = new StringBuilder();

                    foreach (var k in prodObject.Storage)
                    {
                        storageString.Append(k.StorageName)
                            .Append("; ");
                    }

                    histTable.Storage = storageString.ToString();
                }

                if (prodObject.UsedMats != null)
                {
                    StringBuilder usedMats = new StringBuilder();

                    foreach (var t in prodObject.UsedMats)
                    {
                        usedMats.Append("{")
                            .Append(t.ID)
                            .Append(",")
                            .Append(t.NewVal)
                            .Append(",")
                            .Append(t.Proof)
                            .Append(",")
                            .Append(t.DistillableOrigin)
                            .Append(",")
                            .Append(t.BurningDownMethod)
                            .Append("}");
                    }

                    histTable.UsedMats = usedMats.ToString();
                }

                histTable.SpiritCutName = prodObject.SpiritCutName;

                if (prodObject.BlendingAdditives != null)
                {
                    StringBuilder blendingAdditives = new StringBuilder();

                    foreach (var l in prodObject.BlendingAdditives)
                    {
                        blendingAdditives.Append("{")
                            .Append(l.RawMaterialId)
                            .Append(",")
                            .Append(l.RawMaterialName)
                            .Append(",")
                            .Append(l.RawMaterialQuantity)
                            .Append(",")
                            .Append(l.UnitOfMeasurement)
                            .Append("}");
                    }
                }

                if (prodObject.BottlingInfo != null)
                {
                    StringBuilder bottInforStrBuilder = new StringBuilder();
                    bottInforStrBuilder.Append("{")
                        .Append(prodObject.BottlingInfo.CaseCapacity)
                        .Append(",")
                        .Append(prodObject.BottlingInfo.BottleCapacity)
                        .Append(",")
                        .Append(prodObject.BottlingInfo.CaseQuantity)
                        .Append(",")
                        .Append(prodObject.BottlingInfo.BottleQuantity)
                        .Append("}");
                    histTable.BottlingInfo = bottInforStrBuilder.ToString();
                }

                histTable.SpiritTypeReportingID = prodObject.SpiritTypeReportingID;

                histTable.MaterialKindReportingID = prodObject.MaterialKindReportingID;

                histTable.TaxedProof = prodObject.TaxedProof;

                if (prodObject.WithdrawalDate != DateTime.MinValue)
                {
                    histTable.WithdrawalDate = prodObject.WithdrawalDate;
                }

                db.ProductionHistory.Add(histTable);
                db.SaveChanges();

                retMthdExecResult = true;
            }
            catch (Exception e)
            {
                throw e;
            }

            return retMthdExecResult;
        }

        /// <summary>
        /// <returns>Returns original proof value
        /// </summary>
        /// <param name="proofId"></param>
        /// <param name="newProof"></param>
        /// <returns></returns>
        public float UpdateProof(int proofId, float newProof)
        {
            float oldProof = 0.0F;

            if (!(proofId > 0 && newProof >= 0))
            {
                throw new ArgumentOutOfRangeException("ProofId must be more than 0 and proof value must be more or equal to 0");
            }
            else
            {
                var res =
                    (from rec in db.Proof
                     where rec.ProofID == proofId
                     select rec).FirstOrDefault();

                if (res == null)
                {
                    throw new NullReferenceException("Unable to locate proof record with supplied ProofID");
                }
                else
                {
                    oldProof = res.Value;
                    res.Value = newProof;
                }

                db.SaveChanges();

                return oldProof;
            }
        }

        public bool DeleteProduction(ProductionObject productionObject, int userId)
        {
            bool retMthdExecResult = false;

            if (productionObject.ProductionId >= 0)
            {
                try
                {
                    var prodRec =
                        (from rec in db.Production
                         join distillers in db.AspNetUserToDistiller on rec.DistillerID equals distillers.DistillerID into distillers_join
                         from distillers in distillers_join.DefaultIfEmpty()
                         where rec.ProductionID == productionObject.ProductionId
                         && distillers.UserId == userId
                         select rec).FirstOrDefault();

                    if (prodRec != null)
                    {
                        // restore amounts for records that
                        // went into the creation of the record we are about to delete.
                        RestoreBurntdownRecords(prodRec.ProductionID);

                        var prod4Rep =
                           (from rec in db.Production4Reporting
                            where rec.ProductionID == prodRec.ProductionID
                            select rec).FirstOrDefault();

                        if (prod4Rep != null)
                        {
                            db.Production4Reporting.Remove(prod4Rep);
                        }

                        var prodC =
                            (from rec in db.ProductionContent
                             where rec.ProductionID == prodRec.ProductionID
                             select rec).FirstOrDefault();

                        if (prodC != null)
                        {
                            db.ProductionContent.Remove(prodC);
                        }

                        if (productionObject.ProductionType == "Distillation")
                        {
                            var p2scRec =
                                (from rec in db.ProductionToSpiritCut
                                 where rec.ProductionID == prodRec.ProductionID
                                 select rec).FirstOrDefault();

                            if (p2scRec != null)
                            {
                                db.ProductionToSpiritCut.Remove(p2scRec);
                            }

                            var prod2SpiTypeRep =
                               (from rec in db.ProductionToSpiritTypeReporting
                                where rec.ProductionID == prodRec.ProductionID
                                select rec).FirstOrDefault();

                            if (prod2SpiTypeRep != null)
                            {
                                db.ProductionToSpiritTypeReporting.Remove(prod2SpiTypeRep);
                            }
                        }

                        if (productionObject.ProductionType == "Blending")
                        {
                            var prod2SpiTypeRep =
                               (from rec in db.ProductionToSpiritTypeReporting
                                where rec.ProductionID == prodRec.ProductionID
                                select rec).FirstOrDefault();

                            if (prod2SpiTypeRep != null)
                            {
                                db.ProductionToSpiritTypeReporting.Remove(prod2SpiTypeRep);
                            }

                            var p2sRec =
                                (from rec in db.ProductionToSpirit
                                 where rec.ProductionID == prodRec.ProductionID
                                 select rec).FirstOrDefault();

                            if (p2sRec != null)
                            {
                                db.ProductionToSpirit.Remove(p2sRec);
                            }

                            var blendedComp =
                                (from rec in db.BlendedComponent
                                 where rec.ProductionID == prodRec.ProductionID
                                 select rec).FirstOrDefault();

                            if (blendedComp != null)
                            {
                                db.BlendedComponent.Remove(blendedComp);
                            }
                        }

                        if (productionObject.ProductionType == "Bottling")
                        {
                            var p2sRec =
                                (from rec in db.ProductionToSpirit
                                 where rec.ProductionID == prodRec.ProductionID
                                 select rec).FirstOrDefault();

                            if (p2sRec != null)
                            {
                                db.ProductionToSpirit.Remove(p2sRec);
                            }

                            var bttlInfo =
                                (from rec in db.BottlingInfo
                                 where rec.ProductionID == prodRec.ProductionID
                                 select rec);

                            if (bttlInfo != null)
                            {
                                db.BottlingInfo.RemoveRange(bttlInfo);
                            }

                            var fillTest =
                                (from rec in db.FillTest
                                 where rec.ProductionID == prodRec.ProductionID
                                 select rec);

                            if (fillTest != null)
                            {
                                db.FillTest.RemoveRange(fillTest);
                            }
                        }

                        var qtyRec =
                            (from rec in db.Volume
                             where rec.VolumeID == prodRec.VolumeID
                             select rec).FirstOrDefault();

                        if (qtyRec != null)
                        {
                            db.Volume.Remove(qtyRec);
                        }

                        var vbwRec =
                            (from rec in db.Weight
                             where rec.WeightID == prodRec.WeightID
                             select rec).FirstOrDefault();

                        if (vbwRec != null)
                        {
                            db.Weight.Remove(vbwRec);
                        }

                        var alcRec =
                            (from rec in db.Alcohol
                             where rec.AlcoholID == prodRec.AlcoholID
                             select rec).FirstOrDefault();

                        if (alcRec != null)
                        {
                            db.Alcohol.Remove(alcRec);
                        }

                        var prfRec =
                            (from rec in db.Proof
                             where rec.ProofID == prodRec.ProofID
                             select rec).FirstOrDefault();

                        if (prfRec != null)
                        {
                            db.Proof.Remove(prfRec);
                        }

                        var strRecs =
                            (from rec in db.StorageToRecord
                             where rec.RecordId == prodRec.ProductionID && rec.TableIdentifier == "prod"
                             select rec);

                        if (strRecs != null)
                        {
                            db.StorageToRecord.RemoveRange(strRecs);
                        }

                        db.Production.Remove(prodRec);
                    }

                    db.SaveChanges();

                    retMthdExecResult = true;
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Failed to delete Production: " + e);
                    retMthdExecResult = false;
                }
            }
            else
            {
                retMthdExecResult = false;
            }
            return retMthdExecResult;
        }

        private void RestoreBurntdownRecords(int productionId)
        {
            var prodContentRecords =
                (from prodCont in db.ProductionContent
                 where prodCont.ProductionID == productionId
                 select new
                 {
                     isProductionComponent = prodCont.isProductionComponent,
                     recordId = prodCont.RecordID,
                     value = prodCont.ContentValue,
                     valueKind = prodCont.ContentFieldID
                 }).ToList();

            if (prodContentRecords != null)
            {
                foreach (var i in prodContentRecords)
                {
                    bool setActiveStatus = false;

                    if (i.isProductionComponent) // burntdown record is a production record
                    {
                        var prod4Reporting =
                        (from prod4Rep in db.Production4Reporting
                         where prod4Rep.ProductionID == i.recordId
                         select prod4Rep).FirstOrDefault();

                        var productionValues =
                        (from prod in db.Production
                         where prod.ProductionID == i.recordId
                         select prod).FirstOrDefault();

                        if (productionValues != null)
                        {
                            if (i.valueKind == 11 || i.valueKind == 13 || i.valueKind == 15) // Volume
                            {
                                var vol =
                                    (from volume in db.Volume
                                     where volume.VolumeID == productionValues.VolumeID
                                     select volume).FirstOrDefault();

                                if (vol != null)
                                {
                                    vol.Value = i.value;
                                }

                                if (prod4Reporting != null)
                                {
                                    if (prod4Reporting.Volume == i.value)
                                    {
                                        setActiveStatus = true;
                                    }
                                    else
                                    {
                                        setActiveStatus = false;
                                    }
                                }
                            }
                            else if (i.valueKind == 6 || i.valueKind == 12 || i.valueKind == 14) // Weight
                            {
                                var weight =
                                    (from weigh in db.Weight
                                     where weigh.WeightID == productionValues.WeightID
                                     select weigh).FirstOrDefault();

                                if (weight != null)
                                {
                                    weight.Value = i.value;
                                }

                                if (prod4Reporting != null)
                                {
                                    if (prod4Reporting.Weight == i.value)
                                    {
                                        setActiveStatus = true;
                                    }
                                    else
                                    {
                                        setActiveStatus = false;
                                    }
                                }
                            }
                            else if (i.valueKind == 19 || i.valueKind == 21 || i.valueKind == 24) // Alcohol
                            {
                                var alc =
                                    (from alcohol in db.Alcohol
                                     where alcohol.AlcoholID == productionValues.AlcoholID
                                     select alcohol).FirstOrDefault();

                                if (alc != null)
                                {
                                    alc.Value = i.value;
                                }

                                if (prod4Reporting != null)
                                {
                                    if (prod4Reporting.Alcohol == i.value)
                                    {
                                        setActiveStatus = true;
                                    }
                                    else
                                    {
                                        setActiveStatus = false;
                                    }
                                }
                            }
                            else if (i.valueKind == 20 || i.valueKind == 22 || i.valueKind == 23) // Proof
                            {
                                var prf =
                                    (from proof in db.Proof
                                     where proof.ProofID == productionValues.ProofID
                                     select proof).FirstOrDefault();

                                if (prf != null)
                                {
                                    prf.Value = i.value;
                                }

                                if (prod4Reporting != null)
                                {
                                    if (prod4Reporting.Proof == i.value)
                                    {
                                        setActiveStatus = true;
                                    }
                                    else
                                    {
                                        setActiveStatus = false;
                                    }
                                }
                            }

                            // update the status of the record we are restoring 
                            // depending on whether all of its original value is being restored or no. If entire original value is restored
                            // we need to set status to active else, set status to Processing
                            if (setActiveStatus)
                            {
                                productionValues.StatusID = (int)Persistence.BusinessLogicEnums.Status.Active;
                            }
                            else
                            {
                                productionValues.StatusID = (int)Persistence.BusinessLogicEnums.Status.Processing;
                            }
                        }
                    }
                    else // burntdown record is a purchase record
                    {
                        var purch4Reporting =
                        (from pur4Rep in db.Purchase4Reporting
                         where pur4Rep.PurchaseID == i.recordId
                         select pur4Rep).FirstOrDefault();

                        var purchaseValues =
                        (from purch in db.Purchase
                         where purch.PurchaseID == i.recordId
                         select purch).FirstOrDefault();

                        if (purchaseValues != null)
                        {
                            if (i.valueKind == 1 || i.valueKind == 3 || i.valueKind == 7 || i.valueKind == 9) // Volume
                            {
                                var vol =
                                    (from volume in db.Volume
                                     where volume.VolumeID == purchaseValues.VolumeID
                                     select volume).FirstOrDefault();

                                if (vol != null)
                                {
                                    vol.Value = i.value;
                                }

                                if (purch4Reporting != null)
                                {
                                    if (purch4Reporting.Volume == i.value)
                                    {
                                        setActiveStatus = true;
                                    }
                                    else
                                    {
                                        setActiveStatus = false;
                                    }
                                }
                            }
                            else if (i.valueKind == 2 || i.valueKind == 4 || i.valueKind == 8 || i.valueKind == 10) // Weight
                            {
                                var weight =
                                    (from weigh in db.Weight
                                     where weigh.WeightID == purchaseValues.WeightID
                                     select weigh).FirstOrDefault();

                                if (weight != null)
                                {
                                    weight.Value = i.value;
                                }

                                if (purch4Reporting != null)
                                {
                                    if (purch4Reporting.Weight == i.value)
                                    {
                                        setActiveStatus = true;
                                    }
                                    else
                                    {
                                        setActiveStatus = false;
                                    }
                                }
                            }
                            else if (i.valueKind == 15 || i.valueKind == 17) // Alcohol
                            {
                                var alc =
                                    (from alcohol in db.Alcohol
                                     where alcohol.AlcoholID == purchaseValues.AlcoholID
                                     select alcohol).FirstOrDefault();

                                if (alc != null)
                                {
                                    alc.Value = i.value;
                                }

                                if (purch4Reporting != null)
                                {
                                    if (purch4Reporting.Alcohol == i.value)
                                    {
                                        setActiveStatus = true;
                                    }
                                    else
                                    {
                                        setActiveStatus = false;
                                    }
                                }
                            }
                            else if (i.valueKind == 16 || i.valueKind == 18) // Proof
                            {
                                var prf =
                                    (from proof in db.Proof
                                     where proof.ProofID == purchaseValues.ProofID
                                     select proof).FirstOrDefault();

                                if (prf != null)
                                {
                                    prf.Value = i.value;
                                }

                                if (purch4Reporting != null)
                                {
                                    if (purch4Reporting.Proof == i.value)
                                    {
                                        setActiveStatus = true;
                                    }
                                    else
                                    {
                                        setActiveStatus = false;
                                    }
                                }
                            }

                            // update the status of the record we are restoring 
                            // depending on whether all of its original value is being restored or no. If entire original value is restored
                            // we need to set status to active else, set status to Processing
                            if (setActiveStatus)
                            {
                                purchaseValues.StatusID = (int)Persistence.BusinessLogicEnums.Status.Active;
                            }
                            else
                            {
                                purchaseValues.StatusID = (int)Persistence.BusinessLogicEnums.Status.Processing;
                            }
                        }
                    }

                    db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// GetProductionList queries DB for Prodiction data for a particular production type
        /// </summary>
        /// <returns></returns>
        public List<ProductionObject> GetProductionList(int userId, string prodType)
        {
            List<ProductionObject> prodList = new List<ProductionObject>();

            var res =
                from prod in db.Production
                join prodTypes in db.ProductionType on prod.ProductionTypeID equals prodTypes.ProductionTypeID into prodTypes_join
                from prodTypes in prodTypes_join.DefaultIfEmpty()
                join dstlrs in db.AspNetUserToDistiller on prod.DistillerID equals dstlrs.DistillerID into dstlrs_join
                from dstlrs in dstlrs_join.DefaultIfEmpty()
                join galQuant in db.Volume on prod.VolumeID equals galQuant.VolumeID into galQuant_join
                from galQuant in galQuant_join.DefaultIfEmpty()
                join VBW in db.Weight on prod.WeightID equals VBW.WeightID into VBW_join
                from VBW in VBW_join.DefaultIfEmpty()
                join alc in db.Alcohol on prod.AlcoholID equals alc.AlcoholID into alc_join
                from alc in alc_join.DefaultIfEmpty()
                join proof in db.Proof on prod.ProofID equals proof.ProofID into proof_join
                from proof in proof_join.DefaultIfEmpty()
                join spiCutsM in db.ProductionToSpiritCut on prod.ProductionID equals spiCutsM.ProductionID into spiCutsM_join
                from spiCutsM in spiCutsM_join.DefaultIfEmpty()
                join spiCuts in db.SpiritCut on spiCutsM.SpiritCutID equals spiCuts.SpiritCutID into spiCuts_join
                from spiCuts in spiCuts_join.DefaultIfEmpty()
                join p2Spi in db.ProductionToSpirit on prod.ProductionID equals p2Spi.ProductionID into p2Spi_join
                from p2Spi in p2Spi_join.DefaultIfEmpty()
                join spi in db.Spirit on p2Spi.SpiritID equals spi.SpiritID into spi_join
                from spi in spi_join.DefaultIfEmpty()
                join status in db.Status on prod.StatusID equals status.StatusID into status_join
                from status in status_join.DefaultIfEmpty()
                where prodTypes.Name == prodType &&
                dstlrs.UserId == userId &&
                status.Name != "Deleted" &&
                status.Name != "Destroyed" &&
                status.Name != "Closed"
                select new
                {
                    prod.ProductionName,
                    prod.ProductionStartTime,
                    prod.ProductionEndTime,
                    prod.ProductionDate,
                    prod.Note,
                    ProductionID = ((System.Int32?)prod.ProductionID ?? (System.Int32?)0),
                    prod.ProductionTypeID,
                    ProdTypeName = prodTypes.Name,
                    Quantity = ((System.Single?)galQuant.Value ?? (System.Single?)0),
                    VolumeByWeight = ((System.Single?)VBW.Value ?? (System.Single?)0),
                    Alcohol = ((System.Single?)alc.Value ?? (System.Single?)0),
                    Proof = ((System.Single?)proof.Value ?? (System.Single?)0),
                    SpiritCut = (spiCuts.Name ?? string.Empty),
                    SpiritCutID = ((System.Int32?)spiCuts.SpiritCutID ?? (System.Int32?)0),
                    SpiritName = (spi.Name ?? string.Empty),
                    SpiritID = ((System.Int32?)p2Spi.SpiritID ?? (System.Int32?)0)
                };

            if (res != null)
            {
                foreach (var rec in res)
                {
                    ProductionObject pobj = new ProductionObject();
                    pobj.BatchName = rec.ProductionName;
                    pobj.ProductionDate = rec.ProductionDate;
                    pobj.ProductionStart = rec.ProductionStartTime;
                    pobj.ProductionEnd = rec.ProductionEndTime;
                    pobj.ProductionId = (int)rec.ProductionID;
                    pobj.ProductionType = rec.ProdTypeName;
                    pobj.ProductionTypeId = rec.ProductionTypeID;
                    pobj.SpiritCutId = (int)rec.SpiritCutID;
                    pobj.SpiritCutName = rec.SpiritCut;
                    pobj.Quantity = (float)rec.Quantity;
                    pobj.VolumeByWeight = (float)rec.VolumeByWeight;
                    pobj.AlcoholContent = (float)rec.Alcohol;
                    pobj.ProofGallon = (float)rec.Proof;
                    pobj.SpiritId = (int)rec.SpiritID;
                    pobj.SpiritName = rec.SpiritName;
                    pobj.Note = rec.Note;

                    prodList.Add(pobj);
                }
            }

            // now, let's get mutliple storages and Blending Components
            foreach (var i in prodList)
            {
                List<StorageObject> storageL = new List<StorageObject>();
                List<BlendingAdditive> blendCompsL = new List<BlendingAdditive>();
                try
                {
                    var storages =
                        from rec in db.StorageToRecord
                        join stoName in db.Storage on rec.StorageID equals stoName.StorageID
                        where rec.RecordId == i.ProductionId && rec.TableIdentifier == "prod"
                        select new
                        {
                            stoName.Name,
                            rec.StorageID
                        };

                    if (storages != null)
                    {
                        foreach (var it in storages)
                        {
                            StorageObject stor = new StorageObject();
                            stor.StorageId = it.StorageID;
                            stor.StorageName = it.Name;
                            storageL.Add(stor);
                        }
                    }
                    i.Storage = storageL;

                    // fill Blending Additive info for each production record
                    if (prodType == "Blending")
                    {
                        var ress = (from r in db.BlendedComponent
                                    join rM in db.MaterialDict on r.RecordId equals rM.MaterialDictID
                                    where r.ProductionID == i.ProductionId
                                    select new
                                    {
                                        r.BlendedComponentID,
                                        r.ProductionID,
                                        r.RecordId,
                                        r.UnitOfMeasurement,
                                        r.Quantity,
                                        rM.Name
                                    });

                        foreach (var it in ress)
                        {
                            BlendingAdditive bAdd = new BlendingAdditive();
                            bAdd.BlendingAdditiveId = it.BlendedComponentID;
                            bAdd.RawMaterialId = it.RecordId;
                            bAdd.RawMaterialName = it.Name;
                            bAdd.UnitOfMeasurement = it.UnitOfMeasurement;
                            bAdd.RawMaterialQuantity = it.Quantity;
                            blendCompsL.Add(bAdd);
                        }
                        i.BlendingAdditives = blendCompsL;
                    }
                    else if (prodType == "Bottling")
                    {
                        var ress = (from r in db.BottlingInfo
                                    where r.ProductionID == i.ProductionId
                                    select new
                                    {
                                        r.BottleQuantity,
                                        r.BottleVolume,
                                        r.CaseCapacity,
                                        r.CaseQuantity
                                    }).FirstOrDefault();
                        if (ress != null)
                        {
                            BottlingObject bObj = new BottlingObject();
                            bObj.BottleQuantity = (int)ress.BottleQuantity;
                            bObj.BottleCapacity = (float)ress.BottleVolume;
                            bObj.CaseCapacity = (int)ress.CaseCapacity;
                            bObj.CaseQuantity = (float)ress.CaseQuantity;
                            i.BottlingInfo = bObj;
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Error getting production list: " + e);
                }
            }

            return prodList;
        }

        /// <summary>
        /// GetSpiritToKindListObject method returns an object with a list of multiple material kinds mapped to transaction spirit type.
        /// </summary>
        /// <param name="spiritList"></param>
        /// <param name="kindsList"></param>
        /// <returns></returns>
        public List<SpiritToKindListObject> GetSpiritToKindListData()
        {
            List<SpiritToKindListObject> spir2KindList = new List<SpiritToKindListObject>();
            byte[] existsArray = new byte[256];

            try
            {
                var res =
                    from spiType in db.SpiritTypeReporting
                    join spiT2Mat in db.SpiritType2MaterialKindReporting on spiType.SpiritTypeReportingID equals spiT2Mat.SpiritTypeReportingID into spiT2Mat_join
                    from spiT2Mat in spiT2Mat_join.DefaultIfEmpty()
                    join matKind in db.MaterialKindReporting on spiT2Mat.MaterialKindReportingID equals matKind.MaterialKindReportingID into matKind_join
                    from matKind in matKind_join.DefaultIfEmpty()
                    select new
                    {
                        MaterialKindReportingID = (int?)spiT2Mat.MaterialKindReportingID ?? 0,
                        SpiritTypeReportingID = (int?)spiType.SpiritTypeReportingID ?? 0,
                        MaterialKindName = matKind.MaterialKindName ?? string.Empty,
                        ProductTypeName = spiType.ProductTypeName ?? string.Empty
                    };

                foreach (var i in res)
                {
                    if (existsArray[i.SpiritTypeReportingID] == 0)
                    {
                        SpiritToKindListObject spir2Kind = new SpiritToKindListObject();
                        spir2Kind.SpiritTypeReportingID = i.SpiritTypeReportingID;
                        spir2Kind.ProductTypeName = i.ProductTypeName;

                        List<MaterialKindObject> kindList = new List<MaterialKindObject>();

                        if (i.MaterialKindReportingID > 0 && i.MaterialKindName != string.Empty)
                        {
                            MaterialKindObject kind = new MaterialKindObject();
                            kind.MaterialKindID = i.MaterialKindReportingID;
                            kind.MaterialKindName = i.MaterialKindName;
                            kind.SpiritTypeReportingID = i.SpiritTypeReportingID;
                            kindList.Add(kind);
                        }

                        spir2Kind.MaterialKindObject = kindList;
                        spir2KindList.Add(spir2Kind);
                        existsArray[i.SpiritTypeReportingID] = 1;
                    }
                    else if (existsArray[i.SpiritTypeReportingID] == 1)
                    {
                        foreach (var li in spir2KindList)
                        {
                            if (li.SpiritTypeReportingID == i.SpiritTypeReportingID && i.MaterialKindReportingID > 0 && i.MaterialKindName != string.Empty)
                            {
                                MaterialKindObject kind = new MaterialKindObject();
                                kind.MaterialKindID = i.MaterialKindReportingID;
                                kind.MaterialKindName = i.MaterialKindName;
                                kind.SpiritTypeReportingID = i.SpiritTypeReportingID;
                                li.MaterialKindObject.Add(kind);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error Getting Material and Spirit List: " + e);
                return null;
            }

            return spir2KindList;
        }

        #endregion

        #region Destruction Methods
        /// <summary>
        /// Creates record of destruction in Destruction table, clears Quantity, Volume by Weight, Alcohol Content, and Proof Gallons from batch record in relevant table,
        /// marks batch status as "Destroyed" in relevant table. 
        /// </summary>
        /// <param name="destructionObject"></param>
        /// <returns>int</returns> 
        /// Algorithm:
        /// 1) Retrieve quantity, volume by weight, alcohol content, and proof gallons with supplied RecordId
        /// 2) Create a new destruction record with the supplied values
        /// 3) Clear quantity, volume by weight, alcohol content, and proof gallons from the supplied RecordId
        /// 4) Set the Record status to "Destroyed"
        public bool DestroyBatch(DestructionObject destructionObject, int userId)
        {
            //define method execution return value to be false by default
            bool retMthdExecResult = false;
            var recordId = destructionObject.RecordID;

            try
            {
                if (destructionObject.WorkflowType == "Purchase")
                {
                    var res =
                        from rec in db.Purchase
                        join dslrs in db.AspNetUserToDistiller on rec.DistillerID equals dslrs.DistillerID into dslrs_join
                        from dslrs in dslrs_join.DefaultIfEmpty()
                        join qty in db.Volume on rec.VolumeID equals qty.VolumeID into qty_join
                        from qty in qty_join.DefaultIfEmpty()
                        join vbw in db.Weight on rec.WeightID equals vbw.WeightID into vbw_join
                        from vbw in vbw_join.DefaultIfEmpty()
                        join alc in db.Alcohol on rec.AlcoholID equals alc.AlcoholID into alc_join
                        from alc in alc_join.DefaultIfEmpty()
                        join prf in db.Proof on rec.ProofID equals prf.ProofID into prf_join
                        from prf in prf_join.DefaultIfEmpty()
                        where rec.PurchaseID == recordId &&
                            dslrs.UserId == userId
                        select new
                        {
                            Quantity = ((System.Single?)qty.Value ?? (System.Single?)0),
                            VolumeID = ((System.Single?)qty.VolumeID ?? (System.Single?)0),
                            VolumeByWeight = ((System.Single?)vbw.Value ?? (System.Single?)0),
                            WeightID = ((System.Single?)vbw.WeightID ?? (System.Single?)0),
                            AlcoholContent = ((System.Single?)alc.Value ?? (System.Single?)0),
                            AlcoholID = ((System.Single?)alc.AlcoholID ?? (System.Single?)0),
                            ProofGallons = ((System.Single?)prf.Value ?? (System.Single?)0),
                            ProofID = ((System.Single?)prf.ProofID ?? (System.Single?)0)
                        };
                    if (res != null)
                    {
                        foreach (var rec in res)
                        {
                            destructionObject.Quantity = (float)rec.Quantity;
                            destructionObject.VolumeByWeight = (float)rec.VolumeByWeight;
                            destructionObject.AlcoholContent = (float)rec.AlcoholContent;
                            destructionObject.ProofGallons = (float)rec.ProofGallons;
                            destructionObject.QuantityGalID = (int)rec.VolumeID;
                            destructionObject.VolumeByWeightLBID = (int)rec.WeightID;
                            destructionObject.AlcoholID = (int)rec.AlcoholID;
                            destructionObject.ProofID = (int)rec.ProofID;
                        }

                        var QuantityGalID = destructionObject.QuantityGalID;
                        var VolumeByWeightLBID = destructionObject.VolumeByWeightLBID;
                        var AlcoholID = destructionObject.AlcoholID;
                        var ProofID = destructionObject.ProofID;

                        // Create destruction record
                        CreateDestructionRecord(ref destructionObject);

                        // Clear record quantities
                        ClearRecordQuantities(QuantityGalID, VolumeByWeightLBID, AlcoholID, ProofID);

                        // Update purchase record status to "Destoroyed"
                        var status =
                            (from rec in db.Status
                             where rec.Name == "Destroyed"
                             select rec).FirstOrDefault();
                        var prodRec =
                            (from rec in db.Purchase
                             where rec.PurchaseID == recordId
                             select rec).FirstOrDefault();
                        if (prodRec != null)
                        {
                            prodRec.StatusID = status.StatusID;
                            db.SaveChanges();
                        }
                    }
                }
                else if (destructionObject.WorkflowType == "Production")
                {
                    var res =
                         from rec in db.Production
                         join dslrs in db.AspNetUserToDistiller on rec.DistillerID equals dslrs.DistillerID into dslrs_join
                         from dslrs in dslrs_join.DefaultIfEmpty()
                         join qty in db.Volume on rec.VolumeID equals qty.VolumeID into qty_join
                         from qty in qty_join.DefaultIfEmpty()
                         join vbw in db.Weight on rec.WeightID equals vbw.WeightID into vbw_join
                         from vbw in vbw_join.DefaultIfEmpty()
                         join alc in db.Alcohol on rec.AlcoholID equals alc.AlcoholID into alc_join
                         from alc in alc_join.DefaultIfEmpty()
                         join prf in db.Proof on rec.ProofID equals prf.ProofID into prf_join
                         from prf in prf_join.DefaultIfEmpty()
                         where rec.ProductionID == recordId &&
                            dslrs.UserId == userId
                         select new
                         {
                             Quantity = ((System.Single?)qty.Value ?? (System.Single?)0),
                             VolumeID = ((System.Single?)qty.VolumeID ?? (System.Single?)0),
                             VolumeByWeight = ((System.Single?)vbw.Value ?? (System.Single?)0),
                             WeightID = ((System.Single?)vbw.WeightID ?? (System.Single?)0),
                             AlcoholContent = ((System.Single?)alc.Value ?? (System.Single?)0),
                             AlcoholID = ((System.Single?)alc.AlcoholID ?? (System.Single?)0),
                             ProofGallons = ((System.Single?)prf.Value ?? (System.Single?)0),
                             ProofID = ((System.Single?)prf.ProofID ?? (System.Single?)0)
                         };
                    if (res != null)
                    {
                        foreach (var rec in res)
                        {
                            destructionObject.Quantity = (float)rec.Quantity;
                            destructionObject.VolumeByWeight = (float)rec.VolumeByWeight;
                            destructionObject.AlcoholContent = (float)rec.AlcoholContent;
                            destructionObject.ProofGallons = (float)rec.ProofGallons;
                            destructionObject.QuantityGalID = (int)rec.VolumeID;
                            destructionObject.VolumeByWeightLBID = (int)rec.WeightID;
                            destructionObject.AlcoholID = (int)rec.AlcoholID;
                            destructionObject.ProofID = (int)rec.ProofID;
                        }

                        var QuantityGalID = destructionObject.VolumeByWeightLBID;
                        var VolumeByWeightLBID = destructionObject.QuantityGalID;
                        var AlcoholID = destructionObject.AlcoholID;
                        var ProofID = destructionObject.ProofID;

                        // Create destruction record
                        CreateDestructionRecord(ref destructionObject);

                        // Clear record quantities
                        ClearRecordQuantities(QuantityGalID, VolumeByWeightLBID, AlcoholID, ProofID);

                        // Update production record status to "Destoroyed"
                        var status =
                            (from rec in db.Status
                             where rec.Name == "Destroyed"
                             select rec).FirstOrDefault();
                        var prodRec =
                            (from rec in db.Production
                             where rec.ProductionID == recordId
                             select rec).FirstOrDefault();
                        if (prodRec != null)
                        {
                            prodRec.StatusID = status.StatusID;
                            db.SaveChanges();
                        }
                    }
                }
                retMthdExecResult = true;
            }
            catch (Exception e)
            {
                throw;
            }
            return retMthdExecResult;
        }

        /// <summary>
        /// Creates new destruction record within the Destruction table
        /// </summary>
        private void CreateDestructionRecord(ref DestructionObject destructionObject)
        {
            try
            {
                Destruction destrObj = new Destruction();
                destrObj.RecordID = destructionObject.RecordID;
                destrObj.WorkflowType = destructionObject.WorkflowType;
                destrObj.RepresentativeName = destructionObject.RepresentativeName;
                destrObj.StartTime = destructionObject.StartTime;
                destrObj.EndTime = destructionObject.EndTime;
                destrObj.DestructionMethod = destructionObject.DestructionMethod;
                destrObj.Withdrawn = destructionObject.Withdrawn;
                destrObj.Volume = destructionObject.Quantity;
                destrObj.Weight = destructionObject.VolumeByWeight;
                destrObj.AlcoholContent = destructionObject.AlcoholContent;
                destrObj.ProofGallons = destructionObject.ProofGallons;
                destrObj.Note = destructionObject.Note;
                db.Destruction.Add(destrObj);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                throw;
            }
        }

        /// <summary>
        /// Clears record quantites upon record destruction according to passed Quantity, VbW, Alcohol, and Proof record id's
        /// </summary>
        private void ClearRecordQuantities(int QuantityGalID, int VolumeByWeightLBID, int AlcoholID, int ProofID)
        {
            try
            {
                var qtyRec =
                    (from rec in db.Volume
                     where rec.VolumeID == QuantityGalID
                     select rec).FirstOrDefault();
                if (qtyRec != null)
                {
                    qtyRec.Value = 0;
                    db.SaveChanges();
                }
                var vbwRec =
                    (from rec in db.Weight
                     where rec.WeightID == VolumeByWeightLBID
                     select rec).FirstOrDefault();
                if (vbwRec != null)
                {
                    vbwRec.Value = 0;
                    db.SaveChanges();
                }
                var alcRec =
                    (from rec in db.Alcohol
                     where rec.AlcoholID == AlcoholID
                     select rec).FirstOrDefault();
                if (alcRec != null)
                {
                    alcRec.Value = 0;
                    db.SaveChanges();
                }
                var prfRec =
                    (from rec in db.Proof
                     where rec.ProofID == ProofID
                     select rec).FirstOrDefault();
                if (prfRec != null)
                {
                    prfRec.Value = 0;
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        /// <summary>
        /// Retrieves purchased Fermented and Distilled batches
        /// </summary>
        public List<PurchaseObject> GetPurchaseDataForDestruction(int userId)
        {
            List<PurchaseObject> purchaseList = new List<PurchaseObject>();

            // Purchase types to be included in the return list
            string[] purchaseType = new string[] { "Fermented", "Distilled" };

            foreach (string i in purchaseType)
            {
                purchaseList.AddRange(GetPurchasesList(i, userId));
            }
            return purchaseList;
        }

        /// <summary>
        /// Retrieves all produced batches
        /// </summary>
        public List<ProductionObject> GetProductionDataForDestruction(int userId)
        {
            List<ProductionObject> productionList = new List<ProductionObject>();

            // Production types to be included in the return list
            string[] productionType = new string[] { "Fermentation", "Distillation", "Blending", "Bottling" };

            foreach (string i in productionType)
            {
                productionList.AddRange(GetProductionList(userId, i));
            }
            return productionList;
        }

        /// <summary>
        /// Retrieves destroyed purchased batches
        /// </summary>
        public List<DestructionObject> GetDestroyedBatches(int userId)
        {
            List<DestructionObject> destroyedList = new List<DestructionObject>();

            var prodRes =
                from destrRec in db.Destruction
                join prodRec in db.Production on destrRec.RecordID equals prodRec.ProductionID into prodRec_join
                from prodRec in prodRec_join.DefaultIfEmpty()
                join dstlrs in db.AspNetUserToDistiller on prodRec.DistillerID equals dstlrs.DistillerID into dstlrs_join
                from dstlrs in dstlrs_join.DefaultIfEmpty()
                join prodType in db.ProductionType on prodRec.ProductionTypeID equals prodType.ProductionTypeID into prodType_join
                from prodType in prodType_join.DefaultIfEmpty()
                where destrRec.WorkflowType == "Production" &&
                    dstlrs.UserId == userId
                select new
                {
                    destrRec.DestructionID,
                    destrRec.RecordID,
                    prodRec.ProductionName,
                    destrRec.RepresentativeName,
                    destrRec.WorkflowType,
                    prodType.Name,
                    destrRec.StartTime,
                    destrRec.EndTime,
                    destrRec.DestructionMethod,
                    destrRec.Withdrawn,
                    destrRec.Volume,
                    destrRec.Weight,
                    destrRec.AlcoholContent,
                    destrRec.ProofGallons,
                    destrRec.Note
                };

            foreach (var i in prodRes)
            {
                DestructionObject dr = new DestructionObject();
                dr.DestructionID = i.DestructionID;
                dr.RecordID = i.RecordID;
                dr.WorkflowType = i.WorkflowType;
                dr.RepresentativeName = i.RepresentativeName;
                dr.BatchType = i.Name;
                dr.BatchName = i.ProductionName;
                dr.StartTime = i.StartTime;
                dr.EndTime = i.EndTime;
                dr.DestructionMethod = i.DestructionMethod;
                dr.Withdrawn = i.Withdrawn;
                dr.Quantity = i.Volume;
                dr.VolumeByWeight = i.Weight;
                dr.AlcoholContent = i.AlcoholContent;
                dr.ProofGallons = i.ProofGallons;
                dr.Note = i.Note;

                destroyedList.Add(dr);
            }

            var purRes =
                from destrRec in db.Destruction
                join purRec in db.Purchase on destrRec.RecordID equals purRec.PurchaseID into purRec_join
                from purRec in purRec_join.DefaultIfEmpty()
                join dstlrs in db.AspNetUserToDistiller on purRec.DistillerID equals dstlrs.DistillerID into dstlrs_join
                from dstlrs in dstlrs_join.DefaultIfEmpty()
                join purType in db.PurchaseType on purRec.PurchaseTypeID equals purType.PurchaseTypeID into purType_join
                from purType in purType_join.DefaultIfEmpty()
                where destrRec.WorkflowType == "Purchase" &&
                    dstlrs.UserId == userId
                select new
                {
                    destrRec.DestructionID,
                    destrRec.RecordID,
                    purRec.PurchaseName,
                    destrRec.RepresentativeName,
                    destrRec.WorkflowType,
                    purType.Name,
                    destrRec.StartTime,
                    destrRec.EndTime,
                    destrRec.DestructionMethod,
                    destrRec.Withdrawn,
                    destrRec.Volume,
                    destrRec.Weight,
                    destrRec.AlcoholContent,
                    destrRec.ProofGallons,
                    destrRec.Note
                };

            foreach (var i in purRes)
            {
                DestructionObject dr = new DestructionObject();
                dr.DestructionID = i.DestructionID;
                dr.RecordID = i.RecordID;
                dr.WorkflowType = i.WorkflowType;
                dr.BatchType = i.Name;
                dr.BatchName = i.PurchaseName;
                dr.RepresentativeName = i.RepresentativeName;
                dr.StartTime = i.StartTime;
                dr.EndTime = i.EndTime;
                dr.DestructionMethod = i.DestructionMethod;
                dr.Withdrawn = i.Withdrawn;
                dr.Quantity = i.Volume;
                dr.VolumeByWeight = i.Weight;
                dr.AlcoholContent = i.AlcoholContent;
                dr.ProofGallons = i.ProofGallons;
                dr.Note = i.Note;

                destroyedList.Add(dr);
            }

            return destroyedList;
        }

        #endregion

        #region Reporting Methods

        public ProductionReportingObject GetProductionReportData(DateTime start, DateTime end, int userId)
        {
            ProductionReportingObject prodRepObj = new ProductionReportingObject();
            List<ProdReportPart1> part1List = new List<ProdReportPart1>();
            List<ProdReportParts2Through4> part2Thru4List = new List<ProdReportParts2Through4>();
            List<ProdReportPart5> part5 = new List<ProdReportPart5>();
            List<ProdReportPart6> prodReportPart6List = new List<ProdReportPart6>();
            List<ProductionReportHelper> tempRepObjList = new List<ProductionReportHelper>();

            // get distiller information for header report
            int distillerID = GetDistillerId(userId);
            prodRepObj.Header = GetDistillerInfoForReportHeader(distillerID, start);

            // we need this for Part 1
            GetAllProductionReportRecordsForGivenPeriod(userId, start, end, ref tempRepObjList);

            GetProductionReportPart1Records(start, end, ref part1List, tempRepObjList);

            // we need this for line 17(b) of Part 1
            // we only need to get Unfinished Spirit during the quartely returns so doing the check here. Months could change from year to year. Need to check with TTB every year
            if (start.Month == 1 && end.Month == 1 || start.Month == 4 && end.Month == 4 || start.Month == 7 && end.Month == 7 || start.Month == 10 && end.Month == 10)
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

            return prodRepObj;
        }

        private void ProductionReportPart6Data(ref List<ProdReportPart6> prodReportPart6List, ref List<ProductionReportHelper> tempRepObjList)
        {
            List<ProductionPart6DataForParsingInProductionReportWorklfow> productionContentList = new List<ProductionPart6DataForParsingInProductionReportWorklfow>();

            int[] contentFieldId = new int[23]; // making it the same size as ContentField table

            contentFieldId[0] = 1; // PurFermentableVolume 
            contentFieldId[1] = 2; // PurFermentableWeight
            contentFieldId[2] = 4; // PurFermentedWeight

            foreach (var i in tempRepObjList)
            {
                GetValueFromProductionContentRecursively(contentFieldId, ref productionContentList, i.ProductionID);
            }

            foreach (var k in productionContentList)
            {
                try
                {
                    var part6Materials =
                    from prod in db.Production
                    join prod2Purch in db.ProductionToPurchase on prod.ProductionID equals prod2Purch.ProductionID into prod2Purch_join
                    from prod2Purch in prod2Purch_join.DefaultIfEmpty()
                    join purch in db.Purchase on prod2Purch.PurchaseID equals purch.PurchaseID into purch_join
                    from purch in purch_join.DefaultIfEmpty()
                    join matDict in db.MaterialDict on purch.MaterialDictID equals matDict.MaterialDictID into matDict_join
                    from matDict in matDict_join.DefaultIfEmpty()
                    join prod2SpiritType in db.ProductionToSpiritTypeReporting on prod.ProductionID equals prod2SpiritType.ProductionID into prod2SpiritType_join
                    from prod2SpiritType in prod2SpiritType_join.DefaultIfEmpty()
                    join matKindRep in db.MaterialKindReporting on prod2SpiritType.MaterialKindReportingID equals matKindRep.MaterialKindReportingID into matKindRep_join
                    from matKindRep in matKindRep_join.DefaultIfEmpty()
                    join prodRepMatCat2MatKind in db.ProdRepMatCat2MaterialKind on matKindRep.MaterialKindReportingID equals prodRepMatCat2MatKind.MaterialKindReportingID into prodRepMatCat2MatKind_join
                    from prodRepMatCat2MatKind in prodRepMatCat2MatKind_join.DefaultIfEmpty()
                    join prodRepMatCat in db.ProductionReportMaterialCategory on prodRepMatCat2MatKind.ProductionReportMaterialCategoryID equals prodRepMatCat.ProductionReportMaterialCategoryID into prodRepMatCat_join
                    from prodRepMatCat in prodRepMatCat_join.DefaultIfEmpty()
                    where
                    prod.ProductionID == k.ReportingPeriodProductionId
                    select new
                    {
                        MaterialName = matDict.Name,
                        ProductionReportMaterialCategoryID = (int?)prodRepMatCat.ProductionReportMaterialCategoryID ?? (int?)0
                    };

                    var productionTypeOfProductionIdAssociatedWithPurchase =
                    (from prod in db.Production
                     where prod.ProductionID == k.ProductionIdAssociatedWithPurchase
                     select new
                     {
                         productionType = prod.ProductionTypeID
                     }).FirstOrDefault();

                    float updatedRawMaterialAmmt = 0f;

                    if (productionTypeOfProductionIdAssociatedWithPurchase != null)
                    {
                        if (productionTypeOfProductionIdAssociatedWithPurchase.productionType == (int)ProductinWorkflowType.Distillation)
                        {
                            updatedRawMaterialAmmt = k.ContentValue;
                        }
                        else
                        {
                            var originalRawMaterialAmount =
                            (from purchaseForReporting in db.Purchase4Reporting
                             where
                             purchaseForReporting.PurchaseID == k.PurchaseId
                             && purchaseForReporting.ProductionID == k.ProductionIdAssociatedWithPurchase
                             select new
                             {
                                 originalRawMaterialAmmt = purchaseForReporting.Weight > 0 ? purchaseForReporting.Weight : purchaseForReporting.Volume
                             }).FirstOrDefault();

                            var originalFermentedAmount =
                            (from prodForReporting in db.Production4Reporting
                             where
                             prodForReporting.ProductionID == k.ProductionIdAssociatedWithPurchase
                             select new
                             {
                                 originalFermentedAmmt = prodForReporting.Weight > 0 ? prodForReporting.Weight : prodForReporting.Volume,
                             }).FirstOrDefault();

                            var fermentedAmountWentToProduction =
                            (from prodContent in db.ProductionContent
                             where
                             prodContent.RecordID == k.ProductionIdAssociatedWithPurchase
                             && (new int[] { 5, 6 }).Contains(prodContent.ContentFieldID)
                             select new
                             {
                                 fermentedInProductionAmmt = prodContent.ContentValue
                             }).FirstOrDefault();

                            if (originalRawMaterialAmount != null && originalFermentedAmount != null && fermentedAmountWentToProduction != null)
                            {
                                updatedRawMaterialAmmt = (originalRawMaterialAmount.originalRawMaterialAmmt / originalFermentedAmount.originalFermentedAmmt) * fermentedAmountWentToProduction.fermentedInProductionAmmt;
                            }
                        }
                    }

                    if (part6Materials != null)
                    {
                        foreach (var t in part6Materials)
                        {

                            var mater = prodReportPart6List.Find(x => x.KindOfMaterial == (string)t.MaterialName);

                            // case where Material doesn't exist
                            if (mater == null)
                            {
                                ProdReportPart6 prt6 = new ProdReportPart6();
                                prt6.ProductionID = k.ReportingPeriodProductionId;
                                prt6.KindOfMaterial = (string)t.MaterialName;
                                prt6.ProdReportMaterialCategoryID = (int)t.ProductionReportMaterialCategoryID;

                                if (k.ContentFieldId == 1)
                                {
                                    prt6.Volume = updatedRawMaterialAmmt;
                                }
                                else if (k.ContentFieldId == 2 || k.ContentFieldId == 4)
                                {
                                    prt6.Weight = updatedRawMaterialAmmt;
                                }

                                prodReportPart6List.Add(prt6);
                            }
                            else // case where Material already exists
                            {
                                if (k.ContentFieldId == 1)
                                {
                                    mater.Volume += updatedRawMaterialAmmt;
                                }
                                else if (k.ContentFieldId == 2 || k.ContentFieldId == 4)
                                {
                                    mater.Weight += updatedRawMaterialAmmt;
                                }
                            }
                        }
                    }

                    //we need this here to fill up ProdReportMaterialCategoryID
                    foreach (var mat in prodReportPart6List)
                    {
                        if (mat.ProdReportMaterialCategoryID == 0)
                        {
                            var l =
                            (from prodContent in db.ProductionContent
                             join prod2SpiritType in db.ProductionToSpiritTypeReporting on prodContent.ProductionID equals prod2SpiritType.ProductionID into prod2SpiritType_join
                             from prod2SpiritType in prod2SpiritType_join.DefaultIfEmpty()
                             join matKindRep in db.MaterialKindReporting on prod2SpiritType.MaterialKindReportingID equals matKindRep.MaterialKindReportingID into matKindRep_join
                             from matKindRep in matKindRep_join.DefaultIfEmpty()
                             join prodRepMatCat2MatKind in db.ProdRepMatCat2MaterialKind on matKindRep.MaterialKindReportingID equals prodRepMatCat2MatKind.MaterialKindReportingID into prodRepMatCat2MatKind_join
                             from prodRepMatCat2MatKind in prodRepMatCat2MatKind_join.DefaultIfEmpty()
                             join prodRepMatCat in db.ProductionReportMaterialCategory on prodRepMatCat2MatKind.ProductionReportMaterialCategoryID equals prodRepMatCat.ProductionReportMaterialCategoryID into prodRepMatCat_join
                             from prodRepMatCat in prodRepMatCat_join.DefaultIfEmpty()
                             where
                             prodContent.isProductionComponent == true &&
                             prodContent.RecordID == mat.ProductionID
                             select new
                             {
                                 ProductionReportMaterialCategoryID = (int?)prodRepMatCat.ProductionReportMaterialCategoryID
                             }).FirstOrDefault();

                            if (l != null)
                            {
                                if (l.ProductionReportMaterialCategoryID != null)
                                {
                                    mat.ProdReportMaterialCategoryID = (int)l.ProductionReportMaterialCategoryID;
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
        }

        private void GetValueFromProductionContentRecursively(int[] contentFieldId, ref List<ProductionPart6DataForParsingInProductionReportWorklfow> productionContentList, int productionId)
        {
            var records =
                (from prodContent in db.ProductionContent
                 where prodContent.ProductionID == productionId
                 select prodContent).ToList();

            if (records != null)
            {
                foreach (var record in records)
                {
                    ProductionPart6DataForParsingInProductionReportWorklfow part6ObjectForParsing = new ProductionPart6DataForParsingInProductionReportWorklfow();

                    if (contentFieldId[0] == record.ContentFieldID || contentFieldId[1] == record.ContentFieldID || contentFieldId[2] == record.ContentFieldID)
                    {
                        var exists = productionContentList.Where(x => x.ProductionIdAssociatedWithPurchase == record.ProductionID).Where(x => x.PurchaseId == record.RecordID).Where(x => x.ContentFieldId == record.ContentFieldID);

                        if (exists.Count() == 0)
                        {
                            part6ObjectForParsing.ContentFieldId = record.ContentFieldID;
                            part6ObjectForParsing.ContentValue = record.ContentValue;
                            part6ObjectForParsing.ReportingPeriodProductionId = productionId;
                            part6ObjectForParsing.ProductionIdAssociatedWithPurchase = record.ProductionID;
                            part6ObjectForParsing.PurchaseId = record.RecordID;

                            productionContentList.Add(part6ObjectForParsing);
                        }
                    }

                    if (record.isProductionComponent)
                    {
                        GetValueFromProductionContentRecursively(contentFieldId, ref productionContentList, record.RecordID);
                    }
                }
            }
        }

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
                            (from prodForRep in db.Production4Reporting
                             where prodForRep.ProductionID == rec.ProductionID
                             select new
                             {
                                 Proof = (float?)prodForRep.Proof ?? (float?)0
                             }).FirstOrDefault();

                        prodRP2T4.ProofGallons = (float)part4.Proof;

                        prodRP2T4.SpiritTypeReportingID = (int)rec.SpiritTypeReportingID;

                        part2Thru4List.Add(prodRP2T4);
                    }
                    else
                    {
                        var part4 =
                            (from prodForRep in db.Production4Reporting
                             where prodForRep.ProductionID == rec.ProductionID
                             select new
                             {
                                 Proof = (float?)prodForRep.Proof ?? (float?)0
                             }).FirstOrDefault();

                        materialKind.ProofGallons += (float)part4.Proof;
                    }
                }
            }
        }

        /// <summary>
        /// GetProductionReportPart1Records method calls Rows in the report iteratively
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="part1List"></param>
        /// <param name="tempRepObjList"></param>
        private void GetProductionReportPart1Records(DateTime start, DateTime end, ref List<ProdReportPart1> part1List, List<ProductionReportHelper> tempRepObjList)
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
        private void GetEnteredInStorageAndProcessingAccount(DateTime start, DateTime end, ProductionReportHelper record, ref List<ProdReportPart1> part1List)
        {
            float blendedProof = 0f;
            float distilledProof = 0f;

            // Entered into Storage account case 
            var blended =
                (from productionContent in db.ProductionContent
                 join production in db.Production on productionContent.ProductionID equals production.ProductionID into production_join
                 from production in production_join.DefaultIfEmpty()
                 join prod4Rep in db.Production4Reporting on productionContent.ProductionID equals prod4Rep.ProductionID
                 where productionContent.RecordID == record.ProductionID
                 && production.ProductionEndTime >= start
                 && production.ProductionEndTime <= end
                 && prod4Rep.Redistilled == false
                 && production.StateID == (int)Persistence.BusinessLogicEnums.State.Blended
                 select new
                 {
                     id = prod4Rep.ProductionID,
                     proof = (float?)prod4Rep.Proof ?? (float?)0
                 }).Distinct().ToList();

            if (blended != null)
            {
                foreach (var i in blended)
                {
                    blendedProof += (float)i.proof;
                }
            }
            else
            {
                blendedProof = 0f;
            }

            var distilled =
                (from prod4Reporting in db.Production4Reporting
                 where prod4Reporting.ProductionID == record.ProductionID
                 select new
                 {
                     Proof = (float?)prod4Reporting.Proof ?? (float?)0
                 }).FirstOrDefault();

            if (distilled != null)
            {
                distilledProof = (float)distilled.Proof;
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
                part1Obj.ProccessingAcct = blendedProof;
                float storageProof = (distilledProof - blendedProof) >= 0 ? (distilledProof - blendedProof) : 0;
                part1Obj.StorageAcct = storageProof;
                part1Obj.ProducedTotal = blendedProof + storageProof;

                part1List.Add(part1Obj);
            }
            else
            {
                float storageProof = (distilledProof - blendedProof) >= 0 ? (distilledProof - blendedProof) : 0;
                spiritType.StorageAcct += storageProof;
                spiritType.ProducedTotal += blendedProof + storageProof;
            }
        }


        /// <summary>
        /// This method gets PFGals for line 9 on Production report
        /// given the initial list of production Ids for that month.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="record"></param>
        /// <param name="part1List"></param>
        private void GetEnteredInProcessingAccount(DateTime start, DateTime end, ProductionReportHelper record, ref List<ProdReportPart1> part1List)
        {
            float blendedProof = 0f;
            var blended =
                (from prod4Reporting in db.Production4Reporting
                 where prod4Reporting.ProductionID == record.ProductionID
                 select new
                 {
                     Proof = (float?)prod4Reporting.Proof ?? (float?)0
                 }).FirstOrDefault();

            if (blended != null)
            {
                blendedProof = (float)blended.Proof;
            }
            else
            {
                blendedProof = 0f;
            }

            var spiritType = part1List.Find(x => x.SpiritTypeReportingID == record.SpiritTypeReportingID);

            if (spiritType == null)
            {
                ProdReportPart1 part1Obj = new ProdReportPart1();
                part1Obj.SpiritCatName = record.SpiritTypeReportName;
                part1Obj.SpiritTypeReportingID = (int)record.SpiritTypeReportingID;
                part1Obj.ProccessingAcct = blendedProof;
                part1Obj.ProducedTotal = blendedProof;

                part1List.Add(part1Obj);
            }
            else
            {
                spiritType.ProccessingAcct += blendedProof;
                spiritType.ProducedTotal += blendedProof;
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
        private void GetReceivedForRedistillationForLine15(int userId, DateTime start, DateTime end, ref List<ProdReportPart1> part1List, ref List<ProdReportPart5> prodRPart5L)
        {
            var ress =
                (from prod in db.Production
                 join distillers in db.AspNetUserToDistiller on prod.DistillerID equals distillers.DistillerID into distillers_join
                 from distillers in distillers_join.DefaultIfEmpty()
                 join prod2SpiritType in db.ProductionToSpiritTypeReporting on prod.ProductionID equals prod2SpiritType.ProductionID into prod2SpiritType_join
                 from prod2SpiritType in prod2SpiritType_join.DefaultIfEmpty()
                 join matKindRep in db.MaterialKindReporting on prod2SpiritType.MaterialKindReportingID equals matKindRep.MaterialKindReportingID into matKindRep_join
                 from matKindRep in matKindRep_join.DefaultIfEmpty()
                 join spiritTypeRep in db.SpiritTypeReporting on prod2SpiritType.SpiritTypeReportingID equals spiritTypeRep.SpiritTypeReportingID into spiritTypeRep_join
                 from spiritTypeRep in spiritTypeRep_join.DefaultIfEmpty()
                 join prodRepMatCat2MatKind in db.ProdRepMatCat2MaterialKind on matKindRep.MaterialKindReportingID equals prodRepMatCat2MatKind.MaterialKindReportingID into prodRepMatCat2MatKind_join
                 from prodRepMatCat2MatKind in prodRepMatCat2MatKind_join.DefaultIfEmpty()
                 join prodRepMatCat in db.ProductionReportMaterialCategory on prodRepMatCat2MatKind.ProductionReportMaterialCategoryID equals prodRepMatCat.ProductionReportMaterialCategoryID into prodRepMatCat_join
                 from prodRepMatCat in prodRepMatCat_join.DefaultIfEmpty()
                 where
                   distillers.UserId == userId &&
                   (new int[] { 3 }).Contains(prod.StateID) &&
                   prod.ProductionEndTime >= start &&
                   prod.ProductionEndTime <= end
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

        private void GetRedistilledProductionContentRecordsForProcessing(DateTime start, DateTime end, ref List<ProductionContentIterator> prodContentIteratorList, int currRecordId/*if it's a first iteration then it is productionOD else it is recordID*/)
        {
            bool parentGauged = false;
            int parentState = 0;
            int parentProductionId = 0;
            try
            {
                var baseCaseRecords =
                   (from prodContent in db.ProductionContent
                    join CF in db.ContentField on prodContent.ContentFieldID equals CF.ContentFieldID into CF_join
                    from CF in CF_join.DefaultIfEmpty()
                    join prod in db.Production on prodContent.ProductionID equals prod.ProductionID into prod_join
                    from prod in prod_join.DefaultIfEmpty()
                    where
                       prodContent.ProductionID == currRecordId
                       && (new int[] { 16, 18, 20, 22, 23 }).Contains(prodContent.ContentFieldID)
                       && prod.ProductionStartTime >= start
                       && prod.ProductionEndTime <= end
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
                        (from prod in db.Production
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

                        if (current.isProductionComponent == false)
                        {
                            ProductionContentIterator pCI = new ProductionContentIterator();

                            pCI.ProductionId = (int)current.ProductionID;
                            pCI.ProductionContentId = (int)current.ProductionContentID;
                            pCI.RecordId = (int)current.RecordID;
                            pCI.IsProductionComponent = (bool)current.isProductionComponent;
                            pCI.Proof = (float)current.Proof;

                            prodContentIteratorList.Add(pCI);
                        }
                        else if (current.Gauged == true && current.StateID == (int)Persistence.BusinessLogicEnums.State.Distilled && parentGauged == true)
                        {
                            ProductionContentIterator pCI = new ProductionContentIterator();

                            pCI.ProductionId = (int)current.ProductionID;
                            pCI.ProductionContentId = (int)current.ProductionContentID;
                            pCI.RecordId = (int)current.RecordID;
                            pCI.IsProductionComponent = (bool)current.isProductionComponent;
                            pCI.Proof = (float)current.Proof;

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

        private void GetProductionRedistilledRecords(int prodContentId, float proof, ref List<ProdReportPart1> part1List, ref List<ProdReportPart5> prodRPart5L)
        {
            try
            {
                var productionSpiritType =
                (from prodContent in db.ProductionContent
                 join prod2SpiritType in db.ProductionToSpiritTypeReporting on prodContent.RecordID equals prod2SpiritType.ProductionID into pur2SpiritType_join
                 from pur2SpiritType in pur2SpiritType_join.DefaultIfEmpty()
                 join spiritTypeRep in db.SpiritTypeReporting on pur2SpiritType.SpiritTypeReportingID equals spiritTypeRep.SpiritTypeReportingID into spiritTypeRep_join
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
                            spRec.Recd4RedistilaltionL15 += proof;

                            var prod5Rec = prodRPart5L.Find(x => x.KindofSpirits == productionSpiritType.SpiritShortName);

                            if (prod5Rec != null)
                            {
                                prod5Rec.Proof += proof;
                            }
                            else
                            {
                                ProdReportPart5 prod5Inst = new ProdReportPart5();
                                prod5Inst.KindofSpirits = productionSpiritType.SpiritShortName;
                                prod5Inst.Proof = proof;

                                prodRPart5L.Add(prod5Inst);
                            }
                        }
                        else
                        {
                            ProdReportPart1 part1Obj = new ProdReportPart1();
                            part1Obj.Recd4RedistilaltionL15 = proof;
                            part1Obj.SpiritTypeReportingID = (int)productionSpiritType.SpiritTypeReportingID;
                            part1Obj.SpiritCatName = productionSpiritType.SpiritShortName;

                            part1List.Add(part1Obj);

                            ProdReportPart5 prod5Inst = new ProdReportPart5();
                            prod5Inst.KindofSpirits = part1Obj.SpiritCatName;
                            prod5Inst.Proof = part1Obj.Recd4RedistilaltionL15;

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

        public void GetPurchasedRedistilledRecords(int productionContentId, float proof, ref List<ProdReportPart1> part1List, ref List<ProdReportPart5> prodRPart5L)
        {
            try
            {
                var purchaseSpiritType =
                (from prodContent in db.ProductionContent
                 join pur2SpiritType in db.PurchaseToSpiritTypeReporting on prodContent.RecordID equals pur2SpiritType.PurchaseID into pur2SpiritType_join
                 from pur2SpiritType in pur2SpiritType_join.DefaultIfEmpty()
                 join spiritTypeRep in db.SpiritTypeReporting on pur2SpiritType.SpiritTypeReportingID equals spiritTypeRep.SpiritTypeReportingID into spiritTypeRep_join
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
                            spRec.Recd4RedistilaltionL15 += proof;

                            var prod5Rec = prodRPart5L.Find(x => x.KindofSpirits == purchaseSpiritType.SpiritShortName);

                            if (prod5Rec != null)
                            {
                                prod5Rec.Proof += proof;
                            }
                            else
                            {
                                ProdReportPart5 prod5Inst = new ProdReportPart5();
                                prod5Inst.KindofSpirits = purchaseSpiritType.SpiritShortName;
                                prod5Inst.Proof = proof;

                                prodRPart5L.Add(prod5Inst);
                            }
                        }
                        else
                        {
                            ProdReportPart1 part1Obj = new ProdReportPart1();
                            part1Obj.Recd4RedistilaltionL15 = proof;
                            part1Obj.SpiritTypeReportingID = (int)purchaseSpiritType.SpiritTypeReportingID;
                            part1Obj.SpiritCatName = (string)purchaseSpiritType.SpiritShortName;

                            part1List.Add(part1Obj);

                            ProdReportPart5 prod5Inst = new ProdReportPart5();
                            prod5Inst.KindofSpirits = part1Obj.SpiritCatName;
                            prod5Inst.Proof = part1Obj.Recd4RedistilaltionL15;

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
        /// GetAllProductionReportRecordsForGivenPeriod method gets the data for production report part 1
        /// </summary>
        /// <param name="userId">Distiller user id</param>
        /// <param name="start">reporting start period</param>
        /// <param name="end">reporting end period</param>
        /// <param name="tempRepObjList">out parameter object list that needs to be populated</param>
        public void GetAllProductionReportRecordsForGivenPeriod(int userId, DateTime start, DateTime end, ref List<ProductionReportHelper> tempRepObjList)
        {
            try
            {
                var ress =
                (from prod in db.Production
                 join distillers in db.AspNetUserToDistiller on prod.DistillerID equals distillers.DistillerID into distillers_join
                 from distillers in distillers_join.DefaultIfEmpty()
                 join prod2SpiritType in db.ProductionToSpiritTypeReporting on prod.ProductionID equals prod2SpiritType.ProductionID into prod2SpiritType_join
                 from prod2SpiritType in prod2SpiritType_join.DefaultIfEmpty()
                 join matKindRep in db.MaterialKindReporting on prod2SpiritType.MaterialKindReportingID equals matKindRep.MaterialKindReportingID into matKindRep_join
                 from matKindRep in matKindRep_join.DefaultIfEmpty()
                 join spiritTypeRep in db.SpiritTypeReporting on prod2SpiritType.SpiritTypeReportingID equals spiritTypeRep.SpiritTypeReportingID into spiritTypeRep_join
                 from spiritTypeRep in spiritTypeRep_join.DefaultIfEmpty()
                 join prodRepMatCat2MatKind in db.ProdRepMatCat2MaterialKind on matKindRep.MaterialKindReportingID equals prodRepMatCat2MatKind.MaterialKindReportingID into prodRepMatCat2MatKind_join
                 from prodRepMatCat2MatKind in prodRepMatCat2MatKind_join.DefaultIfEmpty()
                 join prodRepMatCat in db.ProductionReportMaterialCategory on prodRepMatCat2MatKind.ProductionReportMaterialCategoryID equals prodRepMatCat.ProductionReportMaterialCategoryID into prodRepMatCat_join
                 from prodRepMatCat in prodRepMatCat_join.DefaultIfEmpty()
                 where
                   distillers.UserId == userId &&
                   prod.Gauged == true &&
                   (new int[] { 3 }).Contains(prod.StateID) &&
                   prod.ProductionEndTime >= start &&
                   prod.ProductionEndTime <= end
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
                    // first, we want to make sure that every record received from the db, has SpiritType and Material Kinds associated with it
                    // because there are cases when Blending and Bottling may not have SpiritType in a reporting sense. Our current design allows it
                    // but maybe worth changing it
                    foreach (var i in tempRepObjList)
                    {
                        if (i.SpiritTypeReportingID == 0)
                        {
                            var k =
                                    (from prodContent in db.ProductionContent
                                     join prod2SpiritType in db.ProductionToSpiritTypeReporting on prodContent.RecordID equals prod2SpiritType.ProductionID into prod2SpiritType_join
                                     from prod2SpiritType in prod2SpiritType_join.DefaultIfEmpty()
                                     join matKindRep in db.MaterialKindReporting on prod2SpiritType.MaterialKindReportingID equals matKindRep.MaterialKindReportingID into matKindRep_join
                                     from matKindRep in matKindRep_join.DefaultIfEmpty()
                                     join spiritTRep in db.SpiritTypeReporting on prod2SpiritType.SpiritTypeReportingID equals spiritTRep.SpiritTypeReportingID into spiritTRep_join
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
                                            (from prodContent in db.ProductionContent
                                             join prod2SpiritType in db.ProductionToSpiritTypeReporting on prodContent.RecordID equals prod2SpiritType.ProductionID into prod2SpiritType_join
                                             from prod2SpiritType in prod2SpiritType_join.DefaultIfEmpty()
                                             join matKindRep in db.MaterialKindReporting on prod2SpiritType.MaterialKindReportingID equals matKindRep.MaterialKindReportingID into matKindRep_join
                                             from matKindRep in matKindRep_join.DefaultIfEmpty()
                                             join spiritTRep in db.SpiritTypeReporting on prod2SpiritType.SpiritTypeReportingID equals spiritTRep.SpiritTypeReportingID into spiritTRep_join
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
        /// GetUnfinishedSpiritsForProductionReport method gets quarterly unfinished spirits data
        /// </summary>
        /// <param name="userId">Distiller user id</param>
        /// <param name="start">reporting start period</param>
        /// <param name="end">reporting end period</param>
        /// <param name="tempRepObjList">out parameter object list that needs to be populated</param>
        public void GetUnfinishedSpiritsForProductionReport(int userId, DateTime start, DateTime end, ref List<ProductionReportHelper> tempRepObjList)
        {
            // this section of code checks the reporting month to determine whether we need to report
            // line 17 of production report
            // for February reporting
            const int february = 2;
            const int may = 5;
            const int august = 8;
            const int november = 11;
            DateTime[] startAndEndDates = new DateTime[2]; // [startDate, endDate]

            if (end.Month == february)
            {
                start = new DateTime(end.Year - 1, 12, 1);
            }
            else if (end.Month == may)
            {
                start = new DateTime(end.Year, 3, 1);
            }
            else if (end.Month == august)
            {
                start = new DateTime(end.Year, 6, 1);
            }
            else if (end.Month == november)
            {
                start = new DateTime(end.Year, 9, 1);
            }

            try
            {
                var ress =
                (from prod in db.Production
                 join prodReport in db.Production4Reporting on prod.ProductionID equals prodReport.ProductionID into prodReport_join
                 from prodReport in prodReport_join.DefaultIfEmpty()
                 join distillers in db.AspNetUserToDistiller on prod.DistillerID equals distillers.DistillerID into distillers_join
                 from distillers in distillers_join.DefaultIfEmpty()
                 join prod2SpiritType in db.ProductionToSpiritTypeReporting on prod.ProductionID equals prod2SpiritType.ProductionID into prod2SpiritType_join
                 from prod2SpiritType in prod2SpiritType_join.DefaultIfEmpty()
                 join matKindRep in db.MaterialKindReporting on prod2SpiritType.MaterialKindReportingID equals matKindRep.MaterialKindReportingID into matKindRep_join
                 from matKindRep in matKindRep_join.DefaultIfEmpty()
                 join spiritTypeRep in db.SpiritTypeReporting on prod2SpiritType.SpiritTypeReportingID equals spiritTypeRep.SpiritTypeReportingID into spiritTypeRep_join
                 from spiritTypeRep in spiritTypeRep_join.DefaultIfEmpty()
                 join prod2Purch in db.ProductionToPurchase on prod.ProductionID equals prod2Purch.ProductionID into prod2Purch_join
                 from prod2Purch in prod2Purch_join.DefaultIfEmpty()
                 join purch4Reprt in db.Purchase4Reporting on prod2Purch.PurchaseID equals purch4Reprt.PurchaseID into purch4Reprt_join
                 from purch4Reprt in purch4Reprt_join.DefaultIfEmpty()
                 join prodRepMatCat2MatKind in db.ProdRepMatCat2MaterialKind on matKindRep.MaterialKindReportingID equals prodRepMatCat2MatKind.MaterialKindReportingID into prodRepMatCat2MatKind_join
                 from prodRepMatCat2MatKind in prodRepMatCat2MatKind_join.DefaultIfEmpty()
                 join prodRepMatCat in db.ProductionReportMaterialCategory on prodRepMatCat2MatKind.ProductionReportMaterialCategoryID equals prodRepMatCat.ProductionReportMaterialCategoryID into prodRepMatCat_join
                 from prodRepMatCat in prodRepMatCat_join.DefaultIfEmpty()
                 where
                   distillers.UserId == userId &&
                   prod.Gauged == false &&
                   (prod.StatusID == (int)Persistence.BusinessLogicEnums.Status.Active ||
                   prod.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processing) &&
                   (new int[] { 3, 4, 5 }).Contains(prod.StateID) &&
                   prod.ProductionEndTime >= start &&
                   prod.ProductionEndTime <= end
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
                    tempRepObj.Weight = (float)l.Weight;
                    tempRepObj.Volume = (float)l.Volume;
                    tempRepObj.Alcohol = (float)l.Alcohol;
                    tempRepObj.Proof = (float)l.Proof;
                    tempRepObj.SpiritTypeReportingID = (int)l.SpiritTypeReportingID;
                    tempRepObj.MaterialKindReportingID = (int)l.MaterialKindReportingID;
                    tempRepObj.Gauged = l.Gauged;

                    tempRepObjList.Add(tempRepObj);
                }

                if (tempRepObjList != null)
                {
                    // first, we want to make sure that every record received from the db, has SpiritType and Material Kinds associated with it
                    // because there are cases when Blending and Bottling may not have SpiritType in a reporting sense. Our current design allows it
                    // but maybe worth changing it
                    foreach (var i in tempRepObjList)
                    {
                        if (i.SpiritTypeReportingID == 0)
                        {
                            try
                            {
                                var k =
                                      (from prodContent in db.ProductionContent
                                       join prod2SpiritType in db.ProductionToSpiritTypeReporting on prodContent.RecordID equals prod2SpiritType.ProductionID into prod2SpiritType_join
                                       from prod2SpiritType in prod2SpiritType_join.DefaultIfEmpty()
                                       join matKindRep in db.MaterialKindReporting on prod2SpiritType.MaterialKindReportingID equals matKindRep.MaterialKindReportingID into matKindRep_join
                                       from matKindRep in matKindRep_join.DefaultIfEmpty()
                                       join spiritTRep in db.SpiritTypeReporting on prod2SpiritType.SpiritTypeReportingID equals spiritTRep.SpiritTypeReportingID into spiritTRep_join
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
                                                (from prodContent in db.ProductionContent
                                                 join prod2SpiritType in db.ProductionToSpiritTypeReporting on prodContent.RecordID equals prod2SpiritType.ProductionID into prod2SpiritType_join
                                                 from prod2SpiritType in prod2SpiritType_join.DefaultIfEmpty()
                                                 join matKindRep in db.MaterialKindReporting on prod2SpiritType.MaterialKindReportingID equals matKindRep.MaterialKindReportingID into matKindRep_join
                                                 from matKindRep in matKindRep_join.DefaultIfEmpty()
                                                 join spiritTRep in db.SpiritTypeReporting on prod2SpiritType.SpiritTypeReportingID equals spiritTRep.SpiritTypeReportingID into spiritTRep_join
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

        public StorageReport GetStorageReportData(DateTime startDate, DateTime endDate, int userId)
        {
            try
            {
                StorageReport storageReport = new StorageReport();
                List<StorageReportCategory> storageReportBody = new List<StorageReportCategory>();

                // get distiller information for header report
                int distillerID = GetDistillerId(userId);

                storageReport.Header = GetDistillerInfoForReportHeader(distillerID, startDate);

                GetProducedOnHandFirstOfMonth(startDate, endDate, userId, ref storageReportBody);

                GetPurchasedOnHandFirstOfMonth(startDate, endDate, userId, ref storageReportBody);

                GetProducedDepositedToStorage(startDate, endDate, userId, ref storageReportBody);

                GetPurchasedDepositedToStorage(startDate, endDate, userId, ref storageReportBody);

                GetProducedStorageToProduction(startDate, endDate, userId, ref storageReportBody);

                GetPurchasedStorageToProduction(startDate, endDate, userId, ref storageReportBody);

                GetProducedStorageToProcessing(startDate, endDate, userId, ref storageReportBody);

                GetPurchasedStorageToProcessing(startDate, endDate, userId, ref storageReportBody);

                GetStorageReportDestroyed(startDate, endDate, userId, ref storageReportBody);

                GetStorageOnHandEndOfMonth(startDate, endDate, userId, ref storageReportBody);

                ComputeLine6ForStorageReport(ref storageReportBody);

                ComputeLine24ForStorageReport(ref storageReportBody);

                storageReport.ReportBody = storageReportBody;

                return storageReport;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// 1. On hand first of month (Purchased)
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="userId"></param>
        /// <param name="storageReportBody"></param>
        private void GetPurchasedOnHandFirstOfMonth(DateTime startDate, DateTime endDate, int userId, ref List<StorageReportCategory> storageReportBody)
        {
            // Query distilled purchase records transferred to storage account
            var records =
                (from purchase in db.Purchase
                 join distiller in db.AspNetUserToDistiller on purchase.DistillerID equals distiller.DistillerID into distiller_join
                 from distiller in distiller_join.DefaultIfEmpty()
                 join mDic in db.MaterialDict on purchase.MaterialDictID equals mDic.MaterialDictID into mDic_join
                 from mDic in mDic_join.DefaultIfEmpty()
                 join uOm in db.UnitOfMeasurement on mDic.UnitOfMeasurementID equals uOm.UnitOfMeasurementID into uOm_join
                 from uOm in uOm_join.DefaultIfEmpty()
                 join dest in db.Destruction on purchase.PurchaseID equals dest.RecordID into dest_join
                 from dest in dest_join.DefaultIfEmpty()
                 join proof in db.Proof on purchase.ProofID equals proof.ProofID into proof_join
                 from proof in proof_join.DefaultIfEmpty()
                 join productionContent in db.ProductionContent on purchase.PurchaseID equals productionContent.RecordID into productionContent_join
                 from productionContent in productionContent_join.DefaultIfEmpty()
                 join production in db.Production on productionContent.ProductionID equals production.ProductionID into production_join
                 from production in production_join.DefaultIfEmpty()
                 join productionToSpiritType in db.PurchaseToSpiritTypeReporting on purchase.PurchaseID equals productionToSpiritType.PurchaseID into productionToSpiritType_join
                 from productionToSpiritType in productionToSpiritType_join.DefaultIfEmpty()
                 join str in db.SpiritTypeReporting on productionToSpiritType.SpiritTypeReportingID equals str.SpiritTypeReportingID into str_join
                 from str in str_join.DefaultIfEmpty()
                 where
                     distiller.UserId == userId
                     && (purchase.PurchaseTypeID == 2 || purchase.PurchaseTypeID == 3)
                     && purchase.PurchaseDate < startDate
                     && ((purchase.StatusID == (int)Persistence.BusinessLogicEnums.Status.Active || purchase.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processing || purchase.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processed) || (purchase.StateID == (int)Persistence.BusinessLogicEnums.Status.Destroyed && dest.EndTime > startDate && dest.EndTime < endDate))
                     && (productionContent == null || (productionContent != null && (productionContent.ContentFieldID == 16 || productionContent.ContentFieldID == 18)))
                     && uOm.UnitOfMeasurementID != 2 // != "lb"
                 select new
                 {
                     productionDate = (DateTime?)production.ProductionEndTime,
                     reportingCategoryName = str.ProductTypeName ?? string.Empty,
                     spiritTypeReportingId = (int?)str.SpiritTypeReportingID ?? 0,
                     proof = (float?)proof.Value ?? 0,
                     destroyedProof = (float?)dest.ProofGallons ?? 0,
                     productionContentProof = (float?)productionContent.ContentValue ?? 0
                 }).DefaultIfEmpty();

            if (records.First() != null)
            {
                foreach (var rec in records)
                {
                    // Search for existing category with matching name
                    var category = storageReportBody.Find(x => x.SpiritTypeReportingID == rec.spiritTypeReportingId);

                    if (category == null)
                    {
                        var total = rec.proof + rec.destroyedProof;

                        if (rec.productionDate != null && rec.productionContentProof > 0 && rec.productionDate >= startDate && rec.productionDate <= endDate)
                        {
                            total += rec.productionContentProof;
                        }

                        if (total > 0)
                        {
                            // Add category to the list with given produced distilled batch ReportingCategoryName and update relevant rows
                            StorageReportCategory cat = new StorageReportCategory();
                            cat.SpiritTypeReportingID = rec.spiritTypeReportingId;
                            cat.CategoryName = rec.reportingCategoryName;
                            cat.r1_OnHandFirstOfMonth += total;
                            storageReportBody.Add(cat);
                        }
                    }
                    else
                    {
                        category.r1_OnHandFirstOfMonth += rec.proof + rec.destroyedProof;

                        if (rec.productionDate != null && rec.productionContentProof > 0 && rec.productionDate >= startDate && rec.productionDate <= endDate)
                        {
                            category.r1_OnHandFirstOfMonth += rec.productionContentProof;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 1. On hand first of month (Produced)
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="userId"></param>
        /// <param name="storageReportBody"></param>
        private void GetProducedOnHandFirstOfMonth(DateTime startDate, DateTime endDate, int userId, ref List<StorageReportCategory> storageReportBody)
        {
            // Query distilled production records transferred to storage account
            var records =
                (from sourceProduction in db.Production
                 join distiller in db.AspNetUserToDistiller on sourceProduction.DistillerID equals distiller.DistillerID into distiller_join
                 from distiller in distiller_join.DefaultIfEmpty()
                 join dest in db.Destruction on sourceProduction.ProductionID equals dest.RecordID into dest_join
                 from dest in dest_join.DefaultIfEmpty()
                 join proof in db.Proof on sourceProduction.ProofID equals proof.ProofID into proof_join
                 from proof in proof_join.DefaultIfEmpty()
                 join productionContent in db.ProductionContent on sourceProduction.ProductionID equals productionContent.RecordID into productionContent_join
                 from productionContent in productionContent_join.DefaultIfEmpty()
                 join outputProduction in db.Production on productionContent.ProductionID equals outputProduction.ProductionID into outputProductionRecord_join
                 from outputProduction in outputProductionRecord_join.DefaultIfEmpty()
                 join productionToSpiritType in db.ProductionToSpiritTypeReporting on sourceProduction.ProductionID equals productionToSpiritType.ProductionID into productionToSpiritType_join
                 from productionToSpiritType in productionToSpiritType_join.DefaultIfEmpty()
                 join str in db.SpiritTypeReporting on productionToSpiritType.SpiritTypeReportingID equals str.SpiritTypeReportingID into str_join
                 from str in str_join.DefaultIfEmpty()
                 where
                     distiller.UserId == userId
                     && (sourceProduction.ProductionTypeID == 1 || sourceProduction.ProductionTypeID == 2)
                     && sourceProduction.ProductionEndTime < startDate
                     && sourceProduction.Gauged == true
                     && ((sourceProduction.StatusID == (int)Persistence.BusinessLogicEnums.Status.Active || sourceProduction.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processing || sourceProduction.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processed) || (sourceProduction.StateID == (int)Persistence.BusinessLogicEnums.Status.Destroyed && dest.EndTime > startDate && dest.EndTime < endDate))
                     && (productionContent == null || (productionContent != null && (productionContent.ContentFieldID == 20 || productionContent.ContentFieldID == 23)))
                 select new
                 {
                     productionDate = (DateTime?)outputProduction.ProductionEndTime,
                     reportingCategoryName = str.ProductTypeName ?? string.Empty,
                     spiritTypeReportingId = (int?)str.SpiritTypeReportingID ?? 0,
                     proof = (float?)proof.Value ?? 0,
                     destroyedProof = (float?)dest.ProofGallons ?? 0,
                     productionContentProof = (float?)productionContent.ContentValue ?? 0
                 }).DefaultIfEmpty();

            if (records.First() != null)
            {
                foreach (var rec in records)
                {
                    // Search for existing category with matching name
                    var category = storageReportBody.Find(x => x.SpiritTypeReportingID == rec.spiritTypeReportingId);

                    if (category == null)
                    {
                        var total = rec.proof + rec.destroyedProof;

                        if (rec.productionContentProof > 0 && rec.productionDate >= startDate && rec.productionDate <= endDate)
                        {
                            total += rec.productionContentProof;
                        }

                        if (total > 0)
                        {
                            // Add category to the list with given produced distilled batch ReportingCategoryName and update relevant rows
                            StorageReportCategory cat = new StorageReportCategory();
                            cat.SpiritTypeReportingID = rec.spiritTypeReportingId;
                            cat.CategoryName = rec.reportingCategoryName;
                            cat.r1_OnHandFirstOfMonth += total;
                            storageReportBody.Add(cat);
                        }
                    }
                    else
                    {
                        category.r1_OnHandFirstOfMonth += rec.proof + rec.destroyedProof;

                        if (rec.productionContentProof > 0 && rec.productionDate >= startDate && rec.productionDate <= endDate)
                        {
                            category.r1_OnHandFirstOfMonth += rec.productionContentProof;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 2. Deposited in bulk storage (purchased)
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="userId"></param>
        /// <param name="storageReportBody"></param>
        private void GetPurchasedDepositedToStorage(DateTime startDate, DateTime endDate, int userId, ref List<StorageReportCategory> storageReportBody)
        {
            // Query fermented and distilled purchase records deposited to storage account
            var records =
                (from purchase in db.Purchase
                 join distiller in db.AspNetUserToDistiller
                    on purchase.DistillerID equals distiller.DistillerID into distiller_join
                 from distiller in distiller_join.DefaultIfEmpty()
                 join materialDictionary in db.MaterialDict
                    on purchase.MaterialDictID equals materialDictionary.MaterialDictID into materialDictionary_join
                 from materialDictionary in materialDictionary_join.DefaultIfEmpty()
                 join unitOfMeasurement in db.UnitOfMeasurement
                    on materialDictionary.UnitOfMeasurementID equals unitOfMeasurement.UnitOfMeasurementID into unitOfMeasurement_join
                 from unitOfMeasurement in unitOfMeasurement_join.DefaultIfEmpty()
                 join destruction in db.Destruction
                    on purchase.PurchaseID equals destruction.RecordID into destruction_join
                 from destruction in destruction_join.DefaultIfEmpty()
                 join productionContent in db.ProductionContent
                    on purchase.PurchaseID equals productionContent.RecordID into productionContent_join
                 from productionContent in productionContent_join.DefaultIfEmpty()
                 join proof in db.Proof
                    on purchase.ProofID equals proof.ProofID into proof_join
                 from proof in proof_join.DefaultIfEmpty()
                 join purchaseToSpiritType in db.PurchaseToSpiritTypeReporting
                    on purchase.PurchaseID equals purchaseToSpiritType.PurchaseID into purchaseToSpiritType_join
                 from purchaseToSpiritType in purchaseToSpiritType_join.DefaultIfEmpty()
                 join spiritTypeReporting in db.SpiritTypeReporting
                    on purchaseToSpiritType.SpiritTypeReportingID equals spiritTypeReporting.SpiritTypeReportingID into spiritTypeReporting_join
                 from spiritTypeReporting in spiritTypeReporting_join.DefaultIfEmpty()
                 where
                     distiller.UserId == userId
                     && (purchase.PurchaseTypeID == 2
                        || purchase.PurchaseTypeID == 3)
                     && (purchase.StatusID == (int)Persistence.BusinessLogicEnums.Status.Active
                        || purchase.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processing
                        || purchase.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processed)
                     && purchase.PurchaseDate >= startDate
                     && purchase.PurchaseDate <= endDate
                     && (productionContent == null
                        || (productionContent != null && (productionContent.ContentFieldID == 16 || productionContent.ContentFieldID == 18))) // 16 = PurFermentedProofGal, 18 = PurDistilledProofGal
                     && unitOfMeasurement.UnitOfMeasurementID != 2 // != "lb"
                 select new
                 {
                     reportingCategoryName = spiritTypeReporting.ProductTypeName ?? string.Empty,
                     spiritTypeReportingId = (int?)spiritTypeReporting.SpiritTypeReportingID ?? 0,
                     purchaseProof = (float?)proof.Value ?? 0,
                     productionProof = (float?)productionContent.ContentValue ?? 0,
                     destroyedProof = (float?)destruction.ProofGallons ?? 0
                 }).DefaultIfEmpty();

            if (records.First() != null)
            {
                foreach (var rec in records)
                {
                    // Search for existing category with matching name
                    var category = storageReportBody.Find(x => x.SpiritTypeReportingID == rec.spiritTypeReportingId);

                    if (category == null)
                    {
                        var total = rec.purchaseProof + rec.productionProof + rec.destroyedProof;

                        if (total > 0)
                        {
                            // Add category to the list with given produced distilled batch ReportingCategoryName and update relevant rows
                            StorageReportCategory cat = new StorageReportCategory();
                            cat.SpiritTypeReportingID = rec.spiritTypeReportingId;
                            cat.CategoryName = rec.reportingCategoryName;
                            cat.r2_DepositedInBulkStorage += total;
                            storageReportBody.Add(cat);
                        }
                    }
                    else
                    {
                        category.r2_DepositedInBulkStorage += rec.purchaseProof + rec.productionProof + rec.destroyedProof;
                    }
                }
            }
        }

        /// <summary>
        /// 2. Deposited in bulk storage (Produced)
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="userId"></param>
        /// <param name="storageReportBody"></param>
        private void GetProducedDepositedToStorage(DateTime startDate, DateTime endDate, int userId, ref List<StorageReportCategory> storageReportBody)
        {
            // Query distilled production records transferred to storage account
            var records =
                (from production in db.Production
                 join distiller in db.AspNetUserToDistiller
                    on production.DistillerID equals distiller.DistillerID into distiller_join
                 from distiller in distiller_join.DefaultIfEmpty()
                 join destruction in db.Destruction
                    on production.ProductionID equals destruction.RecordID into destruction_join
                 from destruction in destruction_join.DefaultIfEmpty()
                 join productionContent in db.ProductionContent
                    on production.ProductionID equals productionContent.RecordID into productionContent_join
                 from productionContent in productionContent_join.DefaultIfEmpty()
                 join productionOutput in db.Production
                    on productionContent.ProductionID equals productionOutput.ProductionID into productionOutput_join
                 from productionOutput in productionOutput_join.DefaultIfEmpty()
                 join proof in db.Proof
                    on production.ProofID equals proof.ProofID into proof_join
                 from proof in proof_join.DefaultIfEmpty()
                 join productionToSpiritType in db.ProductionToSpiritTypeReporting
                    on production.ProductionID equals productionToSpiritType.ProductionID into productionToSpiritType_join
                 from productionToSpiritType in productionToSpiritType_join.DefaultIfEmpty()
                 join spiritTypeReporting in db.SpiritTypeReporting
                    on productionToSpiritType.SpiritTypeReportingID equals spiritTypeReporting.SpiritTypeReportingID into spiritTypeReporting_join
                 from spiritTypeReporting in spiritTypeReporting_join.DefaultIfEmpty()
                 where
                     distiller.UserId == userId
                     && (production.ProductionTypeID == 1
                        || production.ProductionTypeID == 2)
                 && production.ProductionEndTime >= startDate
                 && production.ProductionEndTime <= endDate
                 && production.Gauged == true
                 && (production.StatusID == (int)Persistence.BusinessLogicEnums.Status.Active
                    || production.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processing
                    || (production.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processed && productionOutput.ProductionTypeID == 2)
                    || (production.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processed && productionOutput.ProductionTypeID == 3 && productionOutput.ProductionEndTime > endDate)
                    || (production.StateID == (int)Persistence.BusinessLogicEnums.Status.Destroyed && destruction.EndTime > endDate))
                 && (productionContent == null || (productionContent != null && (productionContent.ContentFieldID == 20 || productionContent.ContentFieldID == 23))) // 20 = ProdDistilledProofGal, 23 = ProdFermentedProofGal
                 select new
                 {
                     productionStatus = (int?)production.StatusID ?? 0,
                     reportingCategoryName = spiritTypeReporting.ProductTypeName ?? string.Empty,
                     spiritTypeReportingId = (int?)spiritTypeReporting.SpiritTypeReportingID ?? 0,
                     proof = (float?)proof.Value ?? 0,
                     productionContentProof = (float?)productionContent.ContentValue ?? 0,
                     destroyedProof = (float?)destruction.ProofGallons ?? 0
                 }).DefaultIfEmpty();

            if (records.First() != null)
            {
                foreach (var rec in records)
                {
                    // Search for existing category with matching name
                    var category = storageReportBody.Find(x => x.SpiritTypeReportingID == rec.spiritTypeReportingId);

                    if (category == null)
                    {
                        var total = rec.proof + rec.destroyedProof + rec.productionContentProof;

                        if (total > 0)
                        {
                            // Add category to the list with given produced distilled batch ReportingCategoryName and update relevant rows
                            StorageReportCategory cat = new StorageReportCategory();
                            cat.SpiritTypeReportingID = rec.spiritTypeReportingId;
                            cat.CategoryName = rec.reportingCategoryName;
                            cat.r2_DepositedInBulkStorage += total;
                            storageReportBody.Add(cat);
                        }
                    }
                    else
                    {
                        category.r2_DepositedInBulkStorage += rec.proof + rec.productionContentProof + rec.destroyedProof;
                    }
                }
            }
        }

        /// <summary>
        /// 6. TOTAL (Lines 1 through 5)
        /// </summary>
        /// <param name="storageReportBody"></param>
        private void ComputeLine6ForStorageReport(ref List<StorageReportCategory> storageReportBody)
        {
            foreach (var storage in storageReportBody)
            {
                storage.r6_TotalLines1Through5 = storage.r1_OnHandFirstOfMonth + storage.r2_DepositedInBulkStorage + storage.r4_ReturnedToBulkStorage;
            }
        }

        /// <summary>
        /// 17. Transferred to processing account (Purchased)
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="userId"></param>
        /// <param name="storageReportBody"></param>
        private void GetPurchasedStorageToProcessing(DateTime startDate, DateTime endDate, int userId, ref List<StorageReportCategory> storageReportBody)
        {
            // Query distilled purchase records transferred from storage account to processing account
            var purRes =
                (from purchase in db.Purchase
                 join distiller in db.AspNetUserToDistiller
                    on purchase.DistillerID equals distiller.DistillerID into distiller_join
                 from distiller in distiller_join.DefaultIfEmpty()
                 join materialDictionary in db.MaterialDict
                    on purchase.MaterialDictID equals materialDictionary.MaterialDictID into materialDictionary_join
                 from materialDictionary in materialDictionary_join.DefaultIfEmpty()
                 join unitOfMeasurement in db.UnitOfMeasurement
                    on materialDictionary.UnitOfMeasurementID equals unitOfMeasurement.UnitOfMeasurementID into unitOfMeasurement_join
                 from unitOfMeasurement in unitOfMeasurement_join.DefaultIfEmpty()
                 join productionContent in db.ProductionContent
                    on purchase.PurchaseID equals productionContent.RecordID into productionContent_join
                 from productionContent in productionContent_join.DefaultIfEmpty()
                 join production in db.Production
                    on productionContent.ProductionID equals production.ProductionID into production_join
                 from production in production_join.DefaultIfEmpty()
                 join purchaseToSpiritType in db.PurchaseToSpiritTypeReporting
                    on purchase.PurchaseID equals purchaseToSpiritType.PurchaseID into purchaseToSpiritType_join
                 from purchaseToSpiritType in purchaseToSpiritType_join.DefaultIfEmpty()
                 join spiritTypeReporting in db.SpiritTypeReporting
                    on purchaseToSpiritType.SpiritTypeReportingID equals spiritTypeReporting.SpiritTypeReportingID into spiritTypeReporting_join
                 from spiritTypeReporting in spiritTypeReporting_join.DefaultIfEmpty()
                 where
                     distiller.UserId == userId
                     && purchase.PurchaseTypeID == 3
                     && production.ProductionTypeID == 3
                     && (purchase.StatusID == (int)Persistence.BusinessLogicEnums.Status.Active
                        || purchase.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processing
                        || purchase.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processed)
                     && purchase.PurchaseDate < endDate
                     && production.ProductionEndTime >= startDate
                     && production.ProductionEndTime <= endDate
                     && productionContent.ContentFieldID == 18 // PurDistilledProofGal
                     && unitOfMeasurement.UnitOfMeasurementID != 2 // != "lb"
                 select new
                 {
                     reportingCategoryName = spiritTypeReporting.ProductTypeName ?? string.Empty,
                     spiritTypeReportingId = (int?)spiritTypeReporting.SpiritTypeReportingID ?? 0,
                     proofGal = (float?)productionContent.ContentValue ?? 0
                 }).DefaultIfEmpty();

            if (purRes.First() != null)
            {
                foreach (var rec in purRes)
                {
                    // Search for existing category with matching name
                    var category = storageReportBody.Find(x => x.SpiritTypeReportingID == rec.spiritTypeReportingId);

                    if (category == null)
                    {
                        var total = rec.proofGal;

                        if (total > 0)
                        {
                            // Add category to the list with given produced distilled batch ReportingCategoryName and update relevant rows
                            StorageReportCategory cat = new StorageReportCategory();
                            cat.SpiritTypeReportingID = rec.spiritTypeReportingId;
                            cat.CategoryName = rec.reportingCategoryName;
                            cat.r17_TransferredToProcessingAccount += total;
                            storageReportBody.Add(cat);
                        }
                    }
                    else
                    {
                        category.r17_TransferredToProcessingAccount += rec.proofGal;
                    }
                }
            }
        }

        /// <summary>
        /// 17. Transferred to processing account (Produced)
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="userId"></param>
        /// <param name="storageReportBody"></param>
        private void GetProducedStorageToProcessing(DateTime startDate, DateTime endDate, int userId, ref List<StorageReportCategory> storageReportBody)
        {
            // Query distilled production records transferred from storage account to processing account
            var prodRes =
                (from sourceProductionRecord in db.Production
                 join distiller in db.AspNetUserToDistiller
                    on sourceProductionRecord.DistillerID equals distiller.DistillerID into distiller_join
                 from distiller in distiller_join.DefaultIfEmpty()
                 join productionContent in db.ProductionContent
                    on sourceProductionRecord.ProductionID equals productionContent.RecordID into productionContent_join
                 from productionContent in productionContent_join.DefaultIfEmpty()
                 join outputProductionRecord in db.Production
                    on productionContent.ProductionID equals outputProductionRecord.ProductionID into outputProductionRecord_join
                 from outputProductionRecord in outputProductionRecord_join.DefaultIfEmpty()
                 join productionToSpiritType in db.ProductionToSpiritTypeReporting
                    on sourceProductionRecord.ProductionID equals productionToSpiritType.ProductionID into productionToSpiritType_join
                 from productionToSpiritType in productionToSpiritType_join.DefaultIfEmpty()
                 join spiritType in db.SpiritTypeReporting
                    on productionToSpiritType.SpiritTypeReportingID equals spiritType.SpiritTypeReportingID into spiritType_join
                 from spiritType in spiritType_join.DefaultIfEmpty()
                 where
                     distiller.UserId == userId
                     && sourceProductionRecord.ProductionTypeID == 2
                     && outputProductionRecord.ProductionTypeID == 3
                     && (sourceProductionRecord.StatusID == (int)Persistence.BusinessLogicEnums.Status.Active
                        || sourceProductionRecord.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processing
                        || sourceProductionRecord.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processed)
                     && productionContent.ContentFieldID == 20 // ProdDistilledProofGal
                     && sourceProductionRecord.ProductionEndTime < startDate
                     && outputProductionRecord.ProductionEndTime >= startDate
                     && outputProductionRecord.ProductionEndTime <= endDate
                 select new
                 {
                     reportingCategoryName = spiritType.ProductTypeName ?? string.Empty,
                     spiritTypeReportingId = (int?)spiritType.SpiritTypeReportingID ?? 0,
                     proofGal = (float?)productionContent.ContentValue ?? 0
                 }).DefaultIfEmpty();

            if (prodRes.First() != null)
            {
                foreach (var rec in prodRes)
                {
                    // Search for existing category with matching name
                    var category = storageReportBody.Find(x => x.SpiritTypeReportingID == rec.spiritTypeReportingId);

                    if (category == null)
                    {
                        var total = rec.proofGal;

                        if (total > 0)
                        {
                            // Add category to the list with given produced distilled batch ReportingCategoryName and update relevant rows
                            StorageReportCategory cat = new StorageReportCategory();
                            cat.SpiritTypeReportingID = rec.spiritTypeReportingId;
                            cat.CategoryName = rec.reportingCategoryName;
                            cat.r17_TransferredToProcessingAccount += total;
                            storageReportBody.Add(cat);
                        }
                    }
                    else
                    {
                        category.r17_TransferredToProcessingAccount += rec.proofGal;
                    }
                }
            }
        }

        /// <summary>
        /// 18. Transferred to production account (Purchased)
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="userId"></param>
        /// <param name="storageReportBody"></param>
        private void GetPurchasedStorageToProduction(DateTime startDate, DateTime endDate, int userId, ref List<StorageReportCategory> storageReportBody)
        {
            // Query distilled purchase records to transfer from storage account to production account
            var purRes =
                (from purchase in db.Purchase
                 join distiller in db.AspNetUserToDistiller
                    on purchase.DistillerID equals distiller.DistillerID into distiller_join
                 from distiller in distiller_join.DefaultIfEmpty()
                 join materialDictionary in db.MaterialDict
                    on purchase.MaterialDictID equals materialDictionary.MaterialDictID into materialDictionary_join
                 from materialDictionary in materialDictionary_join.DefaultIfEmpty()
                 join unitOfMeasurement in db.UnitOfMeasurement
                    on materialDictionary.UnitOfMeasurementID equals unitOfMeasurement.UnitOfMeasurementID into unitOfMeasurement_join
                 from unitOfMeasurement in unitOfMeasurement_join.DefaultIfEmpty()
                 join productionContent in db.ProductionContent
                    on purchase.PurchaseID equals productionContent.RecordID into productionContent_join
                 from productionContent in productionContent_join.DefaultIfEmpty()
                 join production in db.Production
                    on productionContent.ProductionID equals production.ProductionID into production_join
                 from production in production_join.DefaultIfEmpty()
                 join productionToSpiritType in db.PurchaseToSpiritTypeReporting
                    on purchase.PurchaseID equals productionToSpiritType.PurchaseID into productionToSpiritType_join
                 from productionToSpiritType in productionToSpiritType_join.DefaultIfEmpty()
                 join spiritType in db.SpiritTypeReporting
                    on productionToSpiritType.SpiritTypeReportingID equals spiritType.SpiritTypeReportingID into spiritType_join
                 from spiritType in spiritType_join.DefaultIfEmpty()
                 where
                     distiller.UserId == userId
                     && (purchase.PurchaseTypeID == 2
                        || purchase.PurchaseTypeID == 3)
                     && production.ProductionTypeID == 2
                     && (purchase.StatusID == (int)Persistence.BusinessLogicEnums.Status.Active
                        || purchase.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processing
                        || purchase.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processed)
                     && purchase.PurchaseDate < endDate
                     && production.ProductionEndTime >= startDate
                     && production.ProductionEndTime <= endDate
                     && (productionContent.ContentFieldID == 16 // PurFermentedProofGal
                     || productionContent.ContentFieldID == 18) // PurDistilledProofGal
                     && unitOfMeasurement.UnitOfMeasurementID != 2 // != "lb"
                 select new
                 {
                     reportingCategoryName = spiritType.ProductTypeName ?? string.Empty,
                     spiritTypeReportingId = (int?)spiritType.SpiritTypeReportingID ?? 0,
                     proofGal = (float?)productionContent.ContentValue ?? 0
                 }).DefaultIfEmpty();

            if (purRes.First() != null)
            {
                foreach (var rec in purRes)
                {
                    // Search for existing category with matching name
                    var category = storageReportBody.Find(x => x.SpiritTypeReportingID == rec.spiritTypeReportingId);

                    if (category == null)
                    {
                        var total = rec.proofGal;

                        if (total > 0)
                        {
                            // Add category to the list with given produced distilled batch ReportingCategoryName and update relevant rows
                            StorageReportCategory cat = new StorageReportCategory();
                            cat.SpiritTypeReportingID = rec.spiritTypeReportingId;
                            cat.CategoryName = rec.reportingCategoryName;
                            cat.r18_TransferredToProductionAccount += total;
                            storageReportBody.Add(cat);
                        }
                    }
                    else
                    {
                        category.r18_TransferredToProductionAccount += rec.proofGal;
                    }
                }
            }
        }

        /// <summary>
        /// 18. Transferred to production account (Produced)
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="userId"></param>
        /// <param name="storageReportBody"></param>
        private void GetProducedStorageToProduction(DateTime startDate, DateTime endDate, int userId, ref List<StorageReportCategory> storageReportBody)
        {
            // Query distilled production records transferred from storage account to production account
            var prodRes =
                (from sourceProductionRecord in db.Production
                 join distiller in db.AspNetUserToDistiller
                    on sourceProductionRecord.DistillerID equals distiller.DistillerID into distiller_join
                 from distiller in distiller_join.DefaultIfEmpty()
                 join productionContent in db.ProductionContent
                    on sourceProductionRecord.ProductionID equals productionContent.RecordID into productionContent_join
                 from productionContent in productionContent_join.DefaultIfEmpty()
                 join outputProductionRecord in db.Production
                    on productionContent.ProductionID equals outputProductionRecord.ProductionID into outputProductionRecord_join
                 from outputProductionRecord in outputProductionRecord_join.DefaultIfEmpty()
                 join productionToSpiritType in db.ProductionToSpiritTypeReporting
                    on sourceProductionRecord.ProductionID equals productionToSpiritType.ProductionID into productionToSpiritType_join
                 from productionToSpiritType in productionToSpiritType_join.DefaultIfEmpty()
                 join spiritType in db.SpiritTypeReporting
                    on productionToSpiritType.SpiritTypeReportingID equals spiritType.SpiritTypeReportingID into spiritType_join
                 from spiritType in spiritType_join.DefaultIfEmpty()
                 where
                     distiller.UserId == userId
                     && (sourceProductionRecord.ProductionTypeID == 1 || sourceProductionRecord.ProductionTypeID == 2)
                     && outputProductionRecord.ProductionTypeID == 2
                     && (sourceProductionRecord.StatusID == (int)Persistence.BusinessLogicEnums.Status.Active || sourceProductionRecord.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processing || sourceProductionRecord.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processed)
                 && (productionContent == null || (productionContent != null && (productionContent.ContentFieldID == 20 || productionContent.ContentFieldID == 23))) // ProdDistlledProofGal
                 && sourceProductionRecord.ProductionEndTime <= endDate
                 && outputProductionRecord.ProductionEndTime >= startDate
                 && outputProductionRecord.ProductionEndTime <= endDate
                 && sourceProductionRecord.Gauged == true
                 select new
                 {
                     reportingCategoryName = spiritType.ProductTypeName ?? string.Empty,
                     spiritTypeReportingId = (int?)spiritType.SpiritTypeReportingID ?? 0,
                     proofGal = (float?)productionContent.ContentValue ?? 0
                 }).DefaultIfEmpty();

            if (prodRes.First() != null)
            {
                foreach (var rec in prodRes)
                {
                    // Search for existing category with matching name
                    var category = storageReportBody.Find(x => x.SpiritTypeReportingID == rec.spiritTypeReportingId);

                    if (category == null)
                    {
                        var total = rec.proofGal;

                        if (total > 0)
                        {
                            // Add category to the list with given produced distilled batch ReportingCategoryName and update relevant rows
                            StorageReportCategory cat = new StorageReportCategory();
                            cat.SpiritTypeReportingID = rec.spiritTypeReportingId;
                            cat.CategoryName = rec.reportingCategoryName;
                            cat.r18_TransferredToProductionAccount += total;
                            storageReportBody.Add(cat);
                        }
                    }
                    else
                    {
                        category.r18_TransferredToProductionAccount += rec.proofGal;
                    }
                }
            }
        }

        /// <summary>
        /// 20. Destroyed
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="userId"></param>
        /// <param name="storageReportBody"></param>
        private void GetStorageReportDestroyed(DateTime startDate, DateTime endDate, int userId, ref List<StorageReportCategory> storageReportBody)
        {
            // Query production batches destroyed in Storage Account
            var prodDestroyed = (from destruction in db.Destruction
                                 join production in db.Production
                                    on destruction.RecordID equals production.ProductionID
                                 join distiller in db.AspNetUserToDistiller
                                    on production.DistillerID equals distiller.DistillerID
                                 join productionToSpiritType in db.ProductionToSpiritTypeReporting
                                    on production.ProductionID equals productionToSpiritType.ProductionID
                                 join spiritType in db.SpiritTypeReporting
                                    on productionToSpiritType.SpiritTypeReportingID equals spiritType.SpiritTypeReportingID
                                 where
                                    distiller.UserId == userId
                                    && production.ProductionTypeID == 2
                                    && production.StateID == (int)Persistence.BusinessLogicEnums.Status.Destroyed
                                    && production.StateID == (int)Persistence.BusinessLogicEnums.State.Distilled
                                    && production.Gauged == true
                                    && destruction.EndTime > startDate
                                    && destruction.EndTime < endDate
                                    && destruction.WorkflowType == "Production"
                                 select new
                                 {
                                     reportingCategoryName = spiritType.ProductTypeName ?? string.Empty,
                                     spiritTypeReportingId = (int?)spiritType.SpiritTypeReportingID ?? 0,
                                     proof = (float?)destruction.ProofGallons ?? 0
                                 }).DefaultIfEmpty();

            if (prodDestroyed.First() != null)
            {
                foreach (var rec in prodDestroyed)
                {
                    // Search for existing category with matching name
                    var category = storageReportBody.Find(x => x.SpiritTypeReportingID == rec.spiritTypeReportingId);

                    if (category == null)
                    {
                        var total = rec.proof;

                        if (total > 0)
                        {
                            // Add category to the list with given produced distilled batch ReportingCategoryName and update relevant rows
                            StorageReportCategory cat = new StorageReportCategory();
                            cat.SpiritTypeReportingID = rec.spiritTypeReportingId;
                            cat.CategoryName = rec.reportingCategoryName;
                            cat.r20_Destroyed += total;
                            storageReportBody.Add(cat);
                        }
                    }
                    else
                    {
                        category.r20_Destroyed += rec.proof;
                    }
                }
            }

            // Query purchase batches destroyed in Storage Account
            var purDestroyed = (from dest in db.Destruction
                                join pur in db.Purchase
                                    on dest.RecordID equals pur.PurchaseID
                                join dist in db.AspNetUserToDistiller
                                    on pur.DistillerID equals dist.DistillerID
                                join mDic in db.MaterialDict
                                    on pur.MaterialDictID equals mDic.MaterialDictID into mDic_join
                                from mDic in mDic_join.DefaultIfEmpty()
                                join uOm in db.UnitOfMeasurement
                                    on mDic.UnitOfMeasurementID equals uOm.UnitOfMeasurementID into uOm_join
                                from uOm in uOm_join.DefaultIfEmpty()
                                join pur_str in db.PurchaseToSpiritTypeReporting
                                    on pur.PurchaseID equals pur_str.PurchaseID
                                join spiritType in db.SpiritTypeReporting
                                    on pur_str.SpiritTypeReportingID equals spiritType.SpiritTypeReportingID
                                where
                                    dist.UserId == userId
                                    && (pur.PurchaseTypeID == 2 || pur.PurchaseTypeID == 3)
                                    && pur.StateID == (int)Persistence.BusinessLogicEnums.Status.Destroyed
                                    && (pur.StateID == (int)Persistence.BusinessLogicEnums.State.Fermented || pur.StateID == (int)Persistence.BusinessLogicEnums.State.Distilled)
                                    && dest.EndTime > startDate
                                    && dest.EndTime < endDate
                                    && dest.WorkflowType == "Purchase"
                                    && uOm.UnitOfMeasurementID != 2 // != "lb"
                                select new
                                {
                                    reportingCategoryName = spiritType.ProductTypeName ?? string.Empty,
                                    spiritTypeReportingId = (int?)spiritType.SpiritTypeReportingID ?? 0,
                                    proof = (float?)dest.ProofGallons ?? 0
                                }).DefaultIfEmpty();

            if (purDestroyed.First() != null)
            {
                foreach (var rec in purDestroyed)
                {
                    // Search for existing category with matching name
                    var category = storageReportBody.Find(x => x.SpiritTypeReportingID == rec.spiritTypeReportingId);

                    if (category == null)
                    {
                        var total = rec.proof;

                        if (total > 0)
                        {
                            // Add category to the list with given purchased distilled batch ReportingCategoryName and update relevant rows
                            StorageReportCategory cat = new StorageReportCategory();
                            cat.SpiritTypeReportingID = rec.spiritTypeReportingId;
                            cat.CategoryName = rec.reportingCategoryName;
                            cat.r20_Destroyed += total;
                            storageReportBody.Add(cat);
                        }
                    }
                    else
                    {
                        category.r20_Destroyed += rec.proof;
                    }
                }
            }
        }

        /// <summary>
        /// 23. On hand end of month
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="userId"></param>
        /// <param name="storageReportBody"></param>
        private void GetStorageOnHandEndOfMonth(DateTime startDate, DateTime endDate, int userId, ref List<StorageReportCategory> storageReportBody)
        {
            // On hand end of the month is the same as on hand first of the next month
            startDate = startDate.AddMonths(1);
            endDate = endDate.AddMonths(1);

            // Query fermented and distilled purchase records on hand end of the month
            var purchaseRecords =
                (from purchase in db.Purchase
                 join distiller in db.AspNetUserToDistiller on purchase.DistillerID equals distiller.DistillerID into distiller_join
                 from distiller in distiller_join.DefaultIfEmpty()
                 join mDic in db.MaterialDict on purchase.MaterialDictID equals mDic.MaterialDictID into mDic_join
                 from mDic in mDic_join.DefaultIfEmpty()
                 join uOm in db.UnitOfMeasurement on mDic.UnitOfMeasurementID equals uOm.UnitOfMeasurementID into uOm_join
                 from uOm in uOm_join.DefaultIfEmpty()
                 join dest in db.Destruction on purchase.PurchaseID equals dest.RecordID into dest_join
                 from dest in dest_join.DefaultIfEmpty()
                 join proof in db.Proof on purchase.ProofID equals proof.ProofID into proof_join
                 from proof in proof_join.DefaultIfEmpty()
                 join productionContent in db.ProductionContent on purchase.PurchaseID equals productionContent.RecordID into productionContent_join
                 from productionContent in productionContent_join.DefaultIfEmpty()
                 join production in db.Production on productionContent.ProductionID equals production.ProductionID into production_join
                 from production in production_join.DefaultIfEmpty()
                 join productionToSpiritType in db.PurchaseToSpiritTypeReporting on purchase.PurchaseID equals productionToSpiritType.PurchaseID into productionToSpiritType_join
                 from productionToSpiritType in productionToSpiritType_join.DefaultIfEmpty()
                 join str in db.SpiritTypeReporting on productionToSpiritType.SpiritTypeReportingID equals str.SpiritTypeReportingID into str_join
                 from str in str_join.DefaultIfEmpty()
                 where
                     distiller.UserId == userId
                     && (purchase.PurchaseTypeID == 2 || purchase.PurchaseTypeID == 3)
                     && purchase.PurchaseDate < startDate
                     && ((purchase.StatusID == (int)Persistence.BusinessLogicEnums.Status.Active || purchase.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processing || purchase.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processed) || (purchase.StateID == (int)Persistence.BusinessLogicEnums.Status.Destroyed && dest.EndTime > startDate && dest.EndTime < endDate))
                     && (productionContent == null || (productionContent != null && (productionContent.ContentFieldID == 16 || productionContent.ContentFieldID == 18)))
                     && uOm.UnitOfMeasurementID != 2 // != "lb"
                 select new
                 {
                     productionDate = (DateTime?)production.ProductionEndTime,
                     reportingCategoryName = str.ProductTypeName ?? string.Empty,
                     spiritTypeReportingId = (int?)str.SpiritTypeReportingID ?? 0,
                     proof = (float?)proof.Value ?? 0,
                     destroyedProof = (float?)dest.ProofGallons ?? 0,
                     productionContentProof = (float?)productionContent.ContentValue ?? 0
                 }).DefaultIfEmpty();

            if (purchaseRecords.First() != null)
            {
                foreach (var rec in purchaseRecords)
                {
                    // Search for existing category with matching name
                    var category = storageReportBody.Find(x => x.SpiritTypeReportingID == rec.spiritTypeReportingId);

                    if (category == null)
                    {
                        var total = rec.proof + rec.destroyedProof;

                        if (rec.productionDate != null && rec.productionContentProof > 0 && rec.productionDate >= startDate && rec.productionDate <= endDate)
                        {
                            total += rec.productionContentProof;
                        }

                        if (total > 0)
                        {
                            // Add category to the list with given produced distilled batch ReportingCategoryName and update relevant rows
                            StorageReportCategory cat = new StorageReportCategory();
                            cat.SpiritTypeReportingID = rec.spiritTypeReportingId;
                            cat.CategoryName = rec.reportingCategoryName;
                            cat.r23_OnHandEndOfMonth += total;
                            storageReportBody.Add(cat);
                        }
                    }
                    else
                    {
                        category.r23_OnHandEndOfMonth += rec.proof + rec.destroyedProof;

                        if (rec.productionDate != null && rec.productionContentProof > 0 && rec.productionDate >= startDate && rec.productionDate <= endDate)
                        {
                            category.r23_OnHandEndOfMonth += rec.productionContentProof;
                        }
                    }
                }
            }

            // Query distilled production records on hand end of the month
            var producedRecords =
                (from sourceProduction in db.Production
                 join distiller in db.AspNetUserToDistiller on sourceProduction.DistillerID equals distiller.DistillerID into distiller_join
                 from distiller in distiller_join.DefaultIfEmpty()
                 join dest in db.Destruction on sourceProduction.ProductionID equals dest.RecordID into dest_join
                 from dest in dest_join.DefaultIfEmpty()
                 join proof in db.Proof on sourceProduction.ProofID equals proof.ProofID into proof_join
                 from proof in proof_join.DefaultIfEmpty()
                 join productionContent in db.ProductionContent on sourceProduction.ProductionID equals productionContent.RecordID into productionContent_join
                 from productionContent in productionContent_join.DefaultIfEmpty()
                 join outputProduction in db.Production on productionContent.ProductionID equals outputProduction.ProductionID into outputProductionRecord_join
                 from outputProduction in outputProductionRecord_join.DefaultIfEmpty()
                 join productionToSpiritType in db.ProductionToSpiritTypeReporting on sourceProduction.ProductionID equals productionToSpiritType.ProductionID into productionToSpiritType_join
                 from productionToSpiritType in productionToSpiritType_join.DefaultIfEmpty()
                 join str in db.SpiritTypeReporting on productionToSpiritType.SpiritTypeReportingID equals str.SpiritTypeReportingID into str_join
                 from str in str_join.DefaultIfEmpty()
                 where
                     distiller.UserId == userId
                     && (sourceProduction.ProductionTypeID == 1 || sourceProduction.ProductionTypeID == 2)
                     && sourceProduction.ProductionEndTime < startDate
                     && sourceProduction.Gauged == true
                     && ((sourceProduction.StatusID == (int)Persistence.BusinessLogicEnums.Status.Active || sourceProduction.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processing || sourceProduction.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processed) || (sourceProduction.StateID == (int)Persistence.BusinessLogicEnums.Status.Destroyed && dest.EndTime > startDate && dest.EndTime < endDate))
                     && (productionContent == null || (productionContent != null && (productionContent.ContentFieldID == 20 || productionContent.ContentFieldID == 23)))
                 select new
                 {
                     productionDate = (DateTime?)outputProduction.ProductionEndTime,
                     reportingCategoryName = str.ProductTypeName ?? string.Empty,
                     spiritTypeReportingId = (int?)str.SpiritTypeReportingID ?? 0,
                     proof = (float?)proof.Value ?? 0,
                     destroyedProof = (float?)dest.ProofGallons ?? 0,
                     productionContentProof = (float?)productionContent.ContentValue ?? 0
                 }).DefaultIfEmpty();

            if (producedRecords.First() != null)
            {
                foreach (var rec in producedRecords)
                {
                    // Search for existing category with matching name
                    var category = storageReportBody.Find(x => x.SpiritTypeReportingID == rec.spiritTypeReportingId);

                    if (category == null)
                    {
                        var total = rec.proof + rec.destroyedProof;

                        if (rec.productionContentProof > 0 && rec.productionDate >= startDate && rec.productionDate <= endDate)
                        {
                            total += rec.productionContentProof;
                        }

                        if (total > 0)
                        {
                            // Add category to the list with given produced distilled batch ReportingCategoryName and update relevant rows
                            StorageReportCategory cat = new StorageReportCategory();
                            cat.SpiritTypeReportingID = rec.spiritTypeReportingId;
                            cat.CategoryName = rec.reportingCategoryName;
                            cat.r23_OnHandEndOfMonth += total;
                            storageReportBody.Add(cat);
                        }
                    }
                    else
                    {
                        category.r23_OnHandEndOfMonth += rec.proof + rec.destroyedProof;

                        if (rec.productionContentProof > 0 && rec.productionDate >= startDate && rec.productionDate <= endDate)
                        {
                            category.r23_OnHandEndOfMonth += rec.productionContentProof;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 24. TOTAL (Lines 7 through 23)
        /// </summary>
        /// <param name="storageReportBody"></param>
        private void ComputeLine24ForStorageReport(ref List<StorageReportCategory> storageReportBody)
        {
            foreach (var storage in storageReportBody)
            {
                storage.r24_Lines7Through23 = storage.r7_TaxPaid + storage.r17_TransferredToProcessingAccount + storage.r18_TransferredToProductionAccount
                    + +storage.r19_TransferredToOtherBondedPremises + storage.r20_Destroyed + storage.r22_OtherLosses + storage.r23_OnHandEndOfMonth;
            }
        }

        /// <summary>
        /// Report header
        /// </summary>
        /// <param name="distillerID"></param>
        /// <param name="startDate"></param>
        /// <returns></returns>
        private ReportHeader GetDistillerInfoForReportHeader(int distillerID, DateTime startDate)
        {
            try
            {
                ReportHeader header = new ReportHeader();

                var res =
                    (from distT in db.Distiller
                     join distDT in db.DistillerDetail on distT.DistillerID equals distDT.DistillerID
                     where distDT.DistillerID == distillerID
                     select new
                     {
                         DistillerName = distT.Name,
                         EIN = distDT.EIN,
                         DSP = distDT.DSP,
                         Address = distDT.StreetAddress + " " + distDT.City + " " + distDT.State + " " + distDT.Zip
                     }).FirstOrDefault();

                header.ProprietorName = res.DistillerName;
                header.EIN = res.EIN;
                header.DSP = res.DSP;
                header.PlantAddress = res.Address;
                header.ReportDate = startDate.ToString("Y");

                return header;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Query ProductionReportMaterialCategory table for list of Categories eg. Grain, Fruit, etc
        /// </summary>
        /// <returns></returns>
        public List<MaterialCategory> GetMaterialCategoryList()
        {
            List<MaterialCategory> materialCatList = new List<MaterialCategory>();
            try
            {
                var results = (from rec in db.ProductionReportMaterialCategory
                               select new
                               {
                                   rec.ProductionReportMaterialCategoryID,
                                   rec.MaterialCategoryName
                               });
                foreach (var res in results)
                {
                    MaterialCategory categoryInstance = new MaterialCategory();
                    categoryInstance.MaterialCategoryID = res.ProductionReportMaterialCategoryID;
                    categoryInstance.MaterialCategoryName = res.MaterialCategoryName;
                    materialCatList.Add(categoryInstance);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return materialCatList;
        }

        internal bool RemoveRecordsFromDBForUser(int userId)
        {
            bool retMthdExecResult = false;
            try
            {
                #region Production Tables 
                IEnumerable<ProductionToPurchase> queryProductionToPurchase =
                from prod2Purch in db.ProductionToPurchase
                join prod in db.Production on prod2Purch.ProductionID equals prod.ProductionID
                join us2Distills in db.AspNetUserToDistiller on new { DistillerID = prod.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId
                select prod2Purch;

                db.ProductionToPurchase.RemoveRange(queryProductionToPurchase);

                IEnumerable<ProductionToSpiritTypeReporting> queryProd2SpiritTypeReporting =
                from prod2SpirRepo in db.ProductionToSpiritTypeReporting
                join prod in db.Production on prod2SpirRepo.ProductionID equals prod.ProductionID
                join us2Distills in db.AspNetUserToDistiller on new { DistillerID = prod.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId
                select prod2SpirRepo;

                db.ProductionToSpiritTypeReporting.RemoveRange(queryProd2SpiritTypeReporting);

                IEnumerable<Weight> queryWeight =
                from vbw in db.Weight
                join prod in db.Production on vbw.WeightID equals prod.WeightID
                join us2Distills in db.AspNetUserToDistiller on new { DistillerID = prod.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId
                select vbw;

                db.Weight.RemoveRange(queryWeight);

                IEnumerable<Volume> queryVolume =
                from volume in db.Volume
                join prod in db.Production on volume.VolumeID equals prod.VolumeID
                join us2Distills in db.AspNetUserToDistiller on new { DistillerID = prod.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId
                select volume;

                db.Volume.RemoveRange(queryVolume);

                IEnumerable<StorageToRecord> queryStorag2Record =
               from sto2Rec in db.StorageToRecord
               join prod in db.Production on sto2Rec.RecordId equals prod.ProductionID
               join us2Distills in db.AspNetUserToDistiller on new { DistillerID = prod.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
               from us2Distills in us2Distills_join.DefaultIfEmpty()
               where us2Distills.UserId == userId &&
               sto2Rec.TableIdentifier == "prod"
               select sto2Rec;

                db.StorageToRecord.RemoveRange(queryStorag2Record);

                IEnumerable<Alcohol> queryAlcohol =
                from alc in db.Alcohol
                join prod in db.Production on alc.AlcoholID equals prod.AlcoholID
                join us2Distills in db.AspNetUserToDistiller on new { DistillerID = prod.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId
                select alc;

                db.Alcohol.RemoveRange(queryAlcohol);

                IEnumerable<Proof> queryProof =
                from proof in db.Proof
                join prod in db.Production on proof.ProofID equals prod.ProofID
                join us2Distills in db.AspNetUserToDistiller on new { DistillerID = prod.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId
                select proof;

                db.Proof.RemoveRange(queryProof);

                IEnumerable<ProductionToSpirit> queryProductionToSpirit =
                from prod2Spirit in db.ProductionToSpirit
                join prod in db.Production on prod2Spirit.ProductionID equals prod.ProductionID
                join us2Distills in db.AspNetUserToDistiller on new { DistillerID = prod.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId
                select prod2Spirit;

                db.ProductionToSpirit.RemoveRange(queryProductionToSpirit);

                IEnumerable<ProductionToSpiritCut> queryProductionToSpiritCut =
                from prod2SpiritCut in db.ProductionToSpiritCut
                join prod in db.Production on prod2SpiritCut.ProductionID equals prod.ProductionID
                join us2Distills in db.AspNetUserToDistiller on new { DistillerID = prod.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId
                select prod2SpiritCut;

                db.ProductionToSpiritCut.RemoveRange(queryProductionToSpiritCut);

                IEnumerable<BlendedComponent> queryBlendedComponent =
                from blendComponent in db.BlendedComponent
                join prod in db.Production on blendComponent.ProductionID equals prod.ProductionID
                join us2Distills in db.AspNetUserToDistiller on new { DistillerID = prod.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId
                select blendComponent;

                db.BlendedComponent.RemoveRange(queryBlendedComponent);

                IEnumerable<BottlingInfo> queryBottlingInfo =
                from bottlingInfo in db.BottlingInfo
                join prod in db.Production on bottlingInfo.ProductionID equals prod.ProductionID
                join us2Distills in db.AspNetUserToDistiller on new { DistillerID = prod.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId
                select bottlingInfo;

                db.BottlingInfo.RemoveRange(queryBottlingInfo);

                IEnumerable<Production4Reporting> queryProduction4Reporting =
                from prod4Reporting in db.Production4Reporting
                join prod in db.Production on prod4Reporting.ProductionID equals prod.ProductionID
                join us2Distills in db.AspNetUserToDistiller on new { DistillerID = prod.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId
                select prod4Reporting;

                db.Production4Reporting.RemoveRange(queryProduction4Reporting);

                IEnumerable<ProductionContent> queryProductionContent =
                from prodCont in db.ProductionContent
                join prod in db.Production on prodCont.ProductionID equals prod.ProductionID
                join us2Distills in db.AspNetUserToDistiller on new { DistillerID = prod.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId
                select prodCont;

                db.ProductionContent.RemoveRange(queryProductionContent);

                IEnumerable<GainLoss> queryGainLoss =
                from gainloss in db.GainLoss
                join prod in db.Production on new { BottledRecordId = gainloss.BottledRecordId } equals new { BottledRecordId = prod.ProductionID }
                join us2Distills in db.AspNetUserToDistiller on new { DistillerID = prod.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId
                select gainloss;

                db.GainLoss.RemoveRange(queryGainLoss);

                IEnumerable<TaxWithdrawn> queryTaxWithdrawn =
                from taxW in db.TaxWithdrawn
                join prod in db.Production on taxW.ProductionID equals prod.ProductionID
                join us2Distills in db.AspNetUserToDistiller on new { DistillerID = prod.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId
                select taxW;

                db.TaxWithdrawn.RemoveRange(queryTaxWithdrawn);

                IEnumerable<Production> queryProduction =
                from Production in db.Production
                join us2Distills in db.AspNetUserToDistiller on new { DistillerID = Production.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId
                select Production;

                db.Production.RemoveRange(queryProduction);

                #endregion // end of production tables deletion

                #region Purchase Tables
                IEnumerable<StorageToRecord> queryStorageToRecord =
                from sto2Rec in db.StorageToRecord
                join purch in db.Purchase on new { RecordId = sto2Rec.RecordId } equals new { RecordId = purch.PurchaseID }
                join us2Distills in db.AspNetUserToDistiller on new { DistillerID = purch.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId &&
                  sto2Rec.TableIdentifier == "pur"
                select sto2Rec;

                db.StorageToRecord.RemoveRange(queryStorageToRecord);

                IEnumerable<ProductionToPurchase> queryProductionToPurchase4Purchase =
                from prod2Purch in db.ProductionToPurchase
                join purch in db.Purchase on prod2Purch.ProductionID equals purch.PurchaseID
                join us2Distills in db.AspNetUserToDistiller on new { DistillerID = purch.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId
                select prod2Purch;
                db.ProductionToPurchase.RemoveRange(queryProductionToPurchase4Purchase);

                IEnumerable<Weight> queryWeight4Purchase =
               from vbw in db.Weight
               join purch in db.Purchase on vbw.WeightID equals purch.WeightID
               join us2Distills in db.AspNetUserToDistiller on new { DistillerID = purch.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
               from us2Distills in us2Distills_join.DefaultIfEmpty()
               where us2Distills.UserId == userId
               select vbw;

                db.Weight.RemoveRange(queryWeight4Purchase);

                IEnumerable<Volume> queryVolume4Purchase =
                from volume in db.Volume
                join purch in db.Production on volume.VolumeID equals purch.VolumeID
                join us2Distills in db.AspNetUserToDistiller on new { DistillerID = purch.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId
                select volume;

                db.Volume.RemoveRange(queryVolume4Purchase);

                IEnumerable<Alcohol> queryAlcohol4Purchase =
                from alc in db.Alcohol
                join purch in db.Purchase on alc.AlcoholID equals purch.AlcoholID
                join us2Distills in db.AspNetUserToDistiller on new { DistillerID = purch.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId
                select alc;

                db.Alcohol.RemoveRange(queryAlcohol4Purchase);

                IEnumerable<Proof> queryProof4Purchase =
                from proof in db.Proof
                join purch in db.Purchase on proof.ProofID equals purch.ProofID
                join us2Distills in db.AspNetUserToDistiller on new { DistillerID = purch.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId
                select proof;

                db.Proof.RemoveRange(queryProof4Purchase);

                IEnumerable<Purchase4Reporting> queryPurchase4Reporting =
                from purch4Reporting in db.Purchase4Reporting
                join purch in db.Purchase on purch4Reporting.PurchaseID equals purch.PurchaseID
                join us2Distills in db.AspNetUserToDistiller on new { DistillerID = purch.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId
                select purch4Reporting;

                db.Purchase4Reporting.RemoveRange(queryPurchase4Reporting);

                IEnumerable<Purchase> queryPurchase =
                from purch in db.Purchase
                join us2Distills in db.AspNetUserToDistiller on new { DistillerID = purch.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId
                select purch;

                db.Purchase.RemoveRange(queryPurchase);
                #endregion

                db.SaveChanges();

                retMthdExecResult = true;
            }
            catch (Exception e)
            {
                retMthdExecResult = false;
                throw e;
            }
            return retMthdExecResult;
        }
        #endregion
    }

    namespace WebApp.ReportDTO { }

}