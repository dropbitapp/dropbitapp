using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using WebApp.Models;

namespace WebApp.Helpers.Tests
{
    public static class TestContstant
    {
        public static ReportHeader GetProductionReportHeader()
        {
            ReportHeader header = new ReportHeader();
            header.ProprietorName = "Test Distillery";
            header.EIN = "12-3456789";
            header.ReportDate = "October 2017";
            header.PlantAddress = "123 Cognac Drive Renton 98059 WASHINGTON";
            header.DSP = "DSP-WA-21086";

            return header;
        }
    }

    [TestClass()]
    public class DataLayerTests
    {
        private readonly int _userId = 7; /*test account*/
        DataLayer _dl = new DataLayer();
        DistilDBContext _db = new DistilDBContext();

        /// <summary>
        /// Table enum contains dictionary tables identifiers for easy mapping during
        /// the table cleanup stage of test pass
        /// </summary>
        private enum Table
        {
            None = 0,
            Spirit,
            Vendor,
            Storage,
            MaterialDict,
            Purchase,
            Production
        }

        // This covers a full workflow test for Brandy Under 170
        /*
         * We need to make Brandy Under 170 and test its appearance in 
         * Production/Storage/Processing Report
         * 
         * 
         */
        [TestMethod()]
        public void Buy_Wine_Make_Brandy_Under_170_Test()
        {
            // Arrange
            List<Tuple<int/*recordId*/, Table/*table enum vaue*/>> tablesForCleanupTupleList = new List<Tuple<int, Table>>();
            int spiritId = 0;
            int vendorId = 0;
            int storageId = 0;
            int wineMaterialId = 0;
            int waterMaterialId = 0;
            int purchaseId = 0;
            int productionId = 0;
            try
            {
                #region Dictionary
                //  dictionary setup
                SpiritObject spirit = new SpiritObject();
                spirit.SpiritName = "Brandy Under 170";
                spirit.ProcessingReportTypeID = 12;

                spiritId = _dl.CreateSpirit(_userId, spirit);
                tablesForCleanupTupleList.Add(Tuple.Create(spiritId, Table.Spirit));

                // setup Vendor object
                VendorObject vendor = new VendorObject();
                vendor.VendorName = "testVendor";

                vendorId = _dl.CreateVendor(_userId, vendor);
                tablesForCleanupTupleList.Add(Tuple.Create(vendorId, Table.Vendor));

                // setup Storage Object
                StorageObject storage = new StorageObject();
                storage.StorageName = "testStorage";
                storage.SerialNumber = "2H29NNS";

                storageId = _dl.CreateStorage(_userId, storage);
                tablesForCleanupTupleList.Add(Tuple.Create(storageId, Table.Storage));


                // setup Material Object
                // wine
                {
                    RawMaterialObject wineMaterial = new RawMaterialObject();
                    wineMaterial.RawMaterialName = "Wine For Brandy";
                    wineMaterial.MaterialCategoryID = 2;
                    wineMaterial.UnitType = "gal";
                    wineMaterial.UnitTypeId = 1;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Fermented = true;
                    wineMaterial.PurchaseMaterialTypes = materialBoolTypes;

                    wineMaterialId = _dl.CreateRawMaterial(_userId, wineMaterial);
                    tablesForCleanupTupleList.Add(Tuple.Create(wineMaterialId, Table.MaterialDict));
                }

                // water
                {
                    RawMaterialObject waterMaterial = new RawMaterialObject();
                    waterMaterial.RawMaterialName = "Water";
                    waterMaterial.UnitType = "gal";
                    waterMaterial.UnitTypeId = 1;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Additive = true;
                    waterMaterial.PurchaseMaterialTypes = materialBoolTypes;

                    waterMaterialId = _dl.CreateRawMaterial(_userId, waterMaterial);
                    tablesForCleanupTupleList.Add(Tuple.Create(waterMaterialId, Table.MaterialDict));
                }
                #endregion

                #region Purchase
                // create Purchase Record (minimal required fields)
                PurchaseObject purchO = new PurchaseObject();
                purchO.PurBatchName = "test7Purchase";
                purchO.PurchaseType = "Fermented";
                purchO.PurchaseDate = new DateTime(2017, 09, 1);
                purchO.Quantity = 100f; // 100 gallons
                purchO.VolumeByWeight = 0f;
                purchO.AlcoholContent = 9f;
                purchO.ProofGallon = 18f;
                purchO.RecordId = wineMaterialId;
                purchO.Price = 350f;
                purchO.VendorId = vendorId;

                List<StorageObject> storageList = new List<StorageObject>();
                StorageObject storageObject = new StorageObject();
                storageObject.StorageId = storageId;
                storageList.Add(storageObject);
                purchO.Storage = storageList;

                purchO.SpiritTypeReportingID = 11;
                purchO.Gauged = true;

                purchaseId = _dl.CreatePurchase(purchO, _userId);
                tablesForCleanupTupleList.Add(Tuple.Create(purchaseId, Table.Purchase));
                #endregion

                #region Production
                // create 1st Production Distillation Record and don't mark it as Gauged
                ProductionObject prodO = new ProductionObject();
                prodO.BatchName = "test1stDistillRun";
                prodO.ProductionDate = new DateTime(2017, 09, 3);
                prodO.ProductionStart = new DateTime(2017, 09, 3);
                prodO.ProductionEnd = new DateTime(2017, 09, 3);
                prodO.SpiritCutId = 11; // mixed
                prodO.Gauged = false;
                prodO.ProductionType = "Distillation";
                prodO.Quantity = 50f; //50 gallons of alcohol
                prodO.VolumeByWeight = 0f;
                prodO.AlcoholContent = 80f; // 80%
                prodO.ProofGallon = 80f; // 80pfg
                prodO.Storage = storageList; // we are using the same storage id as we use for Purchase to keep things simple
                prodO.SpiritTypeReportingID = 3; // brandy under 170
                prodO.MaterialKindReportingID = 94; // grape brandy
                prodO.ProductionTypeId = 2;

                List<ObjInfo4Burndwn> usedMats = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat = new ObjInfo4Burndwn();
                uMat.ID = purchaseId;
                uMat.OldVal = 0f;
                uMat.NewVal = purchO.Quantity;
                uMat.DistillableOrigin = "pur";
                uMat.BurningDownMethod = "volume";

                usedMats.Add(uMat);

                prodO.UsedMats = usedMats;

                productionId = _dl.CreateProduction(prodO, _userId);
                tablesForCleanupTupleList.Add(Tuple.Create(productionId, Table.Production));

                // create 2nd Production Distillation Record and mark it as Gauged
                ProductionObject prodO2 = new ProductionObject();
                prodO2.BatchName = "testGaugedDistillRun";
                prodO2.ProductionDate = new DateTime(2017, 09, 4);
                prodO2.ProductionStart = new DateTime(2017, 09, 4);
                prodO2.ProductionEnd = new DateTime(2017, 09, 4);
                prodO2.SpiritCutId = 9; // hearts
                prodO2.Gauged = true;
                prodO2.ProductionType = "Distillation";
                prodO2.Quantity = 15f; // 15 gallons of alcohol
                prodO2.VolumeByWeight = 0f;
                prodO2.AlcoholContent = 60f; // 60%
                prodO2.ProofGallon = 18f; // 18pfg
                prodO2.Storage = storageList; // we are using the same storage id as we use for Purchase to keep things simple
                prodO2.SpiritTypeReportingID = 3; // brandy under 170
                prodO2.MaterialKindReportingID = 94; // grape brandy
                prodO2.ProductionTypeId = 2;

                List<ObjInfo4Burndwn> usedMats4Gauge = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat4Gauged = new ObjInfo4Burndwn();
                uMat4Gauged.ID = productionId;
                uMat4Gauged.OldVal = 0f;
                uMat4Gauged.NewVal = prodO.Quantity;
                uMat4Gauged.DistillableOrigin = "prod";
                uMat4Gauged.BurningDownMethod = "volume";

                usedMats4Gauge.Add(uMat4Gauged);
                prodO2.UsedMats = usedMats4Gauge;

                productionId = _dl.CreateProduction(prodO2, _userId); // here productionId is overriden with a new productionId of the new Gauged record
                tablesForCleanupTupleList.Add(Tuple.Create(productionId, Table.Production));

                // create Production Blending Record
                ProductionObject prodBlend = new ProductionObject();
                prodBlend.BatchName = "testProdBlend";
                prodBlend.ProductionDate = new DateTime(2017, 09, 5);
                prodBlend.ProductionStart = new DateTime(2017, 09, 5);
                prodBlend.ProductionEnd = new DateTime(2017, 09, 5);
                prodBlend.Gauged = true;
                prodBlend.ProductionType = "Blending";
                prodBlend.Quantity = 22.5f; // 22.5 gallons of alcohol
                prodBlend.VolumeByWeight = 0f;
                prodBlend.AlcoholContent = 40f; // 40%
                prodBlend.ProofGallon = 18f; // 18pfg
                prodBlend.Storage = storageList; // we are using the same storage id as we use for Purchase to keep things simple
                prodBlend.SpiritTypeReportingID = 3; // brandy under 170
                prodBlend.MaterialKindReportingID = 94; // grape brandy
                prodBlend.SpiritId = spiritId;
                prodBlend.ProductionTypeId = 3;

                List<ObjInfo4Burndwn> usedMats4Blend = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat4Blend = new ObjInfo4Burndwn();
                uMat4Blend.ID = productionId;
                uMat4Blend.OldVal = 0f;
                uMat4Blend.NewVal = prodO2.Quantity;
                uMat4Blend.DistillableOrigin = "prod";
                uMat4Blend.BurningDownMethod = "volume";

                usedMats4Blend.Add(uMat4Blend);
                prodBlend.UsedMats = usedMats4Blend;

                List<BlendingAdditive> blendAdditives = new List<BlendingAdditive>();
                BlendingAdditive blendAd = new BlendingAdditive();
                blendAd.RawMaterialId = waterMaterialId;
                blendAd.RawMaterialQuantity = 7.5f;
                blendAd.RawMaterialName = "Water";
                blendAd.UnitOfMeasurement = "gal";

                blendAdditives.Add(blendAd);

                prodBlend.BlendingAdditives = blendAdditives;

                productionId = _dl.CreateProduction(prodBlend, _userId); // here productionId is overriden with a new productionId of the new Gauged record
                tablesForCleanupTupleList.Add(Tuple.Create(productionId, Table.Production));

                // create Production Bottling Record
                ProductionObject prodBottl = new ProductionObject();
                prodBottl.BatchName = "testProdBottling ";
                prodBottl.ProductionDate = new DateTime(2017, 09, 6);
                prodBottl.ProductionStart = new DateTime(2017, 09, 6);
                prodBottl.ProductionEnd = new DateTime(2017, 09, 6);
                prodBottl.Gauged = true;
                prodBottl.ProductionType = "Bottling";
                prodBottl.Quantity = 22.39f; // 22.39 gallons of alcohol
                prodBottl.VolumeByWeight = 0f;
                prodBottl.AlcoholContent = 40f; // 40%
                prodBottl.ProofGallon = 17.9f; // 17.9 pfg
                prodBottl.Storage = storageList; // we are using the same storage id as we use for Purchase to keep things simple
                prodBottl.SpiritTypeReportingID = 3; // brandy under 170
                prodBottl.MaterialKindReportingID = 94; // grape brandy
                prodBottl.SpiritId = spiritId;
                prodO.ProductionTypeId = 4;

                List<ObjInfo4Burndwn> usedMats4Bottl = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat4Bottl = new ObjInfo4Burndwn();
                uMat4Bottl.ID = productionId;
                uMat4Bottl.OldVal = 0f;
                uMat4Bottl.NewVal = prodBlend.Quantity;
                uMat4Bottl.DistillableOrigin = "prod";
                uMat4Bottl.BurningDownMethod = "volume";

                usedMats4Bottl.Add(uMat4Bottl);
                prodBottl.UsedMats = usedMats4Bottl;

                BottlingObject bottlingObj = new BottlingObject();
                bottlingObj.CaseCapacity = 12;
                bottlingObj.CaseQuantity = 9.42f;
                bottlingObj.BottleCapacity = 750f;
                bottlingObj.BottleQuantity = 113;

                prodBottl.BottlingInfo = bottlingObj;

                prodBottl.GainLoss = .10f;

                prodBottl.FillTestList = null;

                productionId = _dl.CreateProduction(prodBottl, _userId); // here productionId is overriden with a new productionId of the new Gauged record
                tablesForCleanupTupleList.Add(Tuple.Create(productionId, Table.Production));

                #endregion

                #region Reports setup

                // Report Header
                ReportHeader reportHeaderE = new ReportHeader();
                reportHeaderE.ProprietorName = "Test Distillery";
                reportHeaderE.EIN = "12-3456789";
                reportHeaderE.ReportDate = "September 2017";
                reportHeaderE.PlantAddress = "123 Cognac Drive Renton WASHINGTON 98059";
                reportHeaderE.DSP = "DSP-WA-21086";

                // reporting time range
                DateTime start = new DateTime(2017, 09, 01);
                DateTime end = new DateTime(2017, 09, 30);

                /* PRODUCTION REPORT */

                ProdReportPart1 part1E = new ProdReportPart1();
                part1E.ProccessingAcct = 18f;
                part1E.ProducedTotal = 18f;
                part1E.Recd4RedistilL17 = 0f;
                part1E.Recd4RedistilaltionL15 = 0f;
                part1E.StorageAcct = 0f;
                part1E.SpiritCatName = "BrandyUnder170";
                part1E.SpiritTypeReportingID = 3;
                part1E.UnfinishedSpiritsEndOfQuarterL17 = 0f;

                ProdReportParts2Through4 part2thru4 = new ProdReportParts2Through4();
                part2thru4.KindOfMaterial = "GrapeBrandy";
                part2thru4.MaterialKindReportingID = 94;
                part2thru4.NewCoop = false;
                part2thru4.UsedCoop = false;
                part2thru4.ProofGallons = 18f;
                part2thru4.SpiritTypeReportingID = 3;
                part2thru4.Tanks = false;


                ProdReportPart5 part5 = new ProdReportPart5();
                part5.KindofSpirits = "";
                part5.Proof = 0f;

                ProductionReportingObject actualProdReportObject = new ProductionReportingObject();


                actualProdReportObject = _dl.GetProductionReportData(start, end, _userId);

                /* STORAGE REPORT */
                StorageReport actualStorageReportObject = new StorageReport();

                StorageReportCategory storageReportBody = new StorageReportCategory();
                storageReportBody.CategoryName = "Wine";
                storageReportBody.r17_TransferredToProcessingAccount = 0f;
                storageReportBody.r18_TransferredToProductionAccount = 18f;
                storageReportBody.r19_TransferredToOtherBondedPremises = 0f;
                storageReportBody.r1_OnHandFirstOfMonth = 0f;
                storageReportBody.r20_Destroyed = 0f;
                storageReportBody.r22_OtherLosses = 0f;
                storageReportBody.r23_OnHandEndOfMonth = 0f;
                storageReportBody.r24_Lines7Through23 = 18f;
                storageReportBody.r2_DepositedInBulkStorage = 18f;
                storageReportBody.r4_ReturnedToBulkStorage = 0f;
                storageReportBody.r6_TotalLines1Through5 = 18f;
                storageReportBody.r7_TaxPaid = 0f;

                actualStorageReportObject = _dl.GetStorageReportData(start, end, _userId);

                /* PROCESING REPORT */
                ProcessingReportingObject actualProcessingReportObject = new ProcessingReportingObject();

                ProcessReportingPart1 processingReportP1 = new ProcessReportingPart1();
                processingReportP1.AmtBottledPackaged = 17.9f;
                processingReportP1.BulkIngredients = "spirit";
                processingReportP1.Destroyed = 0f;
                processingReportP1.Dumped4Processing = 0f;
                processingReportP1.Gains = 0f;
                processingReportP1.Losses = 0.1f;
                processingReportP1.OnHandEndofMonth = 0f;
                processingReportP1.OnHandFirstofMonth = 0f;
                processingReportP1.Recd4Process = 18f;
                processingReportP1.Transf2Prod4Redistil = 0f;
                processingReportP1.Used4Redistil = 0f;
                processingReportP1.WineMixedWithSpirit = 0f;

                ProcessReportingPart2 processingReportP2 = new ProcessReportingPart2();
                processingReportP2.AmtBottledPackaged = 17.9f;
                processingReportP2.Destroyed = 0f;
                processingReportP2.Dumped4Processing = 0f;
                processingReportP2.FinishedProduct = "bottled";
                processingReportP2.InventoryOverage = 0f;
                processingReportP2.InventoryShortage = 0f;
                processingReportP2.OnHandEndofMonth = 17.9f;
                processingReportP2.OnHandFirstofMonth = 0f;
                processingReportP2.Recd4Process = 0f;
                processingReportP2.RecordedLosses = 0f;
                processingReportP2.TaxWithdrawn = 0f;
                processingReportP2.Transf2Prod4Redistil = 0f;

                ProcessReportingPart4 processingReportP4 = new ProcessReportingPart4();
                processingReportP4.AlcoholNeutral = 0f;
                processingReportP4.BlendedLightWhiskey = 0f;
                processingReportP4.BlendedOtherWhiskey = 0f;
                processingReportP4.BlendedStraightWhiskey = 0f;
                processingReportP4.BlendedWhiskeyWithLight = 0f;
                processingReportP4.BlendedWhiskeyWithNeutral = 0f;
                processingReportP4.Brandy170Under = 22.39f;
                processingReportP4.BrandyOver170 = 0f;
                processingReportP4.Cocktail = 0f;
                processingReportP4.DomesticWhiskey160Under = 0f;
                processingReportP4.DomesticWhiskeyOver160 = 0f;
                processingReportP4.Gin = 0f;
                processingReportP4.ImportedWhiskeyCanadian = 0f;
                processingReportP4.ImportedWhiskeyIrish = 0f;
                processingReportP4.ImportedWhiskeyScotch = 0f;
                processingReportP4.Liqueur = 0f;
                processingReportP4.ProcessingReportTypeName = "BRANDY DISTILLED AT 170 AND UNDER";
                processingReportP4.ProcessingSpirits = "bottled";
                processingReportP4.ProcessingTypeID = 12;
                processingReportP4.RumDomestic = 0f;
                processingReportP4.RumOtherImported = 0f;
                processingReportP4.RumPuertoRican = 0f;
                processingReportP4.RumVirginIslands = 0f;
                processingReportP4.StateID = 5;
                processingReportP4.Tequila = 0f;
                processingReportP4.Vodka = 0f;

                actualProcessingReportObject = _dl.GetProcessingReportData(start, end, _userId);
                #endregion

                // Assert

                #region Production Report

                Tuple<int, float> accumulatedProofGL4SpiritTypeReportingId = new Tuple<int, float>(11, purchO.ProofGallon);

                List<Tuple<int, float>> accumulatedProofGL4SpiritTypeReportingIdL = new List<Tuple<int, float>>();

                accumulatedProofGL4SpiritTypeReportingIdL.Add(accumulatedProofGL4SpiritTypeReportingId);

                // verify Production report header
                Assert.AreEqual(reportHeaderE.DSP, actualProdReportObject.Header.DSP);
                Assert.AreEqual(reportHeaderE.EIN, actualProdReportObject.Header.EIN);
                Assert.AreEqual(reportHeaderE.PlantAddress, actualProdReportObject.Header.PlantAddress);
                Assert.AreEqual(reportHeaderE.ProprietorName, actualProdReportObject.Header.ProprietorName);
                Assert.AreEqual(reportHeaderE.ReportDate, actualProdReportObject.Header.ReportDate);

                // verify Production report Part 1
                Assert.AreEqual(part1E.ProccessingAcct, actualProdReportObject.Part1List[0].ProccessingAcct);
                Assert.AreEqual(part1E.ProducedTotal, actualProdReportObject.Part1List[0].ProducedTotal);
                Assert.AreEqual(part1E.Recd4RedistilL17, actualProdReportObject.Part1List[0].Recd4RedistilL17);

                var expectedWine = accumulatedProofGL4SpiritTypeReportingIdL.Find(l => l.Item1 == 11);
                var actualWine = actualProdReportObject.Part1List.Find(x => x.SpiritTypeReportingID == expectedWine.Item1);

                if (actualWine == null)
                {
                    Assert.AreNotEqual(null, expectedWine, "No records in actualAlcohol190Plus for SpiritTypeID");
                }
                else
                {
                    Assert.AreEqual(expectedWine.Item2, actualWine.Recd4RedistilaltionL15);
                    Assert.AreEqual("Wine", actualWine.SpiritCatName);
                }

                Assert.AreEqual(part1E.StorageAcct, actualProdReportObject.Part1List[0].StorageAcct);
                Assert.AreEqual(part1E.SpiritCatName, actualProdReportObject.Part1List[0].SpiritCatName);
                Assert.AreEqual(part1E.SpiritTypeReportingID, actualProdReportObject.Part1List[0].SpiritTypeReportingID);
                Assert.AreEqual(part1E.UnfinishedSpiritsEndOfQuarterL17, actualProdReportObject.Part1List[0].UnfinishedSpiritsEndOfQuarterL17);

                // verify Production report Part 2 trough 4
                Assert.AreEqual(part2thru4.KindOfMaterial, actualProdReportObject.Part2Through4List[0].KindOfMaterial);
                Assert.AreEqual(part2thru4.MaterialKindReportingID, actualProdReportObject.Part2Through4List[0].MaterialKindReportingID);
                Assert.AreEqual(part2thru4.NewCoop, actualProdReportObject.Part2Through4List[0].NewCoop);
                Assert.AreEqual(part2thru4.UsedCoop, actualProdReportObject.Part2Through4List[0].UsedCoop);
                Assert.AreEqual(part2thru4.ProofGallons, actualProdReportObject.Part2Through4List[0].ProofGallons);
                Assert.AreEqual(part2thru4.SpiritTypeReportingID, actualProdReportObject.Part2Through4List[0].SpiritTypeReportingID);
                Assert.AreEqual(part2thru4.Tanks, actualProdReportObject.Part2Through4List[0].Tanks);

                // verify part 5
                Assert.AreEqual("Wine", actualProdReportObject.part5List[0].KindofSpirits);
                Assert.AreEqual(18f, actualProdReportObject.part5List[0].Proof);

                // verify Production report Part 6 - no records should appear
                Assert.AreEqual(0, actualProdReportObject.ProdReportPart6List.Count);
                #endregion

                #region Storage Report
                // verify Storage report Header
                Assert.AreEqual(reportHeaderE.DSP, actualStorageReportObject.Header.DSP);
                Assert.AreEqual(reportHeaderE.EIN, actualStorageReportObject.Header.EIN);
                Assert.AreEqual(reportHeaderE.PlantAddress, actualStorageReportObject.Header.PlantAddress);
                Assert.AreEqual(reportHeaderE.ProprietorName, actualStorageReportObject.Header.ProprietorName);
                Assert.AreEqual(reportHeaderE.ReportDate, actualStorageReportObject.Header.ReportDate);

                // verify Storage body
                Assert.AreEqual(storageReportBody.CategoryName, actualStorageReportObject.ReportBody[0].CategoryName);
                Assert.AreEqual(storageReportBody.r17_TransferredToProcessingAccount, actualStorageReportObject.ReportBody[0].r17_TransferredToProcessingAccount);
                Assert.AreEqual(storageReportBody.r18_TransferredToProductionAccount, actualStorageReportObject.ReportBody[0].r18_TransferredToProductionAccount);
                Assert.AreEqual(storageReportBody.r19_TransferredToOtherBondedPremises, actualStorageReportObject.ReportBody[0].r19_TransferredToOtherBondedPremises);
                Assert.AreEqual(storageReportBody.r1_OnHandFirstOfMonth, actualStorageReportObject.ReportBody[0].r1_OnHandFirstOfMonth);
                Assert.AreEqual(storageReportBody.r20_Destroyed, actualStorageReportObject.ReportBody[0].r20_Destroyed);
                Assert.AreEqual(storageReportBody.r22_OtherLosses, actualStorageReportObject.ReportBody[0].r22_OtherLosses);
                Assert.AreEqual(storageReportBody.r23_OnHandEndOfMonth, actualStorageReportObject.ReportBody[0].r23_OnHandEndOfMonth);
                Assert.AreEqual(storageReportBody.r24_Lines7Through23, actualStorageReportObject.ReportBody[0].r24_Lines7Through23);
                Assert.AreEqual(storageReportBody.r2_DepositedInBulkStorage, actualStorageReportObject.ReportBody[0].r2_DepositedInBulkStorage);
                Assert.AreEqual(storageReportBody.r4_ReturnedToBulkStorage, actualStorageReportObject.ReportBody[0].r4_ReturnedToBulkStorage);
                Assert.AreEqual(storageReportBody.r6_TotalLines1Through5, actualStorageReportObject.ReportBody[0].r6_TotalLines1Through5);
                Assert.AreEqual(storageReportBody.r7_TaxPaid, actualStorageReportObject.ReportBody[0].r7_TaxPaid);

                #endregion

                #region Processing Report
                Assert.AreEqual(reportHeaderE.DSP, actualProcessingReportObject.Header.DSP);
                Assert.AreEqual(reportHeaderE.EIN, actualProcessingReportObject.Header.EIN);
                Assert.AreEqual(reportHeaderE.PlantAddress, actualProcessingReportObject.Header.PlantAddress);
                Assert.AreEqual(reportHeaderE.ProprietorName, actualProcessingReportObject.Header.ProprietorName);
                Assert.AreEqual(reportHeaderE.ReportDate, actualProcessingReportObject.Header.ReportDate);

                Assert.AreEqual(processingReportP1.AmtBottledPackaged, actualProcessingReportObject.Part1.AmtBottledPackaged);
                Assert.AreEqual(processingReportP1.BulkIngredients, actualProcessingReportObject.Part1.BulkIngredients);
                Assert.AreEqual(processingReportP1.Destroyed, actualProcessingReportObject.Part1.Destroyed);
                Assert.AreEqual(processingReportP1.Dumped4Processing, actualProcessingReportObject.Part1.Dumped4Processing);
                Assert.AreEqual(processingReportP1.Gains, actualProcessingReportObject.Part1.Gains);
                Assert.AreEqual(processingReportP1.Losses, actualProcessingReportObject.Part1.Losses);
                Assert.AreEqual(processingReportP1.OnHandEndofMonth, actualProcessingReportObject.Part1.OnHandEndofMonth);
                Assert.AreEqual(processingReportP1.OnHandFirstofMonth, actualProcessingReportObject.Part1.OnHandFirstofMonth);
                Assert.AreEqual(processingReportP1.Recd4Process, actualProcessingReportObject.Part1.Recd4Process);
                Assert.AreEqual(processingReportP1.Transf2Prod4Redistil, actualProcessingReportObject.Part1.Transf2Prod4Redistil);
                Assert.AreEqual(processingReportP1.Used4Redistil, actualProcessingReportObject.Part1.Used4Redistil);
                Assert.AreEqual(processingReportP1.WineMixedWithSpirit, actualProcessingReportObject.Part1.WineMixedWithSpirit);

                Assert.AreEqual(processingReportP2.AmtBottledPackaged, actualProcessingReportObject.Part2.AmtBottledPackaged);
                Assert.AreEqual(processingReportP2.Destroyed, actualProcessingReportObject.Part2.Destroyed);
                Assert.AreEqual(processingReportP2.Dumped4Processing, actualProcessingReportObject.Part2.Dumped4Processing);
                Assert.AreEqual(processingReportP2.FinishedProduct, actualProcessingReportObject.Part2.FinishedProduct);
                Assert.AreEqual(processingReportP2.InventoryOverage, actualProcessingReportObject.Part2.InventoryOverage);
                Assert.AreEqual(processingReportP2.InventoryShortage, actualProcessingReportObject.Part2.InventoryShortage);
                Assert.AreEqual(processingReportP2.OnHandEndofMonth, actualProcessingReportObject.Part2.OnHandEndofMonth);
                Assert.AreEqual(processingReportP2.OnHandFirstofMonth, actualProcessingReportObject.Part2.OnHandFirstofMonth);
                Assert.AreEqual(processingReportP2.Recd4Process, actualProcessingReportObject.Part2.Recd4Process);
                Assert.AreEqual(processingReportP2.RecordedLosses, actualProcessingReportObject.Part2.RecordedLosses);
                Assert.AreEqual(processingReportP2.TaxWithdrawn, actualProcessingReportObject.Part2.TaxWithdrawn);
                Assert.AreEqual(processingReportP2.Transf2Prod4Redistil, actualProcessingReportObject.Part2.Transf2Prod4Redistil);

                Assert.AreEqual(processingReportP4.AlcoholNeutral, actualProcessingReportObject.Part4List[0].AlcoholNeutral);
                Assert.AreEqual(processingReportP4.BlendedLightWhiskey, actualProcessingReportObject.Part4List[0].BlendedLightWhiskey);
                Assert.AreEqual(processingReportP4.BlendedOtherWhiskey, actualProcessingReportObject.Part4List[0].BlendedOtherWhiskey);
                Assert.AreEqual(processingReportP4.BlendedStraightWhiskey, actualProcessingReportObject.Part4List[0].BlendedStraightWhiskey);
                Assert.AreEqual(processingReportP4.BlendedWhiskeyWithLight, actualProcessingReportObject.Part4List[0].BlendedWhiskeyWithLight);
                Assert.AreEqual(processingReportP4.BlendedWhiskeyWithNeutral, actualProcessingReportObject.Part4List[0].BlendedWhiskeyWithNeutral);
                Assert.AreEqual(processingReportP4.Brandy170Under, actualProcessingReportObject.Part4List[1].Brandy170Under);
                Assert.AreEqual(processingReportP4.BrandyOver170, actualProcessingReportObject.Part4List[0].BrandyOver170);
                Assert.AreEqual(processingReportP4.Cocktail, actualProcessingReportObject.Part4List[0].Cocktail);
                Assert.AreEqual(processingReportP4.DomesticWhiskey160Under, actualProcessingReportObject.Part4List[0].DomesticWhiskey160Under);
                Assert.AreEqual(processingReportP4.DomesticWhiskeyOver160, actualProcessingReportObject.Part4List[0].DomesticWhiskeyOver160);
                Assert.AreEqual(processingReportP4.Gin, actualProcessingReportObject.Part4List[0].Gin);
                Assert.AreEqual(processingReportP4.ImportedWhiskeyCanadian, actualProcessingReportObject.Part4List[0].ImportedWhiskeyCanadian);
                Assert.AreEqual(processingReportP4.ImportedWhiskeyIrish, actualProcessingReportObject.Part4List[0].ImportedWhiskeyIrish);
                Assert.AreEqual(processingReportP4.ImportedWhiskeyScotch, actualProcessingReportObject.Part4List[0].ImportedWhiskeyScotch);
                Assert.AreEqual(processingReportP4.Liqueur, actualProcessingReportObject.Part4List[0].Liqueur);
                Assert.AreEqual(processingReportP4.ProcessingReportTypeName, actualProcessingReportObject.Part4List[0].ProcessingReportTypeName);
                Assert.AreEqual(processingReportP4.ProcessingSpirits, actualProcessingReportObject.Part4List[1].ProcessingSpirits);
                Assert.AreEqual(processingReportP4.ProcessingTypeID, actualProcessingReportObject.Part4List[0].ProcessingTypeID);
                Assert.AreEqual(processingReportP4.RumDomestic, actualProcessingReportObject.Part4List[0].RumDomestic);
                Assert.AreEqual(processingReportP4.RumDomestic, actualProcessingReportObject.Part4List[0].RumDomestic);
                Assert.AreEqual(processingReportP4.RumOtherImported, actualProcessingReportObject.Part4List[0].RumOtherImported);
                Assert.AreEqual(processingReportP4.RumPuertoRican, actualProcessingReportObject.Part4List[0].RumPuertoRican);
                Assert.AreEqual(processingReportP4.StateID, actualProcessingReportObject.Part4List[1].StateID);
                Assert.AreEqual(processingReportP4.Tequila, actualProcessingReportObject.Part4List[0].Tequila);
                Assert.AreEqual(processingReportP4.Vodka, actualProcessingReportObject.Part4List[0].Vodka);

                #endregion
            }
            finally
            {
                // Cleanup
                foreach (var i in tablesForCleanupTupleList)
                {
                    TestRecordCleanup(i.Item1, i.Item2);
                }
            }
        }

        /// <summary>
        /// Buy wine in November, distill once into Brandy Under 170, and blend with water in December.
        /// </summary>
        [TestMethod()]
        public void Buy_Wine_Distill_Brandy_Under_170_Blend_Test()
        {
            // Arrange
            List<Tuple<int/*recordId*/, Table/*table enum vaue*/>> tablesForCleanupTupleList = new List<Tuple<int, Table>>();
            int spiritId = 0;
            int vendorId = 0;
            int storageId = 0;
            int wineMaterialId = 0;
            int waterMaterialId = 0;
            int purchaseId = 0;
            int productionId = 0;

            try
            {
                #region Arrange
                //  dictionary setup
                SpiritObject spirit = new SpiritObject();
                spirit.SpiritName = "Brandy Under 170";
                spirit.ProcessingReportTypeID = 12;

                spiritId = _dl.CreateSpirit(_userId, spirit);
                tablesForCleanupTupleList.Add(Tuple.Create(spiritId, Table.Spirit));

                // setup Vendor object
                VendorObject vendor = new VendorObject();
                vendor.VendorName = "testVendor";

                vendorId = _dl.CreateVendor(_userId, vendor);
                tablesForCleanupTupleList.Add(Tuple.Create(vendorId, Table.Vendor));

                // setup Storage Object
                StorageObject storage = new StorageObject();
                storage.StorageName = "testStorage";
                storage.SerialNumber = "2H29NNS";

                storageId = _dl.CreateStorage(_userId, storage);
                tablesForCleanupTupleList.Add(Tuple.Create(storageId, Table.Storage));

                // setup Material Object
                // wine
                {
                    RawMaterialObject wineMaterial = new RawMaterialObject();
                    wineMaterial.RawMaterialName = "Wine For Brandy";
                    wineMaterial.MaterialCategoryID = 2;
                    wineMaterial.UnitType = "gal";
                    wineMaterial.UnitTypeId = 1;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Fermented = true;
                    wineMaterial.PurchaseMaterialTypes = materialBoolTypes;

                    wineMaterialId = _dl.CreateRawMaterial(_userId, wineMaterial);
                    tablesForCleanupTupleList.Add(Tuple.Create(wineMaterialId, Table.MaterialDict));
                }

                // water
                {
                    RawMaterialObject waterMaterial = new RawMaterialObject();
                    waterMaterial.RawMaterialName = "Water";
                    waterMaterial.UnitType = "gal";
                    waterMaterial.UnitTypeId = 1;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Additive = true;
                    waterMaterial.PurchaseMaterialTypes = materialBoolTypes;

                    waterMaterialId = _dl.CreateRawMaterial(_userId, waterMaterial);
                    tablesForCleanupTupleList.Add(Tuple.Create(waterMaterialId, Table.MaterialDict));
                }

                // create Purchase Record (minimal required fields)
                PurchaseObject purchO = new PurchaseObject();
                purchO.PurBatchName = "test7Purchase";
                purchO.PurchaseType = "Fermented";
                purchO.PurchaseDate = new DateTime(2017, 11, 1);
                purchO.Quantity = 100f; // 100 gallons
                purchO.VolumeByWeight = 0f;
                purchO.AlcoholContent = 98f;
                purchO.ProofGallon = 196f;
                purchO.RecordId = wineMaterialId;
                purchO.Price = 1000f;
                purchO.VendorId = vendorId;

                List<StorageObject> storageList = new List<StorageObject>();
                StorageObject storageObject = new StorageObject();
                storageObject.StorageId = storageId;
                storageList.Add(storageObject);
                purchO.Storage = storageList;

                purchO.SpiritTypeReportingID = 10;
                purchO.Gauged = true;

                purchaseId = _dl.CreatePurchase(purchO, _userId);
                tablesForCleanupTupleList.Add(Tuple.Create(purchaseId, Table.Purchase));

                // create 1st Production Distillation Record and don't mark it as Gauged
                ProductionObject prodO = new ProductionObject();
                prodO.BatchName = "test1stDistillRun";
                prodO.ProductionDate = new DateTime(2017, 11, 3);
                prodO.ProductionStart = new DateTime(2017, 11, 3);
                prodO.ProductionEnd = new DateTime(2017, 11, 3);
                prodO.SpiritCutId = 11; // mixed
                prodO.Gauged = true;
                prodO.ProductionType = "Distillation";
                prodO.Quantity = 100f; //50 gallons of alcohol
                prodO.VolumeByWeight = 0f;
                prodO.AlcoholContent = 98f; // 80%
                prodO.ProofGallon = 196f; // 80pfg
                prodO.Storage = storageList; // we are using the same storage id as we use for Purchase to keep things simple
                prodO.SpiritTypeReportingID = 3; // brandy under 170
                prodO.MaterialKindReportingID = 94; // grape brandy
                prodO.ProductionTypeId = 2;

                List<ObjInfo4Burndwn> usedMats = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat = new ObjInfo4Burndwn();
                uMat.ID = purchaseId;
                uMat.OldVal = 0f;
                uMat.NewVal = purchO.Quantity;
                uMat.DistillableOrigin = "pur";
                uMat.BurningDownMethod = "volume";

                usedMats.Add(uMat);

                prodO.UsedMats = usedMats;

                productionId = _dl.CreateProduction(prodO, _userId);
                tablesForCleanupTupleList.Add(Tuple.Create(productionId, Table.Production));

                // create Production Blending Record
                ProductionObject prodBlend = new ProductionObject();
                prodBlend.BatchName = "testProdBlend";
                prodBlend.ProductionDate = new DateTime(2017, 12, 5);
                prodBlend.ProductionStart = new DateTime(2017, 12, 5);
                prodBlend.ProductionEnd = new DateTime(2017, 12, 5);
                prodBlend.Gauged = true;
                prodBlend.ProductionType = "Blending";
                prodBlend.Quantity = 100f;
                prodBlend.VolumeByWeight = 0f;
                prodBlend.AlcoholContent = 49f;
                prodBlend.ProofGallon = 98f;
                prodBlend.Storage = storageList; // we are using the same storage id as we use for Purchase to keep things simple
                prodBlend.SpiritTypeReportingID = 3; // brandy under 170
                prodBlend.MaterialKindReportingID = 94; // grape brandy
                prodBlend.SpiritId = spiritId;
                prodBlend.ProductionTypeId = 3;

                List<ObjInfo4Burndwn> usedMats4Blend = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat4Blend = new ObjInfo4Burndwn();
                uMat4Blend.ID = productionId;
                uMat4Blend.OldVal = 0f;
                uMat4Blend.NewVal = prodO.Quantity;
                uMat4Blend.DistillableOrigin = "prod";
                uMat4Blend.BurningDownMethod = "volume";

                usedMats4Blend.Add(uMat4Blend);
                prodBlend.UsedMats = usedMats4Blend;

                List<BlendingAdditive> blendAdditives = new List<BlendingAdditive>();
                BlendingAdditive blendAd = new BlendingAdditive();
                blendAd.RawMaterialId = waterMaterialId;
                blendAd.RawMaterialQuantity = 50f;
                blendAd.RawMaterialName = "Water";
                blendAd.UnitOfMeasurement = "gal";

                blendAdditives.Add(blendAd);

                prodBlend.BlendingAdditives = blendAdditives;

                productionId = _dl.CreateProduction(prodBlend, _userId); // here productionId is overriden with a new productionId of the new Gauged record
                tablesForCleanupTupleList.Add(Tuple.Create(productionId, Table.Production));

                #endregion

                #region Act

                int octDays = DateTime.DaysInMonth(2017, 10);
                int novDays = DateTime.DaysInMonth(2017, 11);
                int decDays = DateTime.DaysInMonth(2017, 12);
                int janDays = DateTime.DaysInMonth(2018, 1);

                var octStart = new DateTime(2017, 10, 1);
                var octEnd = new DateTime(2017, 10, octDays);
                var novStart = new DateTime(2017, 11, 1);
                var novEnd = new DateTime(2017, 11, novDays);
                var decStart = new DateTime(2017, 12, 1);
                var decEnd = new DateTime(2017, 12, decDays);
                var janStart = new DateTime(2017, 1, 1);
                var janEnd = new DateTime(2017, 1, janDays);

                StorageReport octStorageReport = _dl.GetStorageReportData(octStart, octEnd, _userId);
                StorageReport novStorageReport = _dl.GetStorageReportData(novStart, novEnd, _userId);
                StorageReport decStorageReport = _dl.GetStorageReportData(decStart, decEnd, _userId);
                StorageReport janStorageReport = _dl.GetStorageReportData(janStart, janEnd, _userId);

                #endregion

                #region Assert

                // October 2016

                // No data records are generated
                Assert.IsFalse(octStorageReport.ReportBody.Any());

                // November 2016 Storage Report

                // Only "Brandy Distilled at 170 and under" AND "Other" records are generated
                Assert.AreEqual(2, novStorageReport.ReportBody.Count);
                Assert.IsTrue(novStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 3));
                Assert.IsTrue(novStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 10));
                Assert.IsFalse(novStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 1));
                Assert.IsFalse(novStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 2));
                Assert.IsFalse(novStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 4));
                Assert.IsFalse(novStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 5));
                Assert.IsFalse(novStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 6));
                Assert.IsFalse(novStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 7));
                Assert.IsFalse(novStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 8));
                Assert.IsFalse(novStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 9));
                Assert.IsFalse(novStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 11));

                // Brandy Distilled at 170 and under
                Assert.AreEqual(0f, novStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 3).Select(x => x.r1_OnHandFirstOfMonth).Single());
                Assert.AreEqual(196f, novStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 3).Select(x => x.r2_DepositedInBulkStorage).Single());
                Assert.AreEqual(0f, novStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 3).Select(x => x.r4_ReturnedToBulkStorage).Single());
                Assert.AreEqual(196f, novStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 3).Select(x => x.r6_TotalLines1Through5).Single());
                Assert.AreEqual(0f, novStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 3).Select(x => x.r7_TaxPaid).Single());
                Assert.AreEqual(0f, novStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 3).Select(x => x.r17_TransferredToProcessingAccount).Single());
                Assert.AreEqual(0f, novStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 3).Select(x => x.r18_TransferredToProductionAccount).Single());
                Assert.AreEqual(0f, novStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 3).Select(x => x.r19_TransferredToOtherBondedPremises).Single());
                Assert.AreEqual(0f, novStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 3).Select(x => x.r20_Destroyed).Single());
                Assert.AreEqual(0f, novStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 3).Select(x => x.r22_OtherLosses).Single());
                Assert.AreEqual(196f, novStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 3).Select(x => x.r23_OnHandEndOfMonth).Single());
                Assert.AreEqual(196f, novStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 3).Select(x => x.r24_Lines7Through23).Single());

                // OTHER
                Assert.AreEqual(0f, novStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 10).Select(x => x.r1_OnHandFirstOfMonth).Single());
                Assert.AreEqual(196f, novStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 10).Select(x => x.r2_DepositedInBulkStorage).Single());
                Assert.AreEqual(0f, novStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 10).Select(x => x.r4_ReturnedToBulkStorage).Single());
                Assert.AreEqual(196f, novStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 10).Select(x => x.r6_TotalLines1Through5).Single());
                Assert.AreEqual(0f, novStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 10).Select(x => x.r7_TaxPaid).Single());
                Assert.AreEqual(0f, novStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 10).Select(x => x.r17_TransferredToProcessingAccount).Single());
                Assert.AreEqual(196f, novStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 10).Select(x => x.r18_TransferredToProductionAccount).Single());
                Assert.AreEqual(0f, novStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 10).Select(x => x.r19_TransferredToOtherBondedPremises).Single());
                Assert.AreEqual(0f, novStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 10).Select(x => x.r20_Destroyed).Single());
                Assert.AreEqual(0f, novStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 10).Select(x => x.r22_OtherLosses).Single());
                Assert.AreEqual(0f, novStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 10).Select(x => x.r23_OnHandEndOfMonth).Single());
                Assert.AreEqual(196f, novStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 10).Select(x => x.r24_Lines7Through23).Single());

                // December 2016 Storage Report

                // Only "Brandy Distilled at 170 and under" records generated
                Assert.AreEqual(1, decStorageReport.ReportBody.Count);
                Assert.IsTrue(decStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 3));

                Assert.IsFalse(decStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 1));
                Assert.IsFalse(decStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 2));
                Assert.IsFalse(decStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 4));
                Assert.IsFalse(decStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 5));
                Assert.IsFalse(decStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 6));
                Assert.IsFalse(decStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 7));
                Assert.IsFalse(decStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 8));
                Assert.IsFalse(decStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 9));
                Assert.IsFalse(decStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 10));
                Assert.IsFalse(decStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 11));

                // Brandy Distilled at 170 and under
                Assert.AreEqual(196f, decStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 3).Select(x => x.r1_OnHandFirstOfMonth).Single());
                Assert.AreEqual(0f, decStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 3).Select(x => x.r2_DepositedInBulkStorage).Single());
                Assert.AreEqual(0f, decStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 3).Select(x => x.r4_ReturnedToBulkStorage).Single());
                Assert.AreEqual(196f, decStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 3).Select(x => x.r6_TotalLines1Through5).Single());
                Assert.AreEqual(0f, decStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 3).Select(x => x.r7_TaxPaid).Single());
                Assert.AreEqual(196f, decStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 3).Select(x => x.r17_TransferredToProcessingAccount).Single());
                Assert.AreEqual(0f, decStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 3).Select(x => x.r18_TransferredToProductionAccount).Single());
                Assert.AreEqual(0f, decStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 3).Select(x => x.r19_TransferredToOtherBondedPremises).Single());
                Assert.AreEqual(0f, decStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 3).Select(x => x.r20_Destroyed).Single());
                Assert.AreEqual(0f, decStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 3).Select(x => x.r22_OtherLosses).Single());
                Assert.AreEqual(0f, decStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 3).Select(x => x.r23_OnHandEndOfMonth).Single());
                Assert.AreEqual(196f, decStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 3).Select(x => x.r24_Lines7Through23).Single());

                // January 2017

                // No data records are generated
                Assert.IsFalse(janStorageReport.ReportBody.Any());

                #endregion
            }
            finally
            {
                // Cleanup
                foreach (var i in tablesForCleanupTupleList)
                {
                    TestRecordCleanup(i.Item1, i.Item2);
                }
            }
        }



        /// <summary>
        /// 1. Purchase Fermented Grape Pomace on 3/6/18 - 2000 lbs
        /// 2. Distill 1000 lbs to produce 55 gallons at 50% ABV on 4/6/18
        /// 3. Blend that with 10 gallons of water on 5/6/18 to produce 55 gallons of 43% ABV
        /// 4. Bottle blended batch on 5/7/18
        /// </summary>
        [TestMethod()]
        public void BuyPomace_DistillBrandy_Blend_Test()
        {
            // Arrange
            List<Tuple<int/*recordId*/, Table/*table enum vaue*/>> tablesForCleanupTupleList = new List<Tuple<int, Table>>();
            int spiritId = 0;
            int vendorId = 0;
            int storageId = 0;
            int rawMaterialPomaceId = 0;
            int rawMaterialWaterId = 0;
            int pomacePurchaseId = 0;
            int productionId = 0;

            try
            {
                #region Arrange Step 1
                //  dictionary setup
                SpiritObject spirit = new SpiritObject();
                spirit.SpiritName = "Brandy Under 170";
                spirit.ProcessingReportTypeID = 12;

                spiritId = _dl.CreateSpirit(_userId, spirit);
                tablesForCleanupTupleList.Add(Tuple.Create(spiritId, Table.Spirit));

                VendorObject vendor = new VendorObject();
                vendor.VendorName = "Vendor#1";

                vendorId = _dl.CreateVendor(_userId, vendor);
                tablesForCleanupTupleList.Add(Tuple.Create(vendorId, Table.Vendor));

                StorageObject storage = new StorageObject();
                storage.StorageName = "Storage#1";
                storage.SerialNumber = "1A2B3C4D";

                storageId = _dl.CreateStorage(_userId, storage);
                tablesForCleanupTupleList.Add(Tuple.Create(storageId, Table.Storage));

                // Fermented Pomace
                {
                    RawMaterialObject fermentedPomace = new RawMaterialObject();
                    fermentedPomace.RawMaterialName = "Grape Pomace";
                    fermentedPomace.MaterialCategoryID = 2;
                    fermentedPomace.UnitType = "lbs";
                    fermentedPomace.UnitTypeId = 2;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Fermented = true;
                    fermentedPomace.PurchaseMaterialTypes = materialBoolTypes;

                    rawMaterialPomaceId = _dl.CreateRawMaterial(_userId, fermentedPomace);
                    tablesForCleanupTupleList.Add(Tuple.Create(rawMaterialPomaceId, Table.MaterialDict));
                }

                // Water
                {
                    RawMaterialObject water = new RawMaterialObject();
                    water.RawMaterialName = "Water";
                    water.UnitType = "gal";
                    water.UnitTypeId = 1;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Additive = true;
                    water.PurchaseMaterialTypes = materialBoolTypes;

                    rawMaterialWaterId = _dl.CreateRawMaterial(_userId, water);
                    tablesForCleanupTupleList.Add(Tuple.Create(rawMaterialWaterId, Table.MaterialDict));
                }

                // 1. Purchase Fermented Grape Pomace on 3/6/18 - 2000 lbs
                PurchaseObject pomacePurchase = new PurchaseObject();
                pomacePurchase.PurBatchName = "Fermented Pomace";
                pomacePurchase.PurchaseType = "Fermented";
                pomacePurchase.PurchaseDate = new DateTime(2018, 3, 6);
                pomacePurchase.Quantity = 0f;
                pomacePurchase.VolumeByWeight = 2000f; // 2000 lbs
                pomacePurchase.AlcoholContent = 0f;
                pomacePurchase.ProofGallon = 0f;
                pomacePurchase.RecordId = rawMaterialPomaceId;
                pomacePurchase.Price = 1000f;
                pomacePurchase.VendorId = vendorId;

                List<StorageObject> storageList = new List<StorageObject>();
                StorageObject storageObject = new StorageObject();
                storageObject.StorageId = storageId;
                storageList.Add(storageObject);
                pomacePurchase.Storage = storageList;

                pomacePurchase.SpiritTypeReportingID = 10;
                pomacePurchase.Gauged = true;

                pomacePurchaseId = _dl.CreatePurchase(pomacePurchase, _userId);
                tablesForCleanupTupleList.Add(Tuple.Create(pomacePurchaseId, Table.Purchase));

                // 2. Distill 1000 lbs to produce 55 gallons at 50% ABV on 4/6/18
                ProductionObject brandyProduction = new ProductionObject();
                brandyProduction.BatchName = "Brandy Distillation";
                brandyProduction.ProductionDate = new DateTime(2018, 4, 6);
                brandyProduction.ProductionStart = new DateTime(2018, 4, 6);
                brandyProduction.ProductionEnd = new DateTime(2018, 4, 6);
                brandyProduction.SpiritCutId = 11; // mixed
                brandyProduction.Gauged = true;
                brandyProduction.ProductionType = "Distillation";
                brandyProduction.Quantity = 55f; // 55 gallons of alcohol
                brandyProduction.VolumeByWeight = 0f;
                brandyProduction.AlcoholContent = 50f; // 50%
                brandyProduction.ProofGallon = 55f; // 55pfg
                brandyProduction.Storage = storageList; // we are using the same storage id as we use for Purchase to keep things simple
                brandyProduction.SpiritTypeReportingID = 3; // brandy under 170
                brandyProduction.MaterialKindReportingID = 94; // grape brandy
                brandyProduction.ProductionTypeId = 2;

                List<ObjInfo4Burndwn> usedMats = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat = new ObjInfo4Burndwn();
                uMat.ID = pomacePurchaseId;
                uMat.OldVal = pomacePurchase.Quantity / 2;
                uMat.NewVal = pomacePurchase.Quantity / 2;
                uMat.DistillableOrigin = "pur";
                uMat.BurningDownMethod = "weight";

                usedMats.Add(uMat);

                brandyProduction.UsedMats = usedMats;

                productionId = _dl.CreateProduction(brandyProduction, _userId);
                tablesForCleanupTupleList.Add(Tuple.Create(productionId, Table.Production));

                // 3. Blend that with 10 gallons of water on 5/6/18 to produce 55 gallons of 43% ABV
                ProductionObject prodBlend = new ProductionObject();
                prodBlend.BatchName = "Blended Brandy";
                prodBlend.ProductionDate = new DateTime(2018, 5, 6);
                prodBlend.ProductionStart = new DateTime(2018, 5, 6);
                prodBlend.ProductionEnd = new DateTime(2018, 5, 6);
                prodBlend.Gauged = true;
                prodBlend.ProductionType = "Blending";
                prodBlend.Quantity = 55f; // 55 gallons of alcohol
                prodBlend.VolumeByWeight = 0f;
                prodBlend.AlcoholContent = 43f; // 43%
                prodBlend.ProofGallon = 47.3f; // 47.3 pfg
                prodBlend.Storage = storageList; // we are using the same storage id as we use for Purchase to keep things simple
                prodBlend.SpiritTypeReportingID = 3; // brandy under 170
                prodBlend.MaterialKindReportingID = 94; // grape brandy
                prodBlend.SpiritId = spiritId;
                prodBlend.ProductionTypeId = 3;

                List<ObjInfo4Burndwn> usedMats4Blend = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat4Blend = new ObjInfo4Burndwn();
                uMat4Blend.ID = productionId;
                uMat4Blend.OldVal = 0f;
                uMat4Blend.NewVal = brandyProduction.Quantity;
                uMat4Blend.DistillableOrigin = "prod";
                uMat4Blend.BurningDownMethod = "volume";

                usedMats4Blend.Add(uMat4Blend);
                prodBlend.UsedMats = usedMats4Blend;

                List<BlendingAdditive> blendAdditives = new List<BlendingAdditive>();
                BlendingAdditive blendAd = new BlendingAdditive();
                blendAd.RawMaterialId = rawMaterialWaterId;
                blendAd.RawMaterialQuantity = 10f;
                blendAd.RawMaterialName = "Water";
                blendAd.UnitOfMeasurement = "gal";

                blendAdditives.Add(blendAd);

                prodBlend.BlendingAdditives = blendAdditives;

                productionId = _dl.CreateProduction(prodBlend, _userId); // here productionId is overriden with a new productionId of the new Gauged record
                tablesForCleanupTupleList.Add(Tuple.Create(productionId, Table.Production));

                #endregion

                #region Act Step 1

                int marDays = DateTime.DaysInMonth(2018, 3);
                int aprDays = DateTime.DaysInMonth(2018, 4);
                int mayDays = DateTime.DaysInMonth(2018, 5);
                int juneDays = DateTime.DaysInMonth(2018, 6);

                var marStart = new DateTime(2018, 3, 1);
                var marEnd = new DateTime(2018, 3, marDays);

                var aprStart = new DateTime(2018, 4, 1);
                var aprEnd = new DateTime(2018, 4, aprDays);

                var mayStart = new DateTime(2018, 5, 1);
                var mayEnd = new DateTime(2018, 5, mayDays);

                var juneStart = new DateTime(2018, 6, 1);
                var juneEnd = new DateTime(2018, 6, juneDays);

                var marProcessingReport = _dl.GetProcessingReportData(marStart, marEnd, _userId);
                var aprProcessingReport = _dl.GetProcessingReportData(aprStart, aprEnd, _userId);
                var mayProcessingReport = _dl.GetProcessingReportData(mayStart, mayEnd, _userId);
                var juneProcessingReport = _dl.GetProcessingReportData(juneStart, juneEnd, _userId);

                #endregion

                #region Assert Step 1

                // March 2018

                // Processing Report Part 1
                Assert.AreEqual(0f, marProcessingReport.Part1.AmtBottledPackaged);
                Assert.AreEqual("spirit", marProcessingReport.Part1.BulkIngredients);
                Assert.AreEqual(0f, marProcessingReport.Part1.Destroyed);
                Assert.AreEqual(0f, marProcessingReport.Part1.Dumped4Processing);
                Assert.AreEqual(0f, marProcessingReport.Part1.Gains);
                Assert.AreEqual(0f, marProcessingReport.Part1.Losses);
                Assert.AreEqual(0f, marProcessingReport.Part1.OnHandEndofMonth);
                Assert.AreEqual(0f, marProcessingReport.Part1.OnHandFirstofMonth);
                Assert.AreEqual(0f, marProcessingReport.Part1.Recd4Process);
                Assert.AreEqual(0f, marProcessingReport.Part1.Transf2Prod4Redistil);
                Assert.AreEqual(0f, marProcessingReport.Part1.Used4Redistil);
                Assert.AreEqual(0f, marProcessingReport.Part1.WineMixedWithSpirit);

                // Processing Report Part 2
                Assert.AreEqual(0f, marProcessingReport.Part2.AmtBottledPackaged);
                Assert.AreEqual(0f, marProcessingReport.Part2.Destroyed);
                Assert.AreEqual(0f, marProcessingReport.Part2.Dumped4Processing);
                Assert.AreEqual("bottled", marProcessingReport.Part2.FinishedProduct);
                Assert.AreEqual(0f, marProcessingReport.Part2.InventoryOverage);
                Assert.AreEqual(0f, marProcessingReport.Part2.InventoryShortage);
                Assert.AreEqual(0f, marProcessingReport.Part2.OnHandEndofMonth);
                Assert.AreEqual(0f, marProcessingReport.Part2.OnHandFirstofMonth);
                Assert.AreEqual(0f, marProcessingReport.Part2.Recd4Process);
                Assert.AreEqual(0f, marProcessingReport.Part2.RecordedLosses);
                Assert.AreEqual(0f, marProcessingReport.Part2.TaxWithdrawn);
                Assert.AreEqual(0f, marProcessingReport.Part2.Transf2Prod4Redistil);

                // Processing Report Part 4
                Assert.AreEqual(0, marProcessingReport.Part4List.Count);

                // April 2018

                // Processing Report Part 1
                Assert.AreEqual(0f, aprProcessingReport.Part1.AmtBottledPackaged);
                Assert.AreEqual("spirit", aprProcessingReport.Part1.BulkIngredients);
                Assert.AreEqual(0f, aprProcessingReport.Part1.Destroyed);
                Assert.AreEqual(0f, aprProcessingReport.Part1.Dumped4Processing);
                Assert.AreEqual(0f, aprProcessingReport.Part1.Gains);
                Assert.AreEqual(0f, aprProcessingReport.Part1.Losses);
                Assert.AreEqual(0f, aprProcessingReport.Part1.OnHandEndofMonth);
                Assert.AreEqual(0f, aprProcessingReport.Part1.OnHandFirstofMonth);
                Assert.AreEqual(0f, aprProcessingReport.Part1.Recd4Process);
                Assert.AreEqual(0f, aprProcessingReport.Part1.Transf2Prod4Redistil);
                Assert.AreEqual(0f, aprProcessingReport.Part1.Used4Redistil);
                Assert.AreEqual(0f, aprProcessingReport.Part1.WineMixedWithSpirit);

                // Processing Report Part 2
                Assert.AreEqual(0f, aprProcessingReport.Part2.AmtBottledPackaged);
                Assert.AreEqual(0f, aprProcessingReport.Part2.Destroyed);
                Assert.AreEqual(0f, aprProcessingReport.Part2.Dumped4Processing);
                Assert.AreEqual("bottled", aprProcessingReport.Part2.FinishedProduct);
                Assert.AreEqual(0f, aprProcessingReport.Part2.InventoryOverage);
                Assert.AreEqual(0f, aprProcessingReport.Part2.InventoryShortage);
                Assert.AreEqual(0f, aprProcessingReport.Part2.OnHandEndofMonth);
                Assert.AreEqual(0f, aprProcessingReport.Part2.OnHandFirstofMonth);
                Assert.AreEqual(0f, aprProcessingReport.Part2.Recd4Process);
                Assert.AreEqual(0f, aprProcessingReport.Part2.RecordedLosses);
                Assert.AreEqual(0f, aprProcessingReport.Part2.TaxWithdrawn);
                Assert.AreEqual(0f, aprProcessingReport.Part2.Transf2Prod4Redistil);

                // Processing Report Part 4
                Assert.AreEqual(0, aprProcessingReport.Part4List.Count);

                // May 2018

                // Processing Report Part 1
                Assert.AreEqual(0f, mayProcessingReport.Part1.AmtBottledPackaged);
                Assert.AreEqual("spirit", mayProcessingReport.Part1.BulkIngredients);
                Assert.AreEqual(0f, mayProcessingReport.Part1.Destroyed);
                Assert.AreEqual(0f, mayProcessingReport.Part1.Dumped4Processing);
                Assert.AreEqual(0f, mayProcessingReport.Part1.Gains);
                Assert.AreEqual(0f, mayProcessingReport.Part1.Losses);
                Assert.AreEqual(47.3f, mayProcessingReport.Part1.OnHandEndofMonth);
                Assert.AreEqual(0f, mayProcessingReport.Part1.OnHandFirstofMonth);
                Assert.AreEqual(47.3f, mayProcessingReport.Part1.Recd4Process);
                Assert.AreEqual(0f, mayProcessingReport.Part1.Transf2Prod4Redistil);
                Assert.AreEqual(0f, mayProcessingReport.Part1.Used4Redistil);
                Assert.AreEqual(0f, mayProcessingReport.Part1.WineMixedWithSpirit);

                // Processing Report Part 2
                Assert.AreEqual(0f, mayProcessingReport.Part2.AmtBottledPackaged);
                Assert.AreEqual(0f, mayProcessingReport.Part2.Destroyed);
                Assert.AreEqual(0f, mayProcessingReport.Part2.Dumped4Processing);
                Assert.AreEqual("bottled", mayProcessingReport.Part2.FinishedProduct);
                Assert.AreEqual(0f, mayProcessingReport.Part2.InventoryOverage);
                Assert.AreEqual(0f, mayProcessingReport.Part2.InventoryShortage);
                Assert.AreEqual(0f, mayProcessingReport.Part2.OnHandEndofMonth);
                Assert.AreEqual(0f, mayProcessingReport.Part2.OnHandFirstofMonth);
                Assert.AreEqual(0f, mayProcessingReport.Part2.Recd4Process);
                Assert.AreEqual(0f, mayProcessingReport.Part2.RecordedLosses);
                Assert.AreEqual(0f, mayProcessingReport.Part2.TaxWithdrawn);
                Assert.AreEqual(0f, mayProcessingReport.Part2.Transf2Prod4Redistil);

                // Processing Report Part 4
                Assert.AreEqual(1, mayProcessingReport.Part4List.Count);
                Assert.IsTrue(mayProcessingReport.Part4List.Exists(x => x.ProcessingSpirits == "bulkSpiritDumped"));
                Assert.AreEqual(47.3f, mayProcessingReport.Part4List.Where(x => x.ProcessingSpirits == "bulkSpiritDumped").Select(x => x.Brandy170Under).Single());
                Assert.AreEqual("BRANDY DISTILLED AT 170 AND UNDER", mayProcessingReport.Part4List.Where(x => x.ProcessingSpirits == "bulkSpiritDumped").Select(x => x.ProcessingReportTypeName).Single());

                // June 2018

                // Processing Report Part 1
                Assert.AreEqual(0f, juneProcessingReport.Part1.AmtBottledPackaged);
                Assert.AreEqual("spirit", juneProcessingReport.Part1.BulkIngredients);
                Assert.AreEqual(0f, juneProcessingReport.Part1.Destroyed);
                Assert.AreEqual(0f, juneProcessingReport.Part1.Dumped4Processing);
                Assert.AreEqual(0f, juneProcessingReport.Part1.Gains);
                Assert.AreEqual(0f, juneProcessingReport.Part1.Losses);
                Assert.AreEqual(47.3f, juneProcessingReport.Part1.OnHandEndofMonth);
                Assert.AreEqual(47.3f, juneProcessingReport.Part1.OnHandFirstofMonth);
                Assert.AreEqual(0f, juneProcessingReport.Part1.Recd4Process);
                Assert.AreEqual(0f, juneProcessingReport.Part1.Transf2Prod4Redistil);
                Assert.AreEqual(0f, juneProcessingReport.Part1.Used4Redistil);
                Assert.AreEqual(0f, juneProcessingReport.Part1.WineMixedWithSpirit);

                // Processing Report Part 2
                Assert.AreEqual(0f, juneProcessingReport.Part2.AmtBottledPackaged);
                Assert.AreEqual(0f, juneProcessingReport.Part2.Destroyed);
                Assert.AreEqual(0f, juneProcessingReport.Part2.Dumped4Processing);
                Assert.AreEqual("bottled", juneProcessingReport.Part2.FinishedProduct);
                Assert.AreEqual(0f, juneProcessingReport.Part2.InventoryOverage);
                Assert.AreEqual(0f, juneProcessingReport.Part2.InventoryShortage);
                Assert.AreEqual(0f, juneProcessingReport.Part2.OnHandEndofMonth);
                Assert.AreEqual(0f, juneProcessingReport.Part2.OnHandFirstofMonth);
                Assert.AreEqual(0f, juneProcessingReport.Part2.Recd4Process);
                Assert.AreEqual(0f, juneProcessingReport.Part2.RecordedLosses);
                Assert.AreEqual(0f, juneProcessingReport.Part2.TaxWithdrawn);
                Assert.AreEqual(0f, juneProcessingReport.Part2.Transf2Prod4Redistil);

                // Processing Report Part 4
                Assert.AreEqual(0, juneProcessingReport.Part4List.Count);

                #endregion

                #region Arrange Step 2

                /// 4. Bottle blended batch on 5/7/18
                ProductionObject prodBottl = new ProductionObject();
                prodBottl.BatchName = "Brandy Under 170 Bottling";
                prodBottl.ProductionDate = new DateTime(2018, 5, 7);
                prodBottl.ProductionStart = new DateTime(2018, 5, 7);
                prodBottl.ProductionEnd = new DateTime(2018, 5, 7);
                prodBottl.Gauged = true;
                prodBottl.ProductionType = "Bottling";
                prodBottl.Quantity = 53.49f; // 53.49 gallons of alcohol
                prodBottl.VolumeByWeight = 0f;
                prodBottl.AlcoholContent = 43f; // 43%
                prodBottl.ProofGallon = 46f; // 46 pfg
                prodBottl.Storage = storageList; // we are using the same storage id as we use for Purchase to keep things simple
                prodBottl.SpiritTypeReportingID = 3; // Brandy
                prodBottl.SpiritId = spiritId;
                prodBottl.ProductionTypeId = 4;

                List<ObjInfo4Burndwn> usedMats4Bottl = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat4Bottl = new ObjInfo4Burndwn();
                uMat4Bottl.ID = productionId;
                uMat4Bottl.OldVal = 0f;
                uMat4Bottl.NewVal = prodBlend.Quantity;
                uMat4Bottl.DistillableOrigin = "prod";
                uMat4Bottl.BurningDownMethod = "volume";

                usedMats4Bottl.Add(uMat4Bottl);
                prodBottl.UsedMats = usedMats4Bottl;

                BottlingObject bottlingObj = new BottlingObject();
                bottlingObj.CaseCapacity = 6;
                bottlingObj.CaseQuantity = 45f; // Should be int instead of float?
                bottlingObj.BottleCapacity = 750f;
                bottlingObj.BottleQuantity = 270;

                prodBottl.BottlingInfo = bottlingObj;

                prodBottl.GainLoss = 1.3f; // Add comment on what this value is in definition

                prodBottl.FillTestList = null;

                productionId = _dl.CreateProduction(prodBottl, _userId); // here productionId is overriden with a new productionId of the new Gauged record
                tablesForCleanupTupleList.Add(Tuple.Create(productionId, Table.Production));

                #endregion

                #region Act Step 2

                var marAfterBottlingProcessingReport = _dl.GetProcessingReportData(marStart, marEnd, _userId);
                var aprAfterBottlingProcessingReport = _dl.GetProcessingReportData(aprStart, aprEnd, _userId);
                var mayAfterBottlingProcessingReport = _dl.GetProcessingReportData(mayStart, mayEnd, _userId);
                var juneAfterBottlingProcessingReport = _dl.GetProcessingReportData(juneStart, juneEnd, _userId);

                #endregion

                #region Assert Step 2

                // March 2018

                // Processing Report Part 1
                Assert.AreEqual(0f, marAfterBottlingProcessingReport.Part1.AmtBottledPackaged);
                Assert.AreEqual("spirit", marAfterBottlingProcessingReport.Part1.BulkIngredients);
                Assert.AreEqual(0f, marAfterBottlingProcessingReport.Part1.Destroyed);
                Assert.AreEqual(0f, marAfterBottlingProcessingReport.Part1.Dumped4Processing);
                Assert.AreEqual(0f, marAfterBottlingProcessingReport.Part1.Gains);
                Assert.AreEqual(0f, marAfterBottlingProcessingReport.Part1.Losses);
                Assert.AreEqual(0f, marAfterBottlingProcessingReport.Part1.OnHandEndofMonth);
                Assert.AreEqual(0f, marAfterBottlingProcessingReport.Part1.OnHandFirstofMonth);
                Assert.AreEqual(0f, marAfterBottlingProcessingReport.Part1.Recd4Process);
                Assert.AreEqual(0f, marAfterBottlingProcessingReport.Part1.Transf2Prod4Redistil);
                Assert.AreEqual(0f, marAfterBottlingProcessingReport.Part1.Used4Redistil);
                Assert.AreEqual(0f, marAfterBottlingProcessingReport.Part1.WineMixedWithSpirit);

                // Processing Report Part 2
                Assert.AreEqual(0f, marAfterBottlingProcessingReport.Part2.AmtBottledPackaged);
                Assert.AreEqual(0f, marAfterBottlingProcessingReport.Part2.Destroyed);
                Assert.AreEqual(0f, marAfterBottlingProcessingReport.Part2.Dumped4Processing);
                Assert.AreEqual("bottled", marAfterBottlingProcessingReport.Part2.FinishedProduct);
                Assert.AreEqual(0f, marAfterBottlingProcessingReport.Part2.InventoryOverage);
                Assert.AreEqual(0f, marAfterBottlingProcessingReport.Part2.InventoryShortage);
                Assert.AreEqual(0f, marAfterBottlingProcessingReport.Part2.OnHandEndofMonth);
                Assert.AreEqual(0f, marAfterBottlingProcessingReport.Part2.OnHandFirstofMonth);
                Assert.AreEqual(0f, marAfterBottlingProcessingReport.Part2.Recd4Process);
                Assert.AreEqual(0f, marAfterBottlingProcessingReport.Part2.RecordedLosses);
                Assert.AreEqual(0f, marAfterBottlingProcessingReport.Part2.TaxWithdrawn);
                Assert.AreEqual(0f, marAfterBottlingProcessingReport.Part2.Transf2Prod4Redistil);

                // Processing Report Part 4
                Assert.AreEqual(0, marAfterBottlingProcessingReport.Part4List.Count);

                // April 2018

                // Processing Report Part 1
                Assert.AreEqual(0f, aprAfterBottlingProcessingReport.Part1.AmtBottledPackaged);
                Assert.AreEqual("spirit", aprAfterBottlingProcessingReport.Part1.BulkIngredients);
                Assert.AreEqual(0f, aprAfterBottlingProcessingReport.Part1.Destroyed);
                Assert.AreEqual(0f, aprAfterBottlingProcessingReport.Part1.Dumped4Processing);
                Assert.AreEqual(0f, aprAfterBottlingProcessingReport.Part1.Gains);
                Assert.AreEqual(0f, aprAfterBottlingProcessingReport.Part1.Losses);
                Assert.AreEqual(0f, aprAfterBottlingProcessingReport.Part1.OnHandEndofMonth);
                Assert.AreEqual(0f, aprAfterBottlingProcessingReport.Part1.OnHandFirstofMonth);
                Assert.AreEqual(0f, aprAfterBottlingProcessingReport.Part1.Recd4Process);
                Assert.AreEqual(0f, aprAfterBottlingProcessingReport.Part1.Transf2Prod4Redistil);
                Assert.AreEqual(0f, aprAfterBottlingProcessingReport.Part1.Used4Redistil);
                Assert.AreEqual(0f, aprAfterBottlingProcessingReport.Part1.WineMixedWithSpirit);

                // Processing Report Part 2
                Assert.AreEqual(0f, aprAfterBottlingProcessingReport.Part2.AmtBottledPackaged);
                Assert.AreEqual(0f, aprAfterBottlingProcessingReport.Part2.Destroyed);
                Assert.AreEqual(0f, aprAfterBottlingProcessingReport.Part2.Dumped4Processing);
                Assert.AreEqual("bottled", aprAfterBottlingProcessingReport.Part2.FinishedProduct);
                Assert.AreEqual(0f, aprAfterBottlingProcessingReport.Part2.InventoryOverage);
                Assert.AreEqual(0f, aprAfterBottlingProcessingReport.Part2.InventoryShortage);
                Assert.AreEqual(0f, aprAfterBottlingProcessingReport.Part2.OnHandEndofMonth);
                Assert.AreEqual(0f, aprAfterBottlingProcessingReport.Part2.OnHandFirstofMonth);
                Assert.AreEqual(0f, aprAfterBottlingProcessingReport.Part2.Recd4Process);
                Assert.AreEqual(0f, aprAfterBottlingProcessingReport.Part2.RecordedLosses);
                Assert.AreEqual(0f, aprAfterBottlingProcessingReport.Part2.TaxWithdrawn);
                Assert.AreEqual(0f, aprAfterBottlingProcessingReport.Part2.Transf2Prod4Redistil);

                // Processing Report Part 4
                Assert.AreEqual(0, aprAfterBottlingProcessingReport.Part4List.Count);

                // May 2018

                // Processing Report Part 1
                Assert.AreEqual(46f, mayAfterBottlingProcessingReport.Part1.AmtBottledPackaged);
                Assert.AreEqual("spirit", mayAfterBottlingProcessingReport.Part1.BulkIngredients);
                Assert.AreEqual(0f, mayAfterBottlingProcessingReport.Part1.Destroyed);
                Assert.AreEqual(0f, mayAfterBottlingProcessingReport.Part1.Dumped4Processing);
                Assert.AreEqual(0f, mayAfterBottlingProcessingReport.Part1.Gains);
                Assert.AreEqual(1.3f, mayAfterBottlingProcessingReport.Part1.Losses);
                Assert.AreEqual(0f, mayAfterBottlingProcessingReport.Part1.OnHandEndofMonth);
                Assert.AreEqual(0f, mayAfterBottlingProcessingReport.Part1.OnHandFirstofMonth);
                Assert.AreEqual(47.3f, mayAfterBottlingProcessingReport.Part1.Recd4Process);
                Assert.AreEqual(0f, mayAfterBottlingProcessingReport.Part1.Transf2Prod4Redistil);
                Assert.AreEqual(0f, mayAfterBottlingProcessingReport.Part1.Used4Redistil);
                Assert.AreEqual(0f, mayAfterBottlingProcessingReport.Part1.WineMixedWithSpirit);

                // Processing Report Part 2
                Assert.AreEqual(46f, mayAfterBottlingProcessingReport.Part2.AmtBottledPackaged);
                Assert.AreEqual(0f, mayAfterBottlingProcessingReport.Part2.Destroyed);
                Assert.AreEqual(0f, mayAfterBottlingProcessingReport.Part2.Dumped4Processing);
                Assert.AreEqual("bottled", mayAfterBottlingProcessingReport.Part2.FinishedProduct);
                Assert.AreEqual(0f, mayAfterBottlingProcessingReport.Part2.InventoryOverage);
                Assert.AreEqual(0f, mayAfterBottlingProcessingReport.Part2.InventoryShortage);
                Assert.AreEqual(46f, mayAfterBottlingProcessingReport.Part2.OnHandEndofMonth);
                Assert.AreEqual(0f, mayAfterBottlingProcessingReport.Part2.OnHandFirstofMonth);
                Assert.AreEqual(0f, mayAfterBottlingProcessingReport.Part2.Recd4Process);
                Assert.AreEqual(0f, mayAfterBottlingProcessingReport.Part2.RecordedLosses);
                Assert.AreEqual(0f, mayAfterBottlingProcessingReport.Part2.TaxWithdrawn);
                Assert.AreEqual(0f, mayAfterBottlingProcessingReport.Part2.Transf2Prod4Redistil);

                // Processing Report Part 4
                Assert.AreEqual(2, mayAfterBottlingProcessingReport.Part4List.Count);
                Assert.IsTrue(mayAfterBottlingProcessingReport.Part4List.Exists(x => x.ProcessingSpirits == "bulkSpiritDumped"));
                Assert.IsTrue(mayAfterBottlingProcessingReport.Part4List.Exists(x => x.ProcessingSpirits == "bottled"));
                Assert.AreEqual("BRANDY DISTILLED AT 170 AND UNDER", mayAfterBottlingProcessingReport.Part4List.Where(x => x.ProcessingSpirits == "bulkSpiritDumped").Select(x => x.ProcessingReportTypeName).Single());
                Assert.AreEqual("BRANDY DISTILLED AT 170 AND UNDER", mayAfterBottlingProcessingReport.Part4List.Where(x => x.ProcessingSpirits == "bottled").Select(x => x.ProcessingReportTypeName).Single());
                Assert.AreEqual(47.3f, mayAfterBottlingProcessingReport.Part4List.Where(x => x.ProcessingSpirits == "bulkSpiritDumped").Select(x => x.Brandy170Under).Single());
                Assert.AreEqual(53.49f, mayAfterBottlingProcessingReport.Part4List.Where(x => x.ProcessingSpirits == "bottled").Select(x => x.Brandy170Under).Single());

                // June 2018

                // Processing Report Part 1
                Assert.AreEqual(0f, juneAfterBottlingProcessingReport.Part1.AmtBottledPackaged);
                Assert.AreEqual("spirit", juneAfterBottlingProcessingReport.Part1.BulkIngredients);
                Assert.AreEqual(0f, juneAfterBottlingProcessingReport.Part1.Destroyed);
                Assert.AreEqual(0f, juneAfterBottlingProcessingReport.Part1.Dumped4Processing);
                Assert.AreEqual(0f, juneAfterBottlingProcessingReport.Part1.Gains);
                Assert.AreEqual(0f, juneAfterBottlingProcessingReport.Part1.Losses);
                Assert.AreEqual(0f, juneAfterBottlingProcessingReport.Part1.OnHandEndofMonth);
                Assert.AreEqual(0f, juneAfterBottlingProcessingReport.Part1.OnHandFirstofMonth);
                Assert.AreEqual(0f, juneAfterBottlingProcessingReport.Part1.Recd4Process);
                Assert.AreEqual(0f, juneAfterBottlingProcessingReport.Part1.Transf2Prod4Redistil);
                Assert.AreEqual(0f, juneAfterBottlingProcessingReport.Part1.Used4Redistil);
                Assert.AreEqual(0f, juneAfterBottlingProcessingReport.Part1.WineMixedWithSpirit);

                // Processing Report Part 2
                Assert.AreEqual(0f, juneAfterBottlingProcessingReport.Part2.AmtBottledPackaged);
                Assert.AreEqual(0f, juneAfterBottlingProcessingReport.Part2.Destroyed);
                Assert.AreEqual(0f, juneAfterBottlingProcessingReport.Part2.Dumped4Processing);
                Assert.AreEqual("bottled", juneAfterBottlingProcessingReport.Part2.FinishedProduct);
                Assert.AreEqual(0f, juneAfterBottlingProcessingReport.Part2.InventoryOverage);
                Assert.AreEqual(0f, juneAfterBottlingProcessingReport.Part2.InventoryShortage);
                Assert.AreEqual(46f, juneAfterBottlingProcessingReport.Part2.OnHandEndofMonth);
                Assert.AreEqual(46f, juneAfterBottlingProcessingReport.Part2.OnHandFirstofMonth);
                Assert.AreEqual(0f, juneAfterBottlingProcessingReport.Part2.Recd4Process);
                Assert.AreEqual(0f, juneAfterBottlingProcessingReport.Part2.RecordedLosses);
                Assert.AreEqual(0f, juneAfterBottlingProcessingReport.Part2.TaxWithdrawn);
                Assert.AreEqual(0f, juneAfterBottlingProcessingReport.Part2.Transf2Prod4Redistil);

                // Processing Report Part 4
                Assert.AreEqual(0, juneAfterBottlingProcessingReport.Part4List.Count);

                #endregion
            }
            finally
            {
                // Cleanup
                foreach (var i in tablesForCleanupTupleList)
                {
                    TestRecordCleanup(i.Item1, i.Item2);
                }
            }
        }

        /// <summary>
        /// Test Processing Report part 4, column b (BULK SPIRITS DUMPED INTO PROCESSING (Whole proof gallons))
        /// </summary>
        [TestMethod()]
        public void PurchaseDistilled_Blend_GenerateProcessingReport()
        {
            // A tuple to log database test records for later clean-up
            List<Tuple<int, Table>> testRecords = new List<Tuple<int, Table>>();

            try
            {
                // Arrange

                // Create Spirit dictionary item
                SpiritObject spirit = new SpiritObject
                {
                    SpiritName = "VODKA-4tUTav",
                    ProcessingReportTypeID = 19
                };

                int spiritId = _dl.CreateSpirit(_userId, spirit);
                testRecords.Add(Tuple.Create(spiritId, Table.Spirit));

                // Create Raw Material dictionary item
                RawMaterialObject rawMaterial = new RawMaterialObject
                {
                    RawMaterialName = "TestMaterial-4tUTav",
                    PurchaseMaterialTypes = new PurchaseMaterialBooleanTypes { Distilled = true },
                    UnitTypeId = 1,
                    UnitType = "gal"
                };

                int rawMaterialId = _dl.CreateRawMaterial(_userId, rawMaterial);
                testRecords.Add(Tuple.Create(rawMaterialId, Table.MaterialDict));

                // Create Vendor dictionary item
                VendorObject vendor = new VendorObject
                {
                    VendorName = "TestVendor-4tUTav"
                };

                int vendorId = _dl.CreateVendor(_userId, vendor);
                testRecords.Add(Tuple.Create(vendorId, Table.Vendor));

                // Create Storage dictionary item
                StorageObject storage = new StorageObject
                {
                    StorageName = "TestStorage-4tUTav"
                };

                int storageId = _dl.CreateStorage(_userId, storage);
                testRecords.Add(Tuple.Create(storageId, Table.Storage));

                // Create Distilled Purchase record
                PurchaseObject purchase = new PurchaseObject
                {
                    PurBatchName = "TestPurchase-4tUTav",
                    PurchaseType = "Distilled",
                    PurchaseDate = new DateTime(2017, 1, 1),
                    Quantity = 100f,
                    AlcoholContent = 50f,
                    ProofGallon = 100f,
                    RecordId = rawMaterialId,
                    Price = 500f,
                    VendorId = vendorId,
                    Gauged = true,
                    SpiritTypeReportingID = 7,
                    Storage = new List<StorageObject>
                    {
                        new StorageObject { StorageId = storageId }
                    }
                };

                int purchaseId = _dl.CreatePurchase(purchase, _userId);
                testRecords.Add(Tuple.Create(purchaseId, Table.Purchase));

                // Create Production Blending record
                ProductionObject production = new ProductionObject
                {
                    BatchName = "TestBlending-4tUTav",
                    ProductionDate = new DateTime(2017, 1, 1),
                    ProductionStart = new DateTime(2017, 1, 1),
                    ProductionEnd = new DateTime(2017, 1, 1),
                    Gauged = true,
                    ProductionType = "Blending",
                    Quantity = 110f,
                    AlcoholContent = 40f,
                    ProofGallon = 88f,
                    SpiritTypeReportingID = 7,
                    ProductionTypeId = 3,
                    SpiritId = spiritId,
                    Storage = new List<StorageObject>
                    {
                        new StorageObject { StorageId = storageId }
                    },
                    UsedMats = new List<ObjInfo4Burndwn>
                    {
                        new ObjInfo4Burndwn
                        {
                            ID = purchaseId,
                            OldVal = 0f,
                            NewVal = purchase.Quantity,
                            DistillableOrigin = "pur",
                            BurningDownMethod = "volume"
                        }
                    }
                };

                int productionId = _dl.CreateProduction(production, _userId);
                testRecords.Add(Tuple.Create(productionId, Table.Production));

                // Act

                // Generate Processing Report
                ProcessingReportingObject report = _dl.GetProcessingReportData(new DateTime(2017, 1, 1), new DateTime(2017, 1, 31), _userId);

                // Assert
                var actual = report.Part4List.Find(x => x.ProcessingReportTypeName == "VODKA" && x.ProcessingSpirits == "bulkSpiritDumped").Vodka;
                Assert.IsTrue(report.Part4List.Any());
                // Blended Production had a quantity of 88 proof gallons
                Assert.AreEqual(88, actual);
            }
            finally
            {
                // Perform table cleanup
                foreach (var rec in testRecords)
                {
                    TestRecordCleanup(rec.Item1, rec.Item2);
                }
            }
        }

        /// <summary>
        /// Test for validation bug tracked in task 1690.
        /// Bug: The proof values in column b of Processing Report are removed when bottling a blended batch.
        /// </summary>
        /// 
        [TestMethod]
        public void PurchaseDistilled_Blend_Bottle()
        {
            // A tuple to log database test records for later clean-up
            List<Tuple<int, Table>> testRecords = new List<Tuple<int, Table>>();

            try
            {
                // Arrange

                // Create Spirit dictionary item
                SpiritObject spirit = new SpiritObject
                {
                    SpiritName = "GIN",
                    ProcessingReportTypeID = 18 // GIN
                };

                int spiritId = _dl.CreateSpirit(_userId, spirit);
                testRecords.Add(Tuple.Create(spiritId, Table.Spirit));

                // Create Raw Material dictionary item
                RawMaterialObject rawMaterial = new RawMaterialObject
                {
                    RawMaterialName = "GNS",
                    PurchaseMaterialTypes = new PurchaseMaterialBooleanTypes { Distilled = true },
                    UnitTypeId = 2, // lb
                    UnitType = "lb",
                    MaterialCategoryID = 1 // grain
                };

                int rawMaterialId = _dl.CreateRawMaterial(_userId, rawMaterial);
                testRecords.Add(Tuple.Create(rawMaterialId, Table.MaterialDict));

                // Create Vendor dictionary item
                VendorObject vendor = new VendorObject
                {
                    VendorName = "BigGrainsDistillery"
                };

                int vendorId = _dl.CreateVendor(_userId, vendor);
                testRecords.Add(Tuple.Create(vendorId, Table.Vendor));

                // Create Storage dictionary item
                StorageObject storage = new StorageObject
                {
                    StorageName = "ThaTank"
                };

                int storageId = _dl.CreateStorage(_userId, storage);
                testRecords.Add(Tuple.Create(storageId, Table.Storage));

                // Distilled purchase
                PurchaseObject purchase = new PurchaseObject
                {
                    PurBatchName = "GNS Gin",
                    PurchaseType = "Distilled",
                    PurchaseDate = new DateTime(2017, 11, 1),
                    VolumeByWeight = 1000f,
                    AlcoholContent = 75f,
                    ProofGallon = 100f,
                    RecordId = rawMaterialId,
                    Price = 2000f,
                    VendorId = vendorId,
                    Gauged = true,
                    Storage = new List<StorageObject>
                    {
                        new StorageObject { StorageId = storageId }
                    }
                };

                int purchaseId = _dl.CreatePurchase(purchase, _userId);
                testRecords.Add(Tuple.Create(purchaseId, Table.Purchase));

                // Blend and gauge
                ProductionObject blending = new ProductionObject
                {
                    BatchName = "Gin blending",
                    ProductionDate = new DateTime(2017, 11, 2),
                    ProductionStart = new DateTime(2017, 11, 2),
                    ProductionEnd = new DateTime(2017, 11, 2),
                    Gauged = true,
                    ProductionType = "Blending",
                    VolumeByWeight = 100f,
                    AlcoholContent = 37.5f,
                    ProofGallon = 100f,
                    SpiritTypeReportingID = 6, // Gin
                    ProductionTypeId = 3, // Blending
                    SpiritId = spiritId,
                    Storage = new List<StorageObject>
                    {
                        new StorageObject { StorageId = storageId }
                    },
                    UsedMats = new List<ObjInfo4Burndwn>
                    {
                        new ObjInfo4Burndwn
                        {
                            ID = purchaseId,
                            OldVal = 0f,
                            NewVal = purchase.VolumeByWeight,
                            DistillableOrigin = "pur",
                            BurningDownMethod = "weight"
                        }
                    }
                };

                int productionId1 = _dl.CreateProduction(blending, _userId);
                testRecords.Add(Tuple.Create(productionId1, Table.Production));

                // Bottle
                ProductionObject bottling = new ProductionObject
                {
                    BatchName = "Gin bottling",
                    ProductionDate = new DateTime(2017, 11, 2),
                    ProductionStart = new DateTime(2017, 11, 2),
                    ProductionEnd = new DateTime(2017, 11, 2),
                    Gauged = true,
                    ProductionType = "Bottling",
                    Quantity = 11.89f,
                    AlcoholContent = 37.5f,
                    ProofGallon = 8.91f,
                    SpiritTypeReportingID = 6, // Gin
                    ProductionTypeId = 3, // Blending
                    SpiritId = spiritId,
                    Storage = new List<StorageObject>
                    {
                        new StorageObject { StorageId = storageId }
                    },
                    UsedMats = new List<ObjInfo4Burndwn>
                    {
                        new ObjInfo4Burndwn
                        {
                            ID = productionId1,
                            OldVal = 0f,
                            NewVal = blending.VolumeByWeight,
                            DistillableOrigin = "prod",
                            BurningDownMethod = "weight"
                        }
                    }
                };

                int productionId2 = _dl.CreateProduction(bottling, _userId);
                testRecords.Add(Tuple.Create(productionId2, Table.Production));

                // Act

                // Generate Processing Report
                ProcessingReportingObject report = _dl.GetProcessingReportData(new DateTime(2017, 11, 1), new DateTime(2017, 11, 30), _userId);

                // Assert
                Assert.IsNotNull(report);
                Assert.IsNotNull(report.Header);
                Assert.IsTrue(report.Part4List.Count() > 0);
                Assert.IsTrue(report.Part4List.Exists(x => x.ProcessingSpirits == "bulkSpiritDumped" && x.Gin == 100f));
                Assert.IsTrue(report.Part4List.Exists(x => x.ProcessingSpirits == "bottled" && x.Gin == 11.89f));
            }
            finally
            {
                // Perform table cleanup
                foreach (var rec in testRecords)
                {
                    TestRecordCleanup(rec.Item1, rec.Item2);
                }
            }
        }

        /// <summary>
        /// Fermented pomace purchase workflow
        /// Buy Pomace and Distil Once
        /// </summary>
        [TestMethod()]
        public void BuyFermentedPomace_Distill_Redistill()
        {
            // A tuple to log database test records for later clean-up
            List<Tuple<int, Table>> testRecords = new List<Tuple<int, Table>>();

            try
            {
                // Arrange

                // Create Spirit dictionary item
                SpiritObject spirit = new SpiritObject
                {
                    SpiritName = "PomaceBrandy",
                    ProcessingReportTypeID = 12 // BRANDY DISTILLED AT 170 AND UNDER
                };

                int spiritId = _dl.CreateSpirit(_userId, spirit);
                testRecords.Add(Tuple.Create(spiritId, Table.Spirit));

                // Create Raw Material dictionary item
                RawMaterialObject rawMaterial = new RawMaterialObject
                {
                    RawMaterialName = "FermentedPomace",
                    PurchaseMaterialTypes = new PurchaseMaterialBooleanTypes { Fermented = true },
                    UnitTypeId = 2,
                    MaterialCategoryID = 2,
                    UnitType = "lb"
                };

                int rawMaterialId = _dl.CreateRawMaterial(_userId, rawMaterial);
                testRecords.Add(Tuple.Create(rawMaterialId, Table.MaterialDict));

                // Create Vendor dictionary item
                VendorObject vendor = new VendorObject
                {
                    VendorName = "BigGrapesWinery"
                };

                int vendorId = _dl.CreateVendor(_userId, vendor);
                testRecords.Add(Tuple.Create(vendorId, Table.Vendor));

                // Create Storage dictionary item
                StorageObject storage = new StorageObject
                {
                    StorageName = "TheTank"
                };

                int storageId = _dl.CreateStorage(_userId, storage);
                testRecords.Add(Tuple.Create(storageId, Table.Storage));

                // Create Fermented Purchase record
                PurchaseObject purchase = new PurchaseObject
                {
                    PurBatchName = "FermentedPomaceFromBigGrapesWinery",
                    PurchaseType = "Fermented",
                    PurchaseDate = new DateTime(2017, 1, 1),
                    VolumeByWeight = 1000f,
                    RecordId = rawMaterialId,
                    Price = 2000f,
                    VendorId = vendorId,
                    Gauged = true,
                    Storage = new List<StorageObject>
                    {
                        new StorageObject { StorageId = storageId }
                    }
                };

                int purchaseId = _dl.CreatePurchase(purchase, _userId);
                testRecords.Add(Tuple.Create(purchaseId, Table.Purchase));

                // Create Production Distillation record and Gauged to true
                ProductionObject production = new ProductionObject
                {
                    BatchName = "PomaceDistillation",
                    ProductionDate = new DateTime(2017, 1, 1),
                    ProductionStart = new DateTime(2017, 1, 1),
                    ProductionEnd = new DateTime(2017, 1, 1),
                    Gauged = true,
                    ProductionType = "Distillation",
                    Quantity = 100f,
                    AlcoholContent = 50f,
                    ProofGallon = 100f,
                    SpiritTypeReportingID = 3, // Brandy Under 170
                    ProductionTypeId = 2,
                    SpiritId = spiritId,
                    SpiritCutId = 11, // mixed
                    MaterialKindReportingID = 95, // All Other Brandy
                    Storage = new List<StorageObject>
                    {
                        new StorageObject { StorageId = storageId }
                    },
                    UsedMats = new List<ObjInfo4Burndwn>
                    {
                        new ObjInfo4Burndwn
                        {
                            ID = purchaseId,
                            OldVal = 0f,
                            NewVal = purchase.VolumeByWeight,
                            DistillableOrigin = "pur",
                            BurningDownMethod = "weight"
                        }
                    }
                };

                int productionId = _dl.CreateProduction(production, _userId);
                testRecords.Add(Tuple.Create(productionId, Table.Production));

                // Act

                // Generate Storage Report
                StorageReport storageReport = _dl.GetStorageReportData(new DateTime(2017, 1, 1), new DateTime(2017, 1, 31), _userId);

                // Generate Production Report
                ProductionReportingObject productionReport = _dl.GetProductionReportData(new DateTime(2017, 1, 1), new DateTime(2017, 1, 31), _userId);

                // Assert

                // storage report should contain only one category named "BrandyUnder170"
                Assert.AreEqual(1, storageReport.ReportBody.Count);
                Assert.AreEqual("BrandyUnder170", storageReport.ReportBody.First().CategoryName);
                Assert.AreEqual(100F, storageReport.ReportBody.First().r2_DepositedInBulkStorage);

                // production report should contain three categories: Part1List, Part2Through4List, Part6
                Assert.AreEqual(1, productionReport.Part1List.Count);
                Assert.AreEqual("BrandyUnder170", productionReport.Part1List.First().SpiritCatName);
                Assert.AreEqual(100F, productionReport.Part1List.First().ProducedTotal);
                Assert.AreEqual(1, productionReport.Part2Through4List.Count);
                Assert.AreEqual("AllOtherBrandy", productionReport.Part2Through4List.First().KindOfMaterial);
                Assert.AreEqual(100F, productionReport.Part2Through4List.First().ProofGallons);
                Assert.AreEqual(1, productionReport.ProdReportPart6List.Count);
                Assert.AreEqual("FermentedPomace", productionReport.ProdReportPart6List.First().KindOfMaterial);
                // Test fails because of this assertion. Weight is 0. Debug.
                Assert.AreEqual(1000F, productionReport.ProdReportPart6List.First().Weight);
            }
            finally
            {
                // Perform table cleanup
                foreach (var rec in testRecords)
                {
                    TestRecordCleanup(rec.Item1, rec.Item2);
                }
            }
        }

        /// <summary>
        /// 1. Purchase 196(100@98) proof gallons of GNS on 1/1/2017
        /// 2. Redistill half of purchased GNS, 98(50@98) proof gallons as GIN on 1/2/2017
        /// 3. Blend the GIN batch with 50 gallons of additive for a total of 98(100@49) proof gallons on 2/1/2017
        /// </summary>
        [TestMethod()]
        public void BuyGNS_RedistilHalfIntoGin_BlendWithWater()
        {
            // Tuple<recordId, table enum value>
            List<Tuple<int, Table>> tablesForCleanupTupleList = new List<Tuple<int, Table>>();

            int spiritId = 0;
            int vendorId = 0;
            int storageId = 0;
            int gnsMaterialId = 0;
            int waterMaterialId = 0;
            int purchaseId = 0;
            int productionId = 0;

            try
            {
                #region Arrange
                //  dictionary setup
                SpiritObject spirit = new SpiritObject();
                spirit.SpiritName = "GIN";
                spirit.ProcessingReportTypeID = 18;

                spiritId = _dl.CreateSpirit(_userId, spirit);

                // setup Vendor object
                VendorObject vendor = new VendorObject();
                vendor.VendorName = "testVendor";

                vendorId = _dl.CreateVendor(_userId, vendor);
                tablesForCleanupTupleList.Add(Tuple.Create(vendorId, Table.Vendor));

                // setup Storage Object
                StorageObject storage = new StorageObject();
                storage.StorageName = "testStorage";
                storage.SerialNumber = "2H29NNS";

                storageId = _dl.CreateStorage(_userId, storage);
                tablesForCleanupTupleList.Add(Tuple.Create(storageId, Table.Storage));

                // setup Material Object
                // GNS
                {
                    RawMaterialObject gnsMaterial = new RawMaterialObject();
                    gnsMaterial.RawMaterialName = "GNS for GIN";
                    gnsMaterial.UnitType = "gal";
                    gnsMaterial.UnitTypeId = 1;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Distilled = true;
                    gnsMaterial.PurchaseMaterialTypes = materialBoolTypes;

                    gnsMaterialId = _dl.CreateRawMaterial(_userId, gnsMaterial);
                    tablesForCleanupTupleList.Add(Tuple.Create(gnsMaterialId, Table.MaterialDict));
                }

                // water
                {
                    RawMaterialObject waterMaterial = new RawMaterialObject();
                    waterMaterial.RawMaterialName = "Water";
                    waterMaterial.UnitType = "gal";
                    waterMaterial.UnitTypeId = 1;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Additive = true;
                    waterMaterial.PurchaseMaterialTypes = materialBoolTypes;

                    waterMaterialId = _dl.CreateRawMaterial(_userId, waterMaterial);
                    tablesForCleanupTupleList.Add(Tuple.Create(waterMaterialId, Table.MaterialDict));
                }

                // create Purchase Record (minimal required fields)
                PurchaseObject purchO = new PurchaseObject();
                purchO.PurBatchName = "GNS";
                purchO.PurchaseType = "Distilled";
                purchO.PurchaseDate = new DateTime(2017, 1, 1);
                purchO.Quantity = 100f; // 100 gallons
                purchO.VolumeByWeight = 0f;
                purchO.AlcoholContent = 98f;
                purchO.ProofGallon = 196f;
                purchO.RecordId = gnsMaterialId;
                purchO.Price = 350f;
                purchO.VendorId = vendorId;
                purchO.Gauged = true;

                List<StorageObject> storageList = new List<StorageObject>();
                StorageObject storageObject = new StorageObject();
                storageObject.StorageId = storageId;
                storageList.Add(storageObject);
                purchO.Storage = storageList;

                purchO.SpiritTypeReportingID = 9;
                purchO.Gauged = true;

                purchaseId = _dl.CreatePurchase(purchO, _userId);
                tablesForCleanupTupleList.Add(Tuple.Create(purchaseId, Table.Purchase));

                // Redistil GNS into GIN and mark it as Gauged
                ProductionObject prodO = new ProductionObject();
                prodO.BatchName = "RedistilledGns";
                prodO.ProductionDate = new DateTime(2017, 1, 2);
                prodO.ProductionStart = new DateTime(2017, 1, 2);
                prodO.ProductionEnd = new DateTime(2017, 1, 2);
                prodO.SpiritCutId = 11; // mixed
                prodO.Gauged = true;
                prodO.ProductionType = "Distillation";
                prodO.Quantity = 50f; // 50 gallons of alcohol
                prodO.VolumeByWeight = 0f;
                prodO.AlcoholContent = 98f; // 98%
                prodO.ProofGallon = 98f; // 98 pfg
                prodO.Storage = storageList; // we are using the same storage id as we use for Purchase to keep things simple
                prodO.SpiritTypeReportingID = 6; // Gin
                prodO.ProductionTypeId = 2;

                List<ObjInfo4Burndwn> usedMats = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat = new ObjInfo4Burndwn();
                uMat.ID = purchaseId;
                uMat.OldVal = 50f;
                uMat.NewVal = 50f;
                uMat.Proof = 98f;
                uMat.DistillableOrigin = "pur";
                uMat.BurningDownMethod = "volume";

                usedMats.Add(uMat);

                prodO.UsedMats = usedMats;

                productionId = _dl.CreateProduction(prodO, _userId);
                tablesForCleanupTupleList.Add(Tuple.Create(productionId, Table.Production));

                // create Production Blending Record
                ProductionObject prodBlend = new ProductionObject();
                prodBlend.BatchName = "Gin";
                prodBlend.ProductionDate = new DateTime(2017, 2, 1);
                prodBlend.ProductionStart = new DateTime(2017, 2, 1);
                prodBlend.ProductionEnd = new DateTime(2017, 2, 1);
                prodBlend.Gauged = true;
                prodBlend.ProductionType = "Blending";
                prodBlend.Quantity = 100f; // 100 gallons of alcohol
                prodBlend.VolumeByWeight = 0f;
                prodBlend.AlcoholContent = 49f; // 49%
                prodBlend.ProofGallon = 98f; // 98pfg
                prodBlend.Storage = storageList; // we are using the same storage id as we use for Purchase to keep things simple
                prodBlend.SpiritTypeReportingID = 6; // GIN
                prodBlend.SpiritId = spiritId;
                prodBlend.ProductionTypeId = 3;

                List<ObjInfo4Burndwn> usedMats4Blend = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat4Blend = new ObjInfo4Burndwn();
                uMat4Blend.ID = productionId;
                uMat4Blend.OldVal = 0f;
                uMat4Blend.NewVal = 50f;
                uMat4Blend.DistillableOrigin = "prod";
                uMat4Blend.BurningDownMethod = "volume";

                usedMats4Blend.Add(uMat4Blend);
                prodBlend.UsedMats = usedMats4Blend;

                List<BlendingAdditive> blendAdditives = new List<BlendingAdditive>();
                BlendingAdditive blendAd = new BlendingAdditive();
                blendAd.RawMaterialId = waterMaterialId;
                blendAd.RawMaterialQuantity = 50f;
                blendAd.RawMaterialName = "Water";
                blendAd.UnitOfMeasurement = "gal";

                blendAdditives.Add(blendAd);

                prodBlend.BlendingAdditives = blendAdditives;

                productionId = _dl.CreateProduction(prodBlend, _userId); // here productionId is overriden with a new productionId of the new Gauged record
                tablesForCleanupTupleList.Add(Tuple.Create(productionId, Table.Production));

                #endregion

                #region Act

                int decDays = DateTime.DaysInMonth(2016, 12);
                int janDays = DateTime.DaysInMonth(2017, 1);
                int febDays = DateTime.DaysInMonth(2017, 2);
                int marDays = DateTime.DaysInMonth(2017, 3);

                var decStart = new DateTime(2016, 12, 1);
                var decEnd = new DateTime(2016, 12, decDays);
                var janStart = new DateTime(2017, 1, 1);
                var janEnd = new DateTime(2017, 1, janDays);
                var febStart = new DateTime(2017, 2, 1);
                var febEnd = new DateTime(2017, 2, febDays);
                var marStart = new DateTime(2017, 3, 1);
                var marEnd = new DateTime(2017, 3, marDays);

                StorageReport decStorageReport = _dl.GetStorageReportData(decStart, decEnd, _userId);
                StorageReport janStorageReport = _dl.GetStorageReportData(janStart, janEnd, _userId);
                StorageReport febStorageReport = _dl.GetStorageReportData(febStart, febEnd, _userId);
                StorageReport marStorageReport = _dl.GetStorageReportData(marStart, marEnd, _userId);

                #endregion

                #region Assert

                // December 2016

                // No data records are generated
                Assert.IsFalse(decStorageReport.ReportBody.Any());

                // January 2017 Storage Report

                // Only "GIN" AND "190 AND OVER" records are generated
                Assert.AreEqual(2, janStorageReport.ReportBody.Count);
                Assert.IsTrue(janStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 6));
                Assert.IsTrue(janStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 9));

                Assert.IsFalse(janStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 1));
                Assert.IsFalse(janStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 2));
                Assert.IsFalse(janStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 3));
                Assert.IsFalse(janStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 4));
                Assert.IsFalse(janStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 5));
                Assert.IsFalse(janStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 7));
                Assert.IsFalse(janStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 8));
                Assert.IsFalse(janStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 10));
                Assert.IsFalse(janStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 11));

                // GIN
                Assert.AreEqual(0f, janStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 6).Select(x => x.r1_OnHandFirstOfMonth).Single());
                Assert.AreEqual(98f, janStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 6).Select(x => x.r2_DepositedInBulkStorage).Single());
                Assert.AreEqual(0f, janStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 6).Select(x => x.r4_ReturnedToBulkStorage).Single());
                Assert.AreEqual(98f, janStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 6).Select(x => x.r6_TotalLines1Through5).Single());
                Assert.AreEqual(0f, janStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 6).Select(x => x.r7_TaxPaid).Single());
                Assert.AreEqual(0f, janStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 6).Select(x => x.r17_TransferredToProcessingAccount).Single());
                Assert.AreEqual(0f, janStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 6).Select(x => x.r18_TransferredToProductionAccount).Single());
                Assert.AreEqual(0f, janStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 6).Select(x => x.r19_TransferredToOtherBondedPremises).Single());
                Assert.AreEqual(0f, janStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 6).Select(x => x.r20_Destroyed).Single());
                Assert.AreEqual(0f, janStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 6).Select(x => x.r22_OtherLosses).Single());
                Assert.AreEqual(98f, janStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 6).Select(x => x.r23_OnHandEndOfMonth).Single());
                Assert.AreEqual(98f, janStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 6).Select(x => x.r24_Lines7Through23).Single());

                // 190 AND OVER
                Assert.AreEqual(0f, janStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 9).Select(x => x.r1_OnHandFirstOfMonth).Single());
                Assert.AreEqual(196f, janStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 9).Select(x => x.r2_DepositedInBulkStorage).Single());
                Assert.AreEqual(0f, janStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 9).Select(x => x.r4_ReturnedToBulkStorage).Single());
                Assert.AreEqual(196f, janStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 9).Select(x => x.r6_TotalLines1Through5).Single());
                Assert.AreEqual(0f, janStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 9).Select(x => x.r7_TaxPaid).Single());
                Assert.AreEqual(0f, janStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 9).Select(x => x.r17_TransferredToProcessingAccount).Single());
                Assert.AreEqual(98f, janStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 9).Select(x => x.r18_TransferredToProductionAccount).Single());
                Assert.AreEqual(0f, janStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 9).Select(x => x.r19_TransferredToOtherBondedPremises).Single());
                Assert.AreEqual(0f, janStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 9).Select(x => x.r20_Destroyed).Single());
                Assert.AreEqual(0f, janStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 9).Select(x => x.r22_OtherLosses).Single());
                Assert.AreEqual(98f, janStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 9).Select(x => x.r23_OnHandEndOfMonth).Single());
                Assert.AreEqual(196f, janStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 9).Select(x => x.r24_Lines7Through23).Single());

                // February 2017 Storage Report

                // Only "GIN" AND "190 AND OVER" records are generated
                Assert.AreEqual(2, febStorageReport.ReportBody.Count);
                Assert.IsTrue(febStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 6));
                Assert.IsTrue(febStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 9));

                Assert.IsFalse(febStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 1));
                Assert.IsFalse(febStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 2));
                Assert.IsFalse(febStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 3));
                Assert.IsFalse(febStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 4));
                Assert.IsFalse(febStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 5));
                Assert.IsFalse(febStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 7));
                Assert.IsFalse(febStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 8));
                Assert.IsFalse(febStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 10));
                Assert.IsFalse(febStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 11));

                // GIN
                Assert.AreEqual(98f, febStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 6).Select(x => x.r1_OnHandFirstOfMonth).Single());
                Assert.AreEqual(0f, febStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 6).Select(x => x.r2_DepositedInBulkStorage).Single());
                Assert.AreEqual(0f, febStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 6).Select(x => x.r4_ReturnedToBulkStorage).Single());
                Assert.AreEqual(98f, febStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 6).Select(x => x.r6_TotalLines1Through5).Single());
                Assert.AreEqual(0f, febStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 6).Select(x => x.r7_TaxPaid).Single());
                Assert.AreEqual(98f, febStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 6).Select(x => x.r17_TransferredToProcessingAccount).Single());
                Assert.AreEqual(0f, febStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 6).Select(x => x.r18_TransferredToProductionAccount).Single());
                Assert.AreEqual(0f, febStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 6).Select(x => x.r19_TransferredToOtherBondedPremises).Single());
                Assert.AreEqual(0f, febStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 6).Select(x => x.r20_Destroyed).Single());
                Assert.AreEqual(0f, febStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 6).Select(x => x.r22_OtherLosses).Single());
                Assert.AreEqual(0f, febStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 6).Select(x => x.r23_OnHandEndOfMonth).Single());
                Assert.AreEqual(98f, febStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 6).Select(x => x.r24_Lines7Through23).Single());

                // 190 AND OVER
                Assert.AreEqual(98f, febStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 9).Select(x => x.r1_OnHandFirstOfMonth).Single());
                Assert.AreEqual(0f, febStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 9).Select(x => x.r2_DepositedInBulkStorage).Single());
                Assert.AreEqual(0f, febStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 9).Select(x => x.r4_ReturnedToBulkStorage).Single());
                Assert.AreEqual(98f, febStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 9).Select(x => x.r6_TotalLines1Through5).Single());
                Assert.AreEqual(0f, febStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 9).Select(x => x.r7_TaxPaid).Single());
                Assert.AreEqual(0f, febStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 9).Select(x => x.r17_TransferredToProcessingAccount).Single());
                Assert.AreEqual(0f, febStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 9).Select(x => x.r18_TransferredToProductionAccount).Single());
                Assert.AreEqual(0f, febStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 9).Select(x => x.r19_TransferredToOtherBondedPremises).Single());
                Assert.AreEqual(0f, febStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 9).Select(x => x.r20_Destroyed).Single());
                Assert.AreEqual(0f, febStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 9).Select(x => x.r22_OtherLosses).Single());
                Assert.AreEqual(98f, febStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 9).Select(x => x.r23_OnHandEndOfMonth).Single());
                Assert.AreEqual(98f, febStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 9).Select(x => x.r24_Lines7Through23).Single());

                // March 2017 Storage Report

                // Only "190 AND OVER" records generated
                Assert.AreEqual(1, marStorageReport.ReportBody.Count);
                Assert.IsTrue(marStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 9));

                Assert.IsFalse(marStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 1));
                Assert.IsFalse(marStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 2));
                Assert.IsFalse(marStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 3));
                Assert.IsFalse(marStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 4));
                Assert.IsFalse(marStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 5));
                Assert.IsFalse(marStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 6));
                Assert.IsFalse(marStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 7));
                Assert.IsFalse(marStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 8));
                Assert.IsFalse(marStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 10));
                Assert.IsFalse(marStorageReport.ReportBody.Exists(x => x.SpiritTypeReportingID == 11));

                // 190 AND OVER
                Assert.AreEqual(98f, marStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 9).Select(x => x.r1_OnHandFirstOfMonth).Single());
                Assert.AreEqual(0f, marStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 9).Select(x => x.r2_DepositedInBulkStorage).Single());
                Assert.AreEqual(0f, marStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 9).Select(x => x.r4_ReturnedToBulkStorage).Single());
                Assert.AreEqual(98f, marStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 9).Select(x => x.r6_TotalLines1Through5).Single());
                Assert.AreEqual(0f, marStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 9).Select(x => x.r7_TaxPaid).Single());
                Assert.AreEqual(0f, marStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 9).Select(x => x.r17_TransferredToProcessingAccount).Single());
                Assert.AreEqual(0f, marStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 9).Select(x => x.r18_TransferredToProductionAccount).Single());
                Assert.AreEqual(0f, marStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 9).Select(x => x.r19_TransferredToOtherBondedPremises).Single());
                Assert.AreEqual(0f, marStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 9).Select(x => x.r20_Destroyed).Single());
                Assert.AreEqual(0f, marStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 9).Select(x => x.r22_OtherLosses).Single());
                Assert.AreEqual(98f, marStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 9).Select(x => x.r23_OnHandEndOfMonth).Single());
                Assert.AreEqual(98f, marStorageReport.ReportBody.Where(x => x.SpiritTypeReportingID == 9).Select(x => x.r24_Lines7Through23).Single());

                #endregion
            }
            finally
            {
                // Cleanup
                foreach (var i in tablesForCleanupTupleList)
                {
                    TestRecordCleanup(i.Item1, i.Item2);
                }
            }
        }

        /// <summary>
        /// 1. Purchase 196(100 gallons @98 %alcohol) proof gallons of GNS on 06/1/2018
        /// 2. Redistill half of purchased GNS, 98(50@98) proof gallons as GIN on 06/02/2018
        /// 3. Blend the GIN batch with 50 gallons of additive for a total of 98(100@49) proof gallons on 6/3/2018
        /// 4. Bottle all of it on 6/4/2018
        /// 5. Pull Processing Report
        /// </summary>
        [TestMethod()]
        public void BuyGNS_RedistilHalfIntoGin_Blend_Bottle_MakeSure_Processing_Doesnt_throw()
        {
            // Tuple<recordId, table enum value>
            List<Tuple<int, Table>> tablesForCleanupTupleList = new List<Tuple<int, Table>>();

            int spiritId = 0;
            int vendorId = 0;
            int storageId = 0;
            int gnsMaterialId = 0;
            int waterMaterialId = 0;
            int purchaseId = 0;
            int productionId = 0;

            try
            {
                #region Arrange
                //  dictionary setup
                SpiritObject spirit = new SpiritObject();
                spirit.SpiritName = "GIN";
                spirit.ProcessingReportTypeID = 18;

                spiritId = _dl.CreateSpirit(_userId, spirit);

                // setup Vendor object
                VendorObject vendor = new VendorObject();
                vendor.VendorName = "testVendor";

                vendorId = _dl.CreateVendor(_userId, vendor);
                tablesForCleanupTupleList.Add(Tuple.Create(vendorId, Table.Vendor));

                // setup Storage Object
                StorageObject storage = new StorageObject();
                storage.StorageName = "testStorage";
                storage.SerialNumber = "2H29NNS";

                storageId = _dl.CreateStorage(_userId, storage);
                tablesForCleanupTupleList.Add(Tuple.Create(storageId, Table.Storage));

                // setup Material Object
                // GNS
                {
                    RawMaterialObject gnsMaterial = new RawMaterialObject();
                    gnsMaterial.RawMaterialName = "GNS for GIN";
                    gnsMaterial.UnitType = "gal";
                    gnsMaterial.UnitTypeId = 1;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Distilled = true;
                    gnsMaterial.PurchaseMaterialTypes = materialBoolTypes;

                    gnsMaterialId = _dl.CreateRawMaterial(_userId, gnsMaterial);
                    tablesForCleanupTupleList.Add(Tuple.Create(gnsMaterialId, Table.MaterialDict));
                }

                // water
                {
                    RawMaterialObject waterMaterial = new RawMaterialObject();
                    waterMaterial.RawMaterialName = "Water";
                    waterMaterial.UnitType = "gal";
                    waterMaterial.UnitTypeId = 1;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Additive = true;
                    waterMaterial.PurchaseMaterialTypes = materialBoolTypes;

                    waterMaterialId = _dl.CreateRawMaterial(_userId, waterMaterial);
                    tablesForCleanupTupleList.Add(Tuple.Create(waterMaterialId, Table.MaterialDict));
                }

                // create Purchase Record (minimal required fields)
                PurchaseObject purchO = new PurchaseObject();
                purchO.PurBatchName = "GNS";
                purchO.PurchaseType = "Distilled";
                purchO.PurchaseDate = new DateTime(2018, 03, 01);
                purchO.Quantity = 100f; // 100 gallons
                purchO.VolumeByWeight = 0f;
                purchO.AlcoholContent = 98f;
                purchO.ProofGallon = 196f;
                purchO.RecordId = gnsMaterialId;
                purchO.Price = 350f;
                purchO.VendorId = vendorId;
                purchO.Gauged = true;

                List<StorageObject> storageList = new List<StorageObject>();
                StorageObject storageObject = new StorageObject();
                storageObject.StorageId = storageId;
                storageList.Add(storageObject);
                purchO.Storage = storageList;

                purchO.SpiritTypeReportingID = 9;
                purchO.Gauged = true;

                purchaseId = _dl.CreatePurchase(purchO, _userId);
                tablesForCleanupTupleList.Add(Tuple.Create(purchaseId, Table.Purchase));

                // Redistil GNS into GIN and mark it as Gauged
                ProductionObject prodO = new ProductionObject();
                prodO.BatchName = "RedistilledGns";
                prodO.ProductionDate = new DateTime(2018, 04, 02);
                prodO.ProductionStart = new DateTime(2018, 04, 02);
                prodO.ProductionEnd = new DateTime(2018, 04, 02);
                prodO.SpiritCutId = 11; // mixed
                prodO.Gauged = true;
                prodO.ProductionType = "Distillation";
                prodO.Quantity = 50f; // 50 gallons of alcohol
                prodO.VolumeByWeight = 0f;
                prodO.AlcoholContent = 98f; // 98%
                prodO.ProofGallon = 98f; // 98 pfg
                prodO.Storage = storageList; // we are using the same storage id as we use for Purchase to keep things simple
                prodO.SpiritTypeReportingID = 6; // Gin
                prodO.ProductionTypeId = 2;

                List<ObjInfo4Burndwn> usedMats = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat = new ObjInfo4Burndwn();
                uMat.ID = purchaseId;
                uMat.OldVal = 50f;
                uMat.NewVal = 50f;
                uMat.Proof = 98f;
                uMat.DistillableOrigin = "pur";
                uMat.BurningDownMethod = "volume";

                usedMats.Add(uMat);

                prodO.UsedMats = usedMats;

                productionId = _dl.CreateProduction(prodO, _userId);
                tablesForCleanupTupleList.Add(Tuple.Create(productionId, Table.Production));

                // create Production Blending Record
                ProductionObject prodBlend = new ProductionObject();
                prodBlend.BatchName = "Gin";
                prodBlend.ProductionDate = new DateTime(2018, 05, 03);
                prodBlend.ProductionStart = new DateTime(2018, 05, 03);
                prodBlend.ProductionEnd = new DateTime(2018, 05, 03);
                prodBlend.Gauged = true;
                prodBlend.ProductionType = "Blending";
                prodBlend.Quantity = 100f; // 100 gallons of alcohol
                prodBlend.VolumeByWeight = 0f;
                prodBlend.AlcoholContent = 49f; // 49%
                prodBlend.ProofGallon = 98f; // 98pfg
                prodBlend.Storage = storageList; // we are using the same storage id as we use for Purchase to keep things simple
                prodBlend.SpiritTypeReportingID = 6; // GIN
                prodBlend.SpiritId = spiritId;
                prodBlend.ProductionTypeId = 3;

                List<ObjInfo4Burndwn> usedMats4Blend = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat4Blend = new ObjInfo4Burndwn();
                uMat4Blend.ID = productionId;
                uMat4Blend.OldVal = 0f;
                uMat4Blend.NewVal = 50f;
                uMat4Blend.DistillableOrigin = "prod";
                uMat4Blend.BurningDownMethod = "volume";

                usedMats4Blend.Add(uMat4Blend);
                prodBlend.UsedMats = usedMats4Blend;

                List<BlendingAdditive> blendAdditives = new List<BlendingAdditive>();
                BlendingAdditive blendAd = new BlendingAdditive();
                blendAd.RawMaterialId = waterMaterialId;
                blendAd.RawMaterialQuantity = 50f;
                blendAd.RawMaterialName = "Water";
                blendAd.UnitOfMeasurement = "gal";

                blendAdditives.Add(blendAd);

                prodBlend.BlendingAdditives = blendAdditives;

                productionId = _dl.CreateProduction(prodBlend, _userId); // here productionId is overriden with a new productionId of the new Gauged record
                tablesForCleanupTupleList.Add(Tuple.Create(productionId, Table.Production));

                // create Production Bottling Record
                ProductionObject prodBottl = new ProductionObject();
                prodBottl.BatchName = "GIN Bottling Test ";
                prodBottl.ProductionDate = new DateTime(2018, 06, 4);
                prodBottl.ProductionStart = new DateTime(2018, 06, 4);
                prodBottl.ProductionEnd = new DateTime(2018, 06, 4);
                prodBottl.Gauged = true;
                prodBottl.ProductionType = "Bottling";
                prodBottl.Quantity = 150f; // 150 gallons of alcohol
                prodBottl.VolumeByWeight = 0f;
                prodBottl.AlcoholContent = 32.5f; // 45%
                prodBottl.ProofGallon = 97.5f; // 97.5 pfg
                prodBottl.Storage = storageList; // we are using the same storage id as we use for Purchase to keep things simple
                prodBottl.SpiritTypeReportingID = 6; // Gin
                prodBottl.SpiritId = spiritId;
                prodO.ProductionTypeId = 4;
                // BUG?

                List<ObjInfo4Burndwn> usedMats4Bottl = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat4Bottl = new ObjInfo4Burndwn();
                uMat4Bottl.ID = productionId;
                uMat4Bottl.OldVal = 0f;
                uMat4Bottl.NewVal = prodBlend.Quantity;
                uMat4Bottl.DistillableOrigin = "prod";
                uMat4Bottl.BurningDownMethod = "volume";

                usedMats4Bottl.Add(uMat4Bottl);
                prodBottl.UsedMats = usedMats4Bottl;

                BottlingObject bottlingObj = new BottlingObject();
                bottlingObj.CaseCapacity = 10;
                bottlingObj.CaseQuantity = 113f;
                bottlingObj.BottleCapacity = 500f;
                bottlingObj.BottleQuantity = 1130;

                prodBottl.BottlingInfo = bottlingObj;

                prodBottl.GainLoss = .4f;

                prodBottl.FillTestList = null;

                productionId = _dl.CreateProduction(prodBottl, _userId); // here productionId is overriden with a new productionId of the new Gauged record
                tablesForCleanupTupleList.Add(Tuple.Create(productionId, Table.Production));
                #endregion

                #region Processing Report
                // Report Header
                ReportHeader reportHeaderE = new ReportHeader();
                reportHeaderE.ProprietorName = "Test Distillery";
                reportHeaderE.EIN = "12-3456789";
                reportHeaderE.ReportDate = "June 2018";
                reportHeaderE.PlantAddress = "123 Cognac Drive Renton WASHINGTON 98059";
                reportHeaderE.DSP = "DSP-WA-21086";

                DateTime start = new DateTime(2018, 06, 01);
                DateTime end = new DateTime(2018, 06, 30);

                ProcessingReportingObject actualProcessingReportObject = new ProcessingReportingObject();

                actualProcessingReportObject = _dl.GetProcessingReportData(start, end, _userId);

                Assert.AreEqual(reportHeaderE.DSP, actualProcessingReportObject.Header.DSP);
                Assert.AreEqual(reportHeaderE.EIN, actualProcessingReportObject.Header.EIN);
                Assert.AreEqual(reportHeaderE.PlantAddress, actualProcessingReportObject.Header.PlantAddress);
                Assert.AreEqual(reportHeaderE.ProprietorName, actualProcessingReportObject.Header.ProprietorName);
                Assert.AreEqual(reportHeaderE.ReportDate, actualProcessingReportObject.Header.ReportDate);

                Assert.AreEqual(97.5f, actualProcessingReportObject.Part1.AmtBottledPackaged);
                Assert.AreEqual("spirit", actualProcessingReportObject.Part1.BulkIngredients);
                Assert.AreEqual(0f, actualProcessingReportObject.Part1.Destroyed);
                Assert.AreEqual(0f, actualProcessingReportObject.Part1.Dumped4Processing);
                Assert.AreEqual(0f, actualProcessingReportObject.Part1.Gains);
                Assert.AreEqual(0.4f, actualProcessingReportObject.Part1.Losses);
                Assert.AreEqual(0.1f, actualProcessingReportObject.Part1.OnHandEndofMonth);
                Assert.AreEqual(98f, actualProcessingReportObject.Part1.OnHandFirstofMonth);
                Assert.AreEqual(0f, actualProcessingReportObject.Part1.Recd4Process);
                Assert.AreEqual(0f, actualProcessingReportObject.Part1.Transf2Prod4Redistil);
                Assert.AreEqual(0f, actualProcessingReportObject.Part1.Used4Redistil);
                Assert.AreEqual(0f, actualProcessingReportObject.Part1.WineMixedWithSpirit);

                Assert.AreEqual(97.5f, actualProcessingReportObject.Part2.AmtBottledPackaged);
                Assert.AreEqual(0f, actualProcessingReportObject.Part2.Destroyed);
                Assert.AreEqual(0f, actualProcessingReportObject.Part2.Dumped4Processing);
                Assert.AreEqual("bottled", actualProcessingReportObject.Part2.FinishedProduct);
                Assert.AreEqual(0f, actualProcessingReportObject.Part2.InventoryOverage);
                Assert.AreEqual(0f, actualProcessingReportObject.Part2.InventoryShortage);
                Assert.AreEqual(97.5f, actualProcessingReportObject.Part2.OnHandEndofMonth);
                Assert.AreEqual(0f, actualProcessingReportObject.Part2.OnHandFirstofMonth);
                Assert.AreEqual(0f, actualProcessingReportObject.Part2.Recd4Process);
                Assert.AreEqual(0f, actualProcessingReportObject.Part2.RecordedLosses);
                Assert.AreEqual(0f, actualProcessingReportObject.Part2.TaxWithdrawn);
                Assert.AreEqual(0f, actualProcessingReportObject.Part2.Transf2Prod4Redistil);

                #endregion
            }
            finally
            {
                // Cleanup
                foreach (var i in tablesForCleanupTupleList)
                {
                    TestRecordCleanup(i.Item1, i.Item2);
                }
            }
        }

        /// <summary>
        /// This test tests workflow: Buy GNS -> Redistil -> Blend -> Bottle
        /// </summary>
        [TestMethod()]
        public void BuyGNS_RedistilOnce_Make_Gin()
        {
            // Arrange
            List<Tuple<int/*recordId*/, Table/*table enum vaue*/>> tablesForCleanupTupleList = new List<Tuple<int, Table>>();
            int spiritId = 0;
            int vendorId = 0;
            int storageId = 0;
            int gnsMaterialId = 0;
            int waterMaterialId = 0;
            int purchaseId = 0;
            int productionId = 0;

            try
            {

                #region Dictionary
                //  dictionary setup
                SpiritObject spirit = new SpiritObject();
                spirit.SpiritName = "GIN";
                spirit.ProcessingReportTypeID = 18;

                spiritId = _dl.CreateSpirit(_userId, spirit);

                // setup Vendor object
                VendorObject vendor = new VendorObject();
                vendor.VendorName = "testVendor";

                vendorId = _dl.CreateVendor(_userId, vendor);
                tablesForCleanupTupleList.Add(Tuple.Create(vendorId, Table.Vendor));

                // setup Storage Object
                StorageObject storage = new StorageObject();
                storage.StorageName = "testStorage";
                storage.SerialNumber = "2H29NNS";

                storageId = _dl.CreateStorage(_userId, storage);
                tablesForCleanupTupleList.Add(Tuple.Create(storageId, Table.Storage));

                // setup Material Object
                // GNS
                {
                    RawMaterialObject gnsMaterial = new RawMaterialObject();
                    gnsMaterial.RawMaterialName = "GNS for GIN";
                    gnsMaterial.UnitType = "gal";
                    gnsMaterial.UnitTypeId = 1;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Distilled = true;
                    gnsMaterial.PurchaseMaterialTypes = materialBoolTypes;

                    gnsMaterialId = _dl.CreateRawMaterial(_userId, gnsMaterial);
                    tablesForCleanupTupleList.Add(Tuple.Create(gnsMaterialId, Table.MaterialDict));
                }

                // water
                {
                    RawMaterialObject waterMaterial = new RawMaterialObject();
                    waterMaterial.RawMaterialName = "Water";
                    waterMaterial.UnitType = "gal";
                    waterMaterial.UnitTypeId = 1;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Additive = true;
                    waterMaterial.PurchaseMaterialTypes = materialBoolTypes;

                    waterMaterialId = _dl.CreateRawMaterial(_userId, waterMaterial);
                    tablesForCleanupTupleList.Add(Tuple.Create(waterMaterialId, Table.MaterialDict));
                }
                #endregion

                #region Purchase
                // create Purchase Record (minimal required fields)
                PurchaseObject purchO = new PurchaseObject();
                purchO.PurBatchName = "GNS Purchase Test";
                purchO.PurchaseType = "Distilled";
                purchO.PurchaseDate = new DateTime(2017, 09, 1);
                purchO.Quantity = 100f; // 100 gallons
                purchO.VolumeByWeight = 0f;
                purchO.AlcoholContent = 90f;
                purchO.ProofGallon = 180f;
                purchO.RecordId = gnsMaterialId;
                purchO.Price = 350f;
                purchO.VendorId = vendorId;
                purchO.Gauged = true;

                List<StorageObject> storageList = new List<StorageObject>();
                StorageObject storageObject = new StorageObject();
                storageObject.StorageId = storageId;
                storageList.Add(storageObject);
                purchO.Storage = storageList;

                purchO.SpiritTypeReportingID = 8;
                purchO.Gauged = true;

                purchaseId = _dl.CreatePurchase(purchO, _userId);
                tablesForCleanupTupleList.Add(Tuple.Create(purchaseId, Table.Purchase));
                #endregion

                #region Production
                // Redistil GNS into GIN and mark it as Gauged
                ProductionObject prodO = new ProductionObject();
                prodO.BatchName = "GNS to Gin";
                prodO.ProductionDate = new DateTime(2017, 09, 3);
                prodO.ProductionStart = new DateTime(2017, 09, 3);
                prodO.ProductionEnd = new DateTime(2017, 09, 3);
                prodO.SpiritCutId = 11; // mixed
                prodO.Gauged = true;
                prodO.ProductionType = "Distillation";
                prodO.Quantity = 100f; // 100 gallons of alcohol
                prodO.VolumeByWeight = 0f;
                prodO.AlcoholContent = 80f; // 80%
                prodO.ProofGallon = 160f; // 160pfg
                prodO.Storage = storageList; // we are using the same storage id as we use for Purchase to keep things simple
                prodO.SpiritTypeReportingID = 6; // GNS
                prodO.ProductionTypeId = 2;

                List<ObjInfo4Burndwn> usedMats = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat = new ObjInfo4Burndwn();
                uMat.ID = purchaseId;
                uMat.OldVal = 0f;
                uMat.NewVal = purchO.Quantity;
                uMat.DistillableOrigin = "pur";
                uMat.BurningDownMethod = "volume";

                usedMats.Add(uMat);

                prodO.UsedMats = usedMats;

                productionId = _dl.CreateProduction(prodO, _userId);
                tablesForCleanupTupleList.Add(Tuple.Create(productionId, Table.Production));

                // create Production Blending Record
                ProductionObject prodBlend = new ProductionObject();
                prodBlend.BatchName = "GIN Blending Test";
                prodBlend.ProductionDate = new DateTime(2017, 09, 5);
                prodBlend.ProductionStart = new DateTime(2017, 09, 5);
                prodBlend.ProductionEnd = new DateTime(2017, 09, 5);
                prodBlend.Gauged = true;
                prodBlend.ProductionType = "Blending";
                prodBlend.Quantity = 173.9f; // 173.9 gallons of alcohol
                prodBlend.VolumeByWeight = 0f;
                prodBlend.AlcoholContent = 46f; // 46%
                prodBlend.ProofGallon = 160f; // 160pfg
                prodBlend.Storage = storageList; // we are using the same storage id as we use for Purchase to keep things simple
                prodBlend.SpiritTypeReportingID = 6; // GIN
                prodBlend.SpiritId = spiritId;
                prodBlend.ProductionTypeId = 3;

                List<ObjInfo4Burndwn> usedMats4Blend = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat4Blend = new ObjInfo4Burndwn();
                uMat4Blend.ID = productionId;
                uMat4Blend.OldVal = 0f;
                uMat4Blend.NewVal = prodO.Quantity;
                uMat4Blend.DistillableOrigin = "prod";
                uMat4Blend.BurningDownMethod = "volume";

                usedMats4Blend.Add(uMat4Blend);
                prodBlend.UsedMats = usedMats4Blend;

                List<BlendingAdditive> blendAdditives = new List<BlendingAdditive>();
                BlendingAdditive blendAd = new BlendingAdditive();
                blendAd.RawMaterialId = waterMaterialId;
                blendAd.RawMaterialQuantity = 50f;
                blendAd.RawMaterialName = "Water";
                blendAd.UnitOfMeasurement = "gal";

                blendAdditives.Add(blendAd);

                prodBlend.BlendingAdditives = blendAdditives;

                productionId = _dl.CreateProduction(prodBlend, _userId); // here productionId is overriden with a new productionId of the new Gauged record
                tablesForCleanupTupleList.Add(Tuple.Create(productionId, Table.Production));

                // create Production Bottling Record
                ProductionObject prodBottl = new ProductionObject();
                prodBottl.BatchName = "GIN Bottling Test ";
                prodBottl.ProductionDate = new DateTime(2017, 09, 6);
                prodBottl.ProductionStart = new DateTime(2017, 09, 6);
                prodBottl.ProductionEnd = new DateTime(2017, 09, 6);
                prodBottl.Gauged = true;
                prodBottl.ProductionType = "Bottling";
                prodBottl.Quantity = 149.92f; // 150 gallons of alcohol
                prodBottl.VolumeByWeight = 0f;
                prodBottl.AlcoholContent = 45f; // 45%
                prodBottl.ProofGallon = 159.92f; // 159.92 pfg
                prodBottl.Storage = storageList; // we are using the same storage id as we use for Purchase to keep things simple
                prodBottl.SpiritTypeReportingID = 6; // Gin
                prodBottl.SpiritId = spiritId;
                prodO.ProductionTypeId = 4;
                // BUG?

                List<ObjInfo4Burndwn> usedMats4Bottl = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat4Bottl = new ObjInfo4Burndwn();
                uMat4Bottl.ID = productionId;
                uMat4Bottl.OldVal = 0f;
                uMat4Bottl.NewVal = prodBlend.Quantity;
                uMat4Bottl.DistillableOrigin = "prod";
                uMat4Bottl.BurningDownMethod = "volume";

                usedMats4Bottl.Add(uMat4Bottl);
                prodBottl.UsedMats = usedMats4Bottl;

                BottlingObject bottlingObj = new BottlingObject();
                bottlingObj.CaseCapacity = 10;
                bottlingObj.CaseQuantity = 113.5f;
                bottlingObj.BottleCapacity = 500f;
                bottlingObj.BottleQuantity = 1135;

                prodBottl.BottlingInfo = bottlingObj;

                prodBottl.GainLoss = .08f;

                prodBottl.FillTestList = null;

                productionId = _dl.CreateProduction(prodBottl, _userId); // here productionId is overriden with a new productionId of the new Gauged record
                tablesForCleanupTupleList.Add(Tuple.Create(productionId, Table.Production));

                #endregion

                #region Reports setup

                // Report Header
                ReportHeader reportHeaderE = new ReportHeader();
                reportHeaderE.ProprietorName = "Test Distillery";
                reportHeaderE.EIN = "12-3456789";
                reportHeaderE.ReportDate = "September 2017";
                reportHeaderE.PlantAddress = "123 Cognac Drive Renton WASHINGTON 98059";
                reportHeaderE.DSP = "DSP-WA-21086";

                // reporting time range
                DateTime start = new DateTime(2017, 09, 01);
                DateTime end = new DateTime(2017, 09, 30);

                #region Produciton
                /* PRODUCTION REPORT */
                ProdReportPart1 part1E = new ProdReportPart1();
                part1E.ProccessingAcct = 160f;
                part1E.ProducedTotal = 160f;
                part1E.Recd4RedistilL17 = 0f;
                part1E.Recd4RedistilaltionL15 = 180f;
                part1E.StorageAcct = 0f;
                part1E.SpiritCatName = "Gin";
                part1E.SpiritTypeReportingID = 6;
                part1E.UnfinishedSpiritsEndOfQuarterL17 = 0f;

                ProdReportParts2Through4 part2thru4 = new ProdReportParts2Through4();
                part2thru4.KindOfMaterial = "";
                part2thru4.MaterialKindReportingID = 0;
                part2thru4.NewCoop = false;
                part2thru4.UsedCoop = false;
                part2thru4.ProofGallons = 0f;
                part2thru4.SpiritTypeReportingID = 0;
                part2thru4.Tanks = false;

                ProdReportPart6 part6 = new ProdReportPart6();
                part6.KindOfMaterial = "";
                part6.ProdReportMaterialCategoryID = 0;
                part6.Volume = 0f;
                part6.Weight = 0f;

                ProdReportPart5 part5 = new ProdReportPart5();
                part5.KindofSpirits = "AlcoholUnder190";
                part5.Proof = 180f;

                ProductionReportingObject actualProdReportObject = new ProductionReportingObject();


                actualProdReportObject = _dl.GetProductionReportData(start, end, _userId);
                #endregion

                #region Storage
                /* STORAGE REPORT */
                StorageReport actualStorageReportObject = new StorageReport();

                StorageReportCategory storageReportBody = new StorageReportCategory();
                storageReportBody.CategoryName = "AlcoholUnder190";
                storageReportBody.r17_TransferredToProcessingAccount = 0f;
                storageReportBody.r18_TransferredToProductionAccount = 180f;
                storageReportBody.r19_TransferredToOtherBondedPremises = 0;
                storageReportBody.r1_OnHandFirstOfMonth = 0f;
                storageReportBody.r20_Destroyed = 0f;
                storageReportBody.r22_OtherLosses = 0f;
                storageReportBody.r23_OnHandEndOfMonth = 0f;
                storageReportBody.r24_Lines7Through23 = 180f;
                storageReportBody.r2_DepositedInBulkStorage = 180f;
                storageReportBody.r4_ReturnedToBulkStorage = 0f;
                storageReportBody.r6_TotalLines1Through5 = 180f;
                storageReportBody.r7_TaxPaid = 0f;

                actualStorageReportObject = _dl.GetStorageReportData(start, end, _userId);
                #endregion

                #region Processing
                /* PROCESING REPORT */
                ProcessingReportingObject actualProcessingReportObject = new ProcessingReportingObject();

                ProcessReportingPart1 processingReportP1 = new ProcessReportingPart1();
                processingReportP1.AmtBottledPackaged = 159.92f;
                processingReportP1.BulkIngredients = "spirit";
                processingReportP1.Destroyed = 0f;
                processingReportP1.Dumped4Processing = 0f;
                processingReportP1.Gains = 0f;
                processingReportP1.Losses = 0.08f;
                processingReportP1.OnHandEndofMonth = 0f;
                processingReportP1.OnHandFirstofMonth = 0f;
                processingReportP1.Recd4Process = 160f;
                processingReportP1.Transf2Prod4Redistil = 0f;
                processingReportP1.Used4Redistil = 0f;
                processingReportP1.WineMixedWithSpirit = 0f;

                ProcessReportingPart2 processingReportP2 = new ProcessReportingPart2();
                processingReportP2.AmtBottledPackaged = 159.92f;
                processingReportP2.Destroyed = 0f;
                processingReportP2.Dumped4Processing = 0f;
                processingReportP2.FinishedProduct = "bottled";
                processingReportP2.InventoryOverage = 0f;
                processingReportP2.InventoryShortage = 0f;
                processingReportP2.OnHandEndofMonth = 159.92f;
                processingReportP2.OnHandFirstofMonth = 0f;
                processingReportP2.Recd4Process = 0f;
                processingReportP2.RecordedLosses = 0f;
                processingReportP2.TaxWithdrawn = 0f;
                processingReportP2.Transf2Prod4Redistil = 0f;

                ProcessReportingPart4 processingReportP4 = new ProcessReportingPart4();
                processingReportP4.AlcoholNeutral = 0f;
                processingReportP4.BlendedLightWhiskey = 0f;
                processingReportP4.BlendedOtherWhiskey = 0f;
                processingReportP4.BlendedStraightWhiskey = 0f;
                processingReportP4.BlendedWhiskeyWithLight = 0f;
                processingReportP4.BlendedWhiskeyWithNeutral = 0f;
                processingReportP4.Brandy170Under = 0f;
                processingReportP4.BrandyOver170 = 0f;
                processingReportP4.Cocktail = 0f;
                processingReportP4.DomesticWhiskey160Under = 0f;
                processingReportP4.DomesticWhiskeyOver160 = 0f;
                processingReportP4.Gin = 149.92f;
                processingReportP4.ImportedWhiskeyCanadian = 0f;
                processingReportP4.ImportedWhiskeyIrish = 0f;
                processingReportP4.ImportedWhiskeyScotch = 0f;
                processingReportP4.Liqueur = 0f;
                processingReportP4.ProcessingReportTypeName = "GIN";
                processingReportP4.ProcessingSpirits = "bottled";
                processingReportP4.ProcessingTypeID = 18;
                processingReportP4.RumDomestic = 0f;
                processingReportP4.RumOtherImported = 0f;
                processingReportP4.RumPuertoRican = 0f;
                processingReportP4.RumVirginIslands = 0f;
                processingReportP4.StateID = 5;
                processingReportP4.Tequila = 0f;
                processingReportP4.Vodka = 0f;

                actualProcessingReportObject = _dl.GetProcessingReportData(start, end, _userId);
                #endregion // end of processing region

                #endregion

                // Assert

                #region Production Report

                // verify Production report header
                Assert.AreEqual(reportHeaderE.DSP, actualProdReportObject.Header.DSP);
                Assert.AreEqual(reportHeaderE.EIN, actualProdReportObject.Header.EIN);
                Assert.AreEqual(reportHeaderE.PlantAddress, actualProdReportObject.Header.PlantAddress);
                Assert.AreEqual(reportHeaderE.ProprietorName, actualProdReportObject.Header.ProprietorName);
                Assert.AreEqual(reportHeaderE.ReportDate, actualProdReportObject.Header.ReportDate);

                // verify Production report Part 1
                Assert.AreEqual(part1E.ProccessingAcct, actualProdReportObject.Part1List[0].ProccessingAcct);
                Assert.AreEqual(part1E.ProducedTotal, actualProdReportObject.Part1List[0].ProducedTotal);
                Assert.AreEqual(part1E.Recd4RedistilL17, actualProdReportObject.Part1List[0].Recd4RedistilL17);


                Assert.AreEqual(part1E.Recd4RedistilaltionL15, actualProdReportObject.Part1List[1].Recd4RedistilaltionL15);
                Assert.AreEqual(part1E.StorageAcct, actualProdReportObject.Part1List[0].StorageAcct);
                Assert.AreEqual(part1E.SpiritCatName, actualProdReportObject.Part1List[0].SpiritCatName);
                Assert.AreEqual(part1E.SpiritTypeReportingID, actualProdReportObject.Part1List[0].SpiritTypeReportingID);
                Assert.AreEqual(part1E.UnfinishedSpiritsEndOfQuarterL17, actualProdReportObject.Part1List[0].UnfinishedSpiritsEndOfQuarterL17);

                // verify Production report Part 2 trough 4
                Assert.AreEqual(0, actualProdReportObject.Part2Through4List.Count);

                // verify part 5
                Assert.AreEqual(part5.KindofSpirits, actualProdReportObject.part5List[0].KindofSpirits);
                Assert.AreEqual(part5.Proof, actualProdReportObject.part5List[0].Proof);

                // verify Production report Part 6
                Assert.AreEqual(0, actualProdReportObject.ProdReportPart6List.Count);
                //Assert.AreEqual(part6.KindOfMaterial, actualProdReportObject.ProdReportPart6List[0].KindOfMaterial);
                //Assert.AreEqual(part6.ProdReportMaterialCategoryID, actualProdReportObject.ProdReportPart6List[0].ProdReportMaterialCategoryID);
                //Assert.AreEqual(part6.Volume, actualProdReportObject.ProdReportPart6List[0].Volume);
                //Assert.AreEqual(part6.Weight, actualProdReportObject.ProdReportPart6List[0].Weight);
                #endregion

                #region Storage Report
                // verify Storage report Header
                Assert.AreEqual(reportHeaderE.DSP, actualStorageReportObject.Header.DSP);
                Assert.AreEqual(reportHeaderE.EIN, actualStorageReportObject.Header.EIN);
                Assert.AreEqual(reportHeaderE.PlantAddress, actualStorageReportObject.Header.PlantAddress);
                Assert.AreEqual(reportHeaderE.ProprietorName, actualStorageReportObject.Header.ProprietorName);
                Assert.AreEqual(reportHeaderE.ReportDate, actualStorageReportObject.Header.ReportDate);

                // verify Storage body
                Assert.AreEqual(storageReportBody.CategoryName, actualStorageReportObject.ReportBody[0].CategoryName);
                Assert.AreEqual(storageReportBody.r17_TransferredToProcessingAccount, actualStorageReportObject.ReportBody[0].r17_TransferredToProcessingAccount);
                Assert.AreEqual(storageReportBody.r18_TransferredToProductionAccount, actualStorageReportObject.ReportBody[0].r18_TransferredToProductionAccount);
                Assert.AreEqual(storageReportBody.r19_TransferredToOtherBondedPremises, actualStorageReportObject.ReportBody[0].r19_TransferredToOtherBondedPremises);
                Assert.AreEqual(storageReportBody.r1_OnHandFirstOfMonth, actualStorageReportObject.ReportBody[0].r1_OnHandFirstOfMonth);
                Assert.AreEqual(storageReportBody.r20_Destroyed, actualStorageReportObject.ReportBody[0].r20_Destroyed);
                Assert.AreEqual(storageReportBody.r22_OtherLosses, actualStorageReportObject.ReportBody[0].r22_OtherLosses);
                Assert.AreEqual(storageReportBody.r23_OnHandEndOfMonth, actualStorageReportObject.ReportBody[0].r23_OnHandEndOfMonth);
                Assert.AreEqual(storageReportBody.r24_Lines7Through23, actualStorageReportObject.ReportBody[0].r24_Lines7Through23);
                Assert.AreEqual(storageReportBody.r2_DepositedInBulkStorage, actualStorageReportObject.ReportBody[0].r2_DepositedInBulkStorage);
                Assert.AreEqual(storageReportBody.r4_ReturnedToBulkStorage, actualStorageReportObject.ReportBody[0].r4_ReturnedToBulkStorage);
                Assert.AreEqual(storageReportBody.r6_TotalLines1Through5, actualStorageReportObject.ReportBody[0].r6_TotalLines1Through5);
                Assert.AreEqual(storageReportBody.r7_TaxPaid, actualStorageReportObject.ReportBody[0].r7_TaxPaid);

                #endregion

                #region Processing Report
                Assert.AreEqual(reportHeaderE.DSP, actualProcessingReportObject.Header.DSP);
                Assert.AreEqual(reportHeaderE.EIN, actualProcessingReportObject.Header.EIN);
                Assert.AreEqual(reportHeaderE.PlantAddress, actualProcessingReportObject.Header.PlantAddress);
                Assert.AreEqual(reportHeaderE.ProprietorName, actualProcessingReportObject.Header.ProprietorName);
                Assert.AreEqual(reportHeaderE.ReportDate, actualProcessingReportObject.Header.ReportDate);

                Assert.AreEqual(processingReportP1.AmtBottledPackaged, actualProcessingReportObject.Part1.AmtBottledPackaged);
                Assert.AreEqual(processingReportP1.BulkIngredients, actualProcessingReportObject.Part1.BulkIngredients);
                Assert.AreEqual(processingReportP1.Destroyed, actualProcessingReportObject.Part1.Destroyed);
                Assert.AreEqual(processingReportP1.Dumped4Processing, actualProcessingReportObject.Part1.Dumped4Processing);
                Assert.AreEqual(processingReportP1.Gains, actualProcessingReportObject.Part1.Gains);
                Assert.AreEqual(processingReportP1.Losses, actualProcessingReportObject.Part1.Losses);
                Assert.AreEqual(processingReportP1.OnHandEndofMonth, actualProcessingReportObject.Part1.OnHandEndofMonth);
                Assert.AreEqual(processingReportP1.OnHandFirstofMonth, actualProcessingReportObject.Part1.OnHandFirstofMonth);
                Assert.AreEqual(processingReportP1.Recd4Process, actualProcessingReportObject.Part1.Recd4Process);
                Assert.AreEqual(processingReportP1.Transf2Prod4Redistil, actualProcessingReportObject.Part1.Transf2Prod4Redistil);
                Assert.AreEqual(processingReportP1.Used4Redistil, actualProcessingReportObject.Part1.Used4Redistil);
                Assert.AreEqual(processingReportP1.WineMixedWithSpirit, actualProcessingReportObject.Part1.WineMixedWithSpirit);

                Assert.AreEqual(processingReportP2.AmtBottledPackaged, actualProcessingReportObject.Part2.AmtBottledPackaged);
                Assert.AreEqual(processingReportP2.Destroyed, actualProcessingReportObject.Part2.Destroyed);
                Assert.AreEqual(processingReportP2.Dumped4Processing, actualProcessingReportObject.Part2.Dumped4Processing);
                Assert.AreEqual(processingReportP2.FinishedProduct, actualProcessingReportObject.Part2.FinishedProduct);
                Assert.AreEqual(processingReportP2.InventoryOverage, actualProcessingReportObject.Part2.InventoryOverage);
                Assert.AreEqual(processingReportP2.InventoryShortage, actualProcessingReportObject.Part2.InventoryShortage);
                Assert.AreEqual(processingReportP2.OnHandEndofMonth, actualProcessingReportObject.Part2.OnHandEndofMonth);
                Assert.AreEqual(processingReportP2.OnHandFirstofMonth, actualProcessingReportObject.Part2.OnHandFirstofMonth);
                Assert.AreEqual(processingReportP2.Recd4Process, actualProcessingReportObject.Part2.Recd4Process);
                Assert.AreEqual(processingReportP2.RecordedLosses, actualProcessingReportObject.Part2.RecordedLosses);
                Assert.AreEqual(processingReportP2.TaxWithdrawn, actualProcessingReportObject.Part2.TaxWithdrawn);
                Assert.AreEqual(processingReportP2.Transf2Prod4Redistil, actualProcessingReportObject.Part2.Transf2Prod4Redistil);

                Assert.AreEqual(processingReportP4.AlcoholNeutral, actualProcessingReportObject.Part4List[0].AlcoholNeutral);
                Assert.AreEqual(processingReportP4.BlendedLightWhiskey, actualProcessingReportObject.Part4List[0].BlendedLightWhiskey);
                Assert.AreEqual(processingReportP4.BlendedOtherWhiskey, actualProcessingReportObject.Part4List[0].BlendedOtherWhiskey);
                Assert.AreEqual(processingReportP4.BlendedStraightWhiskey, actualProcessingReportObject.Part4List[0].BlendedStraightWhiskey);
                Assert.AreEqual(processingReportP4.BlendedWhiskeyWithLight, actualProcessingReportObject.Part4List[0].BlendedWhiskeyWithLight);
                Assert.AreEqual(processingReportP4.BlendedWhiskeyWithNeutral, actualProcessingReportObject.Part4List[0].BlendedWhiskeyWithNeutral);
                Assert.AreEqual(processingReportP4.Brandy170Under, actualProcessingReportObject.Part4List[0].Brandy170Under);
                Assert.AreEqual(processingReportP4.BrandyOver170, actualProcessingReportObject.Part4List[0].BrandyOver170);
                Assert.AreEqual(processingReportP4.Cocktail, actualProcessingReportObject.Part4List[0].Cocktail);
                Assert.AreEqual(processingReportP4.DomesticWhiskey160Under, actualProcessingReportObject.Part4List[0].DomesticWhiskey160Under);
                Assert.AreEqual(processingReportP4.DomesticWhiskeyOver160, actualProcessingReportObject.Part4List[0].DomesticWhiskeyOver160);
                Assert.AreEqual(processingReportP4.Gin, actualProcessingReportObject.Part4List[1].Gin);
                Assert.AreEqual(processingReportP4.ImportedWhiskeyCanadian, actualProcessingReportObject.Part4List[0].ImportedWhiskeyCanadian);
                Assert.AreEqual(processingReportP4.ImportedWhiskeyIrish, actualProcessingReportObject.Part4List[0].ImportedWhiskeyIrish);
                Assert.AreEqual(processingReportP4.ImportedWhiskeyScotch, actualProcessingReportObject.Part4List[0].ImportedWhiskeyScotch);
                Assert.AreEqual(processingReportP4.Liqueur, actualProcessingReportObject.Part4List[0].Liqueur);
                Assert.AreEqual(processingReportP4.ProcessingReportTypeName, actualProcessingReportObject.Part4List[0].ProcessingReportTypeName);
                Assert.AreEqual(processingReportP4.ProcessingSpirits, actualProcessingReportObject.Part4List[1].ProcessingSpirits);
                Assert.AreEqual(processingReportP4.ProcessingTypeID, actualProcessingReportObject.Part4List[0].ProcessingTypeID);
                Assert.AreEqual(processingReportP4.RumDomestic, actualProcessingReportObject.Part4List[0].RumDomestic);
                Assert.AreEqual(processingReportP4.RumDomestic, actualProcessingReportObject.Part4List[0].RumDomestic);
                Assert.AreEqual(processingReportP4.RumOtherImported, actualProcessingReportObject.Part4List[0].RumOtherImported);
                Assert.AreEqual(processingReportP4.RumPuertoRican, actualProcessingReportObject.Part4List[0].RumPuertoRican);
                Assert.AreEqual(processingReportP4.StateID, actualProcessingReportObject.Part4List[1].StateID);
                Assert.AreEqual(processingReportP4.Tequila, actualProcessingReportObject.Part4List[0].Tequila);
                Assert.AreEqual(processingReportP4.Vodka, actualProcessingReportObject.Part4List[0].Vodka);

                #endregion
            }
            finally
            {
                // Cleanup
                foreach (var i in tablesForCleanupTupleList)
                {
                    TestRecordCleanup(i.Item1, i.Item2);
                }
            }
        }

        /// <summary>
        /// This test tests workflow: Buy GNS -> Redistil
        /// this test verifies that line 11 of Production Report is populated
        /// </summary>
        [TestMethod()]
        public void BuyGNS_RedistilOnce_Make_Gin_And_Dont_Blend_Or_Bottle()
        {
            // Arrange
            List<Tuple<int/*recordId*/, Table/*table enum vaue*/>> tablesForCleanupTupleList = new List<Tuple<int, Table>>();
            int spiritId = 0;
            int vendorId = 0;
            int storageId = 0;
            int gnsMaterialId = 0;
            int waterMaterialId = 0;
            int purchaseId = 0;
            int productionId = 0;

            try
            {

                #region Dictionary
                //  dictionary setup
                SpiritObject spirit = new SpiritObject();
                spirit.SpiritName = "GIN";
                spirit.ProcessingReportTypeID = 18;

                spiritId = _dl.CreateSpirit(_userId, spirit);

                // setup Vendor object
                VendorObject vendor = new VendorObject();
                vendor.VendorName = "testVendor";

                vendorId = _dl.CreateVendor(_userId, vendor);
                tablesForCleanupTupleList.Add(Tuple.Create(vendorId, Table.Vendor));

                // setup Storage Object
                StorageObject storage = new StorageObject();
                storage.StorageName = "testStorage";
                storage.SerialNumber = "2H29NNS";

                storageId = _dl.CreateStorage(_userId, storage);
                tablesForCleanupTupleList.Add(Tuple.Create(storageId, Table.Storage));

                // setup Material Object
                // GNS
                {
                    RawMaterialObject gnsMaterial = new RawMaterialObject();
                    gnsMaterial.RawMaterialName = "GNS for GIN";
                    gnsMaterial.UnitType = "gal";
                    gnsMaterial.UnitTypeId = 1;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Distilled = true;
                    gnsMaterial.PurchaseMaterialTypes = materialBoolTypes;

                    gnsMaterialId = _dl.CreateRawMaterial(_userId, gnsMaterial);
                    tablesForCleanupTupleList.Add(Tuple.Create(gnsMaterialId, Table.MaterialDict));
                }

                // water
                {
                    RawMaterialObject waterMaterial = new RawMaterialObject();
                    waterMaterial.RawMaterialName = "Water";
                    waterMaterial.UnitType = "gal";
                    waterMaterial.UnitTypeId = 1;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Additive = true;
                    waterMaterial.PurchaseMaterialTypes = materialBoolTypes;

                    waterMaterialId = _dl.CreateRawMaterial(_userId, waterMaterial);
                    tablesForCleanupTupleList.Add(Tuple.Create(waterMaterialId, Table.MaterialDict));
                }
                #endregion

                #region Purchase
                // create Purchase Record (minimal required fields)
                PurchaseObject purchO = new PurchaseObject();
                purchO.PurBatchName = "GNS Purchase Test";
                purchO.PurchaseType = "Distilled";
                purchO.PurchaseDate = new DateTime(2017, 09, 1);
                purchO.Quantity = 100f; // 100 gallons
                purchO.VolumeByWeight = 0f;
                purchO.AlcoholContent = 90f;
                purchO.ProofGallon = 180f;
                purchO.RecordId = gnsMaterialId;
                purchO.Price = 350f;
                purchO.VendorId = vendorId;
                purchO.Gauged = true;

                List<StorageObject> storageList = new List<StorageObject>();
                StorageObject storageObject = new StorageObject();
                storageObject.StorageId = storageId;
                storageList.Add(storageObject);
                purchO.Storage = storageList;

                purchO.SpiritTypeReportingID = 8;
                purchO.Gauged = true;

                purchaseId = _dl.CreatePurchase(purchO, _userId);
                tablesForCleanupTupleList.Add(Tuple.Create(purchaseId, Table.Purchase));
                #endregion

                #region Production
                // Redistil GNS into GIN and mark it as Gauged
                ProductionObject prodO = new ProductionObject();
                prodO.BatchName = "GNS to Gin";
                prodO.ProductionDate = new DateTime(2017, 09, 3);
                prodO.ProductionStart = new DateTime(2017, 09, 3);
                prodO.ProductionEnd = new DateTime(2017, 09, 3);
                prodO.SpiritCutId = 11; // mixed
                prodO.Gauged = true;
                prodO.ProductionType = "Distillation";
                prodO.Quantity = 100f; // 100 gallons of alcohol
                prodO.VolumeByWeight = 0f;
                prodO.AlcoholContent = 80f; // 80%
                prodO.ProofGallon = 160f; // 160pfg
                prodO.Storage = storageList; // we are using the same storage id as we use for Purchase to keep things simple
                prodO.SpiritTypeReportingID = 6; // GNS
                prodO.ProductionTypeId = 2;

                List<ObjInfo4Burndwn> usedMats = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat = new ObjInfo4Burndwn();
                uMat.ID = purchaseId;
                uMat.OldVal = 0f;
                uMat.NewVal = purchO.Quantity;
                uMat.DistillableOrigin = "pur";
                uMat.BurningDownMethod = "volume";

                usedMats.Add(uMat);

                prodO.UsedMats = usedMats;

                productionId = _dl.CreateProduction(prodO, _userId);
                tablesForCleanupTupleList.Add(Tuple.Create(productionId, Table.Production));
                #endregion

                #region Reports setup

                // Report Header
                ReportHeader reportHeaderE = new ReportHeader();
                reportHeaderE.ProprietorName = "Test Distillery";
                reportHeaderE.EIN = "12-3456789";
                reportHeaderE.ReportDate = "September 2017";
                reportHeaderE.PlantAddress = "123 Cognac Drive Renton WASHINGTON 98059";
                reportHeaderE.DSP = "DSP-WA-21086";

                // reporting time range
                DateTime start = new DateTime(2017, 09, 01);
                DateTime end = new DateTime(2017, 09, 30);

                #region Produciton
                /* PRODUCTION REPORT */
                ProdReportPart1 part1E = new ProdReportPart1();
                part1E.ProccessingAcct = 0f;
                part1E.ProducedTotal = 160f;
                part1E.Recd4RedistilL17 = 0f;
                part1E.Recd4RedistilaltionL15 = 180f;
                part1E.StorageAcct = 160f;
                part1E.SpiritCatName = "Gin";
                part1E.SpiritTypeReportingID = 6;
                part1E.UnfinishedSpiritsEndOfQuarterL17 = 0f;

                ProdReportParts2Through4 part2thru4 = new ProdReportParts2Through4();
                part2thru4.KindOfMaterial = "";
                part2thru4.MaterialKindReportingID = 0;
                part2thru4.NewCoop = false;
                part2thru4.UsedCoop = false;
                part2thru4.ProofGallons = 0f;
                part2thru4.SpiritTypeReportingID = 0;
                part2thru4.Tanks = false;

                ProdReportPart6 part6 = new ProdReportPart6();
                part6.KindOfMaterial = "";
                part6.ProdReportMaterialCategoryID = 0;
                part6.Volume = 0f;
                part6.Weight = 0f;

                ProdReportPart5 part5 = new ProdReportPart5();
                part5.KindofSpirits = "AlcoholUnder190";
                part5.Proof = 180f;

                ProductionReportingObject actualProdReportObject = new ProductionReportingObject();


                actualProdReportObject = _dl.GetProductionReportData(start, end, _userId);
                #endregion

                #region Storage
                /* STORAGE REPORT */
                StorageReport actualStorageReportObject = new StorageReport();

                StorageReportCategory storageReportBody = new StorageReportCategory();
                storageReportBody.CategoryName = "AlcoholUnder190";
                storageReportBody.r17_TransferredToProcessingAccount = 0f;
                storageReportBody.r18_TransferredToProductionAccount = 180f;
                storageReportBody.r19_TransferredToOtherBondedPremises = 0;
                storageReportBody.r1_OnHandFirstOfMonth = 0f;
                storageReportBody.r20_Destroyed = 0f;
                storageReportBody.r22_OtherLosses = 0f;
                storageReportBody.r23_OnHandEndOfMonth = 160f;
                storageReportBody.r24_Lines7Through23 = 160f;
                storageReportBody.r2_DepositedInBulkStorage = 180f;
                storageReportBody.r4_ReturnedToBulkStorage = 0f;
                storageReportBody.r6_TotalLines1Through5 = 160f;
                storageReportBody.r7_TaxPaid = 0f;

                actualStorageReportObject = _dl.GetStorageReportData(start, end, _userId);
                #endregion

                #region Processing
                /* PROCESING REPORT */
                ProcessingReportingObject actualProcessingReportObject = new ProcessingReportingObject();

                ProcessReportingPart1 processingReportP1 = new ProcessReportingPart1();
                processingReportP1.AmtBottledPackaged = 0f;
                processingReportP1.BulkIngredients = "spirit";
                processingReportP1.Destroyed = 0f;
                processingReportP1.Dumped4Processing = 0f;
                processingReportP1.Gains = 0f;
                processingReportP1.Losses = 0f;
                processingReportP1.OnHandEndofMonth = 0f;
                processingReportP1.OnHandFirstofMonth = 0f;
                processingReportP1.Recd4Process = 0f;
                processingReportP1.Transf2Prod4Redistil = 0f;
                processingReportP1.Used4Redistil = 0f;
                processingReportP1.WineMixedWithSpirit = 0f;

                ProcessReportingPart2 processingReportP2 = new ProcessReportingPart2();
                processingReportP2.AmtBottledPackaged = 0f;
                processingReportP2.Destroyed = 0f;
                processingReportP2.Dumped4Processing = 0f;
                processingReportP2.FinishedProduct = "bottled";
                processingReportP2.InventoryOverage = 0f;
                processingReportP2.InventoryShortage = 0f;
                processingReportP2.OnHandEndofMonth = 0f;
                processingReportP2.OnHandFirstofMonth = 0f;
                processingReportP2.Recd4Process = 0f;
                processingReportP2.RecordedLosses = 0f;
                processingReportP2.TaxWithdrawn = 0f;
                processingReportP2.Transf2Prod4Redistil = 0f;

                actualProcessingReportObject = _dl.GetProcessingReportData(start, end, _userId);
                #endregion // end of processing region

                #endregion

                // Assert

                #region Production Report
                // verify Production report header
                Assert.AreEqual(reportHeaderE.DSP, actualProdReportObject.Header.DSP);
                Assert.AreEqual(reportHeaderE.EIN, actualProdReportObject.Header.EIN);
                Assert.AreEqual(reportHeaderE.PlantAddress, actualProdReportObject.Header.PlantAddress);
                Assert.AreEqual(reportHeaderE.ProprietorName, actualProdReportObject.Header.ProprietorName);
                Assert.AreEqual(reportHeaderE.ReportDate, actualProdReportObject.Header.ReportDate);

                // verify Production report Part 1
                Assert.AreEqual(part1E.ProccessingAcct, actualProdReportObject.Part1List[0].ProccessingAcct);
                Assert.AreEqual(part1E.ProducedTotal, actualProdReportObject.Part1List[0].ProducedTotal);
                Assert.AreEqual(part1E.Recd4RedistilL17, actualProdReportObject.Part1List[0].Recd4RedistilL17);
                Assert.AreEqual(part1E.Recd4RedistilaltionL15, actualProdReportObject.Part1List[1].Recd4RedistilaltionL15);
                Assert.AreEqual(part1E.StorageAcct, actualProdReportObject.Part1List[0].StorageAcct);
                Assert.AreEqual(part1E.SpiritCatName, actualProdReportObject.Part1List[0].SpiritCatName);
                Assert.AreEqual(part1E.SpiritTypeReportingID, actualProdReportObject.Part1List[0].SpiritTypeReportingID);
                Assert.AreEqual(part1E.UnfinishedSpiritsEndOfQuarterL17, actualProdReportObject.Part1List[0].UnfinishedSpiritsEndOfQuarterL17);

                // verify Production report Part 2 trough 4
                Assert.AreEqual(0, actualProdReportObject.Part2Through4List.Count);

                // verify part 5
                Assert.AreEqual(part5.KindofSpirits, actualProdReportObject.part5List[0].KindofSpirits);
                Assert.AreEqual(part5.Proof, actualProdReportObject.part5List[0].Proof);

                // verify Production report Part 6
                Assert.AreEqual(0, actualProdReportObject.ProdReportPart6List.Count);
                #endregion

                #region Storage Report
                // verify Storage report Header
                Assert.AreEqual(reportHeaderE.DSP, actualStorageReportObject.Header.DSP);
                Assert.AreEqual(reportHeaderE.EIN, actualStorageReportObject.Header.EIN);
                Assert.AreEqual(reportHeaderE.PlantAddress, actualStorageReportObject.Header.PlantAddress);
                Assert.AreEqual(reportHeaderE.ProprietorName, actualStorageReportObject.Header.ProprietorName);
                Assert.AreEqual(reportHeaderE.ReportDate, actualStorageReportObject.Header.ReportDate);

                // verify Storage body
                Assert.AreEqual("Gin", actualStorageReportObject.ReportBody[0].CategoryName); // verifying receipt of Gin from Production that didn't go into processing report and instead, stayed in Storage report
                Assert.AreEqual(storageReportBody.CategoryName, actualStorageReportObject.ReportBody[1].CategoryName); // verifying receival of Alcohol Over 190 from Purchase
                Assert.AreEqual(storageReportBody.r17_TransferredToProcessingAccount, actualStorageReportObject.ReportBody[1].r17_TransferredToProcessingAccount);
                Assert.AreEqual(storageReportBody.r18_TransferredToProductionAccount, actualStorageReportObject.ReportBody[1].r18_TransferredToProductionAccount);
                Assert.AreEqual(storageReportBody.r19_TransferredToOtherBondedPremises, actualStorageReportObject.ReportBody[1].r19_TransferredToOtherBondedPremises);
                Assert.AreEqual(storageReportBody.r1_OnHandFirstOfMonth, actualStorageReportObject.ReportBody[1].r1_OnHandFirstOfMonth);
                Assert.AreEqual(storageReportBody.r20_Destroyed, actualStorageReportObject.ReportBody[1].r20_Destroyed);
                Assert.AreEqual(storageReportBody.r22_OtherLosses, actualStorageReportObject.ReportBody[1].r22_OtherLosses);
                Assert.AreEqual(storageReportBody.r23_OnHandEndOfMonth, actualStorageReportObject.ReportBody[0].r23_OnHandEndOfMonth); // we should have 160 Proof of Gin on hand at the end of the month
                Assert.AreEqual(storageReportBody.r24_Lines7Through23, actualStorageReportObject.ReportBody[0].r24_Lines7Through23);
                Assert.AreEqual(160, actualStorageReportObject.ReportBody[0].r2_DepositedInBulkStorage); // verifying receipt of Gin
                Assert.AreEqual(storageReportBody.r2_DepositedInBulkStorage, actualStorageReportObject.ReportBody[1].r2_DepositedInBulkStorage); // verifying receipt of Alcohol 190 and over
                Assert.AreEqual(storageReportBody.r4_ReturnedToBulkStorage, actualStorageReportObject.ReportBody[1].r4_ReturnedToBulkStorage);
                Assert.AreEqual(storageReportBody.r6_TotalLines1Through5, actualStorageReportObject.ReportBody[0].r6_TotalLines1Through5);
                Assert.AreEqual(storageReportBody.r7_TaxPaid, actualStorageReportObject.ReportBody[0].r7_TaxPaid);

                #endregion

                #region Processing Report
                Assert.AreEqual(reportHeaderE.DSP, actualProcessingReportObject.Header.DSP);
                Assert.AreEqual(reportHeaderE.EIN, actualProcessingReportObject.Header.EIN);
                Assert.AreEqual(reportHeaderE.PlantAddress, actualProcessingReportObject.Header.PlantAddress);
                Assert.AreEqual(reportHeaderE.ProprietorName, actualProcessingReportObject.Header.ProprietorName);
                Assert.AreEqual(reportHeaderE.ReportDate, actualProcessingReportObject.Header.ReportDate);

                Assert.AreEqual(processingReportP1.AmtBottledPackaged, actualProcessingReportObject.Part1.AmtBottledPackaged);
                Assert.AreEqual(processingReportP1.BulkIngredients, actualProcessingReportObject.Part1.BulkIngredients);
                Assert.AreEqual(processingReportP1.Destroyed, actualProcessingReportObject.Part1.Destroyed);
                Assert.AreEqual(processingReportP1.Dumped4Processing, actualProcessingReportObject.Part1.Dumped4Processing);
                Assert.AreEqual(processingReportP1.Gains, actualProcessingReportObject.Part1.Gains);
                Assert.AreEqual(processingReportP1.Losses, actualProcessingReportObject.Part1.Losses);
                Assert.AreEqual(processingReportP1.OnHandEndofMonth, actualProcessingReportObject.Part1.OnHandEndofMonth);
                Assert.AreEqual(processingReportP1.OnHandFirstofMonth, actualProcessingReportObject.Part1.OnHandFirstofMonth);
                Assert.AreEqual(processingReportP1.Recd4Process, actualProcessingReportObject.Part1.Recd4Process);
                Assert.AreEqual(processingReportP1.Transf2Prod4Redistil, actualProcessingReportObject.Part1.Transf2Prod4Redistil);
                Assert.AreEqual(processingReportP1.Used4Redistil, actualProcessingReportObject.Part1.Used4Redistil);
                Assert.AreEqual(processingReportP1.WineMixedWithSpirit, actualProcessingReportObject.Part1.WineMixedWithSpirit);

                Assert.AreEqual(processingReportP2.AmtBottledPackaged, actualProcessingReportObject.Part2.AmtBottledPackaged);
                Assert.AreEqual(processingReportP2.Destroyed, actualProcessingReportObject.Part2.Destroyed);
                Assert.AreEqual(processingReportP2.Dumped4Processing, actualProcessingReportObject.Part2.Dumped4Processing);
                Assert.AreEqual(processingReportP2.FinishedProduct, actualProcessingReportObject.Part2.FinishedProduct);
                Assert.AreEqual(processingReportP2.InventoryOverage, actualProcessingReportObject.Part2.InventoryOverage);
                Assert.AreEqual(processingReportP2.InventoryShortage, actualProcessingReportObject.Part2.InventoryShortage);
                Assert.AreEqual(processingReportP2.OnHandEndofMonth, actualProcessingReportObject.Part2.OnHandEndofMonth);
                Assert.AreEqual(processingReportP2.OnHandFirstofMonth, actualProcessingReportObject.Part2.OnHandFirstofMonth);
                Assert.AreEqual(processingReportP2.Recd4Process, actualProcessingReportObject.Part2.Recd4Process);
                Assert.AreEqual(processingReportP2.RecordedLosses, actualProcessingReportObject.Part2.RecordedLosses);
                Assert.AreEqual(processingReportP2.TaxWithdrawn, actualProcessingReportObject.Part2.TaxWithdrawn);
                Assert.AreEqual(processingReportP2.Transf2Prod4Redistil, actualProcessingReportObject.Part2.Transf2Prod4Redistil);

                Assert.AreEqual(0, actualProcessingReportObject.Part4List.Count);

                #endregion
            }
            finally
            {
                // Cleanup
                foreach (var i in tablesForCleanupTupleList)
                {
                    TestRecordCleanup(i.Item1, i.Item2);
                }
            }
        }

        /// <summary>
        /// This test checks correct reporting of values in Received for
        /// Redisitllation in Production Report when the same Gin redistilled again in
        /// the next month.
        /// Workflow:
        /// 1.Buy GNS in February
        /// 2.Distil Gin in February
        /// 3.Redistil Gin from #2 in March
        /// 4.Check production report for March.
        /// Received For Redistillation field should not double the values
        /// </summary>
        [TestMethod()]
        public void BuyGNS_Distil_Gin_Twice_In_Different_Months_Check_Production_Report()
        {
            // Arrange
            List<Tuple<int/*recordId*/, Table/*table enum vaue*/>> tablesForCleanupTupleList = new List<Tuple<int, Table>>();
            int spiritId = 0;
            int vendorId = 0;
            int storageId = 0;
            int gnsMaterialId = 0;
            int waterMaterialId = 0;
            int purchaseId = 0;
            int productionId = 0;

            try
            {

                #region Dictionary
                //  dictionary setup
                SpiritObject spirit = new SpiritObject();
                spirit.SpiritName = "GIN";
                spirit.ProcessingReportTypeID = 18;

                spiritId = _dl.CreateSpirit(_userId, spirit);

                // setup Vendor object
                VendorObject vendor = new VendorObject();
                vendor.VendorName = "testVendor";

                vendorId = _dl.CreateVendor(_userId, vendor);
                tablesForCleanupTupleList.Add(Tuple.Create(vendorId, Table.Vendor));

                // setup Storage Object
                StorageObject storage = new StorageObject();
                storage.StorageName = "testStorage";
                storage.SerialNumber = "2H29NNS";

                storageId = _dl.CreateStorage(_userId, storage);
                tablesForCleanupTupleList.Add(Tuple.Create(storageId, Table.Storage));

                // setup Material Object
                // GNS
                {
                    RawMaterialObject gnsMaterial = new RawMaterialObject();
                    gnsMaterial.RawMaterialName = "GNS for GIN";
                    gnsMaterial.UnitType = "gal";
                    gnsMaterial.UnitTypeId = 1;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Distilled = true;
                    gnsMaterial.PurchaseMaterialTypes = materialBoolTypes;

                    gnsMaterialId = _dl.CreateRawMaterial(_userId, gnsMaterial);
                    tablesForCleanupTupleList.Add(Tuple.Create(gnsMaterialId, Table.MaterialDict));
                }

                // water
                {
                    RawMaterialObject waterMaterial = new RawMaterialObject();
                    waterMaterial.RawMaterialName = "Water";
                    waterMaterial.UnitType = "gal";
                    waterMaterial.UnitTypeId = 1;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Additive = true;
                    waterMaterial.PurchaseMaterialTypes = materialBoolTypes;

                    waterMaterialId = _dl.CreateRawMaterial(_userId, waterMaterial);
                    tablesForCleanupTupleList.Add(Tuple.Create(waterMaterialId, Table.MaterialDict));
                }
                #endregion

                #region Purchase
                // create Purchase Record (minimal required fields)
                PurchaseObject purchO = new PurchaseObject();
                purchO.PurBatchName = "GNS Purchase Test";
                purchO.PurchaseType = "Distilled";
                purchO.PurchaseDate = new DateTime(2018, 02, 10);
                purchO.Quantity = 100f; // 100 gallons
                purchO.VolumeByWeight = 0f;
                purchO.AlcoholContent = 96f;
                purchO.ProofGallon = 192f;
                purchO.RecordId = gnsMaterialId;
                purchO.Price = 350f;
                purchO.VendorId = vendorId;
                purchO.Gauged = true;

                List<StorageObject> storageList = new List<StorageObject>();
                StorageObject storageObject = new StorageObject();
                storageObject.StorageId = storageId;
                storageList.Add(storageObject);
                purchO.Storage = storageList;

                purchO.SpiritTypeReportingID = 8;
                purchO.Gauged = true;

                purchaseId = _dl.CreatePurchase(purchO, _userId);
                tablesForCleanupTupleList.Add(Tuple.Create(purchaseId, Table.Purchase));
                #endregion

                #region Production
                // Redistil GNS into GIN and mark it as Gauged
                ProductionObject prodO = new ProductionObject();
                prodO.BatchName = "Navy Gin";
                prodO.ProductionDate = new DateTime(2018, 02, 11);
                prodO.ProductionStart = new DateTime(2018, 02, 11);
                prodO.ProductionEnd = new DateTime(2018, 02, 11);
                prodO.SpiritCutId = 11; // mixed
                prodO.Gauged = true;
                prodO.ProductionType = "Distillation";
                prodO.Quantity = 50f; // 50 gallons of alcohol
                prodO.VolumeByWeight = 0f;
                prodO.AlcoholContent = 96f; // 96%
                prodO.ProofGallon = 96f; // 96pfg
                prodO.Storage = storageList; // we are using the same storage id as we use for Purchase to keep things simple
                prodO.SpiritTypeReportingID = 6; // GNS
                prodO.ProductionTypeId = 2;

                List<ObjInfo4Burndwn> usedMats = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat = new ObjInfo4Burndwn();
                uMat.ID = purchaseId;
                uMat.OldVal = 0f;
                uMat.NewVal = purchO.Quantity;
                uMat.DistillableOrigin = "pur";
                uMat.BurningDownMethod = "volume";

                usedMats.Add(uMat);

                prodO.UsedMats = usedMats;

                productionId = _dl.CreateProduction(prodO, _userId);

                tablesForCleanupTupleList.Add(Tuple.Create(productionId, Table.Production));

                // Redistil Navy GIN into another Navy GIN because the old batch was bad and mark it as Gauged
                ProductionObject prodO1 = new ProductionObject();
                prodO1.BatchName = "Navy Gin Redistil";
                prodO1.ProductionDate = new DateTime(2018, 03, 10);
                prodO1.ProductionStart = new DateTime(2018, 03, 10);
                prodO1.ProductionEnd = new DateTime(2018, 03, 10);
                prodO1.SpiritCutId = 11; // mixed
                prodO1.Gauged = true;
                prodO1.ProductionType = "Distillation";
                prodO1.Quantity = 50f; // 50 gallons of alcohol
                prodO1.VolumeByWeight = 0f;
                prodO1.AlcoholContent = 96f; // 96%
                prodO1.ProofGallon = 96f; // 96pfg
                prodO1.Storage = storageList; // we are using the same storage id as we use for Purchase to keep things simple
                prodO1.SpiritTypeReportingID = 6; // GNS
                prodO1.ProductionTypeId = 2;

                List<ObjInfo4Burndwn> usedMats1 = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat1 = new ObjInfo4Burndwn();
                uMat1.ID = productionId;
                uMat1.OldVal = 0f;
                uMat1.NewVal = prodO1.Quantity;
                uMat1.DistillableOrigin = "prod";
                uMat1.BurningDownMethod = "volume";

                usedMats1.Add(uMat1);

                prodO1.UsedMats = usedMats1;

                productionId = _dl.CreateProduction(prodO1, _userId);

                tablesForCleanupTupleList.Add(Tuple.Create(productionId, Table.Production));
                #endregion

                #region Reports setup

                // Report Header
                ReportHeader reportHeaderE = new ReportHeader();
                reportHeaderE.ProprietorName = "Test Distillery";
                reportHeaderE.EIN = "12-3456789";
                reportHeaderE.ReportDate = "March 2018";
                reportHeaderE.PlantAddress = "123 Cognac Drive Renton WASHINGTON 98059";
                reportHeaderE.DSP = "DSP-WA-21086";

                // reporting time range
                DateTime start = new DateTime(2018, 03, 01);
                DateTime end = new DateTime(2018, 03, 31);

                /* PRODUCTION REPORT */
                ProdReportPart1 part1E = new ProdReportPart1();
                part1E.ProccessingAcct = 0f;
                part1E.ProducedTotal = 96f;
                part1E.Recd4RedistilL17 = 0f;
                part1E.Recd4RedistilaltionL15 = 96f;
                part1E.StorageAcct = 96f;
                part1E.SpiritCatName = "Gin";
                part1E.SpiritTypeReportingID = 6;
                part1E.UnfinishedSpiritsEndOfQuarterL17 = 0f;

                ProdReportParts2Through4 part2thru4 = new ProdReportParts2Through4();
                part2thru4.KindOfMaterial = "";
                part2thru4.MaterialKindReportingID = 0;
                part2thru4.NewCoop = false;
                part2thru4.UsedCoop = false;
                part2thru4.ProofGallons = 0f;
                part2thru4.SpiritTypeReportingID = 0;
                part2thru4.Tanks = false;

                ProdReportPart6 part6 = new ProdReportPart6();
                part6.KindOfMaterial = "";
                part6.ProdReportMaterialCategoryID = 0;
                part6.Volume = 0f;
                part6.Weight = 0f;

                ProdReportPart5 part5 = new ProdReportPart5();
                part5.KindofSpirits = "Gin";
                part5.Proof = 96f;

                ProductionReportingObject actualProdReportObject = new ProductionReportingObject();


                actualProdReportObject = _dl.GetProductionReportData(start, end, _userId);

                #endregion

                // Assert

                #region Production Report
                // verify Production report header
                Assert.AreEqual(reportHeaderE.DSP, actualProdReportObject.Header.DSP);
                Assert.AreEqual(reportHeaderE.EIN, actualProdReportObject.Header.EIN);
                Assert.AreEqual(reportHeaderE.PlantAddress, actualProdReportObject.Header.PlantAddress);
                Assert.AreEqual(reportHeaderE.ProprietorName, actualProdReportObject.Header.ProprietorName);
                Assert.AreEqual(reportHeaderE.ReportDate, actualProdReportObject.Header.ReportDate);

                // verify Production report Part 1
                Assert.AreEqual(part1E.ProccessingAcct, actualProdReportObject.Part1List[0].ProccessingAcct);
                Assert.AreEqual(part1E.ProducedTotal, actualProdReportObject.Part1List[0].ProducedTotal);
                Assert.AreEqual(part1E.Recd4RedistilL17, actualProdReportObject.Part1List[0].Recd4RedistilL17);
                Assert.AreEqual(part1E.Recd4RedistilaltionL15, actualProdReportObject.Part1List[0].Recd4RedistilaltionL15);
                Assert.AreEqual(part1E.StorageAcct, actualProdReportObject.Part1List[0].StorageAcct);
                Assert.AreEqual(part1E.SpiritCatName, actualProdReportObject.Part1List[0].SpiritCatName);
                Assert.AreEqual(part1E.SpiritTypeReportingID, actualProdReportObject.Part1List[0].SpiritTypeReportingID);
                Assert.AreEqual(part1E.UnfinishedSpiritsEndOfQuarterL17, actualProdReportObject.Part1List[0].UnfinishedSpiritsEndOfQuarterL17);

                // verify Production report Part 2 trough 4
                Assert.AreEqual(0, actualProdReportObject.Part2Through4List.Count);

                // verify part 5
                Assert.AreEqual(part5.KindofSpirits, actualProdReportObject.part5List[0].KindofSpirits);
                Assert.AreEqual(part5.Proof, actualProdReportObject.part5List[0].Proof);

                // verify Production report Part 6
                Assert.AreEqual(0, actualProdReportObject.ProdReportPart6List.Count);
                #endregion
            }
            finally
            {
                // Cleanup
                foreach (var i in tablesForCleanupTupleList)
                {
                    TestRecordCleanup(i.Item1, i.Item2);
                }
            }
        }

        /// <summary>
        /// This test population of Redistilled Filled in Production Report 
        /// when redistillation happens out of GNS produced in-house
        /// </summary>
        [TestMethod()]
        public void BuyWine_DistilAndGauge_RedistillIntoBrandyOver170()
        {
            /*------STEPS
            1. Buy Wine
            2. Distil wine into Brandy 170+ and Gauge
            3. Redistill the same record again
            4. Check Production Report
            ----------*/

            // Arrange
            List<Tuple<int/*recordId*/, Table/*table enum vaue*/>> tablesForCleanupTupleList = new List<Tuple<int, Table>>();
            int spiritId = 0;
            int vendorId = 0;
            int storageId = 0;
            int wineMaterialId = 0;
            int waterMaterialId = 0;
            int purchaseId = 0;
            int productionId = 0;

            // reporting time range
            DateTime start = new DateTime(2017, 09, 01);
            DateTime end = new DateTime(2017, 09, 30);

            // List used in assertions for received for redistillation
            List<Tuple<int, float>> received4RedistillationLine15L = new List<Tuple<int, float>>(); // <SpiritTypeID, ProofGallons>

            // List used in assertions for entered into Storage
            List<Tuple<int, float>> enteredInStorageList = new List<Tuple<int, float>>(); // <SpiritTypeID, ProofGallons>

            // List used in asserions for Produced Total
            List<Tuple<int, float>> producedTotalLine14L = new List<Tuple<int, float>>(); // <SpiritTypeID, ProofGallons>

            try
            {
                #region Dictionary
                //  dictionary setup
                SpiritObject spirit = new SpiritObject();
                spirit.SpiritName = "Brandy Under 170";
                spirit.ProcessingReportTypeID = 12;

                spiritId = _dl.CreateSpirit(_userId, spirit);

                // setup Vendor object
                VendorObject vendor = new VendorObject();
                vendor.VendorName = "testVendor";

                vendorId = _dl.CreateVendor(_userId, vendor);
                tablesForCleanupTupleList.Add(Tuple.Create(vendorId, Table.Vendor));

                // setup Storage Object
                StorageObject storage = new StorageObject();
                storage.StorageName = "testStorage";
                storage.SerialNumber = "2H29NNS";

                storageId = _dl.CreateStorage(_userId, storage);
                tablesForCleanupTupleList.Add(Tuple.Create(storageId, Table.Storage));


                // setup Material Object
                // wine
                {
                    RawMaterialObject wineMaterial = new RawMaterialObject();
                    wineMaterial.RawMaterialName = "Wine For Brandy";
                    wineMaterial.MaterialCategoryID = 2;
                    wineMaterial.UnitType = "gal";
                    wineMaterial.UnitTypeId = 1;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Fermented = true;
                    wineMaterial.PurchaseMaterialTypes = materialBoolTypes;

                    wineMaterialId = _dl.CreateRawMaterial(_userId, wineMaterial);
                    tablesForCleanupTupleList.Add(Tuple.Create(wineMaterialId, Table.MaterialDict));
                }

                // water
                {
                    RawMaterialObject waterMaterial = new RawMaterialObject();
                    waterMaterial.RawMaterialName = "Water";
                    waterMaterial.UnitType = "gal";
                    waterMaterial.UnitTypeId = 1;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Additive = true;
                    waterMaterial.PurchaseMaterialTypes = materialBoolTypes;

                    waterMaterialId = _dl.CreateRawMaterial(_userId, waterMaterial);
                    tablesForCleanupTupleList.Add(Tuple.Create(waterMaterialId, Table.MaterialDict));
                }

                #endregion

                #region Purchase
                // create Purchase Record (minimal required fields)
                PurchaseObject purchO = new PurchaseObject();
                purchO.PurBatchName = "test7Purchase";
                purchO.PurchaseType = "Fermented";
                purchO.PurchaseDate = new DateTime(2017, 09, 1);
                purchO.Quantity = 1000f; // 100 gallons
                purchO.VolumeByWeight = 0f;
                purchO.AlcoholContent = 9f;
                purchO.ProofGallon = 180f;
                purchO.RecordId = wineMaterialId;
                purchO.Price = 350f;
                purchO.VendorId = vendorId;

                List<StorageObject> storageList = new List<StorageObject>();
                StorageObject storageObject = new StorageObject();
                storageObject.StorageId = storageId;
                storageList.Add(storageObject);
                purchO.Storage = storageList;

                purchO.SpiritTypeReportingID = 11;
                purchO.Gauged = true;

                purchaseId = _dl.CreatePurchase(purchO, _userId);
                tablesForCleanupTupleList.Add(Tuple.Create(purchaseId, Table.Purchase));
                #endregion

                #region Production
                // create 1st Production Distillation Record and mark it as Gauged
                ProductionObject prodO = new ProductionObject();
                prodO.BatchName = "1stDistilRunAndGauged";
                prodO.ProductionDate = new DateTime(2017, 09, 2);
                prodO.ProductionStart = new DateTime(2017, 09, 2);
                prodO.ProductionEnd = new DateTime(2017, 09, 2);
                prodO.SpiritCutId = 11; // mixed
                prodO.Gauged = true;
                prodO.ProductionType = "Distillation";
                prodO.ProductionTypeId = 2;
                prodO.Quantity = 50f; //50 gallons of alcohol
                prodO.VolumeByWeight = 0f;
                prodO.AlcoholContent = 80f; // 80%
                prodO.ProofGallon = 80f; // 80pfg
                prodO.Storage = storageList; // we are using the same storage id as we use for Purchase to keep things simple
                prodO.SpiritTypeReportingID = 9; // Alcohol 190+
                prodO.MaterialKindReportingID = 81;

                List<ObjInfo4Burndwn> usedMats = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat = new ObjInfo4Burndwn();
                uMat.ID = purchaseId;
                uMat.OldVal = 0f;
                uMat.NewVal = purchO.Quantity;
                uMat.DistillableOrigin = "pur";
                uMat.BurningDownMethod = "volume";

                usedMats.Add(uMat);

                prodO.UsedMats = usedMats;

                productionId = _dl.CreateProduction(prodO, _userId);
                tablesForCleanupTupleList.Add(Tuple.Create(productionId, Table.Production));

                // create 2nd Production Distillation Record and mark it as Gauged
                ProductionObject prodO1 = new ProductionObject();
                prodO1.BatchName = "2ndDistilRunAndGauged";
                prodO1.ProductionDate = new DateTime(2017, 09, 3);
                prodO1.ProductionStart = new DateTime(2017, 09, 3);
                prodO1.ProductionEnd = new DateTime(2017, 09, 3);
                prodO1.SpiritCutId = 11; // mixed
                prodO1.Gauged = true;
                prodO1.ProductionType = "Distillation";
                prodO1.ProductionTypeId = 2;
                prodO1.Quantity = 20f;
                prodO1.VolumeByWeight = 0f;
                prodO1.AlcoholContent = 45f;
                prodO1.ProofGallon = 18f;
                prodO1.Storage = storageList; // we are using the same storage id as we use for Purchase to keep things simple
                prodO1.SpiritTypeReportingID = 4; // Brandy 170 +
                prodO1.MaterialKindReportingID = 96; // neutral grape brandy

                List<ObjInfo4Burndwn> usedMats1 = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat1 = new ObjInfo4Burndwn();
                uMat1.ID = productionId;
                uMat1.OldVal = 0f;
                uMat1.NewVal = prodO.Quantity;
                uMat1.DistillableOrigin = "prod";
                uMat1.BurningDownMethod = "volume";

                usedMats1.Add(uMat1);

                prodO1.UsedMats = usedMats1;

                productionId = _dl.CreateProduction(prodO1, _userId);
                tablesForCleanupTupleList.Add(Tuple.Create(productionId, Table.Production));

                ProductionReportingObject actualProdReportObject = new ProductionReportingObject();

                actualProdReportObject = _dl.GetProductionReportData(start, end, _userId);

                // verify Production report Part 1
                Assert.AreEqual(0, actualProdReportObject.Part1List[0].ProccessingAcct);
                Assert.AreEqual(0, actualProdReportObject.Part1List[0].Recd4RedistilL17);

                // Assert that we redistilled 180 PFGals of wine
                received4RedistillationLine15L.Add(new Tuple<int, float>(purchO.SpiritTypeReportingID, purchO.ProofGallon));

                var wineE = received4RedistillationLine15L.Find(l => l.Item1 == purchO.SpiritTypeReportingID);
                var wineA = actualProdReportObject.Part1List.Find(x => x.SpiritTypeReportingID == wineE.Item1);

                if (wineA == null)
                {
                    Assert.AreNotEqual(null, wineA, "No records for SpiritTypeID");
                }
                else
                {
                    Assert.AreEqual(wineE.Item2, wineA.Recd4RedistilaltionL15);
                }

                // Assert that we redistilled 80 PFGals of Alcohol 190 +
                received4RedistillationLine15L.Add(new Tuple<int, float>(prodO.SpiritTypeReportingID, prodO.ProofGallon));

                var alcohol190PlusE = received4RedistillationLine15L.Find(l => l.Item1 == prodO.SpiritTypeReportingID);
                var alcohol190PlusA = actualProdReportObject.Part1List.Find(x => x.SpiritTypeReportingID == alcohol190PlusE.Item1);

                if (alcohol190PlusA == null)
                {
                    Assert.AreNotEqual(null, alcohol190PlusA, "No records in actualAlcohol190Plus for SpiritTypeID");
                }
                else
                {
                    Assert.AreEqual(alcohol190PlusE.Item2, alcohol190PlusA.Recd4RedistilaltionL15);
                }

                // Assert that 80 PFGals went into storage for 190+
                enteredInStorageList.Add(new Tuple<int, float>(prodO.SpiritTypeReportingID, prodO.ProofGallon));

                var alc190PlusE = enteredInStorageList.Find(l => l.Item1 == prodO.SpiritTypeReportingID);
                var alc190PlusA = actualProdReportObject.Part1List.Find(x => x.SpiritTypeReportingID == alc190PlusE.Item1);

                if (alc190PlusA == null)
                {
                    Assert.AreNotEqual(null, alc190PlusA, "No records for SpiritTypeID");
                }
                else
                {
                    Assert.AreEqual(alc190PlusE.Item2, alc190PlusA.StorageAcct);
                }

                // Assert that 18 PFGals went into storage for Brandy
                enteredInStorageList.Add(new Tuple<int, float>(prodO1.SpiritTypeReportingID, prodO1.ProofGallon));

                var brandy170PlusE = enteredInStorageList.Find(l => l.Item1 == prodO1.SpiritTypeReportingID);
                var brandy170PlusA = actualProdReportObject.Part1List.Find(x => x.SpiritTypeReportingID == brandy170PlusE.Item1);

                if (brandy170PlusA == null)
                {
                    Assert.AreNotEqual(null, alcohol190PlusA, "No records for SpiritTypeID");
                }
                else
                {
                    Assert.AreEqual(brandy170PlusE.Item2, brandy170PlusA.StorageAcct);
                }

                // Assert that 80 PFGals were produced for Alcohol 190+
                producedTotalLine14L.Add(new Tuple<int, float>(prodO.SpiritTypeReportingID, prodO.ProofGallon));

                var alc190PlusTotalProducedE = producedTotalLine14L.Find(l => l.Item1 == prodO.SpiritTypeReportingID);
                var alc190PlusTotalProducedA = actualProdReportObject.Part1List.Find(x => x.SpiritTypeReportingID == alc190PlusTotalProducedE.Item1);

                if (alc190PlusTotalProducedA == null)
                {
                    Assert.AreNotEqual(null, alc190PlusTotalProducedA, "No records for SpiritTypeID");
                }
                else
                {
                    Assert.AreEqual(alc190PlusTotalProducedE.Item2, alc190PlusTotalProducedA.ProducedTotal);
                }

                // Assert that 18 PFGals were produced for Brandy 170+
                producedTotalLine14L.Add(new Tuple<int, float>(prodO1.SpiritTypeReportingID, prodO1.ProofGallon));

                var brandy170PlusTotalProducedE = producedTotalLine14L.Find(l => l.Item1 == prodO1.SpiritTypeReportingID);
                var brandy170PlusTotalProducedA = actualProdReportObject.Part1List.Find(x => x.SpiritTypeReportingID == brandy170PlusTotalProducedE.Item1);

                if (brandy170PlusTotalProducedA == null)
                {
                    Assert.AreNotEqual(null, brandy170PlusTotalProducedA, "No records for SpiritTypeID");
                }
                else
                {
                    Assert.AreEqual(brandy170PlusTotalProducedE.Item2, brandy170PlusTotalProducedA.ProducedTotal);
                }

                Assert.AreEqual(0, actualProdReportObject.Part1List[0].UnfinishedSpiritsEndOfQuarterL17);

                #endregion

            }
            finally
            {
                // Cleanup
                foreach (var i in tablesForCleanupTupleList)
                {
                    TestRecordCleanup(i.Item1, i.Item2);
                }
            }
        }

        /// <summary>
        /// This test cheecks whether Fermented record shows up in received for redistillation in Production report
        /// </summary>
        [TestMethod()]
        public void BuyGrapes_Ferment_DistillAndGaugeIntoBrandyUnder170()
        {
            /*------STEPS
            1. Buy Grapes
            2. Ferment grapes into wine
            3. Distil wine into Brandy 170- and Gauge
            4. Check Production Report
            ----------*/

            // Arrange
            List<Tuple<int/*recordId*/, Table/*table enum vaue*/>> tablesForCleanupTupleList = new List<Tuple<int, Table>>();
            int spiritId = 0;
            int vendorId = 0;
            int storageId = 0;
            int grapeMaterialId = 0;
            int waterMaterialId = 0;
            int purchaseId = 0;
            int productionId = 0;

            // reporting time range
            DateTime start = new DateTime(2017, 09, 01);
            DateTime end = new DateTime(2017, 09, 30);

            // List used in assertions for received for redistillation
            List<Tuple<int, float>> received4RedistillationLine15L = new List<Tuple<int, float>>(); // <SpiritTypeID, ProofGallons>

            // List used in assertions for entered into Storage
            List<Tuple<int, float>> enteredInStorageList = new List<Tuple<int, float>>(); // <SpiritTypeID, ProofGallons>

            // List used in assertions for entered into Storage
            List<Tuple<string, float>> part5List = new List<Tuple<string, float>>(); // <SpiritTypeID, ProofGallons>

            try
            {
                #region Dictionary
                //  dictionary setup
                SpiritObject spirit = new SpiritObject();
                spirit.SpiritName = "Brandy Under 170";
                spirit.ProcessingReportTypeID = 12;

                spiritId = _dl.CreateSpirit(_userId, spirit);

                // setup Vendor object
                VendorObject vendor = new VendorObject();
                vendor.VendorName = "testVendor";

                vendorId = _dl.CreateVendor(_userId, vendor);
                tablesForCleanupTupleList.Add(Tuple.Create(vendorId, Table.Vendor));

                // setup Storage Object
                StorageObject storage = new StorageObject();
                storage.StorageName = "testStorage";
                storage.SerialNumber = "2H29NNS";

                storageId = _dl.CreateStorage(_userId, storage);
                tablesForCleanupTupleList.Add(Tuple.Create(storageId, Table.Storage));


                // setup Material Object
                // grapes
                {
                    RawMaterialObject grapeMaterial = new RawMaterialObject();
                    grapeMaterial.RawMaterialName = "Grapes";
                    grapeMaterial.MaterialCategoryID = (int)Persistence.BusinessLogicEnums.ProductionReportMaterialCategory.Fruit;
                    grapeMaterial.UnitType = "lb";
                    grapeMaterial.UnitTypeId = 2;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Fermentable = true;
                    grapeMaterial.PurchaseMaterialTypes = materialBoolTypes;

                    grapeMaterialId = _dl.CreateRawMaterial(_userId, grapeMaterial);
                    tablesForCleanupTupleList.Add(Tuple.Create(grapeMaterialId, Table.MaterialDict));
                }

                // water
                {
                    RawMaterialObject waterMaterial = new RawMaterialObject();
                    waterMaterial.RawMaterialName = "Water";
                    waterMaterial.UnitType = "gal";
                    waterMaterial.UnitTypeId = 1;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Additive = true;
                    waterMaterial.PurchaseMaterialTypes = materialBoolTypes;

                    waterMaterialId = _dl.CreateRawMaterial(_userId, waterMaterial);
                    tablesForCleanupTupleList.Add(Tuple.Create(waterMaterialId, Table.MaterialDict));
                }

                #endregion

                #region Purchase
                // create Purchase Record (minimal required fields)
                PurchaseObject purchO = new PurchaseObject();
                purchO.PurBatchName = "Riesling Grapes ";
                purchO.PurchaseType = "Fermentable";
                purchO.PurchaseDate = new DateTime(2017, 09, 1);
                purchO.Quantity = 0f;
                purchO.VolumeByWeight = 1000f;
                purchO.RecordId = grapeMaterialId;
                purchO.Price = 350f;
                purchO.VendorId = vendorId;

                List<StorageObject> storageList = new List<StorageObject>();
                StorageObject storageObject = new StorageObject();
                storageObject.StorageId = storageId;
                storageList.Add(storageObject);
                purchO.Storage = storageList;

                purchaseId = _dl.CreatePurchase(purchO, _userId);
                tablesForCleanupTupleList.Add(Tuple.Create(purchaseId, Table.Purchase));
                #endregion

                #region Production
                // create Fermented record
                ProductionObject prodO = new ProductionObject();
                prodO.BatchName = "Riesling Wine";
                prodO.ProductionDate = new DateTime(2017, 09, 2);
                prodO.ProductionStart = new DateTime(2017, 09, 7);
                prodO.ProductionEnd = new DateTime(2017, 09, 7);
                prodO.Gauged = true;
                prodO.ProductionType = "Fermentation";
                prodO.ProductionTypeId = 1;
                prodO.Quantity = 300f; // 300 gallons of wine
                prodO.VolumeByWeight = 0f;
                prodO.AlcoholContent = 12f; // %
                prodO.ProofGallon = 72f; // pfg
                prodO.Storage = storageList; // we are using the same storage id as we use for Purchase to keep things simple
                prodO.SpiritTypeReportingID = 11; // Wine
                prodO.MaterialKindReportingID = 0;
                prodO.SpiritCutName = "Wine";

                List<ObjInfo4Burndwn> usedMats = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat = new ObjInfo4Burndwn();
                uMat.ID = purchaseId;
                uMat.OldVal = 0f;
                uMat.NewVal = purchO.Quantity;
                uMat.DistillableOrigin = "pur";
                uMat.BurningDownMethod = "weight";

                usedMats.Add(uMat);

                prodO.UsedMats = usedMats;

                productionId = _dl.CreateProduction(prodO, _userId);
                tablesForCleanupTupleList.Add(Tuple.Create(productionId, Table.Production));

                // create Production Distillation Record and mark it as Gauged
                ProductionObject prodO1 = new ProductionObject();
                prodO1.BatchName = "DistilRunAndGauged";
                prodO1.ProductionDate = new DateTime(2017, 09, 8);
                prodO1.ProductionStart = new DateTime(2017, 09, 8);
                prodO1.ProductionEnd = new DateTime(2017, 09, 8);
                prodO1.SpiritCutId = 11; // mixed
                prodO1.Gauged = true;
                prodO1.ProductionType = "Distillation";
                prodO1.ProductionTypeId = 2;
                prodO1.Quantity = 80f;
                prodO1.VolumeByWeight = 0f;
                prodO1.AlcoholContent = 42f;
                prodO1.ProofGallon = 67.20f;
                prodO1.Storage = storageList; // we are using the same storage id as we use for Purchase to keep things simple
                prodO1.SpiritTypeReportingID = 3; // Brandy 170-
                prodO1.MaterialKindReportingID = 94; // grape brandy

                List<ObjInfo4Burndwn> usedMats1 = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat1 = new ObjInfo4Burndwn();
                uMat1.ID = productionId;
                uMat1.OldVal = 0f;
                uMat1.NewVal = prodO.Quantity;
                uMat1.DistillableOrigin = "prod";
                uMat1.BurningDownMethod = "volume";

                usedMats1.Add(uMat1);

                prodO1.UsedMats = usedMats1;

                productionId = _dl.CreateProduction(prodO1, _userId);
                tablesForCleanupTupleList.Add(Tuple.Create(productionId, Table.Production));

                #endregion

                #region Reports
                ProductionReportingObject actualProdReportObject = new ProductionReportingObject();

                actualProdReportObject = _dl.GetProductionReportData(start, end, _userId);

                // verify Production report Part 1
                Assert.AreEqual(0, actualProdReportObject.Part1List[0].ProccessingAcct);
                Assert.AreEqual(prodO1.ProofGallon, actualProdReportObject.Part1List[0].ProducedTotal);
                Assert.AreEqual(0, actualProdReportObject.Part1List[0].Recd4RedistilL17);
                Assert.AreEqual(0, actualProdReportObject.Part1List[0].UnfinishedSpiritsEndOfQuarterL17);
                Assert.AreEqual(prodO1.ProofGallon, actualProdReportObject.Part1List[0].StorageAcct); // went into storage

                // Assert that we redistilled 72 PFGals of wine
                received4RedistillationLine15L.Add(new Tuple<int, float>(prodO.SpiritTypeReportingID, prodO.ProofGallon));

                // redistillation verification
                var wineE = received4RedistillationLine15L.Find(l => l.Item1 == prodO.SpiritTypeReportingID);
                var wineA = actualProdReportObject.Part1List.Find(x => x.SpiritTypeReportingID == wineE.Item1);

                if (wineA == null)
                {
                    Assert.AreNotEqual(null, wineA, "No records in actualReceived4RedistillationPFGal for SpiritTypeID");
                }
                else
                {
                    Assert.AreEqual(wineE.Item2, wineA.Recd4RedistilaltionL15);
                }

                // Assert that we get Wine and 72 PFGals in Part5
                part5List.Add(new Tuple<string, float>("Wine", prodO.ProofGallon));

                var part5E = part5List.Find(x => x.Item1 == "Wine");
                var part5A = actualProdReportObject.part5List.Find(x => x.KindofSpirits == part5E.Item1);

                if (part5A == null)
                {
                    Assert.AreNotEqual(null, part5A, "No records for Spirit Type Name");
                }
                else
                {
                    Assert.AreEqual(part5E.Item2, part5A.Proof);
                }

                #endregion
            }
            finally
            {
                // Cleanup created records
                foreach (var i in tablesForCleanupTupleList)
                {
                    TestRecordCleanup(i.Item1, i.Item2);
                }
            }
        }

        /// <summary>
        /// This test method verifies that after user buys grapes and ferments it.
        /// Produced wine should show up in Storage report.
        /// </summary>
        [TestMethod()]
        public void BuyGrapes_ProduceWine_ShowInStorageReport()
        {
            /*
             * 1. Buy Grapes
             * 2. Ferment Grapes
             * 3. Check Storage report
             **/

            // Arrange
            int vendorId = 0;
            int storageId = 0;
            int grapeMaterialId = 0;
            int waterMaterialId = 0;
            int purchaseId = 0;
            int productionId = 0;

            // reporting time range
            DateTime start = new DateTime(2017, 09, 01);
            DateTime end = new DateTime(2017, 09, 30);

            List<Tuple<int/*recordId*/, Table/*table enum vaue*/>> tablesForCleanupTupleList = new List<Tuple<int, Table>>();

            // List used in assertions in Storage report
            List<Tuple<int, float>> storageDepositedInBulkStorageList = new List<Tuple<int, float>>(); // <SpiritTypeID, ProofGallons>
            List<Tuple<int, float>> storageTotalLine6StorageList = new List<Tuple<int, float>>(); // <SpiritTypeID, ProofGallons>
            List<Tuple<int, float>> storageOnHandEndOfMonthLine23StorageList = new List<Tuple<int, float>>(); // <SpiritTypeID, ProofGallons>
            List<Tuple<int, float>> storageTotalLine24StorageList = new List<Tuple<int, float>>(); // <SpiritTypeID, ProofGallons>

            try
            {
                //  dictionary setup
                #region Dictionary

                // setup Vendor object
                VendorObject vendor = new VendorObject();
                vendor.VendorName = "testVendor";

                vendorId = _dl.CreateVendor(_userId, vendor);
                tablesForCleanupTupleList.Add(Tuple.Create(vendorId, Table.Vendor));

                // setup Storage Object
                StorageObject storage = new StorageObject();
                storage.StorageName = "testStorage";
                storage.SerialNumber = "2H29NNS";

                storageId = _dl.CreateStorage(_userId, storage);
                tablesForCleanupTupleList.Add(Tuple.Create(storageId, Table.Storage));

                // setup Material Object
                // grapes
                {
                    RawMaterialObject grapeMaterial = new RawMaterialObject();
                    grapeMaterial.RawMaterialName = "Grapes";
                    grapeMaterial.MaterialCategoryID = (int)Persistence.BusinessLogicEnums.ProductionReportMaterialCategory.Fruit;
                    grapeMaterial.UnitType = "lb";
                    grapeMaterial.UnitTypeId = 2;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Fermentable = true;
                    grapeMaterial.PurchaseMaterialTypes = materialBoolTypes;

                    grapeMaterialId = _dl.CreateRawMaterial(_userId, grapeMaterial);
                    tablesForCleanupTupleList.Add(Tuple.Create(grapeMaterialId, Table.MaterialDict));
                }

                // water
                {
                    RawMaterialObject waterMaterial = new RawMaterialObject();
                    waterMaterial.RawMaterialName = "Water";
                    waterMaterial.UnitType = "gal";
                    waterMaterial.UnitTypeId = 1;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Additive = true;
                    waterMaterial.PurchaseMaterialTypes = materialBoolTypes;

                    waterMaterialId = _dl.CreateRawMaterial(_userId, waterMaterial);
                    tablesForCleanupTupleList.Add(Tuple.Create(waterMaterialId, Table.MaterialDict));
                }

                #endregion

                #region Purchase
                // create Purchase Record (minimal required fields)
                PurchaseObject purchO = new PurchaseObject();
                purchO.PurBatchName = "Riesling Grapes ";
                purchO.PurchaseType = "Fermentable";
                purchO.PurchaseDate = new DateTime(2017, 09, 1);
                purchO.Quantity = 0f;
                purchO.VolumeByWeight = 1000f;
                purchO.RecordId = grapeMaterialId;
                purchO.Price = 350f;
                purchO.VendorId = vendorId;

                List<StorageObject> storageList = new List<StorageObject>();
                StorageObject storageObject = new StorageObject();
                storageObject.StorageId = storageId;
                storageList.Add(storageObject);
                purchO.Storage = storageList;

                purchaseId = _dl.CreatePurchase(purchO, _userId);
                tablesForCleanupTupleList.Add(Tuple.Create(purchaseId, Table.Purchase));

                #endregion

                #region Production
                // create Fermented record
                ProductionObject prodO = new ProductionObject();
                prodO.BatchName = "Riesling Wine";
                prodO.ProductionDate = new DateTime(2017, 09, 2);
                prodO.ProductionStart = new DateTime(2017, 09, 7);
                prodO.ProductionEnd = new DateTime(2017, 09, 7);
                prodO.Gauged = true;
                prodO.ProductionType = "Fermentation";
                prodO.ProductionTypeId = 1;
                prodO.Quantity = 300f; // 300 gallons of wine
                prodO.VolumeByWeight = 0f;
                prodO.AlcoholContent = 12f; // %
                prodO.ProofGallon = 72f; // pfg
                prodO.Storage = storageList; // we are using the same storage id as we use for Purchase to keep things simple
                prodO.SpiritTypeReportingID = 11; // Wine
                prodO.MaterialKindReportingID = 0;

                List<ObjInfo4Burndwn> usedMats = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat = new ObjInfo4Burndwn();
                uMat.ID = purchaseId;
                uMat.OldVal = 0f;
                uMat.NewVal = purchO.Quantity;
                uMat.DistillableOrigin = "pur";
                uMat.BurningDownMethod = "weight";

                usedMats.Add(uMat);

                prodO.UsedMats = usedMats;

                productionId = _dl.CreateProduction(prodO, _userId);
                tablesForCleanupTupleList.Add(Tuple.Create(productionId, Table.Production));

                #endregion

                #region Reports

                // Report Header
                ReportHeader reportHeaderE = new ReportHeader();
                reportHeaderE.ProprietorName = "Test Distillery";
                reportHeaderE.EIN = "12-3456789";
                reportHeaderE.ReportDate = "September 2017";
                reportHeaderE.PlantAddress = "123 Cognac Drive Renton WASHINGTON 98059";
                reportHeaderE.DSP = "DSP-WA-21086";

                #region Storage Report

                StorageReport actualStorageReportObject = new StorageReport();

                //get actual storage data
                actualStorageReportObject = _dl.GetStorageReportData(start, end, _userId);

                // verify Storage report Header
                Assert.AreEqual(reportHeaderE.DSP, actualStorageReportObject.Header.DSP);
                Assert.AreEqual(reportHeaderE.EIN, actualStorageReportObject.Header.EIN);
                Assert.AreEqual(reportHeaderE.PlantAddress, actualStorageReportObject.Header.PlantAddress);
                Assert.AreEqual(reportHeaderE.ProprietorName, actualStorageReportObject.Header.ProprietorName);
                Assert.AreEqual(reportHeaderE.ReportDate, actualStorageReportObject.Header.ReportDate);

                // verify Storage body

                // Assert that we fermented 72 PFGals of wine
                storageDepositedInBulkStorageList.Add(new Tuple<int, float>(prodO.SpiritTypeReportingID, prodO.ProofGallon));

                var wineDeportitedInBulkStorageE = storageDepositedInBulkStorageList.Find(x => x.Item1 == prodO.SpiritTypeReportingID);
                var wineDeportitedInBulkStorageA = actualStorageReportObject.ReportBody.Find(x => x.SpiritTypeReportingID == wineDeportitedInBulkStorageE.Item1);

                if (wineDeportitedInBulkStorageA == null)
                {
                    Assert.AreNotEqual(null, wineDeportitedInBulkStorageA, "No records for SpiritTypeID");
                }
                else
                {
                    Assert.AreEqual(wineDeportitedInBulkStorageE.Item2, wineDeportitedInBulkStorageA.r2_DepositedInBulkStorage);
                }

                storageOnHandEndOfMonthLine23StorageList.Add(new Tuple<int, float>(prodO.SpiritTypeReportingID, prodO.ProofGallon));

                var wineOnHandEndOfMonthE = storageOnHandEndOfMonthLine23StorageList.Find(x => x.Item1 == prodO.SpiritTypeReportingID);
                var wineOnHandEndOfMonthA = actualStorageReportObject.ReportBody.Find(x => x.SpiritTypeReportingID == prodO.SpiritTypeReportingID);

                if (wineOnHandEndOfMonthA == null)
                {
                    Assert.AreNotEqual(null, wineOnHandEndOfMonthA, "No records for SpiritTypeID");
                }
                else
                {
                    Assert.AreEqual(wineOnHandEndOfMonthE.Item2, wineDeportitedInBulkStorageA.r23_OnHandEndOfMonth);
                }

                storageTotalLine6StorageList.Add(new Tuple<int, float>(prodO.SpiritTypeReportingID, prodO.ProofGallon));

                var wineTotalLine6E = storageTotalLine6StorageList.Find(x => x.Item1 == prodO.SpiritTypeReportingID);
                var wineTotalLine6A = actualStorageReportObject.ReportBody.Find(x => x.SpiritTypeReportingID == prodO.SpiritTypeReportingID);

                if (wineOnHandEndOfMonthA == null)
                {
                    Assert.AreNotEqual(null, wineTotalLine6A, "No records for SpiritTypeID");
                }
                else
                {
                    Assert.AreEqual(wineTotalLine6E.Item2, wineTotalLine6A.r6_TotalLines1Through5);
                }

                storageTotalLine24StorageList.Add(new Tuple<int, float>(prodO.SpiritTypeReportingID, prodO.ProofGallon));

                var wineTotalLine24E = storageTotalLine6StorageList.Find(x => x.Item1 == prodO.SpiritTypeReportingID);
                var wineTotalLine24A = actualStorageReportObject.ReportBody.Find(x => x.SpiritTypeReportingID == prodO.SpiritTypeReportingID);

                if (wineTotalLine24A == null)
                {
                    Assert.AreNotEqual(null, wineTotalLine24A, "No records for SpiritTypeID");
                }
                else
                {
                    Assert.AreEqual(wineTotalLine24E.Item2, wineTotalLine24A.r24_Lines7Through23);
                }

                #endregion

                #endregion
            }
            finally
            {
                // Cleanup created records
                foreach (var i in tablesForCleanupTupleList)
                {
                    TestRecordCleanup(i.Item1, i.Item2);
                }
            }

        }

        /// <summary>
        /// This test method asserts the following:
        /// Buy Grapes in Month of February
        /// Make Wine in the month of February
        /// Make Distil in the month of March
        /// When we pull production report for Month of February, no records should show up in Part 6
        /// </summary>
        [TestMethod()]
        public void BuyGrapes_ProduceWine_Distil_InNextMonth_MakeSure_NoRecords_In_PproductionReport_Part6_This_Month()
        {
            // Arrange
            int vendorId = 0;
            int storageId = 0;
            int grapeMaterialId = 0;
            int purchaseId = 0;
            int productionId = 0;

            // reporting time range
            DateTime start = new DateTime(2018, 02, 01);
            DateTime end = new DateTime(2018, 02, 28);

            List<Tuple<int/*recordId*/, Table/*table enum vaue*/>> tablesForCleanupTupleList = new List<Tuple<int, Table>>();

            try
            {
                //  dictionary setup
                #region Dictionary

                // setup Vendor object
                VendorObject vendor = new VendorObject();
                vendor.VendorName = "testVendor";

                vendorId = _dl.CreateVendor(_userId, vendor);
                tablesForCleanupTupleList.Add(Tuple.Create(vendorId, Table.Vendor));

                // setup Storage Object
                StorageObject storage = new StorageObject();
                storage.StorageName = "testStorage";
                storage.SerialNumber = "2H29NNS";

                storageId = _dl.CreateStorage(_userId, storage);
                tablesForCleanupTupleList.Add(Tuple.Create(storageId, Table.Storage));

                // setup Material Object
                // grapes
                {
                    RawMaterialObject grapeMaterial = new RawMaterialObject();
                    grapeMaterial.RawMaterialName = "Grapes";
                    grapeMaterial.MaterialCategoryID = (int)Persistence.BusinessLogicEnums.ProductionReportMaterialCategory.Fruit;
                    grapeMaterial.UnitType = "lb";
                    grapeMaterial.UnitTypeId = 2;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Fermentable = true;
                    grapeMaterial.PurchaseMaterialTypes = materialBoolTypes;

                    grapeMaterialId = _dl.CreateRawMaterial(_userId, grapeMaterial);

                    tablesForCleanupTupleList.Add(Tuple.Create(grapeMaterialId, Table.MaterialDict));
                }

                #endregion

                #region Purchase
                // create Purchase Record (minimal required fields)
                PurchaseObject purchO = new PurchaseObject();
                purchO.PurBatchName = "Riesling Grapes ";
                purchO.PurchaseType = "Fermentable";
                purchO.PurchaseDate = new DateTime(2018, 02, 03);
                purchO.Quantity = 0f;
                purchO.VolumeByWeight = 4000f;
                purchO.RecordId = grapeMaterialId;
                purchO.Price = 350f;
                purchO.VendorId = vendorId;

                List<StorageObject> storageList = new List<StorageObject>();
                StorageObject storageObject = new StorageObject();
                storageObject.StorageId = storageId;
                storageList.Add(storageObject);
                purchO.Storage = storageList;

                purchaseId = _dl.CreatePurchase(purchO, _userId);
                tablesForCleanupTupleList.Add(Tuple.Create(purchaseId, Table.Purchase));

                #endregion

                #region Production
                // create Fermented record
                ProductionObject prodO = new ProductionObject();
                prodO.BatchName = "Riesling Wine";
                prodO.ProductionDate = new DateTime(2018, 02, 15);
                prodO.ProductionStart = new DateTime(2018, 02, 15);
                prodO.ProductionEnd = new DateTime(2018, 02, 15);
                prodO.Gauged = true;
                prodO.ProductionType = "Fermentation";
                prodO.ProductionTypeId = 1;
                prodO.Quantity = 500f; // 500 gallons of wine
                prodO.VolumeByWeight = 0f;
                prodO.AlcoholContent = 12f; // %
                prodO.ProofGallon = 120f; // pfg
                prodO.Storage = storageList; // we are using the same storage id as we use for Purchase to keep things simple
                prodO.SpiritTypeReportingID = 11; // Wine
                prodO.MaterialKindReportingID = 0;

                List<ObjInfo4Burndwn> usedMats = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat = new ObjInfo4Burndwn();
                uMat.ID = purchaseId;
                uMat.OldVal = 0f;
                uMat.NewVal = purchO.VolumeByWeight;
                uMat.DistillableOrigin = "pur";
                uMat.BurningDownMethod = "weight";

                usedMats.Add(uMat);

                prodO.UsedMats = usedMats;

                productionId = _dl.CreateProduction(prodO, _userId);

                tablesForCleanupTupleList.Add(Tuple.Create(productionId, Table.Production));

                // create Production Distillation Record and mark it as Gauged
                ProductionObject prodO1 = new ProductionObject();
                prodO1.BatchName = "DistilRunAndGauged";
                prodO1.ProductionDate = new DateTime(2018, 03, 20);
                prodO1.ProductionStart = new DateTime(2018, 03, 20);
                prodO1.ProductionEnd = new DateTime(2018, 03, 20);
                prodO1.SpiritCutId = 11; // mixed
                prodO1.Gauged = true;
                prodO1.ProductionType = "Distillation";
                prodO1.ProductionTypeId = 2;
                prodO1.Quantity = 25f; // 25 gallons
                prodO1.VolumeByWeight = 0f;
                prodO1.AlcoholContent = 55f;
                prodO1.ProofGallon = 27.5f;
                prodO1.Storage = storageList; // we are using the same storage id as we use for Purchase to keep things simple
                prodO1.SpiritTypeReportingID = 3; // Brandy 170-
                prodO1.MaterialKindReportingID = 94; // grape brandy

                List<ObjInfo4Burndwn> usedMats1 = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat1 = new ObjInfo4Burndwn();
                uMat1.ID = productionId;
                uMat1.OldVal = 0f;
                uMat1.NewVal = prodO.Quantity;
                uMat1.DistillableOrigin = "prod";
                uMat1.BurningDownMethod = "volume";

                usedMats1.Add(uMat1);

                prodO1.UsedMats = usedMats1;

                productionId = _dl.CreateProduction(prodO1, _userId);

                tablesForCleanupTupleList.Add(Tuple.Create(productionId, Table.Production));

                #endregion

                #region Reports

                // Report Header
                ReportHeader reportHeaderE = new ReportHeader();
                reportHeaderE.ProprietorName = "Test Distillery";
                reportHeaderE.EIN = "12-3456789";
                reportHeaderE.ReportDate = "February 2018";
                reportHeaderE.PlantAddress = "123 Cognac Drive Renton WASHINGTON 98059";
                reportHeaderE.DSP = "DSP-WA-21086";

                ProductionReportingObject actualProductionReport = new ProductionReportingObject();

                //get actual storage data
                actualProductionReport = _dl.GetProductionReportData(start, end, _userId);

                // verify Storage report Header
                Assert.AreEqual(reportHeaderE.DSP, actualProductionReport.Header.DSP);
                Assert.AreEqual(reportHeaderE.EIN, actualProductionReport.Header.EIN);
                Assert.AreEqual(reportHeaderE.PlantAddress, actualProductionReport.Header.PlantAddress);
                Assert.AreEqual(reportHeaderE.ProprietorName, actualProductionReport.Header.ProprietorName);
                Assert.AreEqual(reportHeaderE.ReportDate, actualProductionReport.Header.ReportDate);

                // verify no Production Report Part 6
                // we shouldn't have any data shown up in this test
                Assert.AreEqual(0, actualProductionReport.ProdReportPart6List.Count);

                #endregion
            }
            finally
            {
                // Cleanup created records
                foreach (var i in tablesForCleanupTupleList)
                {
                    TestRecordCleanup(i.Item1, i.Item2);
                }
            }
        }

        /// <summary>
        /// This test method asserts the following:
        /// Buy Grapes in Month of February
        /// Make Wine in the month of February
        /// Make Distil in the month of February by burning down portion of Wine
        /// When we pull production report for Month of February, Part 6 
        /// should show partial materials used ammount
        /// </summary>
        [TestMethod()]
        public void BuyGrapes_ProduceWine_DistilPartialAmmount_ProductionReportPart6_Shows_Correct_Materials_Used_Amount()
        {
            // Arrange
            int vendorId = 0;
            int storageId = 0;
            int grapeMaterialId = 0;
            int purchaseId = 0;
            int productionId = 0;

            // reporting time range
            DateTime start = new DateTime(2018, 02, 01);
            DateTime end = new DateTime(2018, 02, 28);

            List<Tuple<int/*recordId*/, Table/*table enum vaue*/>> tablesForCleanupTupleList = new List<Tuple<int, Table>>();

            try
            {
                //  dictionary setup
                #region Dictionary

                // setup Vendor object
                VendorObject vendor = new VendorObject();
                vendor.VendorName = "testVendor";

                vendorId = _dl.CreateVendor(_userId, vendor);
                tablesForCleanupTupleList.Add(Tuple.Create(vendorId, Table.Vendor));

                // setup Storage Object
                StorageObject storage = new StorageObject();
                storage.StorageName = "testStorage";
                storage.SerialNumber = "2H29NNS";

                storageId = _dl.CreateStorage(_userId, storage);
                tablesForCleanupTupleList.Add(Tuple.Create(storageId, Table.Storage));

                // setup Material Object
                // grapes
                {
                    RawMaterialObject grapeMaterial = new RawMaterialObject();
                    grapeMaterial.RawMaterialName = "Grapes";
                    grapeMaterial.MaterialCategoryID = (int)Persistence.BusinessLogicEnums.ProductionReportMaterialCategory.Fruit;
                    grapeMaterial.UnitType = "lb";
                    grapeMaterial.UnitTypeId = 2;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Fermentable = true;
                    grapeMaterial.PurchaseMaterialTypes = materialBoolTypes;

                    grapeMaterialId = _dl.CreateRawMaterial(_userId, grapeMaterial);

                    tablesForCleanupTupleList.Add(Tuple.Create(grapeMaterialId, Table.MaterialDict));
                }

                #endregion

                #region Purchase
                // create Purchase Record (minimal required fields)
                PurchaseObject purchO = new PurchaseObject();
                purchO.PurBatchName = "Riesling Grapes ";
                purchO.PurchaseType = "Fermentable";
                purchO.PurchaseDate = new DateTime(2018, 02, 03);
                purchO.Quantity = 0f;
                purchO.VolumeByWeight = 4000f;
                purchO.RecordId = grapeMaterialId;
                purchO.Price = 350f;
                purchO.VendorId = vendorId;

                List<StorageObject> storageList = new List<StorageObject>();
                StorageObject storageObject = new StorageObject();
                storageObject.StorageId = storageId;
                storageList.Add(storageObject);
                purchO.Storage = storageList;

                purchaseId = _dl.CreatePurchase(purchO, _userId);
                tablesForCleanupTupleList.Add(Tuple.Create(purchaseId, Table.Purchase));

                #endregion

                #region Production
                // create Fermented record
                ProductionObject prodO = new ProductionObject();
                prodO.BatchName = "Riesling Wine";
                prodO.ProductionDate = new DateTime(2018, 02, 15);
                prodO.ProductionStart = new DateTime(2018, 02, 15);
                prodO.ProductionEnd = new DateTime(2018, 02, 15);
                prodO.Gauged = true;
                prodO.ProductionType = "Fermentation";
                prodO.ProductionTypeId = 1;
                prodO.Quantity = 500f; // 500 gallons of wine
                prodO.VolumeByWeight = 0f;
                prodO.AlcoholContent = 12f; // %
                prodO.ProofGallon = 120f; // pfg
                prodO.Storage = storageList; // we are using the same storage id as we use for Purchase to keep things simple
                prodO.SpiritTypeReportingID = 11; // Wine
                prodO.MaterialKindReportingID = 0;

                List<ObjInfo4Burndwn> usedMats = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat = new ObjInfo4Burndwn();
                uMat.ID = purchaseId;
                uMat.OldVal = 0f;
                uMat.NewVal = purchO.VolumeByWeight;
                uMat.DistillableOrigin = "pur";
                uMat.BurningDownMethod = "weight";

                usedMats.Add(uMat);

                prodO.UsedMats = usedMats;

                productionId = _dl.CreateProduction(prodO, _userId);

                tablesForCleanupTupleList.Add(Tuple.Create(productionId, Table.Production));

                // create Production Distillation Record and mark it as Gauged
                ProductionObject prodO1 = new ProductionObject();
                prodO1.BatchName = "DistilRunAndGauged";
                prodO1.ProductionDate = new DateTime(2018, 02, 20);
                prodO1.ProductionStart = new DateTime(2018, 02, 20);
                prodO1.ProductionEnd = new DateTime(2018, 02, 20);
                prodO1.SpiritCutId = 11; // mixed
                prodO1.Gauged = true;
                prodO1.ProductionType = "Distillation";
                prodO1.ProductionTypeId = 2;
                prodO1.Quantity = 25f; // 25 gallons
                prodO1.VolumeByWeight = 0f;
                prodO1.AlcoholContent = 55f;
                prodO1.ProofGallon = 27.5f;
                prodO1.Storage = storageList; // we are using the same storage id as we use for Purchase to keep things simple
                prodO1.SpiritTypeReportingID = 3; // Brandy 170-
                prodO1.MaterialKindReportingID = 94; // grape brandy

                List<ObjInfo4Burndwn> usedMats1 = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat1 = new ObjInfo4Burndwn();
                uMat1.ID = productionId;
                uMat1.OldVal = 0f;
                uMat1.NewVal = (prodO.Quantity / 2);
                uMat1.DistillableOrigin = "prod";
                uMat1.BurningDownMethod = "volume";

                usedMats1.Add(uMat1);

                prodO1.UsedMats = usedMats1;

                productionId = _dl.CreateProduction(prodO1, _userId);

                tablesForCleanupTupleList.Add(Tuple.Create(productionId, Table.Production));

                #endregion

                #region Reports

                // Report Header
                ReportHeader reportHeaderE = new ReportHeader();
                reportHeaderE.ProprietorName = "Test Distillery";
                reportHeaderE.EIN = "12-3456789";
                reportHeaderE.ReportDate = "February 2018";
                reportHeaderE.PlantAddress = "123 Cognac Drive Renton WASHINGTON 98059";
                reportHeaderE.DSP = "DSP-WA-21086";

                ProductionReportingObject actualProductionReport = new ProductionReportingObject();

                //get actual storage data
                actualProductionReport = _dl.GetProductionReportData(start, end, _userId);

                // verify Storage report Header
                Assert.AreEqual(reportHeaderE.DSP, actualProductionReport.Header.DSP);
                Assert.AreEqual(reportHeaderE.EIN, actualProductionReport.Header.EIN);
                Assert.AreEqual(reportHeaderE.PlantAddress, actualProductionReport.Header.PlantAddress);
                Assert.AreEqual(reportHeaderE.ProprietorName, actualProductionReport.Header.ProprietorName);
                Assert.AreEqual(reportHeaderE.ReportDate, actualProductionReport.Header.ReportDate);

                // verify no Production Report Part 6
                // we should display half of the used materials since only half of the wine was used in this production
                Assert.AreEqual(2000, actualProductionReport.ProdReportPart6List[0].Weight);

                #endregion
            }
            finally
            {
                // Cleanup created records
                foreach (var i in tablesForCleanupTupleList)
                {
                    TestRecordCleanup(i.Item1, i.Item2);
                }
            }
        }

        // January:
        // 1) FermentedPurchase-1: Purchase grapes (10000 pounds)
        // 2) FermentedProduction-1: Ferment 40% of FermentedPurchase-1 (4000/10000 pounds into 400 wine gallons at 10% alcohol)
        // 3) DistilledProduction-1: Distill 50% of FermentedProduction-1 and mark as Brandy (200/400 wine gallons at 50% alcohol)

        // February:
        // 1) FermentedProduction-2: Ferment 30% of FermentedPurchase-1 (3000/10000 pounds into 300 wine gallons at 10% alcohol)
        // 2) DistilledProduction-2: Distill 50% of FermentedProduction-1 and mark as Brandy (200/400 wine gallons at 50% alcohol)

        // March:
        // 1) FermentedProduction-3: Ferment 30% of FermentedPurchase-1 (3000/10000 pounds into 300 wine gallons at 10% alcohol)
        // 2) DistilledProduction-3: Distill all of FermentedProduction-2/FermentedProduction-3 and mark as Brandy (600/600 wine gallons at 50% alcohol)

        // April:
        // 1) DistilledProduction-2: Redistill all three DistilledProductions into single Brandy batch (1000/1000 wine gallons at 50% alcohol)

        /// <summary>
        /// This test validates the following:
        /// Buy Grapes in January
        /// Ferment half of Grapes in January
        /// Distil half of Fermented Grapes in January as Brandy Under 170
        /// Storage report should be showing Wine as well as Brandy Under 170 columns 
        /// </summary>
        [TestMethod()]
        public void BuyGrapes_MakeWinePartial_DistilBrandyPartial_MakeSure_WineAndBrandyBothShowUpInStorageReport()
        {
            // Arrange
            int vendorId = 0;
            int storageId = 0;
            int grapeMaterialId = 0;
            int purchaseId = 0;
            int productionId = 0;

            // reporting time range
            DateTime start = new DateTime(2018, 01, 01);
            DateTime end = new DateTime(2018, 01, 31);

            // int - table row id
            // Table - enum identifying table type
            List<Tuple<int, Table>> garbage = new List<Tuple<int, Table>>();

            try
            {
                //  dictionary setup
                #region Dictionary

                // setup Vendor object
                VendorObject vendor = new VendorObject();
                vendor.VendorName = "VendorTest";

                vendorId = _dl.CreateVendor(_userId, vendor);
                garbage.Add(Tuple.Create(vendorId, Table.Vendor));

                // setup Storage Object
                StorageObject storage = new StorageObject();
                storage.StorageName = "testStorage";
                storage.SerialNumber = "2H29NNS";

                storageId = _dl.CreateStorage(_userId, storage);
                garbage.Add(Tuple.Create(storageId, Table.Storage));

                // setup Material Object
                // grapes
                {
                    RawMaterialObject grapeMaterial = new RawMaterialObject();
                    grapeMaterial.RawMaterialName = "Grapes";
                    grapeMaterial.MaterialCategoryID = (int)Persistence.BusinessLogicEnums.ProductionReportMaterialCategory.Fruit;
                    grapeMaterial.UnitType = "lb";
                    grapeMaterial.UnitTypeId = 2;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Fermentable = true;
                    grapeMaterial.PurchaseMaterialTypes = materialBoolTypes;

                    grapeMaterialId = _dl.CreateRawMaterial(_userId, grapeMaterial);

                    garbage.Add(Tuple.Create(grapeMaterialId, Table.MaterialDict));
                }

                #endregion

                #region Purchase
                // create Purchase Record (minimal required fields)
                PurchaseObject purchO = new PurchaseObject();
                purchO.PurBatchName = "Riesling Grapes ";
                purchO.PurchaseType = "Fermentable";
                purchO.PurchaseDate = new DateTime(2018, 01, 03);
                purchO.Quantity = 0f;
                purchO.VolumeByWeight = 2000f;
                purchO.RecordId = grapeMaterialId;
                purchO.Price = 350f;
                purchO.VendorId = vendorId;

                List<StorageObject> storageList = new List<StorageObject>();
                StorageObject storageObject = new StorageObject();
                storageObject.StorageId = storageId;
                storageList.Add(storageObject);
                purchO.Storage = storageList;

                purchaseId = _dl.CreatePurchase(purchO, _userId);
                garbage.Add(Tuple.Create(purchaseId, Table.Purchase));

                #endregion

                #region Production
                // create Fermented record
                ProductionObject prodO = new ProductionObject();
                prodO.BatchName = "Riesling Wine";
                prodO.ProductionDate = new DateTime(2018, 01, 15);
                prodO.ProductionStart = new DateTime(2018, 01, 15);
                prodO.ProductionEnd = new DateTime(2018, 01, 15);
                prodO.Gauged = true;
                prodO.ProductionType = "Fermentation";
                prodO.ProductionTypeId = 1;
                prodO.Quantity = 150f; // 150 gallons of wine
                prodO.VolumeByWeight = 0f;
                prodO.AlcoholContent = 10f; // %
                prodO.ProofGallon = 30f; // pfg
                prodO.Storage = storageList; // we are using the same storage id as we use for Purchase to keep things simple
                prodO.SpiritTypeReportingID = 11; // Wine
                prodO.MaterialKindReportingID = 0;

                List<ObjInfo4Burndwn> usedMats = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat = new ObjInfo4Burndwn();
                uMat.ID = purchaseId;
                uMat.OldVal = 0f;
                uMat.NewVal = purchO.VolumeByWeight;
                uMat.DistillableOrigin = "pur";
                uMat.BurningDownMethod = "weight";

                usedMats.Add(uMat);

                prodO.UsedMats = usedMats;

                productionId = _dl.CreateProduction(prodO, _userId);

                garbage.Add(Tuple.Create(productionId, Table.Production));

                // create Production Distillation Record and mark it as Gauged
                ProductionObject prodO1 = new ProductionObject();
                prodO1.BatchName = "DistilRunAndGauged";
                prodO1.ProductionDate = new DateTime(2018, 01, 20);
                prodO1.ProductionStart = new DateTime(2018, 01, 20);
                prodO1.ProductionEnd = new DateTime(2018, 01, 20);
                prodO1.SpiritCutId = 11; // mixed
                prodO1.Gauged = true;
                prodO1.ProductionType = "Distillation";
                prodO1.ProductionTypeId = 2;
                prodO1.Quantity = 50f;
                prodO1.VolumeByWeight = 0f;
                prodO1.AlcoholContent = 50f;
                prodO1.ProofGallon = 50f;
                prodO1.Storage = storageList; // we are using the same storage id as we use for Purchase to keep things simple
                prodO1.SpiritTypeReportingID = 3; // Brandy 170-
                prodO1.MaterialKindReportingID = 94; // grape brandy

                List<ObjInfo4Burndwn> usedMats1 = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat1 = new ObjInfo4Burndwn();
                uMat1.ID = productionId;
                uMat1.OldVal = prodO.Quantity / 2;
                uMat1.NewVal = prodO.Quantity / 2;
                uMat1.Proof = prodO.ProofGallon / 2;
                uMat1.DistillableOrigin = "prod";
                uMat1.BurningDownMethod = "volume";

                usedMats1.Add(uMat1);

                prodO1.UsedMats = usedMats1;

                productionId = _dl.CreateProduction(prodO1, _userId);

                garbage.Add(Tuple.Create(productionId, Table.Production));

                #endregion

                #region Reports

                // Report Header
                ReportHeader reportHeaderE = new ReportHeader();
                reportHeaderE.ProprietorName = "Test Distillery";
                reportHeaderE.EIN = "12-3456789";
                reportHeaderE.ReportDate = "January 2018";
                reportHeaderE.PlantAddress = "123 Cognac Drive Renton WASHINGTON 98059";
                reportHeaderE.DSP = "DSP-WA-21086";

                StorageReport expectedStorageReport = new StorageReport();

                List<StorageReportCategory> reportList = new List<StorageReportCategory>();

                StorageReportCategory expectedStorageReportBodyWine = new StorageReportCategory();
                expectedStorageReportBodyWine.CategoryName = "Wine";
                expectedStorageReportBodyWine.r17_TransferredToProcessingAccount = 0f;
                expectedStorageReportBodyWine.r18_TransferredToProductionAccount = 15f;
                expectedStorageReportBodyWine.r19_TransferredToOtherBondedPremises = 0;
                expectedStorageReportBodyWine.r1_OnHandFirstOfMonth = 0f;
                expectedStorageReportBodyWine.r20_Destroyed = 0f;
                expectedStorageReportBodyWine.r22_OtherLosses = 0f;
                expectedStorageReportBodyWine.r23_OnHandEndOfMonth = 15f;
                expectedStorageReportBodyWine.r24_Lines7Through23 = 30f;
                expectedStorageReportBodyWine.r2_DepositedInBulkStorage = 30f;
                expectedStorageReportBodyWine.r4_ReturnedToBulkStorage = 0f;
                expectedStorageReportBodyWine.r6_TotalLines1Through5 = 30f;
                expectedStorageReportBodyWine.r7_TaxPaid = 0f;

                reportList.Add(expectedStorageReportBodyWine);

                StorageReportCategory expectedStorageReportBodyBrandy = new StorageReportCategory();
                expectedStorageReportBodyBrandy.CategoryName = "BrandyUnder170";
                expectedStorageReportBodyBrandy.r17_TransferredToProcessingAccount = 0f;
                expectedStorageReportBodyBrandy.r18_TransferredToProductionAccount = 0f;
                expectedStorageReportBodyBrandy.r19_TransferredToOtherBondedPremises = 0;
                expectedStorageReportBodyBrandy.r1_OnHandFirstOfMonth = 0f;
                expectedStorageReportBodyBrandy.r20_Destroyed = 0f;
                expectedStorageReportBodyBrandy.r22_OtherLosses = 0f;
                expectedStorageReportBodyBrandy.r23_OnHandEndOfMonth = 50f;
                expectedStorageReportBodyBrandy.r24_Lines7Through23 = 50f;
                expectedStorageReportBodyBrandy.r2_DepositedInBulkStorage = 50f;
                expectedStorageReportBodyBrandy.r4_ReturnedToBulkStorage = 0f;
                expectedStorageReportBodyBrandy.r6_TotalLines1Through5 = 50f;
                expectedStorageReportBodyBrandy.r7_TaxPaid = 0f;

                reportList.Add(expectedStorageReportBodyBrandy);

                expectedStorageReport.ReportBody = reportList;

                StorageReport actualStorageReport = new StorageReport();

                //get actual storage data
                actualStorageReport = _dl.GetStorageReportData(start, end, _userId);

                // verify Storage report Header
                Assert.AreEqual(reportHeaderE.DSP, actualStorageReport.Header.DSP);
                Assert.AreEqual(reportHeaderE.EIN, actualStorageReport.Header.EIN);
                Assert.AreEqual(reportHeaderE.PlantAddress, actualStorageReport.Header.PlantAddress);
                Assert.AreEqual(reportHeaderE.ProprietorName, actualStorageReport.Header.ProprietorName);
                Assert.AreEqual(reportHeaderE.ReportDate, actualStorageReport.Header.ReportDate);

                Assert.AreEqual(2, actualStorageReport.ReportBody.Count); // make sure number of spirit entries (columns) in the report is two

                var wineColumn = actualStorageReport.ReportBody.Find(x => x.CategoryName == "Wine"); // wine should be there
                var brandyUnder170Column = actualStorageReport.ReportBody.Find(x => x.CategoryName == "BrandyUnder170"); // brandy should be there

                Assert.IsNotNull(wineColumn);
                Assert.IsNotNull(brandyUnder170Column);

                // now, let's compare cells in the report for Wine column
                Assert.AreEqual(expectedStorageReportBodyWine.CategoryName, wineColumn.CategoryName);
                Assert.AreEqual(expectedStorageReportBodyWine.r17_TransferredToProcessingAccount, wineColumn.r17_TransferredToProcessingAccount);
                Assert.AreEqual(expectedStorageReportBodyWine.r18_TransferredToProductionAccount, wineColumn.r18_TransferredToProductionAccount);
                Assert.AreEqual(expectedStorageReportBodyWine.r19_TransferredToOtherBondedPremises, wineColumn.r19_TransferredToOtherBondedPremises);
                Assert.AreEqual(expectedStorageReportBodyWine.r1_OnHandFirstOfMonth, wineColumn.r1_OnHandFirstOfMonth);
                Assert.AreEqual(expectedStorageReportBodyWine.r20_Destroyed, wineColumn.r20_Destroyed);
                Assert.AreEqual(expectedStorageReportBodyWine.r22_OtherLosses, wineColumn.r22_OtherLosses);
                Assert.AreEqual(expectedStorageReportBodyWine.r23_OnHandEndOfMonth, wineColumn.r23_OnHandEndOfMonth);
                Assert.AreEqual(expectedStorageReportBodyWine.r24_Lines7Through23, wineColumn.r24_Lines7Through23);
                Assert.AreEqual(expectedStorageReportBodyWine.r2_DepositedInBulkStorage, wineColumn.r2_DepositedInBulkStorage);
                Assert.AreEqual(expectedStorageReportBodyWine.r4_ReturnedToBulkStorage, wineColumn.r4_ReturnedToBulkStorage);
                Assert.AreEqual(expectedStorageReportBodyWine.r6_TotalLines1Through5, wineColumn.r6_TotalLines1Through5);
                Assert.AreEqual(expectedStorageReportBodyWine.r7_TaxPaid, wineColumn.r7_TaxPaid);

                // now, let's compare cells in the report for Brandy Under170 column
                Assert.AreEqual(expectedStorageReportBodyBrandy.CategoryName, brandyUnder170Column.CategoryName);
                Assert.AreEqual(expectedStorageReportBodyBrandy.r17_TransferredToProcessingAccount, brandyUnder170Column.r17_TransferredToProcessingAccount);
                Assert.AreEqual(expectedStorageReportBodyBrandy.r18_TransferredToProductionAccount, brandyUnder170Column.r18_TransferredToProductionAccount);
                Assert.AreEqual(expectedStorageReportBodyBrandy.r19_TransferredToOtherBondedPremises, brandyUnder170Column.r19_TransferredToOtherBondedPremises);
                Assert.AreEqual(expectedStorageReportBodyBrandy.r1_OnHandFirstOfMonth, brandyUnder170Column.r1_OnHandFirstOfMonth);
                Assert.AreEqual(expectedStorageReportBodyBrandy.r20_Destroyed, brandyUnder170Column.r20_Destroyed);
                Assert.AreEqual(expectedStorageReportBodyBrandy.r22_OtherLosses, brandyUnder170Column.r22_OtherLosses);
                Assert.AreEqual(expectedStorageReportBodyBrandy.r23_OnHandEndOfMonth, brandyUnder170Column.r23_OnHandEndOfMonth);
                Assert.AreEqual(expectedStorageReportBodyBrandy.r24_Lines7Through23, brandyUnder170Column.r24_Lines7Through23);
                Assert.AreEqual(expectedStorageReportBodyBrandy.r2_DepositedInBulkStorage, brandyUnder170Column.r2_DepositedInBulkStorage);
                Assert.AreEqual(expectedStorageReportBodyBrandy.r4_ReturnedToBulkStorage, brandyUnder170Column.r4_ReturnedToBulkStorage);
                Assert.AreEqual(expectedStorageReportBodyBrandy.r6_TotalLines1Through5, brandyUnder170Column.r6_TotalLines1Through5);
                Assert.AreEqual(expectedStorageReportBodyBrandy.r7_TaxPaid, brandyUnder170Column.r7_TaxPaid);
                #endregion
            }
            finally
            {
                // Cleanup created records
                foreach (var i in garbage)
                {
                    TestRecordCleanup(i.Item1, i.Item2);
                }
            }
        }

        /// <summary>
        /// This test validates the following:
        /// Buy Grapes in January
        /// Ferment all of Grapes in January
        /// Distil all of Fermented Grapes in January as Brandy Under 170
        /// Storage report should be showing Wine as well as Brandy Under 170 columns 
        /// </summary>
        [TestMethod()]
        public void BuyGrapes_MakeWine_DistilBrandy_MakeSure_WineAndBrandyBothShowUpInStorageReport()
        {
            // Arrange
            int vendorId = 0;
            int storageId = 0;
            int grapeMaterialId = 0;
            int purchaseId = 0;
            int productionId = 0;

            // reporting time range
            DateTime start = new DateTime(2018, 01, 01);
            DateTime end = new DateTime(2018, 01, 31);

            List<Tuple<int/*recordId*/, Table/*table enum vaue*/>> tablesForCleanupTupleList = new List<Tuple<int, Table>>();

            try
            {
                //  dictionary setup
                #region Dictionary

                // setup Vendor object
                VendorObject vendor = new VendorObject();
                vendor.VendorName = "VendorTest";

                vendorId = _dl.CreateVendor(_userId, vendor);
                tablesForCleanupTupleList.Add(Tuple.Create(vendorId, Table.Vendor));

                // setup Storage Object
                StorageObject storage = new StorageObject();
                storage.StorageName = "testStorage";
                storage.SerialNumber = "2H29NNS";

                storageId = _dl.CreateStorage(_userId, storage);
                tablesForCleanupTupleList.Add(Tuple.Create(storageId, Table.Storage));

                // setup Material Object
                // grapes
                {
                    RawMaterialObject grapeMaterial = new RawMaterialObject();
                    grapeMaterial.RawMaterialName = "Grapes";
                    grapeMaterial.MaterialCategoryID = (int)Persistence.BusinessLogicEnums.ProductionReportMaterialCategory.Fruit;
                    grapeMaterial.UnitType = "lb";
                    grapeMaterial.UnitTypeId = 2;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Fermentable = true;
                    grapeMaterial.PurchaseMaterialTypes = materialBoolTypes;

                    grapeMaterialId = _dl.CreateRawMaterial(_userId, grapeMaterial);

                    tablesForCleanupTupleList.Add(Tuple.Create(grapeMaterialId, Table.MaterialDict));
                }

                #endregion

                #region Purchase
                // create Purchase Record (minimal required fields)
                PurchaseObject purchO = new PurchaseObject();
                purchO.PurBatchName = "Riesling Grapes ";
                purchO.PurchaseType = "Fermentable";
                purchO.PurchaseDate = new DateTime(2018, 01, 03);
                purchO.Quantity = 0f;
                purchO.VolumeByWeight = 2000f;
                purchO.RecordId = grapeMaterialId;
                purchO.Price = 350f;
                purchO.VendorId = vendorId;

                List<StorageObject> storageList = new List<StorageObject>();
                StorageObject storageObject = new StorageObject();
                storageObject.StorageId = storageId;
                storageList.Add(storageObject);
                purchO.Storage = storageList;

                purchaseId = _dl.CreatePurchase(purchO, _userId);
                tablesForCleanupTupleList.Add(Tuple.Create(purchaseId, Table.Purchase));

                #endregion

                #region Production
                // create Fermented record
                ProductionObject prodO = new ProductionObject();
                prodO.BatchName = "Riesling Wine";
                prodO.ProductionDate = new DateTime(2018, 01, 15);
                prodO.ProductionStart = new DateTime(2018, 01, 15);
                prodO.ProductionEnd = new DateTime(2018, 01, 15);
                prodO.Gauged = true;
                prodO.ProductionType = "Fermentation";
                prodO.ProductionTypeId = 1;
                prodO.Quantity = 150f; // 150 gallons of wine
                prodO.VolumeByWeight = 0f;
                prodO.AlcoholContent = 10f; // %
                prodO.ProofGallon = 30f; // pfg
                prodO.Storage = storageList; // we are using the same storage id as we use for Purchase to keep things simple
                prodO.SpiritTypeReportingID = 11; // Wine
                prodO.MaterialKindReportingID = 0;

                List<ObjInfo4Burndwn> usedMats = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat = new ObjInfo4Burndwn();
                uMat.ID = purchaseId;
                uMat.OldVal = 0f;
                uMat.NewVal = purchO.VolumeByWeight;
                uMat.DistillableOrigin = "pur";
                uMat.BurningDownMethod = "weight";

                usedMats.Add(uMat);

                prodO.UsedMats = usedMats;

                productionId = _dl.CreateProduction(prodO, _userId);

                tablesForCleanupTupleList.Add(Tuple.Create(productionId, Table.Production));

                // create Production Distillation Record and mark it as Gauged
                ProductionObject prodO1 = new ProductionObject();
                prodO1.BatchName = "DistilRunAndGauged";
                prodO1.ProductionDate = new DateTime(2018, 01, 20);
                prodO1.ProductionStart = new DateTime(2018, 01, 20);
                prodO1.ProductionEnd = new DateTime(2018, 01, 20);
                prodO1.SpiritCutId = 11; // mixed
                prodO1.Gauged = true;
                prodO1.ProductionType = "Distillation";
                prodO1.ProductionTypeId = 2;
                prodO1.Quantity = 50f;
                prodO1.VolumeByWeight = 0f;
                prodO1.AlcoholContent = 50f;
                prodO1.ProofGallon = 50f;
                prodO1.Storage = storageList; // we are using the same storage id as we use for Purchase to keep things simple
                prodO1.SpiritTypeReportingID = 3; // Brandy 170-
                prodO1.MaterialKindReportingID = 94; // grape brandy

                List<ObjInfo4Burndwn> usedMats1 = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat1 = new ObjInfo4Burndwn();
                uMat1.ID = productionId;
                uMat1.OldVal = 0f;
                uMat1.NewVal = prodO.Quantity;
                uMat1.DistillableOrigin = "prod";
                uMat1.BurningDownMethod = "volume";

                usedMats1.Add(uMat1);

                prodO1.UsedMats = usedMats1;

                productionId = _dl.CreateProduction(prodO1, _userId);

                tablesForCleanupTupleList.Add(Tuple.Create(productionId, Table.Production));

                #endregion

                #region Reports

                // Report Header
                ReportHeader reportHeaderE = new ReportHeader();
                reportHeaderE.ProprietorName = "Test Distillery";
                reportHeaderE.EIN = "12-3456789";
                reportHeaderE.ReportDate = "January 2018";
                reportHeaderE.PlantAddress = "123 Cognac Drive Renton WASHINGTON 98059";
                reportHeaderE.DSP = "DSP-WA-21086";

                StorageReport expectedStorageReport = new StorageReport();

                List<StorageReportCategory> reportList = new List<StorageReportCategory>();

                StorageReportCategory expectedStorageReportBodyWine = new StorageReportCategory();
                expectedStorageReportBodyWine.CategoryName = "Wine";
                expectedStorageReportBodyWine.r17_TransferredToProcessingAccount = 0f;
                expectedStorageReportBodyWine.r18_TransferredToProductionAccount = 30f;
                expectedStorageReportBodyWine.r19_TransferredToOtherBondedPremises = 0;
                expectedStorageReportBodyWine.r1_OnHandFirstOfMonth = 0f;
                expectedStorageReportBodyWine.r20_Destroyed = 0f;
                expectedStorageReportBodyWine.r22_OtherLosses = 0f;
                expectedStorageReportBodyWine.r23_OnHandEndOfMonth = 0f;
                expectedStorageReportBodyWine.r24_Lines7Through23 = 30f;
                expectedStorageReportBodyWine.r2_DepositedInBulkStorage = 30f;
                expectedStorageReportBodyWine.r4_ReturnedToBulkStorage = 0f;
                expectedStorageReportBodyWine.r6_TotalLines1Through5 = 30f;
                expectedStorageReportBodyWine.r7_TaxPaid = 0f;

                reportList.Add(expectedStorageReportBodyWine);

                StorageReportCategory expectedStorageReportBodyBrandy = new StorageReportCategory();
                expectedStorageReportBodyBrandy.CategoryName = "BrandyUnder170";
                expectedStorageReportBodyBrandy.r17_TransferredToProcessingAccount = 0f;
                expectedStorageReportBodyBrandy.r18_TransferredToProductionAccount = 0f;
                expectedStorageReportBodyBrandy.r19_TransferredToOtherBondedPremises = 0;
                expectedStorageReportBodyBrandy.r1_OnHandFirstOfMonth = 0f;
                expectedStorageReportBodyBrandy.r20_Destroyed = 0f;
                expectedStorageReportBodyBrandy.r22_OtherLosses = 0f;
                expectedStorageReportBodyBrandy.r23_OnHandEndOfMonth = 50f;
                expectedStorageReportBodyBrandy.r24_Lines7Through23 = 50f;
                expectedStorageReportBodyBrandy.r2_DepositedInBulkStorage = 50f;
                expectedStorageReportBodyBrandy.r4_ReturnedToBulkStorage = 0f;
                expectedStorageReportBodyBrandy.r6_TotalLines1Through5 = 50f;
                expectedStorageReportBodyBrandy.r7_TaxPaid = 0f;

                reportList.Add(expectedStorageReportBodyBrandy);

                expectedStorageReport.ReportBody = reportList;

                StorageReport actualStorageReport = new StorageReport();

                //get actual storage data
                actualStorageReport = _dl.GetStorageReportData(start, end, _userId);

                // verify Storage report Header
                Assert.AreEqual(reportHeaderE.DSP, actualStorageReport.Header.DSP);
                Assert.AreEqual(reportHeaderE.EIN, actualStorageReport.Header.EIN);
                Assert.AreEqual(reportHeaderE.PlantAddress, actualStorageReport.Header.PlantAddress);
                Assert.AreEqual(reportHeaderE.ProprietorName, actualStorageReport.Header.ProprietorName);
                Assert.AreEqual(reportHeaderE.ReportDate, actualStorageReport.Header.ReportDate);

                Assert.AreEqual(2, actualStorageReport.ReportBody.Count); // make sure number of spirit entries (columns) in the report is two

                var wineColumn = actualStorageReport.ReportBody.Find(x => x.CategoryName == "Wine"); // wine should be there
                var brandyUnder170Column = actualStorageReport.ReportBody.Find(x => x.CategoryName == "BrandyUnder170"); // brandy should be there

                Assert.IsNotNull(wineColumn);
                Assert.IsNotNull(brandyUnder170Column);

                // now, let's compare cells in the report for Wine column
                Assert.AreEqual(expectedStorageReportBodyWine.CategoryName, wineColumn.CategoryName);
                Assert.AreEqual(expectedStorageReportBodyWine.r17_TransferredToProcessingAccount, wineColumn.r17_TransferredToProcessingAccount);
                Assert.AreEqual(expectedStorageReportBodyWine.r18_TransferredToProductionAccount, wineColumn.r18_TransferredToProductionAccount);
                Assert.AreEqual(expectedStorageReportBodyWine.r19_TransferredToOtherBondedPremises, wineColumn.r19_TransferredToOtherBondedPremises);
                Assert.AreEqual(expectedStorageReportBodyWine.r1_OnHandFirstOfMonth, wineColumn.r1_OnHandFirstOfMonth);
                Assert.AreEqual(expectedStorageReportBodyWine.r20_Destroyed, wineColumn.r20_Destroyed);
                Assert.AreEqual(expectedStorageReportBodyWine.r22_OtherLosses, wineColumn.r22_OtherLosses);
                Assert.AreEqual(expectedStorageReportBodyWine.r23_OnHandEndOfMonth, wineColumn.r23_OnHandEndOfMonth);
                Assert.AreEqual(expectedStorageReportBodyWine.r24_Lines7Through23, wineColumn.r24_Lines7Through23);
                Assert.AreEqual(expectedStorageReportBodyWine.r2_DepositedInBulkStorage, wineColumn.r2_DepositedInBulkStorage);
                Assert.AreEqual(expectedStorageReportBodyWine.r4_ReturnedToBulkStorage, wineColumn.r4_ReturnedToBulkStorage);
                Assert.AreEqual(expectedStorageReportBodyWine.r6_TotalLines1Through5, wineColumn.r6_TotalLines1Through5);
                Assert.AreEqual(expectedStorageReportBodyWine.r7_TaxPaid, wineColumn.r7_TaxPaid);

                // now, let's compare cells in the report for Brandy Under170 column
                Assert.AreEqual(expectedStorageReportBodyBrandy.CategoryName, brandyUnder170Column.CategoryName);
                Assert.AreEqual(expectedStorageReportBodyBrandy.r17_TransferredToProcessingAccount, brandyUnder170Column.r17_TransferredToProcessingAccount);
                Assert.AreEqual(expectedStorageReportBodyBrandy.r18_TransferredToProductionAccount, brandyUnder170Column.r18_TransferredToProductionAccount);
                Assert.AreEqual(expectedStorageReportBodyBrandy.r19_TransferredToOtherBondedPremises, brandyUnder170Column.r19_TransferredToOtherBondedPremises);
                Assert.AreEqual(expectedStorageReportBodyBrandy.r1_OnHandFirstOfMonth, brandyUnder170Column.r1_OnHandFirstOfMonth);
                Assert.AreEqual(expectedStorageReportBodyBrandy.r20_Destroyed, brandyUnder170Column.r20_Destroyed);
                Assert.AreEqual(expectedStorageReportBodyBrandy.r22_OtherLosses, brandyUnder170Column.r22_OtherLosses);
                Assert.AreEqual(expectedStorageReportBodyBrandy.r23_OnHandEndOfMonth, brandyUnder170Column.r23_OnHandEndOfMonth);
                Assert.AreEqual(expectedStorageReportBodyBrandy.r24_Lines7Through23, brandyUnder170Column.r24_Lines7Through23);
                Assert.AreEqual(expectedStorageReportBodyBrandy.r2_DepositedInBulkStorage, brandyUnder170Column.r2_DepositedInBulkStorage);
                Assert.AreEqual(expectedStorageReportBodyBrandy.r4_ReturnedToBulkStorage, brandyUnder170Column.r4_ReturnedToBulkStorage);
                Assert.AreEqual(expectedStorageReportBodyBrandy.r6_TotalLines1Through5, brandyUnder170Column.r6_TotalLines1Through5);
                Assert.AreEqual(expectedStorageReportBodyBrandy.r7_TaxPaid, brandyUnder170Column.r7_TaxPaid);
                #endregion
            }
            finally
            {
                // Cleanup created records
                foreach (var i in tablesForCleanupTupleList)
                {
                    TestRecordCleanup(i.Item1, i.Item2);
                }
            }
        }

        [TestMethod()]
        public void CreateSpiritTest()
        {
            // Arrange
            SpiritObject spirit = new SpiritObject();
            spirit.SpiritName = Guid.NewGuid().ToString();
            spirit.Note = Guid.NewGuid().ToString();
            spirit.ProcessingReportTypeID = 1;

            //Act
            int result = _dl.CreateSpirit(_userId, spirit);

            // Assert
            Assert.AreNotEqual(0, result);

            // Cleanup
            TestRecordCleanup(result, Table.Spirit);
        }

        [TestMethod()]
        public void Create_Vendor_W_All_Fields_Test()
        {
            // Arrange
            VendorObject vendor = new VendorObject();
            vendor.VendorName = Guid.NewGuid().ToString();
            vendor.Note = Guid.NewGuid().ToString();

            //Act
            int result = _dl.CreateVendor(_userId, vendor);

            // Assert
            Assert.AreNotEqual(0, result);

            // Cleanup
            TestRecordCleanup(result, Table.Vendor);
        }

        [TestMethod()]
        public void Create_Wine_Material_W_All_Fields()
        {
            // Arrange
            RawMaterialObject wineMaterial = new RawMaterialObject();
            wineMaterial.RawMaterialName = "Wine For Brandy";
            wineMaterial.MaterialCategoryID = 2;
            wineMaterial.UnitType = "gal";
            wineMaterial.UnitTypeId = 1;
            PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
            materialBoolTypes.Fermented = true;
            wineMaterial.PurchaseMaterialTypes = materialBoolTypes;

            //Act
            int result = _dl.CreateRawMaterial(_userId, wineMaterial);

            // Assert
            Assert.AreNotEqual(0, result);

            // Cleanup
            TestRecordCleanup(result, Table.Spirit);
        }

        [TestMethod()]
        public void Create_Water_Material_W_All_Fields()
        {
            // Arrange
            RawMaterialObject waterMaterial = new RawMaterialObject();
            waterMaterial.RawMaterialName = "Water";
            waterMaterial.UnitType = "gal";
            waterMaterial.UnitTypeId = 1;
            PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
            materialBoolTypes.Additive = true;
            waterMaterial.PurchaseMaterialTypes = materialBoolTypes;

            //Act
            int result = _dl.CreateRawMaterial(_userId, waterMaterial);

            // Assert
            Assert.AreNotEqual(0, result);

            // Cleanup
            TestRecordCleanup(result, Table.MaterialDict);
        }

        [TestMethod()]
        public void CreateMaterialDistilledTest()
        {
            // Arrange
            RawMaterialObject distilledMaterial = new RawMaterialObject();
            distilledMaterial.RawMaterialName = "Distilled Product";
            distilledMaterial.UnitType = "gal";
            distilledMaterial.UnitTypeId = 1;
            PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
            materialBoolTypes.Distilled = true;
            distilledMaterial.PurchaseMaterialTypes = materialBoolTypes;

            // Act
            int result = _dl.CreateRawMaterial(_userId, distilledMaterial);
            var materialList = _dl.GetRawMaterialListDict(_userId);
            var dbResult = materialList.Single(m => m.RawMaterialId == result);
            // Assert
            Assert.AreNotEqual(0, result);
            Assert.AreEqual(dbResult.RawMaterialName, distilledMaterial.RawMaterialName);
            Assert.AreEqual(dbResult.UnitType, distilledMaterial.UnitType);
            Assert.AreEqual(dbResult.UnitTypeId, distilledMaterial.UnitTypeId);
            Assert.AreEqual(dbResult.PurchaseMaterialTypes.Distilled, distilledMaterial.PurchaseMaterialTypes.Distilled);
            Assert.AreEqual(dbResult.PurchaseMaterialTypes.Additive, distilledMaterial.PurchaseMaterialTypes.Additive);
            Assert.AreEqual(dbResult.PurchaseMaterialTypes.Fermentable, distilledMaterial.PurchaseMaterialTypes.Fermentable);
            Assert.AreEqual(dbResult.PurchaseMaterialTypes.Fermented, distilledMaterial.PurchaseMaterialTypes.Fermented);
            Assert.AreEqual(dbResult.PurchaseMaterialTypes.Supply, distilledMaterial.PurchaseMaterialTypes.Supply);
            // Cleanup
            TestRecordCleanup(result, Table.MaterialDict);
        }

        /// <summary>
        /// This test checks that all relevant records in all tables are deleted 
        /// when Fermentable Purchase record is deleted
        /// </summary>
        [TestMethod()]
        public void Delete_Purchase_Fermentable_Record_Test()
        {
            // Arrange
            int vendorId = 0;
            int storageId = 0;
            int grapeMaterialId = 0;
            int purchaseId = 0;

            List<Tuple<int/*recordId*/, Table/*table enum vaue*/>> tablesForCleanupTupleList = new List<Tuple<int, Table>>();

            try
            {
                //  dictionary setup
                #region Dictionary

                // setup Vendor object
                VendorObject vendor = new VendorObject();
                vendor.VendorName = "VendorTest";

                vendorId = _dl.CreateVendor(_userId, vendor);
                tablesForCleanupTupleList.Add(Tuple.Create(vendorId, Table.Vendor));

                // setup Storage Object
                StorageObject storage = new StorageObject();
                storage.StorageName = "testStorage";
                storage.SerialNumber = "2H29NNS";

                storageId = _dl.CreateStorage(_userId, storage);
                tablesForCleanupTupleList.Add(Tuple.Create(storageId, Table.Storage));

                // setup Material Object
                // grapes
                {
                    RawMaterialObject grapeMaterial = new RawMaterialObject();
                    grapeMaterial.RawMaterialName = "Grapes";
                    grapeMaterial.MaterialCategoryID = (int)Persistence.BusinessLogicEnums.ProductionReportMaterialCategory.Fruit;
                    grapeMaterial.UnitType = "lb";
                    grapeMaterial.UnitTypeId = 2;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Fermentable = true;
                    grapeMaterial.PurchaseMaterialTypes = materialBoolTypes;

                    grapeMaterialId = _dl.CreateRawMaterial(_userId, grapeMaterial);

                    tablesForCleanupTupleList.Add(Tuple.Create(grapeMaterialId, Table.MaterialDict));
                }

                #endregion

                #region Purchase
                // create Purchase Record (minimal required fields)
                PurchaseObject purchO = new PurchaseObject();
                purchO.PurBatchName = "Riesling Grapes ";
                purchO.PurchaseType = "Fermentable";
                purchO.PurchaseDate = new DateTime(2018, 01, 03);
                purchO.Quantity = 0f;
                purchO.VolumeByWeight = 2000f;
                purchO.RecordId = grapeMaterialId;
                purchO.Price = 350f;
                purchO.VendorId = vendorId;

                List<StorageObject> storageList = new List<StorageObject>();
                StorageObject storageObject = new StorageObject();
                storageObject.StorageId = storageId;
                storageList.Add(storageObject);
                purchO.Storage = storageList;

                purchaseId = _dl.CreatePurchase(purchO, _userId);
                tablesForCleanupTupleList.Add(Tuple.Create(purchaseId, Table.Purchase));

                _dl.DeletePurchase(purchO, _userId);

                var purchaseList = _dl.GetPurchasesList(purchO.PurchaseType, _userId);

                var purchaseFound = purchaseList.Find(x => x.PurchaseId == purchaseId);

                Assert.IsNull(purchaseFound);

                #endregion
            }
            finally
            {
                // Cleanup created records
                foreach (var i in tablesForCleanupTupleList)
                {
                    TestRecordCleanup(i.Item1, i.Item2);
                }
            }
        }

        /// <summary>
        /// This test checks that all relevant records in all tables are deleted 
        /// when Fermented Purchase record is deleted
        /// </summary>
        [TestMethod()]
        public void Delete_Purchase_Fermented_Record_Test()
        {
            // Arrange
            int vendorId = 0;
            int storageId = 0;
            int wineMaterialId = 0;
            int purchaseId = 0;

            List<Tuple<int/*recordId*/, Table/*table enum vaue*/>> tablesForCleanupTupleList = new List<Tuple<int, Table>>();

            try
            {
                //  dictionary setup
                #region Dictionary

                // setup Vendor object
                VendorObject vendor = new VendorObject();
                vendor.VendorName = "VendorTest";

                vendorId = _dl.CreateVendor(_userId, vendor);
                tablesForCleanupTupleList.Add(Tuple.Create(vendorId, Table.Vendor));

                // setup Storage Object
                StorageObject storage = new StorageObject();
                storage.StorageName = "testStorage";
                storage.SerialNumber = "2H29NNS";

                storageId = _dl.CreateStorage(_userId, storage);
                tablesForCleanupTupleList.Add(Tuple.Create(storageId, Table.Storage));

                // setup Material Object
                {
                    RawMaterialObject wineMaterial = new RawMaterialObject();
                    wineMaterial.RawMaterialName = "Wine";
                    wineMaterial.MaterialCategoryID = (int)Persistence.BusinessLogicEnums.ProductionReportMaterialCategory.Fruit;
                    wineMaterial.UnitType = "gal";
                    wineMaterial.UnitTypeId = 1;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Fermented = true;
                    wineMaterial.PurchaseMaterialTypes = materialBoolTypes;

                    wineMaterialId = _dl.CreateRawMaterial(_userId, wineMaterial);

                    tablesForCleanupTupleList.Add(Tuple.Create(wineMaterialId, Table.MaterialDict));
                }

                #endregion

                #region Purchase
                // create Purchase Record (minimal required fields)
                PurchaseObject purchO = new PurchaseObject();
                purchO.PurBatchName = "Riesling Wine ";
                purchO.PurchaseType = "Fermented";
                purchO.PurchaseDate = new DateTime(2018, 01, 03);
                purchO.Quantity = 2000f;
                purchO.VolumeByWeight = 0f;
                purchO.RecordId = wineMaterialId;
                purchO.Price = 350f;
                purchO.VendorId = vendorId;

                List<StorageObject> storageList = new List<StorageObject>();
                StorageObject storageObject = new StorageObject();
                storageObject.StorageId = storageId;
                storageList.Add(storageObject);
                purchO.Storage = storageList;

                purchaseId = _dl.CreatePurchase(purchO, _userId);
                tablesForCleanupTupleList.Add(Tuple.Create(purchaseId, Table.Purchase));

                _dl.DeletePurchase(purchO, _userId);

                var purchaseList = _dl.GetPurchasesList(purchO.PurchaseType, _userId);

                var purchaseFound = purchaseList.Find(x => x.PurchaseId == purchaseId);

                Assert.IsNull(purchaseFound);

                #endregion
            }
            finally
            {
                // Cleanup created records
                foreach (var i in tablesForCleanupTupleList)
                {
                    TestRecordCleanup(i.Item1, i.Item2);
                }
            }
        }

        /// <summary>
        /// This test checks that all relevant records in all tables are deleted 
        /// when Distilled Purchase record is deleted
        /// </summary>
        [TestMethod()]
        public void Delete_Purchase_Distilled_Record_Test()
        {
            // Arrange
            int vendorId = 0;
            int storageId = 0;
            int distilledMaterialId = 0;
            int purchaseId = 0;

            List<Tuple<int/*recordId*/, Table/*table enum vaue*/>> tablesForCleanupTupleList = new List<Tuple<int, Table>>();

            try
            {
                //  dictionary setup
                #region Dictionary

                // setup Vendor object
                VendorObject vendor = new VendorObject();
                vendor.VendorName = "VendorTest";

                vendorId = _dl.CreateVendor(_userId, vendor);
                tablesForCleanupTupleList.Add(Tuple.Create(vendorId, Table.Vendor));

                // setup Storage Object
                StorageObject storage = new StorageObject();
                storage.StorageName = "testStorage";
                storage.SerialNumber = "2H29NNS";

                storageId = _dl.CreateStorage(_userId, storage);
                tablesForCleanupTupleList.Add(Tuple.Create(storageId, Table.Storage));

                // setup Material Object
                {
                    RawMaterialObject gnsMaterial = new RawMaterialObject();
                    gnsMaterial.RawMaterialName = "GNS";
                    gnsMaterial.UnitType = "gal";
                    gnsMaterial.UnitTypeId = 1;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Distilled = true;
                    gnsMaterial.PurchaseMaterialTypes = materialBoolTypes;

                    distilledMaterialId = _dl.CreateRawMaterial(_userId, gnsMaterial);

                    tablesForCleanupTupleList.Add(Tuple.Create(distilledMaterialId, Table.MaterialDict));
                }

                #endregion

                #region Purchase
                // create Purchase Record (minimal required fields)
                PurchaseObject purchO = new PurchaseObject();
                purchO.PurBatchName = "GNS ";
                purchO.PurchaseType = "Distilled";
                purchO.PurchaseDate = new DateTime(2018, 01, 03);
                purchO.Quantity = 2000f;
                purchO.VolumeByWeight = 0f;
                purchO.ProofGallon = 96f;
                purchO.RecordId = distilledMaterialId;
                purchO.Price = 3500f;
                purchO.VendorId = vendorId;

                List<StorageObject> storageList = new List<StorageObject>();
                StorageObject storageObject = new StorageObject();
                storageObject.StorageId = storageId;
                storageList.Add(storageObject);
                purchO.Storage = storageList;

                purchaseId = _dl.CreatePurchase(purchO, _userId);
                tablesForCleanupTupleList.Add(Tuple.Create(purchaseId, Table.Purchase));

                _dl.DeletePurchase(purchO, _userId);

                var purchaseList = _dl.GetPurchasesList(purchO.PurchaseType, _userId);

                var purchaseFound = purchaseList.Find(x => x.PurchaseId == purchaseId);

                Assert.IsNull(purchaseFound);

                #endregion
            }
            finally
            {
                // Cleanup created records
                foreach (var i in tablesForCleanupTupleList)
                {
                    TestRecordCleanup(i.Item1, i.Item2);
                }
            }
        }

        /// <summary>
        /// This test checks that all relevant records in all tables are deleted 
        /// when Additive Purchase record is deleted
        /// </summary>
        [TestMethod()]
        public void Delete_Purchase_Additive_Record_Test()
        {
            // Arrange
            int vendorId = 0;
            int storageId = 0;
            int additiveMaterialId = 0;
            int purchaseId = 0;

            List<Tuple<int/*recordId*/, Table/*table enum vaue*/>> tablesForCleanupTupleList = new List<Tuple<int, Table>>();

            try
            {
                //  dictionary setup
                #region Dictionary

                // setup Vendor object
                VendorObject vendor = new VendorObject();
                vendor.VendorName = "VendorTest";

                vendorId = _dl.CreateVendor(_userId, vendor);
                tablesForCleanupTupleList.Add(Tuple.Create(vendorId, Table.Vendor));

                // setup Storage Object
                StorageObject storage = new StorageObject();
                storage.StorageName = "testStorage";
                storage.SerialNumber = "2H29NNS";

                storageId = _dl.CreateStorage(_userId, storage);
                tablesForCleanupTupleList.Add(Tuple.Create(storageId, Table.Storage));

                // setup Material Object
                {
                    RawMaterialObject additiveMaterial = new RawMaterialObject();
                    additiveMaterial.RawMaterialName = "Honey";
                    additiveMaterial.UnitType = "lb";
                    additiveMaterial.UnitTypeId = 2;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Additive = true;
                    additiveMaterial.PurchaseMaterialTypes = materialBoolTypes;

                    additiveMaterialId = _dl.CreateRawMaterial(_userId, additiveMaterial);

                    tablesForCleanupTupleList.Add(Tuple.Create(additiveMaterialId, Table.MaterialDict));
                }

                #endregion

                #region Purchase
                // create Purchase Record (minimal required fields)
                PurchaseObject purchO = new PurchaseObject();
                purchO.PurBatchName = "Honey";
                purchO.PurchaseType = "Additive";
                purchO.PurchaseDate = new DateTime(2018, 01, 03);
                purchO.Quantity = 0f;
                purchO.VolumeByWeight = 500f;
                purchO.ProofGallon = 0f;
                purchO.RecordId = additiveMaterialId;
                purchO.Price = 3500f;
                purchO.VendorId = vendorId;

                List<StorageObject> storageList = new List<StorageObject>();
                StorageObject storageObject = new StorageObject();
                storageObject.StorageId = storageId;
                storageList.Add(storageObject);
                purchO.Storage = storageList;

                purchaseId = _dl.CreatePurchase(purchO, _userId);
                tablesForCleanupTupleList.Add(Tuple.Create(purchaseId, Table.Purchase));

                _dl.DeletePurchase(purchO, _userId);

                var purchaseList = _dl.GetPurchasesList(purchO.PurchaseType, _userId);

                var purchaseFound = purchaseList.Find(x => x.PurchaseId == purchaseId);

                Assert.IsNull(purchaseFound);

                #endregion
            }
            finally
            {
                // Cleanup created records
                foreach (var i in tablesForCleanupTupleList)
                {
                    TestRecordCleanup(i.Item1, i.Item2);
                }
            }
        }

        /// <summary>
        /// This test checks that all relevant records in all tables are deleted 
        /// when Supply Purchase record is deleted
        /// </summary>
        [TestMethod()]
        public void Delete_Purchase_Supply_Record_Test()
        {
            // Arrange
            int vendorId = 0;
            int storageId = 0;
            int supplyMaterialId = 0;
            int purchaseId = 0;

            List<Tuple<int/*recordId*/, Table/*table enum vaue*/>> tablesForCleanupTupleList = new List<Tuple<int, Table>>();

            try
            {
                //  dictionary setup
                #region Dictionary

                // setup Vendor object
                VendorObject vendor = new VendorObject();
                vendor.VendorName = "VendorTest";

                vendorId = _dl.CreateVendor(_userId, vendor);
                tablesForCleanupTupleList.Add(Tuple.Create(vendorId, Table.Vendor));

                // setup Storage Object
                StorageObject storage = new StorageObject();
                storage.StorageName = "testStorage";
                storage.SerialNumber = "2H29NNS";

                storageId = _dl.CreateStorage(_userId, storage);
                tablesForCleanupTupleList.Add(Tuple.Create(storageId, Table.Storage));

                // setup Material Object
                {
                    RawMaterialObject additiveMaterial = new RawMaterialObject();
                    additiveMaterial.RawMaterialName = "Bottles";
                    additiveMaterial.UnitType = "pc";
                    additiveMaterial.UnitTypeId = 7;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Supply = true;
                    additiveMaterial.PurchaseMaterialTypes = materialBoolTypes;

                    supplyMaterialId = _dl.CreateRawMaterial(_userId, additiveMaterial);

                    tablesForCleanupTupleList.Add(Tuple.Create(supplyMaterialId, Table.MaterialDict));
                }

                #endregion

                #region Purchase
                // create Purchase Record (minimal required fields)
                PurchaseObject purchO = new PurchaseObject();
                purchO.PurBatchName = "Packaging Bottles";
                purchO.PurchaseType = "Supply";
                purchO.PurchaseDate = new DateTime(2018, 01, 03);
                purchO.Quantity = 200f;
                purchO.VolumeByWeight = 0f;
                purchO.ProofGallon = 0f;
                purchO.RecordId = supplyMaterialId;
                purchO.Price = 500f;
                purchO.VendorId = vendorId;

                List<StorageObject> storageList = new List<StorageObject>();
                StorageObject storageObject = new StorageObject();
                storageObject.StorageId = storageId;
                storageList.Add(storageObject);
                purchO.Storage = storageList;

                purchaseId = _dl.CreatePurchase(purchO, _userId);
                tablesForCleanupTupleList.Add(Tuple.Create(purchaseId, Table.Purchase));

                _dl.DeletePurchase(purchO, _userId);

                var purchaseList = _dl.GetPurchasesList(purchO.PurchaseType, _userId);

                var purchaseFound = purchaseList.Find(x => x.PurchaseId == purchaseId);

                Assert.IsNull(purchaseFound);

                #endregion
            }
            finally
            {
                // Cleanup created records
                foreach (var i in tablesForCleanupTupleList)
                {
                    TestRecordCleanup(i.Item1, i.Item2);
                }
            }
        }

        [TestMethod()]
        public void DeleteProductionTest()
        {
            List<Tuple<int, Table>> cleanupList = new List<Tuple<int, Table>>();
            int storageId = 0;
            int materialId = 0;
            int purchaseId = 0;
            int productionId = 0;
            int vendorId = 0;

            try
            {
                // Storage
                StorageObject storage = new StorageObject()
                {
                    StorageName = "DeleteProductionTest_Storage",
                    SerialNumber = "2H29NNS"
                };

                storageId = _dl.CreateStorage(_userId, storage);
                cleanupList.Add(Tuple.Create(storageId, Table.Storage));

                // Vendor
                VendorObject vendor = new VendorObject();
                vendor.VendorName = "DeleteProductionTest_Vendor";

                vendorId = _dl.CreateVendor(_userId, vendor);
                cleanupList.Add(Tuple.Create(vendorId, Table.Vendor));

                // Raw material
                RawMaterialObject material = new RawMaterialObject();
                material.RawMaterialName = "DeleteProductionTest_RawMaterial";
                material.MaterialCategoryID = 2;
                material.UnitType = "gal";
                material.UnitTypeId = 1;
                PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                materialBoolTypes.Fermented = true;
                material.PurchaseMaterialTypes = materialBoolTypes;

                materialId = _dl.CreateRawMaterial(_userId, material);
                cleanupList.Add(Tuple.Create(materialId, Table.MaterialDict));

                // Purchase
                PurchaseObject purchase = new PurchaseObject();
                purchase.PurBatchName = "DeleteProductionTest_Purchase";
                purchase.PurchaseType = "Fermented";
                purchase.PurchaseDate = new DateTime(2017, 09, 1);
                purchase.Quantity = 1000f; // 100 gallons
                purchase.VolumeByWeight = 0f;
                purchase.AlcoholContent = 10f;
                purchase.ProofGallon = 100f;
                purchase.RecordId = materialId;
                purchase.Price = 2500f;
                purchase.VendorId = vendorId;

                purchaseId = _dl.CreatePurchase(purchase, _userId);
                cleanupList.Add(Tuple.Create(purchaseId, Table.Purchase));

                // Production
                ProductionObject prodObject = new ProductionObject
                {
                    AlcoholContent = 39f,
                    BatchName = "TEST",
                    Gauged = true,
                    MaterialKindReportingID = 92,
                    ProductionDate = new DateTime(2018, 1, 1),
                    ProductionEnd = new DateTime(2018, 1, 1),
                    ProductionStart = new DateTime(2018, 1, 1),
                    ProductionType = "Distillation",
                    ProofGallon = 78f,
                    Quantity = 100f,
                    SpiritCutId = 9,
                    SpiritTypeReportingID = 2,
                    Storage = new List<StorageObject>
                    {
                        new StorageObject
                        {
                            StorageId = storageId,
                            StorageName = "DeleteProductionTestStorage"
                        }
                    },
                    UsedMats = new List<ObjInfo4Burndwn> {
                        new ObjInfo4Burndwn
                        {
                            ID = purchaseId,
                            BurningDownMethod = "volume",
                            DistillableOrigin = "pur",
                            NewVal = 100,
                            OldVal = 0,
                            Proof = 0
                        }
                    }
                };

                productionId = _dl.CreateProduction(prodObject, _userId);
                cleanupList.Add(Tuple.Create(productionId, Table.Production));

                ProductionObject deleteObject = new ProductionObject
                {
                    ProductionType = "Distillation",
                    ProductionId = productionId
                };

                // Act
                _dl.DeleteProduction(deleteObject, _userId);

                var prodQuery =
                (from production in _db.Production
                 where production.ProductionID == productionId
                 select production).FirstOrDefault();

                // Assert
                Assert.IsNull(prodQuery);
            }
            finally
            {
                // Cleanup
                foreach (var i in cleanupList)
                {
                    TestRecordCleanup(i.Item1, i.Item2);
                }
            }
        }

        [TestMethod()]
        public void CreateStorageTest()
        {
            // Arrange
            StorageObject storage = new StorageObject();
            storage.StorageName = "TestStorage";
            storage.SerialNumber = "123";
            storage.Capacity = 100;
            storage.Note = "TestNote";

            // Act
            int result = _dl.CreateStorage(_userId, storage);

            // Assert
            Assert.AreNotEqual(0, result);

            // cleanup
            TestRecordCleanup(result, Table.Storage);

        }

        [TestMethod()]
        public void GetStorageListTest()
        {
            // Act
            List<StorageObject> result = _dl.GetStorageList(1);

            // Assert
            Assert.IsNotNull(result, "GetStorageListTest result returned is null");
        }

        /// <summary>
        /// This test validates the following conditions
        /// Buy Grapes in January
        /// Make Wine in January
        /// Distil 100% of Wine in January
        /// Blend 50% of Distillation from the above
        /// Check to make sure Received in Bulk is updated correctly
        /// </summary>
        [TestMethod()]
        public void BuyGrapes_Ferment_DistilGauged_BlendSomeQuantity_Test_ToMakeSure_StorageReportIsCorrect()
        {
            // Arrange
            int spiritId = 0;
            int waterMaterialId = 0;
            int vendorId = 0;
            int storageId = 0;
            int grapeMaterialId = 0;
            int purchaseId = 0;
            int productionId = 0;

            // reporting time range
            DateTime start = new DateTime(2018, 01, 01);
            DateTime end = new DateTime(2018, 01, 31);

            List<Tuple<int/*recordId*/, Table/*table enum vaue*/>> tablesForCleanupTupleList = new List<Tuple<int, Table>>();

            try
            {
                //  dictionary setup
                #region Dictionary

                //  dictionary setup
                SpiritObject spirit = new SpiritObject();
                spirit.SpiritName = "Brandy Under 170";
                spirit.ProcessingReportTypeID = 12;

                spiritId = _dl.CreateSpirit(_userId, spirit);

                // setup Vendor object
                VendorObject vendor = new VendorObject();
                vendor.VendorName = "VendorTest";

                vendorId = _dl.CreateVendor(_userId, vendor);
                tablesForCleanupTupleList.Add(Tuple.Create(vendorId, Table.Vendor));

                // setup Storage Object
                StorageObject storage = new StorageObject();
                storage.StorageName = "testStorage";
                storage.SerialNumber = "2H29NNS";

                storageId = _dl.CreateStorage(_userId, storage);
                tablesForCleanupTupleList.Add(Tuple.Create(storageId, Table.Storage));

                // setup Material Object
                // grapes
                {
                    RawMaterialObject grapeMaterial = new RawMaterialObject();
                    grapeMaterial.RawMaterialName = "Grapes";
                    grapeMaterial.MaterialCategoryID = (int)Persistence.BusinessLogicEnums.ProductionReportMaterialCategory.Fruit;
                    grapeMaterial.UnitType = "lb";
                    grapeMaterial.UnitTypeId = 2;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Fermentable = true;
                    grapeMaterial.PurchaseMaterialTypes = materialBoolTypes;

                    grapeMaterialId = _dl.CreateRawMaterial(_userId, grapeMaterial);

                    tablesForCleanupTupleList.Add(Tuple.Create(grapeMaterialId, Table.MaterialDict));
                }
                // water
                {
                    RawMaterialObject waterMaterial = new RawMaterialObject();
                    waterMaterial.RawMaterialName = "Water";
                    waterMaterial.UnitType = "gal";
                    waterMaterial.UnitTypeId = 1;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Additive = true;
                    waterMaterial.PurchaseMaterialTypes = materialBoolTypes;

                    waterMaterialId = _dl.CreateRawMaterial(_userId, waterMaterial);
                    tablesForCleanupTupleList.Add(Tuple.Create(waterMaterialId, Table.MaterialDict));
                }

                #endregion

                #region Purchase
                // create Purchase Record (minimal required fields)
                PurchaseObject purchO = new PurchaseObject();
                purchO.PurBatchName = "Riesling Grapes ";
                purchO.PurchaseType = "Fermentable";
                purchO.PurchaseDate = new DateTime(2018, 01, 03);
                purchO.Quantity = 0f;
                purchO.VolumeByWeight = 2000f;
                purchO.RecordId = grapeMaterialId;
                purchO.Price = 350f;
                purchO.VendorId = vendorId;

                List<StorageObject> storageList = new List<StorageObject>();
                StorageObject storageObject = new StorageObject();
                storageObject.StorageId = storageId;
                storageList.Add(storageObject);
                purchO.Storage = storageList;

                purchaseId = _dl.CreatePurchase(purchO, _userId);
                tablesForCleanupTupleList.Add(Tuple.Create(purchaseId, Table.Purchase));

                #endregion

                #region Production
                // create Fermented record
                ProductionObject prodO = new ProductionObject();
                prodO.BatchName = "Riesling Wine";
                prodO.ProductionDate = new DateTime(2018, 01, 15);
                prodO.ProductionStart = new DateTime(2018, 01, 15);
                prodO.ProductionEnd = new DateTime(2018, 01, 15);
                prodO.Gauged = true;
                prodO.ProductionType = "Fermentation";
                prodO.ProductionTypeId = 1;
                prodO.Quantity = 150f; // 150 gallons of wine
                prodO.VolumeByWeight = 0f;
                prodO.AlcoholContent = 10f; // %
                prodO.ProofGallon = 30f; // pfg
                prodO.Storage = storageList; // we are using the same storage id as we use for Purchase to keep things simple
                prodO.SpiritTypeReportingID = 11; // Wine
                prodO.MaterialKindReportingID = 0;

                List<ObjInfo4Burndwn> usedMats = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat = new ObjInfo4Burndwn();
                uMat.ID = purchaseId;
                uMat.OldVal = 0f;
                uMat.NewVal = purchO.VolumeByWeight;
                uMat.DistillableOrigin = "pur";
                uMat.BurningDownMethod = "weight";

                usedMats.Add(uMat);

                prodO.UsedMats = usedMats;

                productionId = _dl.CreateProduction(prodO, _userId);

                tablesForCleanupTupleList.Add(Tuple.Create(productionId, Table.Production));

                // create Production Distillation Record and mark it as Gauged
                ProductionObject prodO1 = new ProductionObject();
                prodO1.BatchName = "DistilRunAndGauged";
                prodO1.ProductionDate = new DateTime(2018, 01, 20);
                prodO1.ProductionStart = new DateTime(2018, 01, 20);
                prodO1.ProductionEnd = new DateTime(2018, 01, 20);
                prodO1.SpiritCutId = 11; // mixed
                prodO1.Gauged = true;
                prodO1.ProductionType = "Distillation";
                prodO1.ProductionTypeId = 2;
                prodO1.Quantity = 50f;
                prodO1.VolumeByWeight = 0f;
                prodO1.AlcoholContent = 50f;
                prodO1.ProofGallon = 50f;
                prodO1.Storage = storageList; // we are using the same storage id as we use for Purchase to keep things simple
                prodO1.SpiritTypeReportingID = 3; // Brandy 170-
                prodO1.MaterialKindReportingID = 94; // grape brandy

                List<ObjInfo4Burndwn> usedMats1 = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat1 = new ObjInfo4Burndwn();
                uMat1.ID = productionId;
                uMat1.OldVal = 0f;
                uMat1.NewVal = prodO.Quantity;
                //uMat1.Proof = prodO.ProofGallon;
                uMat1.DistillableOrigin = "prod";
                uMat1.BurningDownMethod = "volume";

                usedMats1.Add(uMat1);

                prodO1.UsedMats = usedMats1;

                productionId = _dl.CreateProduction(prodO1, _userId);

                tablesForCleanupTupleList.Add(Tuple.Create(productionId, Table.Production));

                // create Production Blending Record
                ProductionObject prodBlend = new ProductionObject();
                prodBlend.BatchName = "testProdBlend";
                prodBlend.ProductionDate = new DateTime(2018, 01, 21);
                prodBlend.ProductionStart = new DateTime(2018, 01, 21);
                prodBlend.ProductionEnd = new DateTime(2018, 01, 21);
                prodBlend.Gauged = true;
                prodBlend.ProductionType = "Blending";
                prodBlend.Quantity = 22.5f; // 22.5 gallons of alcohol
                prodBlend.VolumeByWeight = 0f;
                prodBlend.AlcoholContent = 40f; // 40%
                prodBlend.ProofGallon = 25f;
                prodBlend.Storage = storageList; // we are using the same storage id as we use for Purchase to keep things simple
                prodBlend.SpiritTypeReportingID = 3; // brandy under 170
                prodBlend.MaterialKindReportingID = 94; // grape brandy
                prodBlend.SpiritId = spiritId;
                prodBlend.ProductionTypeId = 3;

                List<ObjInfo4Burndwn> usedMats4Blend = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat4Blend = new ObjInfo4Burndwn();
                uMat4Blend.ID = productionId;
                uMat4Blend.OldVal = prodO1.Quantity / 2;
                uMat4Blend.NewVal = prodO1.Quantity / 2;
                uMat4Blend.Proof = prodO1.ProofGallon / 2;
                uMat4Blend.DistillableOrigin = "prod";
                uMat4Blend.BurningDownMethod = "volume";

                usedMats4Blend.Add(uMat4Blend);
                prodBlend.UsedMats = usedMats4Blend;

                List<BlendingAdditive> blendAdditives = new List<BlendingAdditive>();
                BlendingAdditive blendAd = new BlendingAdditive();
                blendAd.RawMaterialId = waterMaterialId;
                blendAd.RawMaterialQuantity = 7.5f;
                blendAd.RawMaterialName = "Water";
                blendAd.UnitOfMeasurement = "gal";

                blendAdditives.Add(blendAd);

                prodBlend.BlendingAdditives = blendAdditives;

                productionId = _dl.CreateProduction(prodBlend, _userId);
                tablesForCleanupTupleList.Add(Tuple.Create(productionId, Table.Production));

                #endregion

                #region Reports

                // verify storage report
                StorageReport actualStorageReportObject = new StorageReport();

                actualStorageReportObject = _dl.GetStorageReportData(start, end, _userId);

                // check grape brandy object
                var actualStoGrapeBrandy = actualStorageReportObject.ReportBody.Find(x => x.CategoryName == "BrandyUnder170");

                if (actualStoGrapeBrandy == null)
                {
                    Assert.IsNotNull(actualStoGrapeBrandy, "Storage Grape Brandy Under 170 object should not be null");
                }
                else
                {
                    Assert.AreEqual(25f, actualStoGrapeBrandy.r2_DepositedInBulkStorage);
                    Assert.AreEqual(25f, actualStoGrapeBrandy.r6_TotalLines1Through5);
                    Assert.AreEqual(25, actualStoGrapeBrandy.r23_OnHandEndOfMonth);
                    Assert.AreEqual(0f, actualStoGrapeBrandy.r1_OnHandFirstOfMonth);
                    Assert.AreEqual(25f, actualStoGrapeBrandy.r24_Lines7Through23);
                }

                // check Wine object
                var actualStoWine = actualStorageReportObject.ReportBody.Find(x => x.CategoryName == "Wine");

                if (actualStoWine == null)
                {
                    Assert.IsNotNull(actualStoGrapeBrandy, "Storage Wine object should not be null");
                }
                else
                {
                    Assert.AreEqual(30f, actualStoWine.r2_DepositedInBulkStorage);
                    Assert.AreEqual(30f, actualStoWine.r6_TotalLines1Through5);
                    Assert.AreEqual(0, actualStoWine.r23_OnHandEndOfMonth);
                    Assert.AreEqual(0f, actualStoWine.r1_OnHandFirstOfMonth);
                    Assert.AreEqual(30f, actualStoWine.r24_Lines7Through23);
                    Assert.AreEqual(30f, actualStoWine.r18_TransferredToProductionAccount);
                    Assert.AreEqual(30f, actualStoWine.r6_TotalLines1Through5);
                }


                ProductionReportingObject actualProdReportObject = new ProductionReportingObject();

                actualProdReportObject = _dl.GetProductionReportData(start, end, _userId);

                var expectedWine = actualProdReportObject.Part1List.Find(x => x.SpiritCatName == "Wine");

                // verify Production report part 1
                if (expectedWine == null)
                {
                    Assert.IsNotNull(expectedWine, "Expect records for Wine Category");
                }
                else
                {
                    // Wine expected output
                    Assert.AreEqual(0, expectedWine.ProccessingAcct);
                    Assert.AreEqual(0, expectedWine.StorageAcct);
                    Assert.AreEqual(0, expectedWine.ProducedTotal);
                    Assert.AreEqual(30, expectedWine.Recd4RedistilaltionL15);
                }

                var expectedBrandy = actualProdReportObject.Part1List.Find(x => x.SpiritCatName == "BrandyUnder170");

                if (expectedBrandy == null)
                {
                    Assert.IsNotNull(expectedWine, "Expect records for Brandy Category");
                }
                else
                {
                    // Brandy expected output
                    Assert.AreEqual(25, expectedBrandy.ProccessingAcct);
                    Assert.AreEqual(25, expectedBrandy.StorageAcct);
                    Assert.AreEqual(0, expectedBrandy.Recd4RedistilL17);
                    Assert.AreEqual(50, expectedBrandy.ProducedTotal);
                }

                // verify Production report Part 2 trough 4
                Assert.AreEqual("GrapeBrandy", actualProdReportObject.Part2Through4List[0].KindOfMaterial);
                Assert.AreEqual(false, actualProdReportObject.Part2Through4List[0].NewCoop);
                Assert.AreEqual(false, actualProdReportObject.Part2Through4List[0].UsedCoop);
                Assert.AreEqual(50, actualProdReportObject.Part2Through4List[0].ProofGallons);
                Assert.AreEqual(false, actualProdReportObject.Part2Through4List[0].Tanks);

                // verify part 5
                Assert.AreEqual("Wine", actualProdReportObject.part5List[0].KindofSpirits);
                Assert.AreEqual(30f, actualProdReportObject.part5List[0].Proof);

                // verify Production report Part 6 - no data should be present in this scenario
                Assert.AreEqual(1, actualProdReportObject.ProdReportPart6List.Count);

                #endregion
            }
            finally
            {
                // Cleanup created records
                foreach (var i in tablesForCleanupTupleList)
                {
                    TestRecordCleanup(i.Item1, i.Item2);
                }
            }
        }
        /// <summary>
        /// This test method checks that ProofGallon records stays on the Production report in September after
        /// it has been Blended and Bottled in October and also, it checks production report in October
        /// </summary>
        [TestMethod()]
        public void Buy_Wine_Distil_And_Gauge_In_September_Blend_And_Bottle_In_October_Make_Brandy_Under_170_And_Test_September_Production()
        {
            // Arrange
            List<Tuple<int/*recordId*/, Table/*table enum vaue*/>> tupleL = new List<Tuple<int, Table>>();
            int spiritId = 0;
            int vendorId = 0;
            int storageId = 0;
            int wineMaterialId = 0;
            int waterMaterialId = 0;
            int purchaseId = 0;
            int productionId = 0;

            // reporting time range
            DateTime start = new DateTime(2017, 09, 01);
            DateTime end = new DateTime(2017, 09, 30);

            try
            {
                #region Dictionary
                //  dictionary setup
                SpiritObject spirit = new SpiritObject();
                spirit.SpiritName = "Brandy Under 170";
                spirit.ProcessingReportTypeID = 12;

                spiritId = _dl.CreateSpirit(_userId, spirit);

                // setup Vendor object
                VendorObject vendor = new VendorObject();
                vendor.VendorName = "testVendor";

                vendorId = _dl.CreateVendor(_userId, vendor);
                tupleL.Add(Tuple.Create(vendorId, Table.Vendor));

                // setup Storage Object
                StorageObject storage = new StorageObject();
                storage.StorageName = "testStorage";
                storage.SerialNumber = "2H29NNS";

                storageId = _dl.CreateStorage(_userId, storage);
                tupleL.Add(Tuple.Create(storageId, Table.Storage));


                // setup Material Object
                // wine
                {
                    RawMaterialObject wineMaterial = new RawMaterialObject();
                    wineMaterial.RawMaterialName = "Wine For Brandy";
                    wineMaterial.MaterialCategoryID = 2;
                    wineMaterial.UnitType = "gal";
                    wineMaterial.UnitTypeId = 1;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Fermented = true;
                    wineMaterial.PurchaseMaterialTypes = materialBoolTypes;

                    wineMaterialId = _dl.CreateRawMaterial(_userId, wineMaterial);
                    tupleL.Add(Tuple.Create(wineMaterialId, Table.MaterialDict));
                }

                // water
                {
                    RawMaterialObject waterMaterial = new RawMaterialObject();
                    waterMaterial.RawMaterialName = "Water";
                    waterMaterial.UnitType = "gal";
                    waterMaterial.UnitTypeId = 1;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Additive = true;
                    waterMaterial.PurchaseMaterialTypes = materialBoolTypes;

                    waterMaterialId = _dl.CreateRawMaterial(_userId, waterMaterial);
                    tupleL.Add(Tuple.Create(waterMaterialId, Table.MaterialDict));
                }
                #endregion

                #region Purchase
                // create Purchase Record (minimal required fields)
                PurchaseObject purchO = new PurchaseObject();
                purchO.PurBatchName = "test7Purchase";
                purchO.PurchaseType = "Fermented";
                purchO.PurchaseDate = new DateTime(2017, 09, 1);
                purchO.Quantity = 100f; // 100 gallons
                purchO.VolumeByWeight = 0f;
                purchO.AlcoholContent = 9f;
                purchO.ProofGallon = 18f;
                purchO.RecordId = wineMaterialId;
                purchO.Price = 350f;
                purchO.VendorId = vendorId;

                List<StorageObject> stoL = new List<StorageObject>();
                StorageObject sto = new StorageObject();
                sto.StorageId = storageId;
                stoL.Add(sto);
                purchO.Storage = stoL;

                purchO.SpiritTypeReportingID = 11;
                purchO.Gauged = true;

                purchaseId = _dl.CreatePurchase(purchO, _userId);
                tupleL.Add(Tuple.Create(purchaseId, Table.Purchase));
                #endregion

                #region Production
                // create 1st Production Distillation Record and don't mark it as Gauged
                ProductionObject prodO = new ProductionObject();
                prodO.BatchName = "test1stDistillRun";
                prodO.ProductionDate = new DateTime(2017, 09, 3);
                prodO.ProductionStart = new DateTime(2017, 09, 3);
                prodO.ProductionEnd = new DateTime(2017, 09, 3);
                prodO.SpiritCutId = 11; // mixed
                prodO.Gauged = false;
                prodO.ProductionType = "Distillation";
                prodO.Quantity = 50f; //50 gallons of alcohol
                prodO.VolumeByWeight = 0f;
                prodO.AlcoholContent = 80f; // 80%
                prodO.ProofGallon = 80f; // 80pfg
                prodO.Storage = stoL; // we are using the same storage id as we use for Purchase to keep things simple
                prodO.SpiritTypeReportingID = 3; // brandy under 170
                prodO.MaterialKindReportingID = 94; // grape brandy
                prodO.ProductionTypeId = 2;

                List<ObjInfo4Burndwn> usedMats = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat = new ObjInfo4Burndwn();
                uMat.ID = purchaseId;
                uMat.OldVal = 0f;
                uMat.NewVal = purchO.Quantity;
                uMat.DistillableOrigin = "pur";
                uMat.BurningDownMethod = "volume";

                usedMats.Add(uMat);

                prodO.UsedMats = usedMats;

                productionId = _dl.CreateProduction(prodO, _userId);
                tupleL.Add(Tuple.Create(productionId, Table.Production));

                // create 2nd Production Distillation Record and mark it as Gauged
                ProductionObject prodO2 = new ProductionObject();
                prodO2.BatchName = "testGaugedDistillRun";
                prodO2.ProductionDate = new DateTime(2017, 09, 4);
                prodO2.ProductionStart = new DateTime(2017, 09, 4);
                prodO2.ProductionEnd = new DateTime(2017, 09, 4);
                prodO2.SpiritCutId = 9; // hearts
                prodO2.Gauged = true;
                prodO2.ProductionType = "Distillation";
                prodO2.Quantity = 15f; // 15 gallons of alcohol
                prodO2.VolumeByWeight = 0f;
                prodO2.AlcoholContent = 60f; // 60%
                prodO2.ProofGallon = 18f; // 18pfg
                prodO2.Storage = stoL; // we are using the same storage id as we use for Purchase to keep things simple
                prodO2.SpiritTypeReportingID = 3; // brandy under 170
                prodO2.MaterialKindReportingID = 94; // grape brandy
                prodO2.ProductionTypeId = 2;

                List<ObjInfo4Burndwn> usedMats4Gauge = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat4Gauged = new ObjInfo4Burndwn();
                uMat4Gauged.ID = productionId;
                uMat4Gauged.OldVal = 0f;
                uMat4Gauged.NewVal = prodO.Quantity;
                uMat4Gauged.DistillableOrigin = "prod";
                uMat4Gauged.BurningDownMethod = "volume";

                usedMats4Gauge.Add(uMat4Gauged);
                prodO2.UsedMats = usedMats4Gauge;

                productionId = _dl.CreateProduction(prodO2, _userId); // here productionId is overriden with a new productionId of the new Gauged record
                tupleL.Add(Tuple.Create(productionId, Table.Production));

                // create Production Blending Record
                ProductionObject prodBlend = new ProductionObject();
                prodBlend.BatchName = "testProdBlend";
                prodBlend.ProductionDate = new DateTime(2017, 10, 6);
                prodBlend.ProductionStart = new DateTime(2017, 10, 6);
                prodBlend.ProductionEnd = new DateTime(2017, 10, 6);
                prodBlend.Gauged = true;
                prodBlend.ProductionType = "Blending";
                prodBlend.Quantity = 22.5f; // 22.5 gallons of alcohol
                prodBlend.VolumeByWeight = 0f;
                prodBlend.AlcoholContent = 40f; // 40%
                prodBlend.ProofGallon = 18f; // 18pfg
                prodBlend.Storage = stoL; // we are using the same storage id as we use for Purchase to keep things simple
                prodBlend.SpiritTypeReportingID = 3; // brandy under 170
                prodBlend.MaterialKindReportingID = 94; // grape brandy
                prodBlend.SpiritId = spiritId;
                prodBlend.ProductionTypeId = 3;

                List<ObjInfo4Burndwn> usedMats4Blend = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat4Blend = new ObjInfo4Burndwn();
                uMat4Blend.ID = productionId;
                uMat4Blend.OldVal = 0f;
                uMat4Blend.NewVal = prodO2.Quantity;
                uMat4Blend.DistillableOrigin = "prod";
                uMat4Blend.BurningDownMethod = "volume";

                usedMats4Blend.Add(uMat4Blend);
                prodBlend.UsedMats = usedMats4Blend;

                List<BlendingAdditive> blendAdditives = new List<BlendingAdditive>();
                BlendingAdditive blendAd = new BlendingAdditive();
                blendAd.RawMaterialId = waterMaterialId;
                blendAd.RawMaterialQuantity = 7.5f;
                blendAd.RawMaterialName = "Water";
                blendAd.UnitOfMeasurement = "gal";

                blendAdditives.Add(blendAd);

                prodBlend.BlendingAdditives = blendAdditives;

                productionId = _dl.CreateProduction(prodBlend, _userId); // here productionId is overriden with a new productionId of the new Gauged record
                tupleL.Add(Tuple.Create(productionId, Table.Production));

                // create Production Bottling Record
                ProductionObject prodBottl = new ProductionObject();
                prodBottl.BatchName = "testProdBottling ";
                prodBottl.ProductionDate = new DateTime(2017, 10, 6);
                prodBottl.ProductionStart = new DateTime(2017, 10, 6);
                prodBottl.ProductionEnd = new DateTime(2017, 10, 6);
                prodBottl.Gauged = true;
                prodBottl.ProductionType = "Bottling";
                prodBottl.Quantity = 22.39f; // 22.39 gallons of alcohol
                prodBottl.VolumeByWeight = 0f;
                prodBottl.AlcoholContent = 40f; // 40%
                prodBottl.ProofGallon = 17.9f; // 17.9 pfg
                prodBottl.Storage = stoL; // we are using the same storage id as we use for Purchase to keep things simple
                prodBottl.SpiritTypeReportingID = 3; // brandy under 170
                prodBottl.MaterialKindReportingID = 94; // grape brandy
                prodBottl.SpiritId = spiritId;
                prodO.ProductionTypeId = 4;

                List<ObjInfo4Burndwn> usedMats4Bottl = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat4Bottl = new ObjInfo4Burndwn();
                uMat4Bottl.ID = productionId;
                uMat4Bottl.OldVal = 0f;
                uMat4Bottl.NewVal = prodBlend.Quantity;
                uMat4Bottl.DistillableOrigin = "prod";
                uMat4Bottl.BurningDownMethod = "volume";

                usedMats4Bottl.Add(uMat4Bottl);
                prodBottl.UsedMats = usedMats4Bottl;

                BottlingObject bottlingObj = new BottlingObject();
                bottlingObj.CaseCapacity = 12;
                bottlingObj.CaseQuantity = 9.42f;
                bottlingObj.BottleCapacity = 750f;
                bottlingObj.BottleQuantity = 113;

                prodBottl.BottlingInfo = bottlingObj;

                prodBottl.GainLoss = .10f;

                prodBottl.FillTestList = null;

                productionId = _dl.CreateProduction(prodBottl, _userId); // here productionId is overriden with a new productionId of the new Gauged record
                tupleL.Add(Tuple.Create(productionId, Table.Production));

                #endregion

                #region Reports setup

                // Report Header
                ReportHeader reportHeaderE = new ReportHeader();
                reportHeaderE.ProprietorName = "Test Distillery";
                reportHeaderE.EIN = "12-3456789";
                reportHeaderE.ReportDate = "September 2017";
                reportHeaderE.PlantAddress = "123 Cognac Drive Renton WASHINGTON 98059";
                reportHeaderE.DSP = "DSP-WA-21086";

                /* PRODUCTION REPORT */

                ProdReportPart1 part1E = new ProdReportPart1();
                part1E.ProccessingAcct = 0f;
                part1E.ProducedTotal = 18f;
                part1E.Recd4RedistilL17 = 0f;
                part1E.Recd4RedistilaltionL15 = 0f;
                part1E.StorageAcct = 18f;
                part1E.SpiritTypeReportingID = 3;
                part1E.UnfinishedSpiritsEndOfQuarterL17 = 0f;

                ProdReportParts2Through4 part2thru4 = new ProdReportParts2Through4();
                part2thru4.KindOfMaterial = "GrapeBrandy";
                part2thru4.MaterialKindReportingID = 94;
                part2thru4.NewCoop = false;
                part2thru4.UsedCoop = false;
                part2thru4.ProofGallons = 18f;
                part2thru4.SpiritTypeReportingID = 3;
                part2thru4.Tanks = false;

                ProdReportPart5 part5 = new ProdReportPart5();
                part5.KindofSpirits = "";
                part5.Proof = 0f;

                ProductionReportingObject actualProdReportObject = new ProductionReportingObject();


                actualProdReportObject = _dl.GetProductionReportData(start, end, _userId);

                #endregion

                // Assert

                #region Production Report

                Tuple<int, float> accumulatedProofGL4SpiritTypeReportingId = new Tuple<int, float>(11, purchO.ProofGallon);

                List<Tuple<int, float>> accumulatedProofGL4SpiritTypeReportingIdL = new List<Tuple<int, float>>();

                accumulatedProofGL4SpiritTypeReportingIdL.Add(accumulatedProofGL4SpiritTypeReportingId);

                // verify Production report header
                Assert.AreEqual(reportHeaderE.DSP, actualProdReportObject.Header.DSP);
                Assert.AreEqual(reportHeaderE.EIN, actualProdReportObject.Header.EIN);
                Assert.AreEqual(reportHeaderE.PlantAddress, actualProdReportObject.Header.PlantAddress);
                Assert.AreEqual(reportHeaderE.ProprietorName, actualProdReportObject.Header.ProprietorName);
                Assert.AreEqual(reportHeaderE.ReportDate, actualProdReportObject.Header.ReportDate);

                // verify Production report Part 1
                Assert.AreEqual(part1E.ProccessingAcct, actualProdReportObject.Part1List[0].ProccessingAcct);
                Assert.AreEqual(part1E.StorageAcct, actualProdReportObject.Part1List[0].StorageAcct);
                Assert.AreEqual(part1E.ProducedTotal, actualProdReportObject.Part1List[0].ProducedTotal);
                Assert.AreEqual(part1E.Recd4RedistilL17, actualProdReportObject.Part1List[0].Recd4RedistilL17);

                var expectedWine = accumulatedProofGL4SpiritTypeReportingIdL.Find(l => l.Item1 == 11);
                var actualWine = actualProdReportObject.Part1List.Find(x => x.SpiritTypeReportingID == expectedWine.Item1);

                if (actualWine == null)
                {
                    Assert.AreNotEqual(null, expectedWine, "No records in actualAlcohol190Plus for SpiritTypeID");
                }
                else
                {
                    Assert.AreEqual(expectedWine.Item2, actualWine.Recd4RedistilaltionL15);
                    Assert.AreEqual("Wine", actualWine.SpiritCatName);
                }

                Assert.AreEqual(part1E.UnfinishedSpiritsEndOfQuarterL17, actualProdReportObject.Part1List[0].UnfinishedSpiritsEndOfQuarterL17);

                // verify Production report Part 2 trough 4
                Assert.AreEqual(part2thru4.KindOfMaterial, actualProdReportObject.Part2Through4List[0].KindOfMaterial);
                Assert.AreEqual(part2thru4.MaterialKindReportingID, actualProdReportObject.Part2Through4List[0].MaterialKindReportingID);
                Assert.AreEqual(part2thru4.NewCoop, actualProdReportObject.Part2Through4List[0].NewCoop);
                Assert.AreEqual(part2thru4.UsedCoop, actualProdReportObject.Part2Through4List[0].UsedCoop);
                Assert.AreEqual(part2thru4.ProofGallons, actualProdReportObject.Part2Through4List[0].ProofGallons);
                Assert.AreEqual(part2thru4.SpiritTypeReportingID, actualProdReportObject.Part2Through4List[0].SpiritTypeReportingID);
                Assert.AreEqual(part2thru4.Tanks, actualProdReportObject.Part2Through4List[0].Tanks);

                // verify part 5
                Assert.AreEqual("Wine", actualProdReportObject.part5List[0].KindofSpirits);
                Assert.AreEqual(18, actualProdReportObject.part5List[0].Proof);

                // verify Production report Part 6 - no data should be present in this scenario
                Assert.AreEqual(0, actualProdReportObject.ProdReportPart6List.Count);

                // Test October Production report
                ProductionReportingObject actualProdReportObjectOctober = new ProductionReportingObject();

                actualProdReportObjectOctober = _dl.GetProductionReportData(new DateTime(2017, 10, 01), new DateTime(2017, 10, 31), _userId);
                // verify Production report header
                Assert.AreEqual(reportHeaderE.DSP, actualProdReportObjectOctober.Header.DSP);
                Assert.AreEqual(reportHeaderE.EIN, actualProdReportObjectOctober.Header.EIN);
                Assert.AreEqual(reportHeaderE.PlantAddress, actualProdReportObjectOctober.Header.PlantAddress);
                Assert.AreEqual(reportHeaderE.ProprietorName, actualProdReportObjectOctober.Header.ProprietorName);
                Assert.AreEqual("October 2017", actualProdReportObjectOctober.Header.ReportDate);

                // verify Production report Part 1
                Assert.AreEqual(0, actualProdReportObjectOctober.Part1List.Count);
                // verify Production report Part 2 trough 4
                Assert.AreEqual(0, actualProdReportObjectOctober.Part2Through4List.Count);
                // verify part 5
                Assert.AreEqual(0, actualProdReportObjectOctober.part5List.Count);
                // verify Production report Part 6
                Assert.AreEqual(0, actualProdReportObjectOctober.ProdReportPart6List.Count);
                #endregion
            }
            finally
            {
                // Cleanup
                foreach (var i in tupleL)
                {
                    TestRecordCleanup(i.Item1, i.Item2);
                }
            }
        }

        /// <summary>
        /// This test verifies part 4 of Production report when two different distills are distilled
        /// in Brandy Under 170 category but under different Material Categories
        /// So, in this case:
        /// Distil 1: Brandy Under 170 with Material Category of Grape Brandy
        /// Distil 2: Brandy Under 170 with Material Category of All Other Brandy
        /// </summary>
        [TestMethod()]
        public void Distill_Brandy_Under_170_Twice_Under_Two_Different_MaterialCategroires_Test()
        {
            // Arrange
            List<Tuple<int/*recordId*/, Table/*table enum vaue*/>> tablesForCleanupTupleList = new List<Tuple<int, Table>>();
            int spiritId = 0;
            int vendorId = 0;
            int storageId = 0;
            int wineMaterialId = 0;
            int waterMaterialId = 0;
            int purchaseId = 0;
            int productionId = 0;

            try
            {
                #region Arrange
                //  dictionary setup
                SpiritObject spirit = new SpiritObject();
                spirit.SpiritName = "Brandy Under 170";
                spirit.ProcessingReportTypeID = 12;

                spiritId = _dl.CreateSpirit(_userId, spirit);
                tablesForCleanupTupleList.Add(Tuple.Create(spiritId, Table.Spirit));

                // setup Vendor object
                VendorObject vendor = new VendorObject();
                vendor.VendorName = "testVendor";

                vendorId = _dl.CreateVendor(_userId, vendor);
                tablesForCleanupTupleList.Add(Tuple.Create(vendorId, Table.Vendor));

                // setup Storage Object
                StorageObject storage = new StorageObject();
                storage.StorageName = "testStorage";
                storage.SerialNumber = "2H29NNS";

                storageId = _dl.CreateStorage(_userId, storage);
                tablesForCleanupTupleList.Add(Tuple.Create(storageId, Table.Storage));

                // setup Material Object
                // wine
                {
                    RawMaterialObject wineMaterial = new RawMaterialObject();
                    wineMaterial.RawMaterialName = "Wine For Brandy";
                    wineMaterial.MaterialCategoryID = 2;
                    wineMaterial.UnitType = "gal";
                    wineMaterial.UnitTypeId = 1;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Fermented = true;
                    wineMaterial.PurchaseMaterialTypes = materialBoolTypes;

                    wineMaterialId = _dl.CreateRawMaterial(_userId, wineMaterial);
                    tablesForCleanupTupleList.Add(Tuple.Create(wineMaterialId, Table.MaterialDict));
                }

                // water
                {
                    RawMaterialObject waterMaterial = new RawMaterialObject();
                    waterMaterial.RawMaterialName = "Water";
                    waterMaterial.UnitType = "gal";
                    waterMaterial.UnitTypeId = 1;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Additive = true;
                    waterMaterial.PurchaseMaterialTypes = materialBoolTypes;

                    waterMaterialId = _dl.CreateRawMaterial(_userId, waterMaterial);
                    tablesForCleanupTupleList.Add(Tuple.Create(waterMaterialId, Table.MaterialDict));
                }

                // create Purchase Record (minimal required fields)
                PurchaseObject purchO = new PurchaseObject();
                purchO.PurBatchName = "test7Purchase";
                purchO.PurchaseType = "Fermented";
                purchO.PurchaseDate = new DateTime(2017, 11, 1);
                purchO.Quantity = 100f; // 100 gallons
                purchO.VolumeByWeight = 0f;
                purchO.AlcoholContent = 98f;
                purchO.ProofGallon = 196f;
                purchO.RecordId = wineMaterialId;
                purchO.Price = 1000f;
                purchO.VendorId = vendorId;

                List<StorageObject> storageList = new List<StorageObject>();
                StorageObject storageObject = new StorageObject();
                storageObject.StorageId = storageId;
                storageList.Add(storageObject);
                purchO.Storage = storageList;

                purchO.SpiritTypeReportingID = 10;
                purchO.Gauged = true;

                purchaseId = _dl.CreatePurchase(purchO, _userId);
                tablesForCleanupTupleList.Add(Tuple.Create(purchaseId, Table.Purchase));

                // create 1st Production Distillation Record and mark it as Gauged
                ProductionObject prodO = new ProductionObject();
                prodO.BatchName = "test1stDistillRun";
                prodO.ProductionDate = new DateTime(2017, 11, 3);
                prodO.ProductionStart = new DateTime(2017, 11, 3);
                prodO.ProductionEnd = new DateTime(2017, 11, 3);
                prodO.SpiritCutId = 11; // mixed
                prodO.Gauged = true;
                prodO.ProductionType = "Distillation";
                prodO.Quantity = 100f; //100 gallons of alcohol
                prodO.VolumeByWeight = 0f;
                prodO.AlcoholContent = 98f; // 80%
                prodO.ProofGallon = 196f; // 80pfg
                prodO.Storage = storageList; // we are using the same storage id as we use for Purchase to keep things simple
                prodO.SpiritTypeReportingID = 3; // brandy under 170
                prodO.MaterialKindReportingID = 94; // grape brandy
                prodO.ProductionTypeId = 2;

                List<ObjInfo4Burndwn> usedMats = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat = new ObjInfo4Burndwn();
                uMat.ID = purchaseId;
                uMat.OldVal = 0f;
                uMat.NewVal = purchO.Quantity;
                uMat.DistillableOrigin = "pur";
                uMat.BurningDownMethod = "volume";

                usedMats.Add(uMat);

                prodO.UsedMats = usedMats;

                productionId = _dl.CreateProduction(prodO, _userId);
                tablesForCleanupTupleList.Add(Tuple.Create(productionId, Table.Production));

                // create Purchase Record (minimal required fields)
                PurchaseObject purchO1 = new PurchaseObject();
                purchO1.PurBatchName = "test7Purchase";
                purchO1.PurchaseType = "Fermented";
                purchO1.PurchaseDate = new DateTime(2017, 11, 1);
                purchO1.Quantity = 100f; // 100 gallons
                purchO1.VolumeByWeight = 0f;
                purchO1.AlcoholContent = 98f;
                purchO1.ProofGallon = 196f;
                purchO1.RecordId = wineMaterialId;
                purchO1.Price = 1000f;
                purchO1.VendorId = vendorId;

                List<StorageObject> storageList1 = new List<StorageObject>();
                StorageObject storageObject1 = new StorageObject();
                storageObject1.StorageId = storageId;
                storageList1.Add(storageObject1);
                purchO1.Storage = storageList1;

                purchO1.SpiritTypeReportingID = 10;
                purchO1.Gauged = true;

                purchaseId = _dl.CreatePurchase(purchO1, _userId);
                tablesForCleanupTupleList.Add(Tuple.Create(purchaseId, Table.Purchase));

                // create 1st Production Distillation Record and mark it as Gauged
                ProductionObject prodO1 = new ProductionObject();
                prodO1.BatchName = "test1stDistill2";
                prodO1.ProductionDate = new DateTime(2017, 11, 3);
                prodO1.ProductionStart = new DateTime(2017, 11, 3);
                prodO1.ProductionEnd = new DateTime(2017, 11, 3);
                prodO1.SpiritCutId = 11; // mixed
                prodO1.Gauged = true;
                prodO1.ProductionType = "Distillation";
                prodO1.Quantity = 50f; //50 gallons of alcohol
                prodO1.VolumeByWeight = 0f;
                prodO1.AlcoholContent = 98f; // 98%
                prodO1.ProofGallon = 98f; // 98pfg
                prodO1.Storage = storageList; // we are using the same storage id as we use for Purchase to keep things simple
                prodO1.SpiritTypeReportingID = 3; // brandy under 170
                prodO1.MaterialKindReportingID = 95; // all other brandy
                prodO1.ProductionTypeId = 2;

                List<ObjInfo4Burndwn> usedMats1 = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat1 = new ObjInfo4Burndwn();
                uMat1.ID = purchaseId;
                uMat1.OldVal = 0f;
                uMat1.NewVal = purchO.Quantity;
                uMat1.DistillableOrigin = "pur";
                uMat1.BurningDownMethod = "volume";

                usedMats.Add(uMat1);

                prodO1.UsedMats = usedMats1;

                productionId = _dl.CreateProduction(prodO1, _userId);
                tablesForCleanupTupleList.Add(Tuple.Create(productionId, Table.Production));

                #endregion

                #region Act

                int novDays = DateTime.DaysInMonth(2017, 11);
                var novStart = new DateTime(2017, 11, 1);
                var novEnd = new DateTime(2017, 11, novDays);

                ProductionReportingObject productionReport = _dl.GetProductionReportData(novStart, novEnd, _userId);

                #endregion

                #region Assert

                // Test October Production Report
                // production report Part 4
                Dictionary<int, float> materialKindAmmounts = new Dictionary<int, float>();
                materialKindAmmounts.Add(94, prodO.ProofGallon); //Grape Brandy
                materialKindAmmounts.Add(95, prodO1.ProofGallon); //All Other Brandy

                // check Grape Brandy
                var actual = productionReport.Part2Through4List.Find(x => x.MaterialKindReportingID == 94);
                var expectedGrapeBrandyProof = materialKindAmmounts.FirstOrDefault(x => x.Key == 94).Value;

                Assert.IsNotNull(actual, "actual is null since Part4 returned no records for MaterialKindReportingID: " + 94);
                Assert.AreEqual(expectedGrapeBrandyProof, actual.ProofGallons);
                Assert.AreEqual("GrapeBrandy", actual.KindOfMaterial);

                // check All Other Brandy
                actual = productionReport.Part2Through4List.Find(x => x.MaterialKindReportingID == 95);
                var expectedAllOtherBrandyProof = materialKindAmmounts.FirstOrDefault(x => x.Key == 95).Value;

                Assert.IsNotNull(actual, "actual is null since Part4 returned no records for MaterialKindReportingID: " + 95);
                Assert.AreEqual(expectedAllOtherBrandyProof, actual.ProofGallons);
                Assert.AreEqual("AllOtherBrandy", actual.KindOfMaterial);

                #endregion
            }
            finally
            {
                // Cleanup
                foreach (var i in tablesForCleanupTupleList)
                {
                    TestRecordCleanup(i.Item1, i.Item2);
                }
            }
        }

        /// <summary>
        /// This test method verifies that when Bottling record
        /// is deleted, all other records amounts' that were involved in making this
        /// bottling record originally, are reinstated. Effectively, we should be re-instating values for Blending records
        /// In addition, we are verifying reports update it's values as well.
        /// </summary>
        [TestMethod()]
        public void DeleteWorkFlow_BuyWine_Distil_DistilAgain_Blend_Bottle_DeleteBottling_EnsureOtherRecords_Intact()
        {
            // Arrange
            List<Tuple<int/*recordId*/, Table/*table enum vaue*/>> tupleL = new List<Tuple<int, Table>>();
            int spiritId = 0;
            int vendorId = 0;
            int storageId = 0;
            int wineMaterialId = 0;
            int waterMaterialId = 0;
            int purchaseId = 0;
            int productionId = 0;

            DateTime start = new DateTime(2017, 09, 01);
            DateTime end = new DateTime(2017, 09, 30);

            try
            {
                #region Dictionary
                //  dictionary setup
                SpiritObject spirit = new SpiritObject();
                spirit.SpiritName = "Brandy Under 170";
                spirit.ProcessingReportTypeID = 12;

                spiritId = _dl.CreateSpirit(_userId, spirit);

                // setup Vendor object
                VendorObject vendor = new VendorObject();
                vendor.VendorName = "testVendor";

                vendorId = _dl.CreateVendor(_userId, vendor);
                tupleL.Add(Tuple.Create(vendorId, Table.Vendor));

                // setup Storage Object
                StorageObject storage = new StorageObject();
                storage.StorageName = "testStorage";
                storage.SerialNumber = "2H29NNS";

                storageId = _dl.CreateStorage(_userId, storage);
                tupleL.Add(Tuple.Create(storageId, Table.Storage));


                // setup Material Object
                // wine
                {
                    RawMaterialObject wineMaterial = new RawMaterialObject();
                    wineMaterial.RawMaterialName = "Wine For Brandy";
                    wineMaterial.MaterialCategoryID = 2;
                    wineMaterial.UnitType = "gal";
                    wineMaterial.UnitTypeId = 1;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Fermented = true;
                    wineMaterial.PurchaseMaterialTypes = materialBoolTypes;

                    wineMaterialId = _dl.CreateRawMaterial(_userId, wineMaterial);
                    tupleL.Add(Tuple.Create(wineMaterialId, Table.MaterialDict));
                }

                // water
                {
                    RawMaterialObject waterMaterial = new RawMaterialObject();
                    waterMaterial.RawMaterialName = "Water";
                    waterMaterial.UnitType = "gal";
                    waterMaterial.UnitTypeId = 1;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Additive = true;
                    waterMaterial.PurchaseMaterialTypes = materialBoolTypes;

                    waterMaterialId = _dl.CreateRawMaterial(_userId, waterMaterial);
                    tupleL.Add(Tuple.Create(waterMaterialId, Table.MaterialDict));
                }
                #endregion

                #region Purchase
                // create Purchase Record (minimal required fields)
                PurchaseObject purchO = new PurchaseObject();
                purchO.PurBatchName = "Feremented Purchase";
                purchO.PurchaseType = "Fermented";
                purchO.PurchaseDate = new DateTime(2017, 09, 1);
                purchO.Quantity = 100f; // 100 gallons
                purchO.VolumeByWeight = 0f;
                purchO.AlcoholContent = 9f;
                purchO.ProofGallon = 18f;
                purchO.RecordId = wineMaterialId;
                purchO.Price = 350f;
                purchO.VendorId = vendorId;

                List<StorageObject> stoL = new List<StorageObject>();
                StorageObject sto = new StorageObject();
                sto.StorageId = storageId;
                stoL.Add(sto);
                purchO.Storage = stoL;

                purchO.SpiritTypeReportingID = 11;
                purchO.Gauged = true;

                purchaseId = _dl.CreatePurchase(purchO, _userId);
                tupleL.Add(Tuple.Create(purchaseId, Table.Purchase));
                #endregion

                #region Production
                // create 1st Production Distillation Record and don't mark it as Gauged
                ProductionObject prodO = new ProductionObject();
                prodO.BatchName = "test1stDistillRun";
                prodO.ProductionDate = new DateTime(2017, 09, 3);
                prodO.ProductionStart = new DateTime(2017, 09, 3);
                prodO.ProductionEnd = new DateTime(2017, 09, 3);
                prodO.SpiritCutId = 11; // mixed
                prodO.Gauged = false;
                prodO.ProductionType = "Distillation";
                prodO.Quantity = 50f; //50 gallons of alcohol
                prodO.VolumeByWeight = 0f;
                prodO.AlcoholContent = 80f; // 80%
                prodO.ProofGallon = 80f; // 80pfg
                prodO.Storage = stoL; // we are using the same storage id as we use for Purchase to keep things simple
                prodO.SpiritTypeReportingID = 3; // brandy under 170
                prodO.MaterialKindReportingID = 94; // grape brandy
                prodO.ProductionTypeId = 2;

                List<ObjInfo4Burndwn> usedMats = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat = new ObjInfo4Burndwn();
                uMat.ID = purchaseId;
                uMat.OldVal = 0f;
                uMat.NewVal = purchO.Quantity;
                uMat.DistillableOrigin = "pur";
                uMat.BurningDownMethod = "volume";

                usedMats.Add(uMat);

                prodO.UsedMats = usedMats;

                productionId = _dl.CreateProduction(prodO, _userId);
                tupleL.Add(Tuple.Create(productionId, Table.Production));

                // create 2nd Production Distillation Record and mark it as Gauged
                ProductionObject prodO2 = new ProductionObject();
                prodO2.BatchName = "testGaugedDistillRun";
                prodO2.ProductionDate = new DateTime(2017, 09, 4);
                prodO2.ProductionStart = new DateTime(2017, 09, 4);
                prodO2.ProductionEnd = new DateTime(2017, 09, 4);
                prodO2.SpiritCutId = 9; // hearts
                prodO2.Gauged = true;
                prodO2.ProductionType = "Distillation";
                prodO2.Quantity = 15f; // 15 gallons of alcohol
                prodO2.VolumeByWeight = 0f;
                prodO2.AlcoholContent = 60f; // 60%
                prodO2.ProofGallon = 18f; // 18pfg
                prodO2.Storage = stoL; // we are using the same storage id as we use for Purchase to keep things simple
                prodO2.SpiritTypeReportingID = 3; // brandy under 170
                prodO2.MaterialKindReportingID = 94; // grape brandy
                prodO2.ProductionTypeId = 2;

                List<ObjInfo4Burndwn> usedMats4Gauge = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat4Gauged = new ObjInfo4Burndwn();
                uMat4Gauged.ID = productionId;
                uMat4Gauged.OldVal = 0f;
                uMat4Gauged.NewVal = prodO.Quantity;
                uMat4Gauged.DistillableOrigin = "prod";
                uMat4Gauged.BurningDownMethod = "volume";

                usedMats4Gauge.Add(uMat4Gauged);
                prodO2.UsedMats = usedMats4Gauge;

                int productionFinalDistillationId = _dl.CreateProduction(prodO2, _userId);

                tupleL.Add(Tuple.Create(productionFinalDistillationId, Table.Production));

                // create Production Blending Record
                ProductionObject prodBlend = new ProductionObject();
                prodBlend.BatchName = "testProdBlend";
                prodBlend.ProductionDate = new DateTime(2017, 09, 6);
                prodBlend.ProductionStart = new DateTime(2017, 09, 6);
                prodBlend.ProductionEnd = new DateTime(2017, 09, 6);
                prodBlend.Gauged = true;
                prodBlend.ProductionType = "Blending";
                prodBlend.Quantity = 22.5f; // 22.5 gallons of alcohol
                prodBlend.VolumeByWeight = 0f;
                prodBlend.AlcoholContent = 40f; // 40%
                prodBlend.ProofGallon = 18f; // 18pfg
                prodBlend.Storage = stoL; // we are using the same storage id as we use for Purchase to keep things simple
                prodBlend.SpiritTypeReportingID = 3; // brandy under 170
                prodBlend.MaterialKindReportingID = 94; // grape brandy
                prodBlend.SpiritId = spiritId;
                prodBlend.ProductionTypeId = 3;

                List<ObjInfo4Burndwn> usedMats4Blend = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat4Blend = new ObjInfo4Burndwn();
                uMat4Blend.ID = productionFinalDistillationId;
                uMat4Blend.OldVal = 0f;
                uMat4Blend.NewVal = prodO2.Quantity;
                uMat4Blend.DistillableOrigin = "prod";
                uMat4Blend.BurningDownMethod = "volume";

                usedMats4Blend.Add(uMat4Blend);
                prodBlend.UsedMats = usedMats4Blend;

                List<BlendingAdditive> blendAdditives = new List<BlendingAdditive>();
                BlendingAdditive blendAd = new BlendingAdditive();
                blendAd.RawMaterialId = waterMaterialId;
                blendAd.RawMaterialQuantity = 7.5f;
                blendAd.RawMaterialName = "Water";
                blendAd.UnitOfMeasurement = "gal";

                blendAdditives.Add(blendAd);

                prodBlend.BlendingAdditives = blendAdditives;

                int productionBlendingId = _dl.CreateProduction(prodBlend, _userId); // here productionId is overriden with a new productionId of the new Gauged record
                tupleL.Add(Tuple.Create(productionBlendingId, Table.Production));

                // create Production Bottling Record
                ProductionObject prodBottl = new ProductionObject();
                prodBottl.BatchName = "testProdBottling ";
                prodBottl.ProductionDate = new DateTime(2017, 09, 6);
                prodBottl.ProductionStart = new DateTime(2017, 09, 6);
                prodBottl.ProductionEnd = new DateTime(2017, 09, 6);
                prodBottl.Gauged = true;
                prodBottl.ProductionType = "Bottling";
                prodBottl.Quantity = 22.39f; // 22.39 gallons of alcohol
                prodBottl.VolumeByWeight = 0f;
                prodBottl.AlcoholContent = 40f; // 40%
                prodBottl.ProofGallon = 17.9f; // 17.9 pfg
                prodBottl.Storage = stoL; // we are using the same storage id as we use for Purchase to keep things simple
                prodBottl.SpiritTypeReportingID = 3; // brandy under 170
                prodBottl.MaterialKindReportingID = 94; // grape brandy
                prodBottl.SpiritId = spiritId;
                prodO.ProductionTypeId = 4;

                List<ObjInfo4Burndwn> usedMats4Bottl = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat4Bottl = new ObjInfo4Burndwn();
                uMat4Bottl.ID = productionBlendingId;
                uMat4Bottl.OldVal = 0f;
                uMat4Bottl.NewVal = prodBlend.Quantity;
                uMat4Bottl.DistillableOrigin = "prod";
                uMat4Bottl.BurningDownMethod = "volume";

                usedMats4Bottl.Add(uMat4Bottl);
                prodBottl.UsedMats = usedMats4Bottl;

                BottlingObject bottlingObj = new BottlingObject();
                bottlingObj.CaseCapacity = 12;
                bottlingObj.CaseQuantity = 9.42f;
                bottlingObj.BottleCapacity = 750f;
                bottlingObj.BottleQuantity = 113;

                prodBottl.BottlingInfo = bottlingObj;

                prodBottl.GainLoss = .10f;

                prodBottl.FillTestList = null;

                int productionBottlingId = _dl.CreateProduction(prodBottl, _userId); // here productionId is overriden with a new productionId of the new Gauged record
                tupleL.Add(Tuple.Create(productionBottlingId, Table.Production));

                #endregion

                #region Reports setup

                // Report Header
                ReportHeader reportHeaderE = new ReportHeader();
                reportHeaderE.ProprietorName = "Test Distillery";
                reportHeaderE.EIN = "12-3456789";
                reportHeaderE.ReportDate = "September 2017";
                reportHeaderE.PlantAddress = "123 Cognac Drive Renton WASHINGTON 98059";
                reportHeaderE.DSP = "DSP-WA-21086";

                /* PRODUCTION REPORT */

                ProdReportPart1 part1E = new ProdReportPart1();
                part1E.ProccessingAcct = 18f;
                part1E.ProducedTotal = 18f;
                part1E.Recd4RedistilL17 = 0f;
                part1E.Recd4RedistilaltionL15 = 0f;
                part1E.StorageAcct = 0f;
                part1E.SpiritTypeReportingID = 3;
                part1E.UnfinishedSpiritsEndOfQuarterL17 = 0f;

                ProdReportParts2Through4 part2thru4 = new ProdReportParts2Through4();
                part2thru4.KindOfMaterial = "GrapeBrandy";
                part2thru4.MaterialKindReportingID = 94;
                part2thru4.NewCoop = false;
                part2thru4.UsedCoop = false;
                part2thru4.ProofGallons = 18f;
                part2thru4.SpiritTypeReportingID = 3;
                part2thru4.Tanks = false;

                ProdReportPart5 part5 = new ProdReportPart5();
                part5.KindofSpirits = "";
                part5.Proof = 0f;

                ProductionReportingObject actualProdReportObject = new ProductionReportingObject();

                actualProdReportObject = _dl.GetProductionReportData(start, end, _userId);

                ProcessingReportingObject actualProcessingReportO = new ProcessingReportingObject();

                #endregion

                // Assert

                #region Production Report Before Deletion

                Tuple<int, float> accumulatedProofGL4SpiritTypeReportingId = new Tuple<int, float>(11, purchO.ProofGallon);

                List<Tuple<int, float>> accumulatedProofGL4SpiritTypeReportingIdL = new List<Tuple<int, float>>();

                accumulatedProofGL4SpiritTypeReportingIdL.Add(accumulatedProofGL4SpiritTypeReportingId);

                // verify Production report header
                Assert.AreEqual(reportHeaderE.DSP, actualProdReportObject.Header.DSP);
                Assert.AreEqual(reportHeaderE.EIN, actualProdReportObject.Header.EIN);
                Assert.AreEqual(reportHeaderE.PlantAddress, actualProdReportObject.Header.PlantAddress);
                Assert.AreEqual(reportHeaderE.ProprietorName, actualProdReportObject.Header.ProprietorName);
                Assert.AreEqual(reportHeaderE.ReportDate, actualProdReportObject.Header.ReportDate);

                // verify Production report Part 1
                Assert.AreEqual(part1E.ProccessingAcct, actualProdReportObject.Part1List[0].ProccessingAcct);
                Assert.AreEqual(part1E.StorageAcct, actualProdReportObject.Part1List[0].StorageAcct);
                Assert.AreEqual(part1E.ProducedTotal, actualProdReportObject.Part1List[0].ProducedTotal);
                Assert.AreEqual(part1E.Recd4RedistilL17, actualProdReportObject.Part1List[0].Recd4RedistilL17);

                var expectedWine = accumulatedProofGL4SpiritTypeReportingIdL.Find(l => l.Item1 == 11);
                var actualWine = actualProdReportObject.Part1List.Find(x => x.SpiritTypeReportingID == expectedWine.Item1);

                if (actualWine == null)
                {
                    Assert.AreNotEqual(null, expectedWine, "No records in actualAlcohol190Plus for SpiritTypeID");
                }
                else
                {
                    Assert.AreEqual(expectedWine.Item2, actualWine.Recd4RedistilaltionL15);
                    Assert.AreEqual("Wine", actualWine.SpiritCatName);
                }

                Assert.AreEqual(part1E.UnfinishedSpiritsEndOfQuarterL17, actualProdReportObject.Part1List[0].UnfinishedSpiritsEndOfQuarterL17);

                // verify Production report Part 2 trough 4
                Assert.AreEqual(part2thru4.KindOfMaterial, actualProdReportObject.Part2Through4List[0].KindOfMaterial);
                Assert.AreEqual(part2thru4.MaterialKindReportingID, actualProdReportObject.Part2Through4List[0].MaterialKindReportingID);
                Assert.AreEqual(part2thru4.NewCoop, actualProdReportObject.Part2Through4List[0].NewCoop);
                Assert.AreEqual(part2thru4.UsedCoop, actualProdReportObject.Part2Through4List[0].UsedCoop);
                Assert.AreEqual(part2thru4.ProofGallons, actualProdReportObject.Part2Through4List[0].ProofGallons);
                Assert.AreEqual(part2thru4.SpiritTypeReportingID, actualProdReportObject.Part2Through4List[0].SpiritTypeReportingID);
                Assert.AreEqual(part2thru4.Tanks, actualProdReportObject.Part2Through4List[0].Tanks);

                // verify part 5
                Assert.AreEqual("Wine", actualProdReportObject.part5List[0].KindofSpirits);
                Assert.AreEqual(18, actualProdReportObject.part5List[0].Proof);

                // verify Production report Part 6 - no data should be present in this scenario
                Assert.AreEqual(0, actualProdReportObject.ProdReportPart6List.Count);

                #endregion

                #region Processing Report Before Deletion
                /* PROCESING REPORT */
                ProcessingReportingObject actualProcessingReportObject = new ProcessingReportingObject();

                ProcessReportingPart1 processingReportP1 = new ProcessReportingPart1();
                processingReportP1.AmtBottledPackaged = 17.9f;
                processingReportP1.BulkIngredients = "spirit";
                processingReportP1.Destroyed = 0f;
                processingReportP1.Dumped4Processing = 0f;
                processingReportP1.Gains = 0f;
                processingReportP1.Losses = 0.1f;
                processingReportP1.OnHandEndofMonth = 0f;
                processingReportP1.OnHandFirstofMonth = 0f;
                processingReportP1.Recd4Process = 18f;
                processingReportP1.Transf2Prod4Redistil = 0f;
                processingReportP1.Used4Redistil = 0f;
                processingReportP1.WineMixedWithSpirit = 0f;

                ProcessReportingPart2 processingReportP2 = new ProcessReportingPart2();
                processingReportP2.AmtBottledPackaged = 17.9f;
                processingReportP2.Destroyed = 0f;
                processingReportP2.Dumped4Processing = 0f;
                processingReportP2.FinishedProduct = "bottled";
                processingReportP2.InventoryOverage = 0f;
                processingReportP2.InventoryShortage = 0f;
                processingReportP2.OnHandEndofMonth = 17.9f;
                processingReportP2.OnHandFirstofMonth = 0f;
                processingReportP2.Recd4Process = 0f;
                processingReportP2.RecordedLosses = 0f;
                processingReportP2.TaxWithdrawn = 0f;
                processingReportP2.Transf2Prod4Redistil = 0f;

                ProcessReportingPart4 processingReportP4 = new ProcessReportingPart4();
                processingReportP4.AlcoholNeutral = 0f;
                processingReportP4.BlendedLightWhiskey = 0f;
                processingReportP4.BlendedOtherWhiskey = 0f;
                processingReportP4.BlendedStraightWhiskey = 0f;
                processingReportP4.BlendedWhiskeyWithLight = 0f;
                processingReportP4.BlendedWhiskeyWithNeutral = 0f;
                processingReportP4.Brandy170Under = 22.39f;
                processingReportP4.BrandyOver170 = 0f;
                processingReportP4.Cocktail = 0f;
                processingReportP4.DomesticWhiskey160Under = 0f;
                processingReportP4.DomesticWhiskeyOver160 = 0f;
                processingReportP4.Gin = 0f;
                processingReportP4.ImportedWhiskeyCanadian = 0f;
                processingReportP4.ImportedWhiskeyIrish = 0f;
                processingReportP4.ImportedWhiskeyScotch = 0f;
                processingReportP4.Liqueur = 0f;
                processingReportP4.ProcessingReportTypeName = "BRANDY DISTILLED AT 170 AND UNDER";
                processingReportP4.ProcessingSpirits = "bottled";
                processingReportP4.ProcessingTypeID = 12;
                processingReportP4.RumDomestic = 0f;
                processingReportP4.RumOtherImported = 0f;
                processingReportP4.RumPuertoRican = 0f;
                processingReportP4.RumVirginIslands = 0f;
                processingReportP4.StateID = 5;
                processingReportP4.Tequila = 0f;
                processingReportP4.Vodka = 0f;

                actualProcessingReportObject = _dl.GetProcessingReportData(start, end, _userId);

                Assert.AreEqual(reportHeaderE.DSP, actualProcessingReportObject.Header.DSP);
                Assert.AreEqual(reportHeaderE.EIN, actualProcessingReportObject.Header.EIN);
                Assert.AreEqual(reportHeaderE.PlantAddress, actualProcessingReportObject.Header.PlantAddress);
                Assert.AreEqual(reportHeaderE.ProprietorName, actualProcessingReportObject.Header.ProprietorName);
                Assert.AreEqual(reportHeaderE.ReportDate, actualProcessingReportObject.Header.ReportDate);

                Assert.AreEqual(processingReportP1.AmtBottledPackaged, actualProcessingReportObject.Part1.AmtBottledPackaged);
                Assert.AreEqual(processingReportP1.BulkIngredients, actualProcessingReportObject.Part1.BulkIngredients);
                Assert.AreEqual(processingReportP1.Destroyed, actualProcessingReportObject.Part1.Destroyed);
                Assert.AreEqual(processingReportP1.Dumped4Processing, actualProcessingReportObject.Part1.Dumped4Processing);
                Assert.AreEqual(processingReportP1.Gains, actualProcessingReportObject.Part1.Gains);
                Assert.AreEqual(processingReportP1.Losses, actualProcessingReportObject.Part1.Losses);
                Assert.AreEqual(processingReportP1.OnHandEndofMonth, actualProcessingReportObject.Part1.OnHandEndofMonth);
                Assert.AreEqual(processingReportP1.OnHandFirstofMonth, actualProcessingReportObject.Part1.OnHandFirstofMonth);
                Assert.AreEqual(processingReportP1.Recd4Process, actualProcessingReportObject.Part1.Recd4Process);
                Assert.AreEqual(processingReportP1.Transf2Prod4Redistil, actualProcessingReportObject.Part1.Transf2Prod4Redistil);
                Assert.AreEqual(processingReportP1.Used4Redistil, actualProcessingReportObject.Part1.Used4Redistil);
                Assert.AreEqual(processingReportP1.WineMixedWithSpirit, actualProcessingReportObject.Part1.WineMixedWithSpirit);

                Assert.AreEqual(processingReportP2.AmtBottledPackaged, actualProcessingReportObject.Part2.AmtBottledPackaged);
                Assert.AreEqual(processingReportP2.Destroyed, actualProcessingReportObject.Part2.Destroyed);
                Assert.AreEqual(processingReportP2.Dumped4Processing, actualProcessingReportObject.Part2.Dumped4Processing);
                Assert.AreEqual(processingReportP2.FinishedProduct, actualProcessingReportObject.Part2.FinishedProduct);
                Assert.AreEqual(processingReportP2.InventoryOverage, actualProcessingReportObject.Part2.InventoryOverage);
                Assert.AreEqual(processingReportP2.InventoryShortage, actualProcessingReportObject.Part2.InventoryShortage);
                Assert.AreEqual(processingReportP2.OnHandEndofMonth, actualProcessingReportObject.Part2.OnHandEndofMonth);
                Assert.AreEqual(processingReportP2.OnHandFirstofMonth, actualProcessingReportObject.Part2.OnHandFirstofMonth);
                Assert.AreEqual(processingReportP2.Recd4Process, actualProcessingReportObject.Part2.Recd4Process);
                Assert.AreEqual(processingReportP2.RecordedLosses, actualProcessingReportObject.Part2.RecordedLosses);
                Assert.AreEqual(processingReportP2.TaxWithdrawn, actualProcessingReportObject.Part2.TaxWithdrawn);
                Assert.AreEqual(processingReportP2.Transf2Prod4Redistil, actualProcessingReportObject.Part2.Transf2Prod4Redistil);

                Assert.AreEqual(processingReportP4.AlcoholNeutral, actualProcessingReportObject.Part4List[0].AlcoholNeutral);
                Assert.AreEqual(processingReportP4.BlendedLightWhiskey, actualProcessingReportObject.Part4List[0].BlendedLightWhiskey);
                Assert.AreEqual(processingReportP4.BlendedOtherWhiskey, actualProcessingReportObject.Part4List[0].BlendedOtherWhiskey);
                Assert.AreEqual(processingReportP4.BlendedStraightWhiskey, actualProcessingReportObject.Part4List[0].BlendedStraightWhiskey);
                Assert.AreEqual(processingReportP4.BlendedWhiskeyWithLight, actualProcessingReportObject.Part4List[0].BlendedWhiskeyWithLight);
                Assert.AreEqual(processingReportP4.BlendedWhiskeyWithNeutral, actualProcessingReportObject.Part4List[0].BlendedWhiskeyWithNeutral);
                Assert.AreEqual(processingReportP4.Brandy170Under, actualProcessingReportObject.Part4List[1].Brandy170Under);
                Assert.AreEqual(processingReportP4.BrandyOver170, actualProcessingReportObject.Part4List[0].BrandyOver170);
                Assert.AreEqual(processingReportP4.Cocktail, actualProcessingReportObject.Part4List[0].Cocktail);
                Assert.AreEqual(processingReportP4.DomesticWhiskey160Under, actualProcessingReportObject.Part4List[0].DomesticWhiskey160Under);
                Assert.AreEqual(processingReportP4.DomesticWhiskeyOver160, actualProcessingReportObject.Part4List[0].DomesticWhiskeyOver160);
                Assert.AreEqual(processingReportP4.Gin, actualProcessingReportObject.Part4List[0].Gin);
                Assert.AreEqual(processingReportP4.ImportedWhiskeyCanadian, actualProcessingReportObject.Part4List[0].ImportedWhiskeyCanadian);
                Assert.AreEqual(processingReportP4.ImportedWhiskeyIrish, actualProcessingReportObject.Part4List[0].ImportedWhiskeyIrish);
                Assert.AreEqual(processingReportP4.ImportedWhiskeyScotch, actualProcessingReportObject.Part4List[0].ImportedWhiskeyScotch);
                Assert.AreEqual(processingReportP4.Liqueur, actualProcessingReportObject.Part4List[0].Liqueur);
                Assert.AreEqual(processingReportP4.ProcessingReportTypeName, actualProcessingReportObject.Part4List[0].ProcessingReportTypeName);
                Assert.AreEqual(processingReportP4.ProcessingSpirits, actualProcessingReportObject.Part4List[1].ProcessingSpirits);
                Assert.AreEqual(processingReportP4.ProcessingTypeID, actualProcessingReportObject.Part4List[0].ProcessingTypeID);
                Assert.AreEqual(processingReportP4.RumDomestic, actualProcessingReportObject.Part4List[0].RumDomestic);
                Assert.AreEqual(processingReportP4.RumDomestic, actualProcessingReportObject.Part4List[0].RumDomestic);
                Assert.AreEqual(processingReportP4.RumOtherImported, actualProcessingReportObject.Part4List[0].RumOtherImported);
                Assert.AreEqual(processingReportP4.RumPuertoRican, actualProcessingReportObject.Part4List[0].RumPuertoRican);
                Assert.AreEqual(processingReportP4.StateID, actualProcessingReportObject.Part4List[1].StateID);
                Assert.AreEqual(processingReportP4.Tequila, actualProcessingReportObject.Part4List[0].Tequila);
                Assert.AreEqual(processingReportP4.Vodka, actualProcessingReportObject.Part4List[0].Vodka);
                #endregion

                #region Storage Report Before Deletion
                StorageReport actualStorageReportObject = new StorageReport();

                StorageReportCategory storageReportBody = new StorageReportCategory();
                storageReportBody.CategoryName = "Wine";
                storageReportBody.r17_TransferredToProcessingAccount = 0f;
                storageReportBody.r18_TransferredToProductionAccount = 18f;
                storageReportBody.r19_TransferredToOtherBondedPremises = 0;
                storageReportBody.r1_OnHandFirstOfMonth = 0f;
                storageReportBody.r20_Destroyed = 0f;
                storageReportBody.r22_OtherLosses = 0f;
                storageReportBody.r23_OnHandEndOfMonth = 0f;
                storageReportBody.r24_Lines7Through23 = 18f;
                storageReportBody.r2_DepositedInBulkStorage = 18f;
                storageReportBody.r4_ReturnedToBulkStorage = 0f;
                storageReportBody.r6_TotalLines1Through5 = 18f;
                storageReportBody.r7_TaxPaid = 0f;

                actualStorageReportObject = _dl.GetStorageReportData(start, end, _userId);

                // verify Storage report Header
                Assert.AreEqual(reportHeaderE.DSP, actualStorageReportObject.Header.DSP);
                Assert.AreEqual(reportHeaderE.EIN, actualStorageReportObject.Header.EIN);
                Assert.AreEqual(reportHeaderE.PlantAddress, actualStorageReportObject.Header.PlantAddress);
                Assert.AreEqual(reportHeaderE.ProprietorName, actualStorageReportObject.Header.ProprietorName);
                Assert.AreEqual(reportHeaderE.ReportDate, actualStorageReportObject.Header.ReportDate);

                // verify Storage body
                Assert.AreEqual(storageReportBody.CategoryName, actualStorageReportObject.ReportBody[0].CategoryName);
                Assert.AreEqual(storageReportBody.r17_TransferredToProcessingAccount, actualStorageReportObject.ReportBody[0].r17_TransferredToProcessingAccount);
                Assert.AreEqual(storageReportBody.r18_TransferredToProductionAccount, actualStorageReportObject.ReportBody[0].r18_TransferredToProductionAccount);
                Assert.AreEqual(storageReportBody.r19_TransferredToOtherBondedPremises, actualStorageReportObject.ReportBody[0].r19_TransferredToOtherBondedPremises);
                Assert.AreEqual(storageReportBody.r1_OnHandFirstOfMonth, actualStorageReportObject.ReportBody[0].r1_OnHandFirstOfMonth);
                Assert.AreEqual(storageReportBody.r20_Destroyed, actualStorageReportObject.ReportBody[0].r20_Destroyed);
                Assert.AreEqual(storageReportBody.r22_OtherLosses, actualStorageReportObject.ReportBody[0].r22_OtherLosses);
                Assert.AreEqual(storageReportBody.r23_OnHandEndOfMonth, actualStorageReportObject.ReportBody[0].r23_OnHandEndOfMonth);
                Assert.AreEqual(storageReportBody.r24_Lines7Through23, actualStorageReportObject.ReportBody[0].r24_Lines7Through23);
                Assert.AreEqual(storageReportBody.r2_DepositedInBulkStorage, actualStorageReportObject.ReportBody[0].r2_DepositedInBulkStorage);
                Assert.AreEqual(storageReportBody.r4_ReturnedToBulkStorage, actualStorageReportObject.ReportBody[0].r4_ReturnedToBulkStorage);
                Assert.AreEqual(storageReportBody.r6_TotalLines1Through5, actualStorageReportObject.ReportBody[0].r6_TotalLines1Through5);
                Assert.AreEqual(storageReportBody.r7_TaxPaid, actualStorageReportObject.ReportBody[0].r7_TaxPaid);

                #endregion

                // let's verify the values in production report after we deleted bottling
                bool bottlingDeleted = _dl.DeleteProduction(prodBottl, _userId);

                #region Blending and Distiliing after Bottling deletion

                var amounts =
                    (from prod in _db.Production
                     where prod.ProductionID == productionBlendingId
                     join volume in _db.Volume on prod.VolumeID equals volume.VolumeID into volume_join
                     from volume in volume_join.DefaultIfEmpty()
                     join weight in _db.Weight on prod.WeightID equals weight.WeightID into weight_join
                     from weight in weight_join.DefaultIfEmpty()
                     join alcohol in _db.Alcohol on prod.AlcoholID equals alcohol.AlcoholID into alcohol_join
                     from alcohol in alcohol_join.DefaultIfEmpty()
                     join proof in _db.Proof on prod.ProofID equals proof.ProofID into proof_join
                     from proof in proof_join.DefaultIfEmpty()
                     select new
                     {
                         volume = (float?)volume.Value ?? (float?)0,
                         weight = (float?)weight.Value ?? (float?)0,
                         alcohol = (float?)alcohol.Value ?? (float?)0,
                         proof = (float?)proof.Value ?? (float?)0
                     }).FirstOrDefault();

                if (amounts != null)
                {
                    Assert.AreEqual(prodBlend.Quantity, amounts.volume);
                    Assert.AreEqual(prodBlend.VolumeByWeight, amounts.weight);
                    Assert.AreEqual(prodBlend.AlcoholContent, amounts.alcohol);
                    Assert.AreEqual(prodBlend.ProofGallon, amounts.proof);
                }
                else
                {
                    Assert.Inconclusive("amounts query yielded no results so could not perform this part of the test");
                }

                var isBlendRecord =
                    (from prod in _db.Production
                     where prod.ProductionID == productionBlendingId
                     select prod).FirstOrDefault();

                if (isBlendRecord == null)
                {
                    Assert.IsNotNull(isBlendRecord, "blending record should exist after the deletion of Bottling record");
                }

                var isDistilRecord =
                    (from prod in _db.Production
                     where prod.ProductionID == productionFinalDistillationId
                     select prod).FirstOrDefault();

                if (isDistilRecord == null)
                {
                    Assert.IsNotNull(isDistilRecord, "distilling record should exist after the deletion of Bottling record");
                }
                #endregion

                #region Production Report After Deletion
                // verify Production report Part 1
                Assert.AreEqual(part1E.ProccessingAcct, actualProdReportObject.Part1List[0].ProccessingAcct);
                Assert.AreEqual(part1E.StorageAcct, actualProdReportObject.Part1List[0].StorageAcct);
                Assert.AreEqual(part1E.ProducedTotal, actualProdReportObject.Part1List[0].ProducedTotal);
                Assert.AreEqual(part1E.Recd4RedistilL17, actualProdReportObject.Part1List[0].Recd4RedistilL17);

                expectedWine = accumulatedProofGL4SpiritTypeReportingIdL.Find(l => l.Item1 == 11);
                actualWine = actualProdReportObject.Part1List.Find(x => x.SpiritTypeReportingID == expectedWine.Item1);

                if (actualWine == null)
                {
                    Assert.AreNotEqual(null, expectedWine, "No records in actualAlcohol190Plus for SpiritTypeID");
                }
                else
                {
                    Assert.AreEqual(expectedWine.Item2, actualWine.Recd4RedistilaltionL15);
                    Assert.AreEqual("Wine", actualWine.SpiritCatName);
                }

                Assert.AreEqual(part1E.UnfinishedSpiritsEndOfQuarterL17, actualProdReportObject.Part1List[0].UnfinishedSpiritsEndOfQuarterL17);

                // verify Production report Part 2 trough 4
                Assert.AreEqual(part2thru4.KindOfMaterial, actualProdReportObject.Part2Through4List[0].KindOfMaterial);
                Assert.AreEqual(part2thru4.MaterialKindReportingID, actualProdReportObject.Part2Through4List[0].MaterialKindReportingID);
                Assert.AreEqual(part2thru4.NewCoop, actualProdReportObject.Part2Through4List[0].NewCoop);
                Assert.AreEqual(part2thru4.UsedCoop, actualProdReportObject.Part2Through4List[0].UsedCoop);
                Assert.AreEqual(part2thru4.ProofGallons, actualProdReportObject.Part2Through4List[0].ProofGallons);
                Assert.AreEqual(part2thru4.SpiritTypeReportingID, actualProdReportObject.Part2Through4List[0].SpiritTypeReportingID);
                Assert.AreEqual(part2thru4.Tanks, actualProdReportObject.Part2Through4List[0].Tanks);

                // verify part 5
                Assert.AreEqual("Wine", actualProdReportObject.part5List[0].KindofSpirits);
                Assert.AreEqual(18, actualProdReportObject.part5List[0].Proof);

                // verify Production report Part 6 - no data should be present in this scenario
                Assert.AreEqual(0, actualProdReportObject.ProdReportPart6List.Count);
                #endregion

                #region Processing Report After Deletion
                /* PROCESING REPORT */
                actualProcessingReportObject = new ProcessingReportingObject();

                processingReportP1 = new ProcessReportingPart1();
                processingReportP1.AmtBottledPackaged = 0f;
                processingReportP1.BulkIngredients = "spirit";
                processingReportP1.Destroyed = 0f;
                processingReportP1.Dumped4Processing = 0f;
                processingReportP1.Gains = 0f;
                processingReportP1.Losses = 0f;
                processingReportP1.OnHandEndofMonth = 18f;
                processingReportP1.OnHandFirstofMonth = 0f;
                processingReportP1.Recd4Process = 18f;
                processingReportP1.Transf2Prod4Redistil = 0f;
                processingReportP1.Used4Redistil = 0f;
                processingReportP1.WineMixedWithSpirit = 0f;

                processingReportP2 = new ProcessReportingPart2();
                processingReportP2.AmtBottledPackaged = 0f;
                processingReportP2.Destroyed = 0f;
                processingReportP2.Dumped4Processing = 0f;
                processingReportP2.FinishedProduct = "bottled";
                processingReportP2.InventoryOverage = 0f;
                processingReportP2.InventoryShortage = 0f;
                processingReportP2.OnHandEndofMonth = 0f;
                processingReportP2.OnHandFirstofMonth = 0f;
                processingReportP2.Recd4Process = 0f;
                processingReportP2.RecordedLosses = 0f;
                processingReportP2.TaxWithdrawn = 0f;
                processingReportP2.Transf2Prod4Redistil = 0f;

                processingReportP4 = new ProcessReportingPart4();
                processingReportP4.ProcessingSpirits = "bulkSpiritDumped";
                processingReportP4.AlcoholNeutral = 0f;
                processingReportP4.BlendedLightWhiskey = 0f;
                processingReportP4.BlendedOtherWhiskey = 0f;
                processingReportP4.BlendedStraightWhiskey = 0f;
                processingReportP4.BlendedWhiskeyWithLight = 0f;
                processingReportP4.BlendedWhiskeyWithNeutral = 0f;
                processingReportP4.Brandy170Under = 18f;
                processingReportP4.BrandyOver170 = 0f;
                processingReportP4.Cocktail = 0f;
                processingReportP4.DomesticWhiskey160Under = 0f;
                processingReportP4.DomesticWhiskeyOver160 = 0f;
                processingReportP4.Gin = 0f;
                processingReportP4.ImportedWhiskeyCanadian = 0f;
                processingReportP4.ImportedWhiskeyIrish = 0f;
                processingReportP4.ImportedWhiskeyScotch = 0f;
                processingReportP4.Liqueur = 0f;
                processingReportP4.ProcessingReportTypeName = "BRANDY DISTILLED AT 170 AND UNDER";
                processingReportP4.ProcessingTypeID = 12;
                processingReportP4.RumDomestic = 0f;
                processingReportP4.RumOtherImported = 0f;
                processingReportP4.RumPuertoRican = 0f;
                processingReportP4.RumVirginIslands = 0f;
                processingReportP4.StateID = 4;
                processingReportP4.Tequila = 0f;
                processingReportP4.Vodka = 0f;

                actualProcessingReportObject = _dl.GetProcessingReportData(start, end, _userId);

                Assert.AreEqual(processingReportP1.AmtBottledPackaged, actualProcessingReportObject.Part1.AmtBottledPackaged);
                Assert.AreEqual(processingReportP1.BulkIngredients, actualProcessingReportObject.Part1.BulkIngredients);
                Assert.AreEqual(processingReportP1.Destroyed, actualProcessingReportObject.Part1.Destroyed);
                Assert.AreEqual(processingReportP1.Dumped4Processing, actualProcessingReportObject.Part1.Dumped4Processing);
                Assert.AreEqual(processingReportP1.Gains, actualProcessingReportObject.Part1.Gains);
                Assert.AreEqual(processingReportP1.Losses, actualProcessingReportObject.Part1.Losses);
                Assert.AreEqual(processingReportP1.OnHandEndofMonth, actualProcessingReportObject.Part1.OnHandEndofMonth);
                Assert.AreEqual(processingReportP1.OnHandFirstofMonth, actualProcessingReportObject.Part1.OnHandFirstofMonth);
                Assert.AreEqual(processingReportP1.Recd4Process, actualProcessingReportObject.Part1.Recd4Process);
                Assert.AreEqual(processingReportP1.Transf2Prod4Redistil, actualProcessingReportObject.Part1.Transf2Prod4Redistil);
                Assert.AreEqual(processingReportP1.Used4Redistil, actualProcessingReportObject.Part1.Used4Redistil);
                Assert.AreEqual(processingReportP1.WineMixedWithSpirit, actualProcessingReportObject.Part1.WineMixedWithSpirit);

                Assert.AreEqual(processingReportP2.AmtBottledPackaged, actualProcessingReportObject.Part2.AmtBottledPackaged);
                Assert.AreEqual(processingReportP2.Destroyed, actualProcessingReportObject.Part2.Destroyed);
                Assert.AreEqual(processingReportP2.Dumped4Processing, actualProcessingReportObject.Part2.Dumped4Processing);
                Assert.AreEqual(processingReportP2.FinishedProduct, actualProcessingReportObject.Part2.FinishedProduct);
                Assert.AreEqual(processingReportP2.InventoryOverage, actualProcessingReportObject.Part2.InventoryOverage);
                Assert.AreEqual(processingReportP2.InventoryShortage, actualProcessingReportObject.Part2.InventoryShortage);
                Assert.AreEqual(processingReportP2.OnHandEndofMonth, actualProcessingReportObject.Part2.OnHandEndofMonth);
                Assert.AreEqual(processingReportP2.OnHandFirstofMonth, actualProcessingReportObject.Part2.OnHandFirstofMonth);
                Assert.AreEqual(processingReportP2.Recd4Process, actualProcessingReportObject.Part2.Recd4Process);
                Assert.AreEqual(processingReportP2.RecordedLosses, actualProcessingReportObject.Part2.RecordedLosses);
                Assert.AreEqual(processingReportP2.TaxWithdrawn, actualProcessingReportObject.Part2.TaxWithdrawn);
                Assert.AreEqual(processingReportP2.Transf2Prod4Redistil, actualProcessingReportObject.Part2.Transf2Prod4Redistil);

                var actualproccessingReportPart4BottledObject = actualProcessingReportObject.Part4List.Find(x => x.ProcessingSpirits == "bottled");

                if (actualproccessingReportPart4BottledObject != null)
                {
                    Assert.IsNull(actualproccessingReportPart4BottledObject, "there should be no bottling record");
                }

                var actualproccessingReportPart4Object = actualProcessingReportObject.Part4List.Find(x => x.ProcessingSpirits == "bulkSpiritDumped");

                if (actualproccessingReportPart4Object == null)
                {
                    Assert.IsNotNull(actualproccessingReportPart4Object);
                }
                else
                {
                    Assert.AreEqual(processingReportP4.AlcoholNeutral, actualproccessingReportPart4Object.AlcoholNeutral);
                    Assert.AreEqual(processingReportP4.BlendedLightWhiskey, actualproccessingReportPart4Object.BlendedLightWhiskey);
                    Assert.AreEqual(processingReportP4.BlendedOtherWhiskey, actualproccessingReportPart4Object.BlendedOtherWhiskey);
                    Assert.AreEqual(processingReportP4.BlendedStraightWhiskey, actualproccessingReportPart4Object.BlendedStraightWhiskey);
                    Assert.AreEqual(processingReportP4.BlendedWhiskeyWithLight, actualproccessingReportPart4Object.BlendedWhiskeyWithLight);
                    Assert.AreEqual(processingReportP4.BlendedWhiskeyWithNeutral, actualproccessingReportPart4Object.BlendedWhiskeyWithNeutral);
                    Assert.AreEqual(processingReportP4.Brandy170Under, actualproccessingReportPart4Object.Brandy170Under);
                    Assert.AreEqual(processingReportP4.BrandyOver170, actualproccessingReportPart4Object.BrandyOver170);
                    Assert.AreEqual(processingReportP4.Cocktail, actualproccessingReportPart4Object.Cocktail);
                    Assert.AreEqual(processingReportP4.DomesticWhiskey160Under, actualproccessingReportPart4Object.DomesticWhiskey160Under);
                    Assert.AreEqual(processingReportP4.DomesticWhiskeyOver160, actualproccessingReportPart4Object.DomesticWhiskeyOver160);
                    Assert.AreEqual(processingReportP4.Gin, actualproccessingReportPart4Object.Gin);
                    Assert.AreEqual(processingReportP4.ImportedWhiskeyCanadian, actualproccessingReportPart4Object.ImportedWhiskeyCanadian);
                    Assert.AreEqual(processingReportP4.ImportedWhiskeyIrish, actualproccessingReportPart4Object.ImportedWhiskeyIrish);
                    Assert.AreEqual(processingReportP4.ImportedWhiskeyScotch, actualproccessingReportPart4Object.ImportedWhiskeyScotch);
                    Assert.AreEqual(processingReportP4.Liqueur, actualproccessingReportPart4Object.Liqueur);
                    Assert.AreEqual(processingReportP4.ProcessingReportTypeName, actualproccessingReportPart4Object.ProcessingReportTypeName);
                    Assert.AreEqual(processingReportP4.ProcessingSpirits, actualproccessingReportPart4Object.ProcessingSpirits);
                    Assert.AreEqual(processingReportP4.ProcessingTypeID, actualproccessingReportPart4Object.ProcessingTypeID);
                    Assert.AreEqual(processingReportP4.RumDomestic, actualproccessingReportPart4Object.RumDomestic);
                    Assert.AreEqual(processingReportP4.RumDomestic, actualproccessingReportPart4Object.RumDomestic);
                    Assert.AreEqual(processingReportP4.RumOtherImported, actualproccessingReportPart4Object.RumOtherImported);
                    Assert.AreEqual(processingReportP4.RumPuertoRican, actualproccessingReportPart4Object.RumPuertoRican);
                    Assert.AreEqual(processingReportP4.StateID, actualproccessingReportPart4Object.StateID);
                    Assert.AreEqual(processingReportP4.Tequila, actualproccessingReportPart4Object.Tequila);
                    Assert.AreEqual(processingReportP4.Vodka, actualproccessingReportPart4Object.Vodka);
                }

                #endregion

                #region Storage Report After Deletion

                #endregion
            }
            finally
            {
                // Cleanup
                foreach (var i in tupleL)
                {
                    TestRecordCleanup(i.Item1, i.Item2);
                }
            }
        }

        /// <summary>
        /// This test method verifies that when Blending record
        /// is deleted, all other records amounts' that were involved in making this
        /// blending record originally, are reinstated. Effectively, we should be re-instating values for Distilling records
        /// In addition, we are verifying reports update it's values as well.
        /// </summary>
        [TestMethod()]
        public void DeleteWorkFlow_BuyWine_Distil_DistilAgain_Blend_DeleteBlending_EnsureOtherRecords_Intact()
        {
            // Arrange
            List<Tuple<int/*recordId*/, Table/*table enum vaue*/>> tupleL = new List<Tuple<int, Table>>();
            int spiritId = 0;
            int vendorId = 0;
            int storageId = 0;
            int wineMaterialId = 0;
            int waterMaterialId = 0;
            int purchaseId = 0;
            int productionId = 0;

            DateTime start = new DateTime(2017, 09, 01);
            DateTime end = new DateTime(2017, 09, 30);

            try
            {
                #region Dictionary
                //  dictionary setup
                SpiritObject spirit = new SpiritObject();
                spirit.SpiritName = "Brandy Under 170";
                spirit.ProcessingReportTypeID = 12;

                spiritId = _dl.CreateSpirit(_userId, spirit);

                // setup Vendor object
                VendorObject vendor = new VendorObject();
                vendor.VendorName = "testVendor";

                vendorId = _dl.CreateVendor(_userId, vendor);
                tupleL.Add(Tuple.Create(vendorId, Table.Vendor));

                // setup Storage Object
                StorageObject storage = new StorageObject();
                storage.StorageName = "testStorage";
                storage.SerialNumber = "2H29NNS";

                storageId = _dl.CreateStorage(_userId, storage);
                tupleL.Add(Tuple.Create(storageId, Table.Storage));


                // setup Material Object
                // wine
                {
                    RawMaterialObject wineMaterial = new RawMaterialObject();
                    wineMaterial.RawMaterialName = "Wine For Brandy";
                    wineMaterial.MaterialCategoryID = 2;
                    wineMaterial.UnitType = "gal";
                    wineMaterial.UnitTypeId = 1;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Fermented = true;
                    wineMaterial.PurchaseMaterialTypes = materialBoolTypes;

                    wineMaterialId = _dl.CreateRawMaterial(_userId, wineMaterial);
                    tupleL.Add(Tuple.Create(wineMaterialId, Table.MaterialDict));
                }

                // water
                {
                    RawMaterialObject waterMaterial = new RawMaterialObject();
                    waterMaterial.RawMaterialName = "Water";
                    waterMaterial.UnitType = "gal";
                    waterMaterial.UnitTypeId = 1;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Additive = true;
                    waterMaterial.PurchaseMaterialTypes = materialBoolTypes;

                    waterMaterialId = _dl.CreateRawMaterial(_userId, waterMaterial);
                    tupleL.Add(Tuple.Create(waterMaterialId, Table.MaterialDict));
                }
                #endregion

                #region Purchase
                // create Purchase Record (minimal required fields)
                PurchaseObject purchO = new PurchaseObject();
                purchO.PurBatchName = "Feremented Purchase";
                purchO.PurchaseType = "Fermented";
                purchO.PurchaseDate = new DateTime(2017, 09, 1);
                purchO.Quantity = 100f; // 100 gallons
                purchO.VolumeByWeight = 0f;
                purchO.AlcoholContent = 9f;
                purchO.ProofGallon = 18f;
                purchO.RecordId = wineMaterialId;
                purchO.Price = 350f;
                purchO.VendorId = vendorId;

                List<StorageObject> stoL = new List<StorageObject>();
                StorageObject sto = new StorageObject();
                sto.StorageId = storageId;
                stoL.Add(sto);
                purchO.Storage = stoL;

                purchO.SpiritTypeReportingID = 11;
                purchO.Gauged = true;

                purchaseId = _dl.CreatePurchase(purchO, _userId);
                tupleL.Add(Tuple.Create(purchaseId, Table.Purchase));
                #endregion

                #region Production
                // create 1st Production Distillation Record and don't mark it as Gauged
                ProductionObject prodO = new ProductionObject();
                prodO.BatchName = "test1stDistillRun";
                prodO.ProductionDate = new DateTime(2017, 09, 3);
                prodO.ProductionStart = new DateTime(2017, 09, 3);
                prodO.ProductionEnd = new DateTime(2017, 09, 3);
                prodO.SpiritCutId = 11; // mixed
                prodO.Gauged = false;
                prodO.ProductionType = "Distillation";
                prodO.Quantity = 50f; //50 gallons of alcohol
                prodO.VolumeByWeight = 0f;
                prodO.AlcoholContent = 80f; // 80%
                prodO.ProofGallon = 80f; // 80pfg
                prodO.Storage = stoL; // we are using the same storage id as we use for Purchase to keep things simple
                prodO.SpiritTypeReportingID = 3; // brandy under 170
                prodO.MaterialKindReportingID = 94; // grape brandy
                prodO.ProductionTypeId = 2;

                List<ObjInfo4Burndwn> usedMats = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat = new ObjInfo4Burndwn();
                uMat.ID = purchaseId;
                uMat.OldVal = 0f;
                uMat.NewVal = purchO.Quantity;
                uMat.DistillableOrigin = "pur";
                uMat.BurningDownMethod = "volume";

                usedMats.Add(uMat);

                prodO.UsedMats = usedMats;

                productionId = _dl.CreateProduction(prodO, _userId);
                tupleL.Add(Tuple.Create(productionId, Table.Production));

                // create 2nd Production Distillation Record and mark it as Gauged
                ProductionObject prodO2 = new ProductionObject();
                prodO2.BatchName = "testGaugedDistillRun";
                prodO2.ProductionDate = new DateTime(2017, 09, 4);
                prodO2.ProductionStart = new DateTime(2017, 09, 4);
                prodO2.ProductionEnd = new DateTime(2017, 09, 4);
                prodO2.SpiritCutId = 9; // hearts
                prodO2.Gauged = true;
                prodO2.ProductionType = "Distillation";
                prodO2.Quantity = 15f; // 15 gallons of alcohol
                prodO2.VolumeByWeight = 0f;
                prodO2.AlcoholContent = 60f; // 60%
                prodO2.ProofGallon = 18f; // 18pfg
                prodO2.Storage = stoL; // we are using the same storage id as we use for Purchase to keep things simple
                prodO2.SpiritTypeReportingID = 3; // brandy under 170
                prodO2.MaterialKindReportingID = 94; // grape brandy
                prodO2.ProductionTypeId = 2;

                List<ObjInfo4Burndwn> usedMats4Gauge = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat4Gauged = new ObjInfo4Burndwn();
                uMat4Gauged.ID = productionId;
                uMat4Gauged.OldVal = 0f;
                uMat4Gauged.NewVal = prodO.Quantity;
                uMat4Gauged.DistillableOrigin = "prod";
                uMat4Gauged.BurningDownMethod = "volume";

                usedMats4Gauge.Add(uMat4Gauged);
                prodO2.UsedMats = usedMats4Gauge;

                int productionFinalDistillationId = _dl.CreateProduction(prodO2, _userId);

                tupleL.Add(Tuple.Create(productionFinalDistillationId, Table.Production));

                // create Production Blending Record
                ProductionObject prodBlend = new ProductionObject();
                prodBlend.BatchName = "testProdBlend";
                prodBlend.ProductionDate = new DateTime(2017, 09, 6);
                prodBlend.ProductionStart = new DateTime(2017, 09, 6);
                prodBlend.ProductionEnd = new DateTime(2017, 09, 6);
                prodBlend.Gauged = true;
                prodBlend.ProductionType = "Blending";
                prodBlend.Quantity = 22.5f; // 22.5 gallons of alcohol
                prodBlend.VolumeByWeight = 0f;
                prodBlend.AlcoholContent = 40f; // 40%
                prodBlend.ProofGallon = 18f; // 18pfg
                prodBlend.Storage = stoL; // we are using the same storage id as we use for Purchase to keep things simple
                prodBlend.SpiritTypeReportingID = 3; // brandy under 170
                prodBlend.MaterialKindReportingID = 94; // grape brandy
                prodBlend.SpiritId = spiritId;
                prodBlend.ProductionTypeId = 3;

                List<ObjInfo4Burndwn> usedMats4Blend = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat4Blend = new ObjInfo4Burndwn();
                uMat4Blend.ID = productionFinalDistillationId;
                uMat4Blend.OldVal = 0f;
                uMat4Blend.NewVal = prodO2.Quantity;
                uMat4Blend.DistillableOrigin = "prod";
                uMat4Blend.BurningDownMethod = "volume";

                usedMats4Blend.Add(uMat4Blend);
                prodBlend.UsedMats = usedMats4Blend;

                List<BlendingAdditive> blendAdditives = new List<BlendingAdditive>();
                BlendingAdditive blendAd = new BlendingAdditive();
                blendAd.RawMaterialId = waterMaterialId;
                blendAd.RawMaterialQuantity = 7.5f;
                blendAd.RawMaterialName = "Water";
                blendAd.UnitOfMeasurement = "gal";

                blendAdditives.Add(blendAd);

                prodBlend.BlendingAdditives = blendAdditives;

                int productionBlendingId = _dl.CreateProduction(prodBlend, _userId); // here productionId is overriden with a new productionId of the new Gauged record
                tupleL.Add(Tuple.Create(productionBlendingId, Table.Production));

                #endregion

                #region Reports setup

                // Report Header
                ReportHeader reportHeaderE = new ReportHeader();
                reportHeaderE.ProprietorName = "Test Distillery";
                reportHeaderE.EIN = "12-3456789";
                reportHeaderE.ReportDate = "September 2017";
                reportHeaderE.PlantAddress = "123 Cognac Drive Renton WASHINGTON 98059";
                reportHeaderE.DSP = "DSP-WA-21086";

                /* PRODUCTION REPORT */

                ProdReportPart1 part1E = new ProdReportPart1();
                part1E.ProccessingAcct = 18f;
                part1E.ProducedTotal = 18f;
                part1E.Recd4RedistilL17 = 0f;
                part1E.Recd4RedistilaltionL15 = 0f;
                part1E.StorageAcct = 0f;
                part1E.SpiritTypeReportingID = 3;
                part1E.UnfinishedSpiritsEndOfQuarterL17 = 0f;

                ProdReportParts2Through4 part2thru4 = new ProdReportParts2Through4();
                part2thru4.KindOfMaterial = "GrapeBrandy";
                part2thru4.MaterialKindReportingID = 94;
                part2thru4.NewCoop = false;
                part2thru4.UsedCoop = false;
                part2thru4.ProofGallons = 18f;
                part2thru4.SpiritTypeReportingID = 3;
                part2thru4.Tanks = false;

                ProdReportPart5 part5 = new ProdReportPart5();
                part5.KindofSpirits = "";
                part5.Proof = 0f;

                ProductionReportingObject actualProdReportObject = new ProductionReportingObject();

                actualProdReportObject = _dl.GetProductionReportData(start, end, _userId);

                ProcessingReportingObject actualProcessingReportO = new ProcessingReportingObject();

                #endregion

                // Assert

                #region Production Report Before Deletion

                Tuple<int, float> accumulatedProofGL4SpiritTypeReportingId = new Tuple<int, float>(11, purchO.ProofGallon);

                List<Tuple<int, float>> accumulatedProofGL4SpiritTypeReportingIdL = new List<Tuple<int, float>>();

                accumulatedProofGL4SpiritTypeReportingIdL.Add(accumulatedProofGL4SpiritTypeReportingId);

                // verify Production report header
                Assert.AreEqual(reportHeaderE.DSP, actualProdReportObject.Header.DSP);
                Assert.AreEqual(reportHeaderE.EIN, actualProdReportObject.Header.EIN);
                Assert.AreEqual(reportHeaderE.PlantAddress, actualProdReportObject.Header.PlantAddress);
                Assert.AreEqual(reportHeaderE.ProprietorName, actualProdReportObject.Header.ProprietorName);
                Assert.AreEqual(reportHeaderE.ReportDate, actualProdReportObject.Header.ReportDate);

                // verify Production report Part 1
                Assert.AreEqual(part1E.ProccessingAcct, actualProdReportObject.Part1List[0].ProccessingAcct);
                Assert.AreEqual(part1E.StorageAcct, actualProdReportObject.Part1List[0].StorageAcct);
                Assert.AreEqual(part1E.ProducedTotal, actualProdReportObject.Part1List[0].ProducedTotal);
                Assert.AreEqual(part1E.Recd4RedistilL17, actualProdReportObject.Part1List[0].Recd4RedistilL17);

                var expectedWine = accumulatedProofGL4SpiritTypeReportingIdL.Find(l => l.Item1 == 11);
                var actualWine = actualProdReportObject.Part1List.Find(x => x.SpiritTypeReportingID == expectedWine.Item1);

                if (actualWine == null)
                {
                    Assert.AreNotEqual(null, expectedWine, "No records in actualAlcohol190Plus for SpiritTypeID");
                }
                else
                {
                    Assert.AreEqual(expectedWine.Item2, actualWine.Recd4RedistilaltionL15);
                    Assert.AreEqual("Wine", actualWine.SpiritCatName);
                }

                Assert.AreEqual(part1E.UnfinishedSpiritsEndOfQuarterL17, actualProdReportObject.Part1List[0].UnfinishedSpiritsEndOfQuarterL17);

                // verify Production report Part 2 trough 4
                Assert.AreEqual(part2thru4.KindOfMaterial, actualProdReportObject.Part2Through4List[0].KindOfMaterial);
                Assert.AreEqual(part2thru4.MaterialKindReportingID, actualProdReportObject.Part2Through4List[0].MaterialKindReportingID);
                Assert.AreEqual(part2thru4.NewCoop, actualProdReportObject.Part2Through4List[0].NewCoop);
                Assert.AreEqual(part2thru4.UsedCoop, actualProdReportObject.Part2Through4List[0].UsedCoop);
                Assert.AreEqual(part2thru4.ProofGallons, actualProdReportObject.Part2Through4List[0].ProofGallons);
                Assert.AreEqual(part2thru4.SpiritTypeReportingID, actualProdReportObject.Part2Through4List[0].SpiritTypeReportingID);
                Assert.AreEqual(part2thru4.Tanks, actualProdReportObject.Part2Through4List[0].Tanks);

                // verify part 5
                Assert.AreEqual("Wine", actualProdReportObject.part5List[0].KindofSpirits);
                Assert.AreEqual(18, actualProdReportObject.part5List[0].Proof);

                // verify Production report Part 6 - no data should be present in this scenario
                Assert.AreEqual(0, actualProdReportObject.ProdReportPart6List.Count);

                #endregion

                #region Processing Report Before Deletion
                /* PROCESING REPORT */
                ProcessingReportingObject actualProcessingReportObject = new ProcessingReportingObject();

                ProcessReportingPart1 processingReportP1 = new ProcessReportingPart1();
                processingReportP1.AmtBottledPackaged = 0f;
                processingReportP1.BulkIngredients = "spirit";
                processingReportP1.Destroyed = 0f;
                processingReportP1.Dumped4Processing = 0f;
                processingReportP1.Gains = 0f;
                processingReportP1.Losses = 0f;
                processingReportP1.OnHandEndofMonth = 18f;
                processingReportP1.OnHandFirstofMonth = 0f;
                processingReportP1.Recd4Process = 18f;
                processingReportP1.Transf2Prod4Redistil = 0f;
                processingReportP1.Used4Redistil = 0f;
                processingReportP1.WineMixedWithSpirit = 0f;

                ProcessReportingPart2 processingReportP2 = new ProcessReportingPart2();
                processingReportP2.AmtBottledPackaged = 0f;
                processingReportP2.Destroyed = 0f;
                processingReportP2.Dumped4Processing = 0f;
                processingReportP2.FinishedProduct = "bottled";
                processingReportP2.InventoryOverage = 0f;
                processingReportP2.InventoryShortage = 0f;
                processingReportP2.OnHandEndofMonth = 0f;
                processingReportP2.OnHandFirstofMonth = 0f;
                processingReportP2.Recd4Process = 0f;
                processingReportP2.RecordedLosses = 0f;
                processingReportP2.TaxWithdrawn = 0f;
                processingReportP2.Transf2Prod4Redistil = 0f;

                ProcessReportingPart4 processingReportP4 = new ProcessReportingPart4();
                processingReportP4.AlcoholNeutral = 0f;
                processingReportP4.BlendedLightWhiskey = 0f;
                processingReportP4.BlendedOtherWhiskey = 0f;
                processingReportP4.BlendedStraightWhiskey = 0f;
                processingReportP4.BlendedWhiskeyWithLight = 0f;
                processingReportP4.BlendedWhiskeyWithNeutral = 0f;
                processingReportP4.Brandy170Under = 18f;
                processingReportP4.BrandyOver170 = 0f;
                processingReportP4.Cocktail = 0f;
                processingReportP4.DomesticWhiskey160Under = 0f;
                processingReportP4.DomesticWhiskeyOver160 = 0f;
                processingReportP4.Gin = 0f;
                processingReportP4.ImportedWhiskeyCanadian = 0f;
                processingReportP4.ImportedWhiskeyIrish = 0f;
                processingReportP4.ImportedWhiskeyScotch = 0f;
                processingReportP4.Liqueur = 0f;
                processingReportP4.ProcessingReportTypeName = "BRANDY DISTILLED AT 170 AND UNDER";
                processingReportP4.ProcessingTypeID = 12;
                processingReportP4.RumDomestic = 0f;
                processingReportP4.RumOtherImported = 0f;
                processingReportP4.RumPuertoRican = 0f;
                processingReportP4.RumVirginIslands = 0f;
                processingReportP4.StateID = 4;
                processingReportP4.Tequila = 0f;
                processingReportP4.Vodka = 0f;

                actualProcessingReportObject = _dl.GetProcessingReportData(start, end, _userId);

                Assert.AreEqual(reportHeaderE.DSP, actualProcessingReportObject.Header.DSP);
                Assert.AreEqual(reportHeaderE.EIN, actualProcessingReportObject.Header.EIN);
                Assert.AreEqual(reportHeaderE.PlantAddress, actualProcessingReportObject.Header.PlantAddress);
                Assert.AreEqual(reportHeaderE.ProprietorName, actualProcessingReportObject.Header.ProprietorName);
                Assert.AreEqual(reportHeaderE.ReportDate, actualProcessingReportObject.Header.ReportDate);

                Assert.AreEqual(processingReportP1.AmtBottledPackaged, actualProcessingReportObject.Part1.AmtBottledPackaged);
                Assert.AreEqual(processingReportP1.BulkIngredients, actualProcessingReportObject.Part1.BulkIngredients);
                Assert.AreEqual(processingReportP1.Destroyed, actualProcessingReportObject.Part1.Destroyed);
                Assert.AreEqual(processingReportP1.Dumped4Processing, actualProcessingReportObject.Part1.Dumped4Processing);
                Assert.AreEqual(processingReportP1.Gains, actualProcessingReportObject.Part1.Gains);
                Assert.AreEqual(processingReportP1.Losses, actualProcessingReportObject.Part1.Losses);
                Assert.AreEqual(processingReportP1.OnHandEndofMonth, actualProcessingReportObject.Part1.OnHandEndofMonth);
                Assert.AreEqual(processingReportP1.OnHandFirstofMonth, actualProcessingReportObject.Part1.OnHandFirstofMonth);
                Assert.AreEqual(processingReportP1.Recd4Process, actualProcessingReportObject.Part1.Recd4Process);
                Assert.AreEqual(processingReportP1.Transf2Prod4Redistil, actualProcessingReportObject.Part1.Transf2Prod4Redistil);
                Assert.AreEqual(processingReportP1.Used4Redistil, actualProcessingReportObject.Part1.Used4Redistil);
                Assert.AreEqual(processingReportP1.WineMixedWithSpirit, actualProcessingReportObject.Part1.WineMixedWithSpirit);

                Assert.AreEqual(processingReportP2.AmtBottledPackaged, actualProcessingReportObject.Part2.AmtBottledPackaged);
                Assert.AreEqual(processingReportP2.Destroyed, actualProcessingReportObject.Part2.Destroyed);
                Assert.AreEqual(processingReportP2.Dumped4Processing, actualProcessingReportObject.Part2.Dumped4Processing);
                Assert.AreEqual(processingReportP2.FinishedProduct, actualProcessingReportObject.Part2.FinishedProduct);
                Assert.AreEqual(processingReportP2.InventoryOverage, actualProcessingReportObject.Part2.InventoryOverage);
                Assert.AreEqual(processingReportP2.InventoryShortage, actualProcessingReportObject.Part2.InventoryShortage);
                Assert.AreEqual(processingReportP2.OnHandEndofMonth, actualProcessingReportObject.Part2.OnHandEndofMonth);
                Assert.AreEqual(processingReportP2.OnHandFirstofMonth, actualProcessingReportObject.Part2.OnHandFirstofMonth);
                Assert.AreEqual(processingReportP2.Recd4Process, actualProcessingReportObject.Part2.Recd4Process);
                Assert.AreEqual(processingReportP2.RecordedLosses, actualProcessingReportObject.Part2.RecordedLosses);
                Assert.AreEqual(processingReportP2.TaxWithdrawn, actualProcessingReportObject.Part2.TaxWithdrawn);
                Assert.AreEqual(processingReportP2.Transf2Prod4Redistil, actualProcessingReportObject.Part2.Transf2Prod4Redistil);

                var actualProcessingReportWithBulkSpiritsDumped = actualProcessingReportObject.Part4List.Find(x => x.ProcessingSpirits == "bulkSpiritDumped");

                Assert.AreEqual(processingReportP4.AlcoholNeutral, actualProcessingReportWithBulkSpiritsDumped.AlcoholNeutral);
                Assert.AreEqual(processingReportP4.BlendedLightWhiskey, actualProcessingReportWithBulkSpiritsDumped.BlendedLightWhiskey);
                Assert.AreEqual(processingReportP4.BlendedOtherWhiskey, actualProcessingReportWithBulkSpiritsDumped.BlendedOtherWhiskey);
                Assert.AreEqual(processingReportP4.BlendedStraightWhiskey, actualProcessingReportWithBulkSpiritsDumped.BlendedStraightWhiskey);
                Assert.AreEqual(processingReportP4.BlendedWhiskeyWithLight, actualProcessingReportWithBulkSpiritsDumped.BlendedWhiskeyWithLight);
                Assert.AreEqual(processingReportP4.BlendedWhiskeyWithNeutral, actualProcessingReportWithBulkSpiritsDumped.BlendedWhiskeyWithNeutral);
                Assert.AreEqual(processingReportP4.Brandy170Under, actualProcessingReportWithBulkSpiritsDumped.Brandy170Under);
                Assert.AreEqual(processingReportP4.BrandyOver170, actualProcessingReportWithBulkSpiritsDumped.BrandyOver170);
                Assert.AreEqual(processingReportP4.Cocktail, actualProcessingReportWithBulkSpiritsDumped.Cocktail);
                Assert.AreEqual(processingReportP4.DomesticWhiskey160Under, actualProcessingReportWithBulkSpiritsDumped.DomesticWhiskey160Under);
                Assert.AreEqual(processingReportP4.DomesticWhiskeyOver160, actualProcessingReportWithBulkSpiritsDumped.DomesticWhiskeyOver160);
                Assert.AreEqual(processingReportP4.Gin, actualProcessingReportWithBulkSpiritsDumped.Gin);
                Assert.AreEqual(processingReportP4.ImportedWhiskeyCanadian, actualProcessingReportWithBulkSpiritsDumped.ImportedWhiskeyCanadian);
                Assert.AreEqual(processingReportP4.ImportedWhiskeyIrish, actualProcessingReportWithBulkSpiritsDumped.ImportedWhiskeyIrish);
                Assert.AreEqual(processingReportP4.ImportedWhiskeyScotch, actualProcessingReportWithBulkSpiritsDumped.ImportedWhiskeyScotch);
                Assert.AreEqual(processingReportP4.Liqueur, actualProcessingReportWithBulkSpiritsDumped.Liqueur);
                Assert.AreEqual(processingReportP4.ProcessingReportTypeName, actualProcessingReportWithBulkSpiritsDumped.ProcessingReportTypeName);
                Assert.AreEqual(processingReportP4.ProcessingTypeID, actualProcessingReportWithBulkSpiritsDumped.ProcessingTypeID);
                Assert.AreEqual(processingReportP4.RumDomestic, actualProcessingReportWithBulkSpiritsDumped.RumDomestic);
                Assert.AreEqual(processingReportP4.RumDomestic, actualProcessingReportWithBulkSpiritsDumped.RumDomestic);
                Assert.AreEqual(processingReportP4.RumOtherImported, actualProcessingReportWithBulkSpiritsDumped.RumOtherImported);
                Assert.AreEqual(processingReportP4.RumPuertoRican, actualProcessingReportWithBulkSpiritsDumped.RumPuertoRican);
                Assert.AreEqual(processingReportP4.StateID, actualProcessingReportWithBulkSpiritsDumped.StateID);
                Assert.AreEqual(processingReportP4.Tequila, actualProcessingReportWithBulkSpiritsDumped.Tequila);
                Assert.AreEqual(processingReportP4.Vodka, actualProcessingReportWithBulkSpiritsDumped.Vodka);

                var actualProcessingReportPart4Bottled = actualProcessingReportObject.Part4List.Find(x => x.ProcessingSpirits == "bottled");

                Assert.IsNull(actualProcessingReportPart4Bottled);

                #endregion

                #region Storage Report Before Deletion
                StorageReport actualStorageReportObject = new StorageReport();

                StorageReportCategory storageReportBody = new StorageReportCategory();
                storageReportBody.CategoryName = "Wine";
                storageReportBody.r17_TransferredToProcessingAccount = 0f;
                storageReportBody.r18_TransferredToProductionAccount = 18f;
                storageReportBody.r19_TransferredToOtherBondedPremises = 0;
                storageReportBody.r1_OnHandFirstOfMonth = 0f;
                storageReportBody.r20_Destroyed = 0f;
                storageReportBody.r22_OtherLosses = 0f;
                storageReportBody.r23_OnHandEndOfMonth = 0f;
                storageReportBody.r24_Lines7Through23 = 18f;
                storageReportBody.r2_DepositedInBulkStorage = 18f;
                storageReportBody.r4_ReturnedToBulkStorage = 0f;
                storageReportBody.r6_TotalLines1Through5 = 18f;
                storageReportBody.r7_TaxPaid = 0f;

                actualStorageReportObject = _dl.GetStorageReportData(start, end, _userId);

                // verify Storage report Header
                Assert.AreEqual(reportHeaderE.DSP, actualStorageReportObject.Header.DSP);
                Assert.AreEqual(reportHeaderE.EIN, actualStorageReportObject.Header.EIN);
                Assert.AreEqual(reportHeaderE.PlantAddress, actualStorageReportObject.Header.PlantAddress);
                Assert.AreEqual(reportHeaderE.ProprietorName, actualStorageReportObject.Header.ProprietorName);
                Assert.AreEqual(reportHeaderE.ReportDate, actualStorageReportObject.Header.ReportDate);

                // verify Storage body
                Assert.AreEqual(storageReportBody.CategoryName, actualStorageReportObject.ReportBody[0].CategoryName);
                Assert.AreEqual(storageReportBody.r17_TransferredToProcessingAccount, actualStorageReportObject.ReportBody[0].r17_TransferredToProcessingAccount);
                Assert.AreEqual(storageReportBody.r18_TransferredToProductionAccount, actualStorageReportObject.ReportBody[0].r18_TransferredToProductionAccount);
                Assert.AreEqual(storageReportBody.r19_TransferredToOtherBondedPremises, actualStorageReportObject.ReportBody[0].r19_TransferredToOtherBondedPremises);
                Assert.AreEqual(storageReportBody.r1_OnHandFirstOfMonth, actualStorageReportObject.ReportBody[0].r1_OnHandFirstOfMonth);
                Assert.AreEqual(storageReportBody.r20_Destroyed, actualStorageReportObject.ReportBody[0].r20_Destroyed);
                Assert.AreEqual(storageReportBody.r22_OtherLosses, actualStorageReportObject.ReportBody[0].r22_OtherLosses);
                Assert.AreEqual(storageReportBody.r23_OnHandEndOfMonth, actualStorageReportObject.ReportBody[0].r23_OnHandEndOfMonth);
                Assert.AreEqual(storageReportBody.r24_Lines7Through23, actualStorageReportObject.ReportBody[0].r24_Lines7Through23);
                Assert.AreEqual(storageReportBody.r2_DepositedInBulkStorage, actualStorageReportObject.ReportBody[0].r2_DepositedInBulkStorage);
                Assert.AreEqual(storageReportBody.r4_ReturnedToBulkStorage, actualStorageReportObject.ReportBody[0].r4_ReturnedToBulkStorage);
                Assert.AreEqual(storageReportBody.r6_TotalLines1Through5, actualStorageReportObject.ReportBody[0].r6_TotalLines1Through5);
                Assert.AreEqual(storageReportBody.r7_TaxPaid, actualStorageReportObject.ReportBody[0].r7_TaxPaid);

                #endregion

                // let's verify the values in production report after we deleted bottling
                bool bottlingDeleted = _dl.DeleteProduction(prodBlend, _userId);

                #region Blending and Distiliing after Bottling deletion

                var amounts =
                    (from prod in _db.Production
                     where prod.ProductionID == productionFinalDistillationId
                     join volume in _db.Volume on prod.VolumeID equals volume.VolumeID into volume_join
                     from volume in volume_join.DefaultIfEmpty()
                     join weight in _db.Weight on prod.WeightID equals weight.WeightID into weight_join
                     from weight in weight_join.DefaultIfEmpty()
                     join alcohol in _db.Alcohol on prod.AlcoholID equals alcohol.AlcoholID into alcohol_join
                     from alcohol in alcohol_join.DefaultIfEmpty()
                     join proof in _db.Proof on prod.ProofID equals proof.ProofID into proof_join
                     from proof in proof_join.DefaultIfEmpty()
                     select new
                     {
                         volume = (float?)volume.Value ?? (float?)0,
                         weight = (float?)weight.Value ?? (float?)0,
                         alcohol = (float?)alcohol.Value ?? (float?)0,
                         proof = (float?)proof.Value ?? (float?)0
                     }).FirstOrDefault();

                if (amounts != null)
                {
                    Assert.AreEqual(prodO2.Quantity, amounts.volume);
                    Assert.AreEqual(prodO2.VolumeByWeight, amounts.weight);
                    Assert.AreEqual(prodO2.AlcoholContent, amounts.alcohol);
                    Assert.AreEqual(prodO2.ProofGallon, amounts.proof);
                }
                else
                {
                    Assert.Inconclusive("amounts query yielded no results so could not perform this part of the test");
                }

                var isBlendRecord =
                    (from prod in _db.Production
                     where prod.ProductionID == productionFinalDistillationId
                     select prod).FirstOrDefault();

                if (isBlendRecord == null)
                {
                    Assert.IsNotNull(isBlendRecord, "blending record should exist after the deletion of Bottling record");
                }

                var isDistilRecord =
                    (from prod in _db.Production
                     where prod.ProductionID == productionFinalDistillationId
                     select prod).FirstOrDefault();

                if (isDistilRecord == null)
                {
                    Assert.IsNotNull(isDistilRecord, "distilling record should exist after the deletion of Bottling record");
                }
                #endregion

                #region Production Report After Deletion
                // verify Production report Part 1
                actualProdReportObject = _dl.GetProductionReportData(start, end, _userId);

                Assert.AreEqual(0, actualProdReportObject.Part1List[0].ProccessingAcct);
                Assert.AreEqual(18, actualProdReportObject.Part1List[0].StorageAcct);
                Assert.AreEqual(18, actualProdReportObject.Part1List[0].ProducedTotal);
                Assert.AreEqual(part1E.Recd4RedistilL17, actualProdReportObject.Part1List[0].Recd4RedistilL17);

                expectedWine = accumulatedProofGL4SpiritTypeReportingIdL.Find(l => l.Item1 == 11);
                actualWine = actualProdReportObject.Part1List.Find(x => x.SpiritTypeReportingID == expectedWine.Item1);

                if (actualWine == null)
                {
                    Assert.AreNotEqual(null, expectedWine, "No records in actualAlcohol190Plus for SpiritTypeID");
                }
                else
                {
                    Assert.AreEqual(expectedWine.Item2, actualWine.Recd4RedistilaltionL15);
                    Assert.AreEqual("Wine", actualWine.SpiritCatName);
                }

                Assert.AreEqual(part1E.UnfinishedSpiritsEndOfQuarterL17, actualProdReportObject.Part1List[0].UnfinishedSpiritsEndOfQuarterL17);

                // verify Production report Part 2 trough 4
                Assert.AreEqual(part2thru4.KindOfMaterial, actualProdReportObject.Part2Through4List[0].KindOfMaterial);
                Assert.AreEqual(part2thru4.MaterialKindReportingID, actualProdReportObject.Part2Through4List[0].MaterialKindReportingID);
                Assert.AreEqual(part2thru4.NewCoop, actualProdReportObject.Part2Through4List[0].NewCoop);
                Assert.AreEqual(part2thru4.UsedCoop, actualProdReportObject.Part2Through4List[0].UsedCoop);
                Assert.AreEqual(part2thru4.ProofGallons, actualProdReportObject.Part2Through4List[0].ProofGallons);
                Assert.AreEqual(part2thru4.SpiritTypeReportingID, actualProdReportObject.Part2Through4List[0].SpiritTypeReportingID);
                Assert.AreEqual(part2thru4.Tanks, actualProdReportObject.Part2Through4List[0].Tanks);

                // verify part 5
                Assert.AreEqual("Wine", actualProdReportObject.part5List[0].KindofSpirits);
                Assert.AreEqual(18, actualProdReportObject.part5List[0].Proof);

                // verify Production report Part 6 - no data should be present in this scenario
                Assert.AreEqual(0, actualProdReportObject.ProdReportPart6List.Count);
                #endregion

                #region Processing Report After Deletion
                /* PROCESING REPORT */
                actualProcessingReportObject = new ProcessingReportingObject();

                processingReportP1 = new ProcessReportingPart1();
                processingReportP1.AmtBottledPackaged = 0f;
                processingReportP1.BulkIngredients = "spirit";
                processingReportP1.Destroyed = 0f;
                processingReportP1.Dumped4Processing = 0f;
                processingReportP1.Gains = 0f;
                processingReportP1.Losses = 0f;
                processingReportP1.OnHandEndofMonth = 0f;
                processingReportP1.OnHandFirstofMonth = 0f;
                processingReportP1.Recd4Process = 0f;
                processingReportP1.Transf2Prod4Redistil = 0f;
                processingReportP1.Used4Redistil = 0f;
                processingReportP1.WineMixedWithSpirit = 0f;

                processingReportP2 = new ProcessReportingPart2();
                processingReportP2.AmtBottledPackaged = 0f;
                processingReportP2.Destroyed = 0f;
                processingReportP2.Dumped4Processing = 0f;
                processingReportP2.InventoryOverage = 0f;
                processingReportP2.InventoryShortage = 0f;
                processingReportP2.OnHandEndofMonth = 0f;
                processingReportP2.OnHandFirstofMonth = 0f;
                processingReportP2.Recd4Process = 0f;
                processingReportP2.RecordedLosses = 0f;
                processingReportP2.TaxWithdrawn = 0f;
                processingReportP2.Transf2Prod4Redistil = 0f;

                processingReportP4 = new ProcessReportingPart4();
                processingReportP4.ProcessingSpirits = "bulkSpiritDumped";
                processingReportP4.AlcoholNeutral = 0f;
                processingReportP4.BlendedLightWhiskey = 0f;
                processingReportP4.BlendedOtherWhiskey = 0f;
                processingReportP4.BlendedStraightWhiskey = 0f;
                processingReportP4.BlendedWhiskeyWithLight = 0f;
                processingReportP4.BlendedWhiskeyWithNeutral = 0f;
                processingReportP4.Brandy170Under = 0f;
                processingReportP4.BrandyOver170 = 0f;
                processingReportP4.Cocktail = 0f;
                processingReportP4.DomesticWhiskey160Under = 0f;
                processingReportP4.DomesticWhiskeyOver160 = 0f;
                processingReportP4.Gin = 0f;
                processingReportP4.ImportedWhiskeyCanadian = 0f;
                processingReportP4.ImportedWhiskeyIrish = 0f;
                processingReportP4.ImportedWhiskeyScotch = 0f;
                processingReportP4.Liqueur = 0f;
                processingReportP4.ProcessingReportTypeName = "BRANDY DISTILLED AT 170 AND UNDER";
                processingReportP4.ProcessingTypeID = 12;
                processingReportP4.RumDomestic = 0f;
                processingReportP4.RumOtherImported = 0f;
                processingReportP4.RumPuertoRican = 0f;
                processingReportP4.RumVirginIslands = 0f;
                processingReportP4.StateID = 0;
                processingReportP4.Tequila = 0f;
                processingReportP4.Vodka = 0f;

                actualProcessingReportObject = _dl.GetProcessingReportData(start, end, _userId);

                Assert.AreEqual(processingReportP1.AmtBottledPackaged, actualProcessingReportObject.Part1.AmtBottledPackaged);
                Assert.AreEqual(processingReportP1.BulkIngredients, actualProcessingReportObject.Part1.BulkIngredients);
                Assert.AreEqual(processingReportP1.Destroyed, actualProcessingReportObject.Part1.Destroyed);
                Assert.AreEqual(processingReportP1.Dumped4Processing, actualProcessingReportObject.Part1.Dumped4Processing);
                Assert.AreEqual(processingReportP1.Gains, actualProcessingReportObject.Part1.Gains);
                Assert.AreEqual(processingReportP1.Losses, actualProcessingReportObject.Part1.Losses);
                Assert.AreEqual(processingReportP1.OnHandEndofMonth, actualProcessingReportObject.Part1.OnHandEndofMonth);
                Assert.AreEqual(processingReportP1.OnHandFirstofMonth, actualProcessingReportObject.Part1.OnHandFirstofMonth);
                Assert.AreEqual(processingReportP1.Recd4Process, actualProcessingReportObject.Part1.Recd4Process);
                Assert.AreEqual(processingReportP1.Transf2Prod4Redistil, actualProcessingReportObject.Part1.Transf2Prod4Redistil);
                Assert.AreEqual(processingReportP1.Used4Redistil, actualProcessingReportObject.Part1.Used4Redistil);
                Assert.AreEqual(processingReportP1.WineMixedWithSpirit, actualProcessingReportObject.Part1.WineMixedWithSpirit);

                Assert.AreEqual(processingReportP2.AmtBottledPackaged, actualProcessingReportObject.Part2.AmtBottledPackaged);
                Assert.AreEqual(processingReportP2.Destroyed, actualProcessingReportObject.Part2.Destroyed);
                Assert.AreEqual(processingReportP2.Dumped4Processing, actualProcessingReportObject.Part2.Dumped4Processing);
                Assert.AreEqual(processingReportP2.InventoryOverage, actualProcessingReportObject.Part2.InventoryOverage);
                Assert.AreEqual(processingReportP2.InventoryShortage, actualProcessingReportObject.Part2.InventoryShortage);
                Assert.AreEqual(processingReportP2.OnHandEndofMonth, actualProcessingReportObject.Part2.OnHandEndofMonth);
                Assert.AreEqual(processingReportP2.OnHandFirstofMonth, actualProcessingReportObject.Part2.OnHandFirstofMonth);
                Assert.AreEqual(processingReportP2.Recd4Process, actualProcessingReportObject.Part2.Recd4Process);
                Assert.AreEqual(processingReportP2.RecordedLosses, actualProcessingReportObject.Part2.RecordedLosses);
                Assert.AreEqual(processingReportP2.TaxWithdrawn, actualProcessingReportObject.Part2.TaxWithdrawn);
                Assert.AreEqual(processingReportP2.Transf2Prod4Redistil, actualProcessingReportObject.Part2.Transf2Prod4Redistil);

                var actualproccessingReportPart4BottledObject = actualProcessingReportObject.Part4List.Find(x => x.ProcessingSpirits == "bottled");

                if (actualproccessingReportPart4BottledObject != null)
                {
                    Assert.IsNull(actualproccessingReportPart4BottledObject, "there should be no bottling record");
                }

                var actualproccessingReportPart4Object = actualProcessingReportObject.Part4List.Find(x => x.ProcessingSpirits == "bulkSpiritDumped");

                if (actualproccessingReportPart4Object == null)
                {
                    Assert.IsNull(actualproccessingReportPart4Object, "there should be no blending record");
                }
                else
                {
                    Assert.AreEqual(processingReportP4.AlcoholNeutral, actualproccessingReportPart4Object.AlcoholNeutral);
                    Assert.AreEqual(processingReportP4.BlendedLightWhiskey, actualproccessingReportPart4Object.BlendedLightWhiskey);
                    Assert.AreEqual(processingReportP4.BlendedOtherWhiskey, actualproccessingReportPart4Object.BlendedOtherWhiskey);
                    Assert.AreEqual(processingReportP4.BlendedStraightWhiskey, actualproccessingReportPart4Object.BlendedStraightWhiskey);
                    Assert.AreEqual(processingReportP4.BlendedWhiskeyWithLight, actualproccessingReportPart4Object.BlendedWhiskeyWithLight);
                    Assert.AreEqual(processingReportP4.BlendedWhiskeyWithNeutral, actualproccessingReportPart4Object.BlendedWhiskeyWithNeutral);
                    Assert.AreEqual(processingReportP4.Brandy170Under, actualproccessingReportPart4Object.Brandy170Under);
                    Assert.AreEqual(processingReportP4.BrandyOver170, actualproccessingReportPart4Object.BrandyOver170);
                    Assert.AreEqual(processingReportP4.Cocktail, actualproccessingReportPart4Object.Cocktail);
                    Assert.AreEqual(processingReportP4.DomesticWhiskey160Under, actualproccessingReportPart4Object.DomesticWhiskey160Under);
                    Assert.AreEqual(processingReportP4.DomesticWhiskeyOver160, actualproccessingReportPart4Object.DomesticWhiskeyOver160);
                    Assert.AreEqual(processingReportP4.Gin, actualproccessingReportPart4Object.Gin);
                    Assert.AreEqual(processingReportP4.ImportedWhiskeyCanadian, actualproccessingReportPart4Object.ImportedWhiskeyCanadian);
                    Assert.AreEqual(processingReportP4.ImportedWhiskeyIrish, actualproccessingReportPart4Object.ImportedWhiskeyIrish);
                    Assert.AreEqual(processingReportP4.ImportedWhiskeyScotch, actualproccessingReportPart4Object.ImportedWhiskeyScotch);
                    Assert.AreEqual(processingReportP4.Liqueur, actualproccessingReportPart4Object.Liqueur);
                    Assert.AreEqual(processingReportP4.ProcessingReportTypeName, actualproccessingReportPart4Object.ProcessingReportTypeName);
                    Assert.AreEqual(processingReportP4.ProcessingSpirits, actualproccessingReportPart4Object.ProcessingSpirits);
                    Assert.AreEqual(processingReportP4.ProcessingTypeID, actualproccessingReportPart4Object.ProcessingTypeID);
                    Assert.AreEqual(processingReportP4.RumDomestic, actualproccessingReportPart4Object.RumDomestic);
                    Assert.AreEqual(processingReportP4.RumDomestic, actualproccessingReportPart4Object.RumDomestic);
                    Assert.AreEqual(processingReportP4.RumOtherImported, actualproccessingReportPart4Object.RumOtherImported);
                    Assert.AreEqual(processingReportP4.RumPuertoRican, actualproccessingReportPart4Object.RumPuertoRican);
                    Assert.AreEqual(processingReportP4.StateID, actualproccessingReportPart4Object.StateID);
                    Assert.AreEqual(processingReportP4.Tequila, actualproccessingReportPart4Object.Tequila);
                    Assert.AreEqual(processingReportP4.Vodka, actualproccessingReportPart4Object.Vodka);
                }

                #endregion

                #region Storage Report After Deletion
                actualStorageReportObject = new StorageReport();

                storageReportBody = new StorageReportCategory();
                storageReportBody.CategoryName = "Wine";
                storageReportBody.r17_TransferredToProcessingAccount = 0f;
                storageReportBody.r18_TransferredToProductionAccount = 18f;
                storageReportBody.r19_TransferredToOtherBondedPremises = 0;
                storageReportBody.r1_OnHandFirstOfMonth = 0f;
                storageReportBody.r20_Destroyed = 0f;
                storageReportBody.r22_OtherLosses = 0f;
                storageReportBody.r23_OnHandEndOfMonth = 0f;
                storageReportBody.r24_Lines7Through23 = 18f;
                storageReportBody.r2_DepositedInBulkStorage = 18f;
                storageReportBody.r4_ReturnedToBulkStorage = 0f;
                storageReportBody.r6_TotalLines1Through5 = 18f;
                storageReportBody.r7_TaxPaid = 0f;

                actualStorageReportObject = _dl.GetStorageReportData(start, end, _userId);

                // verify Storage report Header
                Assert.AreEqual(reportHeaderE.DSP, actualStorageReportObject.Header.DSP);
                Assert.AreEqual(reportHeaderE.EIN, actualStorageReportObject.Header.EIN);
                Assert.AreEqual(reportHeaderE.PlantAddress, actualStorageReportObject.Header.PlantAddress);
                Assert.AreEqual(reportHeaderE.ProprietorName, actualStorageReportObject.Header.ProprietorName);
                Assert.AreEqual(reportHeaderE.ReportDate, actualStorageReportObject.Header.ReportDate);

                // verify Storage body for Wine
                // verify Storage body for CategoryName BrandyUnder170
                var storageWine = actualStorageReportObject.ReportBody.Find(x => x.CategoryName == "Wine");

                if (storageWine == null)
                {
                    Assert.IsNotNull(storageWine);
                }
                else
                {
                    Assert.AreEqual(storageReportBody.CategoryName, storageWine.CategoryName);
                    Assert.AreEqual(storageReportBody.r17_TransferredToProcessingAccount, storageWine.r17_TransferredToProcessingAccount);
                    Assert.AreEqual(storageReportBody.r18_TransferredToProductionAccount, storageWine.r18_TransferredToProductionAccount);
                    Assert.AreEqual(storageReportBody.r19_TransferredToOtherBondedPremises, storageWine.r19_TransferredToOtherBondedPremises);
                    Assert.AreEqual(storageReportBody.r1_OnHandFirstOfMonth, storageWine.r1_OnHandFirstOfMonth);
                    Assert.AreEqual(storageReportBody.r20_Destroyed, storageWine.r20_Destroyed);
                    Assert.AreEqual(storageReportBody.r22_OtherLosses, storageWine.r22_OtherLosses);
                    Assert.AreEqual(storageReportBody.r23_OnHandEndOfMonth, storageWine.r23_OnHandEndOfMonth);
                    Assert.AreEqual(storageReportBody.r24_Lines7Through23, storageWine.r24_Lines7Through23);
                    Assert.AreEqual(storageReportBody.r2_DepositedInBulkStorage, storageWine.r2_DepositedInBulkStorage);
                    Assert.AreEqual(storageReportBody.r4_ReturnedToBulkStorage, storageWine.r4_ReturnedToBulkStorage);
                    Assert.AreEqual(storageReportBody.r6_TotalLines1Through5, storageWine.r6_TotalLines1Through5);
                    Assert.AreEqual(storageReportBody.r7_TaxPaid, storageWine.r7_TaxPaid);
                }

                // verify Storage body for CategoryName BrandyUnder170
                var storageBrandy = actualStorageReportObject.ReportBody.Find(x => x.CategoryName == "BrandyUnder170");

                if (storageBrandy == null)
                {
                    Assert.IsNotNull(storageBrandy);
                }
                else
                {
                    storageReportBody = new StorageReportCategory();
                    storageReportBody.CategoryName = "BrandyUnder170";
                    storageReportBody.r17_TransferredToProcessingAccount = 0f;
                    storageReportBody.r18_TransferredToProductionAccount = 0f;
                    storageReportBody.r19_TransferredToOtherBondedPremises = 0;
                    storageReportBody.r1_OnHandFirstOfMonth = 0f;
                    storageReportBody.r20_Destroyed = 0f;
                    storageReportBody.r22_OtherLosses = 0f;
                    storageReportBody.r23_OnHandEndOfMonth = 18f;
                    storageReportBody.r24_Lines7Through23 = 18f;
                    storageReportBody.r2_DepositedInBulkStorage = 18f;
                    storageReportBody.r4_ReturnedToBulkStorage = 0f;
                    storageReportBody.r6_TotalLines1Through5 = 18f;
                    storageReportBody.r7_TaxPaid = 0f;
                    storageReportBody.r6_TotalLines1Through5 = 18;

                    Assert.AreEqual(storageReportBody.CategoryName, storageBrandy.CategoryName);
                    Assert.AreEqual(storageReportBody.r17_TransferredToProcessingAccount, storageBrandy.r17_TransferredToProcessingAccount);
                    Assert.AreEqual(storageReportBody.r18_TransferredToProductionAccount, storageBrandy.r18_TransferredToProductionAccount);
                    Assert.AreEqual(storageReportBody.r19_TransferredToOtherBondedPremises, storageBrandy.r19_TransferredToOtherBondedPremises);
                    Assert.AreEqual(storageReportBody.r1_OnHandFirstOfMonth, storageBrandy.r1_OnHandFirstOfMonth);
                    Assert.AreEqual(storageReportBody.r20_Destroyed, storageBrandy.r20_Destroyed);
                    Assert.AreEqual(storageReportBody.r22_OtherLosses, storageBrandy.r22_OtherLosses);
                    Assert.AreEqual(storageReportBody.r23_OnHandEndOfMonth, storageBrandy.r23_OnHandEndOfMonth);
                    Assert.AreEqual(storageReportBody.r24_Lines7Through23, storageBrandy.r24_Lines7Through23);
                    Assert.AreEqual(storageReportBody.r2_DepositedInBulkStorage, storageBrandy.r2_DepositedInBulkStorage);
                    Assert.AreEqual(storageReportBody.r4_ReturnedToBulkStorage, storageBrandy.r4_ReturnedToBulkStorage);
                    Assert.AreEqual(storageReportBody.r6_TotalLines1Through5, storageBrandy.r6_TotalLines1Through5);
                    Assert.AreEqual(storageReportBody.r7_TaxPaid, storageBrandy.r7_TaxPaid);
                    Assert.AreEqual(storageReportBody.r6_TotalLines1Through5, storageBrandy.r6_TotalLines1Through5);
                }

                #endregion
            }
            finally
            {
                // Cleanup
                foreach (var i in tupleL)
                {
                    TestRecordCleanup(i.Item1, i.Item2);
                }
            }
        }

        /// <summary>
        /// This test method verifies that when Distilled record
        /// is deleted, all other records amounts' that were involved in making this
        /// distilled record originally, are reinstated. Effectively, we should be re-instating values for production and purchase records
        /// In addition, we are verifying reports update it's values as well.
        /// </summary>
        [TestMethod()]
        public void DeleteWorkflow_BuyWine_Distil_DistilAgain_DeleteDistilling_EnsureOtherRecords_Intact()
        {
            // Arrange
            List<Tuple<int/*recordId*/, Table/*table enum vaue*/>> tupleL = new List<Tuple<int, Table>>();
            int spiritId = 0;
            int vendorId = 0;
            int storageId = 0;
            int wineMaterialId = 0;
            int waterMaterialId = 0;
            int purchaseId = 0;
            int productionId = 0;

            DateTime start = new DateTime(2017, 09, 01);
            DateTime end = new DateTime(2017, 09, 30);

            try
            {
                #region Dictionary
                //  dictionary setup
                SpiritObject spirit = new SpiritObject();
                spirit.SpiritName = "Brandy Under 170";
                spirit.ProcessingReportTypeID = 12;

                spiritId = _dl.CreateSpirit(_userId, spirit);

                // setup Vendor object
                VendorObject vendor = new VendorObject();
                vendor.VendorName = "testVendor";

                vendorId = _dl.CreateVendor(_userId, vendor);
                tupleL.Add(Tuple.Create(vendorId, Table.Vendor));

                // setup Storage Object
                StorageObject storage = new StorageObject();
                storage.StorageName = "testStorage";
                storage.SerialNumber = "2H29NNS";

                storageId = _dl.CreateStorage(_userId, storage);
                tupleL.Add(Tuple.Create(storageId, Table.Storage));


                // setup Material Object
                // wine
                {
                    RawMaterialObject wineMaterial = new RawMaterialObject();
                    wineMaterial.RawMaterialName = "Wine For Brandy";
                    wineMaterial.MaterialCategoryID = 2;
                    wineMaterial.UnitType = "gal";
                    wineMaterial.UnitTypeId = 1;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Fermented = true;
                    wineMaterial.PurchaseMaterialTypes = materialBoolTypes;

                    wineMaterialId = _dl.CreateRawMaterial(_userId, wineMaterial);
                    tupleL.Add(Tuple.Create(wineMaterialId, Table.MaterialDict));
                }

                // water
                {
                    RawMaterialObject waterMaterial = new RawMaterialObject();
                    waterMaterial.RawMaterialName = "Water";
                    waterMaterial.UnitType = "gal";
                    waterMaterial.UnitTypeId = 1;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Additive = true;
                    waterMaterial.PurchaseMaterialTypes = materialBoolTypes;

                    waterMaterialId = _dl.CreateRawMaterial(_userId, waterMaterial);
                    tupleL.Add(Tuple.Create(waterMaterialId, Table.MaterialDict));
                }
                #endregion

                #region Purchase
                // create Purchase Record (minimal required fields)
                PurchaseObject purchO = new PurchaseObject();
                purchO.PurBatchName = "Feremented Purchase";
                purchO.PurchaseType = "Fermented";
                purchO.PurchaseDate = new DateTime(2017, 09, 1);
                purchO.Quantity = 100f; // 100 gallons
                purchO.VolumeByWeight = 0f;
                purchO.AlcoholContent = 9f;
                purchO.ProofGallon = 18f;
                purchO.RecordId = wineMaterialId;
                purchO.Price = 350f;
                purchO.VendorId = vendorId;

                List<StorageObject> stoL = new List<StorageObject>();
                StorageObject sto = new StorageObject();
                sto.StorageId = storageId;
                stoL.Add(sto);
                purchO.Storage = stoL;

                purchO.SpiritTypeReportingID = 11;
                purchO.Gauged = true;

                purchaseId = _dl.CreatePurchase(purchO, _userId);
                tupleL.Add(Tuple.Create(purchaseId, Table.Purchase));
                #endregion

                #region Production
                // create 1st Production Distillation Record and don't mark it as Gauged
                ProductionObject prodO = new ProductionObject();
                prodO.BatchName = "test1stDistillRun";
                prodO.ProductionDate = new DateTime(2017, 09, 3);
                prodO.ProductionStart = new DateTime(2017, 09, 3);
                prodO.ProductionEnd = new DateTime(2017, 09, 3);
                prodO.SpiritCutId = 11; // mixed
                prodO.Gauged = false;
                prodO.ProductionType = "Distillation";
                prodO.Quantity = 50f; //50 gallons of alcohol
                prodO.VolumeByWeight = 0f;
                prodO.AlcoholContent = 80f; // 80%
                prodO.ProofGallon = 80f; // 80pfg
                prodO.Storage = stoL; // we are using the same storage id as we use for Purchase to keep things simple
                prodO.SpiritTypeReportingID = 3; // brandy under 170
                prodO.MaterialKindReportingID = 94; // grape brandy
                prodO.ProductionTypeId = 2;

                List<ObjInfo4Burndwn> usedMats = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat = new ObjInfo4Burndwn();
                uMat.ID = purchaseId;
                uMat.OldVal = 0f;
                uMat.NewVal = purchO.Quantity;
                uMat.DistillableOrigin = "pur";
                uMat.BurningDownMethod = "volume";

                usedMats.Add(uMat);

                prodO.UsedMats = usedMats;

                productionId = _dl.CreateProduction(prodO, _userId);
                tupleL.Add(Tuple.Create(productionId, Table.Production));

                // create 2nd Production Distillation Record and mark it as Gauged
                ProductionObject prodO2 = new ProductionObject();
                prodO2.BatchName = "testGaugedDistillRun";
                prodO2.ProductionDate = new DateTime(2017, 09, 4);
                prodO2.ProductionStart = new DateTime(2017, 09, 4);
                prodO2.ProductionEnd = new DateTime(2017, 09, 4);
                prodO2.SpiritCutId = 9; // hearts
                prodO2.Gauged = true;
                prodO2.ProductionType = "Distillation";
                prodO2.Quantity = 15f; // 15 gallons of alcohol
                prodO2.VolumeByWeight = 0f;
                prodO2.AlcoholContent = 60f; // 60%
                prodO2.ProofGallon = 18f; // 18pfg
                prodO2.Storage = stoL; // we are using the same storage id as we use for Purchase to keep things simple
                prodO2.SpiritTypeReportingID = 3; // brandy under 170
                prodO2.MaterialKindReportingID = 94; // grape brandy
                prodO2.ProductionTypeId = 2;

                List<ObjInfo4Burndwn> usedMats4Gauge = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat4Gauged = new ObjInfo4Burndwn();
                uMat4Gauged.ID = productionId;
                uMat4Gauged.OldVal = 0f;
                uMat4Gauged.NewVal = prodO.Quantity;
                uMat4Gauged.DistillableOrigin = "prod";
                uMat4Gauged.BurningDownMethod = "volume";

                usedMats4Gauge.Add(uMat4Gauged);
                prodO2.UsedMats = usedMats4Gauge;

                int productionFinalDistillationId = _dl.CreateProduction(prodO2, _userId);

                tupleL.Add(Tuple.Create(productionFinalDistillationId, Table.Production));

                #endregion

                #region Reports setup

                // Report Header
                ReportHeader reportHeaderE = new ReportHeader();
                reportHeaderE.ProprietorName = "Test Distillery";
                reportHeaderE.EIN = "12-3456789";
                reportHeaderE.ReportDate = "September 2017";
                reportHeaderE.PlantAddress = "123 Cognac Drive Renton WASHINGTON 98059";
                reportHeaderE.DSP = "DSP-WA-21086";

                /* PRODUCTION REPORT */

                ProdReportPart1 part1E = new ProdReportPart1();
                part1E.ProccessingAcct = 0f;
                part1E.ProducedTotal = 18f;
                part1E.Recd4RedistilL17 = 0f;
                part1E.Recd4RedistilaltionL15 = 0f;
                part1E.StorageAcct = 18f;
                part1E.SpiritTypeReportingID = 3;
                part1E.UnfinishedSpiritsEndOfQuarterL17 = 0f;

                ProdReportParts2Through4 part2thru4 = new ProdReportParts2Through4();
                part2thru4.KindOfMaterial = "GrapeBrandy";
                part2thru4.MaterialKindReportingID = 94;
                part2thru4.NewCoop = false;
                part2thru4.UsedCoop = false;
                part2thru4.ProofGallons = 18f;
                part2thru4.SpiritTypeReportingID = 3;
                part2thru4.Tanks = false;

                ProdReportPart5 part5 = new ProdReportPart5();
                part5.KindofSpirits = "";
                part5.Proof = 0f;

                ProductionReportingObject actualProdReportObject = new ProductionReportingObject();

                actualProdReportObject = _dl.GetProductionReportData(start, end, _userId);

                ProcessingReportingObject actualProcessingReportO = new ProcessingReportingObject();

                #endregion

                // Assert

                #region Production Report Before Deletion

                Tuple<int, float> accumulatedProofGL4SpiritTypeReportingId = new Tuple<int, float>(11, purchO.ProofGallon);

                List<Tuple<int, float>> accumulatedProofGL4SpiritTypeReportingIdL = new List<Tuple<int, float>>();

                accumulatedProofGL4SpiritTypeReportingIdL.Add(accumulatedProofGL4SpiritTypeReportingId);

                // verify Production report header
                Assert.AreEqual(reportHeaderE.DSP, actualProdReportObject.Header.DSP);
                Assert.AreEqual(reportHeaderE.EIN, actualProdReportObject.Header.EIN);
                Assert.AreEqual(reportHeaderE.PlantAddress, actualProdReportObject.Header.PlantAddress);
                Assert.AreEqual(reportHeaderE.ProprietorName, actualProdReportObject.Header.ProprietorName);
                Assert.AreEqual(reportHeaderE.ReportDate, actualProdReportObject.Header.ReportDate);

                // verify Production report Part 1
                Assert.AreEqual(part1E.ProccessingAcct, actualProdReportObject.Part1List[0].ProccessingAcct);
                Assert.AreEqual(part1E.StorageAcct, actualProdReportObject.Part1List[0].StorageAcct);
                Assert.AreEqual(part1E.ProducedTotal, actualProdReportObject.Part1List[0].ProducedTotal);
                Assert.AreEqual(part1E.Recd4RedistilL17, actualProdReportObject.Part1List[0].Recd4RedistilL17);

                var expectedWine = accumulatedProofGL4SpiritTypeReportingIdL.Find(l => l.Item1 == 11);
                var actualWine = actualProdReportObject.Part1List.Find(x => x.SpiritTypeReportingID == expectedWine.Item1);

                if (actualWine == null)
                {
                    Assert.AreNotEqual(null, expectedWine, "No records in actualAlcohol190Plus for SpiritTypeID");
                }
                else
                {
                    Assert.AreEqual(expectedWine.Item2, actualWine.Recd4RedistilaltionL15);
                    Assert.AreEqual("Wine", actualWine.SpiritCatName);
                }

                Assert.AreEqual(part1E.UnfinishedSpiritsEndOfQuarterL17, actualProdReportObject.Part1List[0].UnfinishedSpiritsEndOfQuarterL17);

                // verify Production report Part 2 trough 4
                Assert.AreEqual(part2thru4.KindOfMaterial, actualProdReportObject.Part2Through4List[0].KindOfMaterial);
                Assert.AreEqual(part2thru4.MaterialKindReportingID, actualProdReportObject.Part2Through4List[0].MaterialKindReportingID);
                Assert.AreEqual(part2thru4.NewCoop, actualProdReportObject.Part2Through4List[0].NewCoop);
                Assert.AreEqual(part2thru4.UsedCoop, actualProdReportObject.Part2Through4List[0].UsedCoop);
                Assert.AreEqual(part2thru4.ProofGallons, actualProdReportObject.Part2Through4List[0].ProofGallons);
                Assert.AreEqual(part2thru4.SpiritTypeReportingID, actualProdReportObject.Part2Through4List[0].SpiritTypeReportingID);
                Assert.AreEqual(part2thru4.Tanks, actualProdReportObject.Part2Through4List[0].Tanks);

                // verify part 5
                Assert.AreEqual("Wine", actualProdReportObject.part5List[0].KindofSpirits);
                Assert.AreEqual(18, actualProdReportObject.part5List[0].Proof);

                // verify Production report Part 6 - no data should be present in this scenario
                Assert.AreEqual(0, actualProdReportObject.ProdReportPart6List.Count);

                #endregion


                #region Storage Report Before Deletion
                StorageReport actualStorageReportObject = new StorageReport();

                StorageReportCategory storageReportBody = new StorageReportCategory();
                storageReportBody.CategoryName = "Wine";
                storageReportBody.r17_TransferredToProcessingAccount = 0f;
                storageReportBody.r18_TransferredToProductionAccount = 18f;
                storageReportBody.r19_TransferredToOtherBondedPremises = 0;
                storageReportBody.r1_OnHandFirstOfMonth = 0f;
                storageReportBody.r20_Destroyed = 0f;
                storageReportBody.r22_OtherLosses = 0f;
                storageReportBody.r23_OnHandEndOfMonth = 0f;
                storageReportBody.r24_Lines7Through23 = 18f;
                storageReportBody.r2_DepositedInBulkStorage = 18f;
                storageReportBody.r4_ReturnedToBulkStorage = 0f;
                storageReportBody.r6_TotalLines1Through5 = 18f;
                storageReportBody.r7_TaxPaid = 0f;

                actualStorageReportObject = _dl.GetStorageReportData(start, end, _userId);

                // verify Storage report Header
                Assert.AreEqual(reportHeaderE.DSP, actualStorageReportObject.Header.DSP);
                Assert.AreEqual(reportHeaderE.EIN, actualStorageReportObject.Header.EIN);
                Assert.AreEqual(reportHeaderE.PlantAddress, actualStorageReportObject.Header.PlantAddress);
                Assert.AreEqual(reportHeaderE.ProprietorName, actualStorageReportObject.Header.ProprietorName);
                Assert.AreEqual(reportHeaderE.ReportDate, actualStorageReportObject.Header.ReportDate);

                // verify Storage // verify Storage body for CategoryName BrandyUnder170
                var storageWine = actualStorageReportObject.ReportBody.Find(x => x.CategoryName == "Wine");

                if (storageWine == null)
                {
                    Assert.IsNotNull(storageWine);
                }
                else
                {
                    Assert.AreEqual(storageReportBody.CategoryName, storageWine.CategoryName);
                    Assert.AreEqual(storageReportBody.r17_TransferredToProcessingAccount, storageWine.r17_TransferredToProcessingAccount);
                    Assert.AreEqual(storageReportBody.r18_TransferredToProductionAccount, storageWine.r18_TransferredToProductionAccount);
                    Assert.AreEqual(storageReportBody.r19_TransferredToOtherBondedPremises, storageWine.r19_TransferredToOtherBondedPremises);
                    Assert.AreEqual(storageReportBody.r1_OnHandFirstOfMonth, storageWine.r1_OnHandFirstOfMonth);
                    Assert.AreEqual(storageReportBody.r20_Destroyed, storageWine.r20_Destroyed);
                    Assert.AreEqual(storageReportBody.r22_OtherLosses, storageWine.r22_OtherLosses);
                    Assert.AreEqual(storageReportBody.r23_OnHandEndOfMonth, storageWine.r23_OnHandEndOfMonth);
                    Assert.AreEqual(storageReportBody.r24_Lines7Through23, storageWine.r24_Lines7Through23);
                    Assert.AreEqual(storageReportBody.r2_DepositedInBulkStorage, storageWine.r2_DepositedInBulkStorage);
                    Assert.AreEqual(storageReportBody.r4_ReturnedToBulkStorage, storageWine.r4_ReturnedToBulkStorage);
                    Assert.AreEqual(storageReportBody.r6_TotalLines1Through5, storageWine.r6_TotalLines1Through5);
                    Assert.AreEqual(storageReportBody.r7_TaxPaid, storageWine.r7_TaxPaid);
                }

                // verify Storage body for CategoryName BrandyUnder170
                var storageBrandy = actualStorageReportObject.ReportBody.Find(x => x.CategoryName == "BrandyUnder170");

                if (storageBrandy == null)
                {
                    Assert.IsNotNull(storageBrandy);
                }
                else
                {
                    storageReportBody = new StorageReportCategory();
                    storageReportBody.CategoryName = "BrandyUnder170";
                    storageReportBody.r17_TransferredToProcessingAccount = 0f;
                    storageReportBody.r18_TransferredToProductionAccount = 0f;
                    storageReportBody.r19_TransferredToOtherBondedPremises = 0;
                    storageReportBody.r1_OnHandFirstOfMonth = 0f;
                    storageReportBody.r20_Destroyed = 0f;
                    storageReportBody.r22_OtherLosses = 0f;
                    storageReportBody.r23_OnHandEndOfMonth = 18f;
                    storageReportBody.r24_Lines7Through23 = 18f;
                    storageReportBody.r2_DepositedInBulkStorage = 18f;
                    storageReportBody.r4_ReturnedToBulkStorage = 0f;
                    storageReportBody.r6_TotalLines1Through5 = 18f;
                    storageReportBody.r7_TaxPaid = 0f;
                    storageReportBody.r6_TotalLines1Through5 = 18;

                    Assert.AreEqual(storageReportBody.CategoryName, storageBrandy.CategoryName);
                    Assert.AreEqual(storageReportBody.r17_TransferredToProcessingAccount, storageBrandy.r17_TransferredToProcessingAccount);
                    Assert.AreEqual(storageReportBody.r18_TransferredToProductionAccount, storageBrandy.r18_TransferredToProductionAccount);
                    Assert.AreEqual(storageReportBody.r19_TransferredToOtherBondedPremises, storageBrandy.r19_TransferredToOtherBondedPremises);
                    Assert.AreEqual(storageReportBody.r1_OnHandFirstOfMonth, storageBrandy.r1_OnHandFirstOfMonth);
                    Assert.AreEqual(storageReportBody.r20_Destroyed, storageBrandy.r20_Destroyed);
                    Assert.AreEqual(storageReportBody.r22_OtherLosses, storageBrandy.r22_OtherLosses);
                    Assert.AreEqual(storageReportBody.r23_OnHandEndOfMonth, storageBrandy.r23_OnHandEndOfMonth);
                    Assert.AreEqual(storageReportBody.r24_Lines7Through23, storageBrandy.r24_Lines7Through23);
                    Assert.AreEqual(storageReportBody.r2_DepositedInBulkStorage, storageBrandy.r2_DepositedInBulkStorage);
                    Assert.AreEqual(storageReportBody.r4_ReturnedToBulkStorage, storageBrandy.r4_ReturnedToBulkStorage);
                    Assert.AreEqual(storageReportBody.r6_TotalLines1Through5, storageBrandy.r6_TotalLines1Through5);
                    Assert.AreEqual(storageReportBody.r7_TaxPaid, storageBrandy.r7_TaxPaid);
                    Assert.AreEqual(storageReportBody.r6_TotalLines1Through5, storageBrandy.r6_TotalLines1Through5);
                }

                #endregion

                // let's verify the values in production report after we deleted bottling
                bool DistillationDeleted = _dl.DeleteProduction(prodO2, _userId);

                #region Distilled Not Gauged after Distilled deletion

                var amounts =
                    (from prod in _db.Production
                     where prod.ProductionID == productionId
                     join volume in _db.Volume on prod.VolumeID equals volume.VolumeID into volume_join
                     from volume in volume_join.DefaultIfEmpty()
                     join weight in _db.Weight on prod.WeightID equals weight.WeightID into weight_join
                     from weight in weight_join.DefaultIfEmpty()
                     join alcohol in _db.Alcohol on prod.AlcoholID equals alcohol.AlcoholID into alcohol_join
                     from alcohol in alcohol_join.DefaultIfEmpty()
                     join proof in _db.Proof on prod.ProofID equals proof.ProofID into proof_join
                     from proof in proof_join.DefaultIfEmpty()
                     select new
                     {
                         volume = (float?)volume.Value ?? (float?)0,
                         weight = (float?)weight.Value ?? (float?)0,
                         alcohol = (float?)alcohol.Value ?? (float?)0,
                         proof = (float?)proof.Value ?? (float?)0
                     }).FirstOrDefault();

                if (amounts != null)
                {
                    Assert.AreEqual(prodO.Quantity, amounts.volume);
                    Assert.AreEqual(prodO.VolumeByWeight, amounts.weight);
                    Assert.AreEqual(prodO.AlcoholContent, amounts.alcohol);
                    Assert.AreEqual(prodO.ProofGallon, amounts.proof);
                }
                else
                {
                    Assert.Inconclusive("amounts query yielded no results so could not perform this part of the test");
                }

                var isDistilledRecord =
                    (from prod in _db.Production
                     where prod.ProductionID == productionId
                     select prod).FirstOrDefault();

                if (isDistilledRecord == null)
                {
                    Assert.IsNotNull(isDistilledRecord, "distilled not gauged record should exist after the deletion of Distilled record");
                }

                #endregion

                #region Production Report After Deletion
                // verify Production report Part 1
                actualProdReportObject = _dl.GetProductionReportData(start, end, _userId);

                Assert.AreEqual(0, actualProdReportObject.Part1List[0].ProccessingAcct);
                Assert.AreEqual(0, actualProdReportObject.Part1List[0].StorageAcct);
                Assert.AreEqual(0, actualProdReportObject.Part1List[0].ProducedTotal);
                Assert.AreEqual(18, actualProdReportObject.Part1List[0].Recd4RedistilaltionL15);
                Assert.AreEqual(part1E.Recd4RedistilL17, actualProdReportObject.Part1List[0].Recd4RedistilL17);

                expectedWine = accumulatedProofGL4SpiritTypeReportingIdL.Find(l => l.Item1 == 11);
                actualWine = actualProdReportObject.Part1List.Find(x => x.SpiritTypeReportingID == expectedWine.Item1);

                if (actualWine == null)
                {
                    Assert.AreNotEqual(null, expectedWine, "No records in actualAlcohol190Plus for SpiritTypeID");
                }
                else
                {
                    Assert.AreEqual(expectedWine.Item2, actualWine.Recd4RedistilaltionL15);
                    Assert.AreEqual("Wine", actualWine.SpiritCatName);
                }

                Assert.AreEqual(part1E.UnfinishedSpiritsEndOfQuarterL17, actualProdReportObject.Part1List[0].UnfinishedSpiritsEndOfQuarterL17);

                // verify Production report Part 2 trough 4
                Assert.AreEqual(0, actualProdReportObject.Part2Through4List.Count);

                // verify part 5
                Assert.AreEqual("Wine", actualProdReportObject.part5List[0].KindofSpirits);
                Assert.AreEqual(18, actualProdReportObject.part5List[0].Proof);

                // verify Production report Part 6 - no data should be present in this scenario
                Assert.AreEqual(0, actualProdReportObject.ProdReportPart6List.Count);
                #endregion

                #region Storage Report After Deletion
                actualStorageReportObject = new StorageReport();

                storageReportBody = new StorageReportCategory();
                storageReportBody.CategoryName = "Wine";
                storageReportBody.r17_TransferredToProcessingAccount = 0f;
                storageReportBody.r18_TransferredToProductionAccount = 18f;
                storageReportBody.r19_TransferredToOtherBondedPremises = 0;
                storageReportBody.r1_OnHandFirstOfMonth = 0f;
                storageReportBody.r20_Destroyed = 0f;
                storageReportBody.r22_OtherLosses = 0f;
                storageReportBody.r23_OnHandEndOfMonth = 0f;
                storageReportBody.r24_Lines7Through23 = 18f;
                storageReportBody.r2_DepositedInBulkStorage = 18f;
                storageReportBody.r4_ReturnedToBulkStorage = 0f;
                storageReportBody.r6_TotalLines1Through5 = 18f;
                storageReportBody.r7_TaxPaid = 0f;

                actualStorageReportObject = _dl.GetStorageReportData(start, end, _userId);

                // verify Storage report Header
                Assert.AreEqual(reportHeaderE.DSP, actualStorageReportObject.Header.DSP);
                Assert.AreEqual(reportHeaderE.EIN, actualStorageReportObject.Header.EIN);
                Assert.AreEqual(reportHeaderE.PlantAddress, actualStorageReportObject.Header.PlantAddress);
                Assert.AreEqual(reportHeaderE.ProprietorName, actualStorageReportObject.Header.ProprietorName);
                Assert.AreEqual(reportHeaderE.ReportDate, actualStorageReportObject.Header.ReportDate);

                // verify Storage body for Wine
                storageWine = actualStorageReportObject.ReportBody.Find(x => x.CategoryName == "Wine");

                if (storageWine == null)
                {
                    Assert.IsNotNull(storageWine);
                }

                Assert.AreEqual(storageReportBody.CategoryName, storageWine.CategoryName);
                Assert.AreEqual(storageReportBody.r17_TransferredToProcessingAccount, storageWine.r17_TransferredToProcessingAccount);
                Assert.AreEqual(storageReportBody.r18_TransferredToProductionAccount, storageWine.r18_TransferredToProductionAccount);
                Assert.AreEqual(storageReportBody.r19_TransferredToOtherBondedPremises, storageWine.r19_TransferredToOtherBondedPremises);
                Assert.AreEqual(storageReportBody.r1_OnHandFirstOfMonth, storageWine.r1_OnHandFirstOfMonth);
                Assert.AreEqual(storageReportBody.r20_Destroyed, storageWine.r20_Destroyed);
                Assert.AreEqual(storageReportBody.r22_OtherLosses, storageWine.r22_OtherLosses);
                Assert.AreEqual(storageReportBody.r23_OnHandEndOfMonth, storageWine.r23_OnHandEndOfMonth);
                Assert.AreEqual(storageReportBody.r24_Lines7Through23, storageWine.r24_Lines7Through23);
                Assert.AreEqual(storageReportBody.r2_DepositedInBulkStorage, storageWine.r2_DepositedInBulkStorage);
                Assert.AreEqual(storageReportBody.r4_ReturnedToBulkStorage, storageWine.r4_ReturnedToBulkStorage);
                Assert.AreEqual(storageReportBody.r6_TotalLines1Through5, storageWine.r6_TotalLines1Through5);
                Assert.AreEqual(storageReportBody.r7_TaxPaid, storageWine.r7_TaxPaid);

                // verify Storage body for CategoryName BrandyUnder170
                storageBrandy = actualStorageReportObject.ReportBody.Find(x => x.CategoryName == "BrandyUnder170");

                Assert.IsNull(storageBrandy);

                #endregion
            }
            finally
            {
                // Cleanup
                foreach (var i in tupleL)
                {
                    TestRecordCleanup(i.Item1, i.Item2);
                }
            }
        }

        /// <summary>
        /// This test validates the following scenario
        /// Purchase 2000 lbs of grapes in January
        /// Ferment 2000 lbs of grapes in January into 150 gallons of wine @10% ABV
        /// Distil all of wine into 15 gallons of distill @60%ABV in January
        /// Blend entire distillation into Blending in January
        /// Bottle all of blended quantity in January
        /// Check Storage, Production and Processing Reports
        /// </summary>
        [TestMethod()]
        public void BuyGrapes_Ferment_DistilGauged_BlendAll_BottleAll_TestProcessingReport()
        {
            // Arrange
            int spiritId = 0;
            int waterMaterialId = 0;
            int vendorId = 0;
            int storageId = 0;
            int grapeMaterialId = 0;
            int purchaseId = 0;
            int productionId = 0;

            // reporting time range
            DateTime start = new DateTime(2018, 01, 01);
            DateTime end = new DateTime(2018, 01, 31);

            List<Tuple<int/*recordId*/, Table/*table enum vaue*/>> tablesForCleanupTupleList = new List<Tuple<int, Table>>();

            try
            {
                //  dictionary setup
                #region Dictionary

                //  dictionary setup
                SpiritObject spirit = new SpiritObject();
                spirit.SpiritName = "Brandy Under 170";
                spirit.ProcessingReportTypeID = 12;

                spiritId = _dl.CreateSpirit(_userId, spirit);

                // setup Vendor object
                VendorObject vendor = new VendorObject();
                vendor.VendorName = "VendorTest";

                vendorId = _dl.CreateVendor(_userId, vendor);
                tablesForCleanupTupleList.Add(Tuple.Create(vendorId, Table.Vendor));

                // setup Storage Object
                StorageObject storage = new StorageObject();
                storage.StorageName = "testStorage";
                storage.SerialNumber = "2H29NNS";

                storageId = _dl.CreateStorage(_userId, storage);
                tablesForCleanupTupleList.Add(Tuple.Create(storageId, Table.Storage));

                // setup Material Object
                // grapes
                {
                    RawMaterialObject grapeMaterial = new RawMaterialObject();
                    grapeMaterial.RawMaterialName = "Grapes";
                    grapeMaterial.MaterialCategoryID = (int)Persistence.BusinessLogicEnums.ProductionReportMaterialCategory.Fruit;
                    grapeMaterial.UnitType = "lb";
                    grapeMaterial.UnitTypeId = 2;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Fermentable = true;
                    grapeMaterial.PurchaseMaterialTypes = materialBoolTypes;

                    grapeMaterialId = _dl.CreateRawMaterial(_userId, grapeMaterial);

                    tablesForCleanupTupleList.Add(Tuple.Create(grapeMaterialId, Table.MaterialDict));
                }
                // water
                {
                    RawMaterialObject waterMaterial = new RawMaterialObject();
                    waterMaterial.RawMaterialName = "Water";
                    waterMaterial.UnitType = "gal";
                    waterMaterial.UnitTypeId = 1;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Additive = true;
                    waterMaterial.PurchaseMaterialTypes = materialBoolTypes;

                    waterMaterialId = _dl.CreateRawMaterial(_userId, waterMaterial);
                    tablesForCleanupTupleList.Add(Tuple.Create(waterMaterialId, Table.MaterialDict));
                }

                #endregion

                #region Purchase
                // create Purchase Record (minimal required fields)
                PurchaseObject purchO = new PurchaseObject();
                purchO.PurBatchName = "Riesling Grapes ";
                purchO.PurchaseType = "Fermentable";
                purchO.PurchaseDate = new DateTime(2018, 01, 03);
                purchO.Quantity = 0f;
                purchO.VolumeByWeight = 2000f;
                purchO.RecordId = grapeMaterialId;
                purchO.Price = 350f;
                purchO.VendorId = vendorId;

                List<StorageObject> storageList = new List<StorageObject>();
                StorageObject storageObject = new StorageObject();
                storageObject.StorageId = storageId;
                storageList.Add(storageObject);
                purchO.Storage = storageList;

                purchaseId = _dl.CreatePurchase(purchO, _userId);
                tablesForCleanupTupleList.Add(Tuple.Create(purchaseId, Table.Purchase));

                #endregion

                #region Production
                // create Fermented record
                ProductionObject prodO = new ProductionObject();
                prodO.BatchName = "Riesling Wine";
                prodO.ProductionDate = new DateTime(2018, 01, 15);
                prodO.ProductionStart = new DateTime(2018, 01, 15);
                prodO.ProductionEnd = new DateTime(2018, 01, 15);
                prodO.Gauged = true;
                prodO.ProductionType = "Fermentation";
                prodO.ProductionTypeId = 1;
                prodO.Quantity = 150f; // 150 gallons of wine
                prodO.VolumeByWeight = 0f;
                prodO.AlcoholContent = 10f; // %
                prodO.ProofGallon = 30f; // pfg
                prodO.Storage = storageList; // we are using the same storage id as we use for Purchase to keep things simple
                prodO.SpiritTypeReportingID = 11; // Wine
                prodO.MaterialKindReportingID = 0;

                List<ObjInfo4Burndwn> usedMats = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat = new ObjInfo4Burndwn();
                uMat.ID = purchaseId;
                uMat.OldVal = 0f;
                uMat.NewVal = purchO.VolumeByWeight;
                uMat.DistillableOrigin = "pur";
                uMat.BurningDownMethod = "weight";

                usedMats.Add(uMat);

                prodO.UsedMats = usedMats;

                productionId = _dl.CreateProduction(prodO, _userId);

                tablesForCleanupTupleList.Add(Tuple.Create(productionId, Table.Production));

                // create Production Distillation Record and mark it as Gauged
                ProductionObject prodO1 = new ProductionObject();
                prodO1.BatchName = "DistilRunAndGauged";
                prodO1.ProductionDate = new DateTime(2018, 01, 20);
                prodO1.ProductionStart = new DateTime(2018, 01, 20);
                prodO1.ProductionEnd = new DateTime(2018, 01, 20);
                prodO1.SpiritCutId = 11; // mixed
                prodO1.Gauged = true;
                prodO1.ProductionType = "Distillation";
                prodO1.ProductionTypeId = 2;
                prodO1.Quantity = 15f;
                prodO1.VolumeByWeight = 0f;
                prodO1.AlcoholContent = 60f;
                prodO1.ProofGallon = 18f;
                prodO1.Storage = storageList; // we are using the same storage id as we use for Purchase to keep things simple
                prodO1.SpiritTypeReportingID = 3; // Brandy 170-
                prodO1.MaterialKindReportingID = 94; // grape brandy

                List<ObjInfo4Burndwn> usedMats1 = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat1 = new ObjInfo4Burndwn();
                uMat1.ID = productionId;
                uMat1.OldVal = 0f;
                uMat1.NewVal = prodO.Quantity;
                uMat1.DistillableOrigin = "prod";
                uMat1.BurningDownMethod = "volume";

                usedMats1.Add(uMat1);

                prodO1.UsedMats = usedMats1;

                productionId = _dl.CreateProduction(prodO1, _userId);

                tablesForCleanupTupleList.Add(Tuple.Create(productionId, Table.Production));

                // create Production Blending Record
                ProductionObject prodBlend = new ProductionObject();
                prodBlend.BatchName = "testProdBlend";
                prodBlend.ProductionDate = new DateTime(2018, 01, 21);
                prodBlend.ProductionStart = new DateTime(2018, 01, 21);
                prodBlend.ProductionEnd = new DateTime(2018, 01, 21);
                prodBlend.Gauged = true;
                prodBlend.ProductionType = "Blending";
                prodBlend.Quantity = 22.5f;
                prodBlend.VolumeByWeight = 0f;
                prodBlend.AlcoholContent = 40f; // 40%
                prodBlend.ProofGallon = 18f;
                prodBlend.Storage = storageList; // we are using the same storage id as we use for Purchase to keep things simple
                prodBlend.SpiritTypeReportingID = 3; // brandy under 170
                prodBlend.MaterialKindReportingID = 94; // grape brandy
                prodBlend.SpiritId = spiritId;
                prodBlend.ProductionTypeId = 3;

                List<ObjInfo4Burndwn> usedMats4Blend = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat4Blend = new ObjInfo4Burndwn();
                uMat4Blend.ID = productionId;
                uMat4Blend.OldVal = 0;
                uMat4Blend.NewVal = prodO1.Quantity;
                uMat4Blend.DistillableOrigin = "prod";
                uMat4Blend.BurningDownMethod = "volume";
                uMat4Blend.Proof = prodO1.ProofGallon;

                usedMats4Blend.Add(uMat4Blend);
                prodBlend.UsedMats = usedMats4Blend;

                List<BlendingAdditive> blendAdditives = new List<BlendingAdditive>();
                BlendingAdditive blendAd = new BlendingAdditive();
                blendAd.RawMaterialId = waterMaterialId;
                blendAd.RawMaterialQuantity = 7.5f;
                blendAd.RawMaterialName = "Water";
                blendAd.UnitOfMeasurement = "gal";

                blendAdditives.Add(blendAd);

                prodBlend.BlendingAdditives = blendAdditives;

                int productionBlendingId = _dl.CreateProduction(prodBlend, _userId);
                tablesForCleanupTupleList.Add(Tuple.Create(productionBlendingId, Table.Production));

                // create Production Bottling Record
                ProductionObject prodBottl = new ProductionObject();
                prodBottl.BatchName = "testProdBottling ";
                prodBottl.ProductionDate = new DateTime(2018, 01, 22);
                prodBottl.ProductionStart = new DateTime(2018, 01, 22);
                prodBottl.ProductionEnd = new DateTime(2018, 01, 22);
                prodBottl.Gauged = true;
                prodBottl.ProductionType = "Bottling";
                prodBottl.Quantity = 22.39f; // 22.39 gallons of alcohol
                prodBottl.VolumeByWeight = 0f;
                prodBottl.AlcoholContent = 40f; // 40%
                prodBottl.ProofGallon = 17.9f; // 17.9 pfg
                prodBottl.Storage = storageList; // we are using the same storage id as we use for Purchase to keep things simple
                prodBottl.SpiritTypeReportingID = 3; // brandy under 170
                prodBottl.MaterialKindReportingID = 94; // grape brandy
                prodBottl.SpiritId = spiritId;
                prodO.ProductionTypeId = 4;

                List<ObjInfo4Burndwn> usedMats4Bottl = new List<ObjInfo4Burndwn>();
                ObjInfo4Burndwn uMat4Bottl = new ObjInfo4Burndwn();
                uMat4Bottl.ID = productionBlendingId;
                uMat4Bottl.OldVal = 0f;
                uMat4Bottl.NewVal = prodBlend.Quantity;
                uMat4Bottl.DistillableOrigin = "prod";
                uMat4Bottl.BurningDownMethod = "volume";

                usedMats4Bottl.Add(uMat4Bottl);
                prodBottl.UsedMats = usedMats4Bottl;

                BottlingObject bottlingObj = new BottlingObject();
                bottlingObj.CaseCapacity = 12;
                bottlingObj.CaseQuantity = 9.42f;
                bottlingObj.BottleCapacity = 750f;
                bottlingObj.BottleQuantity = 113;

                prodBottl.BottlingInfo = bottlingObj;

                prodBottl.GainLoss = .10f;

                prodBottl.FillTestList = null;

                int productionBottlingId = _dl.CreateProduction(prodBottl, _userId); // here productionId is overriden with a new productionId of the new Gauged record
                tablesForCleanupTupleList.Add(Tuple.Create(productionBottlingId, Table.Production));

                #endregion

                #region Reports
                /* PROCESING REPORT */
                #region Processing Report
                ProcessingReportingObject actualProcessingReportObject = new ProcessingReportingObject();

                ProcessReportingPart1 processingReportP1 = new ProcessReportingPart1();
                processingReportP1.AmtBottledPackaged = 17.9f;
                processingReportP1.BulkIngredients = "spirit";
                processingReportP1.Destroyed = 0f;
                processingReportP1.Dumped4Processing = 0f;
                processingReportP1.Gains = 0f;
                processingReportP1.Losses = 0.1f;
                processingReportP1.OnHandEndofMonth = 0f;
                processingReportP1.OnHandFirstofMonth = 0f;
                processingReportP1.Recd4Process = 18f;
                processingReportP1.Transf2Prod4Redistil = 0f;
                processingReportP1.Used4Redistil = 0f;
                processingReportP1.WineMixedWithSpirit = 0f;

                ProcessReportingPart2 processingReportP2 = new ProcessReportingPart2();
                processingReportP2.AmtBottledPackaged = 17.9f;
                processingReportP2.Destroyed = 0f;
                processingReportP2.Dumped4Processing = 0f;
                processingReportP2.FinishedProduct = "bottled";
                processingReportP2.InventoryOverage = 0f;
                processingReportP2.InventoryShortage = 0f;
                processingReportP2.OnHandEndofMonth = 17.9f;
                processingReportP2.OnHandFirstofMonth = 0f;
                processingReportP2.Recd4Process = 0f;
                processingReportP2.RecordedLosses = 0f;
                processingReportP2.TaxWithdrawn = 0f;
                processingReportP2.Transf2Prod4Redistil = 0f;

                ProcessReportingPart4 processingReportP4 = new ProcessReportingPart4();
                processingReportP4.AlcoholNeutral = 0f;
                processingReportP4.BlendedLightWhiskey = 0f;
                processingReportP4.BlendedOtherWhiskey = 0f;
                processingReportP4.BlendedStraightWhiskey = 0f;
                processingReportP4.BlendedWhiskeyWithLight = 0f;
                processingReportP4.BlendedWhiskeyWithNeutral = 0f;
                processingReportP4.Brandy170Under = 22.39f;
                processingReportP4.BrandyOver170 = 0f;
                processingReportP4.Cocktail = 0f;
                processingReportP4.DomesticWhiskey160Under = 0f;
                processingReportP4.DomesticWhiskeyOver160 = 0f;
                processingReportP4.Gin = 0f;
                processingReportP4.ImportedWhiskeyCanadian = 0f;
                processingReportP4.ImportedWhiskeyIrish = 0f;
                processingReportP4.ImportedWhiskeyScotch = 0f;
                processingReportP4.Liqueur = 0f;
                processingReportP4.ProcessingReportTypeName = "BRANDY DISTILLED AT 170 AND UNDER";
                processingReportP4.ProcessingSpirits = "bottled";
                processingReportP4.ProcessingTypeID = 12;
                processingReportP4.RumDomestic = 0f;
                processingReportP4.RumOtherImported = 0f;
                processingReportP4.RumPuertoRican = 0f;
                processingReportP4.RumVirginIslands = 0f;
                processingReportP4.StateID = 5;
                processingReportP4.Tequila = 0f;
                processingReportP4.Vodka = 0f;

                actualProcessingReportObject = _dl.GetProcessingReportData(start, end, _userId);

                Assert.AreEqual(processingReportP1.AmtBottledPackaged, actualProcessingReportObject.Part1.AmtBottledPackaged);
                Assert.AreEqual(processingReportP1.BulkIngredients, actualProcessingReportObject.Part1.BulkIngredients);
                Assert.AreEqual(processingReportP1.Destroyed, actualProcessingReportObject.Part1.Destroyed);
                Assert.AreEqual(processingReportP1.Dumped4Processing, actualProcessingReportObject.Part1.Dumped4Processing);
                Assert.AreEqual(processingReportP1.Gains, actualProcessingReportObject.Part1.Gains);
                Assert.AreEqual(processingReportP1.Losses, actualProcessingReportObject.Part1.Losses);
                Assert.AreEqual(processingReportP1.OnHandEndofMonth, actualProcessingReportObject.Part1.OnHandEndofMonth);
                Assert.AreEqual(processingReportP1.OnHandFirstofMonth, actualProcessingReportObject.Part1.OnHandFirstofMonth);
                Assert.AreEqual(processingReportP1.Recd4Process, actualProcessingReportObject.Part1.Recd4Process);
                Assert.AreEqual(processingReportP1.Transf2Prod4Redistil, actualProcessingReportObject.Part1.Transf2Prod4Redistil);
                Assert.AreEqual(processingReportP1.Used4Redistil, actualProcessingReportObject.Part1.Used4Redistil);
                Assert.AreEqual(processingReportP1.WineMixedWithSpirit, actualProcessingReportObject.Part1.WineMixedWithSpirit);

                Assert.AreEqual(processingReportP2.AmtBottledPackaged, actualProcessingReportObject.Part2.AmtBottledPackaged);
                Assert.AreEqual(processingReportP2.Destroyed, actualProcessingReportObject.Part2.Destroyed);
                Assert.AreEqual(processingReportP2.Dumped4Processing, actualProcessingReportObject.Part2.Dumped4Processing);
                Assert.AreEqual(processingReportP2.FinishedProduct, actualProcessingReportObject.Part2.FinishedProduct);
                Assert.AreEqual(processingReportP2.InventoryOverage, actualProcessingReportObject.Part2.InventoryOverage);
                Assert.AreEqual(processingReportP2.InventoryShortage, actualProcessingReportObject.Part2.InventoryShortage);
                Assert.AreEqual(processingReportP2.OnHandEndofMonth, actualProcessingReportObject.Part2.OnHandEndofMonth);
                Assert.AreEqual(processingReportP2.OnHandFirstofMonth, actualProcessingReportObject.Part2.OnHandFirstofMonth);
                Assert.AreEqual(processingReportP2.Recd4Process, actualProcessingReportObject.Part2.Recd4Process);
                Assert.AreEqual(processingReportP2.RecordedLosses, actualProcessingReportObject.Part2.RecordedLosses);
                Assert.AreEqual(processingReportP2.TaxWithdrawn, actualProcessingReportObject.Part2.TaxWithdrawn);
                Assert.AreEqual(processingReportP2.Transf2Prod4Redistil, actualProcessingReportObject.Part2.Transf2Prod4Redistil);

                Assert.AreEqual(processingReportP4.AlcoholNeutral, actualProcessingReportObject.Part4List[0].AlcoholNeutral);
                Assert.AreEqual(processingReportP4.BlendedLightWhiskey, actualProcessingReportObject.Part4List[0].BlendedLightWhiskey);
                Assert.AreEqual(processingReportP4.BlendedOtherWhiskey, actualProcessingReportObject.Part4List[0].BlendedOtherWhiskey);
                Assert.AreEqual(processingReportP4.BlendedStraightWhiskey, actualProcessingReportObject.Part4List[0].BlendedStraightWhiskey);
                Assert.AreEqual(processingReportP4.BlendedWhiskeyWithLight, actualProcessingReportObject.Part4List[0].BlendedWhiskeyWithLight);
                Assert.AreEqual(processingReportP4.BlendedWhiskeyWithNeutral, actualProcessingReportObject.Part4List[0].BlendedWhiskeyWithNeutral);
                Assert.AreEqual(processingReportP4.Brandy170Under, actualProcessingReportObject.Part4List[1].Brandy170Under);
                Assert.AreEqual(processingReportP4.BrandyOver170, actualProcessingReportObject.Part4List[0].BrandyOver170);
                Assert.AreEqual(processingReportP4.Cocktail, actualProcessingReportObject.Part4List[0].Cocktail);
                Assert.AreEqual(processingReportP4.DomesticWhiskey160Under, actualProcessingReportObject.Part4List[0].DomesticWhiskey160Under);
                Assert.AreEqual(processingReportP4.DomesticWhiskeyOver160, actualProcessingReportObject.Part4List[0].DomesticWhiskeyOver160);
                Assert.AreEqual(processingReportP4.Gin, actualProcessingReportObject.Part4List[0].Gin);
                Assert.AreEqual(processingReportP4.ImportedWhiskeyCanadian, actualProcessingReportObject.Part4List[0].ImportedWhiskeyCanadian);
                Assert.AreEqual(processingReportP4.ImportedWhiskeyIrish, actualProcessingReportObject.Part4List[0].ImportedWhiskeyIrish);
                Assert.AreEqual(processingReportP4.ImportedWhiskeyScotch, actualProcessingReportObject.Part4List[0].ImportedWhiskeyScotch);
                Assert.AreEqual(processingReportP4.Liqueur, actualProcessingReportObject.Part4List[0].Liqueur);
                Assert.AreEqual(processingReportP4.ProcessingReportTypeName, actualProcessingReportObject.Part4List[0].ProcessingReportTypeName);
                Assert.AreEqual(processingReportP4.ProcessingSpirits, actualProcessingReportObject.Part4List[1].ProcessingSpirits);
                Assert.AreEqual(processingReportP4.ProcessingTypeID, actualProcessingReportObject.Part4List[0].ProcessingTypeID);
                Assert.AreEqual(processingReportP4.RumDomestic, actualProcessingReportObject.Part4List[0].RumDomestic);
                Assert.AreEqual(processingReportP4.RumDomestic, actualProcessingReportObject.Part4List[0].RumDomestic);
                Assert.AreEqual(processingReportP4.RumOtherImported, actualProcessingReportObject.Part4List[0].RumOtherImported);
                Assert.AreEqual(processingReportP4.RumPuertoRican, actualProcessingReportObject.Part4List[0].RumPuertoRican);
                Assert.AreEqual(processingReportP4.StateID, actualProcessingReportObject.Part4List[1].StateID);
                Assert.AreEqual(processingReportP4.Tequila, actualProcessingReportObject.Part4List[0].Tequila);
                Assert.AreEqual(processingReportP4.Vodka, actualProcessingReportObject.Part4List[0].Vodka);
                #endregion

                #region Storage Report
                // verify storage report
                StorageReport actualStorageReportObject = new StorageReport();

                actualStorageReportObject = _dl.GetStorageReportData(start, end, _userId);

                // check grape brandy object
                var actualStoGrapeBrandy = actualStorageReportObject.ReportBody.Find(x => x.CategoryName == "BrandyUnder170");

                if (actualStoGrapeBrandy != null)
                {
                    Assert.IsNull(actualStoGrapeBrandy, "Storage Grape Brandy Under 170 object should be null");
                }

                // check Wine object
                var actualStoWine = actualStorageReportObject.ReportBody.Find(x => x.CategoryName == "Wine");

                if (actualStoWine == null)
                {
                    Assert.IsNotNull(actualStoGrapeBrandy, "Storage Wine object should not be null");
                }
                else
                {
                    Assert.AreEqual(30f, actualStoWine.r2_DepositedInBulkStorage);
                    Assert.AreEqual(30f, actualStoWine.r6_TotalLines1Through5);
                    Assert.AreEqual(0, actualStoWine.r23_OnHandEndOfMonth);
                    Assert.AreEqual(0f, actualStoWine.r1_OnHandFirstOfMonth);
                    Assert.AreEqual(30f, actualStoWine.r24_Lines7Through23);
                    Assert.AreEqual(30f, actualStoWine.r18_TransferredToProductionAccount);
                    Assert.AreEqual(30f, actualStoWine.r6_TotalLines1Through5);
                }

                #endregion

                #region Production Report

                ProductionReportingObject actualProdReportObject = new ProductionReportingObject();

                actualProdReportObject = _dl.GetProductionReportData(start, end, _userId);

                var expectedWine = actualProdReportObject.Part1List.Find(x => x.SpiritCatName == "Wine");

                // verify Production report part 1
                if (expectedWine == null)
                {
                    Assert.IsNotNull(expectedWine, "Expect records for Wine Category");
                }
                else
                {
                    // Wine expected output
                    Assert.AreEqual(0, expectedWine.ProccessingAcct);
                    Assert.AreEqual(0, expectedWine.StorageAcct);
                    Assert.AreEqual(0, expectedWine.ProducedTotal);
                    Assert.AreEqual(30, expectedWine.Recd4RedistilaltionL15);
                }

                var expectedBrandy = actualProdReportObject.Part1List.Find(x => x.SpiritCatName == "BrandyUnder170");

                if (expectedBrandy == null)
                {
                    Assert.IsNotNull(expectedWine, "Expect records for Brandy Category");
                }
                else
                {
                    // Brandy expected output
                    Assert.AreEqual(18, expectedBrandy.ProccessingAcct);
                    Assert.AreEqual(0, expectedBrandy.StorageAcct);
                    Assert.AreEqual(0, expectedBrandy.Recd4RedistilL17);
                    Assert.AreEqual(18, expectedBrandy.ProducedTotal);
                }

                // verify Production report Part 2 trough 4
                Assert.AreEqual("GrapeBrandy", actualProdReportObject.Part2Through4List[0].KindOfMaterial);
                Assert.AreEqual(false, actualProdReportObject.Part2Through4List[0].NewCoop);
                Assert.AreEqual(false, actualProdReportObject.Part2Through4List[0].UsedCoop);
                Assert.AreEqual(18, actualProdReportObject.Part2Through4List[0].ProofGallons);
                Assert.AreEqual(false, actualProdReportObject.Part2Through4List[0].Tanks);

                // verify part 5
                Assert.AreEqual("Wine", actualProdReportObject.part5List[0].KindofSpirits);
                Assert.AreEqual(30f, actualProdReportObject.part5List[0].Proof);

                // verify Production report Part 6 - no data should be present in this scenario
                Assert.AreEqual(1, actualProdReportObject.ProdReportPart6List.Count);
                #endregion

                #endregion
            }
            finally
            {
                // Cleanup created records
                foreach (var i in tablesForCleanupTupleList)
                {
                    TestRecordCleanup(i.Item1, i.Item2);
                }
            }
        }

        /// <summary>
        /// TestRecordCleanup method cleans up test data from tables
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tableIdent"></param>
        /// <returns></returns>
        private bool TestRecordCleanup(int id, Table tableIdent)
        {
            bool success = false;

            // Arrange
            int distillerId = _dl.GetDistillerId(1);

            switch (tableIdent)
            {
                case Table.Spirit:
                    {
                        try
                        {
                            var rec =
                            (from res in _db.Spirit
                             where res.SpiritID == id
                             select res).FirstOrDefault();

                            if (rec != null)
                            {
                                _db.Spirit.Remove(rec);
                            }

                            _db.SaveChanges();

                            success = true;
                        }
                        catch
                        {
                            throw;
                        }

                        break;
                    }

                case Table.Vendor:
                    {
                        try
                        {
                            var rec =
                            (from res in _db.Vendor
                             where res.VendorID == id
                             select res).FirstOrDefault();

                            if (rec != null)
                            {
                                _db.Vendor.Remove(rec);
                            }

                            var rec1 =
                            (from res in _db.VendorDetail
                             where res.VendorID == id
                             select res).FirstOrDefault();

                            if (rec1 != null)
                            {
                                _db.VendorDetail.Remove(rec1);
                            }

                            _db.SaveChanges();

                            success = true;
                        }
                        catch
                        {
                            throw;
                        }

                        break;
                    }

                case Table.Storage:
                    {
                        try
                        {
                            var rec =
                            (from res in _db.Storage
                             where res.StorageID == id
                             select res).FirstOrDefault();

                            if (rec != null)
                            {
                                _db.Storage.Remove(rec);
                            }

                            var rec1 =
                            (from res in _db.StorageState
                             where res.StorageID == id
                             select res).FirstOrDefault();

                            if (rec1 != null)
                            {
                                _db.StorageState.Remove(rec1);
                            }

                            var sto2Rec =
                                    (from res in _db.StorageToRecord
                                     where res.StorageID == id
                                     select res);

                            if (sto2Rec != null)
                            {
                                foreach (var i in sto2Rec)
                                {
                                    _db.StorageToRecord.Remove(i);
                                }
                            }

                            _db.SaveChanges();

                            success = true;
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }

                        break;
                    }

                case Table.MaterialDict:
                    {
                        try
                        {
                            var rec =
                            (from res in _db.MaterialDict
                             where res.MaterialDictID == id
                             select res).FirstOrDefault();

                            if (rec != null)
                            {
                                _db.MaterialDict.Remove(rec);
                            }

                            var rec1 =
                            (from res in _db.MaterialType
                             where res.MaterialDictID == id
                             select res).FirstOrDefault();

                            if (rec1 != null)
                            {
                                _db.MaterialType.Remove(rec1);
                            }

                            var rec2 =
                            (from res in _db.MaterialDict2MaterialCategory
                             where res.MaterialDictID == id
                             select res).FirstOrDefault();

                            if (rec2 != null)
                            {
                                _db.MaterialDict2MaterialCategory.Remove(rec2);
                            }

                            _db.SaveChanges();

                            success = true;
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }

                        break;
                    }
                case Table.Purchase:
                    {
                        try
                        {
                            var pur =
                                (from rec in _db.Purchase
                                 where rec.PurchaseID == id
                                 select rec).FirstOrDefault();

                            if (pur != null)
                            {
                                var vol =
                                    (from rec in _db.Volume
                                     where rec.VolumeID == pur.VolumeID
                                     select rec).FirstOrDefault();

                                if (vol != null)
                                {
                                    _db.Volume.Remove(vol);
                                }

                                var weight =
                                    (from rec in _db.Weight
                                     where rec.WeightID == pur.WeightID
                                     select rec).FirstOrDefault();

                                if (weight != null)
                                {
                                    _db.Weight.Remove(weight);
                                }

                                var alc =
                                    (from rec in _db.Alcohol
                                     where rec.AlcoholID == pur.AlcoholID
                                     select rec).FirstOrDefault();

                                if (alc != null)
                                {
                                    _db.Alcohol.Remove(alc);
                                }

                                var prf =
                                    (from rec in _db.Proof
                                     where rec.ProofID == pur.ProofID
                                     select rec).FirstOrDefault();

                                if (prf != null)
                                {
                                    _db.Proof.Remove(prf);
                                }

                                var pur2SpiTRep =
                                    (from rec in _db.PurchaseToSpiritTypeReporting
                                     where rec.PurchaseID == pur.PurchaseID
                                     select rec).FirstOrDefault();

                                if (pur2SpiTRep != null)
                                {
                                    _db.PurchaseToSpiritTypeReporting.Remove(pur2SpiTRep);
                                }

                                var sto2Rec =
                                    (from rec in _db.StorageToRecord
                                     where rec.RecordId == pur.PurchaseID && rec.TableIdentifier == "pur"
                                     select rec);

                                if (sto2Rec != null)
                                {
                                    foreach (var i in sto2Rec)
                                    {
                                        _db.StorageToRecord.Remove(i);
                                    }
                                }

                                var pur4Rep =
                                    (from rec in _db.Purchase4Reporting
                                     where rec.PurchaseID == pur.PurchaseID
                                     select rec);

                                if (pur4Rep != null)
                                {
                                    foreach (var l in pur4Rep)
                                    {
                                        _db.Purchase4Reporting.Remove(l);
                                    }
                                }

                                var purH =
                                    (from rec in _db.PurchaseHistory
                                     where rec.PurchaseID == pur.PurchaseID
                                     select rec);

                                if (purH != null)
                                {
                                    foreach (var i in purH)
                                    {
                                        _db.PurchaseHistory.Remove(i);
                                    }
                                }

                                _db.Purchase.Remove(pur);
                            }

                            _db.SaveChanges();
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }

                        break;
                    }
                case Table.Production:
                    {
                        try
                        {
                            var prod =
                                (from rec in _db.Production
                                 where rec.ProductionID == id
                                 select rec).FirstOrDefault();

                            var prod2Spi =
                                (from rec in _db.ProductionToSpirit
                                 where rec.ProductionID == id
                                 select rec).FirstOrDefault();

                            if (prod2Spi != null)
                            {
                                var spirit =
                                    (from res in _db.Spirit
                                     where res.SpiritID == prod2Spi.SpiritID
                                     select res).FirstOrDefault();

                                if (spirit != null)
                                {
                                    _db.Spirit.Remove(spirit);
                                }

                                _db.ProductionToSpirit.Remove(prod2Spi);
                            }

                            if (prod != null)
                            {
                                var vol =
                                    (from rec in _db.Volume
                                     where rec.VolumeID == prod.VolumeID
                                     select rec).FirstOrDefault();

                                if (vol != null)
                                {
                                    _db.Volume.Remove(vol);
                                }

                                var weight =
                                    (from rec in _db.Weight
                                     where rec.WeightID == prod.WeightID
                                     select rec).FirstOrDefault();

                                if (weight != null)
                                {
                                    _db.Weight.Remove(weight);
                                }

                                var alc =
                                    (from rec in _db.Alcohol
                                     where rec.AlcoholID == prod.AlcoholID
                                     select rec).FirstOrDefault();

                                if (alc != null)
                                {
                                    _db.Alcohol.Remove(alc);
                                }

                                var prf =
                                    (from rec in _db.Proof
                                     where rec.ProofID == prod.ProofID
                                     select rec).FirstOrDefault();

                                if (prf != null)
                                {
                                    _db.Proof.Remove(prf);
                                }

                                var sto2Rec =
                                    (from rec in _db.StorageToRecord
                                     where rec.RecordId == prod.ProductionID && rec.TableIdentifier == "prod"
                                     select rec);

                                if (sto2Rec != null)
                                {
                                    foreach (var i in sto2Rec)
                                    {
                                        _db.StorageToRecord.Remove(i);
                                    }
                                }

                                var prod2SpiritTRep =
                                    (from rec in _db.ProductionToSpiritTypeReporting
                                     where rec.ProductionID == prod.ProductionID
                                     select rec).FirstOrDefault();

                                if (prod2SpiritTRep != null)
                                {
                                    _db.ProductionToSpiritTypeReporting.Remove(prod2SpiritTRep);
                                }

                                var prod2SpiCut =
                                    (from rec in _db.ProductionToSpiritCut
                                     where rec.ProductionID == prod.ProductionID
                                     select rec).FirstOrDefault();

                                if (prod2SpiCut != null)
                                {
                                    _db.ProductionToSpiritCut.Remove(prod2SpiCut);
                                }

                                var prodCont =
                                    (from rec in _db.ProductionContent
                                     where rec.ProductionID == prod.ProductionID
                                     select rec);

                                if (prodCont != null)
                                {
                                    foreach (var l in prodCont)
                                    {
                                        _db.ProductionContent.Remove(l);
                                    }
                                }

                                var prodHist =
                                    (from rec in _db.ProductionHistory
                                     where rec.ProductionID == prod.ProductionID
                                     select rec);

                                if (prodHist != null)
                                {
                                    foreach (var l in prodHist)
                                    {
                                        _db.ProductionHistory.Remove(l);
                                    }
                                }

                                var prod4Rep =
                                    (from rec in _db.Production4Reporting
                                     where rec.ProductionID == prod.ProductionID
                                     select rec);

                                if (prod4Rep != null)
                                {
                                    foreach (var i in prod4Rep)
                                    {
                                        _db.Production4Reporting.Remove(i);
                                    }
                                }

                                var prod2Pur =
                                    (from rec in _db.ProductionToPurchase
                                     where rec.ProductionID == prod.ProductionID
                                     select rec);

                                if (prod2Pur != null)
                                {
                                    foreach (var l in prod2Pur)
                                    {
                                        _db.ProductionToPurchase.Remove(l);
                                    }
                                }

                                var blendedComp =
                                    (from rec in _db.BlendedComponent
                                     where rec.ProductionID == prod.ProductionID
                                     select rec);

                                if (blendedComp != null)
                                {
                                    foreach (var l in blendedComp)
                                    {
                                        _db.BlendedComponent.Remove(l);
                                    }
                                }

                                var bottlI =
                                    (from rec in _db.BottlingInfo
                                     where rec.ProductionID == prod.ProductionID
                                     select rec);

                                if (bottlI != null)
                                {
                                    foreach (var l in bottlI)
                                    {
                                        _db.BottlingInfo.Remove(l);
                                    }
                                }

                                var fillTest =
                                    (from rec in _db.FillTest
                                     where rec.ProductionID == prod.ProductionID
                                     select rec);

                                if (fillTest != null)
                                {
                                    foreach (var l in fillTest)
                                    {
                                        _db.FillTest.Remove(l);
                                    }
                                }

                                var gainsLosses =
                                    (from rec in _db.GainLoss
                                     where rec.BottledRecordId == prod.ProductionID
                                     select rec);

                                if (gainsLosses != null)
                                {
                                    foreach (var l in gainsLosses)
                                    {
                                        _db.GainLoss.Remove(l);
                                    }
                                }

                                var spiritCut =
                                    (from rec in _db.ProductionToSpiritCut
                                     where rec.ProductionID == prod.ProductionID
                                     select rec);

                                if (spiritCut != null)
                                {
                                    foreach (var l in spiritCut)
                                    {
                                        _db.ProductionToSpiritCut.Remove(l);
                                    }
                                }

                                _db.Production.Remove(prod);
                            }
                            _db.SaveChanges();
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                        break;
                    }
            }

            return success;
        }

        /// <summary>
        /// WorkflowBrandyTestSetup this method creates records in 
        /// dictionary tables for multistep workflow test scenarios
        /// </summary>
        /// <param name="tablesForCleanupTupleList"></param>
        private void WorkflowBrandyTestSetup(ref List<Tuple<int/*recordId*/, Table/*table enum vaue*/>> tablesForCleanupTupleList)
        {
            int spiritId = 0;
            int vendorId = 0;
            int storageId = 0;
            int materialDictId = 0;

            // setup Brandy Spirit object
            SpiritObject spirit = new SpiritObject();
            spirit.SpiritName = "Brandy Under 170";
            spirit.ProcessingReportTypeID = 12;

            spiritId = _dl.CreateSpirit(_userId, spirit);
            tablesForCleanupTupleList.Add(Tuple.Create(spiritId, Table.Spirit));


            // setup Vendor object
            VendorObject vendor = new VendorObject();
            vendor.VendorName = "testVendor";

            vendorId = _dl.CreateVendor(_userId, vendor);
            tablesForCleanupTupleList.Add(Tuple.Create(vendorId, Table.Vendor));

            // setup Storage Object
            StorageObject storage = new StorageObject();
            storage.StorageName = "testStorage";
            storage.SerialNumber = "2H29NNS";

            storageId = _dl.CreateStorage(_userId, storage);
            tablesForCleanupTupleList.Add(Tuple.Create(storageId, Table.Storage));


            // setup Material Object
            // wine
            {
                RawMaterialObject wineMaterial = new RawMaterialObject();
                wineMaterial.RawMaterialName = "Wine For Brandy";
                wineMaterial.MaterialCategoryID = 2;
                wineMaterial.UnitType = "gal";
                wineMaterial.UnitTypeId = 1;
                PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                materialBoolTypes.Fermented = true;
                wineMaterial.PurchaseMaterialTypes = materialBoolTypes;

                materialDictId = _dl.CreateRawMaterial(_userId, wineMaterial);
                tablesForCleanupTupleList.Add(Tuple.Create(materialDictId, Table.MaterialDict));
            }

            // water
            {
                RawMaterialObject waterMaterial = new RawMaterialObject();
                waterMaterial.RawMaterialName = "Water";
                waterMaterial.UnitType = "gal";
                waterMaterial.UnitTypeId = 1;
                PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                materialBoolTypes.Additive = true;
                waterMaterial.PurchaseMaterialTypes = materialBoolTypes;

                materialDictId = _dl.CreateRawMaterial(_userId, waterMaterial);
                tablesForCleanupTupleList.Add(Tuple.Create(materialDictId, Table.MaterialDict));
            }

        }
    }
}