using Microsoft.AspNet.Identity;
using System.Web.Mvc;
using NLog;
using System;
using System.Web.Script.Serialization;
using WebApp.Helpers;
using WebApp.Models;
using WebApp.Persistence.BusinessLogicEnums;
using WebApp.Persistence.Repositories;
using WebApp.Reports;

namespace WebApp.Controllers
{
    public class ReportingController : Controller
    {
        private readonly DistilDBContext _db;
        private readonly DataLayer _dl;
        private readonly ProductionReport _productionR;
        private readonly ProcessingReport _processingR;
        private readonly StorageReport _storageR;
        private readonly ReportRepository _reportRepository;
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private bool _enableNewReportingImplementation = false;

        public ReportingController()
        {
            _db = new DistilDBContext();
            _dl = new DataLayer(_db);
            _productionR = new ProductionReport(_db, _dl);
            _processingR = new ProcessingReport(_db, _dl);
            _storageR = new StorageReport(_db, _dl);
            _reportRepository = new ReportRepository(_db, _dl);
        }

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
                    try
                    {
                        _logger.Info("{0} {1}\n\tReporting Start: {2}\n\tReporting End: {3}", User.Identity.Name, Request.Url.ToString(), startOfReporting, endOfReporting);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Logging error");
                    }

                    var productionReport = _productionR.GetProductionReportData(startOfReporting, endOfReporting, userId);

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
                    try
                    {
                        _logger.Info("{0} {1}\n\tReporting Start: {2}\n\tReporting End: {3}", User.Identity.Name, Request.Url.ToString(), startOfReporting, endOfReporting);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Logging error");
                    }

                    var processingReport = _processingR.GetProcessingReportData(startOfReporting, endOfReporting, userId);

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
                    try
                    {
                        _logger.Info("{0} {1}\n\tReporting Start: {2}\n\tReporting End: {3}", User.Identity.Name, Request.Url.ToString(), startOfReporting, endOfReporting);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Logging error");
                    }

                    if (_enableNewReportingImplementation)
                    {
                        var report = _reportRepository.GetReportData(endOfReporting, userId, PersistReportType.Storage);

                        return Json(report, JsonRequestBehavior.AllowGet);
                    }

                    var storageReport = _storageR.GetStorageReportData(startOfReporting, endOfReporting, userId);

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