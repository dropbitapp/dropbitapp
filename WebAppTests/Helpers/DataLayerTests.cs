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
            List<Tuple<int/*recordId*/, Table/*table enum vaue*/>> tupleL = new List<Tuple<int, Table>>();
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
                prodBlend.ProductionDate = new DateTime(2017, 09, 5);
                prodBlend.ProductionStart = new DateTime(2017, 09, 5);
                prodBlend.ProductionEnd = new DateTime(2017, 09, 5);
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

                // reporting time range
                DateTime start = new DateTime(2017, 09, 01);
                DateTime end = new DateTime(2017, 09, 30);

                /* PRODUCTION REPORT */

                ProdReportPart1 part1E = new ProdReportPart1();
                part1E.ProccessingAcct = 17.9f;
                part1E.ProducedTotal = 17.9f;
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
                part2thru4.ProofGallons = 17.9f;
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
                storageReportBody.r24_Lines7Through23 = 0f;
                storageReportBody.r2_DepositedInBulkStorage = 18f;
                storageReportBody.r4_ReturnedToBulkStorage = 0f;
                storageReportBody.r6_TotalLines1Through5 = 0f;
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
                // verify Production report header
                Assert.AreEqual(reportHeaderE.DSP, actualProdReportObject.Header.DSP);
                Assert.AreEqual(reportHeaderE.EIN, actualProdReportObject.Header.EIN);
                Assert.AreEqual(reportHeaderE.PlantAddress, actualProdReportObject.Header.PlantAddress);
                Assert.AreEqual(reportHeaderE.ProprietorName, actualProdReportObject.Header.ProprietorName);
                Assert.AreEqual(reportHeaderE.ReportDate, actualProdReportObject.Header.ReportDate);

                // verify Production report Part 1
                Assert.AreEqual(part1E.ProccessingAcct, actualProdReportObject.Part1[0].ProccessingAcct);
                Assert.AreEqual(part1E.ProducedTotal, actualProdReportObject.Part1[0].ProducedTotal);
                Assert.AreEqual(part1E.Recd4RedistilL17, actualProdReportObject.Part1[0].Recd4RedistilL17);
                Assert.AreEqual(part1E.Recd4RedistilaltionL15, actualProdReportObject.Part1[0].Recd4RedistilaltionL15);
                Assert.AreEqual(part1E.StorageAcct, actualProdReportObject.Part1[0].StorageAcct);
                Assert.AreEqual(part1E.SpiritCatName, actualProdReportObject.Part1[0].SpiritCatName);
                Assert.AreEqual(part1E.SpiritTypeReportingID, actualProdReportObject.Part1[0].SpiritTypeReportingID);
                Assert.AreEqual(part1E.UnfinishedSpiritsEndOfQuarterL17, actualProdReportObject.Part1[0].UnfinishedSpiritsEndOfQuarterL17);

                // verify Production report Part 2 trough 4
                Assert.AreEqual(part2thru4.KindOfMaterial, actualProdReportObject.Part2Through4[0].KindOfMaterial);
                Assert.AreEqual(part2thru4.MaterialKindReportingID, actualProdReportObject.Part2Through4[0].MaterialKindReportingID);
                Assert.AreEqual(part2thru4.NewCoop, actualProdReportObject.Part2Through4[0].NewCoop);
                Assert.AreEqual(part2thru4.UsedCoop, actualProdReportObject.Part2Through4[0].UsedCoop);
                Assert.AreEqual(part2thru4.ProofGallons, actualProdReportObject.Part2Through4[0].ProofGallons);
                Assert.AreEqual(part2thru4.SpiritTypeReportingID, actualProdReportObject.Part2Through4[0].SpiritTypeReportingID);
                Assert.AreEqual(part2thru4.Tanks, actualProdReportObject.Part2Through4[0].Tanks);

                // verify part 5
                Assert.AreEqual(0, actualProdReportObject.part5List.Count);

                // verify Production report Part 6
                Assert.AreEqual(part6.KindOfMaterial, actualProdReportObject.ProdReportPart6[0].KindOfMaterial);
                Assert.AreEqual(part6.ProdReportMaterialCategoryID, actualProdReportObject.ProdReportPart6[0].ProdReportMaterialCategoryID);
                Assert.AreEqual(part6.Volume, actualProdReportObject.ProdReportPart6[0].Volume);
                Assert.AreEqual(part6.Weight, actualProdReportObject.ProdReportPart6[0].Weight);
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
                Assert.AreEqual(processingReportP4.Gin, actualProcessingReportObject.Part4List[0].Gin);
                Assert.AreEqual(processingReportP4.ImportedWhiskeyCanadian, actualProcessingReportObject.Part4List[0].ImportedWhiskeyCanadian);
                Assert.AreEqual(processingReportP4.ImportedWhiskeyIrish, actualProcessingReportObject.Part4List[0].ImportedWhiskeyIrish);
                Assert.AreEqual(processingReportP4.ImportedWhiskeyScotch, actualProcessingReportObject.Part4List[0].ImportedWhiskeyScotch);
                Assert.AreEqual(processingReportP4.Liqueur, actualProcessingReportObject.Part4List[0].Liqueur);
                Assert.AreEqual(processingReportP4.ProcessingReportTypeName, actualProcessingReportObject.Part4List[0].ProcessingReportTypeName);
                Assert.AreEqual(processingReportP4.ProcessingSpirits, actualProcessingReportObject.Part4List[0].ProcessingSpirits);
                Assert.AreEqual(processingReportP4.ProcessingTypeID, actualProcessingReportObject.Part4List[0].ProcessingTypeID);
                Assert.AreEqual(processingReportP4.RumDomestic, actualProcessingReportObject.Part4List[0].RumDomestic);
                Assert.AreEqual(processingReportP4.RumDomestic, actualProcessingReportObject.Part4List[0].RumDomestic);
                Assert.AreEqual(processingReportP4.RumOtherImported, actualProcessingReportObject.Part4List[0].RumOtherImported);
                Assert.AreEqual(processingReportP4.RumPuertoRican, actualProcessingReportObject.Part4List[0].RumPuertoRican);
                Assert.AreEqual(processingReportP4.StateID, actualProcessingReportObject.Part4List[0].StateID);
                Assert.AreEqual(processingReportP4.Tequila, actualProcessingReportObject.Part4List[0].Tequila);
                Assert.AreEqual(processingReportP4.Vodka, actualProcessingReportObject.Part4List[0].Vodka);

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
        /// This test tests workflow: Buy GNS -> Redistil -> Blend -> Bottle
        /// </summary>
        [TestMethod()]
        public void BuyGNS_RedistilOnce_Make_Gin()
        {
            // Arrange
            List<Tuple<int/*recordId*/, Table/*table enum vaue*/>> tupleL = new List<Tuple<int, Table>>();
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
                tupleL.Add(Tuple.Create(vendorId, Table.Vendor));

                // setup Storage Object
                StorageObject storage = new StorageObject();
                storage.StorageName = "testStorage";
                storage.SerialNumber = "2H29NNS";

                storageId = _dl.CreateStorage(_userId, storage);
                tupleL.Add(Tuple.Create(storageId, Table.Storage));

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
                    tupleL.Add(Tuple.Create(gnsMaterialId, Table.MaterialDict));
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

                List<StorageObject> stoL = new List<StorageObject>();
                StorageObject sto = new StorageObject();
                sto.StorageId = storageId;
                stoL.Add(sto);
                purchO.Storage = stoL;

                purchO.SpiritTypeReportingID = 8;
                purchO.Gauged = true;

                purchaseId = _dl.CreatePurchase(purchO, _userId);
                tupleL.Add(Tuple.Create(purchaseId, Table.Purchase));
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
                prodO.Storage = stoL; // we are using the same storage id as we use for Purchase to keep things simple
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
                tupleL.Add(Tuple.Create(productionId, Table.Production));

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
                prodBlend.Storage = stoL; // we are using the same storage id as we use for Purchase to keep things simple
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
                tupleL.Add(Tuple.Create(productionId, Table.Production));

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
                prodBottl.Storage = stoL; // we are using the same storage id as we use for Purchase to keep things simple
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

                // reporting time range
                DateTime start = new DateTime(2017, 09, 01);
                DateTime end = new DateTime(2017, 09, 30);

                #region Produciton
                /* PRODUCTION REPORT */
                ProdReportPart1 part1E = new ProdReportPart1();
                part1E.ProccessingAcct = 159.92f;
                part1E.ProducedTotal = 159.92f;
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
                storageReportBody.r24_Lines7Through23 = 0f;
                storageReportBody.r2_DepositedInBulkStorage = 180f;
                storageReportBody.r4_ReturnedToBulkStorage = 0f;
                storageReportBody.r6_TotalLines1Through5 = 0f;
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
                Assert.AreEqual(part1E.ProccessingAcct, actualProdReportObject.Part1[0].ProccessingAcct);
                Assert.AreEqual(part1E.ProducedTotal, actualProdReportObject.Part1[0].ProducedTotal);
                Assert.AreEqual(part1E.Recd4RedistilL17, actualProdReportObject.Part1[0].Recd4RedistilL17);
                Assert.AreEqual(part1E.Recd4RedistilaltionL15, actualProdReportObject.Part1[1].Recd4RedistilaltionL15);
                Assert.AreEqual(part1E.StorageAcct, actualProdReportObject.Part1[0].StorageAcct);
                Assert.AreEqual(part1E.SpiritCatName, actualProdReportObject.Part1[0].SpiritCatName);
                Assert.AreEqual(part1E.SpiritTypeReportingID, actualProdReportObject.Part1[0].SpiritTypeReportingID);
                Assert.AreEqual(part1E.UnfinishedSpiritsEndOfQuarterL17, actualProdReportObject.Part1[0].UnfinishedSpiritsEndOfQuarterL17);

                // verify Production report Part 2 trough 4
                Assert.AreEqual(0, actualProdReportObject.Part2Through4.Count);

                // verify part 5
                Assert.AreEqual(part5.KindofSpirits, actualProdReportObject.part5List[0].KindofSpirits);
                Assert.AreEqual(part5.Proof, actualProdReportObject.part5List[0].Proof);

                // verify Production report Part 6
                Assert.AreEqual(0, actualProdReportObject.ProdReportPart6.Count);
                //Assert.AreEqual(part6.KindOfMaterial, actualProdReportObject.ProdReportPart6[0].KindOfMaterial);
                //Assert.AreEqual(part6.ProdReportMaterialCategoryID, actualProdReportObject.ProdReportPart6[0].ProdReportMaterialCategoryID);
                //Assert.AreEqual(part6.Volume, actualProdReportObject.ProdReportPart6[0].Volume);
                //Assert.AreEqual(part6.Weight, actualProdReportObject.ProdReportPart6[0].Weight);
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
                Assert.AreEqual(processingReportP4.Gin, actualProcessingReportObject.Part4List[0].Gin);
                Assert.AreEqual(processingReportP4.ImportedWhiskeyCanadian, actualProcessingReportObject.Part4List[0].ImportedWhiskeyCanadian);
                Assert.AreEqual(processingReportP4.ImportedWhiskeyIrish, actualProcessingReportObject.Part4List[0].ImportedWhiskeyIrish);
                Assert.AreEqual(processingReportP4.ImportedWhiskeyScotch, actualProcessingReportObject.Part4List[0].ImportedWhiskeyScotch);
                Assert.AreEqual(processingReportP4.Liqueur, actualProcessingReportObject.Part4List[0].Liqueur);
                Assert.AreEqual(processingReportP4.ProcessingReportTypeName, actualProcessingReportObject.Part4List[0].ProcessingReportTypeName);
                Assert.AreEqual(processingReportP4.ProcessingSpirits, actualProcessingReportObject.Part4List[0].ProcessingSpirits);
                Assert.AreEqual(processingReportP4.ProcessingTypeID, actualProcessingReportObject.Part4List[0].ProcessingTypeID);
                Assert.AreEqual(processingReportP4.RumDomestic, actualProcessingReportObject.Part4List[0].RumDomestic);
                Assert.AreEqual(processingReportP4.RumDomestic, actualProcessingReportObject.Part4List[0].RumDomestic);
                Assert.AreEqual(processingReportP4.RumOtherImported, actualProcessingReportObject.Part4List[0].RumOtherImported);
                Assert.AreEqual(processingReportP4.RumPuertoRican, actualProcessingReportObject.Part4List[0].RumPuertoRican);
                Assert.AreEqual(processingReportP4.StateID, actualProcessingReportObject.Part4List[0].StateID);
                Assert.AreEqual(processingReportP4.Tequila, actualProcessingReportObject.Part4List[0].Tequila);
                Assert.AreEqual(processingReportP4.Vodka, actualProcessingReportObject.Part4List[0].Vodka);

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
        /// <param name="tupleL"></param>
        private void WorkflowBrandyTestSetup(ref List<Tuple<int/*recordId*/, Table/*table enum vaue*/>> tupleL)
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
            tupleL.Add(Tuple.Create(spiritId, Table.Spirit));


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

                materialDictId = _dl.CreateRawMaterial(_userId, wineMaterial);
                tupleL.Add(Tuple.Create(materialDictId, Table.MaterialDict));
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
                tupleL.Add(Tuple.Create(materialDictId, Table.MaterialDict));
            }

        }
    }
}