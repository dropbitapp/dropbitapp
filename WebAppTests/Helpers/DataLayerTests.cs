using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Models;

namespace WebApp.Helpers.Tests
{
    [TestClass()]
    public class DataLayerTests
    {
        DataLayer dLayer = new DataLayer();
        DistilDBContext db = new DistilDBContext();

        [TestMethod()]
        public void GetUnitListTest()
        {
            // Arrange

            // Act
            List<UnitObject> result = dLayer.GetUnitList();

            // Assert
            Assert.IsNotNull(result, "GetUnitList result returned is null");
        }

        [TestMethod()]
        public void GetMaterialCategoryList()
        {
            // Arrange

            // Act
            List<MaterialCategory> result = dLayer.GetMaterialCategoryList();

            // Assert
            Assert.IsNotNull(result, "GetUnitList result returned is null");
        }

        [TestMethod()]
        public void GetRawMaterialList4FermentationTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetMaterialListForProductionTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetAdditivesListForProductionTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetRawMaterialListForPurchaseTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetRawMaterialListDictTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetSpiritTypeListTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetUnitListTest1()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetVendorDataTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetStorageDataTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetSpiritCutDataTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetReportingSpiritTypesTest()
        {
            // Arrange

            // Act
            List<SpiritToKindListObject> result = dLayer.GetReportingSpiritTypes();

            // Assert
            Assert.IsNotNull(result, "GetReportingSpiritTypesTest result returned is null");
        }

        [TestMethod()]
        public void CreateSpiritTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void CreateVendorTest()
        {
            Assert.Fail();
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
            bool result = dLayer.CreateStorage(1, storage);

            var rec =
                (
                    from res in db.Storage
                    where res.Note == storage.Note &&
                          res.Name == storage.StorageName &&
                          res.SerialNumber == storage.SerialNumber &&
                          res.Capacity == storage.Capacity
                    select res).FirstOrDefault();

            var queryRes = rec;

            db.Storage.Remove(rec);
            db.SaveChanges();

            // Assert
            Assert.IsTrue(result);
            Assert.IsNotNull(queryRes);
        }

        [TestMethod()]
        public void CreateRawMaterialTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void UpdateSpiritTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void UpdateVendorTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void UpdateStorageTest()
        {
            // Arrange
            int distillerId = dLayer.GetDistillerId(1);

            Storage storage = new Storage()
            {
                Name = "TestStorage",
                Capacity = 500,
                SerialNumber = "123",
                Note = "TestNote",
                DistillerID = distillerId
            };

            db.Storage.Add(storage);
            db.SaveChanges();

            StorageObject storageObject = new StorageObject();
            storageObject.StorageId = storage.StorageID;
            storageObject.Capacity = storage.Capacity;
            storageObject.SerialNumber = storage.SerialNumber;
            storageObject.Note = storage.Note;
            storageObject.StorageName = storage.Name;

            // Act
            bool result = dLayer.UpdateStorage(1, storageObject);

            var rec =
                (
                    from res in db.Storage
                    where res.StorageID == storage.StorageID &&
                          res.Name == storage.Name &&
                          res.DistillerID == storage.DistillerID &&
                          res.Capacity == storage.Capacity &&
                          res.Note == storage.Note &&
                          res.SerialNumber == storage.SerialNumber
                    select res).FirstOrDefault();

            var queryRes = rec;

            db.Storage.Remove(rec);
            db.SaveChanges();

            // Assert
            Assert.IsTrue(result);
            Assert.IsNotNull(queryRes);
        }

        [TestMethod()]
        public void UpdateRawMaterialTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetSpiritListTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetVendorListTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetStorageListTest()
        {
            // Arrange

            // Act
            List<StorageObject> result = dLayer.GetStorageList(1);

            // Assert
            Assert.IsNotNull(result, "GetStorageListTest result returned is null");
        }

        [TestMethod()]
        public void CreatePurchaseTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetPurchasesListTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void CreateProductionTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetProductionListTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetSpiritToKindListDataTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void DestroyBatchTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetPurchaseDataForDestructionTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetProductionDataForDestructionTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetDestroyedBatchesTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetMaterialCategoryListTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetStorageReportDataTest()
        {

            // Arrange

            // Act
            //StorageReport result = dLayer.GetStorageReportData();

            // Assert
            //Assert.IsNotNull(result, "GetUnitList result returned is null");
        }

        [TestMethod()]
        public void GetUnfinishedSpiritsForProductionReportTest()
        {
            Assert.Fail();
        }
    }
}