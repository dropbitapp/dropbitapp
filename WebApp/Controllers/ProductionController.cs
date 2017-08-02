using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebApp.Models;
using WebApp.Helpers;
using Microsoft.AspNet.Identity;

namespace WebApp.Controllers
{
    public class ProductionController : Controller
    {
        private DataLayer dl = new DataLayer();

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
                        bool returnResult = dl.CreateProduction(prodObject, userId);
                        if (returnResult)
                        {
                            string message = "Production record created sucessfully.";
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
                    var prodList = dl.GetProductionList(userId, prodType);
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
                    blendingList = dl.GetBlendingList(prodType, userId);
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
                    var rawMaterialList = dl.GetRawMaterialList4Fermentation(userId);
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
                    var rawMaterialList = dl.GetMaterialListForProduction(productionType, userId);
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
                    var additiveList = dl.GetAdditivesListForProduction(matType, userId);
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
                    var storageList = dl.GetStorageData(userId);
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
                    var storageList = dl.GetSpiritTypeList(userId);
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
            var spiritCutList = dl.GetSpiritCutData();
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
                        bool returnResult = dl.UpdateProduction(pObj, userId);
                        if (returnResult)
                        {
                            string message = "Production record updated sucessfully.";
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
        #endregion

        #region Fermentation
        // GET: Fermentation
        public ActionResult Fermentation()
        {
            return View();
        }

        [HttpPost]
        public JsonResult DeleteFermentation(ProductionObject productionObject)
        {
            if (productionObject.ProductionId >= 0)
            {
                if (User.Identity.IsAuthenticated)
                {
                    var userId = User.Identity.GetUserId<int>();
                    if (userId > 0)
                    {
                        bool returnResult = dl.DeleteProduction(productionObject, userId);
                        if (returnResult)
                        {
                            string message = "Fermentation record deleted sucessfully.";
                            return Json(message);
                        }
                        else
                        {
                            string message = "Failed to delete fermentation record!";
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
                string message = "Even though DeleteFermentation method was called from the client but ProductionId was unacceptable " + productionObject.ProductionId;
                return Json(message);
            }
        }

        #endregion

        #region Distillation
        // GET: Distillation
        public ActionResult Distillation()
        {
            return View();
        }

        [HttpPost]
        public JsonResult DeleteDistillation(ProductionObject productionObject)
        {
            if (productionObject.ProductionId >= 0)
            {
                if (User.Identity.IsAuthenticated)
                {
                    var userId = User.Identity.GetUserId<int>();
                    if (userId > 0)
                    {
                        bool returnResult = dl.DeleteProduction(productionObject, userId);
                        if (returnResult)
                        {
                            string message = "Distillation record deleted sucessfully.";
                            return Json(message);
                        }
                        else
                        {
                            string message = "Failed to delete Distillation record!";
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
                string message = "Even though DeleteDistillation method was called from the client but ProductionId was unacceptable " + productionObject.ProductionId;
                return Json(message);
            }
        }

        /// <summary>
        /// Get reporting spirit and kinds values for data binding in inputs
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetSpiritToKindListData()
        {
            var spiritToKindList = dl.GetSpiritToKindListData();
            return Json(spiritToKindList, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Blending
        //GET: Blending
        public ActionResult Blending()
        {
            return View();
        }

        [HttpPost]
        public JsonResult DeleteBlending(ProductionObject productionObject)
        {
            if (productionObject.ProductionId >= 0)
            {
                if (User.Identity.IsAuthenticated)
                {
                    var userId = User.Identity.GetUserId<int>();
                    if (userId > 0)
                    {
                        bool returnResult = dl.DeleteProduction(productionObject, userId);
                        if (returnResult)
                        {
                            string message = "Blending record deleted sucessfully.";
                            return Json(message);
                        }
                        else
                        {
                            string message = "Failed to delete blending record!";
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
                string message = "Even though DeleteBlending method was called from the client but ProductionId was unacceptable " + productionObject.ProductionId;
                return Json(message);
            }
        }

        #endregion

        #region Bottling
        //GET: Bottling
        public ActionResult Bottling()
        {
            return View();
        }

        [HttpPost]
        public JsonResult DeleteBottling(ProductionObject productionObject)
        {
            if (productionObject.ProductionId >= 0)
            {
                if (User.Identity.IsAuthenticated)
                {
                    var userId = User.Identity.GetUserId<int>();
                    if (userId > 0)
                    {
                        bool returnResult = dl.DeleteProduction(productionObject, userId);
                        if (returnResult)
                        {
                            string message = "Bottling record deleted sucessfully.";
                            return Json(message);
                        }
                        else
                        {
                            string message = "Failed to delete bottling record!";
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
                string message = "Even though DeleteBottling method was called from the client but ProductionId was unacceptable " + productionObject.ProductionId;
                return Json(message);
            }
        }

        #endregion
    }
}