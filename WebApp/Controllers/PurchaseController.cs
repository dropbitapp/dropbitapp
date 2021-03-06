﻿using System.Web.Mvc;
using WebApp.Models;
using WebApp.Helpers;
using Microsoft.AspNet.Identity;
using WebApp.Workflows;
using NLog;
using System;
using System.Web.Script.Serialization;

namespace WebApp.Controllers
{
    public class PurchaseController : Controller
    {
        private readonly DistilDBContext _db;
        private readonly DataLayer _dl;
        private readonly PurchaseWorkflow _purchase;
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public PurchaseController()
        {
            _db = new DistilDBContext();
            _dl = new DataLayer(_db);
            _purchase = new PurchaseWorkflow(_db, _dl);
        }

        #region Shared Methods

        /// <summary>
        /// Get reporting spirit and kinds values for data binding in inputs
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetReportingSpiritTypes()
        {
            var spiritTypeList = _dl.GetReportingSpiritTypes();
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
                        try
                        {
                            _logger.Info("{0} {1}\n\tPurchase Date: {2}\n\tCreatePurchase: {3}", User.Identity.Name, Request.Url.ToString(), purchaseObject.PurchaseDate, new JavaScriptSerializer().Serialize(purchaseObject));
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "Logging error");
                        }
                        var returnResult = _purchase.CreatePurchase(purchaseObject, userId);
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
                    var recsList = _purchase.GetPurchasesList(purchaseType, userId);
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
                    var rawMaterialList = _purchase.GetRawMaterialListForPurchase(userId, purchaseType);

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
                    var vendorList = _dl.GetVendorData(userId);
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
                        ReturnObject returnResult = _purchase.DeletePurchaseRecord(userId, deleteObject);
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
                string message = "ID " + deleteObject.DeleteRecordID + " of the record being deleted is not valid or Purchase Object being deleted is null";
                return Json(message);
            }
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
                        bool returnResult = _purchase.UpdatePurchase(purchaseObject, userId);
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
                        bool returnResult = _purchase.UpdatePurchase(purchaseObject, userId);
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
                        bool returnResult = _purchase.UpdatePurchase(purchaseObject, userId);
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
                        bool returnResult = _purchase.UpdatePurchase(purchaseObject, userId);
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
                        bool returnResult = _purchase.UpdatePurchase(purchaseObject, userId);
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

        #endregion
    }
}