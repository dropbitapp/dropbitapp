using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Web.Mvc;
using WebApp.Helpers;

namespace WebApp.Controllers
{
    public class ReportingController : Controller
    {
        private DataLayer dl = new DataLayer();
        private bool _enableNewReportingImplementation = false;

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
                    IEnumerable<ReportDto> report;

                    if (_enableNewReportingImplementation)
                    {
                        report = dl.GetReportData(startOfReporting, endOfReporting, userId, ReportType.Storage);

                        return Json(report, JsonRequestBehavior.AllowGet);
                    }

                    var storageReport = dl.GetStorageReportData(startOfReporting, endOfReporting, userId);
                    return Json(storageReport, JsonRequestBehavior.AllowGet);
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