using aad_dotnet_multiple_apis.Cache;
using aad_dotnet_multiple_apis.Models;
using Microsoft.Azure.ActiveDirectory.GraphClient;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace aad_dotnet_multiple_apis.Controllers
{
    [Authorize]
    public class UserProfileController : Controller
    {
        

        // GET: UserProfile
        public async Task<ActionResult> Index()
        {
            string tenantID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;
            string userObjectID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
            string signedInUserId = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
            string key = AuthHelper.GetKey();

            try
            {

                Uri servicePointUri = new Uri(AuthHelper.AzureADGraphResourceId);
                Uri serviceRoot = new Uri(servicePointUri, tenantID);

                var authHelper = new AuthHelper(new DbTokenCache(signedInUserId));

                ActiveDirectoryClient activeDirectoryClient = new ActiveDirectoryClient(serviceRoot,
                       async () => await authHelper.GetTokenForApplication(AuthHelper.AzureADGraphResourceId, key));

                // use the token for querying the graph to get the user details
                
                var result = await activeDirectoryClient.Users
                    .Where(u => u.ObjectId.Equals(userObjectID))
                    .ExecuteAsync();
                IUser user = result.CurrentPage.ToList().First();

                return View(user);
            }
            catch (AdalException)
            {
                // Return to error page.
                return View("Error");
            }
            // if the above failed, the user needs to explicitly re-authenticate for the app to obtain the required token
            catch (Exception oops)
            {
                ViewBag.ExceptionMessage = oops.Message;
                return View("Relogin");
            }
        }

        //public async Task<string> GetTokenForApplication()
        //{
        //    var signedInUserId = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
        //    var cache = new DbTokenCache(signedInUserId);
        //    var authHelper = new AuthHelper(cache);
        //    string aadGraphResourceId = AuthHelper.AzureADGraphResourceId;
        //    var accessToken = await authHelper.GetTokenForApplication(aadGraphResourceId);
        //    return accessToken;
        //}

        public ActionResult RefreshSession()
        {
            AuthHelper.RefreshSession("/UserProfile");
            return View("Relogin");
        }



    }
}
