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

                ProdReportPart6 part6 = new ProdReportPart6();
                part6.KindOfMaterial = "Wine For Brandy";
                part6.ProdReportMaterialCategoryID = 2;
                part6.Volume = 0f;
                part6.Weight = 0f;

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
                Assert.AreEqual(18, actualProdReportObject.part5List[0].Proof);

                // verify Production report Part 6
                Assert.AreEqual(part6.KindOfMaterial, actualProdReportObject.ProdReportPart6List[0].KindOfMaterial);
                Assert.AreEqual(part6.ProdReportMaterialCategoryID, actualProdReportObject.ProdReportPart6List[0].ProdReportMaterialCategoryID);
                Assert.AreEqual(part6.Volume, actualProdReportObject.ProdReportPart6List[0].Volume);
                Assert.AreEqual(part6.Weight, actualProdReportObject.ProdReportPart6List[0].Weight);
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
        /// Test Processing Report part 4, column b (BULK SPIRITS DUMPED INTO PROCESSING (Whole proof gallons))
        /// </summary>
        [TestMethod()]
        public void PurchaseDistilled_Blend_GenerateProcessingReport()
        {
            // A dictionary to log database test records for later clean-up
            Dictionary<int, Table> testRecords = new Dictionary<int, Table>();

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
                testRecords.Add(spiritId, Table.Spirit);

                // Create Raw Material dictionary item
                RawMaterialObject rawMaterial = new RawMaterialObject
                {
                    RawMaterialName = "TestMaterial-4tUTav",
                    PurchaseMaterialTypes = new PurchaseMaterialBooleanTypes { Distilled = true },
                    UnitTypeId = 1,
                    UnitType = "gal"
                };

                int rawMaterialId = _dl.CreateRawMaterial(_userId, rawMaterial);
                testRecords.Add(rawMaterialId, Table.MaterialDict);

                // Create Vendor dictionary item
                VendorObject vendor = new VendorObject
                {
                    VendorName = "TestVendor-4tUTav"
                };

                int vendorId = _dl.CreateVendor(_userId, vendor);
                testRecords.Add(vendorId, Table.Vendor);

                // Create Storage dictionary item
                StorageObject storage = new StorageObject
                {
                    StorageName = "TestStorage-4tUTav"
                };

                int storageId = _dl.CreateStorage(_userId, storage);
                testRecords.Add(storageId, Table.Storage);

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
                testRecords.Add(purchaseId, Table.Purchase);

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
                testRecords.Add(productionId, Table.Production);

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
                    TestRecordCleanup(rec.Key, rec.Value);
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
            // A dictionary to log database test records for later clean-up
            Dictionary<int, Table> testRecords = new Dictionary<int, Table>();

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
                testRecords.Add(spiritId, Table.Spirit);

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
                testRecords.Add(rawMaterialId, Table.MaterialDict);

                // Create Vendor dictionary item
                VendorObject vendor = new VendorObject
                {
                    VendorName = "BigGrainsDistillery"
                };

                int vendorId = _dl.CreateVendor(_userId, vendor);
                testRecords.Add(vendorId, Table.Vendor);

                // Create Storage dictionary item
                StorageObject storage = new StorageObject
                {
                    StorageName = "ThaTank"
                };

                int storageId = _dl.CreateStorage(_userId, storage);
                testRecords.Add(storageId, Table.Storage);

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
                testRecords.Add(purchaseId, Table.Purchase);

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
                testRecords.Add(productionId1, Table.Production);

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
                testRecords.Add(productionId2, Table.Production);

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
                    TestRecordCleanup(rec.Key, rec.Value);
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
            // A dictionary to log database test records for later clean-up
            Dictionary<int, Table> testRecords = new Dictionary<int, Table>();

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
                testRecords.Add(spiritId, Table.Spirit);

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
                testRecords.Add(rawMaterialId, Table.MaterialDict);

                // Create Vendor dictionary item
                VendorObject vendor = new VendorObject
                {
                    VendorName = "BigGrapesWinery"
                };

                int vendorId = _dl.CreateVendor(_userId, vendor);
                testRecords.Add(vendorId, Table.Vendor);

                // Create Storage dictionary item
                StorageObject storage = new StorageObject
                {
                    StorageName = "TheTank"
                };

                int storageId = _dl.CreateStorage(_userId, storage);
                testRecords.Add(storageId, Table.Storage);

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
                testRecords.Add(purchaseId, Table.Purchase);

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
                testRecords.Add(productionId, Table.Production);

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
                    TestRecordCleanup(rec.Key, rec.Value);
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
                var receivedForRedistillationTotalResult = 0f; // this 

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
                    grapeMaterial.MaterialCategoryID = 2;
                    grapeMaterial.UnitType = "lb";
                    grapeMaterial.UnitTypeId = 2;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Fermentable = true;
                    grapeMaterial.PurchaseMaterialTypes = materialBoolTypes;
                    grapeMaterial.MaterialCategoryID = 2;

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
                    grapeMaterial.MaterialCategoryID = 2;
                    grapeMaterial.UnitType = "lb";
                    grapeMaterial.UnitTypeId = 2;
                    PurchaseMaterialBooleanTypes materialBoolTypes = new PurchaseMaterialBooleanTypes();
                    materialBoolTypes.Fermentable = true;
                    grapeMaterial.PurchaseMaterialTypes = materialBoolTypes;
                    grapeMaterial.MaterialCategoryID = 2;

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

                ProdReportPart6 part6 = new ProdReportPart6();
                part6.KindOfMaterial = "Wine For Brandy";
                part6.ProdReportMaterialCategoryID = 2;
                part6.Volume = 0f;
                part6.Weight = 0f;

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

                // verify Production report Part 6
                Assert.AreEqual(part6.KindOfMaterial, actualProdReportObject.ProdReportPart6List[0].KindOfMaterial);
                Assert.AreEqual(part6.ProdReportMaterialCategoryID, actualProdReportObject.ProdReportPart6List[0].ProdReportMaterialCategoryID);
                Assert.AreEqual(part6.Volume, actualProdReportObject.ProdReportPart6List[0].Volume);
                Assert.AreEqual(part6.Weight, actualProdReportObject.ProdReportPart6List[0].Weight);

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