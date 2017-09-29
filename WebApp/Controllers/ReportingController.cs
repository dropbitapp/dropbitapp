using Microsoft.AspNet.Identity;
using System.Web.Mvc;
using WebApp.Helpers;

namespace WebApp.Controllers
{
    public class ReportingController : Controller
    {
        private DataLayer dl = new DataLayer();

        // GET: Production Operations
        public ActionResult Production()
        {
            return View();
        }

        // GET: Processing Operations
        public ActionResult Processing()
        {
            return View();
        }

        // GET: Storage Operations
        public ActionResult Storage()
        {
            return View();
        }

        /// <summary>
        /// This method is called when user is asking data for Production Report
        /// </summary>
        /// <param name="startOfReporting"></param>
        /// <param name="endOfReporting"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetProductionReport(System.DateTime startOfReporting, System.DateTime endOfReporting)
        {
            if (User.Identity.IsAuthenticated)
            {
                int userId = User.Identity.GetUserId<int>();
                if (userId > 0)
                {
                    var productionReport = dl.GetProductionReportData(startOfReporting, endOfReporting, userId);
                    return Json(productionReport, JsonRequestBehavior.AllowGet);
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
        /// This method is called when user is asking data for Processing Report
        /// </summary>
        /// <param name="startOfReporting"></param>
        /// <param name="endOfReporting"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetProcessingReport(System.DateTime startOfReporting, System.DateTime endOfReporting)
        {
            if (User.Identity.IsAuthenticated)
            {
                int userId = User.Identity.GetUserId<int>();
                if (userId > 0)
                {
                    var processingReport = dl.GetProcessingReportData(startOfReporting, endOfReporting, userId);
                    return Json(processingReport, JsonRequestBehavior.AllowGet);
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
        /// This method is called when user is asking data for Storage Report
        /// </summary>
        /// <param name="startOfReporting"></param>
        /// <param name="endOfReporting"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetStorageReport(System.DateTime startOfReporting, System.DateTime endOfReporting)
        {
            if (User.Identity.IsAuthenticated)
            {
                int userId = User.Identity.GetUserId<int>();
                if (userId > 0)
                {
                    var report = dl.GetStorageReportData(startOfReporting, endOfReporting, userId);
                    return Json(report, JsonRequestBehavior.AllowGet);
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
    }
}