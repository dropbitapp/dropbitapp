using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using WebApp.Helpers;

namespace WebApp.Controllers
{
    public class DeveloperController : Controller
    {
        private DataLayer dl = new DataLayer();

        public void Test()
        {
            #if DEBUG
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.Identity.GetUserId<int>();
                if (userId > 0)
                {
                    // Test code goes here
                }
            }
            #endif
        }

        public void NukeRecords()
        {
            #if DEBUG
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.Identity.GetUserId<int>();
                if (userId > 0)
                {
                    try
                    {
                        bool returnResult = dl.RemoveRecordsFromDBForUser(7);
                    }
                    catch
                    {
                        throw;
                    }
                }
            }
            #endif
        }
    }
}