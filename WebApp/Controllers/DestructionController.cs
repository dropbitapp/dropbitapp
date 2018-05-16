using Microsoft.AspNet.Identity;
using System.Web.Mvc;
using WebApp.Helpers;
using WebApp.Models;
using WebApp.Workflows;

namespace WebApp.Controllers
{
    public class DestructionController : Controller
    {
        private readonly DistilDBContext _db;
        private readonly DataLayer _dl;
        private readonly PurchaseWorkflow _purchase;
        private readonly ProductionWorkflow _production;

        public DestructionController()
        {
            _db = new DistilDBContext();
            _dl = new DataLayer(_db);
            _purchase = new PurchaseWorkflow(_db, _dl);
            _production = new ProductionWorkflow(_db, _dl);
        }

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
                    var prodList = _production.GetProductionDataForDestruction(userId);
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
                    var recsList = _purchase.GetPurchaseDataForDestruction(userId);
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
                    var recsList = _dl.GetDestroyedBatches(userId);
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
                        bool returnResult = _dl.DestroyBatch(destructionObject, userId);
                        if (returnResult)
                        {
                            string message = "Batch successfully destroyed.";
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