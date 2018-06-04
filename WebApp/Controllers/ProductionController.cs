using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebApp.Models;
using WebApp.Helpers;
using Microsoft.AspNet.Identity;
using WebApp.Workflows;

namespace WebApp.Controllers
{
    public class ProductionController : Controller
    {
        private readonly DistilDBContext _db;
        private readonly DataLayer _dl;
        private readonly ProductionWorkflow _production;

        public ProductionController()
        {
            _db = new DistilDBContext();
            _dl = new DataLayer(_db);
            _production = new ProductionWorkflow(_db, _dl);
        }

        // --- Production Methods for Fermenting and Distilling ---

        #region Shared

        /// <summary>
        /// CreatePurchaseRecord method receives purchase object from the client and provides relevant
        /// information to the methods in the backend
        /// </summary>
        /// <param name="prodObject"></param>
        /// <returns>JsonResult</returns>
        [HttpPost]
        public JsonResult CreateProductionRecord(ProductionObject prodObject)
        {
            if (prodObject != null)
            {
                if (User.Identity.IsAuthenticated)
                {
                    var userId = User.Identity.GetUserId<int>();
                    if (userId > 0)
                    {
                        int returnResult = _production.CreateProduction(prodObject, userId);
                        if (returnResult > 0)
                        {
                            string message = "Production record created successfully.";
                            return Json(message);
                        }
                        else
                        {
                            string message = "Failed to create production record!";
                            return Json(message);
                        }
                    }
                    else
                        return Json("Unable to find UserId!");
                }
                else
                    return Json("Unauthenticated user!");
            }
            else
            {
                return Json("Backend received empty, undefined, or null Object from the client!");
            }
        }

        /// <summary>
        /// GetProductionData method that is used in Production View workflows
        /// </summary>
        /// <param name="prodType"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetProductionData(string prodType)
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.Identity.GetUserId<int>();
                if (userId > 0)
                {
                    var prodList = _production.GetProductionList(userId, prodType);
                    return Json(prodList, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Unable to find UserId!");
                }
            }
            else
                return Json("Unauthenticated user!");
        }

        /// <summary>
        /// GetBlendingData method used in Bottling workflow grabs ProductionObject and reduces it 
        /// to fewer fields so Blending data can be shown in a more compact way
        /// so it can be displayed in the list
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetBlendingData(string prodType)
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.Identity.GetUserId<int>();
                if (userId > 0)
                {
                    List<ProdObjectConcise> blendingList = new List<ProdObjectConcise>();
                    blendingList = _production.GetBlendingList(prodType, userId);
                    return Json(blendingList, JsonRequestBehavior.AllowGet);
                }
                else
                    return Json("Unable to find userId!");
            }
            else
                return Json("Unauthenticated user!");
        }

        /// <summary>
        /// GetRawMaterialList method used in Fermentation prod workflow 
        /// to display list of avaiable materials
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetRawMaterialList()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.Identity.GetUserId<int>();
                if (userId > 0)
                {
                    var rawMaterialList = _dl.GetRawMaterialList4Fermentation(userId);
                    return Json(rawMaterialList, JsonRequestBehavior.AllowGet);
                }
                else
                    return Json("Unable to find userId!");
            }
            else
                return Json("Unauthenticated user!");
        }

        /// <summary>
        /// GetRawMaterialListForDistillation is used in Distillation and Blending Production
        /// </summary>
        /// <param name="productionType">parameter indicating method from which particular distil was made{from fermented or from another distill}</param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetMaterialListForProduction(string productionType)
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.Identity.GetUserId<int>();
                if (userId > 0)
                {
                    var rawMaterialList = _dl.GetMaterialListForProduction(productionType, userId);
                    return Json(rawMaterialList, JsonRequestBehavior.AllowGet);
                }
                else
                    return Json("Unable to find UserId!");
            }
            else
                return Json("Unauthenticated user!");
        }

        /// <summary>
        /// GetAdditivesList gets a list of additives
        /// Usage: {Blending workflow}
        /// </summary>
        /// <param name="prodType"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetAdditivesList(string matType)
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.Identity.GetUserId<int>();
                if (userId > 0)
                {
                    // get the list
                    var additiveList = _dl.GetAdditivesListForProduction(matType, userId);
                    return Json(additiveList, JsonRequestBehavior.AllowGet);
                }
                else {
                }
                    return Json("Unable to find userId!");
            }
            else
            {
                return Json("Unauthenticated user!");
            }
        }

        /// <summary>
        /// Get storage data for site's dropdowns
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetStorageData()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.Identity.GetUserId<int>();
                if (userId > 0)
                {
                    var storageList = _dl.GetStorageData(userId);
                    return Json(storageList, JsonRequestBehavior.AllowGet);
                }
                else
                    return Json("Unable to find UserId!");
            }
            else
                return Json("Unauthenticated user!");
        }

        /// <summary>
        /// GetSpiritTypes method get spirit types from backend
        /// and forwards to front end
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetSpiritTypes()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.Identity.GetUserId<int>();
                if (userId > 0)
                {
                    var storageList = _dl.GetSpiritTypeList(userId);
                    return Json(storageList, JsonRequestBehavior.AllowGet);
                }
                else
                    return Json("Unable to find UserId!");
            }
            else
                return Json("Unauthenticated user!");
        }

        /// <summary>
        /// Get spirit cut values for data binding in inputs
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetSpiritCutData()
        {
            var spiritCutList = _dl.GetSpiritCutData();
            return Json(spiritCutList, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateProduction(ProductionObject pObj)
        {
            if (pObj != null)
            {
                if (User.Identity.IsAuthenticated)
                {
                    var userId = User.Identity.GetUserId<int>();
                    if (userId > 0)
                    {
                        bool returnResult = _production.UpdateProduction(pObj, userId);
                        if (returnResult)
                        {
                            string message = "Production record updated successfully.";
                            return Json(message);
                        }
                        else
                        {
                            string message = "Failed to update production record!";
                            return Json(message);
                        }
                    }
                    else
                        return Json("Unable to find UserId!");
                }
                else
                    return Json("Unauthenticated user!");
            }
            else
            {
                return Json("Back End received empty or undefined or null Object from the client");
            }
        }

        /// <summary>
        /// Generic method for deleting a record from the database.
        /// </summary>
        /// <param name="deleteRecordObject"></param>
        /// <returns>JsonResult</returns>
        [HttpPost]
        public JsonResult DeleteRecord(DeleteRecordObject deleteObject)
        {
            if (deleteObject.DeleteRecordID >= 0 && deleteObject != null)
            {
                if (User.Identity.IsAuthenticated)
                {
                    int userId = User.Identity.GetUserId<int>();
                    if (userId > 0)
                    {
                        ReturnObject returnResult = _production.DeleteProductionRecord(userId, deleteObject);
                        if (returnResult.ExecuteResult)
                        {
                            string message = deleteObject.DeleteRecordType + " Record was deleted successfully";
                            return Json(message);
                        }
                        else
                        {
                            string message = "Wasn't able to delete " + deleteObject.DeleteRecordType + " Record because it's associated with " + returnResult.ExecuteMessage;
                            return Json(message);
                        }
                    }
                    else
                    {
                        return Json("Unable to find UserId!");
                    }
                }
                else
                {
                    return Json("Unauthenticated user!");
                }
            }
            else
            {
                string message = "ID " + deleteObject.DeleteRecordID + " of the record being deleted is not valid or Production Object being deleted is null";
                return Json(message);
            }
        }
        #endregion

        #region Fermentation
        // GET: Fermentation
        public ActionResult Fermentation()
        {
            return View();
        }

        #endregion

        #region Distillation
        // GET: Distillation
        public ActionResult Distillation()
        {
            return View();
        }

        /// <summary>
        /// Get reporting spirit and kinds values for data binding in inputs
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetSpiritToKindListData()
        {
            var spiritToKindList = _dl.GetSpiritToKindListData();
            return Json(spiritToKindList, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Blending
        //GET: Blending
        public ActionResult Blending()
        {
            return View();
        }

        #endregion

        #region Bottling
        //GET: Bottling
        public ActionResult Bottling()
        {
            return View();
        }

        #endregion
    }
}