using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using WebApp.Models;
using WebApp.Persistence.BusinessLogicEnums;

namespace WebApp.Helpers
{
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
        private readonly DistilDBContext _db;

        public DataLayer(DistilDBContext db)
        {
            _db = db;
        }

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

        #region Shared Methods

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
                    from spiType in _db.SpiritTypeReporting
                    join spiT2Mat in _db.SpiritType2MaterialKindReporting on spiType.SpiritTypeReportingID equals spiT2Mat.SpiritTypeReportingID into spiT2Mat_join
                    from spiT2Mat in spiT2Mat_join.DefaultIfEmpty()
                    join matKind in _db.MaterialKindReporting on spiT2Mat.MaterialKindReportingID equals matKind.MaterialKindReportingID into matKind_join
                    from matKind in matKind_join.DefaultIfEmpty()
                    where spiType.SpiritTypeReportingID != (int)ReportSpiritTypes.Total
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
                throw e;
            }

            return spir2KindList;
        }

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
            var last = (from rec in _db.GaugeSerial
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
            _db.GaugeSerial.Add(serialRec);
            _db.SaveChanges();
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
            var res = (from rec in _db.GaugeSerial
                       join production in _db.Production on rec.RecordID equals production.ProductionID into production_join
                       from production in production_join.DefaultIfEmpty()
                       join purchase in _db.Purchase on rec.RecordID equals purchase.PurchaseID into purchase_join
                       from purchase in purchase_join.DefaultIfEmpty()
                       where (rec.RecordType == (int)RecordType.Purchase &&
                             purchase.PurchaseDate > start &&
                             purchase.PurchaseDate < end)
                             || (rec.RecordType == (int)RecordType.Production &&
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
            var res = (from rec in _db.GaugeSerial
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
                from purch in _db.Purchase
                join quant in _db.Volume on purch.VolumeID equals quant.VolumeID into quant_join
                from quant in quant_join.DefaultIfEmpty()
                join vbw in _db.Weight on purch.WeightID equals vbw.WeightID into vbw_join
                from vbw in vbw_join.DefaultIfEmpty()
                join matDic in _db.MaterialDict on purch.MaterialDictID equals matDic.MaterialDictID into matDic_join
                from matDic in matDic_join.DefaultIfEmpty()
                join distiller in _db.AspNetUserToDistiller on new { DistillerID = purch.DistillerID } equals new { DistillerID = distiller.DistillerID } into distiller_join
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
                    from pur in _db.Purchase
                    join qty in _db.Volume on pur.VolumeID equals qty.VolumeID into qty_join
                    from qty in qty_join.DefaultIfEmpty()
                    join vbw in _db.Weight on pur.WeightID equals vbw.WeightID into vbw_join
                    from vbw in vbw_join.DefaultIfEmpty()
                    join alc in _db.Alcohol on pur.AlcoholID equals alc.AlcoholID into alc_join
                    from alc in alc_join.DefaultIfEmpty()
                    join distiller in _db.AspNetUserToDistiller on pur.DistillerID equals distiller.DistillerID into distiller_join
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
                    from prod in _db.Production
                    join qty in _db.Volume on prod.VolumeID equals qty.VolumeID into qty_join
                    from qty in qty_join.DefaultIfEmpty()
                    join vbw in _db.Weight on prod.WeightID equals vbw.WeightID into vbw_join
                    from vbw in vbw_join.DefaultIfEmpty()
                    join alc in _db.Alcohol on prod.AlcoholID equals alc.AlcoholID into alc_join
                    from alc in alc_join.DefaultIfEmpty()
                    join distiller in _db.AspNetUserToDistiller on prod.DistillerID equals distiller.DistillerID into distiller_join
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
                 (from purch in _db.Purchase
                  join distillers in _db.AspNetUserToDistiller on purch.DistillerID equals distillers.DistillerID into distillers_join
                  from distillers in distillers_join.DefaultIfEmpty()
                  join quant in _db.Volume on purch.VolumeID equals quant.VolumeID into quant_join
                  from quant in quant_join.DefaultIfEmpty()
                  join alc in _db.Alcohol on purch.AlcoholID equals alc.AlcoholID into alc_join
                  from alc in alc_join.DefaultIfEmpty()
                  join vbw in _db.Weight on purch.WeightID equals vbw.WeightID into vbw_join
                  from vbw in vbw_join.DefaultIfEmpty()
                  where
                   (purch.StatusID == (int)Persistence.BusinessLogicEnums.Status.Active ||
                   purch.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processing) &&
                   purch.StateID == (int)Persistence.BusinessLogicEnums.State.Distilled &&
                   distillers.UserId == userId
                  select new
                  {
                      AlcoholContent = ((System.Single?)alc.Value ?? (System.Single?)0),
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
                        prodO.AlcoholContent = (float)i.AlcoholContent;
                        list.Add(prodO);
                        combinedId++;
                    }
                }

                var res1 =
                   (from prod in _db.Production
                    join distillers in _db.AspNetUserToDistiller on new { DistillerID = prod.DistillerID } equals new { DistillerID = distillers.DistillerID } into distillers_join
                    from distillers in distillers_join.DefaultIfEmpty()
                    join quant in _db.Volume on prod.VolumeID equals quant.VolumeID into quant_join
                    from quant in quant_join.DefaultIfEmpty()
                    join alc in _db.Alcohol on prod.AlcoholID equals alc.AlcoholID into alc_join
                    from alc in alc_join.DefaultIfEmpty()
                    join vbw in _db.Weight on prod.WeightID equals vbw.WeightID into vbw_join
                    from vbw in vbw_join.DefaultIfEmpty()
                    where
                      (prod.StatusID == (int)Persistence.BusinessLogicEnums.Status.Active ||
                      prod.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processing) &&
                      prod.StateID == (int)Persistence.BusinessLogicEnums.State.Distilled &&
                      distillers.UserId == userId
                    select new
                    {
                        AlcoholContent = ((System.Single?)alc.Value ?? (System.Single?)0),
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
                        prodO.AlcoholContent = (float)i.AlcoholContent;
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
            from Mats in _db.MaterialDict
            join distillers in _db.AspNetUserToDistiller on Mats.DistillerID equals distillers.DistillerID into distillers_join
            from distillers in distillers_join
            join MatsType in _db.MaterialType on Mats.MaterialDictID equals MatsType.MaterialDictID into MatsType_join
            from MatsType in MatsType_join.DefaultIfEmpty()
            join units in _db.UnitOfMeasurement on Mats.UnitOfMeasurementID equals units.UnitOfMeasurementID into units_join
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
                    (from i in _db.Spirit
                     join distillers in _db.AspNetUserToDistiller on i.DistillerID equals distillers.DistillerID into distillers_join
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
        /// GetVendorData queries db for list of Vendors and their associated id's
        /// </summary>
        /// <returns>List<VendorObject></returns>
        public List<VendorObject> GetVendorData(int userId)
        {
            var vendorList = new List<VendorObject>();

            try
            {
                var VendorFinalResults =
                    from ven in _db.Vendor
                    join dslrs in _db.AspNetUserToDistiller on ven.DistillerID equals dslrs.DistillerID into dslrs_join
                    from dslrs in dslrs_join.DefaultIfEmpty()
                    join venD in _db.VendorDetail on ven.VendorID equals venD.VendorID into venD_join
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
                var storageTable = _db.Storage.ToList();
                var storages =
                    (from storS in _db.Storage
                     join dslrs in _db.AspNetUserToDistiller on storS.DistillerID equals dslrs.DistillerID into dslrs_join
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
                    (from spiritCut in _db.SpiritCut
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

        // todo: technical debt: this method can be part of the base class from which workflows can inherit and override it's behavior.
        // the reason is this method is used in both Purchase and Production workflows
        public List<SpiritToKindListObject> GetReportingSpiritTypes()
        {
            List<SpiritToKindListObject> stList = new List<SpiritToKindListObject>();
            try
            {
                var str =
                    from st in _db.SpiritTypeReporting
                    where st.SpiritTypeReportingID != (int)ReportSpiritTypes.Total
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
        /// Retrieves a list of processing report types
        /// </summary>
        ///  <param name="userId"></param>
        /// <returns>List<SpiritObject></returns>
        public List<ProcessingReportTypeObject> GetProcessingReportTypes()
        {
            List<ProcessingReportTypeObject> types = new List<ProcessingReportTypeObject>();

            try
            {
                var res = from rec in _db.ProcessingReportType
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
            int distillerId = (from rec in _db.AspNetUserToDistiller
                               where rec.UserId == userId
                               select rec.DistillerID).FirstOrDefault();
            return distillerId;
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

                _db.PurchaseHistory.Add(purH);
                _db.SaveChanges();
            }
            catch (Exception e)
            {
                retMthdExecResult = false;
                throw e;
            }

            return retMthdExecResult;
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
                        from rec in _db.Purchase
                        join dslrs in _db.AspNetUserToDistiller on rec.DistillerID equals dslrs.DistillerID into dslrs_join
                        from dslrs in dslrs_join.DefaultIfEmpty()
                        join qty in _db.Volume on rec.VolumeID equals qty.VolumeID into qty_join
                        from qty in qty_join.DefaultIfEmpty()
                        join vbw in _db.Weight on rec.WeightID equals vbw.WeightID into vbw_join
                        from vbw in vbw_join.DefaultIfEmpty()
                        join alc in _db.Alcohol on rec.AlcoholID equals alc.AlcoholID into alc_join
                        from alc in alc_join.DefaultIfEmpty()
                        join prf in _db.Proof on rec.ProofID equals prf.ProofID into prf_join
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
                            (from rec in _db.Status
                             where rec.Name == "Destroyed"
                             select rec).FirstOrDefault();
                        var prodRec =
                            (from rec in _db.Purchase
                             where rec.PurchaseID == recordId
                             select rec).FirstOrDefault();
                        if (prodRec != null)
                        {
                            prodRec.StatusID = status.StatusID;
                            _db.SaveChanges();
                        }
                    }
                }
                else if (destructionObject.WorkflowType == "Production")
                {
                    var res =
                         from rec in _db.Production
                         join dslrs in _db.AspNetUserToDistiller on rec.DistillerID equals dslrs.DistillerID into dslrs_join
                         from dslrs in dslrs_join.DefaultIfEmpty()
                         join qty in _db.Volume on rec.VolumeID equals qty.VolumeID into qty_join
                         from qty in qty_join.DefaultIfEmpty()
                         join vbw in _db.Weight on rec.WeightID equals vbw.WeightID into vbw_join
                         from vbw in vbw_join.DefaultIfEmpty()
                         join alc in _db.Alcohol on rec.AlcoholID equals alc.AlcoholID into alc_join
                         from alc in alc_join.DefaultIfEmpty()
                         join prf in _db.Proof on rec.ProofID equals prf.ProofID into prf_join
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
                            (from rec in _db.Status
                             where rec.Name == "Destroyed"
                             select rec).FirstOrDefault();
                        var prodRec =
                            (from rec in _db.Production
                             where rec.ProductionID == recordId
                             select rec).FirstOrDefault();
                        if (prodRec != null)
                        {
                            prodRec.StatusID = status.StatusID;
                            _db.SaveChanges();
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
                _db.Destruction.Add(destrObj);
                _db.SaveChanges();
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
                    (from rec in _db.Volume
                     where rec.VolumeID == QuantityGalID
                     select rec).FirstOrDefault();
                if (qtyRec != null)
                {
                    qtyRec.Value = 0;
                    _db.SaveChanges();
                }
                var vbwRec =
                    (from rec in _db.Weight
                     where rec.WeightID == VolumeByWeightLBID
                     select rec).FirstOrDefault();
                if (vbwRec != null)
                {
                    vbwRec.Value = 0;
                    _db.SaveChanges();
                }
                var alcRec =
                    (from rec in _db.Alcohol
                     where rec.AlcoholID == AlcoholID
                     select rec).FirstOrDefault();
                if (alcRec != null)
                {
                    alcRec.Value = 0;
                    _db.SaveChanges();
                }
                var prfRec =
                    (from rec in _db.Proof
                     where rec.ProofID == ProofID
                     select rec).FirstOrDefault();
                if (prfRec != null)
                {
                    prfRec.Value = 0;
                    _db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        /// <summary>
        /// Retrieves destroyed purchased batches
        /// </summary>
        public List<DestructionObject> GetDestroyedBatches(int userId)
        {
            List<DestructionObject> destroyedList = new List<DestructionObject>();

            var prodRes =
                from destrRec in _db.Destruction
                join prodRec in _db.Production on destrRec.RecordID equals prodRec.ProductionID into prodRec_join
                from prodRec in prodRec_join.DefaultIfEmpty()
                join dstlrs in _db.AspNetUserToDistiller on prodRec.DistillerID equals dstlrs.DistillerID into dstlrs_join
                from dstlrs in dstlrs_join.DefaultIfEmpty()
                join prodType in _db.ProductionType on prodRec.ProductionTypeID equals prodType.ProductionTypeID into prodType_join
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
                from destrRec in _db.Destruction
                join purRec in _db.Purchase on destrRec.RecordID equals purRec.PurchaseID into purRec_join
                from purRec in purRec_join.DefaultIfEmpty()
                join dstlrs in _db.AspNetUserToDistiller on purRec.DistillerID equals dstlrs.DistillerID into dstlrs_join
                from dstlrs in dstlrs_join.DefaultIfEmpty()
                join purType in _db.PurchaseType on purRec.PurchaseTypeID equals purType.PurchaseTypeID into purType_join
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

        public ReportHeader GetDistillerInfoForReportHeader(int distillerID, DateTime startDate)
        {
            try
            {
                ReportHeader header = new ReportHeader();

                var res =
                    (from distT in _db.Distiller
                     join distDT in _db.DistillerDetail on distT.DistillerID equals distDT.DistillerID
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
                (from prod4Reporting in _db.Production4Reporting
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
        /// Query ProductionReportMaterialCategory table for list of Categories eg. Grain, Fruit, etc
        /// </summary>
        /// <returns></returns>
        public List<MaterialCategory> GetMaterialCategoryList()
        {
            List<MaterialCategory> materialCatList = new List<MaterialCategory>();
            try
            {
                var results = (from rec in _db.ProductionReportMaterialCategory
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
                from prod2Purch in _db.ProductionToPurchase
                join prod in _db.Production on prod2Purch.ProductionID equals prod.ProductionID
                join us2Distills in _db.AspNetUserToDistiller on new { DistillerID = prod.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId
                select prod2Purch;

                _db.ProductionToPurchase.RemoveRange(queryProductionToPurchase);

                IEnumerable<ProductionToSpiritTypeReporting> queryProd2SpiritTypeReporting =
                from prod2SpirRepo in _db.ProductionToSpiritTypeReporting
                join prod in _db.Production on prod2SpirRepo.ProductionID equals prod.ProductionID
                join us2Distills in _db.AspNetUserToDistiller on new { DistillerID = prod.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId
                select prod2SpirRepo;

                _db.ProductionToSpiritTypeReporting.RemoveRange(queryProd2SpiritTypeReporting);

                IEnumerable<Weight> queryWeight =
                from vbw in _db.Weight
                join prod in _db.Production on vbw.WeightID equals prod.WeightID
                join us2Distills in _db.AspNetUserToDistiller on new { DistillerID = prod.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId
                select vbw;

                _db.Weight.RemoveRange(queryWeight);

                IEnumerable<Volume> queryVolume =
                from volume in _db.Volume
                join prod in _db.Production on volume.VolumeID equals prod.VolumeID
                join us2Distills in _db.AspNetUserToDistiller on new { DistillerID = prod.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId
                select volume;

                _db.Volume.RemoveRange(queryVolume);

                IEnumerable<StorageToRecord> queryStorag2Record =
               from sto2Rec in _db.StorageToRecord
               join prod in _db.Production on sto2Rec.RecordId equals prod.ProductionID
               join us2Distills in _db.AspNetUserToDistiller on new { DistillerID = prod.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
               from us2Distills in us2Distills_join.DefaultIfEmpty()
               where us2Distills.UserId == userId &&
               sto2Rec.TableIdentifier == "prod"
               select sto2Rec;

                _db.StorageToRecord.RemoveRange(queryStorag2Record);

                IEnumerable<Alcohol> queryAlcohol =
                from alc in _db.Alcohol
                join prod in _db.Production on alc.AlcoholID equals prod.AlcoholID
                join us2Distills in _db.AspNetUserToDistiller on new { DistillerID = prod.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId
                select alc;

                _db.Alcohol.RemoveRange(queryAlcohol);

                IEnumerable<Proof> queryProof =
                from proof in _db.Proof
                join prod in _db.Production on proof.ProofID equals prod.ProofID
                join us2Distills in _db.AspNetUserToDistiller on new { DistillerID = prod.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId
                select proof;

                _db.Proof.RemoveRange(queryProof);

                IEnumerable<ProductionToSpirit> queryProductionToSpirit =
                from prod2Spirit in _db.ProductionToSpirit
                join prod in _db.Production on prod2Spirit.ProductionID equals prod.ProductionID
                join us2Distills in _db.AspNetUserToDistiller on new { DistillerID = prod.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId
                select prod2Spirit;

                _db.ProductionToSpirit.RemoveRange(queryProductionToSpirit);

                IEnumerable<ProductionToSpiritCut> queryProductionToSpiritCut =
                from prod2SpiritCut in _db.ProductionToSpiritCut
                join prod in _db.Production on prod2SpiritCut.ProductionID equals prod.ProductionID
                join us2Distills in _db.AspNetUserToDistiller on new { DistillerID = prod.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId
                select prod2SpiritCut;

                _db.ProductionToSpiritCut.RemoveRange(queryProductionToSpiritCut);

                IEnumerable<BlendedComponent> queryBlendedComponent =
                from blendComponent in _db.BlendedComponent
                join prod in _db.Production on blendComponent.ProductionID equals prod.ProductionID
                join us2Distills in _db.AspNetUserToDistiller on new { DistillerID = prod.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId
                select blendComponent;

                _db.BlendedComponent.RemoveRange(queryBlendedComponent);

                IEnumerable<BottlingInfo> queryBottlingInfo =
                from bottlingInfo in _db.BottlingInfo
                join prod in _db.Production on bottlingInfo.ProductionID equals prod.ProductionID
                join us2Distills in _db.AspNetUserToDistiller on new { DistillerID = prod.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId
                select bottlingInfo;

                _db.BottlingInfo.RemoveRange(queryBottlingInfo);

                IEnumerable<Production4Reporting> queryProduction4Reporting =
                from prod4Reporting in _db.Production4Reporting
                join prod in _db.Production on prod4Reporting.ProductionID equals prod.ProductionID
                join us2Distills in _db.AspNetUserToDistiller on new { DistillerID = prod.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId
                select prod4Reporting;

                _db.Production4Reporting.RemoveRange(queryProduction4Reporting);

                IEnumerable<ProductionContent> queryProductionContent =
                from prodCont in _db.ProductionContent
                join prod in _db.Production on prodCont.ProductionID equals prod.ProductionID
                join us2Distills in _db.AspNetUserToDistiller on new { DistillerID = prod.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId
                select prodCont;

                _db.ProductionContent.RemoveRange(queryProductionContent);

                IEnumerable<GainLoss> queryGainLoss =
                from gainloss in _db.GainLoss
                join prod in _db.Production on new { ProductionId = gainloss.ProductionId } equals new { ProductionId = prod.ProductionID }
                join us2Distills in _db.AspNetUserToDistiller on new { DistillerID = prod.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId
                select gainloss;

                _db.GainLoss.RemoveRange(queryGainLoss);

                IEnumerable<TaxWithdrawn> queryTaxWithdrawn =
                from taxW in _db.TaxWithdrawn
                join prod in _db.Production on taxW.ProductionID equals prod.ProductionID
                join us2Distills in _db.AspNetUserToDistiller on new { DistillerID = prod.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId
                select taxW;

                _db.TaxWithdrawn.RemoveRange(queryTaxWithdrawn);

                IEnumerable<Production> queryProduction =
                from Production in _db.Production
                join us2Distills in _db.AspNetUserToDistiller on new { DistillerID = Production.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId
                select Production;

                _db.Production.RemoveRange(queryProduction);

                #endregion // end of production tables deletion

                #region Purchase Tables
                IEnumerable<StorageToRecord> queryStorageToRecord =
                from sto2Rec in _db.StorageToRecord
                join purch in _db.Purchase on new { RecordId = sto2Rec.RecordId } equals new { RecordId = purch.PurchaseID }
                join us2Distills in _db.AspNetUserToDistiller on new { DistillerID = purch.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId &&
                  sto2Rec.TableIdentifier == "pur"
                select sto2Rec;

                _db.StorageToRecord.RemoveRange(queryStorageToRecord);

                IEnumerable<ProductionToPurchase> queryProductionToPurchase4Purchase =
                from prod2Purch in _db.ProductionToPurchase
                join purch in _db.Purchase on prod2Purch.ProductionID equals purch.PurchaseID
                join us2Distills in _db.AspNetUserToDistiller on new { DistillerID = purch.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId
                select prod2Purch;
                _db.ProductionToPurchase.RemoveRange(queryProductionToPurchase4Purchase);

                IEnumerable<Weight> queryWeight4Purchase =
               from vbw in _db.Weight
               join purch in _db.Purchase on vbw.WeightID equals purch.WeightID
               join us2Distills in _db.AspNetUserToDistiller on new { DistillerID = purch.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
               from us2Distills in us2Distills_join.DefaultIfEmpty()
               where us2Distills.UserId == userId
               select vbw;

                _db.Weight.RemoveRange(queryWeight4Purchase);

                IEnumerable<Volume> queryVolume4Purchase =
                from volume in _db.Volume
                join purch in _db.Production on volume.VolumeID equals purch.VolumeID
                join us2Distills in _db.AspNetUserToDistiller on new { DistillerID = purch.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId
                select volume;

                _db.Volume.RemoveRange(queryVolume4Purchase);

                IEnumerable<Alcohol> queryAlcohol4Purchase =
                from alc in _db.Alcohol
                join purch in _db.Purchase on alc.AlcoholID equals purch.AlcoholID
                join us2Distills in _db.AspNetUserToDistiller on new { DistillerID = purch.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId
                select alc;

                _db.Alcohol.RemoveRange(queryAlcohol4Purchase);

                IEnumerable<Proof> queryProof4Purchase =
                from proof in _db.Proof
                join purch in _db.Purchase on proof.ProofID equals purch.ProofID
                join us2Distills in _db.AspNetUserToDistiller on new { DistillerID = purch.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId
                select proof;

                _db.Proof.RemoveRange(queryProof4Purchase);

                IEnumerable<Purchase4Reporting> queryPurchase4Reporting =
                from purch4Reporting in _db.Purchase4Reporting
                join purch in _db.Purchase on purch4Reporting.PurchaseID equals purch.PurchaseID
                join us2Distills in _db.AspNetUserToDistiller on new { DistillerID = purch.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId
                select purch4Reporting;

                _db.Purchase4Reporting.RemoveRange(queryPurchase4Reporting);

                IEnumerable<Purchase> queryPurchase =
                from purch in _db.Purchase
                join us2Distills in _db.AspNetUserToDistiller on new { DistillerID = purch.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                from us2Distills in us2Distills_join.DefaultIfEmpty()
                where us2Distills.UserId == userId
                select purch;

                _db.Purchase.RemoveRange(queryPurchase);
                #endregion

                _db.SaveChanges();

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
}