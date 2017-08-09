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
            Assert.IsNotNull(result, "GetUnitList result returned is null");
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
            Assert.Fail();
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
            Assert.Fail();
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
            Assert.Fail();
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
            Assert.Fail();
        }
    }
}