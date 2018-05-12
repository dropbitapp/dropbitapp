using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp.Helpers;
using WebApp.Models;
using WebApp.Persistence.Repositories;

namespace WebApp.Workflows
{
    public class PurchaseWorkflow
    {
        private DistilDBContext db;

        public PurchaseWorkflow()
        {
            db = new DistilDBContext();
        }

        internal bool UpdatePurchase(PurchaseObject purchaseObject, int userId)
        {
            //instantiate DataLayer to call shared method
            DataLayer dl = new DataLayer();

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

                dl.SavePurchaseHistory(purchaseObject, userId);
            }
            catch (Exception e)
            {
                throw;
            }

            return retMthdExecResult;
        }

        /// <summary>
        /// CreatePurchase Method creates a new Purchase Record
        /// </summary>
        /// <param name="purchaseObject"></param>
        /// <returns>int</returns>
        public int CreatePurchase(PurchaseObject purchaseObject, int userId)
        {
            //instantiate DataLayer to call shared method
            DataLayer dl = new DataLayer();

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
                purchT.DistillerID = dl.GetDistillerId(userId);

                var pTypes =
                    (from rec in db.PurchaseType
                     where rec.Name == purchaseObject.PurchaseType
                     select rec).FirstOrDefault();

                if (pTypes != null)
                {
                    purchT.Gauged = pTypes.PurchaseTypeID == (int)Persistence.BusinessLogicEnums.PurchaseType.Fermented || pTypes.PurchaseTypeID == (int)Persistence.BusinessLogicEnums.PurchaseType.Distilled ? true : false;
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
                if ((pTypes.PurchaseTypeID == (int)Persistence.BusinessLogicEnums.PurchaseType.Fermented || pTypes.PurchaseTypeID == (int)Persistence.BusinessLogicEnums.PurchaseType.Distilled) && purchaseObject?.SpiritTypeReportingID != null && purchaseObject?.SpiritTypeReportingID != 0)
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

                if (purchT.PurchaseTypeID == (int)Persistence.BusinessLogicEnums.PurchaseType.Fermentable || purchT.PurchaseTypeID == (int)Persistence.BusinessLogicEnums.PurchaseType.Fermented || purchT.PurchaseTypeID == (int)Persistence.BusinessLogicEnums.PurchaseType.Distilled || purchT.PurchaseTypeID == (int)Persistence.BusinessLogicEnums.PurchaseType.Additive)
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
                dl.SavePurchaseHistory(purchaseObject, userId);
            }
            catch (Exception e)
            {
                retMthdExecResult = 0;
                throw e;
            }

            return retMthdExecResult;
        }

        /// <summary>
        /// GetRawMaterialListForPurchase method is used to view RawMaterials list in Purchase workflows
        /// </summary>
        /// <param name="optimized"></param>
        /// <param name="purchaseMatType"></param>
        /// <returns></returns>
        public List<RawMaterialObject> GetRawMaterialListForPurchase(int userId, string purchaseMatType = "")
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
        /// This method validates that Purchase object is not being used by Production object
        /// prior to deletion. If Purchase object is used by Production object, method doesn't 
        /// delete Purchase object and instead surfaces name of Production object to user. 
        /// </summary>
        /// <param name="deleteObject"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public ReturnObject DeletePurchaseRecord(int userId, DeleteRecordObject deleteObject)
        {
            ReturnObject delReturn = new ReturnObject();
            //instantiate DataLayer to call shared method
            DataLayer dl = new DataLayer();

            int recordId = deleteObject.DeleteRecordID;
            string recordType = deleteObject.DeleteRecordType;
            int distillerId = dl.GetDistillerId(userId);

            if (recordId > 0)
            {
                try
                {
                    var res =
                        from rec in db.ProductionToPurchase
                        join prod2Name in db.Production on rec.ProductionID equals prod2Name.ProductionID into prod2Name_join
                        where rec.PurchaseID == recordId
                        from prod2Name in prod2Name_join.DefaultIfEmpty()
                        select new
                        {
                            ProductionID = (int?)prod2Name.ProductionID ?? 0,
                            ProductionName = prod2Name.ProductionName ?? string.Empty
                        };

                    if (res.Count() == 0)
                    {
                        delReturn.ExecuteResult = DeletePurchaseExecute(recordId, userId);
                    }
                    else
                    {
                        foreach (var item in res)
                        {
                            delReturn.ExecuteMessage = item.ProductionName;
                        }
                    }
                }
                catch (Exception e)
                {
                    delReturn.ExecuteResult = false;
                }
            }
            else
            {
                delReturn.ExecuteResult = false;
                delReturn.ExecuteMessage = "Purchase Id is Null";
            }
            return delReturn;
        }

        /// <summary>
        /// This method removes all relevant records in the DB in all associated tables
        /// </summary>
        /// <param name="purchaseObject"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool DeletePurchaseExecute(int purchaseId, int userId)
        {
            bool retMthdExecResult = false;
            if (purchaseId > 0)
            {
                try
                {
                    var purRec =
                        (from rec in db.Purchase
                         join dslrs in db.AspNetUserToDistiller on rec.DistillerID equals dslrs.DistillerID into dslrs_join
                         from dslrs in dslrs_join.DefaultIfEmpty()
                         where rec.PurchaseID == purchaseId &&
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
        /// GetPurMaterialType method maps current PurchaseMaterialBooleanTypes types
        /// </summary>
        /// <param name="purMatBooleanTypes"></param>
        /// <param name="purchaseMaterialTypeName"></param>
        /// <returns></returns>
        //private void GetPurMaterialType(ref PurchaseMaterialBooleanTypes purMatBooleanTypes, string purchaseMaterialTypeName)
        //{
        //    if (purchaseMaterialTypeName == "Additive")
        //    {
        //        purMatBooleanTypes.Additive = true;
        //    }
        //    if (purchaseMaterialTypeName == "Supply")
        //    {
        //        purMatBooleanTypes.Supply = true;
        //    }
        //    if (purchaseMaterialTypeName == "Distilled")
        //    {
        //        purMatBooleanTypes.Distilled = true;
        //    }
        //    if (purchaseMaterialTypeName == "Fermented")
        //    {
        //        purMatBooleanTypes.Fermented = true;
        //    }
        //    if (purchaseMaterialTypeName == "Fermentable")
        //    {
        //        purMatBooleanTypes.Fermentable = true;
        //    }
        //}

        /// <summary>
        /// IsOfPurMaterialType method returns value (true/false) for a given set of purchaseMaterialTypes and purchaseMaterialType in question.
        /// For example I am trying to see what bool value a current purchaseMaterial type of "Fermented" has.
        /// </summary>
        /// <param name="purchaseMaterialTypes"></param>
        /// <param name="purchaseMatType"></param>
        /// <returns></returns>
        //private bool IsOfPurMaterialType(PurchaseMaterialBooleanTypes purchaseMaterialTypes, string purchaseMatType)
        //{
        //    if (purchaseMatType == "Additive")
        //    {
        //        return purchaseMaterialTypes.Additive;
        //    }
        //    else if (purchaseMatType == "Supply")
        //    {
        //        return purchaseMaterialTypes.Supply;
        //    }
        //    else if (purchaseMatType == "Distilled")
        //    {
        //        return purchaseMaterialTypes.Distilled;
        //    }
        //    else if (purchaseMatType == "Fermented")
        //    {
        //        return purchaseMaterialTypes.Fermented;
        //    }
        //    else if (purchaseMatType == "Fermentable")
        //    {
        //        return purchaseMaterialTypes.Fermentable;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}
    }
}