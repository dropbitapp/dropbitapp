using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApp.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Models;
using WebApp.Helpers;

namespace WebApp.Controllers.Tests
{
    [TestClass()]
    public class ProductionControllerTests
    {
        DataLayer dLayer = new DataLayer();
        DistilDBContext db = new DistilDBContext();

        [TestMethod()]
        public void Create_Ungauged_Production_Distillation_Record_Test()
        {
            // Arrange
            ProductionObject prodObj = new ProductionObject();
            //prodObj.

            //Act

            // Assert
            //Assert.IsTrue(result);
            //Assert.AreEqual(oldProof, result, 0.1F);
        }

        [TestMethod()]
        public void Create_RawMaterial_Purchase_Record_Test()
        {
            /*
            Purchase Raw Material: Grape
            Batch Name: Any
            Purchase Date: 08 / 05 / 2017
            Volume: 0
            Weight: 1000 lbs
            Price: $350
            Vendor: Any
            Storage: Any
            Notes: Any / None
            */
            // Arrange
            PurchaseObject purObj = new PurchaseObject();
            Assert.Fail();
        }

        [TestMethod()]
        public void Create_Grape_RawMaterial_Record_Test()
        {
            /*Create Grape RawMaterial Dictionary item
                MaterialCategoryID: 2
                Note: null
                PurchaseMaterialTypes: 
                    Additive: false
                    Distilled: false
                    Fermentable: true
                    Fermented: false
                    Supply: false
                RawMaterialId: 0
                RawMaterialName: "Grape"
                UnitType: "lb"
                UnitTypeId: 2
             */

            // Arrange
            RawMaterialObject matObj = new RawMaterialObject();
            PurchaseMaterialBooleanTypes PurchaseMaterialTypes = new PurchaseMaterialBooleanTypes();
            matObj.MaterialCategoryID = 2;
            matObj.Note = null;
            PurchaseMaterialTypes.Additive = false;
            PurchaseMaterialTypes.Distilled = false;
            PurchaseMaterialTypes.Fermentable = true;
            PurchaseMaterialTypes.Fermented = false;
            PurchaseMaterialTypes.Supply = false;
            matObj.PurchaseMaterialTypes = PurchaseMaterialTypes;
            matObj.RawMaterialId = 0;
            matObj.RawMaterialName = "Grape";
            matObj.UnitType = "lb";
            matObj.UnitTypeId = 2;

            var dictController = new WebApp.Controllers.DictionaryController();
            var result = dictController.CreateRawMaterial(matObj);
            Assert.Fail();
        }
    }
}