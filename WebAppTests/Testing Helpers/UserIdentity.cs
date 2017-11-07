using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace WebAppTests.Testing_Helpers
{
    public static class GenericUserIdentity
    {
       public static void BaseUserIdentity(string userId)
       {

            //var identity = new GenericIdentity("TestUsername");
            //identity.AddClaim(new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", "UserIdIWantReturned"));

            //var fakePrincipal = A.Fake<IPrincipal>();
            //A.CallTo(() => fakePrincipal.Identity).Returns(identity);

            //var _controller = new MyController
            //{
            //    ControllerContext = A.Fake<ControllerContext>()
            //};

            //A.CallTo(() => _controller.ControllerContext.HttpContext.User).Returns(fakePrincipal);

            //IPrincipal principal = null;
            //principal = new GenericPrincipal(new GenericIdentity(userId),
            //null);

            //Thread.CurrentPrincipal = principal;
            //if(HttpContext.Current != null)
            //{
            //    HttpContext.Current.User = principal;
            //}
       }
    }
}
