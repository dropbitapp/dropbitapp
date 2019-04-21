using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WebApp.Helpers;
using WebApp.Models;

namespace WebApp.Workflows
{
    public class DictionaryWorkflow
    {
        private readonly DistilDBContext _db;
        private readonly DataLayer _dl;

        public DictionaryWorkflow(DistilDBContext db, DataLayer dl)
        {
            _db = db;
            _dl = dl;
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
                var recs = _db.UnitOfMeasurement.ToList();
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
        /// CreateSpirit Method inserts new record in Spirit table
        /// </summary>
        /// <param name="spiritObject"></param>
        /// <returns>int</returns>
        public int CreateSpirit(int userId, SpiritObject spiritObject)
        {

            var distillerId = _dl.GetDistillerId(userId);

            //define method execution return value to be false by default
            var retMthdExecResult = 0;

            if (spiritObject != null)
            {
                try
                {
                    Spirit tbl = new Spirit();
                    tbl.Name = spiritObject.SpiritName;
                    tbl.ProcessingReportTypeID = spiritObject.ProcessingReportTypeID;
                    tbl.DistillerID = distillerId;
                    if (spiritObject.Note != string.Empty && spiritObject.Note != null)
                    {
                        tbl.Note = spiritObject.Note;
                    }
                    _db.Spirit.Add(tbl);
                    _db.SaveChanges();
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
            int distillerID = _dl.GetDistillerId(userId);
            if (vendorObject != null)
            {
                try
                {
                    Vendor tbl = new Vendor();
                    tbl.Name = vendorObject.VendorName;
                    tbl.DistillerID = distillerID;
                    _db.Vendor.Add(tbl);
                    _db.SaveChanges();

                    VendorDetail tbl1 = new VendorDetail();
                    if (vendorObject.Note != string.Empty && vendorObject.Note != null)
                    {
                        tbl1.Note = vendorObject.Note;
                    }
                    tbl1.VendorID = tbl.VendorID;
                    _db.VendorDetail.Add(tbl1);
                    _db.SaveChanges();
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
            int distillerId = _dl.GetDistillerId(userId);

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
                    _db.Storage.Add(storRec);
                    _db.SaveChanges();

                    StorageState storState = new StorageState();
                    storState.StorageID = storRec.StorageID;
                    storState.Available = true;
                    _db.StorageState.Add(storState);
                    _db.SaveChanges();
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
            int distillerId = _dl.GetDistillerId(userId);

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

                    _db.MaterialDict.Add(matDict);
                    _db.SaveChanges();

                    materialDictID = matDict.MaterialDictID;

                    // build relationships between given raw material and purchase material types
                    if (rawMObject.PurchaseMaterialTypes.Additive)
                    {
                        MaterialType matType = new MaterialType();
                        matType.MaterialDictID = materialDictID;
                        matType.Name = "Additive";
                        _db.MaterialType.Add(matType);
                        _db.SaveChanges();
                    }

                    if (rawMObject.PurchaseMaterialTypes.Distilled)
                    {
                        MaterialType matType = new MaterialType();
                        matType.MaterialDictID = materialDictID;
                        matType.Name = "Distilled";
                        _db.MaterialType.Add(matType);
                        _db.SaveChanges();
                    }

                    if (rawMObject.PurchaseMaterialTypes.Fermentable)
                    {
                        MaterialType matType = new MaterialType();
                        matType.MaterialDictID = materialDictID;
                        matType.Name = "Fermentable";
                        _db.MaterialType.Add(matType);

                        MaterialDict2MaterialCategory md2mc = new MaterialDict2MaterialCategory();
                        md2mc.MaterialDictID = materialDictID;
                        md2mc.ProductionReportMaterialCategoryID = rawMObject.MaterialCategoryID;
                        _db.MaterialDict2MaterialCategory.Add(md2mc);
                        _db.SaveChanges();
                    }

                    if (rawMObject.PurchaseMaterialTypes.Fermented)
                    {
                        MaterialType matType = new MaterialType();
                        matType.MaterialDictID = materialDictID;
                        matType.Name = "Fermented";
                        _db.MaterialType.Add(matType);

                        MaterialDict2MaterialCategory md2mc = new MaterialDict2MaterialCategory();
                        md2mc.MaterialDictID = materialDictID;
                        md2mc.ProductionReportMaterialCategoryID = rawMObject.MaterialCategoryID;
                        _db.MaterialDict2MaterialCategory.Add(md2mc);
                        _db.SaveChanges();
                    }

                    if (rawMObject.PurchaseMaterialTypes.Supply)
                    {
                        MaterialType matType = new MaterialType();
                        matType.MaterialDictID = materialDictID;
                        matType.Name = "Supply";
                        _db.MaterialType.Add(matType);
                        _db.SaveChanges();
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
            int distillerId = _dl.GetDistillerId(userId);

            if (spiritObject != null)
            {
                try
                {
                    var recs =
                        from rec in _db.Spirit
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

                    _db.SaveChanges();
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
            int distillerId = _dl.GetDistillerId(userId);

            if (vendorObject != null)
            {
                try
                {
                    var recs =
                        from rec in _db.Vendor
                        where rec.VendorID == vendorObject.VendorId && rec.DistillerID == distillerId
                        select rec;

                    var vendorItem = recs.FirstOrDefault();

                    if (vendorItem.Name != vendorObject.VendorName || vendorObject.Note != string.Empty)
                    {
                        vendorItem.Name = vendorObject.VendorName;
                        _db.SaveChanges();
                    }

                    var recs1 =
                        from rec1 in _db.VendorDetail
                        where rec1.VendorID == vendorObject.VendorId
                        select rec1;

                    var vendorItem1 = recs1.FirstOrDefault();

                    if (vendorItem1.Note != vendorObject.Note || vendorObject.Note != null)
                    {
                        vendorItem1.Note = vendorObject.Note;
                        _db.SaveChanges();
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
            int distillerId = _dl.GetDistillerId(userId);

            if (storageObject != null)
            {
                try
                {
                    var storRes =
                        from storRecord in _db.Storage
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

                    _db.SaveChanges();
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
            int distillerId = _dl.GetDistillerId(userId);

            if (rawMObject != null)
            {
                try
                {
                    materialDictID = rawMObject.RawMaterialId;

                    var ress =
                        (from rec in _db.MaterialDict
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
                    _db.SaveChanges();

                    // re-build relationships between given raw material and purchase material types
                    var res =
                        (from rec in _db.MaterialType
                         where rec.MaterialDictID == materialDictID
                         select rec);
                    if (res != null)
                    {
                        foreach (var i in res)
                        {
                            _db.MaterialType.Remove(i);
                        }
                        _db.SaveChanges();
                    }

                    if (rawMObject.PurchaseMaterialTypes.Additive)
                    {
                        try
                        {
                            MaterialType matType = new MaterialType();
                            matType.MaterialDictID = materialDictID;
                            matType.Name = "Additive";
                            _db.MaterialType.Add(matType);
                            _db.SaveChanges();
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
                            _db.MaterialType.Add(matType);
                            _db.SaveChanges();
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
                            _db.MaterialType.Add(matType);
                            _db.SaveChanges();
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
                            _db.MaterialType.Add(matType);
                            _db.SaveChanges();
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
                            _db.MaterialType.Add(matType);
                            _db.SaveChanges();
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
                    from spirit in _db.Spirit
                    join us2Distills in _db.AspNetUserToDistiller on new { DistillerID = spirit.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
                    from us2Distills in us2Distills_join.DefaultIfEmpty()
                    join reportTypes in _db.ProcessingReportType on spirit.ProcessingReportTypeID equals reportTypes.ProcessingReportTypeID into reportTypes_join
                    from reportTypes in reportTypes_join.DefaultIfEmpty()
                    where
                      us2Distills.UserId == userId
                    select new
                    {
                        SpiritID = (System.Int32?)spirit.SpiritID ?? (System.Int32?)0,
                        Name = spirit.Name ?? string.Empty,
                        ProcessingReportTypeID = (System.Int32?)reportTypes.ProcessingReportTypeID ?? (System.Int32)0,
                        ProcessingReportTypeName = reportTypes.ProcessingReportTypeName ?? string.Empty,
                        Note = spirit.Note ?? string.Empty
                    };

                foreach (var iter in recs)
                {
                    var curSpirit = new SpiritObject();
                    curSpirit.SpiritId = (int)iter.SpiritID;
                    curSpirit.SpiritName = iter.Name;
                    curSpirit.ProcessingReportTypeID = iter.ProcessingReportTypeID;
                    curSpirit.ProcessingReportTypeName = iter.ProcessingReportTypeName;
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
            int distillerId = _dl.GetDistillerId(userId);

            try
            {
                var VendorFinalResults =
                    from vendRes in _db.Vendor
                    join vendDetails in _db.VendorDetail on vendRes.VendorID equals vendDetails.VendorID into vendDetails_join
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
                    from storage in _db.Storage
                    join us2Distills in _db.AspNetUserToDistiller on new { DistillerID = storage.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
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

        /// <summary>
        /// GetRawMaterialListDict method that we use in dictionary workflows to view All Raw Materials record in Raw Material view workflow
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<RawMaterialObject> GetRawMaterialListDict(int userId)
        {
            List<RawMaterialObject> rawMaterialList = new List<RawMaterialObject>();
            var res =
                from matDict in _db.MaterialDict
                join unit in _db.UnitOfMeasurement on matDict.UnitOfMeasurementID equals unit.UnitOfMeasurementID into unit_join
                from unit in unit_join.DefaultIfEmpty()
                join us2Distills in _db.AspNetUserToDistiller on new { DistillerID = matDict.DistillerID } equals new { DistillerID = us2Distills.DistillerID } into us2Distills_join
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
                        from mattype in _db.MaterialType
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

        public ReturnObject DeleteDictionaryRecord(int userId, DeleteRecordObject deleteObject)
        {
            int RecordID = deleteObject.DeleteRecordID;
            string RecordType = deleteObject.DeleteRecordType;
            int distillerId = _dl.GetDistillerId(userId);
            ReturnObject delReturn = new ReturnObject();
            if (RecordID > 0)
            {
                try
                {
                    if (RecordType == "RawMaterial")
                    {
                        // check is material used in purchase
                        var res1 = from rec in _db.Purchase
                                   where rec.MaterialDictID == RecordID && rec.DistillerID == distillerId
                                   select rec;
                        // check is material used in blending
                        var res2 = (from rec in _db.BlendedComponent
                                    where rec.RecordId == RecordID
                                    select rec).ToList();

                        if (res1.Count() == 0 && res2.Count() == 0)
                        {
                            delReturn.ExecuteResult = DeleteRawMaterial(userId, RecordID);
                        }
                        if (res1.Count() != 0)
                        {
                            foreach (var item in res1)
                            {
                                delReturn.ExecuteMessage = item.PurchaseName;
                            }
                        }
                        else if (res2.Count() != 0)
                        {
                            string recordName;
                            foreach (var item in res2)
                            {
                                recordName =
                                        (from rec in _db.Production
                                         where rec.ProductionID == item.ProductionID && rec.DistillerID == distillerId
                                         select rec.ProductionName).FirstOrDefault();
                                delReturn.ExecuteMessage = recordName;
                            }
                        }
                    }
                    else if (RecordType == "Spirit")
                    {

                        var res =
                            from rec in _db.ProductionToSpirit
                            join prod2Name in _db.Production on rec.ProductionID equals prod2Name.ProductionID into prod2Name_join
                            where rec.SpiritID == RecordID
                            from prod2Name in prod2Name_join.DefaultIfEmpty()
                            select new
                            {
                                ProductionID = (int?)prod2Name.ProductionID ?? 0,
                                ProductionName = prod2Name.ProductionName ?? string.Empty
                            };

                        if (res.Count() == 0)
                        {
                            delReturn.ExecuteResult = DeleteSpirit(userId, RecordID);
                        }
                        else
                        {
                            foreach (var item in res)
                            {
                                delReturn.ExecuteMessage = item.ProductionName;
                            }
                        }
                    }
                    else if (RecordType == "Storage")
                    {
                        var res = (from rec in _db.StorageToRecord
                                   where rec.StorageID == RecordID
                                   select rec).ToList();

                        if (res.Count() == 0)
                        {
                            delReturn.ExecuteResult = DeleteStorage(userId, RecordID);
                        }
                        else
                        {
                            string recordName;
                            foreach (var item in res)
                            {
                                if (item.TableIdentifier == "pur")
                                {
                                    recordName = (from rec in _db.Purchase
                                                  where rec.PurchaseID == item.RecordId && rec.DistillerID == distillerId
                                                  select rec.PurchaseName).FirstOrDefault();
                                    delReturn.ExecuteMessage = recordName;
                                }
                                if (item.TableIdentifier == "prod")
                                {
                                    recordName = (from rec in _db.Production
                                                  where rec.ProductionID == item.RecordId && rec.DistillerID == distillerId
                                                  select rec.ProductionName).FirstOrDefault();
                                    delReturn.ExecuteMessage = recordName;
                                }
                            }
                        }
                    }
                    else if (RecordType == "Vendor")
                    {
                        var res = from rec in _db.Purchase
                                  where rec.VendorID == RecordID && rec.DistillerID == distillerId
                                  select rec;

                        if (res.Count() == 0)
                        {
                            delReturn.ExecuteResult = DeleteVendor(userId, RecordID);
                        }
                        else
                        {
                            foreach (var item in res)
                            {
                                delReturn.ExecuteMessage = item.PurchaseName;
                            }
                        }
                    }
                    else
                    {
                        delReturn.ExecuteResult = false;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Failed to delete " + RecordType + ": " + e);
                    delReturn.ExecuteResult = false;
                }
            }
            else
            {
                delReturn.ExecuteResult = false;
            }
            return delReturn;
        }

        private bool DeleteVendor(int userId, int vendorID)
        {
            bool retMthdExecResult = false;
            int distillerId = _dl.GetDistillerId(userId);
            if (vendorID > 0)
            {
                try
                {
                    var recs1 =
                        (from rec in _db.VendorDetail
                         where rec.VendorID == vendorID
                         select rec).FirstOrDefault();

                    if (recs1 != null)
                    {
                        _db.VendorDetail.Remove(recs1);
                        _db.SaveChanges();
                    }

                    var recs2 =
                        (from rec2 in _db.Vendor
                         where rec2.VendorID == vendorID && rec2.DistillerID == distillerId
                         select rec2).FirstOrDefault();

                    if (recs2 != null)
                    {
                        _db.Vendor.Remove(recs2);
                        _db.SaveChanges();
                    }

                    retMthdExecResult = true;
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Failed to delete Vendor: " + e);
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
            int distillerId = _dl.GetDistillerId(userId);
            if (spiritID > 0)
            {
                try
                {
                    var recs =
                        (from rec in _db.Spirit
                         where rec.SpiritID == spiritID && rec.DistillerID == distillerId
                         select rec).FirstOrDefault();
                    if (recs != null)
                    {
                        _db.Spirit.Remove(recs);
                        _db.SaveChanges();
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
            int distillerId = _dl.GetDistillerId(userId);
            if (storageID >= 0)
            {
                try
                {
                    var recs1 = (from rec in _db.StorageState
                                 where rec.StorageID == storageID
                                 select rec).FirstOrDefault();
                    if (recs1 != null)
                    {
                        _db.StorageState.Remove(recs1);
                        _db.SaveChanges();
                    }
                    var recs2 =
                        (from rec in _db.Storage
                         where rec.StorageID == storageID && rec.DistillerID == distillerId
                         select rec).FirstOrDefault();
                    if (recs2 != null)
                    {
                        _db.Storage.Remove(recs2);
                        _db.SaveChanges();
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
            int distillerId = _dl.GetDistillerId(userId);
            if (rawMaterialID >= 0)
            {
                try
                {
                    var recs1 =
                          (from rec in _db.MaterialDict2MaterialCategory
                           where rec.MaterialDictID == rawMaterialID
                           select rec).FirstOrDefault();
                    if (recs1 != null)
                    {
                        _db.MaterialDict2MaterialCategory.Remove(recs1);
                        _db.SaveChanges();
                    }
                    var recs2 =
                        (from rec in _db.MaterialDict
                         where rec.MaterialDictID == rawMaterialID && rec.DistillerID == distillerId
                         select rec).FirstOrDefault();
                    if (recs2 != null)
                    {
                        _db.MaterialDict.Remove(recs2);
                        _db.SaveChanges();
                    }
                    var recs3 =
                        (from rec in _db.MaterialType
                         where rec.MaterialDictID == rawMaterialID
                         select rec).FirstOrDefault();
                    if (recs3 != null)
                    {
                        _db.MaterialType.Remove(recs3);
                        _db.SaveChanges();
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
    }
}