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
            // Arrange
            Guid name = Guid.NewGuid();
            Guid note = Guid.NewGuid();
            SpiritObject spirit = new SpiritObject();
            spirit.SpiritName = name.ToString();
            spirit.Note = note.ToString();

            //Act
            bool result = dLayer.CreateSpirit(1, spirit);

            var rec =
                (from res in db.Spirit
                 where res.Name == spirit.SpiritName &&
                       res.Note == spirit.Note
                 select res).FirstOrDefault();

            var queryRes = rec;

            db.Spirit.Remove(rec);
            db.SaveChanges();

            // Assert
            Assert.IsTrue(result);
            Assert.IsNotNull(queryRes);
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
        public void UpdateProofTest()
        {
            // Arrange
            float newProof = 20.0F;
            float oldProof = 10.0F;

            Proof proof = new Proof()
            {
                Value = 10.0F
            };

            db.Proof.Add(proof);
            db.SaveChanges();

            // Act
            float result = dLayer.UpdateProof(proof.ProofID, newProof);
            var proofRec =
                    (from res in db.Proof
                     where res.ProofID == proof.ProofID &&
                           res.Value == newProof
                     select res).FirstOrDefault();

            db.Proof.Remove(proofRec);
            db.SaveChanges();

            // Assert
            Assert.AreEqual(oldProof, result, 0.1F);
            Assert.IsNotNull(proofRec);
        }

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
        public void GetStorageListTest()
        {
            // Arrange

            // Act
            List<StorageObject> result = dLayer.GetStorageList(1);

            // Assert
            Assert.IsNotNull(result, "GetStorageListTest result returned is null");
        }
    }
}