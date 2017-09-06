using Microsoft.AspNet.Identity;
using System;
using System.Web.Mvc;
using WebApp.Helpers;

namespace WebApp.Controllers
{
    public class DestructionController : Controller
    {
        private DataLayer dl = new DataLayer();

        // GET: Destruction
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Returns produced batches available for destruction.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetProductionData(string prodType)
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.Identity.GetUserId<int>();
                if (userId > 0)
                {
                    var prodList = dl.GetProductionDataForDestruction(userId);
                    return Json(prodList, JsonRequestBehavior.AllowGet);
                }
                else
                    return Json("Unable to find UserId!");
            }
            else
                return Json("Unauthenticated user!");
        }

        /// <summary>
        /// Returns purchased batches available for destruction.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetPurchaseData()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.Identity.GetUserId<int>();
                if (userId > 0)
                {
                    var recsList = dl.GetPurchaseDataForDestruction(userId);
                    return Json(recsList, JsonRequestBehavior.AllowGet);
                }
                else
                    return Json("Unable to find UserId!");
            }
            else
                return Json("Unauthenticated user!");
        }

        /// <summary>
        /// Returns destroyed purchased batches
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetDestroyedBatches()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.Identity.GetUserId<int>();
                if (userId > 0)
                {
                    var recsList = dl.GetDestroyedBatches(userId);
                    return Json(recsList, JsonRequestBehavior.AllowGet);
                }
                else
                    return Json("Unable to find UserId!");
            }
            else
                return Json("Unauthenticated user!");
        }

        /// <summary>
        /// CreatePurchaseRecord method receives purchase object from the client and provides relevant
        /// information to the methods in the backend
        /// </summary>
        /// <param name="destructionObject"></param>
        /// <returns>JsonResult</returns>
        [HttpPost]
        public JsonResult DestroyBatch(DestructionObject destructionObject)
        {
            if (destructionObject != null)
            {
                if (User.Identity.IsAuthenticated)
                {
                    var userId = User.Identity.GetUserId<int>();
                    if (userId > 0)
                    {
                        bool returnResult = dl.DestroyBatch(destructionObject, userId);
                        if (returnResult)
                        {
                            string message = "Batch sucessfully destroyed.";
                            return Json(message);
                        }
                        else
                        {
                            string message = "Batch destuction failed!";
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
                return Json("Backend received empty, undefined, or null object from the client");
            }
        }
    }
}