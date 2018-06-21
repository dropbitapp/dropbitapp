using System;
using System.Collections.Generic;
using System.Linq;
using WebApp.Helpers;
using WebApp.Models;
using WebApp.Persistence.BusinessLogicEnums;

namespace WebApp.Reports
{
    public class StorageReport
    {
        private readonly DistilDBContext _db;
        private readonly DataLayer _dl;

        public StorageReport(DistilDBContext db, DataLayer dl)
        {
            _db = db;
            _dl = dl;
        }

        public StorageReportObject GetStorageReportData(DateTime startDate, DateTime endDate, int userId)
        {
            try
            {
                StorageReportObject storageReport = new StorageReportObject();
                List<StorageReportCategory> storageReportBody = new List<StorageReportCategory>();

                // get distiller information for header report
                int distillerID = _dl.GetDistillerId(userId);

                storageReport.Header = _dl.GetDistillerInfoForReportHeader(distillerID, startDate);

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

                CalculateRowTotals(ref storageReportBody);

                storageReport.ReportBody = storageReportBody;

                return storageReport;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void CalculateRowTotals(ref List<StorageReportCategory> storageReportBody)
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
                    // Declare an instance of StorageReportCategory to save total values for each row
                    StorageReportCategory cat = new StorageReportCategory
                    {
                        CategoryName = spiritTypeCategory.ProductTypeName,
                        SpiritTypeReportingID = spiritTypeCategory.SpiritTypeReportingID
                    };

                    // Sum up values for each row and save to total category
                    foreach (var i in storageReportBody)
                    {
                        cat.r1_OnHandFirstOfMonth += i.r1_OnHandFirstOfMonth;
                        cat.r2_DepositedInBulkStorage += i.r2_DepositedInBulkStorage;
                        cat.r4_ReturnedToBulkStorage += i.r4_ReturnedToBulkStorage;
                        cat.r6_TotalLines1Through5 += i.r6_TotalLines1Through5;
                        cat.r7_TaxPaid += i.r7_TaxPaid;
                        cat.r17_TransferredToProcessingAccount += i.r17_TransferredToProcessingAccount;
                        cat.r18_TransferredToProductionAccount += i.r18_TransferredToProductionAccount;
                        cat.r19_TransferredToOtherBondedPremises += i.r19_TransferredToOtherBondedPremises;
                        cat.r20_Destroyed += i.r20_Destroyed;
                        cat.r22_OtherLosses += i.r22_OtherLosses;
                        cat.r23_OnHandEndOfMonth += i.r23_OnHandEndOfMonth;
                        cat.r24_Lines7Through23 += i.r24_Lines7Through23;
                    }

                    // Round totals
                    cat.r1_OnHandFirstOfMonth = (float)Math.Round(cat.r1_OnHandFirstOfMonth, 3);
                    cat.r2_DepositedInBulkStorage = (float)Math.Round(cat.r2_DepositedInBulkStorage, 3);
                    cat.r4_ReturnedToBulkStorage = (float)Math.Round(cat.r4_ReturnedToBulkStorage, 3);
                    cat.r6_TotalLines1Through5 = (float)Math.Round(cat.r6_TotalLines1Through5, 3);
                    cat.r7_TaxPaid = (float)Math.Round(cat.r7_TaxPaid, 3);
                    cat.r17_TransferredToProcessingAccount = (float)Math.Round(cat.r17_TransferredToProcessingAccount, 3);
                    cat.r18_TransferredToProductionAccount = (float)Math.Round(cat.r18_TransferredToProductionAccount, 3);
                    cat.r19_TransferredToOtherBondedPremises = (float)Math.Round(cat.r19_TransferredToOtherBondedPremises, 3);
                    cat.r20_Destroyed = (float)Math.Round(cat.r20_Destroyed, 3);
                    cat.r22_OtherLosses = (float)Math.Round(cat.r22_OtherLosses, 3);
                    cat.r23_OnHandEndOfMonth = (float)Math.Round(cat.r23_OnHandEndOfMonth, 3);
                    cat.r24_Lines7Through23 = (float)Math.Round(cat.r24_Lines7Through23, 3);

                    // Add total category to existing report body
                    storageReportBody.Add(cat);
                }
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Returns a collection of purchased on hand first of month.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        private IEnumerable<OnHandFirstOfMonth> GetPurchasedOnHandFirstOfMonthQuery(DateTime startDate, DateTime endDate, int userId)
        {
            return from purchase in _db.Purchase
                   join distiller in _db.AspNetUserToDistiller on purchase.DistillerID equals distiller.DistillerID into distiller_join
                   from distiller in distiller_join.DefaultIfEmpty()
                   join mDic in _db.MaterialDict on purchase.MaterialDictID equals mDic.MaterialDictID into mDic_join
                   from mDic in mDic_join.DefaultIfEmpty()
                   join uOm in _db.UnitOfMeasurement on mDic.UnitOfMeasurementID equals uOm.UnitOfMeasurementID into uOm_join
                   from uOm in uOm_join.DefaultIfEmpty()
                   join dest in _db.Destruction on purchase.PurchaseID equals dest.RecordID into dest_join
                   from dest in dest_join.DefaultIfEmpty()
                   join proof in _db.Proof on purchase.ProofID equals proof.ProofID into proof_join
                   from proof in proof_join.DefaultIfEmpty()
                   join productionContent in _db.ProductionContent on purchase.PurchaseID equals productionContent.RecordID into productionContent_join
                   from productionContent in productionContent_join.DefaultIfEmpty()
                   join production in _db.Production on productionContent.ProductionID equals production.ProductionID into production_join
                   from production in production_join.DefaultIfEmpty()
                   join productionToSpiritType in _db.PurchaseToSpiritTypeReporting on purchase.PurchaseID equals productionToSpiritType.PurchaseID into productionToSpiritType_join
                   from productionToSpiritType in productionToSpiritType_join.DefaultIfEmpty()
                   join str in _db.SpiritTypeReporting on productionToSpiritType.SpiritTypeReportingID equals str.SpiritTypeReportingID into str_join
                   from str in str_join.DefaultIfEmpty()
                   where
                       distiller.UserId == userId
                       && (purchase.PurchaseTypeID == (int)Persistence.BusinessLogicEnums.PurchaseType.Fermented || purchase.PurchaseTypeID == (int)Persistence.BusinessLogicEnums.PurchaseType.Distilled)
                       && purchase.PurchaseDate < startDate
                       && ((purchase.StatusID == (int)Persistence.BusinessLogicEnums.Status.Active || purchase.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processing || purchase.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processed) || (purchase.StateID == (int)Persistence.BusinessLogicEnums.Status.Destroyed && dest.EndTime > startDate && dest.EndTime < endDate))
                       && (productionContent == null || (productionContent != null && (productionContent.ContentFieldID == (int)Persistence.BusinessLogicEnums.ContenField.PurFermentedProofGal || productionContent.ContentFieldID == (int)Persistence.BusinessLogicEnums.ContenField.PurDistilledProofGal)))
                       && uOm.UnitOfMeasurementID != (int)Persistence.BusinessLogicEnums.UnitOfMeasurement.lb
                   select new OnHandFirstOfMonth()
                   {
                       ProductionDate = production.ProductionEndTime != null ? production.ProductionEndTime : DateTime.MinValue,
                       ReportingCategoryName = str.ProductTypeName ?? string.Empty,
                       SpiritTypeReportingId = (int?)str.SpiritTypeReportingID ?? 0,
                       Proof = (float?)proof.Value ?? 0,
                       PurchaseId = (int?)productionContent.RecordID ?? 0,
                       DestroyedProof = (float?)dest.ProofGallons ?? 0,
                       ProductionContentProof = (float?)productionContent.ContentValue ?? 0
                   };
        }

        /// <summary>
        /// Returns a collection of produced on hand first of month. 
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        private IEnumerable<OnHandFirstOfMonth> GetProducedOnHandFirstOfMonthQuery(DateTime startDate, DateTime endDate, int userId)
        {
            return from sourceProduction in _db.Production
                   join distiller in _db.AspNetUserToDistiller on sourceProduction.DistillerID equals distiller.DistillerID into distiller_join
                   from distiller in distiller_join.DefaultIfEmpty()
                   join dest in _db.Destruction on sourceProduction.ProductionID equals dest.RecordID into dest_join
                   from dest in dest_join.DefaultIfEmpty()
                   join proof in _db.Proof on sourceProduction.ProofID equals proof.ProofID into proof_join
                   from proof in proof_join.DefaultIfEmpty()
                   join productionContent in _db.ProductionContent on sourceProduction.ProductionID equals productionContent.RecordID into productionContent_join
                   from productionContent in productionContent_join.DefaultIfEmpty()
                   join outputProduction in _db.Production on productionContent.ProductionID equals outputProduction.ProductionID into outputProductionRecord_join
                   from outputProduction in outputProductionRecord_join.DefaultIfEmpty()
                   join productionToSpiritType in _db.ProductionToSpiritTypeReporting on sourceProduction.ProductionID equals productionToSpiritType.ProductionID into productionToSpiritType_join
                   from productionToSpiritType in productionToSpiritType_join.DefaultIfEmpty()
                   join str in _db.SpiritTypeReporting on productionToSpiritType.SpiritTypeReportingID equals str.SpiritTypeReportingID into str_join
                   from str in str_join.DefaultIfEmpty()
                   where
                       distiller.UserId == userId
                       && (sourceProduction.ProductionTypeID == (int)Persistence.BusinessLogicEnums.ProductionType.Fermentation || sourceProduction.ProductionTypeID == (int)Persistence.BusinessLogicEnums.ProductionType.Distillation)
                       && sourceProduction.ProductionEndTime < startDate
                       && sourceProduction.Gauged == true
                       && ((sourceProduction.StatusID == (int)Persistence.BusinessLogicEnums.Status.Active || sourceProduction.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processing || sourceProduction.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processed) || (sourceProduction.StateID == (int)Persistence.BusinessLogicEnums.Status.Destroyed && dest.EndTime > startDate && dest.EndTime < endDate))
                       && (productionContent == null || (productionContent != null && (productionContent.ContentFieldID == (int)Persistence.BusinessLogicEnums.ContenField.ProdDistilledProofGal || productionContent.ContentFieldID == (int)Persistence.BusinessLogicEnums.ContenField.ProdFermentedProofGal)))
                   select new OnHandFirstOfMonth()
                   {
                       ProductionDate = outputProduction.ProductionEndTime != null ? outputProduction.ProductionEndTime : DateTime.MinValue,
                       ReportingCategoryName = str.ProductTypeName ?? string.Empty,
                       SpiritTypeReportingId = (int?)str.SpiritTypeReportingID ?? 0,
                       Proof = (float?)proof.Value ?? 0,
                       DestroyedProof = (float?)dest.ProofGallons ?? 0,
                       ProductionContentProof = (float?)productionContent.ContentValue ?? 0
                   };
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
            var records = GetPurchasedOnHandFirstOfMonthQuery(startDate, endDate, userId);

            if (records.Any())
            {
                foreach (var rec in records)
                {
                    // Search for existing category with matching name
                    var category = storageReportBody.Find(x => x.SpiritTypeReportingID == rec.SpiritTypeReportingId);

                    if (category == null)
                    {
                        var total = rec.Proof + rec.DestroyedProof;

                        if (rec.ProductionContentProof > 0 && rec.ProductionDate >= startDate)
                        {
                            total += rec.ProductionContentProof;
                        }

                        if (total > 0)
                        {
                            // Add category to the list with given produced distilled batch ReportingCategoryName and update relevant rows
                            StorageReportCategory cat = new StorageReportCategory();
                            cat.SpiritTypeReportingID = rec.SpiritTypeReportingId;
                            cat.CategoryName = rec.ReportingCategoryName;
                            cat.r1_OnHandFirstOfMonth += total;
                            cat.r1_OnHandFirstOfMonth = (float)Math.Round(cat.r1_OnHandFirstOfMonth, 3);
                            storageReportBody.Add(cat);
                        }
                    }
                    else
                    {
                        category.r1_OnHandFirstOfMonth += rec.Proof + rec.DestroyedProof;

                        if (rec.ProductionContentProof > 0 && rec.ProductionDate >= startDate)
                        {
                            category.r1_OnHandFirstOfMonth += rec.ProductionContentProof;
                        }

                        category.r1_OnHandFirstOfMonth = (float)Math.Round(category.r1_OnHandFirstOfMonth, 3);
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
            var records = GetProducedOnHandFirstOfMonthQuery(startDate, endDate, userId);

            if (records.Any())
            {
                foreach (var rec in records)
                {
                    // Search for existing category with matching name
                    var category = storageReportBody.Find(x => x.SpiritTypeReportingID == rec.SpiritTypeReportingId);

                    if (category == null)
                    {
                        var total = rec.Proof + rec.DestroyedProof;

                        if (rec.ProductionContentProof > 0 && rec.ProductionDate >= startDate)
                        {
                            total += rec.ProductionContentProof;
                        }

                        if (total > 0)
                        {
                            // Add category to the list with given produced distilled batch ReportingCategoryName and update relevant rows
                            StorageReportCategory cat = new StorageReportCategory();
                            cat.SpiritTypeReportingID = rec.SpiritTypeReportingId;
                            cat.CategoryName = rec.ReportingCategoryName;
                            cat.r1_OnHandFirstOfMonth += total;
                            cat.r1_OnHandFirstOfMonth = (float)Math.Round(cat.r1_OnHandFirstOfMonth, 3);
                            storageReportBody.Add(cat);
                        }
                    }
                    else
                    {
                        category.r1_OnHandFirstOfMonth += rec.Proof + rec.DestroyedProof;

                        if (rec.ProductionContentProof > 0 && rec.ProductionDate >= startDate)
                        {
                            category.r1_OnHandFirstOfMonth += rec.ProductionContentProof;
                        }

                        category.r1_OnHandFirstOfMonth = (float)Math.Round(category.r1_OnHandFirstOfMonth, 3);
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
                (from purchase in _db.Purchase
                 join distiller in _db.AspNetUserToDistiller
                    on purchase.DistillerID equals distiller.DistillerID into distiller_join
                 from distiller in distiller_join.DefaultIfEmpty()
                 join materialDictionary in _db.MaterialDict
                    on purchase.MaterialDictID equals materialDictionary.MaterialDictID into materialDictionary_join
                 from materialDictionary in materialDictionary_join.DefaultIfEmpty()
                 join unitOfMeasurement in _db.UnitOfMeasurement
                    on materialDictionary.UnitOfMeasurementID equals unitOfMeasurement.UnitOfMeasurementID into unitOfMeasurement_join
                 from unitOfMeasurement in unitOfMeasurement_join.DefaultIfEmpty()
                 join destruction in _db.Destruction
                    on purchase.PurchaseID equals destruction.RecordID into destruction_join
                 from destruction in destruction_join.DefaultIfEmpty()
                 join productionContent in _db.ProductionContent
                    on purchase.PurchaseID equals productionContent.RecordID into productionContent_join
                 from productionContent in productionContent_join.DefaultIfEmpty()
                 join proof in _db.Proof
                    on purchase.ProofID equals proof.ProofID into proof_join
                 from proof in proof_join.DefaultIfEmpty()
                 join purchaseToSpiritType in _db.PurchaseToSpiritTypeReporting
                    on purchase.PurchaseID equals purchaseToSpiritType.PurchaseID into purchaseToSpiritType_join
                 from purchaseToSpiritType in purchaseToSpiritType_join.DefaultIfEmpty()
                 join spiritTypeReporting in _db.SpiritTypeReporting
                    on purchaseToSpiritType.SpiritTypeReportingID equals spiritTypeReporting.SpiritTypeReportingID into spiritTypeReporting_join
                 from spiritTypeReporting in spiritTypeReporting_join.DefaultIfEmpty()
                 where
                     distiller.UserId == userId
                     && (purchase.PurchaseTypeID == (int)Persistence.BusinessLogicEnums.PurchaseType.Fermented
                        || purchase.PurchaseTypeID == (int)Persistence.BusinessLogicEnums.PurchaseType.Distilled)
                     && (purchase.StatusID == (int)Persistence.BusinessLogicEnums.Status.Active
                        || purchase.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processing
                        || purchase.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processed)
                     && purchase.PurchaseDate >= startDate
                     && purchase.PurchaseDate <= endDate
                     && (productionContent == null
                        || (productionContent != null && (productionContent.ContentFieldID == (int)Persistence.BusinessLogicEnums.ContenField.PurFermentedProofGal || productionContent.ContentFieldID == (int)Persistence.BusinessLogicEnums.ContenField.PurDistilledProofGal)))
                     && unitOfMeasurement.UnitOfMeasurementID != (int)Persistence.BusinessLogicEnums.UnitOfMeasurement.lb
                 select new
                 {
                     reportingCategoryName = spiritTypeReporting.ProductTypeName ?? string.Empty,
                     spiritTypeReportingId = (int?)spiritTypeReporting.SpiritTypeReportingID ?? 0,
                     purchaseId = (int?)productionContent.RecordID ?? 0,
                     purchaseProof = (float?)proof.Value ?? 0,
                     productionProof = (float?)productionContent.ContentValue ?? 0,
                     destroyedProof = (float?)destruction.ProofGallons ?? 0
                 }).DefaultIfEmpty();

            if (records.First() != null)
            {
                // store pruchase ids
                List<int> purchIdSet = new List<int>();

                foreach (var rec in records)
                {
                    // Search for existing category with matching name
                    var category = storageReportBody.Find(x => x.SpiritTypeReportingID == rec.spiritTypeReportingId);

                    if (category == null)
                    {
                        var total = rec.purchaseProof + rec.productionProof + rec.destroyedProof;
                        purchIdSet.Add(rec.purchaseId);

                        if (total > 0)
                        {
                            // Add category to the list with given produced distilled batch ReportingCategoryName and update relevant rows
                            StorageReportCategory cat = new StorageReportCategory();
                            cat.SpiritTypeReportingID = rec.spiritTypeReportingId;
                            cat.CategoryName = rec.reportingCategoryName;
                            cat.r2_DepositedInBulkStorage += total;
                            cat.r2_DepositedInBulkStorage = (float)Math.Round(cat.r2_DepositedInBulkStorage, 3);
                            storageReportBody.Add(cat);
                        }
                    }
                    else
                    {
                        // check to ensure proof for that purchase id was not already added to total.
                        if(purchIdSet.Contains(rec.purchaseId))
                        {
                            category.r2_DepositedInBulkStorage += rec.productionProof + rec.destroyedProof;
                        }
                        else
                        {
                            category.r2_DepositedInBulkStorage += rec.purchaseProof + rec.productionProof + rec.destroyedProof;
                            purchIdSet.Add(rec.purchaseId);
                        }
                        category.r2_DepositedInBulkStorage = (float)Math.Round(category.r2_DepositedInBulkStorage, 3);
                    }
                }
            }
        }

        /// <summary>
        /// 2. Deposited in bulk storage 
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="userId"></param>
        /// <param name="storageReportBody"></param>
        private void GetProducedDepositedToStorage(DateTime startDate, DateTime endDate, int userId, ref List<StorageReportCategory> storageReportBody)
        {
            // Query distilled production records transferred to storage account
            var records =
                (from production in _db.Production
                 join distiller in _db.AspNetUserToDistiller on production.DistillerID equals distiller.DistillerID into distiller_join
                 from distiller in distiller_join.DefaultIfEmpty()
                 join destruction in _db.Destruction on production.ProductionID equals destruction.RecordID into destruction_join
                 from destruction in destruction_join.DefaultIfEmpty()
                 join productionContent in _db.ProductionContent on production.ProductionID equals productionContent.RecordID into productionContent_join // gets amounts of materials used in production
                 from productionContent in productionContent_join.DefaultIfEmpty()
                 join productionOutput in _db.Production on productionContent.ProductionID equals productionOutput.ProductionID into productionOutput_join
                 from productionOutput in productionOutput_join.DefaultIfEmpty()
                 join proof in _db.Proof on production.ProofID equals proof.ProofID into proof_join
                 from proof in proof_join.DefaultIfEmpty()
                 join productionToSpiritType in _db.ProductionToSpiritTypeReporting on production.ProductionID equals productionToSpiritType.ProductionID into productionToSpiritType_join
                 from productionToSpiritType in productionToSpiritType_join.DefaultIfEmpty()
                 join spiritTypeReporting in _db.SpiritTypeReporting on productionToSpiritType.SpiritTypeReportingID equals spiritTypeReporting.SpiritTypeReportingID into spiritTypeReporting_join
                 from spiritTypeReporting in spiritTypeReporting_join.DefaultIfEmpty()
                 where
                    distiller.UserId == userId
                 && (production.ProductionTypeID == (int)Persistence.BusinessLogicEnums.ProductionType.Fermentation || production.ProductionTypeID == (int)Persistence.BusinessLogicEnums.ProductionType.Distillation)
                 && production.ProductionEndTime >= startDate
                 && production.ProductionEndTime <= endDate
                 && production.Gauged == true
                 && (production.StatusID == (int)Persistence.BusinessLogicEnums.Status.Active
                    || production.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processing
                    || (production.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processed && productionOutput.ProductionTypeID == (int)Persistence.BusinessLogicEnums.ProductionType.Distillation)
                    || (production.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processed && productionOutput.ProductionTypeID == (int)Persistence.BusinessLogicEnums.ProductionType.Blending && productionOutput.ProductionEndTime > endDate)
                    || (production.StatusID == (int)Persistence.BusinessLogicEnums.Status.Destroyed && destruction.EndTime > endDate))
                 && (productionContent == null || (productionContent != null && (productionContent.ContentFieldID == (int)Persistence.BusinessLogicEnums.ContenField.ProdDistilledProofGal || productionContent.ContentFieldID == (int)Persistence.BusinessLogicEnums.ContenField.ProdFermentedProofGal))) // 20 = ProdDistilledProofGal, 23 = ProdFermentedProofGal
                 select new
                 {
                     productionStatusId = (int?)production.StatusID ?? 0,
                     productionStateId = (int?)production.StateID ?? 0,
                     productionOutputStausId = (int?)productionOutput.StatusID ?? 0,
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
                        float total = 0f;

                        if (rec.productionStatusId == (int)Persistence.BusinessLogicEnums.Status.Active || rec.productionStatusId == (int)Persistence.BusinessLogicEnums.Status.Processed)
                        {
                            total += rec.proof + rec.destroyedProof + rec.productionContentProof;
                        }
                        else if (rec.productionStatusId == (int)Persistence.BusinessLogicEnums.Status.Processing)
                        {
                            if (rec.proof != 0f && rec.productionStateId != (int)Persistence.BusinessLogicEnums.State.Fermented)
                            {
                                total += rec.proof + rec.destroyedProof;
                            }
                            else
                            {
                                total += rec.proof + rec.destroyedProof + rec.productionContentProof;
                            }
                        }

                        if (total > 0)
                        {
                            // Add category to the list with given produced distilled batch ReportingCategoryName and update relevant rows
                            StorageReportCategory cat = new StorageReportCategory();
                            cat.SpiritTypeReportingID = rec.spiritTypeReportingId;
                            cat.CategoryName = rec.reportingCategoryName;
                            cat.r2_DepositedInBulkStorage += total;
                            cat.r2_DepositedInBulkStorage = (float)Math.Round(cat.r2_DepositedInBulkStorage, 3);
                            storageReportBody.Add(cat);
                        }
                    }
                    else
                    {
                        category.r2_DepositedInBulkStorage += rec.proof + rec.productionContentProof + rec.destroyedProof;
                        category.r2_DepositedInBulkStorage = (float)Math.Round(category.r2_DepositedInBulkStorage, 3);
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
                storage.r6_TotalLines1Through5 = (float)Math.Round(storage.r1_OnHandFirstOfMonth + storage.r2_DepositedInBulkStorage + storage.r4_ReturnedToBulkStorage, 3);
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
                (from purchase in _db.Purchase
                 join distiller in _db.AspNetUserToDistiller
                    on purchase.DistillerID equals distiller.DistillerID into distiller_join
                 from distiller in distiller_join.DefaultIfEmpty()
                 join materialDictionary in _db.MaterialDict
                    on purchase.MaterialDictID equals materialDictionary.MaterialDictID into materialDictionary_join
                 from materialDictionary in materialDictionary_join.DefaultIfEmpty()
                 join unitOfMeasurement in _db.UnitOfMeasurement
                    on materialDictionary.UnitOfMeasurementID equals unitOfMeasurement.UnitOfMeasurementID into unitOfMeasurement_join
                 from unitOfMeasurement in unitOfMeasurement_join.DefaultIfEmpty()
                 join productionContent in _db.ProductionContent
                    on purchase.PurchaseID equals productionContent.RecordID into productionContent_join
                 from productionContent in productionContent_join.DefaultIfEmpty()
                 join production in _db.Production
                    on productionContent.ProductionID equals production.ProductionID into production_join
                 from production in production_join.DefaultIfEmpty()
                 join purchaseToSpiritType in _db.PurchaseToSpiritTypeReporting
                    on purchase.PurchaseID equals purchaseToSpiritType.PurchaseID into purchaseToSpiritType_join
                 from purchaseToSpiritType in purchaseToSpiritType_join.DefaultIfEmpty()
                 join spiritTypeReporting in _db.SpiritTypeReporting
                    on purchaseToSpiritType.SpiritTypeReportingID equals spiritTypeReporting.SpiritTypeReportingID into spiritTypeReporting_join
                 from spiritTypeReporting in spiritTypeReporting_join.DefaultIfEmpty()
                 where
                     distiller.UserId == userId
                     && purchase.PurchaseTypeID == (int)Persistence.BusinessLogicEnums.PurchaseType.Distilled
                     && production.ProductionTypeID == (int)Persistence.BusinessLogicEnums.ProductionType.Blending
                     && (purchase.StatusID == (int)Persistence.BusinessLogicEnums.Status.Active
                        || purchase.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processing
                        || purchase.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processed)
                     && purchase.PurchaseDate < endDate
                     && production.ProductionEndTime >= startDate
                     && production.ProductionEndTime <= endDate
                     && productionContent.ContentFieldID == (int)Persistence.BusinessLogicEnums.ContenField.PurDistilledProofGal
                     && unitOfMeasurement.UnitOfMeasurementID != (int)Persistence.BusinessLogicEnums.UnitOfMeasurement.lb
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
                            cat.r17_TransferredToProcessingAccount = (float)Math.Round(cat.r17_TransferredToProcessingAccount, 3);
                            storageReportBody.Add(cat);
                        }
                    }
                    else
                    {
                        category.r17_TransferredToProcessingAccount += rec.proofGal;
                        category.r17_TransferredToProcessingAccount = (float)Math.Round(category.r17_TransferredToProcessingAccount, 3);
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
                (from sourceProductionRecord in _db.Production
                 join distiller in _db.AspNetUserToDistiller
                    on sourceProductionRecord.DistillerID equals distiller.DistillerID into distiller_join
                 from distiller in distiller_join.DefaultIfEmpty()
                 join productionContent in _db.ProductionContent
                    on sourceProductionRecord.ProductionID equals productionContent.RecordID into productionContent_join
                 from productionContent in productionContent_join.DefaultIfEmpty()
                 join outputProductionRecord in _db.Production
                    on productionContent.ProductionID equals outputProductionRecord.ProductionID into outputProductionRecord_join
                 from outputProductionRecord in outputProductionRecord_join.DefaultIfEmpty()
                 join productionToSpiritType in _db.ProductionToSpiritTypeReporting
                    on sourceProductionRecord.ProductionID equals productionToSpiritType.ProductionID into productionToSpiritType_join
                 from productionToSpiritType in productionToSpiritType_join.DefaultIfEmpty()
                 join spiritType in _db.SpiritTypeReporting
                    on productionToSpiritType.SpiritTypeReportingID equals spiritType.SpiritTypeReportingID into spiritType_join
                 from spiritType in spiritType_join.DefaultIfEmpty()
                 where
                     distiller.UserId == userId
                     && sourceProductionRecord.ProductionTypeID == (int)Persistence.BusinessLogicEnums.ProductionType.Distillation
                     && outputProductionRecord.ProductionTypeID == (int)Persistence.BusinessLogicEnums.ProductionType.Blending
                     && (sourceProductionRecord.StatusID == (int)Persistence.BusinessLogicEnums.Status.Active
                        || sourceProductionRecord.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processing
                        || sourceProductionRecord.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processed)
                     && productionContent.ContentFieldID == (int)Persistence.BusinessLogicEnums.ContenField.ProdDistilledProofGal // ProdDistilledProofGal
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
                            cat.r17_TransferredToProcessingAccount = (float)Math.Round(cat.r17_TransferredToProcessingAccount, 3);
                            storageReportBody.Add(cat);
                        }
                    }
                    else
                    {
                        category.r17_TransferredToProcessingAccount += rec.proofGal;
                        category.r17_TransferredToProcessingAccount = (float)Math.Round(category.r17_TransferredToProcessingAccount, 3);
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
                (from purchase in _db.Purchase
                 join distiller in _db.AspNetUserToDistiller
                    on purchase.DistillerID equals distiller.DistillerID into distiller_join
                 from distiller in distiller_join.DefaultIfEmpty()
                 join materialDictionary in _db.MaterialDict
                    on purchase.MaterialDictID equals materialDictionary.MaterialDictID into materialDictionary_join
                 from materialDictionary in materialDictionary_join.DefaultIfEmpty()
                 join unitOfMeasurement in _db.UnitOfMeasurement
                    on materialDictionary.UnitOfMeasurementID equals unitOfMeasurement.UnitOfMeasurementID into unitOfMeasurement_join
                 from unitOfMeasurement in unitOfMeasurement_join.DefaultIfEmpty()
                 join productionContent in _db.ProductionContent
                    on purchase.PurchaseID equals productionContent.RecordID into productionContent_join
                 from productionContent in productionContent_join.DefaultIfEmpty()
                 join production in _db.Production
                    on productionContent.ProductionID equals production.ProductionID into production_join
                 from production in production_join.DefaultIfEmpty()
                 join productionToSpiritType in _db.PurchaseToSpiritTypeReporting
                    on purchase.PurchaseID equals productionToSpiritType.PurchaseID into productionToSpiritType_join
                 from productionToSpiritType in productionToSpiritType_join.DefaultIfEmpty()
                 join spiritType in _db.SpiritTypeReporting
                    on productionToSpiritType.SpiritTypeReportingID equals spiritType.SpiritTypeReportingID into spiritType_join
                 from spiritType in spiritType_join.DefaultIfEmpty()
                 where
                     distiller.UserId == userId
                     && (purchase.PurchaseTypeID == (int)Persistence.BusinessLogicEnums.PurchaseType.Fermented
                        || purchase.PurchaseTypeID == (int)Persistence.BusinessLogicEnums.PurchaseType.Distilled)
                     && production.ProductionTypeID == (int)Persistence.BusinessLogicEnums.ProductionType.Distillation
                     && (purchase.StatusID == (int)Persistence.BusinessLogicEnums.Status.Active
                        || purchase.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processing
                        || purchase.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processed)
                     && purchase.PurchaseDate < endDate
                     && production.ProductionEndTime >= startDate
                     && production.ProductionEndTime <= endDate
                     && (productionContent.ContentFieldID == (int)Persistence.BusinessLogicEnums.ContenField.PurFermentedProofGal
                     || productionContent.ContentFieldID == (int)Persistence.BusinessLogicEnums.ContenField.PurDistilledProofGal)
                     && unitOfMeasurement.UnitOfMeasurementID != (int)Persistence.BusinessLogicEnums.UnitOfMeasurement.lb
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
                            cat.r18_TransferredToProductionAccount = (float)Math.Round(cat.r18_TransferredToProductionAccount, 3);
                            storageReportBody.Add(cat);
                        }
                    }
                    else
                    {
                        category.r18_TransferredToProductionAccount += rec.proofGal;
                        category.r18_TransferredToProductionAccount = (float)Math.Round(category.r18_TransferredToProductionAccount, 3);
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
                (from sourceProductionRecord in _db.Production
                 join distiller in _db.AspNetUserToDistiller
                    on sourceProductionRecord.DistillerID equals distiller.DistillerID into distiller_join
                 from distiller in distiller_join.DefaultIfEmpty()
                 join productionContent in _db.ProductionContent
                    on sourceProductionRecord.ProductionID equals productionContent.RecordID into productionContent_join
                 from productionContent in productionContent_join.DefaultIfEmpty()
                 join outputProductionRecord in _db.Production
                    on productionContent.ProductionID equals outputProductionRecord.ProductionID into outputProductionRecord_join
                 from outputProductionRecord in outputProductionRecord_join.DefaultIfEmpty()
                 join productionToSpiritType in _db.ProductionToSpiritTypeReporting
                    on sourceProductionRecord.ProductionID equals productionToSpiritType.ProductionID into productionToSpiritType_join
                 from productionToSpiritType in productionToSpiritType_join.DefaultIfEmpty()
                 join spiritType in _db.SpiritTypeReporting
                    on productionToSpiritType.SpiritTypeReportingID equals spiritType.SpiritTypeReportingID into spiritType_join
                 from spiritType in spiritType_join.DefaultIfEmpty()
                 where
                     distiller.UserId == userId
                     && (sourceProductionRecord.ProductionTypeID == (int)Persistence.BusinessLogicEnums.ProductionType.Fermentation || sourceProductionRecord.ProductionTypeID == (int)Persistence.BusinessLogicEnums.ProductionType.Distillation)
                     && outputProductionRecord.ProductionTypeID == (int)Persistence.BusinessLogicEnums.ProductionType.Distillation
                     && (sourceProductionRecord.StatusID == (int)Persistence.BusinessLogicEnums.Status.Active || sourceProductionRecord.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processing || sourceProductionRecord.StatusID == (int)Persistence.BusinessLogicEnums.Status.Processed)
                 && (productionContent == null || (productionContent != null && (productionContent.ContentFieldID == (int)Persistence.BusinessLogicEnums.ContenField.ProdDistilledProofGal || productionContent.ContentFieldID == (int)Persistence.BusinessLogicEnums.ContenField.ProdFermentedProofGal)))
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
                            cat.r18_TransferredToProductionAccount = (float)Math.Round(category.r18_TransferredToProductionAccount, 3);
                            storageReportBody.Add(cat);
                        }
                    }
                    else
                    {
                        category.r18_TransferredToProductionAccount += rec.proofGal;
                        category.r18_TransferredToProductionAccount = (float)Math.Round(category.r18_TransferredToProductionAccount, 3);
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
            var prodDestroyed = (from destruction in _db.Destruction
                                 join production in _db.Production
                                    on destruction.RecordID equals production.ProductionID
                                 join distiller in _db.AspNetUserToDistiller
                                    on production.DistillerID equals distiller.DistillerID
                                 join productionToSpiritType in _db.ProductionToSpiritTypeReporting
                                    on production.ProductionID equals productionToSpiritType.ProductionID
                                 join spiritType in _db.SpiritTypeReporting
                                    on productionToSpiritType.SpiritTypeReportingID equals spiritType.SpiritTypeReportingID
                                 where
                                    distiller.UserId == userId
                                    && production.ProductionTypeID == (int)Persistence.BusinessLogicEnums.ProductionType.Distillation
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
                            cat.r20_Destroyed = (float)Math.Round(cat.r20_Destroyed, 3);
                            storageReportBody.Add(cat);
                        }
                    }
                    else
                    {
                        category.r20_Destroyed += rec.proof;
                        category.r20_Destroyed = (float)Math.Round(category.r20_Destroyed, 3);
                    }
                }
            }

            // Query purchase batches destroyed in Storage Account
            var purDestroyed = (from dest in _db.Destruction
                                join pur in _db.Purchase
                                    on dest.RecordID equals pur.PurchaseID
                                join dist in _db.AspNetUserToDistiller
                                    on pur.DistillerID equals dist.DistillerID
                                join mDic in _db.MaterialDict
                                    on pur.MaterialDictID equals mDic.MaterialDictID into mDic_join
                                from mDic in mDic_join.DefaultIfEmpty()
                                join uOm in _db.UnitOfMeasurement
                                    on mDic.UnitOfMeasurementID equals uOm.UnitOfMeasurementID into uOm_join
                                from uOm in uOm_join.DefaultIfEmpty()
                                join pur_str in _db.PurchaseToSpiritTypeReporting
                                    on pur.PurchaseID equals pur_str.PurchaseID
                                join spiritType in _db.SpiritTypeReporting
                                    on pur_str.SpiritTypeReportingID equals spiritType.SpiritTypeReportingID
                                where
                                    dist.UserId == userId
                                    && (pur.PurchaseTypeID == (int)Persistence.BusinessLogicEnums.PurchaseType.Fermented || pur.PurchaseTypeID == (int)Persistence.BusinessLogicEnums.PurchaseType.Distilled)
                                    && pur.StateID == (int)Persistence.BusinessLogicEnums.Status.Destroyed
                                    && (pur.StateID == (int)Persistence.BusinessLogicEnums.State.Fermented || pur.StateID == (int)Persistence.BusinessLogicEnums.State.Distilled)
                                    && dest.EndTime > startDate
                                    && dest.EndTime < endDate
                                    && dest.WorkflowType == "Purchase"
                                    && uOm.UnitOfMeasurementID != (int)Persistence.BusinessLogicEnums.UnitOfMeasurement.lb
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
                            cat.r20_Destroyed = (float)Math.Round(cat.r20_Destroyed, 3);
                            storageReportBody.Add(cat);
                        }
                    }
                    else
                    {
                        category.r20_Destroyed += rec.proof;
                        category.r20_Destroyed = (float)Math.Round(category.r20_Destroyed, 3);
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
            var nextStart = startDate.AddMonths(1);
            var nextEnd = endDate.Date.AddMonths(1).AddHours(11).AddMinutes(59).AddSeconds(59);

            // On hand end of month is the same as on hand first of month for next month
            var records = GetPurchasedOnHandFirstOfMonthQuery(nextStart, nextEnd, userId);

            if (records.Any())
            {
                // store purchase ids
                List<int> purchIdSet = new List<int>();

                foreach (var rec in records)
                {
                    // Search for existing category with matching name
                    var category = storageReportBody.Find(x => x.SpiritTypeReportingID == rec.SpiritTypeReportingId);

                    if (category == null)
                    {
                        var total = rec.Proof + rec.DestroyedProof;
                        purchIdSet.Add(rec.PurchaseId);

                        if (rec.ProductionContentProof > 0 && rec.ProductionDate >= nextStart)
                        {
                            total += rec.ProductionContentProof;
                        }

                        if (total > 0)
                        {
                            // Add category to the list with given produced distilled batch ReportingCategoryName and update relevant rows
                            StorageReportCategory cat = new StorageReportCategory();
                            cat.SpiritTypeReportingID = rec.SpiritTypeReportingId;
                            cat.CategoryName = rec.ReportingCategoryName;
                            cat.r23_OnHandEndOfMonth += total;
                            cat.r23_OnHandEndOfMonth = (float)Math.Round(cat.r23_OnHandEndOfMonth, 3);
                            storageReportBody.Add(cat);
                        }
                    }
                    else
                    {
                        if (purchIdSet.Contains(rec.PurchaseId))
                        {
                            category.r23_OnHandEndOfMonth += rec.DestroyedProof;
                        }
                        else
                        {
                            category.r23_OnHandEndOfMonth += rec.Proof + rec.DestroyedProof;
                            purchIdSet.Add(rec.PurchaseId);
                        }

                        if (rec.ProductionContentProof > 0 && rec.ProductionDate >= nextStart)
                        {
                            category.r23_OnHandEndOfMonth += rec.ProductionContentProof;
                        }
                        category.r23_OnHandEndOfMonth = (float)Math.Round(category.r23_OnHandEndOfMonth, 3);
                    }
                }
            }

            // On hand end of month is the same as on hand first of month for next month
            records = GetProducedOnHandFirstOfMonthQuery(nextStart, nextEnd, userId);

            if (records.Any())
            {
                foreach (var rec in records)
                {
                    // Search for existing category with matching name
                    var category = storageReportBody.Find(x => x.SpiritTypeReportingID == rec.SpiritTypeReportingId);

                    if (category == null)
                    {
                        var total = rec.Proof + rec.DestroyedProof;

                        if (rec.ProductionContentProof > 0 && rec.ProductionDate >= nextStart)
                        {
                            total += rec.ProductionContentProof;
                        }

                        if (total > 0)
                        {
                            // Add category to the list with given produced distilled batch ReportingCategoryName and update relevant rows
                            StorageReportCategory cat = new StorageReportCategory();
                            cat.SpiritTypeReportingID = rec.SpiritTypeReportingId;
                            cat.CategoryName = rec.ReportingCategoryName;
                            cat.r23_OnHandEndOfMonth += total;
                            cat.r23_OnHandEndOfMonth = (float)Math.Round(cat.r23_OnHandEndOfMonth, 3);
                            storageReportBody.Add(cat);
                        }
                    }
                    else
                    {
                        category.r23_OnHandEndOfMonth += rec.Proof + rec.DestroyedProof;

                        if (rec.ProductionContentProof > 0 && rec.ProductionDate >= nextStart)
                        {
                            category.r23_OnHandEndOfMonth += rec.ProductionContentProof;
                            category.r23_OnHandEndOfMonth = (float)Math.Round(category.r23_OnHandEndOfMonth, 3);
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
                storage.r24_Lines7Through23 = (float)Math.Round(storage.r7_TaxPaid + storage.r17_TransferredToProcessingAccount + storage.r18_TransferredToProductionAccount
                    + storage.r19_TransferredToOtherBondedPremises + storage.r20_Destroyed + storage.r22_OtherLosses + storage.r23_OnHandEndOfMonth, 3);
            }
        }
    }
}