using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebApp.Models;
using WebApp.Helpers;
using System.Web.Security;
using Microsoft.AspNet.Identity;
using System;

namespace WebApp.Controllers
{
    public class PurchaseController : Controller
    {
        private DataLayer dl = new DataLayer();

        #region Shared Methods

        /// <summary>
        /// Get reporting spirit and kinds values for data binding in inputs
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetReportingSpiritTypes()
        {
            var spiritTypeList = dl.GetReportingSpiritTypes();
            return Json(spiritTypeList, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// CreatePurchaseRecord method receives purchase object from the client and provides relevant
        /// information to the methods in the backend
        /// </summary>
        /// <param name="purchaseObject"></param>
        /// <returns>JsonResult</returns>
        [HttpPost]
        public JsonResult CreatePurchaseRecord(PurchaseObject purchaseObject)
        {
            if (purchaseObject != null)
            {
                if (User.Identity.IsAuthenticated)
                {
                    int userId = User.Identity.GetUserId<int>();
                    if (userId > 0)
                    {
                        var returnResult = dl.CreatePurchase(purchaseObject, userId);
                        if (returnResult > 0)
                        {
                            string message = "Purchase record created successfully.";
                            return Json(message);
                        }
                        else
                        {
                            string message = "Failed to create purchase record!";
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
                return Json("Backend received empty, undefined, or null object from the client!");
            }
        }

        /// <summary>
        /// GetListOfPurchases get the list of purchase records and sends them to front end.
        /// method only sends data that pertains to particular purchase type that is sent from 
        /// front end as a parameter
        /// </summary>
        /// <param name="purchaseType"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetListOfPurchases(string purchaseType)
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.Identity.GetUserId<int>();
                if (userId > 0)
                {
                    var recsList = dl.GetPurchasesList(purchaseType, userId);
                    return Json(recsList, JsonRequestBehavior.AllowGet);
                }
                else
                    return Json("Unable to find UserId!");
            }
            else
                return Json("Unauthenticated user!");
        }

        /// <summary>
        /// GetRawMaterialList method queries for the list of raw materials and filters them based on purchase type(Fermentable, Distilled, Fermented, Additive, etc)
        /// </summary>
        /// <param name="purType">this parameter indicates whether this is a fermentable vs distilled vs etc purchase</param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetRawMaterialList(string purchaseType)
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.Identity.GetUserId<int>();
                if (userId > 0)
                {
                    // get the list
                    int identifierId = (int)TypeIdentifier.RawMaterial;
                    var rawMaterialList = dl.GetRawMaterialListForPurchase(identifierId, userId, purchaseType);
                    //filter the list for raw materials, relevant to this workflow
                    return Json(rawMaterialList, JsonRequestBehavior.AllowGet);
                }
                else
                    return Json("Unable to find UserId!");
            }
            else
                return Json("Unauthenticated user!");
        }

        /// <summary>
        /// Get list of Vendors for site's dropdowns in purchase workflows
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetVendorData()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.Identity.GetUserId<int>();
                if (userId > 0)
                {
                    var vendorList = dl.GetVendorData(userId);
                    return Json(vendorList, JsonRequestBehavior.AllowGet);
                }
                else
                    return Json("Unable to find UserId!");
            }
            else
                return Json("Unauthenticated user!");
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
        #endregion

        #region Fermentable
        // GET: Fermentable Purchases
        [HttpGet]
        public ActionResult Fermentable()
        {
            return View();
        }

        [HttpPost]
        public JsonResult UpdateFermentable(PurchaseObject purchaseObject)
        {
            if (purchaseObject != null)
            {
                if (User.Identity.IsAuthenticated)
                {
                    var userId = User.Identity.GetUserId<int>();
                    if (userId > 0)
                    {
                        bool returnResult = dl.UpdatePurchase(purchaseObject, userId);
                        if (returnResult)
                        {
                            string message = "Fermentable record updated successfully.";
                            return Json(message);
                        }
                        else
                        {
                            string message = "Failed to update fermentable record!";
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
                return Json("Backend received empty, undefined, or null object from the client!");
            }
        }

        [HttpPost]
        public JsonResult DeleteFermentable(PurchaseObject purchaseObject)
        {
            if (purchaseObject.PurchaseId >= 0)
            {
                if (User.Identity.IsAuthenticated)
                {
                    var userId = User.Identity.GetUserId<int>();
                    if (userId > 0)
                    {
                        bool returnResult = dl.DeletePurchase(purchaseObject, userId);
                        if (returnResult)
                        {
                            string message = "Fermentable record deleted successfully.";
                            return Json(message);
                        }
                        else
                        {
                            string message = "Failed to delete fermentable record!";
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
                string message = "Invalid PurchaseId received from the front-end!" + purchaseObject.PurchaseId;
                return Json(message);
            }
        }
        #endregion

        #region Fermented

        // GET: Fermented Purchases
        [HttpGet]
        public ActionResult Fermented()
        {
            return View();
        }

        [HttpPost]
        public JsonResult UpdateFermented (PurchaseObject purchaseObject)
        {
            if (purchaseObject != null)
            {
                if (User.Identity.IsAuthenticated)
                {
                    var userId = User.Identity.GetUserId<int>();
                    if (userId > 0)
                    {
                        bool returnResult = dl.UpdatePurchase(purchaseObject, userId);
                        if (returnResult)
                        {
                            string message = "Fermented record updated successfully.";
                            return Json(message);
                        }
                        else
                        {
                            string message = "Failed to update fermented record!";
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
                return Json("Backend received empty, undefined, or null object from the client!");
            }
        }

        [HttpPost]
        public JsonResult DeleteFermented(PurchaseObject purchaseObject)
        {
            if (purchaseObject.PurchaseId >= 0)
            {
                if (User.Identity.IsAuthenticated)
                {
                    var userId = User.Identity.GetUserId<int>();
                    if (userId > 0)
                    {
                        bool returnResult = dl.DeletePurchase(purchaseObject, userId);
                        if (returnResult)
                        {
                            string message = "Fermented record deleted successfully.";
                            return Json(message);
                        }
                        else
                        {
                            string message = "Failed to delete fermented record!";
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
                string message = "Invalid PurchaseId received from the front-end!" + purchaseObject.PurchaseId;
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

        #region Distilled
        // GET: Distilled Purchases
        [HttpGet]
        public ActionResult Distilled()
        {
            return View();
        }

        [HttpPost]
        public JsonResult UpdateDistilled(PurchaseObject purchaseObject)
        {
            if (purchaseObject != null)
            {
                if (User.Identity.IsAuthenticated)
                {
                    var userId = User.Identity.GetUserId<int>();
                    if (userId > 0)
                    {
                        bool returnResult = dl.UpdatePurchase(purchaseObject, userId);
                        if (returnResult)
                        {
                            string message = "Distilled record updated successfully.";
                            return Json(message);
                        }
                        else
                        {
                            string message = "Failed to update distilled record!";
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

        [HttpPost]
        public JsonResult DeleteDistilled(PurchaseObject purchaseObject)
        {
            if (purchaseObject.PurchaseId >= 0)
            {
                if (User.Identity.IsAuthenticated)
                {
                    var userId = User.Identity.GetUserId<int>();
                    if (userId > 0)
                    {
                        bool returnResult = dl.DeletePurchase(purchaseObject, userId);
                        if (returnResult)
                        {
                            string message = "Distilled record deleted successfully.";
                            return Json(message);
                        }
                        else
                        {
                            string message = "Failed to delete distilled record!";
                            return Json(message);
                        }
                    }
                    else
                        return Json("Unable to find userId!");
                }
                else
                    return Json("Unauthenticated user!");
            }
            else
            {
                string message = "Invalid PurchaseId received from the front-end!" + purchaseObject.PurchaseId;
                return Json(message);
            }
        }
        #endregion // end of Distilled controller methods

        #region Supplies
        // GET: Supplies Purchases
        [HttpGet]
        public ActionResult Supplies()
        {
            return View();
        }

        [HttpPost]
        public JsonResult UpdateSupply(PurchaseObject purchaseObject)
        {
            if (purchaseObject != null)
            {
                if (User.Identity.IsAuthenticated)
                {
                    var userId = User.Identity.GetUserId<int>();
                    if (userId > 0)
                    {
                        bool returnResult = dl.UpdatePurchase(purchaseObject, userId);
                        if (returnResult)
                        {
                            string message = "Supply record updated successfully.";
                            return Json(message);
                        }
                        else
                        {
                            string message = "Failed to update supply record!";
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
                return Json("Backend received empty, undefined, or null object from the client!");
            }
        }

        [HttpPost]
        public JsonResult DeleteSupply(PurchaseObject purchaseObject)
        {
            if (purchaseObject.PurchaseId >= 0)
            {
                if (User.Identity.IsAuthenticated)
                {
                    var userId = User.Identity.GetUserId<int>();
                    if (userId > 0)
                    {
                        bool returnResult = dl.DeletePurchase(purchaseObject, userId);
                        if (returnResult)
                        {
                            string message = "Supply record deleted successfully.";
                            return Json(message);
                        }
                        else
                        {
                            string message = "Failed to delete supply record!";
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
                string message = "Invalid PurchaseId received from the front-end!" + purchaseObject.PurchaseId;
                return Json(message);
            }
        }
        #endregion

        #region Additives
        // GET: Additive Purchases
        [HttpGet]
        public ActionResult Additives()
        {
            return View();
        }

        [HttpPost]
        public JsonResult UpdateAdditives(PurchaseObject purchaseObject)
        {
            if (purchaseObject != null)
            {
                if (User.Identity.IsAuthenticated)
                {
                    var userId = User.Identity.GetUserId<int>();
                    if (userId > 0)
                    {
                        bool returnResult = dl.UpdatePurchase(purchaseObject, userId);
                        if (returnResult)
                        {
                            string message = "Additive record updated successfully.";
                            return Json(message);
                        }
                        else
                        {
                            string message = "Failed to update additive record!";
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
                return Json("Backend received empty, undefined, or null object from the client!");
            }
        }

        [HttpPost]
        public JsonResult DeleteAdditive(PurchaseObject purchaseObject)
        {
            if (purchaseObject.PurchaseId >= 0)
            {
                if (User.Identity.IsAuthenticated)
                {
                    var userId = User.Identity.GetUserId<int>();
                    if (userId > 0)
                    {
                        bool returnResult = dl.DeletePurchase(purchaseObject, userId);
                        if (returnResult)
                        {
                            string message = "Additive record deleted successfully.";
                            return Json(message);
                        }
                        else
                        {
                            string message = "Failed to delete additive record!";
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
                string message = "Invalid PurchaseId received from the front-end!" + purchaseObject.PurchaseId;
                return Json(message);
            }
        }
        #endregion
    }
}