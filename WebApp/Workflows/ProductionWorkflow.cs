using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebApp.Helpers;
using WebApp.Models;

namespace WebApp.Workflows
{
    public class ProductionWorkflow
    {
        private readonly DistilDBContext _db;
        private readonly DataLayer _dl;

        public ProductionWorkflow(DistilDBContext db, DataLayer dl)
        {
            _db = db;
            _dl = dl;
        }

        /// <summary>
        /// CreateProduction Method creates a new Production Record
        /// </summary>
        /// <param name="prodObject"></param>
        /// <returns>int</returns> 
        public int CreateProduction(ProductionObject prodObject, int userId)
        {
            //define method execution return value to be false by default
            int retMthdExecResult = 0;

            var distillerId = _dl.GetDistillerId(userId);

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
                    (from rec in _db.ProductionType
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
                _db.Volume.Add(quantG);
                _db.SaveChanges();

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
                _db.Weight.Add(vBW);
                _db.SaveChanges();

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
                _db.Alcohol.Add(alc);
                _db.SaveChanges();

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
                _db.Proof.Add(proof);
                _db.SaveChanges();

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
                    (from rec in _db.State
                     where rec.Name == "Fermented"
                     select rec.StateID).FirstOrDefault();
            }
            else if (prodObject.ProductionType == "Distillation")
            {
                prod.StateID =
                    (from rec in _db.State
                     where rec.Name == "Distilled"
                     select rec.StateID).FirstOrDefault();
            }
            else if (prodObject.ProductionType == "Blending")
            {
                prod.StateID =
                    (from rec in _db.State
                     where rec.Name == "Blended"
                     select rec.StateID).FirstOrDefault();
            }
            else if (prodObject.ProductionType == "Bottling")
            {
                prod.StateID =
                    (from rec in _db.State
                     where rec.Name == "Bottled"
                     select rec.StateID).FirstOrDefault();
            }

            prod.StatusID =
                (from rec in _db.Status
                 where rec.Name == "Active"
                 select rec.StatusID).FirstOrDefault();

            // save new records in Production table
            _db.Production.Add(prod);
            _db.SaveChanges();

            if (prodObject.Storage != null)
            {
                //update StorageToRecord
                foreach (var iter in prodObject.Storage)
                {
                    StorageToRecord storToRec = new StorageToRecord();
                    storToRec.StorageID = iter.StorageId;
                    storToRec.RecordId = prod.ProductionID;
                    storToRec.TableIdentifier = "prod";
                    _db.StorageToRecord.Add(storToRec);
                    _db.SaveChanges();
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
                _db.ProductionToSpiritTypeReporting.Add(prodToSpirType);
                _db.SaveChanges();
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
                            _db.ProductionToSpiritCut.Add(prodToSCut);
                            _db.SaveChanges();
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
                        _db.ProductionToSpirit.Add(prodToSpirit);
                        _db.SaveChanges();
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
                            _db.BlendedComponent.Add(bC);
                            _db.SaveChanges();
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
                        _db.ProductionToSpirit.Add(prodToSpirit);
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
                        _db.GainLoss.Add(glt);
                        _db.SaveChanges();
                    }
                    else if (prodObject.GainLoss < 0)
                    {
                        // loss
                        GainLoss glt = new GainLoss();
                        glt.Type = false;
                        glt.Quantity = Math.Abs(prodObject.GainLoss); // since cumulativeGainLoss is negative, making it to be positive
                        glt.DateRecorded = DateTime.UtcNow;
                        glt.BottledRecordId = prod.ProductionID;
                        _db.GainLoss.Add(glt);
                        _db.SaveChanges();
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

                        _db.BottlingInfo.Add(bottI);
                        _db.SaveChanges();
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
                            _db.FillTest.Add(fillTest);
                            _db.SaveChanges();
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

                _db.Production4Reporting.Add(prod4RepT);
                _db.SaveChanges();
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

        /// <summary>
        /// this method updates records that are used as burndowns in Production workflows
        /// </summary>
        /// <param name="usedMats"></param>
        /// <param name="productionIDBeingCreated"></param>
        /// <param name="userId"></param>
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
                            (from rec in _db.Purchase
                             where rec.PurchaseID == k.ID
                             select rec).FirstOrDefault();

                        // all of the batch volume/weight used in the distillation
                        if (k.OldVal == 0)
                        {
                            status = "Processed";
                            var statusId =
                                (from rec in _db.Status
                                 where rec.Name == status
                                 select rec.StatusID).FirstOrDefault();

                            purch.StatusID = statusId;
                            purObj.Status = status;
                        }
                        else if (k.OldVal > 0)
                        {
                            status = "Processing";
                            var statusId =
                                (from rec in _db.Status
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
                            (from rec in _db.Purchase4Reporting
                             where purch.PurchaseID == rec.PurchaseID
                             select rec).FirstOrDefault();

                        if (p != null)
                        {
                            if (p.Proof == k.Proof && p.PurchaseID == k.ID)
                            {
                                p.Redistilled = true;
                                p.ProductionID = productionIDBeingCreated;

                                _db.SaveChanges();
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

                                _db.Purchase4Reporting.Add(purch4Rep);
                                _db.SaveChanges();
                            }
                        }

                        // update proof value after it has been recalculated
                        // on front-end using the new volume quantity and also
                        // store left over Proof into materials that are being burnt down
                        float tempProofGHolder = 0f;

                        if (purch.ProofID > 0 && k.Proof >= 0)
                        {
                            var proof =
                                (from rec in _db.Proof
                                 where rec.ProofID == purch.ProofID
                                 select rec).FirstOrDefault();

                            if (proof != null)
                            {
                                tempProofGHolder = proof.Value - k.Proof;
                                proof.Value = k.Proof;
                            }

                            _db.SaveChanges();
                        }

                        //todo: perhaps, we can re-use Production content workflow below to record Blending additives as well
                        // save to the ProductionContent table
                        List<ProductionContent> prodContentL = new List<ProductionContent>();

                        if (k.BurningDownMethod == "volume" && purch.VolumeID > 0)
                        {
                            if (purch.PurchaseTypeID == (int)Persistence.BusinessLogicEnums.PurchaseType.Fermentable)
                            {
                                // PurFermentableVolume
                                ProductionContent prodContent = PrepareProductionContentTableInfo4Saving(productionIDBeingCreated, k.ID, (int)Persistence.BusinessLogicEnums.ContenField.PurFermentableVolume, false, k.NewVal);
                                prodContentL.Add(prodContent);
                            }

                            if (purch.PurchaseTypeID == (int)Persistence.BusinessLogicEnums.PurchaseType.Fermented)
                            {
                                // PurFermentedVolume
                                ProductionContent prodContent = PrepareProductionContentTableInfo4Saving(productionIDBeingCreated, k.ID, (int)Persistence.BusinessLogicEnums.ContenField.PurFermentedVolume, false, k.NewVal);
                                prodContentL.Add(prodContent);

                                ProductionContent prodContent4ProofG = PrepareProductionContentTableInfo4Saving(productionIDBeingCreated, k.ID, (int)Persistence.BusinessLogicEnums.ContenField.PurFermentedProofGal, false, tempProofGHolder);
                                prodContentL.Add(prodContent4ProofG);
                            }

                            if (purch.PurchaseTypeID == (int)Persistence.BusinessLogicEnums.PurchaseType.Distilled)
                            {
                                // PurDistilledVolume
                                ProductionContent prodContent = PrepareProductionContentTableInfo4Saving(productionIDBeingCreated, k.ID, (int)Persistence.BusinessLogicEnums.ContenField.PurDistilledVolume, false, k.NewVal);
                                prodContentL.Add(prodContent);

                                ProductionContent prodContent4ProofG = PrepareProductionContentTableInfo4Saving(productionIDBeingCreated, k.ID, (int)Persistence.BusinessLogicEnums.ContenField.PurDistilledProofGal, false, tempProofGHolder);
                                prodContentL.Add(prodContent4ProofG);
                            }

                            var q =
                                (from rec in _db.Volume
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
                            if (purch.PurchaseTypeID == (int)Persistence.BusinessLogicEnums.PurchaseType.Fermentable)
                            {
                                // PurFermentableWeight
                                ProductionContent prodContent = PrepareProductionContentTableInfo4Saving(productionIDBeingCreated, k.ID, (int)Persistence.BusinessLogicEnums.ContenField.PurFermentableWeight, false, k.NewVal);
                                prodContentL.Add(prodContent);
                            }

                            if (purch.PurchaseTypeID == (int)Persistence.BusinessLogicEnums.PurchaseType.Fermented)
                            {
                                // PurFermentedWeight
                                ProductionContent prodContent = PrepareProductionContentTableInfo4Saving(productionIDBeingCreated, k.ID, (int)Persistence.BusinessLogicEnums.ContenField.PurFermentedWeight, false, k.NewVal);
                                prodContentL.Add(prodContent);
                            }

                            if (purch.PurchaseTypeID == (int)Persistence.BusinessLogicEnums.PurchaseType.Distilled)
                            {
                                // PurDistilledWeight
                                ProductionContent prodContent = PrepareProductionContentTableInfo4Saving(productionIDBeingCreated, k.ID, (int)Persistence.BusinessLogicEnums.ContenField.PurDistilledWeight, false, k.NewVal);
                                prodContentL.Add(prodContent);
                            }

                            var vBW =
                                (from rec in _db.Weight
                                 where rec.WeightID == purch.WeightID
                                 select rec).FirstOrDefault();

                            if (vBW != null)
                            {
                                vBW.Value = k.OldVal;
                            }

                            purObj.VolumeByWeight = k.OldVal;
                        }

                        _db.ProductionContent.AddRange(prodContentL);
                        _db.SaveChanges();

                        _dl.SavePurchaseHistory(purObj, userId);
                    }
                    // production batch used in the distillation
                    else if (k.DistillableOrigin == "prod")
                    {
                        ProductionObject prodObj = new ProductionObject();

                        prodObj.ProductionId = k.ID;

                        // query for purchaseIds associated with production record
                        // that is being used in the distillation
                        var prod2PurIds =
                            (from rec in _db.ProductionToPurchase
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
                            (from rec in _db.Production
                             where rec.ProductionID == k.ID
                             select rec).FirstOrDefault();

                        // all of the batch volume/weight used in the distillation
                        if (k.OldVal <= 0)
                        {
                            status = "Processed";
                            var statusId =
                                (from rec in _db.Status
                                 where rec.Name == status
                                 select rec.StatusID).FirstOrDefault();

                            prodRec.StatusID = statusId;

                            prodObj.StatusName = status;
                        }
                        else if (k.OldVal > 0)
                        {
                            status = "Processing";
                            var statusId =
                                (from rec in _db.Status
                                 where rec.Name == status
                                 select rec.StatusID).FirstOrDefault();

                            prodRec.StatusID = statusId;
                            prodObj.StatusName = status;
                        }
                        // we need to make sure that if the used material that was produced by us is a distilate and being re-distiled again,
                        // it needs to be marked as redistilled for reporting purposes if all of the proof gallons are used. Else, we need to insert
                        // another record into Production4Reporting with the same ProductionID but with different Proof and volume/weight values.
                        var p =
                            (from rec in _db.Production4Reporting
                             where prodRec.ProductionID == rec.ProductionID
                             select rec).FirstOrDefault();
                        if (p != null)
                        {
                            if (p.Proof == k.Proof && p.ProductionID == k.ID)
                            {
                                p.Redistilled = true;
                                _db.SaveChanges();
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
                                _db.Production4Reporting.Add(prod4Rep);
                                _db.SaveChanges();
                            }
                        }

                        // update proof value after it has been recalculated
                        // on front-end using the new volume quantity and also
                        // store left over Proof into materials that are being burnt down
                        float tempProofGHolder = 0f;

                        if (prodRec.ProofID > 0 && k.Proof >= 0)
                        {
                            var proof =
                                (from rec in _db.Proof
                                 where rec.ProofID == prodRec.ProofID
                                 select rec).FirstOrDefault();

                            if (proof != null)
                            {
                                tempProofGHolder = proof.Value - k.Proof;
                                proof.Value = k.Proof;
                            }

                            _db.SaveChanges();
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
                            if (prodRec.ProductionTypeID == (int)Persistence.BusinessLogicEnums.ProductionType.Fermentation)
                            {
                                // ProdFermentedVolume
                                ProductionContent prodContent = PrepareProductionContentTableInfo4Saving(productionIDBeingCreated, k.ID, (int)Persistence.BusinessLogicEnums.ContenField.ProdFermentedVolume, true, k.NewVal);
                                prodContentL.Add(prodContent);

                                ProductionContent prodContent4ProofG = PrepareProductionContentTableInfo4Saving(productionIDBeingCreated, k.ID, (int)Persistence.BusinessLogicEnums.ContenField.ProdFermentedProofGal, true, tempProofGHolder);
                                prodContentL.Add(prodContent4ProofG);
                            }

                            if (prodRec.ProductionTypeID == (int)Persistence.BusinessLogicEnums.ProductionType.Distillation)
                            {
                                // ProdDistilledVolume
                                ProductionContent prodContent = PrepareProductionContentTableInfo4Saving(productionIDBeingCreated, k.ID, (int)Persistence.BusinessLogicEnums.ContenField.ProdDistilledVolume, true, k.NewVal);
                                prodContentL.Add(prodContent);

                                ProductionContent prodContent4ProofG = PrepareProductionContentTableInfo4Saving(productionIDBeingCreated, k.ID, (int)Persistence.BusinessLogicEnums.ContenField.ProdDistilledProofGal, true, tempProofGHolder);
                                prodContentL.Add(prodContent4ProofG);
                            }

                            if (prodRec.ProductionTypeID == (int)Persistence.BusinessLogicEnums.ProductionType.Blending)
                            {
                                // ProdBlendedVolume
                                ProductionContent prodContent = PrepareProductionContentTableInfo4Saving(productionIDBeingCreated, k.ID, (int)Persistence.BusinessLogicEnums.ContenField.ProdBlendedVolume, true, k.NewVal);
                                prodContentL.Add(prodContent);

                                ProductionContent prodContent4ProofG = PrepareProductionContentTableInfo4Saving(productionIDBeingCreated, k.ID, (int)Persistence.BusinessLogicEnums.ContenField.ProdBlendedProofGal, true, tempProofGHolder);
                                prodContentL.Add(prodContent4ProofG);
                            }

                            _db.ProductionContent.AddRange(prodContentL);

                            var q =
                                (from rec in _db.Volume
                                 where prodRec.VolumeID == rec.VolumeID
                                 select rec).FirstOrDefault();

                            if (q != null)
                            {
                                q.Value = k.OldVal;
                            }

                            prodObj.Quantity = k.OldVal;

                            _db.SaveChanges();
                        }

                        if (k.BurningDownMethod == "weight" && prodRec.WeightID > 0)
                        {
                            if (prodRec.ProductionTypeID == (int)Persistence.BusinessLogicEnums.ProductionType.Fermentation)
                            {
                                // ProdFermentedWeight
                                ProductionContent prodContent = PrepareProductionContentTableInfo4Saving(productionIDBeingCreated, k.ID, (int)Persistence.BusinessLogicEnums.ContenField.ProdFermentedWeight, true, k.NewVal);
                                prodContentL.Add(prodContent);

                                ProductionContent prodContent4ProofG = PrepareProductionContentTableInfo4Saving(productionIDBeingCreated, k.ID, (int)Persistence.BusinessLogicEnums.ContenField.ProdFermentedProofGal, true, tempProofGHolder);
                                prodContentL.Add(prodContent4ProofG);
                            }

                            if (prodRec.ProductionTypeID == (int)Persistence.BusinessLogicEnums.ProductionType.Distillation)
                            {
                                // ProdDistilledWeight
                                ProductionContent prodContent = PrepareProductionContentTableInfo4Saving(productionIDBeingCreated, k.ID, (int)Persistence.BusinessLogicEnums.ContenField.ProdDistilledWeight, true, k.NewVal);
                                prodContentL.Add(prodContent);

                                ProductionContent prodContent4ProofG = PrepareProductionContentTableInfo4Saving(productionIDBeingCreated, k.ID, (int)Persistence.BusinessLogicEnums.ContenField.ProdDistilledProofGal, true, tempProofGHolder);
                                prodContentL.Add(prodContent4ProofG);
                            }

                            if (prodRec.ProductionTypeID == (int)Persistence.BusinessLogicEnums.ProductionType.Blending)
                            {
                                // ProdBlendedWeight
                                ProductionContent prodContent = PrepareProductionContentTableInfo4Saving(productionIDBeingCreated, k.ID, (int)Persistence.BusinessLogicEnums.ContenField.ProdBlendedWeight, true, k.NewVal);
                                prodContentL.Add(prodContent);

                                ProductionContent prodContent4ProofG = PrepareProductionContentTableInfo4Saving(productionIDBeingCreated, k.ID, (int)Persistence.BusinessLogicEnums.ContenField.ProdBlendedProofGal, true, tempProofGHolder);
                                prodContentL.Add(prodContent4ProofG);
                            }

                            _db.ProductionContent.AddRange(prodContentL);

                            var vBW =
                            (from rec in _db.Weight
                             where prodRec.WeightID == rec.WeightID
                             select rec).FirstOrDefault();

                            if (vBW != null)
                            {
                                vBW.Value = k.OldVal;
                            }

                            prodObj.VolumeByWeight = k.OldVal;
                        }

                        _db.SaveChanges();

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
                            _db.ProductionToPurchase.Add(prodToPur);
                            _db.SaveChanges();
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

                _db.ProductionHistory.Add(histTable);
                _db.SaveChanges();

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
                    (from rec in _db.Proof
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

                _db.SaveChanges();

                return oldProof;
            }
        }

        public ReturnObject DeleteProductionRecord(int userId, DeleteRecordObject deleteObject)
        {
            //intitialize return object
            ReturnObject delReturn = new ReturnObject();

            int recordId = deleteObject.DeleteRecordID;
            string recordType = deleteObject.DeleteRecordType;

            int distillerId = _dl.GetDistillerId(userId);

            if (recordId > 0)
            {
                try
                {
                    var res =
                        from rec in _db.ProductionContent
                        join prod2Name in _db.Production on rec.ProductionID equals prod2Name.ProductionID into prod2Name_join
                        where rec.RecordID == recordId
                        from prod2Name in prod2Name_join.DefaultIfEmpty()
                        select new
                        {
                            ProductionID = (int?)prod2Name.ProductionID ?? 0,
                            ProductionName = prod2Name.ProductionName ?? string.Empty
                        };

                    if (res.Count() == 0)
                    {
                        delReturn.ExecuteResult = DeleteProductionExecute(deleteObject, userId);
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
                    delReturn.ExecuteMessage = "Failed to delete " + recordType + ": " + e;
                    delReturn.ExecuteResult = false;
                }
            }
            else
            {
                delReturn.ExecuteResult = false;
                delReturn.ExecuteMessage = "Production Id is Null";
            }
            return delReturn;
        }

        public bool DeleteProductionExecute(DeleteRecordObject deleteObject, int userId)
        {
            bool retMthdExecResult = false;
            int productionId = deleteObject.DeleteRecordID;
            string productionType = deleteObject.DeleteRecordType;

            if (productionId > 0)
            {
                try
                {
                    var prodRec =
                        (from rec in _db.Production
                         join distillers in _db.AspNetUserToDistiller on rec.DistillerID equals distillers.DistillerID into distillers_join
                         from distillers in distillers_join.DefaultIfEmpty()
                         where rec.ProductionID == productionId
                         && distillers.UserId == userId
                         select rec).FirstOrDefault();

                    if (prodRec != null)
                    {
                        // restore amounts for records that
                        // went into the creation of the record we are about to delete.
                        RestoreBurntdownRecords(prodRec.ProductionID);

                        var prod4Rep =
                           (from rec in _db.Production4Reporting
                            where rec.ProductionID == prodRec.ProductionID
                            select rec).FirstOrDefault();

                        if (prod4Rep != null)
                        {
                            _db.Production4Reporting.Remove(prod4Rep);
                        }

                        var purch4Rep =
                            (from rec in _db.Purchase4Reporting
                             where rec.ProductionID == prodRec.ProductionID
                             select rec).ToList();
                        if (purch4Rep != null)
                        {
                            foreach (var item in purch4Rep)
                            {
                                _db.Purchase4Reporting.Remove(item);
                            }

                        }

                        var prodC =
                            (from rec in _db.ProductionContent
                             where rec.ProductionID == prodRec.ProductionID
                             select rec);

                        if (prodC != null)
                        {
                            _db.ProductionContent.RemoveRange(prodC);
                        }

                        var prod2purch =
                            (from rec in _db.ProductionToPurchase
                             where rec.ProductionID == prodRec.ProductionID
                             select rec).FirstOrDefault();

                        if (prod2purch != null)
                        {
                            _db.ProductionToPurchase.Remove(prod2purch);
                        }

                        if (productionType == "Fermentation")
                        {
                            var prod2SpiTypeRep =
                               (from rec in _db.ProductionToSpiritTypeReporting
                                where rec.ProductionID == prodRec.ProductionID
                                select rec).FirstOrDefault();

                            if (prod2SpiTypeRep != null)
                            {
                                _db.ProductionToSpiritTypeReporting.Remove(prod2SpiTypeRep);
                            }
                        }

                        if (productionType == "Distillation")
                        {
                            var p2scRec =
                                (from rec in _db.ProductionToSpiritCut
                                 where rec.ProductionID == prodRec.ProductionID
                                 select rec).FirstOrDefault();

                            if (p2scRec != null)
                            {
                                _db.ProductionToSpiritCut.Remove(p2scRec);
                            }

                            var prod2SpiTypeRep =
                               (from rec in _db.ProductionToSpiritTypeReporting
                                where rec.ProductionID == prodRec.ProductionID
                                select rec).FirstOrDefault();

                            if (prod2SpiTypeRep != null)
                            {
                                _db.ProductionToSpiritTypeReporting.Remove(prod2SpiTypeRep);
                            }
                        }

                        if (productionType == "Blending")
                        {
                            var prod2SpiTypeRep =
                               (from rec in _db.ProductionToSpiritTypeReporting
                                where rec.ProductionID == prodRec.ProductionID
                                select rec).FirstOrDefault();

                            if (prod2SpiTypeRep != null)
                            {
                                _db.ProductionToSpiritTypeReporting.Remove(prod2SpiTypeRep);
                            }

                            var p2sRec =
                                (from rec in _db.ProductionToSpirit
                                 where rec.ProductionID == prodRec.ProductionID
                                 select rec).FirstOrDefault();

                            if (p2sRec != null)
                            {
                                _db.ProductionToSpirit.Remove(p2sRec);
                            }

                            var blendedComp =
                                (from rec in _db.BlendedComponent
                                 where rec.ProductionID == prodRec.ProductionID
                                 select rec);

                            if (blendedComp != null)
                            {
                                _db.BlendedComponent.RemoveRange(blendedComp);
                            }
                        }

                        if (productionType == "Bottling")
                        {
                            var p2sRec =
                                (from rec in _db.ProductionToSpirit
                                 where rec.ProductionID == prodRec.ProductionID
                                 select rec).FirstOrDefault();

                            if (p2sRec != null)
                            {
                                _db.ProductionToSpirit.Remove(p2sRec);
                            }

                            var bttlInfo =
                                (from rec in _db.BottlingInfo
                                 where rec.ProductionID == prodRec.ProductionID
                                 select rec);

                            if (bttlInfo != null)
                            {
                                _db.BottlingInfo.RemoveRange(bttlInfo);
                            }

                            var fillTest =
                                (from rec in _db.FillTest
                                 where rec.ProductionID == prodRec.ProductionID
                                 select rec);

                            if (fillTest != null)
                            {
                                _db.FillTest.RemoveRange(fillTest);
                            }
                        }

                        var qtyRec =
                            (from rec in _db.Volume
                             where rec.VolumeID == prodRec.VolumeID
                             select rec).FirstOrDefault();

                        if (qtyRec != null)
                        {
                            _db.Volume.Remove(qtyRec);
                        }

                        var vbwRec =
                            (from rec in _db.Weight
                             where rec.WeightID == prodRec.WeightID
                             select rec).FirstOrDefault();

                        if (vbwRec != null)
                        {
                            _db.Weight.Remove(vbwRec);
                        }

                        var alcRec =
                            (from rec in _db.Alcohol
                             where rec.AlcoholID == prodRec.AlcoholID
                             select rec).FirstOrDefault();

                        if (alcRec != null)
                        {
                            _db.Alcohol.Remove(alcRec);
                        }

                        var prfRec =
                            (from rec in _db.Proof
                             where rec.ProofID == prodRec.ProofID
                             select rec).FirstOrDefault();

                        if (prfRec != null)
                        {
                            _db.Proof.Remove(prfRec);
                        }

                        var strRecs =
                            (from rec in _db.StorageToRecord
                             where rec.RecordId == prodRec.ProductionID && rec.TableIdentifier == "prod"
                             select rec);

                        if (strRecs != null)
                        {
                            _db.StorageToRecord.RemoveRange(strRecs);
                        }

                        _db.SaveChanges();

                        retMthdExecResult = true;
                    }

                    _db.Production.Remove(prodRec);
                    _db.SaveChanges();
                }
                catch (Exception e)
                {
                    retMthdExecResult = false;
                    _db.Database.BeginTransaction().Rollback();
                }
            }
            else
            {
                retMthdExecResult = false;
            }
            return retMthdExecResult;
        }

        /// <summary>
        /// This methd restores records that were used up for the record, being deleted
        /// </summary>
        /// <param name="productionId"></param>
        private void RestoreBurntdownRecords(int productionId)
        {
            try
            {
                var prodContentRecords =
                (from prodCont in _db.ProductionContent
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
                            (from prod4Rep in _db.Production4Reporting
                             where prod4Rep.ProductionID == i.recordId
                             select prod4Rep).FirstOrDefault();

                            var productionValues =
                            (from prod in _db.Production
                             where prod.ProductionID == i.recordId
                             select prod).FirstOrDefault();

                            if (productionValues != null)
                            {
                                if (i.valueKind == (int)Persistence.BusinessLogicEnums.ContenField.ProdDistilledVolume || i.valueKind == (int)Persistence.BusinessLogicEnums.ContenField.ProdBlendedVolume || i.valueKind == (int)Persistence.BusinessLogicEnums.ContenField.PurFermentedAlcohol) // Volume
                                {
                                    var vol =
                                        (from volume in _db.Volume
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
                                else if (i.valueKind == (int)Persistence.BusinessLogicEnums.ContenField.ProdFermentedWeight || i.valueKind == (int)Persistence.BusinessLogicEnums.ContenField.ProdDistilledWeight || i.valueKind == (int)Persistence.BusinessLogicEnums.ContenField.ProdBlendedWeight) // Weight
                                {
                                    var weight =
                                        (from weigh in _db.Weight
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
                                else if (i.valueKind == (int)Persistence.BusinessLogicEnums.ContenField.ProdFermentedAlcohol || i.valueKind == (int)Persistence.BusinessLogicEnums.ContenField.ProdBlendedAlcohol || i.valueKind == (int)Persistence.BusinessLogicEnums.ContenField.ProdDistilledAlcohol) // Alcohol
                                {
                                    var alc =
                                        (from alcohol in _db.Alcohol
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
                                else if (i.valueKind == (int)Persistence.BusinessLogicEnums.ContenField.ProdDistilledProofGal || i.valueKind == (int)Persistence.BusinessLogicEnums.ContenField.ProdBlendedProofGal || i.valueKind == (int)Persistence.BusinessLogicEnums.ContenField.ProdFermentedProofGal) // Proof
                                {
                                    var prf =
                                        (from proof in _db.Proof
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
                            (from pur4Rep in _db.Purchase4Reporting
                             where pur4Rep.PurchaseID == i.recordId
                             select pur4Rep).FirstOrDefault();

                            var purchaseValues =
                            (from purch in _db.Purchase
                             where purch.PurchaseID == i.recordId
                             select purch).FirstOrDefault();

                            if (purchaseValues != null)
                            {
                                if (i.valueKind == (int)Persistence.BusinessLogicEnums.ContenField.PurFermentableVolume || i.valueKind == (int)Persistence.BusinessLogicEnums.ContenField.PurFermentedVolume || i.valueKind == (int)Persistence.BusinessLogicEnums.ContenField.PurAdditiveVolume || i.valueKind == (int)Persistence.BusinessLogicEnums.ContenField.PurDistilledVolume) // Volume
                                {
                                    var vol =
                                        (from volume in _db.Volume
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
                                else if (i.valueKind == (int)Persistence.BusinessLogicEnums.ContenField.PurFermentableWeight || i.valueKind == (int)Persistence.BusinessLogicEnums.ContenField.PurFermentedWeight || i.valueKind == (int)Persistence.BusinessLogicEnums.ContenField.PurAdditiveWeight || i.valueKind == (int)Persistence.BusinessLogicEnums.ContenField.PurDistilledWeight) // Weight
                                {
                                    var weight =
                                        (from weigh in _db.Weight
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
                                else if (i.valueKind == (int)Persistence.BusinessLogicEnums.ContenField.PurFermentedAlcohol || i.valueKind == (int)Persistence.BusinessLogicEnums.ContenField.PurDistilledAlcohol) // Alcohol
                                {
                                    var alc =
                                        (from alcohol in _db.Alcohol
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
                                else if (i.valueKind == (int)Persistence.BusinessLogicEnums.ContenField.PurFermentedProofGal || i.valueKind == (int)Persistence.BusinessLogicEnums.ContenField.PurDistilledProofGal) // Proof
                                {
                                    var prf =
                                        (from proof in _db.Proof
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
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// GetProductionList queries DB for Production data for a particular production type
        /// </summary>
        /// <returns></returns>
        public List<ProductionObject> GetProductionList(int userId, string prodType)
        {
            List<ProductionObject> prodList = new List<ProductionObject>();

            var res =
                from prod in _db.Production
                join prodTypes in _db.ProductionType on prod.ProductionTypeID equals prodTypes.ProductionTypeID into prodTypes_join
                from prodTypes in prodTypes_join.DefaultIfEmpty()
                join dstlrs in _db.AspNetUserToDistiller on prod.DistillerID equals dstlrs.DistillerID into dstlrs_join
                from dstlrs in dstlrs_join.DefaultIfEmpty()
                join galQuant in _db.Volume on prod.VolumeID equals galQuant.VolumeID into galQuant_join
                from galQuant in galQuant_join.DefaultIfEmpty()
                join VBW in _db.Weight on prod.WeightID equals VBW.WeightID into VBW_join
                from VBW in VBW_join.DefaultIfEmpty()
                join alc in _db.Alcohol on prod.AlcoholID equals alc.AlcoholID into alc_join
                from alc in alc_join.DefaultIfEmpty()
                join proof in _db.Proof on prod.ProofID equals proof.ProofID into proof_join
                from proof in proof_join.DefaultIfEmpty()
                join spiCutsM in _db.ProductionToSpiritCut on prod.ProductionID equals spiCutsM.ProductionID into spiCutsM_join
                from spiCutsM in spiCutsM_join.DefaultIfEmpty()
                join spiCuts in _db.SpiritCut on spiCutsM.SpiritCutID equals spiCuts.SpiritCutID into spiCuts_join
                from spiCuts in spiCuts_join.DefaultIfEmpty()
                join p2Spi in _db.ProductionToSpirit on prod.ProductionID equals p2Spi.ProductionID into p2Spi_join
                from p2Spi in p2Spi_join.DefaultIfEmpty()
                join spi in _db.Spirit on p2Spi.SpiritID equals spi.SpiritID into spi_join
                from spi in spi_join.DefaultIfEmpty()
                join status in _db.Status on prod.StatusID equals status.StatusID into status_join
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
                        from rec in _db.StorageToRecord
                        join stoName in _db.Storage on rec.StorageID equals stoName.StorageID
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
                        var ress = (from r in _db.BlendedComponent
                                    join rM in _db.MaterialDict on r.RecordId equals rM.MaterialDictID
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
                        var ress = (from r in _db.BottlingInfo
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
                    throw e;
                }
            }

            return prodList;
        }

        /// <summary>
        /// Method that updates DB with updated production values from fron-end
        /// </summary>
        /// <param name="pObj"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        internal bool UpdateProduction(ProductionObject pObj, int userId)
        {
            bool retMthdExecResult = false;
            try
            {
                var prodT =
                    (from rec in _db.Production
                     join distillers in _db.AspNetUserToDistiller on rec.DistillerID equals distillers.DistillerID into distillers_join
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
                    _db.SaveChanges();
                    //todo: need to be able to add update for Material Type(even though, updating material type might be difficult)

                    // update Spirit type for production if applicable
                    if (pObj?.SpiritId != null && pObj.SpiritId != 0)
                    {
                        var p2S =
                            (from rec in _db.ProductionToSpirit
                             where rec.ProductionID == prodT.ProductionID
                             select rec).FirstOrDefault();

                        if (p2S != null)
                        {
                            if (pObj.SpiritId != p2S.SpiritID)
                            {
                                p2S.SpiritID = pObj.SpiritId;
                            }
                        }
                        _db.SaveChanges();
                    }

                    // update Spirit Cut if applicable
                    if (pObj?.SpiritCutId != null && pObj.SpiritCutId != 0)
                    {
                        var p2SC =
                         (from rec in _db.ProductionToSpiritCut
                          where rec.ProductionID == prodT.ProductionID
                          select rec).FirstOrDefault();

                        if (p2SC != null)
                        {
                            if (pObj.SpiritId != p2SC.SpiritCutID)
                            {
                                p2SC.SpiritCutID = pObj.SpiritCutId;
                            }
                        }
                        _db.SaveChanges();
                    }

                    //Quantity
                    if (prodT.VolumeID != 0 && pObj.Quantity != null)
                    {
                        //update quantity record
                        var qtyRec =
                            (from rec in _db.Volume
                             where rec.VolumeID == prodT.VolumeID
                             select rec).FirstOrDefault();
                        if (qtyRec != null && qtyRec.Value != pObj.Quantity)
                        {
                            qtyRec.Value = pObj.Quantity;
                            _db.SaveChanges();
                        }
                    }
                    else if (prodT.VolumeID == 0 && pObj.Quantity != null)
                    {
                        //create quantity record
                        Volume newQtyRec = new Volume();
                        newQtyRec.Value = pObj.Quantity;
                        _db.Volume.Add(newQtyRec);
                        _db.SaveChanges();
                        prodT.VolumeID = newQtyRec.VolumeID;
                    }

                    if (pObj.ProductionType != "Bottling")
                    {
                        //Volume By Weight
                        if (prodT.WeightID != 0 && pObj.VolumeByWeight != null)
                        {
                            //update volume by weight record
                            var vbwRec =
                                (from rec in _db.Weight
                                 where rec.WeightID == prodT.WeightID
                                 select rec).FirstOrDefault();
                            if (vbwRec != null & vbwRec.Value != pObj.VolumeByWeight)
                            {
                                vbwRec.Value = pObj.VolumeByWeight;
                                _db.SaveChanges();
                            }
                        }
                        else if (prodT.WeightID == 0 && pObj.VolumeByWeight != null)
                        {
                            //create new volume by weight record
                            Weight newVbwRec = new Weight();
                            newVbwRec.Value = pObj.VolumeByWeight;
                            _db.Weight.Add(newVbwRec);
                            _db.SaveChanges();
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

                        _db.TaxWithdrawn.Add(taxes);
                        _db.SaveChanges();
                    }

                    //Alcohol Content
                    if (prodT.AlcoholID != 0 && pObj.AlcoholContent != null)
                    {
                        //update alcohol content record
                        var alcRec =
                            (from rec in _db.Alcohol
                             where rec.AlcoholID == prodT.AlcoholID
                             select rec).FirstOrDefault();
                        if (alcRec != null && alcRec.Value != pObj.AlcoholContent)
                        {
                            alcRec.Value = pObj.AlcoholContent;
                            _db.SaveChanges();
                        }
                    }
                    else if (prodT.AlcoholID == 0 && pObj.AlcoholContent != null)
                    {
                        //create alcohol content record
                        Alcohol newAlcRec = new Alcohol();
                        newAlcRec.Value = pObj.AlcoholContent;
                        _db.Alcohol.Add(newAlcRec);
                        _db.SaveChanges();
                        prodT.AlcoholID = newAlcRec.AlcoholID;
                    }

                    //Proof
                    if (prodT.ProofID != 0 && pObj.ProofGallon != null)
                    {
                        //update proof record
                        var prfRec =
                            (from rec in _db.Proof
                             where rec.ProofID == prodT.ProofID
                             select rec).FirstOrDefault();
                        if (prfRec != null && prfRec.Value != pObj.ProofGallon)
                        {
                            prfRec.Value = pObj.ProofGallon;
                            _db.SaveChanges();
                        }
                    }
                    else if (prodT.ProofID == 0 && pObj.ProofGallon != null)
                    {
                        //create proof record
                        Proof newPrfRec = new Proof();
                        newPrfRec.Value = pObj.ProofGallon;
                        _db.Proof.Add(newPrfRec);
                        _db.SaveChanges();
                        prodT.ProofID = newPrfRec.ProofID;
                    }

                    // storage update
                    var storages =
                        from rec in _db.StorageToRecord
                        where rec.RecordId == prodT.ProductionID && rec.TableIdentifier == "prod"
                        select rec;

                    // empty StorageToRecord table records first
                    if (storages != null)
                    {
                        foreach (var i in storages)
                        {
                            _db.StorageToRecord.Remove(i);
                        }
                        _db.SaveChanges();
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
                            _db.StorageToRecord.Add(stoR);
                        }
                        _db.SaveChanges();
                    }

                    // update Blended Component If applicable 
                    if (pObj.BlendingAdditives != null)
                    {
                        var blenComp =
                            (from rec in _db.BlendedComponent
                             where rec.ProductionID == prodT.ProductionID
                             select rec);

                        if (blenComp != null)
                        {
                            foreach (var bc in blenComp)
                            {
                                _db.BlendedComponent.Remove(bc);
                            }
                        }

                        foreach (var bA in pObj.BlendingAdditives)
                        {
                            BlendedComponent blendCT = new BlendedComponent();
                            blendCT.RecordId = bA.RawMaterialId;
                            blendCT.ProductionID = prodT.ProductionID;
                            blendCT.Quantity = bA.RawMaterialQuantity;
                            blendCT.UnitOfMeasurement = bA.UnitOfMeasurement;
                            _db.BlendedComponent.Add(blendCT);
                        }
                        _db.SaveChanges();
                    }

                    // update Bottling info if applicable
                    if (pObj.BottlingInfo != null)
                    {
                        var botlR =
                            (from rec in _db.BottlingInfo
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
                        _db.SaveChanges();
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

        /// <summary>
        /// Returns a list of all active Blends for a given userId -> distillerId
        /// </summary>
        /// <param name="prodType"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        internal List<ProdObjectConcise> GetBlendingList(string prodType, int userId)
        {
            List<ProdObjectConcise> bList = new List<ProdObjectConcise>();
            try
            {
                var res =
                   from prod in _db.Production
                   join quants in _db.Volume on prod.VolumeID equals quants.VolumeID into quants_join
                   from quants in quants_join.DefaultIfEmpty()
                   join VBW in _db.Weight on prod.WeightID equals VBW.WeightID into VBW_join
                   from VBW in VBW_join.DefaultIfEmpty()
                   join alc in _db.Alcohol on prod.AlcoholID equals alc.AlcoholID into alc_join
                   from alc in alc_join.DefaultIfEmpty()
                   join proof in _db.Proof on prod.ProofID equals proof.ProofID into proof_join
                   from proof in proof_join.DefaultIfEmpty()
                   join p2Spi in _db.ProductionToSpirit on prod.ProductionID equals p2Spi.ProductionID into p2Spi_join
                   from p2Spi in p2Spi_join.DefaultIfEmpty()
                   join spi in _db.Spirit on p2Spi.SpiritID equals spi.SpiritID into spi_join
                   from spi in spi_join.DefaultIfEmpty()
                   join status in _db.Status on prod.StatusID equals status.StatusID into status_join
                   from status in status_join.DefaultIfEmpty()
                   join state in _db.State on prod.StateID equals state.StateID into state_join
                   from state in state_join.DefaultIfEmpty()
                   join distiller in _db.AspNetUserToDistiller on new { DistillerID = prod.DistillerID } equals new { DistillerID = distiller.DistillerID } into distiller_join
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
    }
}