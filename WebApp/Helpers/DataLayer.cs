using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp.Models;
using System.Web.Mvc;
using WebApp.Helpers;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Collections;
using Microsoft.AspNet.Identity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;

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

    public enum State // as in State of the Raw Material
    {

    }

    public enum RecordType
    {
        Purchase = 1,
        Production
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
                  (purch.StatusID == 1 ||
                  purch.StatusID == 2) &&
                  purch.StateID == 1 &&
                  distiller.UserId == userId
                select new
                {
                    PurchaseID = ((System.Int32?)purch.PurchaseID ?? (System.Int32?)0),
                    PurchaseBatchName = purch.PurchaseName ?? "",
                    StatusID = ((System.Int32?)purch.StatusID ?? (System.Int32?)0),
                    RawMaterialName = matDic.Name ?? "",
                    MaterialDictID = ((System.Int32?)matDic.MaterialDictID ?? (System.Int32?)0),
                    Quantity = ((System.Single?)quant.Value ?? (System.Single?)0),
                    VolumeByWeight = ((System.Single?)vbw.Value ?? (System.Single?)0),
                    BurningDownMethod = purch.BurningDownMethod ?? null,
                };
            
            if (fermentables != null)
            {
                foreach (var i in fermentables)
                {
                    PurMatObject obj = new PurMatObject();
                    obj.PurchaseBatchName = i.PurchaseBatchName;
                    obj.PurchaseId = (int)i.PurchaseID;
                    obj.RawMaterialId = (int)i.MaterialDictID;
                    obj.MaterialName = i.RawMaterialName;
                    obj.QtyGal = (float)i.Quantity;
                    obj.VBW = (float)i.VolumeByWeight;
                    obj.BurningDownMethod = i.BurningDownMethod;
                    rawMList.Add(obj);
                }
            }

            return rawMList;
        }

        internal ProcessingReportingObject GetProcessingReportData(DateTime startOfReporting, DateTime endOfReporting, int userId)
        {
            ProcessingReportingObject procRepObj = new ProcessingReportingObject();
            ProcessReportingPart1 procRepP1 = new ProcessReportingPart1();
            ProcessReportingPart2 procRepP2 = new ProcessReportingPart2();
            List<ProcessReportingPart4> procRepP4L = new List<ProcessReportingPart4>();

            List<ProductionReportHelper> tempRepObjList = new List<ProductionReportHelper>();

            // get distiller information for header report
            int distillerID = GetDistillerId(userId);
            procRepObj.Header = GetDistillerInfoForReportHeader(distillerID, startOfReporting);

            // Processing Report Part 1 Section
            procRepP1.BulkIngredients = "spirit";
            
            // 1(c) previous month
            var onHands1stMoC =
                (from prod in
                    (from prod in db.Production
                     join proof in db.Proof on prod.ProofID equals proof.ProofID into proof_join
                     from proof in proof_join.DefaultIfEmpty()
                     join distillers in db.AspNetUserToDistiller on prod.DistillerID equals distillers.DistillerID into distillers_join
                     from distillers in distillers_join.DefaultIfEmpty()
                     where
                       distillers.UserId == userId &&
                       prod.ProductionTypeID == 3 &&
                       prod.Gauged == true &&
                       prod.ProductionEndTime < startOfReporting &&
                       (prod.StatusID == 1 ||
                        prod.StatusID == 2 ||
                        prod.StateID == 4)
                     select new
                     {
                         Value = (System.Single?)proof.Value ?? (System.Single?) 0,
                         Dummy = "x"
                     })
                group prod by new { prod.Dummy } into g
                select new
                {
                    OnHandFirstOfMonthBulk = g.Sum(p => p.Value)
                }).FirstOrDefault();

            if (onHands1stMoC != null)
            {
                procRepP1.OnHandFirstofMonth = (float)onHands1stMoC.OnHandFirstOfMonthBulk;
            }

            // 2(c) current month received bulk
            var recBulk =
                (from prod in
                    (from prod in db.Production
                     join proof in db.Proof on prod.ProofID equals proof.ProofID into proof_join
                     from proof in proof_join.DefaultIfEmpty()
                     join distillers in db.AspNetUserToDistiller on prod.DistillerID equals distillers.DistillerID into distillers_join
                     from distillers in distillers_join.DefaultIfEmpty()
                     where
                       distillers.UserId == userId &&
                       prod.Gauged == true &&
                       prod.ProductionTypeID == 3 &&
                       prod.ProductionEndTime >= startOfReporting &&
                       prod.ProductionEndTime <= endOfReporting &&
                       (prod.StatusID == 1 ||
                       prod.StatusID == 2 ||
                       prod.StateID == 4)
                     select new
                     {
                         Value = (System.Single?)proof.Value ?? (System.Single?)0,
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

            // 9 (c) Bottled or Packaged
            var bottledPackaged =
                (from prod in
                    (from prod in db.Production
                     join proof in db.Proof on prod.ProofID equals proof.ProofID into proof_join
                     from proof in proof_join.DefaultIfEmpty()
                     join distillers in db.AspNetUserToDistiller on prod.DistillerID equals distillers.DistillerID into distillers_join
                     from distillers in distillers_join.DefaultIfEmpty()
                     where
                       distillers.UserId == userId  &&
                       prod.ProductionTypeID == 4 &&
                       prod.Gauged == true &&
                       prod.ProductionEndTime >= startOfReporting &&
                       prod.ProductionEndTime <= endOfReporting &&
                       (prod.StatusID == 1 ||
                       prod.StatusID == 2 ||
                       prod.StateID == 5)
                     select new
                     {
                         Value = (System.Single?)proof.Value ?? (System.Single?)0,
                         Dummy = "x"
                     })
                group prod by new { prod.Dummy } into g
                select new
                {
                    BottledPackagedBulk = g.Sum(p => p.Value)
                }).FirstOrDefault();

            if(bottledPackaged != null)
            {
                procRepP1.AmtBottledPackaged = (float)bottledPackaged.BottledPackagedBulk;
            }

            // 25(c) On hand end of month
            procRepP1.OnHandEndofMonth = (float)procRepP1.Recd4Process - procRepP1.AmtBottledPackaged;

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
                       (prod.StatusID == 1 ||
                       prod.StatusID == 2 ||
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
                     join proof in db.Proof on prod.ProofID equals proof.ProofID into proof_join
                     from proof in proof_join.DefaultIfEmpty()
                     join distillers in db.AspNetUserToDistiller on prod.DistillerID equals distillers.DistillerID into distillers_join
                     from distillers in distillers_join.DefaultIfEmpty()
                     where
                       distillers.UserId == userId &&
                       prod.ProductionTypeID == 4 &&
                       prod.Gauged == true &&
                       prod.ProductionEndTime >= startOfReporting &&
                       prod.ProductionEndTime <= endOfReporting &&
                       (prod.StatusID == 1 ||
                       prod.StatusID == 2 ||
                       prod.StateID == 5)
                     select new
                     {
                         Value = (System.Single?)proof.Value ?? (System.Single?)0,
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

            // 28(b) On hand End of Month
            var onHandEndOfMonthP2 =
             (from prod in
                    (from prod in db.Production
                     join proof in db.Proof on prod.ProofID equals proof.ProofID into proof_join
                     from proof in proof_join.DefaultIfEmpty()
                     join distillers in db.AspNetUserToDistiller on prod.DistillerID equals distillers.DistillerID into distillers_join
                     from distillers in distillers_join.DefaultIfEmpty()
                     where
                       distillers.UserId == 1 &&
                       prod.ProductionTypeID == 4 &&
                       prod.ProductionEndTime <= endOfReporting &&
                       prod.Gauged == true &&
                       (prod.StatusID == 1 ||
                       prod.StatusID == 2 ||
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

            if(taxWithdrawn != null)
            {
                foreach(var i in taxWithdrawn)
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
                  join spiritTypeRep in db.SpiritTypeReporting on prod2SpiritType.SpiritTypeReportingID equals spiritTypeRep.SpiritTypeReportingID into spiritTypeRep_join
                  from spiritTypeRep in spiritTypeRep_join.DefaultIfEmpty()
                  join prod2Purch in db.ProductionToPurchase on prod.ProductionID equals prod2Purch.ProductionID into prod2Purch_join
                  from prod2Purch in prod2Purch_join.DefaultIfEmpty()
                  join purch4Reprt in db.Purchase4Reporting on prod2Purch.PurchaseID equals purch4Reprt.PurchaseID into purch4Reprt_join
                  from purch4Reprt in purch4Reprt_join.DefaultIfEmpty()
                  where
                    distillers.UserId == userId &&
                    prod.Gauged == true &&
                    (prod.StatusID == 1 ||
                    prod.StatusID == 2) &&
                    (new int[] { 4, 5 }).Contains(prod.StateID) &&
                    prod.ProductionEndTime >= startOfReporting &&
                    prod.ProductionEndTime <= endOfReporting
                  select new
                  {
                      prod.StateID,
                      ProductionID = (int?)prod.ProductionID,
                      SpiritTypeName = spiritTypeRep.ProductTypeName ?? "",
                      Redistilled = (bool?)prodReport.Redistilled,
                      Weight = (System.Single?)prodReport.Weight ?? (System.Single?)0,
                      Volume = (System.Single?)prodReport.Volume ?? (System.Single?)0,
                      Alcohol = (System.Single?)prodReport.Alcohol ?? (System.Single?)0,
                      Proof = (System.Single?)prodReport.Proof ?? (System.Single?)0,
                      SpiritTypeReportingID = (int?)spiritTypeRep.SpiritTypeReportingID ?? (int?)0,
                  }).Distinct();

                // fill in temporary query results object so we can massage the data for further processing
                foreach (var l in ress)
                {
                    ProductionReportHelper tempRepObj = new ProductionReportHelper();
                    tempRepObj.StateID = l.StateID;
                    tempRepObj.ProductionID = (int)l.ProductionID;
                    tempRepObj.SpiritTypeReportName = l.SpiritTypeName;
                    tempRepObj.Redistilled = (bool)l.Redistilled;
                    tempRepObj.Weight = (float)l.Weight;
                    tempRepObj.Volume = (float)l.Volume;
                    tempRepObj.Alcohol = (float)l.Alcohol;
                    tempRepObj.Proof = (float)l.Proof;
                    tempRepObj.SpiritTypeReportingID = (int)l.SpiritTypeReportingID;

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
                                                 }).FirstOrDefault();
                                            if (t != null)
                                            {
                                                if (t.SpiritTypeReportingID != null || t.SpiritTypeReportingID != 0)
                                                {
                                                    var record = tempRepObjList.Find(x => x.ProductionID == i.ProductionID);
                                                    record.SpiritTypeReportingID = (int)t.SpiritTypeReportingID;
                                                    record.SpiritTypeReportName = t.SpiritTypeReportingName;
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
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                throw;
                            }
                        }
                    }

                    foreach (var rec in tempRepObjList)
                    {
                        var spiritType = procRepP4L.Find(x => x.StateID == rec.StateID);

                        if (spiritType == null)
                        {
                            ProcessReportingPart4 part4Obj = new ProcessReportingPart4();
                            part4Obj.SpiritCatName = rec.SpiritTypeReportName;
                            part4Obj.SpiritTypeReportingID = (int)rec.SpiritTypeReportingID;

                            if( rec.StateID == 4)
                            {
                                part4Obj.ProcessingSpirits = "bulkSpiritDumped";
                            }
                            else if(rec.StateID == 5)
                            {
                                part4Obj.ProcessingSpirits = "bottled";
                            }

                            if(rec.SpiritTypeReportName == "WhiskyUnder160")
                            {
                                part4Obj.DomesticWhiskey160Under = (float)rec.Proof;
                            }
                            else if (rec.SpiritTypeReportName == "WhiskyOver160")
                            {
                                part4Obj.DomesticWhiskeyOver160 = (float)rec.Proof;
                            }
                            else if (rec.SpiritTypeReportName == "BrandyUnder170")
                            {
                                part4Obj.Brandy170Under = (float)rec.Proof;
                            }
                            else if (rec.SpiritTypeReportName == "BrandyOver170")
                            {
                                part4Obj.BrandyOver170 = (float)rec.Proof;
                            }
                            else if (rec.SpiritTypeReportName == "Rum")
                            {
                                // need clarification about RUM reporting: Task 1427: Implement ability to report RUM in processing report Part 4
                            }
                            else if (rec.SpiritTypeReportName == "Gin")
                            {
                                part4Obj.Gin = (float)rec.Proof;
                            }
                            else if (rec.SpiritTypeReportName == "Vodka")
                            {
                                part4Obj.Vodka = (float)rec.Proof;
                            }

                            // todo:
                            // Task 1428: ALCOHOL AND NEUTRAL SPIRITS (Other than vodka) - line 49 in Processing Report
                            // Task 1429: BLENDED STRAIGHT WHISKEY5 processing report Part 4 - line 50
                            // Task 1430: BLENDED WHISKEY: processing report Part 4 - line 51
                            // Task 1431: BLENDED LIGHT WHISKEY: processing report Part 4 - line 52
                            // Task 1432: ANY OTHER BLENDS OF 100% WHISKEY: processing report Part 4 - line 53
                            // Task 1433: IMPORTED WHISKEY: processing report Part 4 - line 54
                            // Task 1434: CORDIALS, LIQUEURS, AND SPECIALTIES: processing report Part 4 - line 63
                            // Task 1435: COCKTAILS AND MIXED DRINKS: processing report Part 4 - line 64
                            // Task 1436: TEQUILA: processing report Part 4 - line 65
                            part4Obj.StateID = rec.StateID;
                            procRepP4L.Add(part4Obj);
                        }
                        else
                        {
                            if (rec.SpiritTypeReportName == "WhiskyUnder160")
                            {
                                spiritType.DomesticWhiskey160Under += (float)rec.Proof;
                            }
                            else if (rec.SpiritTypeReportName == "WhiskyOver160")
                            {
                                spiritType.DomesticWhiskeyOver160 += (float)rec.Proof;
                            }
                            else if (rec.SpiritTypeReportName == "BrandyUnder170")
                            {
                                spiritType.Brandy170Under += (float)rec.Proof;
                            }
                            else if (rec.SpiritTypeReportName == "BrandyOver170")
                            {
                                spiritType.BrandyOver170 += (float)rec.Proof;
                            }
                            else if (rec.SpiritTypeReportName == "Rum")
                            {
                                // need clarification about RUM reporting: Task 1427: Implement ability to report RUM in processing report Part 4
                            }
                            else if (rec.SpiritTypeReportName == "Gin")
                            {
                                spiritType.Gin += (float)rec.Proof;
                            }
                            else if (rec.SpiritTypeReportName == "Vodka")
                            {
                                spiritType.Vodka += (float)rec.Proof;
                            }

                            // todo:
                            // Task 1428: ALCOHOL AND NEUTRAL SPIRITS (Other than vodka) - line 49 in Processing Report
                            // Task 1429: BLENDED STRAIGHT WHISKEY5 processing report Part 4 - line 50
                            // Task 1430: BLENDED WHISKEY: processing report Part 4 - line 51
                            // Task 1431: BLENDED LIGHT WHISKEY: processing report Part 4 - line 52
                            // Task 1432: ANY OTHER BLENDS OF 100% WHISKEY: processing report Part 4 - line 53
                            // Task 1433: IMPORTED WHISKEY: processing report Part 4 - line 54
                            // Task 1434: CORDIALS, LIQUEURS, AND SPECIALTIES: processing report Part 4 - line 63
                            // Task 1435: COCKTAILS AND MIXED DRINKS: processing report Part 4 - line 64
                            // Task 1436: TEQUILA: processing report Part 4 - line 65
                        }
                    }
                }
            }
            catch(Exception e)
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

                    // Widrawn For Tax update:
                    TaxWithdrawn taxes = new TaxWithdrawn();
                    taxes.DateOfSale = pObj.WithdrawalDate;
                    taxes.DateRecorded = DateTime.Today;
                    taxes.ProductionID = pObj.ProductionId;
                    taxes.Value = pObj.TaxedProof;

                    db.TaxWithdrawn.Add(taxes);
                    db.SaveChanges();

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
                 (prod.StatusID == 1 ||
                 prod.StatusID == 2) &&
                 prod.StateID == 4 &&
                 distiller.UserId == userId
               select new
                {
                   ProductionName = prod.ProductionName,
                   ProductionID = ((System.Int32?)prod.ProductionID ?? (System.Int32?)0),
                   StatusName = status.Name,
                   StateName = state.Name,
                   Quantity = ((System.Single?)quants.Value ?? (System.Single?)0),
                   VolumeByWeight = ((System.Single?)VBW.Value ?? (System.Single?)0),
                   Alcohol = ((System.Single?)alc.Value ?? (System.Single?)0),
                   Proof = ((System.Single?)proof.Value ?? (System.Single?)0),
                   SpiritName = (spi.Name ?? ""),
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
                    pobj.Quantity = (float)rec.Quantity;
                    pobj.VolumeByWeight = (float)rec.VolumeByWeight;
                    pobj.AlcoholContent = (float)rec.Alcohol;
                    pobj.ProofGallon = (float)rec.Proof;
                    pobj.SpiritId = (int)rec.SpiritID;
                    pobj.SpiritName = rec.SpiritName;

                    bList.Add(pobj);
                }
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

                    if (purchaseObject.PurchaseType == "Distilled")
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
                        string storagesString = "";
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

                try
                {
                    SavePurchaseHistory(purchaseObject, userId);
                }
                catch(Exception e)
                {
                    throw;
                }
            }
            catch (Exception e)
            {
                throw;
            }

            return retMthdExecResult;
        }

        internal bool DeletePurchase(PurchaseObject purchaseObject, int userId)
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

                        if(purch4Rep != null)
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
                        (pur.StatusID == 1 || pur.StatusID == 2) &&
                        (pur.StateID == 2 || pur.StateID == 3) &&
                        distiller.UserId == userId
                    select new
                    {
                        PurchaseID = ((System.Int32?)pur.PurchaseID ?? (System.Int32?)0),
                        PurchaseBatchName = pur.PurchaseName ?? "",
                        StatusID = ((System.Int32?)pur.StatusID ?? (System.Int32?)0),
                        Quantity = ((System.Single?)qty.Value ?? (System.Single?)0),
                        AlcoholContent = ((System.Single?)alc.Value ?? (System.Single?)0),
                        VolumeByWeight = ((System.Single?)vbw.Value ?? (System.Single?)0),
                        BurningDownMethod = pur.BurningDownMethod ?? null
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
                        (prod.StatusID == 1 || prod.StatusID == 2) &&
                        (prod.StateID == 2 || prod.StateID == 3) &&
                        distiller.UserId == userId
                    select new
                    {
                        ProductionID = ((System.Int32?)prod.ProductionID ?? (System.Int32?)0),
                        ProductionBatchName = prod.ProductionName ?? "",
                        StatusID = ((System.Int32?)prod.StatusID ?? (System.Int32?)0),
                        Quantity = ((System.Single?)qty.Value ?? (System.Single?)0),
                        VolumeByWeight = ((System.Single?)vbw.Value ?? (System.Single?)0),
                        AlcoholContent = ((System.Single?)alc.Value ?? (System.Single?)0),
                        BurningDownMethod = prod.BurningDownMethod ?? null
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
                   (purch.StatusID == 1 ||
                   purch.StatusID == 2) &&
                   purch.StateID == 3 &&
                   distillers.UserId == userId
                  select new
                  {
                      PurchaseID = ((System.Int32?)purch.PurchaseID ?? (System.Int32?)0),
                      PurchaseBatchName = purch.PurchaseName ?? "",
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
                      (prod.StatusID == 1 ||
                      prod.StatusID == 2) &&
                      prod.StateID == 3 &&
                      distillers.UserId == userId
                    select new
                    {
                        ProductionID = ((System.Int32?)prod.ProductionID ?? (System.Int32?)0),
                        ProductionName = prod.ProductionName ?? "",
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
                Name = Mats.Name ?? "",
                UnitOfMeasurementID = (System.Int32?)Mats.UnitOfMeasurementID ?? (System.Int32?)0,
                Note = Mats.Note ?? "",
                UnitName = units.Name ?? ""
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
                    Name = matDict.Name ?? "",
                    UnitOfMeasurementID = (System.Int32?)matDict.UnitOfMeasurementID ?? (System.Int32?)0,
                    Note = matDict.Note ?? "",
                    UnitName = unit.Name ?? ""
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
            catch(Exception e)
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
        /// <returns>bool</returns>
        public bool CreateSpirit(int userId, SpiritObject spiritObject)
        {
            //define method execution return value to be false by default
            var retMthdExecResult = false;

            if (spiritObject != null)
            {
                try
                {
                    Spirit tbl = new Spirit();
                    tbl.Name = spiritObject.SpiritName;
                    tbl.DistillerID = GetDistillerId(userId);
                    if (spiritObject.Note != "" && spiritObject.Note != null)
                    {
                        tbl.Note = spiritObject.Note;
                    } 
                    db.Spirit.Add(tbl);
                    db.SaveChanges();
                    retMthdExecResult = true;
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Failed to create Spirit Record : " + e);
                }
            }
            else
            {
                //grisha -todo: we have to figure a way to fail such that we send some feedback to the user
                Debug.WriteLine("Failed to create Spirit Record because the te object came in empty from the client");
                retMthdExecResult =  false;
            }

            return retMthdExecResult;
        }

        /// <summary>
        /// CreateVendor Method inserts new record in Vendor table and also updates Note table if there is a note
        /// </summary>
        /// <param name="vendorObject"></param>
        /// <param name="userId"></param>
        /// <returns>bool</returns>
        public bool CreateVendor(int userId, VendorObject vendorObject)
        {
            //define method execution return value to be false by default
            var retMthdExecResult = false;
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
                    if (vendorObject.Note != "" && vendorObject.Note != null)
                    {
                        tbl1.Note = vendorObject.Note;
                    }
                    tbl1.VendorID = tbl.VendorID;
                    db.VendorDetail.Add(tbl1);
                    db.SaveChanges();
                    retMthdExecResult = true;
                }
                catch(Exception e)
                {
                    Debug.WriteLine("Failed to create Vendor Record : " + e);
                }
            }
            else
            {
                //grisha -todo: we have to figure a way to fail such that we send some feedback to the user
                Debug.WriteLine("Failed to create Vendo Record because the te object came in empty from the client");
                retMthdExecResult = false;
            }

            return retMthdExecResult;
        }

        /// <summary>
        /// CreateStorage method inserts a new record in Storage table and a note if such exists
        /// </summary>
        /// <param name="storageObject"></param>
        /// <param name="userId"></param>
        /// <returns>bool</returns>
        public bool CreateStorage(int userId, StorageObject storageObject)
        {
            //define method execution return value to be false by default
            var retMthdExecResult = false;
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
                    if (storageObject.Note != "" && storageObject.Note != null)
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
                    retMthdExecResult = true;
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Failed to Create Storage Record: " + e);
                    retMthdExecResult = false;
                }
            }

            return retMthdExecResult;
        }

        /// <summary>
        /// CreateRawMaterial creates new record in Raw Materials table, inserts/updates Note table and inserts/updates 
        /// </summary>
        /// <param name="rawMObject"></param>
        /// <param name="userId"></param>
        /// <returns>bool</returns>
        public bool CreateRawMaterial(int userId, RawMaterialObject rawMObject)
        {
            //define method execution return value to be false by default
            var retMthdExecResult = false;
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

                    if (rawMObject.Note != "" && rawMObject.Note != null)
                    {
                        matDict.Note = rawMObject.Note;
                    }

                    db.MaterialDict.Add(matDict);
                    db.SaveChanges();

                    materialDictID = matDict.MaterialDictID;

                    // build relationships between given raw material and purchase material types
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
                        try
                        {
                            MaterialDict2MaterialCategory md2mc = new MaterialDict2MaterialCategory();
                            md2mc.MaterialDictID = materialDictID;
                            md2mc.ProductionReportMaterialCategoryID = rawMObject.MaterialCategoryID;
                            db.MaterialDict2MaterialCategory.Add(md2mc);
                            db.SaveChanges();
                            retMthdExecResult = true;
                        }
                        catch(Exception e)
                        {
                            throw;
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
                        try
                        {
                            MaterialDict2MaterialCategory md2mc = new MaterialDict2MaterialCategory();
                            md2mc.MaterialDictID = materialDictID;
                            md2mc.ProductionReportMaterialCategoryID = rawMObject.MaterialCategoryID;
                            db.MaterialDict2MaterialCategory.Add(md2mc);
                            db.SaveChanges();
                            retMthdExecResult = true;
                        }
                        catch (Exception e)
                        {
                            throw;
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
                    Debug.WriteLine("Failed to Create Raw Material Record: " + e);
                    return retMthdExecResult;
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

                    if (item.Note != spiritObject.Note )
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

                    if (vendorItem.Name != vendorObject.VendorName || vendorObject.Note != "")
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

                    if(ress != null)
                    {
                        if(ress.Name != rawMObject.RawMaterialName)
                        {
                            ress.Name = rawMObject.RawMaterialName;
                        }

                        if(ress.Note != rawMObject.Note)
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
                    if(res != null)
                    {
                        foreach(var i in res)
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
                        Name = spirit.Name ?? "",
                        Note = spirit.Note ?? ""
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
                        Note = (vendDetails.Note ?? "")
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
                        Name = storage.Name ?? "",
                        Capacity = (System.Single?)storage.Capacity ?? (System.Single?)0,
                        SerialNumber = storage.SerialNumber ?? "",
                        Note = storage.Note ?? ""
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
        /// <returns>bool</returns>
        public bool CreatePurchase(PurchaseObject purchaseObject, int userId)
        {
            var retMthdExecResult = false;

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

                if(pTypes != null)
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
                if ((pTypes.PurchaseTypeID == 2 || pTypes.PurchaseTypeID == 3) && purchaseObject?.SpiritTypeReportingID != null)
                {
                    PurchaseToSpiritTypeReporting pstr = new PurchaseToSpiritTypeReporting();
                    pstr.PurchaseID = purchT.PurchaseID;
                    pstr.SpiritTypeReportingID = purchaseObject.SpiritTypeReportingID;
                    db.PurchaseToSpiritTypeReporting.Add(pstr);
                    db.SaveChanges();
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

                if(purchT.PurchaseTypeID == 3)
                {
                    try
                    {
                        // save purchase distil data and quantities into Purchase4Reporting table which is used for reporting
                        Purchase4Reporting purch4RepT = new Purchase4Reporting();
                        purch4RepT.PurchaseID = purchT.PurchaseID;
                        purch4RepT.Weight = purchaseObject.VolumeByWeight;
                        purch4RepT.Volume = purchaseObject.Quantity;
                        purch4RepT.Proof = purchaseObject.ProofGallon;
                        purch4RepT.Alcohol = purchaseObject.AlcoholContent;
                        purch4RepT.Redistilled = false;

                        db.Purchase4Reporting.Add(purch4RepT);
                        db.SaveChanges();
                        retMthdExecResult = true;
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Exception creating an entry in Purchase4Reporting: ", e);
                        retMthdExecResult = false;
                    }
                }

                retMthdExecResult = true;

                // now, lets' try to save to history table
                try
                {
                    purchaseObject.PurchaseId = purchT.PurchaseID;
                    SavePurchaseHistory(purchaseObject, userId);
                }
                catch (Exception e)
                {
                    throw;
                }
                
            }
            catch (Exception e)
            {
                retMthdExecResult = false;
                return retMthdExecResult;
                throw;
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
                purH.Alcohol = purObject.AlcoholContent;
                purH.Gauged = purObject.Gauged;
                purH.Note = purObject.Note;
                purH.Price = purObject.Price;
                purH.Proof = purObject.ProofGallon;
                purH.PurchaseDate = purObject.PurchaseDate;
                purH.PurchaseID = purObject.PurchaseId;
                purH.State = purObject.State;
                purH.Status = purObject.Status;
                purH.UserID = userId;
                purH.PurchaseName = purObject.PurBatchName;
                purH.UpdateDate = DateTime.Today;
                purH.Vendor = purObject.VendorName;
                purH.Volume = purObject.Quantity;
                purH.Weight = purObject.VolumeByWeight;
                purH.Storage = purObject.StorageName;

                db.PurchaseHistory.Add(purH);
                db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw;
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
                    MaterialName = (material.Name ?? ""),
                    VendorName = vendor.Name,
                    Gallons = ((System.Single?)galQuant.Value ?? (System.Single?)0),
                    VolumeByWeight = ((System.Single?)VBW.Value ?? (System.Single?)0),
                    Alcohol = ((System.Single?)alc.Value ?? (System.Single?)0),
                    Proof = ((System.Single?)proof.Value ?? (System.Single?)0),
                    State = (states.Name ?? ""),
                    Status = (statuses.Name ?? "")
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
                if(storages != null)
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
        /* algorithm:
        * Example with Production type Fermentation
        * if productionType == "Fermentation"
        *   Insert into Fermented table
        *   Insert into Fermentation table
        *   Insert into FermentedToPurchase table
        *   Insert into Quantity table
        *   Insert into VolumeByWeight table
        *   Insert into Notes table
        *   Insert into RecordToStorage table
        *   Insert into StatusToPurchase table
        *   Insert into StateToPurchase 
        */
        public bool CreateProduction(ProductionObject prodObject, int userId)
        {
            //define method execution return value to be false by default
            bool retMthdExecResult = false;
            float cumulativeGainLoss = 0;
            int currentProdId = 0;

            var distillerId = GetDistillerId(userId);

            Production prod = new Production();
            prod.ProductionName = prodObject.BatchName;
            prod.DistillerID = distillerId;
            prod.ProductionDate = prodObject.ProductionDate;
            prod.ProductionStartTime = prodObject.ProductionStart;
            prod.ProductionEndTime = prodObject.ProductionEnd;
            prod.Note = prodObject.Note;

            if(prodObject.Gauged)
            {
                prod.Gauged = prodObject.Gauged;
            }
            else if(!prodObject.Gauged)
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

            if (prodObject?.SpiritTypeReportingID > 0 && prodObject?.MaterialKindReportingID > 0)
            {
                ProductionToSpiritTypeReporting prodToSpirType = new ProductionToSpiritTypeReporting();
                prodToSpirType.SpiritTypeReportingID = prodObject.SpiritTypeReportingID;
                prodToSpirType.MaterialKindReportingID = prodObject.MaterialKindReportingID;
                prodToSpirType.ProductionID = prod.ProductionID;
                db.ProductionToSpiritTypeReporting.Add(prodToSpirType);
                db.SaveChanges();
            }

            if (prodObject.ProductionType == "Fermentation")
            {
                var statusString = "";

                // in this section, we need to handle used materials. update its statuses, values, states
                if (prodObject.UsedMats != null)
                {
                    foreach (var i in prodObject.UsedMats)
                    {
                        var purch =
                            (from rec in db.Purchase
                             where rec.PurchaseID == i.ID
                             select rec).FirstOrDefault();

                        // we need to determine the status of used material. It could be either "Processing" or "Processed" - depending on if we burnt down all or partial of used material
                        if (i.OldVal <= 0)
                        {
                            statusString = "Processed";
                        }
                        else if (i.OldVal >= 0)
                        {
                            statusString = "Processing";
                        }

                        var status =
                            (from rec in db.Status
                             where rec.Name == statusString
                             select rec).FirstOrDefault();

                        purch.StatusID = status.StatusID;

                        // we need to make sure that if the purchased used material is being partially used, we need to create 
                        // a new production fermentation record in Purchase4Reporting table with the same Purchase ID but with different Volume/Weight value.
                        if (i.BurningDownMethod != null)
                        {
                            purch.BurningDownMethod = i.BurningDownMethod;
                        }

                        Purchase4Reporting purch4Rep = new Purchase4Reporting();
                        purch4Rep.PurchaseID = i.ID;

                        // save to the ProductionContent table
                        ProductionContent prodContent = new ProductionContent();
                        prodContent.ProductionID = prod.ProductionID;
                        prodContent.RecordID = i.ID;

                        if (i.BurningDownMethod == "volume")
                        {
                            purch4Rep.Volume = i.OldVal;
                            prodContent.ContentFieldID = 1; // PurFermentableVolume in ContentField table
                        }
                        else if (i.BurningDownMethod == "weight")
                        {
                            purch4Rep.Weight = i.OldVal;
                            prodContent.ContentFieldID = 2; // PurFermentableWeight in ContentField table
                        }

                        prodContent.ContentValue = i.NewVal;
                        prodContent.isProductionComponent = false;

                        db.ProductionContent.Add(prodContent);
                        db.Purchase4Reporting.Add(purch4Rep);

                        db.SaveChanges();

                        if (i.BurningDownMethod == "volume" && purch.VolumeID > 0)
                        {
                            var q =
                                (from rec in db.Volume
                                 where rec.VolumeID == purch.VolumeID
                                 select rec).FirstOrDefault();

                            if (q != null)
                            {
                                q.Value = i.OldVal;
                            }
                        }

                        if (purch.WeightID > 0)
                        {
                            var vBW =
                                (from rec in db.Weight
                                 where purch.WeightID == rec.WeightID
                                 select rec).FirstOrDefault();

                            if (vBW != null)
                            {
                                vBW.Value = i.OldVal;
                            }
                        }

                        // save purchase table after we've updated State, Status, Burndown method and Quantities of Materials just used
                        db.SaveChanges();

                        // we need to update this table because we need to associate production records with IDs of materials used for those production records.
                        ProductionToPurchase prodToPur = new ProductionToPurchase();
                        prodToPur.ProductionID = prod.ProductionID;
                        prodToPur.PurchaseID = i.ID;
                        db.ProductionToPurchase.Add(prodToPur);
                        db.SaveChanges();
                    }
                }

                retMthdExecResult = true;
            }

            else if (prodObject.ProductionType == "Distillation")
            {
                // list of purchase ids of purchase records and ids associated with production records
                // that are used in distillation process
                List<int> purIdL = new List<int>();
                try
                {
                    // verify list of batches received from the front-end and used in distillation is not empty
                    if (prodObject.UsedMats != null)
                    {
                        if (prodObject?.SpiritCutId != null)
                        {
                            ProductionToSpiritCut prodToSCut = new ProductionToSpiritCut();
                            prodToSCut.SpiritCutID = prodObject.SpiritCutId;
                            prodToSCut.ProductionID = prod.ProductionID;
                            db.ProductionToSpiritCut.Add(prodToSCut);
                            db.SaveChanges();
                        }

                        // iterate through purchased and produced batches used in distillation
                        foreach (var k in prodObject.UsedMats)
                        {
                            var status = string.Empty;

                            // all of the batch volume/weight used in the distillation
                            if (k.OldVal <= 0)
                            {
                                status = "Processed";
                            }
                            // only a part of the batch volume/weight used in the distillation
                            else if (k.OldVal >= 0)
                            {
                                status = "Processing";
                            }

                            var statusId =
                                (from rec in db.Status
                                 where rec.Name == status
                                 select rec.StatusID).FirstOrDefault();

                            // purchase batch used in the distillation
                            if (k.DistillableOrigin == "pur")
                            {
                                purIdL.Add(k.ID);

                                var purch =
                                    (from rec in db.Purchase
                                     where rec.PurchaseID == k.ID
                                     select rec).FirstOrDefault();

                                // set burning down method for the batch used in distillation,
                                // if it hasn't been done yet, to "volume" or "weight"
                                if (purch.BurningDownMethod == null && k.BurningDownMethod != null)
                                {
                                    purch.BurningDownMethod = k.BurningDownMethod;
                                }

                                // we need to make sure that if the purchased used material is being partially distilled, we need to create 
                                // a new distilate record in Purchase4Reporting table with the same Purchase ID but with different Proof value.
                                // else, the same record in Purchase4Reporting needs to be marked as redistilled for reporting purposes.

                                if (purch.PurchaseTypeID == 3)
                                {
                                    var p =
                                        (from rec in db.Purchase4Reporting
                                         where purch.PurchaseID == rec.PurchaseID
                                         select rec).FirstOrDefault();
                                    if (p != null)
                                    {
                                        if (p.Proof == k.Proof && p.PurchaseID == k.ID)
                                        {
                                            p.Redistilled = true;
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
                                            db.Purchase4Reporting.Add(purch4Rep);
                                            db.SaveChanges();
                                        }
                                    }

                                    // update proof value after it has been recalculated
                                    // on front-end using the new volume quantity.
                                    // Update only if purchased batch is distilled, since fermented purchase proof is not stored
                                    if (purch.ProofID > 0 && k.Proof >= 0)
                                    {
                                        var proof =
                                            (from rec in db.Proof
                                             where rec.ProofID == purch.ProofID
                                             select rec).FirstOrDefault();

                                        if (proof != null)
                                        {
                                            proof.Value = k.Proof;
                                        }
                                        db.SaveChanges();
                                    }
                                }

                                // update status to "Processing" or "Processed"
                                if (statusId > 0 && purch != null)
                                {
                                    purch.StatusID = statusId;
                                }

                                if (k.BurningDownMethod == "volume" && purch.VolumeID > 0)
                                {
                                    var q =
                                        (from rec in db.Volume
                                         where purch.VolumeID == rec.VolumeID
                                         select rec).FirstOrDefault();

                                    if (q != null)
                                    {
                                        q.Value = k.OldVal;
                                    }

                                    try
                                    {
                                        // Update proof recalculated on the front-end for a batch used in production 
                                        UpdateProof(purch.ProofID, k.Proof);
                                    }
                                    catch(ArgumentOutOfRangeException ex)
                                    {
                                        Debug.WriteLine(ex.Message);
                                    }
                                }

                                if (k.BurningDownMethod == "weight" && purch.WeightID > 0)
                                {
                                    var vBW =
                                        (from rec in db.Weight
                                         where rec.WeightID == purch.WeightID
                                         select rec).FirstOrDefault();

                                    if (vBW != null)
                                    {
                                        vBW.Value = k.OldVal;
                                    }
                                    db.SaveChanges();
                                }

                                // save to the ProductionContent table
                                ProductionContent prodContent = new ProductionContent();
                                prodContent.ProductionID = prod.ProductionID;
                                prodContent.RecordID = k.ID;
                                if (k.BurningDownMethod == "weight")
                                {
                                    if(purch.PurchaseTypeID == 2)
                                    {
                                        prodContent.ContentFieldID = 4; // PurFermentedWeight in ContentField table
                                    }

                                    if (purch.PurchaseTypeID == 3)
                                    {
                                        prodContent.ContentFieldID = 10; // PurDistilledWeight in ContentField table
                                    }
                                }

                                if (k.BurningDownMethod == "volume")
                                {
                                    if (purch.PurchaseTypeID == 2)
                                    {
                                        prodContent.ContentFieldID = 3; // PurFermentedVolume in ContentField table
                                    }

                                    if (purch.PurchaseTypeID == 3)
                                    {
                                        prodContent.ContentFieldID = 9; // PurDistilledVolume in ContentField table
                                    }
                                }

                                prodContent.ContentValue = k.NewVal;
                                prodContent.isProductionComponent = false;

                                db.ProductionContent.Add(prodContent);

                                db.SaveChanges();

                            }
                            // production batch used in the distillation
                            else if (k.DistillableOrigin == "prod")
                            {
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

                                // we need to make sure that if the used material that was produced by us is a distilate and being re-distiled again,
                                // it needs to be marked as redistilled for reporting purposes if all of the proof gallons are used. Else, we need to insert
                                // another record into Production4Reporting with the same ProductionID but with different Proof and volume/weight values.
                                if (prodRec.ProductionTypeID == 2)
                                {
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
                                }

                                // set burning down method for the batch used in distillation,
                                // if it hasn't been done yet, to "volume" or "weight"
                                if (prodRec.BurningDownMethod == null && k.BurningDownMethod != null)
                                {
                                    prodRec.BurningDownMethod = k.BurningDownMethod;
                                }

                                // update status to "Processing" or "Processed"
                                if (statusId > 0 && prodRec != null)
                                {
                                    prodRec.StatusID = statusId;
                                }

                                if (k.BurningDownMethod == "volume" && prodRec.VolumeID > 0)
                                {
                                    var q =
                                        (from rec in db.Volume
                                         where prodRec.VolumeID == rec.VolumeID
                                         select rec).FirstOrDefault();

                                    if (q != null)
                                    {
                                        q.Value = k.OldVal;
                                    }

                                    try
                                    {
                                        // Update proof recalculated on the front-end for a batch used in production 
                                        UpdateProof(prodRec.ProofID, k.Proof);
                                    }
                                    catch (ArgumentOutOfRangeException ex)
                                    {
                                        Debug.WriteLine(ex.Message);
                                    }
                                }

                                if (k.BurningDownMethod == "weight" && prodRec.WeightID > 0)
                                {
                                    var vBW =
                                        (from rec in db.Weight
                                         where prodRec.WeightID == rec.WeightID
                                         select rec).FirstOrDefault();

                                    if (vBW != null)
                                    {
                                        vBW.Value = k.OldVal;
                                    }
                                }

                                db.SaveChanges();

                                // save to the ProductionContent table
                                ProductionContent prodContent = new ProductionContent();
                                prodContent.ProductionID = prod.ProductionID;
                                prodContent.RecordID = k.ID;
                                if (k.BurningDownMethod == "weight")
                                {
                                    if (prodRec.ProductionTypeID == 1)
                                    {
                                        prodContent.ContentFieldID = 6; // ProdFermentedWeight in ContentField table
                                    }

                                    if (prodRec.ProductionTypeID == 2)
                                    {
                                        prodContent.ContentFieldID = 12; // ProdDistilledWeight in ContentField table
                                    }
                                }

                                if (k.BurningDownMethod == "volume")
                                {
                                    if (prodRec.ProductionTypeID == 1)
                                    {
                                        prodContent.ContentFieldID = 5; // ProdFermentedVolume in ContentField table
                                    }

                                    if (prodRec.ProductionTypeID == 2)
                                    {
                                        prodContent.ContentFieldID = 11; // ProdDistilledVolume in ContentField table
                                    }
                                }

                                prodContent.ContentValue = k.NewVal;
                                prodContent.isProductionComponent = true;

                                db.ProductionContent.Add(prodContent);

                                db.SaveChanges();
                            }

                            if (purIdL != null)
                            {
                                // iterate through list of purchaseIds of purchase records
                                // and purchase records associated with production records
                                // used in the distillation
                                foreach (var i in purIdL)
                                {
                                    ProductionToPurchase prodToPur = new ProductionToPurchase();
                                    prodToPur.ProductionID = prod.ProductionID;
                                    prodToPur.PurchaseID = i;
                                    db.ProductionToPurchase.Add(prodToPur);
                                    db.SaveChanges();
                                }
                            }
                        }
                        retMthdExecResult = true;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Exception creating Distillation production: ", e);
                    retMthdExecResult = false;
                }
            }

            else if (prodObject.ProductionType == "Blending")
            {
                string statusString = string.Empty;
                List<int> purIdL = new List<int>(); // this is used as a temp holder for purchase ids

                if (prodObject.UsedMats != null) // we need to makre sure that in Production workflow front-end we assign either raw materials or distil IDs to it
                {
                    if (prodObject?.SpiritId != null)
                    {
                        ProductionToSpirit prodToSpirit = new ProductionToSpirit();
                        prodToSpirit.SpiritID = prodObject.SpiritId;
                        prodToSpirit.ProductionID = prod.ProductionID;
                        db.ProductionToSpirit.Add(prodToSpirit);
                        db.SaveChanges();
                    }

                    foreach (var k in prodObject.UsedMats) // this scope is used to address materials that come from  purchase
                    {
                        // we need to determine the status of used material. It could be either "Processing" or "Processed" - depending on if we burnt down all or partial of used material
                        if (k.OldVal <= 0)
                        {
                            statusString = "Processed";
                        }
                        else if (k.OldVal >= 0)
                        {
                            statusString = "Processing";
                        }

                        var status =
                        (from rec in db.Status
                         where rec.Name == statusString
                         select rec).FirstOrDefault();

                        if (k.DistillableOrigin == "pur")
                        {
                            purIdL.Add(k.ID);

                            var purch =
                                (from rec in db.Purchase
                                 where rec.PurchaseID == k.ID
                                 select rec).FirstOrDefault();

                            if (purch.BurningDownMethod == null && k.BurningDownMethod != null)
                            {
                                purch.BurningDownMethod = k.BurningDownMethod;
                            }

                            // we need to make sure that if the purchased used material is being partially distilled, we need to create 
                            // a new distilate record in Purchase4Reporting table with the same Purchase ID but with different Proof value.
                            // else, the same record in Purchase4Reporting needs to be marked as redistilled for reporting purposes.

                            if (purch.PurchaseTypeID == 3) // gl - why are we checking for PurchaseType here. Wouldn't we want to burn down anything and save it to Purchase4Reporting table?
                            {
                                var p =
                                    (from rec in db.Purchase4Reporting
                                     where purch.PurchaseID == rec.PurchaseID
                                     select rec).FirstOrDefault();
                                if (p != null)
                                {
                                    if (p.Proof == k.Proof && p.PurchaseID == k.ID)
                                    {
                                        p.Redistilled = true;
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
                                        db.Purchase4Reporting.Add(purch4Rep);
                                        db.SaveChanges();
                                    }
                                }
                            }

                            if (purch.VolumeID > 0 && k.BurningDownMethod == "volume")
                            {
                                var q =
                                    (from rec in db.Volume
                                     where purch.VolumeID == rec.VolumeID
                                     select rec).FirstOrDefault();

                                if (q != null)
                                {
                                    q.Value = k.OldVal;
                                }

                                try
                                {
                                    // Update proof recalculated on the front-end for a batch used in production 
                                    UpdateProof(purch.ProofID, k.Proof);
                                }
                                catch (ArgumentOutOfRangeException ex)
                                {
                                    Debug.WriteLine(ex.Message);
                                }
                            }

                            if (purch.WeightID > 0 && k.BurningDownMethod == "weight")
                            {
                                var vBW =
                                    (from rec in db.Weight
                                     where purch.WeightID == rec.WeightID
                                     select rec).FirstOrDefault();

                                if (vBW != null)
                                {
                                    vBW.Value = k.OldVal;
                                }
                            }

                            purch.StatusID = status.StatusID;

                            db.SaveChanges();

                            // save to the ProductionContent table
                            ProductionContent prodContent = new ProductionContent();
                            prodContent.ProductionID = prod.ProductionID;
                            prodContent.RecordID = k.ID;
                            if (k.BurningDownMethod == "weight")
                            {
                                if (purch.PurchaseTypeID == 3)
                                {
                                    prodContent.ContentFieldID = 10; // PurDistilledWeight in ContentField table
                                }
                            }

                            if (k.BurningDownMethod == "volume")
                            {
                                if (purch.PurchaseTypeID == 3)
                                {
                                    prodContent.ContentFieldID = 9; // PurDistilledVolume in ContentField table
                                }
                            }

                            prodContent.ContentValue = k.NewVal;
                            prodContent.isProductionComponent = false;

                            db.ProductionContent.Add(prodContent);

                            db.SaveChanges();
                        }
                        else if (k.DistillableOrigin == "prod")
                        {
                            purIdL.Add(k.ID);

                            var prodd =
                                    (from rec in db.Production
                                     where rec.ProductionID == k.ID
                                     select rec).FirstOrDefault();

                            if (prodd.BurningDownMethod == null && k.BurningDownMethod != null)
                            {
                                prodd.BurningDownMethod = k.BurningDownMethod;
                            }

                            // we need to make sure that if the used material that was produced by us is a distilate and being re-distiled again,
                            // it needs to be marked as redistilled for reporting purposes if all of the proof gallons are used. Else, we need to insert
                            // another record into Production4Reporting with the same ProductionID but with different Proof and volume/weight values.
                            if (prodd.ProductionTypeID == 2)
                            {
                                var p =
                                    (from rec in db.Production4Reporting
                                     where prodd.ProductionID == rec.ProductionID
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

                                        if (k.BurningDownMethod == "weight" && prodd.WeightID > 0)
                                        {
                                            prod4Rep.Weight = k.OldVal;
                                        }
                                        if (k.BurningDownMethod == "volume" && prodd.VolumeID > 0)
                                        {
                                            prod4Rep.Volume = k.OldVal;
                                        }
                                        prod4Rep.Redistilled = true;
                                        db.Production4Reporting.Add(prod4Rep);
                                        db.SaveChanges();
                                    }
                                }
                            }

                            if (prodd.VolumeID > 0 && k.BurningDownMethod == "volume")
                            {
                                var q =
                                    (from rec in db.Volume
                                     where prodd.VolumeID == rec.VolumeID
                                     select rec).FirstOrDefault();

                                if (q != null)
                                {
                                    q.Value = k.OldVal;
                                }

                                try
                                {
                                    // Update proof recalculated on the front-end for a batch used in production
                                    UpdateProof(prodd.ProofID, k.Proof);
                                }
                                catch (ArgumentOutOfRangeException ex)
                                {
                                    Debug.WriteLine(ex.Message);
                                }
                            }

                            if (prodd.WeightID > 0 && k.BurningDownMethod == "weight")
                            {
                                var vBW =
                                    (from rec in db.Weight
                                     where prodd.WeightID == rec.WeightID
                                     select rec).FirstOrDefault();

                                if (vBW != null)
                                {
                                    vBW.Value = k.OldVal;
                                }
                            }

                            prodd.StatusID = status.StatusID;

                            db.SaveChanges();

                            // now, let's get PurchaseIds from ProductionToPurchase table for later update of ProductionToPurchase table
                            var prod2Purch =
                                (from rec in db.ProductionToPurchase
                                    where rec.ProductionID == k.ID
                                    select rec.PurchaseID);

                            if (prod2Purch != null)
                            {
                                purIdL.AddRange(prod2Purch);
                            }

                            // save to the ProductionContent table
                            ProductionContent prodContent = new ProductionContent();
                            prodContent.ProductionID = prod.ProductionID;
                            prodContent.RecordID = k.ID;
                            if (k.BurningDownMethod == "weight")
                            {
                                if (prodd.ProductionTypeID == 2)
                                {
                                    prodContent.ContentFieldID = 12; // ProdDistilledWeight in ContentField table
                                }
                            }

                            if (k.BurningDownMethod == "volume")
                            {
                                if (prodd.ProductionTypeID == 2)
                                {
                                    prodContent.ContentFieldID = 11; // ProdDistilledVolume in ContentField table
                                }
                            }

                            prodContent.ContentValue = k.NewVal;
                            prodContent.isProductionComponent = true;

                            db.ProductionContent.Add(prodContent);

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
                    }

                    // update ProductionToPurchase table
                    if (purIdL != null)
                    {
                        foreach (var i in purIdL)
                        {
                            ProductionToPurchase prodToPur = new ProductionToPurchase();
                            prodToPur.ProductionID = prod.ProductionID;
                            prodToPur.PurchaseID = i;
                            db.ProductionToPurchase.Add(prodToPur);

                            db.SaveChanges();
                        }
                    }

                    retMthdExecResult = true;
                }
            }

            else if (prodObject.ProductionType == "Bottling")
            {
                string statusString = string.Empty;

                List<int> purIdL = new List<int>(); // this is used as a temp holder for purchase ids

                if (prodObject.UsedMats != null) // we need to makre sure that in Production workflow front-end we assign either raw materials or distil IDs to it
                {
                    if (prodObject?.SpiritId != null)
                    {
                        ProductionToSpirit prodToSpirit = new ProductionToSpirit();
                        prodToSpirit.SpiritID = prodObject.SpiritId;
                        prodToSpirit.ProductionID = prod.ProductionID;
                        db.ProductionToSpirit.Add(prodToSpirit);
                    }

                    foreach (var k in prodObject.UsedMats) // this scope is used to address materials that come from  purchase
                    {
                        // we need to determine the status of used material. It could be either "Processing" or "Processed" - depending on if we burnt down all or partial of used material
                        if (k.OldVal <= 0)
                        {
                            statusString = "Processed";
                        }
                        else if (k.OldVal >= 0)
                        {
                            statusString = "Processing";
                        }

                        var status =
                        (from rec in db.Status
                         where rec.Name == statusString
                         select rec).FirstOrDefault();

                        if (k.DistillableOrigin == "prod")
                        {
                            purIdL.Add(k.ID);

                            var prodd =
                                    (from rec in db.Production
                                     where rec.ProductionID == k.ID
                                     select rec).FirstOrDefault();

                            if (prodd.BurningDownMethod == null && k.BurningDownMethod != null)
                            {
                                prodd.BurningDownMethod = k.BurningDownMethod;
                            }

                            if (prodd.VolumeID > 0 && k.BurningDownMethod == "volume")
                            {
                                var q =
                                    (from rec in db.Volume
                                     where prodd.VolumeID == rec.VolumeID
                                     select rec).FirstOrDefault();

                                if (q != null)
                                {
                                    q.Value = k.OldVal;
                                }

                                float oldProof = UpdateProof(prodd.ProofID, k.Proof);

                                // todo: grisha: I am not sure how calculating gain/loss is going be relevant when we add mutliple blends during bottling though.
                                // but we can leave this for a later discussion.
                                cumulativeGainLoss += prodObject.ProofGallon - oldProof; // negative means loss and positive means true
                            }

                            if (prodd.WeightID > 0 && k.BurningDownMethod == "weight")
                            {
                                var vBW =
                                    (from rec in db.Weight
                                     where prodd.WeightID == rec.WeightID
                                     select rec).FirstOrDefault();

                                if (vBW != null)
                                {
                                    vBW.Value = k.OldVal;
                                }
                            }

                            prodd.StatusID = status.StatusID;

                            db.SaveChanges();

                            // now, let's get PurchaseIds from ProductionToPurchase table for later update of ProductionToPurchase table
                            var prod2Purch =
                                (from rec in db.ProductionToPurchase
                                 where rec.ProductionID == k.ID
                                 select rec.PurchaseID);

                            if (prod2Purch != null)
                            {
                                purIdL.AddRange(prod2Purch);
                            }

                            // save to the ProductionContent table
                            ProductionContent prodContent = new ProductionContent();
                            prodContent.ProductionID = prod.ProductionID;
                            prodContent.RecordID = k.ID;
                            if (k.BurningDownMethod == "weight")
                            {
                                if (prodd.ProductionTypeID == 3)
                                {
                                    prodContent.ContentFieldID = 12; // ProdDistilledWeight in ContentField table
                                }
                            }

                            if (k.BurningDownMethod == "volume")
                            {
                                if (prodd.ProductionTypeID == 3)
                                {
                                    prodContent.ContentFieldID = 11; // ProdDistilledVolume in ContentField table
                                }
                            }

                            prodContent.ContentValue = k.NewVal;
                            prodContent.isProductionComponent = true;

                            db.ProductionContent.Add(prodContent);

                            db.SaveChanges();
                        }
                    }
                    // update ProductionToPurchase table
                    if (purIdL != null)
                    {
                        foreach (var i in purIdL)
                        {
                            ProductionToPurchase prodToPur = new ProductionToPurchase();
                            prodToPur.ProductionID = prod.ProductionID;
                            prodToPur.PurchaseID = i;
                            db.ProductionToPurchase.Add(prodToPur);

                            db.SaveChanges();
                        }
                    }

                    // now, lets register gains/losses
                    if (cumulativeGainLoss > 0)
                    {
                        // gain
                        GainLoss glt = new GainLoss();
                        glt.Type = true;
                        glt.Quantity = cumulativeGainLoss;
                        glt.DateRecorded = DateTime.Today;
                        glt.BlendedRecordId = currentProdId;
                        glt.BottledRecordId = prod.ProductionID;
                        //glt.DistillerID = DistillerID;
                        db.GainLoss.Add(glt);
                        db.SaveChanges();
                    }
                    else if (cumulativeGainLoss < 0)
                    {
                        // loss
                        GainLoss glt = new GainLoss();
                        glt.Type = false;
                        glt.Quantity = Math.Abs(cumulativeGainLoss); // since cumulativeGainLoss is negative, making it to be positive
                        glt.DateRecorded = DateTime.Today;
                        glt.BlendedRecordId = currentProdId;
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
                }
                retMthdExecResult = true;
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
                retMthdExecResult = true;
            }
            catch(Exception e)
            {
                Debug.WriteLine("Exception creating an entry in Production4Reporting: ", e);
                retMthdExecResult = false;
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

        internal bool DeleteProduction(ProductionObject productionObject, int userId)
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
                         where rec.ProductionID == productionObject.ProductionId &&
                            distillers.UserId == userId
                         select rec).FirstOrDefault();

                    if (prodRec != null)
                    {
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
                            foreach (var rec in strRecs)
                            {
                                db.StorageToRecord.Remove(rec);
                            }
                        }

                        db.Production.Remove(prodRec);

                        db.SaveChanges();
                    }

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
                status.Name != "Deleted"  &&
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
                    SpiritCut = (spiCuts.Name ?? ""),
                    SpiritCutID = ((System.Int32?)spiCuts.SpiritCutID ?? (System.Int32?)0),
                    SpiritName = (spi.Name ?? ""),
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
            byte [] existsArray = new byte[256];

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
                        MaterialKindReportingID = ((System.Single?)spiT2Mat.MaterialKindReportingID ?? (System.Single?)0),
                        SpiritTypeReportingID = ((System.Single?)spiT2Mat.SpiritTypeReportingID ?? (System.Single?)0),
                        MaterialKindName = (matKind.MaterialKindName ?? ""),
                        ProductTypeName = (spiType.ProductTypeName ?? ""),
                    };
                foreach (var i in res)
                {
                    if (existsArray[(int)i.SpiritTypeReportingID] == 0)
                    {
                        SpiritToKindListObject spir2Kind = new SpiritToKindListObject();
                        spir2Kind.SpiritTypeReportingID = (int)i.SpiritTypeReportingID;
                        spir2Kind.ProductTypeName = i.ProductTypeName;
                        List<MaterialKindObject> kindList = new List<MaterialKindObject>(); // material kind list
                        MaterialKindObject kind = new MaterialKindObject();
                        kind.MaterialKindID = (int)i.MaterialKindReportingID;
                        kind.MaterialKindName = i.MaterialKindName;
                        kind.SpiritTypeReportingID = (int)i.SpiritTypeReportingID;
                        kindList.Add(kind);
                        spir2Kind.MaterialKindObject = kindList;
                        spir2KindList.Add(spir2Kind);
                        existsArray[(int)i.SpiritTypeReportingID] = 1;
                    }
                    else if (existsArray[(int)i.SpiritTypeReportingID] == 1)
                    {
                        foreach(var li in spir2KindList)
                        {
                            if(li.SpiritTypeReportingID == (int)i.SpiritTypeReportingID)
                            {
                                MaterialKindObject kind = new MaterialKindObject();
                                kind.MaterialKindID = (int)i.MaterialKindReportingID;
                                kind.MaterialKindName = i.MaterialKindName;
                                kind.SpiritTypeReportingID = (int)i.SpiritTypeReportingID;
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

        #region Reporting

        internal ProductionReportingObject GetProductionReportData(DateTime start, DateTime end, int userId)
        {
            ProductionReportingObject prodRepObj = new ProductionReportingObject();
            List<ProdReportPart1> part1List = new List<ProdReportPart1>();
            List<ProdReportParts2Through5> part2Thru5List = new List<ProdReportParts2Through5>();
            List<ProdReportPart6> prodReportPart6List = new List<ProdReportPart6>();
            List<ProductionReportHelper> tempRepObjList = new List<ProductionReportHelper>();

            // get distiller information for header report
            int distillerID = GetDistillerId(userId);
            prodRepObj.Header = GetDistillerInfoForReportHeader(distillerID, start);

            // we need this of Part 1
            GetSpiritsForProductionReport(userId, start, end, ref tempRepObjList);

            // we need this for line 17(b) of Part 1
            GetUnfinishedSpiritsForProductionReport(userId, start, end, ref tempRepObjList);

            foreach (var rec in tempRepObjList)
            {
                // Deal with part 1 Start
                // Assign data for part 1
                var spiritType = part1List.Find(x => x.SpiritTypeReportingID == rec.SpiritTypeReportingID);

                if (spiritType == null)
                {
                    ProdReportPart1 part1Obj = new ProdReportPart1();
                    part1Obj.SpiritCatName = rec.SpiritTypeReportName;
                    part1Obj.SpiritTypeReportingID = (int)rec.SpiritTypeReportingID;

                    if (!(bool)rec.Gauged)
                    {
                        part1Obj.UnfinishedSpiritsEndOfQuarter += (float)rec.Proof;
                    }
                    else if ((bool)rec.Redistilled == true)
                    {
                        part1Obj.Recd4Redistil += (float)rec.Proof;
                    }
                    else
                    {
                        if(rec.StateID == 3) // Distilled - Storage report case
                        {
                            part1Obj.SorageAcct += (float)rec.Proof;
                        }
                        else if(rec.StateID == 4 || rec.StateID == 5) // Blended or Bottled cases- Processing report case
                        {
                            part1Obj.ProccessingAcct += (float)rec.Proof;
                        }
                    }
                    part1List.Add(part1Obj);
                }
                else
                {
                    if(!(bool)rec.Gauged)
                    {
                        spiritType.UnfinishedSpiritsEndOfQuarter += (float)rec.Proof;
                    }
                    else if ((bool)rec.Redistilled == true)
                    {
                        spiritType.Recd4Redistil += (float)rec.Proof;
                    }
                    else
                    {
                        if (rec.StateID == 3) // Distilled - Storage report case
                        {
                            spiritType.SorageAcct += (float)rec.Proof;
                        }
                        else if (rec.StateID == 4 || rec.StateID == 5) // Blended or Bottled cases- Processing report case
                        {
                            spiritType.ProccessingAcct += (float)rec.Proof;
                        }
                    }
                }

                // Deal with parts 2 through 5 Start
                var materialKind = part2Thru5List.Find(x => x.MaterialKindReportingID == rec.MaterialKindReportingID);

                if (materialKind == null)
                {
                    ProdReportParts2Through5 prodRP2T5 = new ProdReportParts2Through5();
                    prodRP2T5.KindOfMaterial = rec.MaterialKindReportingName;
                    prodRP2T5.MaterialKindReportingID = (int)rec.MaterialKindReportingID;
                    prodRP2T5.ProofGallons = (float)rec.Proof;
                    prodRP2T5.SpiritTypeReportingID = (int)rec.SpiritTypeReportingID;
                    part2Thru5List.Add(prodRP2T5);
                }
                else
                {
                    materialKind.ProofGallons += (float)rec.Proof;
                }
            }

            prodRepObj.Part1 = part1List;
            prodRepObj.Part2Through5 = part2Thru5List;

            try
            {
                // get data for part 6(page 2 of production report)
                var materials =
                    from prodCont in db.ProductionContent
                    join prod in db.Production on prodCont.ProductionID equals prod.ProductionID into prod_join
                    from prod in prod_join.DefaultIfEmpty()
                    join distillers in db.AspNetUserToDistiller on prod.DistillerID equals distillers.DistillerID into distillers_join
                    from distillers in distillers_join.DefaultIfEmpty()
                    join prodC in db.ProductionContent on prod.ProductionID equals prodC.ProductionID into prodC_join
                    from prodC in prodC_join.DefaultIfEmpty()
                    join prod2Purch in db.ProductionToPurchase on prod.ProductionID equals prod2Purch.ProductionID into prod2Purch_join
                    from prod2Purch in prod2Purch_join.DefaultIfEmpty()
                    join purch in db.Purchase on prod2Purch.PurchaseID equals purch.PurchaseID into purch_join
                    from purch in purch_join.DefaultIfEmpty()
                    join matDict in db.MaterialDict on purch.MaterialDictID equals matDict.MaterialDictID into matDict_join
                    from matDict in matDict_join.DefaultIfEmpty()
                    join contF in db.ContentField on prodC.ContentFieldID equals contF.ContentFieldID into contF_join
                    from contF in contF_join.DefaultIfEmpty()
                    join prod2SpiritType in db.ProductionToSpiritTypeReporting on prod.ProductionID equals prod2SpiritType.ProductionID into prod2SpiritType_join
                    from prod2SpiritType in prod2SpiritType_join.DefaultIfEmpty()
                    join matKindRep in db.MaterialKindReporting on prod2SpiritType.MaterialKindReportingID equals matKindRep.MaterialKindReportingID into matKindRep_join
                    from matKindRep in matKindRep_join.DefaultIfEmpty()
                    join prodRepMatCat2MatKind in db.ProdRepMatCat2MaterialKind on matKindRep.MaterialKindReportingID equals prodRepMatCat2MatKind.MaterialKindReportingID into prodRepMatCat2MatKind_join
                    from prodRepMatCat2MatKind in prodRepMatCat2MatKind_join.DefaultIfEmpty()
                    join prodRepMatCat in db.ProductionReportMaterialCategory on prodRepMatCat2MatKind.ProductionReportMaterialCategoryID equals prodRepMatCat.ProductionReportMaterialCategoryID into prodRepMatCat_join
                    from prodRepMatCat in prodRepMatCat_join.DefaultIfEmpty()
                    where
                      distillers.UserId == userId &&
                      prod.ProductionEndTime >= start &&
                      prod.ProductionEndTime <= end &&
                      prodC.isProductionComponent == false &&
                      purch.PurchaseTypeID != 3
                    select new
                    {
                        ProductionID = (int?)prod.ProductionID,
                        MaterialName = matDict.Name,
                        Value = prodC.ContentValue,
                        ContentFieldID = (int?)contF.ContentFieldID,
                        ProductionReportMaterialCategoryID = (int?)prodRepMatCat.ProductionReportMaterialCategoryID ?? (int?)0
                    };

                if (materials != null)
                {
                    foreach (var t in materials)
                    {
                        var mater = prodReportPart6List.Find(x => x.KindOfMaterial == (string)t.MaterialName);

                        // case where Material doesn't exist
                        if (mater == null)
                        {
                            ProdReportPart6 prt6 = new ProdReportPart6();
                            prt6.ProductionID = (int)t.ProductionID;
                            prt6.KindOfMaterial = (string)t.MaterialName;
                            prt6.ProdReportMaterialCategoryID = (int)t.ProductionReportMaterialCategoryID;
                            if ((int)t.ContentFieldID == 1)
                            {
                                prt6.Volume = (float)t.Value;
                            }
                            else if ((int)t.ContentFieldID == 2)
                            {
                                prt6.Weight = (float)t.Value;
                            }
                            else if ((int)t.ContentFieldID == 3)
                            {
                                prt6.Volume = (float)t.Value;
                            }
                            else if ((int)t.ContentFieldID == 4)
                            {
                                prt6.Weight = (float)t.Value;
                            }
                            prodReportPart6List.Add(prt6);
                        }
                        else // case where Material already exists
                        {
                            if ((int)t.ContentFieldID == 1)
                            {
                                mater.Volume += (float)t.Value;
                            }
                            else if ((int)t.ContentFieldID == 2)
                            {
                                mater.Weight += (float)t.Value;
                            }
                            else if ((int)t.ContentFieldID == 3)
                            {
                                mater.Volume += (float)t.Value;
                            }
                            else if ((int)t.ContentFieldID == 4)
                            {
                                mater.Weight += (float)t.Value;
                            }
                        } 
                    }
                }

                // we need this here to fill up ProdReportMaterialCategoryID
                foreach (var mat in prodReportPart6List)
                {
                    if(mat.ProdReportMaterialCategoryID == 0)
                    {
                        var k =
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
                        if (k != null)
                        {
                            if (k.ProductionReportMaterialCategoryID != null)
                            {
                                mat.ProdReportMaterialCategoryID = (int)k.ProductionReportMaterialCategoryID;
                            }
                        }
                    } 
                }
            }
            catch(Exception e)
            {
                throw;
            }

            prodRepObj.ProdReportPart6 = prodReportPart6List;

            return prodRepObj;
        }

        /// <summary>
        /// GetSpiritsForProductionReport method gets the data for production report part 1
        /// </summary>
        /// <param name="userId">Distiller user id</param>
        /// <param name="start">reporting start period</param>
        /// <param name="end">reporting end period</param>
        /// <param name="tempRepObjList">out parameter object list that needs to be populated</param>
        public void GetSpiritsForProductionReport(int userId, DateTime start, DateTime end, ref List<ProductionReportHelper> tempRepObjList)
        {
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
                   prod.Gauged == true &&
                   (prod.StatusID == 1 ||
                   prod.StatusID == 2) &&
                   (new int[] { 3, 4, 5 }).Contains(prod.StateID) &&
                   prod.ProductionEndTime >= start &&
                   prod.ProductionEndTime <= end
                 select new
                 {
                     prod.StateID,
                     ProductionID = (int?)prod.ProductionID,
                     SpiritTypeName = spiritTypeRep.ProductTypeName ?? "",
                     Redistilled = (bool?)prodReport.Redistilled,
                     MaterialKindName = matKindRep.MaterialKindName ?? "",
                     Weight = (System.Single?)prodReport.Weight ?? (System.Single?)0,
                     Volume = (System.Single?)prodReport.Volume ?? (System.Single?)0,
                     Alcohol = (System.Single?)prodReport.Alcohol ?? (System.Single?)0,
                     Proof = (System.Single?)prodReport.Proof ?? (System.Single?)0,
                     SpiritTypeReportingID = (int?)spiritTypeRep.SpiritTypeReportingID ?? (int?)0,
                     MaterialKindReportingID = (int?)matKindRep.MaterialKindReportingID ?? (int?)0,
                     MaterialCategoryName = prodRepMatCat.MaterialCategoryName ?? "",
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
                            }
                            catch (Exception e)
                            {
                                throw;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw;
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
                   (prod.StatusID == 1 ||
                   prod.StatusID == 2) &&
                   (new int[] { 3, 4, 5 }).Contains(prod.StateID) &&
                   prod.ProductionEndTime >= start &&
                   prod.ProductionEndTime <= end
                 select new
                 {
                     prod.StateID,
                     ProductionID = (int?)prod.ProductionID,
                     SpiritTypeName = spiritTypeRep.ProductTypeName ?? "",
                     Redistilled = (bool?)prodReport.Redistilled,
                     MaterialKindName = matKindRep.MaterialKindName ?? "",
                     Weight = (System.Single?)prodReport.Weight ?? (System.Single?)0,
                     Volume = (System.Single?)prodReport.Volume ?? (System.Single?)0,
                     Alcohol = (System.Single?)prodReport.Alcohol ?? (System.Single?)0,
                     Proof = (System.Single?)prodReport.Proof ?? (System.Single?)0,
                     SpiritTypeReportingID = (int?)spiritTypeRep.SpiritTypeReportingID ?? (int?)0,
                     MaterialKindReportingID = (int?)matKindRep.MaterialKindReportingID ?? (int?)0,
                     MaterialCategoryName = prodRepMatCat.MaterialCategoryName ?? "",
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
                                throw;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw;
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

                GetProducedOnHandAtStart(startDate, endDate, userId, ref storageReportBody);
                GetPurchasedOnHandAtStart(startDate, endDate, userId, ref storageReportBody);
                GetProducedDepositedToStorage(startDate, endDate, userId, ref storageReportBody);
                GetPurchasedDepositedToStorage(startDate, endDate, userId, ref storageReportBody);
                GetProducedStorageToProduction(startDate, endDate, userId, ref storageReportBody);
                GetPurchasedStorageToProduction(startDate, endDate, userId, ref storageReportBody);
                GetProducedStorageToProcessing(startDate, endDate, userId, ref storageReportBody);
                GetPurchasedStorageToProcessing(startDate, endDate, userId, ref storageReportBody);

                storageReport.ReportBody = storageReportBody;

                return storageReport;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private void GetProducedOnHandAtStart(DateTime startDate, DateTime endDate, int userId, ref List<StorageReportCategory> storageReportBody)
        {
            // Query distilled production records transferred to storage account
            var records =
                from rec in db.Production
                join proof in db.Proof on rec.ProofID equals proof.ProofID into proof_join
                from proof in proof_join.DefaultIfEmpty()
                join distiller in db.AspNetUserToDistiller on rec.DistillerID equals distiller.DistillerID into distiller_join
                from distiller in distiller_join.DefaultIfEmpty()
                join p2str in db.ProductionToSpiritTypeReporting on rec.ProductionID equals p2str.ProductionID into p2str_join
                from p2str in p2str_join.DefaultIfEmpty()
                join str in db.SpiritTypeReporting on p2str.SpiritTypeReportingID equals str.SpiritTypeReportingID into str_join
                from str in str_join.DefaultIfEmpty()
                where
                    distiller.UserId == userId &&
                    rec.ProductionTypeID == 2 &&
                    (rec.StatusID == 1 ||
                    rec.StatusID == 2 ||
                    rec.StateID == 3) &&
                    rec.ProductionEndTime < startDate &&
                    rec.Gauged == true
                select new
                {
                    reportingCategoryName = str.ProductTypeName,
                    proof = proof.Value
                };

            if (records != null)
            {
                foreach (var rec in records)
                {
                    // Search for existing category with matching name
                    var category = storageReportBody.Find(x => x.CategoryName == rec.reportingCategoryName);

                    if (category == null)
                    {
                        // Add category to the list with given produced distilled batch ReportingCategoryName and update relevant rows
                        StorageReportCategory cat = new StorageReportCategory();
                        cat.CategoryName = rec.reportingCategoryName;
                        cat.r1_OnHandFirstOfMonth += (float)rec.proof;
                        storageReportBody.Add(cat);
                    }
                    else
                    {
                        category.r1_OnHandFirstOfMonth += (float)rec.proof;
                    }
                }
            }
        }

        private void GetPurchasedOnHandAtStart(DateTime startDate, DateTime endDate, int userId, ref List<StorageReportCategory> storageReportBody)
        {
            // Query distilled purchase records transferred to storage account
            var records =
                from rec in db.Purchase
                join proof in db.Proof on rec.ProofID equals proof.ProofID into proof_join
                from proof in proof_join.DefaultIfEmpty()
                join distiller in db.AspNetUserToDistiller on rec.DistillerID equals distiller.DistillerID into distiller_join
                from distiller in distiller_join.DefaultIfEmpty()
                join p2str in db.PurchaseToSpiritTypeReporting on rec.PurchaseID equals p2str.PurchaseID into p2str_join
                from p2str in p2str_join.DefaultIfEmpty()
                join str in db.SpiritTypeReporting on p2str.SpiritTypeReportingID equals str.SpiritTypeReportingID into str_join
                from str in str_join.DefaultIfEmpty()
                where
                    distiller.UserId == userId &&
                    rec.PurchaseTypeID == 2 &&
                    rec.PurchaseTypeID == 3 &&
                    (rec.StatusID == 1 ||
                    rec.StatusID == 2 ||
                    rec.StatusID == 3) &&
                    rec.PurchaseDate < startDate
                select new
                {
                    reportingCategoryName = str.ProductTypeName,
                    proof = proof.Value
                };

            if (records != null)
            {
                foreach (var rec in records)
                {
                    // Search for existing category with matching name
                    var category = storageReportBody.Find(x => x.CategoryName == rec.reportingCategoryName);

                    if (category == null)
                    {
                        // Add category to the list with given produced distilled batch ReportingCategoryName and update relevant rows
                        StorageReportCategory cat = new StorageReportCategory();
                        cat.CategoryName = rec.reportingCategoryName;
                        cat.r1_OnHandFirstOfMonth += (float)rec.proof;
                        storageReportBody.Add(cat);
                    }
                    else
                    {
                        category.r1_OnHandFirstOfMonth += (float)rec.proof;
                    }
                }
            }
        }

        private void GetProducedDepositedToStorage(DateTime startDate, DateTime endDate, int userId, ref List<StorageReportCategory> storageReportBody)
        {
            // Query distilled production records transferred to storage account
            var records =
                from rec in db.Production
                join proof in db.Proof on rec.ProofID equals proof.ProofID into proof_join
                from proof in proof_join.DefaultIfEmpty()
                join distiller in db.AspNetUserToDistiller on rec.DistillerID equals distiller.DistillerID into distiller_join
                from distiller in distiller_join.DefaultIfEmpty()
                join p2str in db.ProductionToSpiritTypeReporting on rec.ProductionID equals p2str.ProductionID into p2str_join
                from p2str in p2str_join.DefaultIfEmpty()
                join str in db.SpiritTypeReporting on p2str.SpiritTypeReportingID equals str.SpiritTypeReportingID into str_join
                from str in str_join.DefaultIfEmpty()
                where
                    distiller.UserId == userId &&
                    rec.ProductionTypeID == 2 &&
                    (rec.StatusID == 1 ||
                    rec.StatusID == 2) &&
                    rec.ProductionEndTime >= startDate &&
                    rec.ProductionEndTime <= endDate &&
                    rec.Gauged == true
                select new
                {
                    reportingCategoryName = str.ProductTypeName,
                    proof = proof.Value
                };

            if (records != null)
            {
                foreach (var rec in records)
                {
                    // Search for existing category with matching name
                    var category = storageReportBody.Find(x => x.CategoryName == rec.reportingCategoryName);

                    if (category == null)
                    {
                        // Add category to the list with given produced distilled batch ReportingCategoryName and update relevant rows
                        StorageReportCategory cat = new StorageReportCategory();
                        cat.CategoryName = rec.reportingCategoryName;
                        cat.r2_DepositedInBulkStorage += (float)rec.proof;
                        storageReportBody.Add(cat);
                    }
                    else
                    {
                        category.r2_DepositedInBulkStorage += (float)rec.proof;
                    }
                }
            }
        }

        private void GetPurchasedDepositedToStorage(DateTime startDate, DateTime endDate, int userId, ref List<StorageReportCategory> storageReportBody)
        {
            // Query distilled purchase records transferred to storage account
            var records =
                from rec in db.Purchase
                join proof in db.Proof on rec.ProofID equals proof.ProofID into proof_join
                from proof in proof_join.DefaultIfEmpty()
                join distiller in db.AspNetUserToDistiller on rec.DistillerID equals distiller.DistillerID into distiller_join
                from distiller in distiller_join.DefaultIfEmpty()
                join p2str in db.PurchaseToSpiritTypeReporting on rec.PurchaseID equals p2str.PurchaseID into p2str_join
                from p2str in p2str_join.DefaultIfEmpty()
                join str in db.SpiritTypeReporting on p2str.SpiritTypeReportingID equals str.SpiritTypeReportingID into str_join
                from str in str_join.DefaultIfEmpty()
                where
                    distiller.UserId == userId &&
                    rec.PurchaseTypeID == 2 &&
                    rec.PurchaseTypeID == 3 &&
                    (rec.StatusID == 1 ||
                    rec.StatusID == 2) &&
                    rec.PurchaseDate >= startDate &&
                    rec.PurchaseDate <= endDate
                select new
                {
                    reportingCategoryName = str.ProductTypeName,
                    proof = proof.Value
                };

            if (records != null)
            {
                foreach (var rec in records)
                {
                    // Search for existing category with matching name
                    var category = storageReportBody.Find(x => x.CategoryName == rec.reportingCategoryName);

                    if (category == null)
                    {
                        // Add category to the list with given produced distilled batch ReportingCategoryName and update relevant rows
                        StorageReportCategory cat = new StorageReportCategory();
                        cat.CategoryName = rec.reportingCategoryName;
                        cat.r2_DepositedInBulkStorage += (float)rec.proof;
                        storageReportBody.Add(cat);
                    }
                    else
                    {
                        category.r2_DepositedInBulkStorage += (float)rec.proof;
                    }
                }
            }
        }

        private void GetProducedStorageToProduction(DateTime startDate, DateTime endDate, int userId, ref List<StorageReportCategory> storageReportBody)
        {
            // Query distilled production records transferred from storage account to production account
            var prodRes =
                from sourceProductionRecord in db.Production
                join alcohol in db.Alcohol on sourceProductionRecord.AlcoholID equals alcohol.AlcoholID into alcohol_join
                from alcohol in alcohol_join.DefaultIfEmpty()
                join productionContent in db.ProductionContent on sourceProductionRecord.ProductionID equals productionContent.RecordID into productionContent_join
                from productionContent in productionContent_join.DefaultIfEmpty()
                join contentField in db.ContentField on productionContent.ContentFieldID equals contentField.ContentFieldID into contentField_join
                from contentField in contentField_join.DefaultIfEmpty()
                join outputProductionRecord in db.Production on productionContent.ProductionID equals outputProductionRecord.ProductionID into outputProductionRecord_join
                from outputProductionRecord in outputProductionRecord_join.DefaultIfEmpty()
                join productionType in db.ProductionType on outputProductionRecord.ProductionTypeID equals productionType.ProductionTypeID into productionType_join
                from productionType in productionType_join.DefaultIfEmpty()
                join distiller in db.AspNetUserToDistiller on sourceProductionRecord.DistillerID equals distiller.DistillerID into distiller_join
                from distiller in distiller_join.DefaultIfEmpty()
                join p2str in db.ProductionToSpiritTypeReporting on sourceProductionRecord.ProductionID equals p2str.ProductionID into p2str_join
                from p2str in p2str_join.DefaultIfEmpty()
                join str in db.SpiritTypeReporting on p2str.SpiritTypeReportingID equals str.SpiritTypeReportingID into str_join
                from str in str_join.DefaultIfEmpty()
                where
                    distiller.UserId == userId &&
                    sourceProductionRecord.ProductionTypeID == 2 &&
                    outputProductionRecord.ProductionTypeID == 2 &&
                    (sourceProductionRecord.StatusID == 1 ||
                    sourceProductionRecord.StatusID == 2 ||
                    sourceProductionRecord.StatusID == 3) &&
                    contentField.ContentFieldID == 11 &&
                    sourceProductionRecord.ProductionEndTime < startDate &&
                    outputProductionRecord.ProductionEndTime >= startDate &&
                    outputProductionRecord.ProductionEndTime <= endDate &&
                    sourceProductionRecord.Gauged == true
                select new
                {
                    reportingCategoryName = str.ProductTypeName,
                    proofGal = (float)(productionContent.ContentValue * alcohol.Value * 2) / 100
                };

            if (prodRes != null)
            {
                foreach (var rec in prodRes)
                {
                    // Search for existing category with matching name
                    var category = storageReportBody.Find(x => x.CategoryName == rec.reportingCategoryName);

                    if (category == null)
                    {
                        // Add category to the list with given produced distilled batch ReportingCategoryName and update relevant rows
                        StorageReportCategory cat = new StorageReportCategory();
                        cat.CategoryName = rec.reportingCategoryName;
                        cat.r18_TransferredToProductionAccount += (float)rec.proofGal;
                        storageReportBody.Add(cat);
                    }
                    else
                    {
                        category.r18_TransferredToProductionAccount += (float)rec.proofGal;
                    }
                }
            }
        }

        private void GetPurchasedStorageToProduction(DateTime startDate, DateTime endDate, int userId, ref List<StorageReportCategory> storageReportBody)
        {
            // Query distilled purchase records to transfer from storage account to production account
            var purRes =
                from sourcePurchaseRecord in db.Purchase
                join alcohol in db.Alcohol on sourcePurchaseRecord.AlcoholID equals alcohol.AlcoholID into alcohol_join
                from alcohol in alcohol_join.DefaultIfEmpty()
                join productionContent in db.ProductionContent on sourcePurchaseRecord.PurchaseID equals productionContent.RecordID into productionContent_join
                from productionContent in productionContent_join.DefaultIfEmpty()
                join contentField in db.ContentField on productionContent.ContentFieldID equals contentField.ContentFieldID into contentField_join
                from contentField in contentField_join.DefaultIfEmpty()
                join outputProductionRecord in db.Production on productionContent.ProductionID equals outputProductionRecord.ProductionID into outputProductionRecord_join
                from outputProductionRecord in outputProductionRecord_join.DefaultIfEmpty()
                join productionType in db.ProductionType on outputProductionRecord.ProductionTypeID equals productionType.ProductionTypeID into productionType_join
                from productionType in productionType_join.DefaultIfEmpty()
                join distiller in db.AspNetUserToDistiller on sourcePurchaseRecord.DistillerID equals distiller.DistillerID into distiller_join
                from distiller in distiller_join.DefaultIfEmpty()
                join p2str in db.PurchaseToSpiritTypeReporting on sourcePurchaseRecord.PurchaseID equals p2str.PurchaseID into p2str_join
                from p2str in p2str_join.DefaultIfEmpty()
                join str in db.SpiritTypeReporting on p2str.SpiritTypeReportingID equals str.SpiritTypeReportingID into str_join
                from str in str_join.DefaultIfEmpty()
                where
                    distiller.UserId == userId &&
                    sourcePurchaseRecord.PurchaseTypeID == 2 &&
                    sourcePurchaseRecord.PurchaseTypeID == 3 &&
                    outputProductionRecord.ProductionTypeID == 2 &&
                    (sourcePurchaseRecord.StatusID == 1 ||
                    sourcePurchaseRecord.StatusID == 2 ||
                    sourcePurchaseRecord.StatusID == 3) &&
                    sourcePurchaseRecord.PurchaseDate < startDate &&
                    outputProductionRecord.ProductionEndTime >= startDate &&
                    outputProductionRecord.ProductionEndTime <= endDate
                select new
                {
                    reportingCategoryName = str.ProductTypeName,
                    proofGal = (float)(productionContent.ContentValue * alcohol.Value * 2) / 100
                };

            if (purRes != null)
            {
                foreach (var rec in purRes)
                {
                    // Search for existing category with matching name
                    var category = storageReportBody.Find(x => x.CategoryName == rec.reportingCategoryName);

                    if (category == null)
                    {
                        // Add category to the list with given produced distilled batch ReportingCategoryName and update relevant rows
                        StorageReportCategory cat = new StorageReportCategory();
                        cat.CategoryName = rec.reportingCategoryName;
                        cat.r18_TransferredToProductionAccount += (float)rec.proofGal;
                        storageReportBody.Add(cat);
                    }
                    else
                    {
                        category.r18_TransferredToProductionAccount += (float)rec.proofGal;
                    }
                }
            }
        }

        private void GetProducedStorageToProcessing(DateTime startDate, DateTime endDate, int userId, ref List<StorageReportCategory> storageReportBody)
        {
            // Query distilled production records transferred from storage account to processing account
            var prodRes =
                from sourceProductionRecord in db.Production
                join alcohol in db.Alcohol on sourceProductionRecord.AlcoholID equals alcohol.AlcoholID into alcohol_join
                from alcohol in alcohol_join.DefaultIfEmpty()
                join productionContent in db.ProductionContent on sourceProductionRecord.ProductionID equals productionContent.RecordID into productionContent_join
                from productionContent in productionContent_join.DefaultIfEmpty()
                join contentField in db.ContentField on productionContent.ContentFieldID equals contentField.ContentFieldID into contentField_join
                from contentField in contentField_join.DefaultIfEmpty()
                join outputProductionRecord in db.Production on productionContent.ProductionID equals outputProductionRecord.ProductionID into outputProductionRecord_join
                from outputProductionRecord in outputProductionRecord_join.DefaultIfEmpty()
                join productionType in db.ProductionType on outputProductionRecord.ProductionTypeID equals productionType.ProductionTypeID into productionType_join
                from productionType in productionType_join.DefaultIfEmpty()
                join distiller in db.AspNetUserToDistiller on sourceProductionRecord.DistillerID equals distiller.DistillerID into distiller_join
                from distiller in distiller_join.DefaultIfEmpty()
                join p2str in db.ProductionToSpiritTypeReporting on sourceProductionRecord.ProductionID equals p2str.ProductionID into p2str_join
                from p2str in p2str_join.DefaultIfEmpty()
                join str in db.SpiritTypeReporting on p2str.SpiritTypeReportingID equals str.SpiritTypeReportingID into str_join
                from str in str_join.DefaultIfEmpty()
                where
                    distiller.UserId == userId &&
                    sourceProductionRecord.ProductionTypeID == 2 &&
                    outputProductionRecord.ProductionTypeID == 3 &&
                    (sourceProductionRecord.StatusID == 1 ||
                    sourceProductionRecord.StatusID == 2 ||
                    sourceProductionRecord.StatusID == 3) &&
                    contentField.ContentFieldID == 11 &&
                    sourceProductionRecord.ProductionEndTime < startDate &&
                    outputProductionRecord.ProductionEndTime >= startDate &&
                    outputProductionRecord.ProductionEndTime <= endDate
                select new
                {
                    reportingCategoryName = str.ProductTypeName,
                    proofGal = (float)(productionContent.ContentValue * alcohol.Value * 2) / 100
                };

            if (prodRes != null)
            {
                foreach (var rec in prodRes)
                {
                    // Search for existing category with matching name
                    var category = storageReportBody.Find(x => x.CategoryName == rec.reportingCategoryName);

                    if (category == null)
                    {
                        // Add category to the list with given produced distilled batch ReportingCategoryName and update relevant rows
                        StorageReportCategory cat = new StorageReportCategory();
                        cat.CategoryName = rec.reportingCategoryName;
                        cat.r17_TransferredToProcessingAccount += (float)rec.proofGal;
                        storageReportBody.Add(cat);
                    }
                    else
                    {
                        category.r17_TransferredToProcessingAccount += (float)rec.proofGal;
                    }
                }
            }
        }

        private void GetPurchasedStorageToProcessing(DateTime startDate, DateTime endDate, int userId, ref List<StorageReportCategory> storageReportBody)
        {
            // Query distilled purchase records transferred from storage account to processing account
            var purRes =
                from sourcePurchaseRecord in db.Purchase
                join alcohol in db.Alcohol on sourcePurchaseRecord.AlcoholID equals alcohol.AlcoholID into alcohol_join
                from alcohol in alcohol_join.DefaultIfEmpty()
                join productionContent in db.ProductionContent on sourcePurchaseRecord.PurchaseID equals productionContent.RecordID into productionContent_join
                from productionContent in productionContent_join.DefaultIfEmpty()
                join contentField in db.ContentField on productionContent.ContentFieldID equals contentField.ContentFieldID into contentField_join
                from contentField in contentField_join.DefaultIfEmpty()
                join outputProductionRecord in db.Production on productionContent.ProductionID equals outputProductionRecord.ProductionID into outputProductionRecord_join
                from outputProductionRecord in outputProductionRecord_join.DefaultIfEmpty()
                join productionType in db.ProductionType on outputProductionRecord.ProductionTypeID equals productionType.ProductionTypeID into productionType_join
                from productionType in productionType_join.DefaultIfEmpty()
                join distiller in db.AspNetUserToDistiller on sourcePurchaseRecord.DistillerID equals distiller.DistillerID into distiller_join
                from distiller in distiller_join.DefaultIfEmpty()
                join p2str in db.PurchaseToSpiritTypeReporting on sourcePurchaseRecord.PurchaseID equals p2str.PurchaseID into p2str_join
                from p2str in p2str_join.DefaultIfEmpty()
                join str in db.SpiritTypeReporting on p2str.SpiritTypeReportingID equals str.SpiritTypeReportingID into str_join
                from str in str_join.DefaultIfEmpty()
                where
                    distiller.UserId == userId &&
                    sourcePurchaseRecord.PurchaseTypeID == 3 &&
                    outputProductionRecord.ProductionTypeID == 3 &&
                    (sourcePurchaseRecord.StatusID == 1 ||
                    sourcePurchaseRecord.StatusID == 2 ||
                    sourcePurchaseRecord.StatusID == 3) &&
                    sourcePurchaseRecord.PurchaseDate < startDate &&
                    outputProductionRecord.ProductionEndTime >= startDate &&
                    outputProductionRecord.ProductionEndTime <= endDate
                select new
                {
                    reportingCategoryName = str.ProductTypeName,
                    proofGal = (float)(productionContent.ContentValue * alcohol.Value * 2) / 100
                };

            if (purRes != null)
            {
                foreach (var rec in purRes)
                {
                    // Search for existing category with matching name
                    var category = storageReportBody.Find(x => x.CategoryName == rec.reportingCategoryName);

                    if (category == null)
                    {
                        // Add category to the list with given produced distilled batch ReportingCategoryName and update relevant rows
                        StorageReportCategory cat = new StorageReportCategory();
                        cat.CategoryName = rec.reportingCategoryName;
                        cat.r17_TransferredToProcessingAccount += (float)rec.proofGal;
                        storageReportBody.Add(cat);
                    }
                    else
                    {
                        category.r17_TransferredToProcessingAccount += (float)rec.proofGal;
                    }
                }
            }
        }

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
            catch(Exception e)
            {
                throw;
            }
        }

        /// <summary>
        /// GetaterialCategoryList Method queries ProductionReportMaterialCategory table for list of Categories eg. Grain, Fruit, etc
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
                Debug.WriteLine("Error getting the list of Material Categories: " + e);
                throw;
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
                join prod in db.Production on new { BlendedRecordId = gainloss.BlendedRecordId } equals new { BlendedRecordId = prod.ProductionID }
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

                IEnumerable< Production> queryProduction =
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
            }
            catch (DbUpdateException e)
            {
                throw e;
            }
            catch (DbEntityValidationException e)
            {
                throw e;
            }
            catch (NotSupportedException e)
            {
                throw e;
            }
            catch (ObjectDisposedException e)
            {
                throw e;
            }
            catch (InvalidOperationException e)
            {
                throw e;
            }
            return retMthdExecResult;
        }
        #endregion
    }
}