using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebApp.Helpers;

namespace WebApp.Controllers
{
    public class DictionaryController : Controller
    {
        private DataLayer dl = new DataLayer();

        /// <summary>
        /// Spirit ActionResult Method
        /// </summary>
        /// <returns></returns>
        public ActionResult Spirit()
        {
            return View();
        }

        /// <summary>
        /// Vendor ActionResult Method
        /// </summary>
        /// <returns></returns>
        public ActionResult Vendor()
        {
            return View();
        }

        /// <summary>
        /// Storage ActionResult Method
        /// </summary>
        /// <returns></returns>
        public ActionResult Storage()
        {
            return View();
        }

        /// <summary>
        /// RawMaterial ActionResult Method
        /// </summary>
        /// <returns></returns>
        public ActionResult Material()
        {
            return View();
        }

        /// <summary>
        /// Additive ActionResult Method
        /// </summary>
        /// <returns></returns>
        public ActionResult Additive()
        {
            return View();
        }

        /// <summary>
        /// Supply ActionResult Method
        /// </summary>
        /// <returns></returns>
        public ActionResult Supply()
        {
            return View();
        }

        #region Shared Methods
        /// <summary>
        /// GetUnitList sends list of units avaiable for measurement materials
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetUnitList ()
        {
            if(User.Identity.IsAuthenticated)
            {
                var unitList = dl.GetUnitList();
                return Json(unitList, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Unauthenticated user!");
            }
        }
        #endregion

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
                        bool returnResult = dl.DeleteDictionaryRecord(userId, deleteObject);
                        if (returnResult)
                        {
                            string message = deleteObject.DeleteRecordType + " Record was deleted successfully";
                            return Json(message);
                        }
                        else
                        {
                            string message = "Wasn't able to delete " + deleteObject.DeleteRecordType + " Record";
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
                string message = "Even though DeleteRecord method was called from the client the RecordID was unacceptable " + deleteObject.DeleteRecordID;
                return Json(message);
            }
        }

        #region Spirit Methods

        /// <summary>
        /// Insert a new Spirit record into the database
        /// </summary>
        /// <param name="spiritObject"></param>
        /// <returns>JsonResult</returns>
        [HttpPost]
        public JsonResult CreateSpirit(SpiritObject spiritObject)
        {
            if (spiritObject != null)
            {
                if (User.Identity.IsAuthenticated)
                {
                    int userId = User.Identity.GetUserId<int>();
                    if (userId > 0)
                    {
                        int returnResult = dl.CreateSpirit(userId, spiritObject);
                        if (returnResult > 0)
                        {
                            string message = "Spirit dictionary record created successfully.";
                            return Json(message);
                        }
                        else
                        {
                            string message = "Failed to create spirit dictionary record!";
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
                return Json("Back End received empty or undefined Spirit Object from the client");
            }
        }

        /// <summary>
        /// UpdateSpirit updates spirit information
        /// </summary>
        /// <param name="spiritObject"></param>
        /// <returns>JsonResult</returns>
        [HttpPost]
        public JsonResult UpdateSpirit(SpiritObject spiritObject)
        {
            if (spiritObject != null)
            {
                if (User.Identity.IsAuthenticated)
                {
                    int userId = User.Identity.GetUserId<int>();
                    if (userId > 0)
                    {
                        bool returnResult = dl.UpdateSpirit(userId, spiritObject);
                        if (returnResult)
                        {
                            string message = "Spirit dictionary record updated successfully.";
                            return Json(message);
                        }
                        else
                        {
                            string message = "Failed to update spirit dictionary record!";
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
                return Json("something went wrong when we tried to execute updateSpirit info in the db");
            }
        }

        /// <summary>
        /// GetSpiritList method is used to get list of spirits from db
        /// </summary>
        /// <returns>JsonResult</returns>
        [HttpGet]
        public JsonResult GetSpiritList()
        {
            if (User.Identity.IsAuthenticated)
            {
                int userId = User.Identity.GetUserId<int>();
                if (userId > 0)
                {
                    var spiritList = dl.GetSpiritList(userId);
                    if(spiritList != null)
                    {
                        return Json(spiritList, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("There were no items associated with this Distillery", JsonRequestBehavior.AllowGet);
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

        /// <summary>
        /// Retrieves a list of processing report types
        /// </summary>
        /// <returns>JsonResult</returns>
        [HttpGet]
        public JsonResult GetProcessingReportTypes()
        {
            if (User.Identity.IsAuthenticated)
            {
                int userId = User.Identity.GetUserId<int>();
                if (userId > 0)
                {
                    var types = dl.GetProcessingReportTypes();
                    if (types != null)
                    {
                        return Json(types, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("There were no items associated with this Distillery", JsonRequestBehavior.AllowGet);
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

        #endregion

        #region Vendor Methods
        /// <summary>
        /// Insert a new Vendor record into the database
        /// </summary>
        /// <param name="vendorObject"></param>
        /// <returns>JsonResult</returns>
        [HttpPost]
        public JsonResult CreateVendor(VendorObject vendorObject)

        {
            if (vendorObject != null)
            {
                if (User.Identity.IsAuthenticated)
                {
                    int userId = User.Identity.GetUserId<int>();
                    if (userId > 0)
                    {
                        int returnResult = dl.CreateVendor(userId, vendorObject);
                        if (returnResult > 0)
                        {
                            string message = "Vendor dictionary record created successfully.";
                            return Json(message);
                        }
                        else
                        {
                            string message = "Failed to create vendor dictionary record!";
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
                return Json("Back End received empty or undefined Vendor Object from the client");
            }
        }

        /// <summary>
        /// UpdateVendor updates vendor information
        /// </summary>
        /// <param name="vendorObject"></param>
        /// <returns>JsonResult</returns>
        [HttpPost]
        public JsonResult UpdateVendor(VendorObject vendorObject)
        {
            if (vendorObject != null)
            {
                if (User.Identity.IsAuthenticated)
                {
                    int userId = User.Identity.GetUserId<int>();
                    if (userId > 0)
                    {
                        bool returnResult = dl.UpdateVendor(userId, vendorObject);
                        if (returnResult)
                        {
                            string message = "Vendor dictionary record updated successfully.";
                            return Json(message);
                        }
                        else
                        {
                            string message = "Failed to update vendor dictionary record!";
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
                return Json("Back End received empty or undefined Vendor Object from the client");
            }
        }

        /// <summary>
        /// GetVendorList method is used to get list of vendors from db
        /// </summary>
        /// <returns>JsonResult</returns>
        [HttpGet]
        public JsonResult GetVendorList()
        {
            if (User.Identity.IsAuthenticated)
            {
                int userId = User.Identity.GetUserId<int>();
                if (userId > 0)
                {
                    var vendorList = dl.GetVendorList(userId);

                    if (vendorList != null)
                    {
                        return Json(vendorList, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("There were no items associated with this Distillery", JsonRequestBehavior.AllowGet);
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

        #endregion

        #region Storage Methods
        /// <summary>
        /// CreateStorage Method is used to create a new storage
        /// </summary>
        /// <param name="storageObject"></param>
        /// <returns>JsonResult</returns>
        [HttpPost]
        public JsonResult CreateStorage(StorageObject storageObject)
        {
            if (storageObject != null)
            {
                if (User.Identity.IsAuthenticated)
                {
                    int userId = User.Identity.GetUserId<int>();
                    if (userId > 0)
                    {
                        int returnResult = dl.CreateStorage(userId, storageObject);
                        if (returnResult > 0)
                        {
                            string message = "Storage dictionary record created successfully.";
                            return Json(message);
                        }
                        else
                        {
                            string message = "Failed to create storage dictionary record!";
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
                return Json("Back End received empty or undefined Storage Object from the client");
            }
        }

        /// <summary>
        /// UpdateStorage receives request from the client with updated storage info and forwards it to Data Layer
        /// </summary>
        /// <param name="storageObject"></param>
        /// <returns>JsonResult</returns>
        [HttpPost]
        public JsonResult UpdateStorage(StorageObject storageObject)
        {
            if (storageObject != null)
            {
                if (User.Identity.IsAuthenticated)
                {
                    int userId = User.Identity.GetUserId<int>();
                    if (userId > 0)
                    {
                        bool returnResult = dl.UpdateStorage(userId, storageObject);
                        if (returnResult)
                        {
                            string message = "Storage dictionary record updated successfully.";
                            return Json(message);
                        }
                        else
                        {
                            string message = "Failed to update storage dictionary record!";
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
                return Json("Back End received empty or undefined Storage Object from the client");
            }
        }

        /// <summary>
        /// GetStorageList receives request from the client for the list of Storages and forwards it to Data Layer
        /// </summary>
        /// <returns>JsonResult</returns>
        [HttpGet]
        public JsonResult GetStorageList()
        {
            if (User.Identity.IsAuthenticated)
            {
                int userId = User.Identity.GetUserId<int>();
                if (userId > 0)
                {
                    var storageList = dl.GetStorageList(userId);

                    if (storageList != null)
                    {
                        return Json(storageList, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("There were no items associated with this Distillery", JsonRequestBehavior.AllowGet);
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

        #endregion

        #region Raw Material Methods
        /// <summary>
        /// CreateRawMaterial receives request from the client with RawMaterial object and forwards it to Data Layer
        /// </summary>
        /// <param name="rawMObject"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CreateRawMaterial(RawMaterialObject rawMObject, bool testUserId = false)
        {
            if (rawMObject != null)
            {
                if (User.Identity.IsAuthenticated)
                {
                    int userId = User.Identity.GetUserId<int>();
                    if (userId > 0)
                    {
                        int returnResult = dl.CreateRawMaterial(userId, rawMObject);
                        if (returnResult > 0)
                        {
                            string message = "Raw Material dictionary record created successfully.";
                            return Json(message);
                        }
                        else
                        {
                            string message = "Failed to create Raw Material dictionary record!";
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
                return Json("Back End received empty or undefined Raw Material Object from the client");
            }
        }

        /// <summary>
        /// UpdateRawMaterial receives request from the client with updated rawMaterial info and forwards it to Data Layer
        /// </summary>
        /// <param name="rawMaterialObject"></param>
        /// <returns>JsonResult</returns>
        [HttpPost]
        public JsonResult UpdateRawMaterial(RawMaterialObject rawMaterialObject)
        {
            if (rawMaterialObject != null)
            {
                if (User.Identity.IsAuthenticated)
                {
                    int userId = User.Identity.GetUserId<int>();
                    if (userId > 0)
                    {
                        bool returnResult = dl.UpdateRawMaterial(userId, rawMaterialObject);
                        if (returnResult)
                        {
                            string message = "Raw Material dictionary record updated successfully.";
                            return Json(message);
                        }
                        else
                        {
                            string message = "Failed to update Raw Material dictionary record!";
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
                return Json("Back End received empty or undefined RawMaterial Object from the client");
            }
        }

        /// <summary>
        /// GetRawMaterialList receives request from the client for the list of RawMaterials and forwards it to Data Layer
        /// </summary>
        /// <returns>JsonResult</returns>
        [HttpGet]
        public JsonResult GetRawMaterialList()
        {
            if (User.Identity.IsAuthenticated)
            {
                int userId = User.Identity.GetUserId<int>();
                if (userId > 0)
                {
                    var rawMaterialList = dl.GetRawMaterialListDict(userId);

                    if (rawMaterialList != null)
                    {
                        return Json(rawMaterialList, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("There were no items associated with this Distillery", JsonRequestBehavior.AllowGet);
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

        /// <summary>
        /// GetMaterialCategory method returns list of Material Categories to be chosen in Fermentable and Fermented workflows
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetMaterialCategory()
        {
            if (User.Identity.IsAuthenticated)
            {
                var materialCategoryList = dl.GetMaterialCategoryList();

                if(materialCategoryList != null)
                {
                    return Json(materialCategoryList, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("There were no items associated with this Distillery", JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json("Unauthenticated user!");
            }
        }

        #endregion
    }
}