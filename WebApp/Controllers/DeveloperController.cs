using Microsoft.AspNet.Identity;
using System.Web.Mvc;
using WebApp.Helpers;

namespace WebApp.Controllers
{
    public class DeveloperController : Controller
    {
        private DataLayer dl = new DataLayer();

#if DEBUG
        public void Test()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.Identity.GetUserId<int>();
                if (userId > 0)
                {
                    // Test code goes here
                }
            }
        }

        // /Developer/NukeRecords
        public void NukeRecords()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.Identity.GetUserId<int>();
                if (userId > 0)
                {
                    try
                    {
                        bool returnResult = dl.RemoveRecordsFromDBForUser(userId);
                    }
                    catch
                    {
                        throw;
                    }
                }
            }
        }

        // /Developer/NukeTestAccountRecords
        public void NukeTestAccountRecords()
        {
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
        }
#endif
    }
}