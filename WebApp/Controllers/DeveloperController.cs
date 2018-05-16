using Microsoft.AspNet.Identity;
using System.Web.Mvc;
using WebApp.Helpers;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class DeveloperController : Controller
    {
        private readonly DistilDBContext _db;
        private readonly DataLayer _dl;

        public DeveloperController()
        {
            _db = new DistilDBContext();
            _dl = new DataLayer(_db);
        }

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
                        bool returnResult = _dl.RemoveRecordsFromDBForUser(userId);
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
                        bool returnResult = _dl.RemoveRecordsFromDBForUser(7);
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