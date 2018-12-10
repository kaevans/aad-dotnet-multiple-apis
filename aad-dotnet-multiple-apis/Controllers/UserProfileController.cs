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
        private readonly string clientId = AuthHelper.ClientId;
        private readonly string appKey = AuthHelper.GetKey();
        private readonly string aadInstance = AuthHelper.AadInstance;
        private readonly string graphResourceID = AuthHelper.AzureADGraphResourceId;

        // GET: UserProfile
        public async Task<ActionResult> Index()
        {
            string tenantID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;
            string userObjectID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
            try
            {                
                Uri servicePointUri = new Uri("https://graph.windows.net");
                Uri serviceRoot = new Uri(servicePointUri, tenantID);

                ActiveDirectoryClient activeDirectoryClient = new ActiveDirectoryClient(serviceRoot,
                      async () => await GetTokenForApplication());
                
                // use the token for querying the graph to get the user details

                var result = await activeDirectoryClient.Users
                    .Where(u => u.ObjectId.Equals(userObjectID))
                    .ExecuteAsync();
                IUser user = result.CurrentPage.ToList().First();

                return View(user);
            }
            // if the above failed, the user needs to explicitly re-authenticate for the app to obtain the required token
            catch (AdalSilentTokenAcquisitionException ee)
            {
                System.Diagnostics.Trace.TraceError("AdalSilentTokenAcquisitionException: " + ee.Message);
                AuthHelper.RefreshSession("/UserProfile");
                return View("Relogin");
            }
            catch (Exception oops)
            {
                System.Diagnostics.Trace.TraceError("Exception: " + oops.Message);
                return View("Error");
            }
        }

        public void RefreshSession()
        {
            AuthHelper.RefreshSession("/UserProfile");
        }

        public async Task<string> GetTokenForApplication()
        {
            string signedInUserID = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
            string tenantID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;
            string userObjectID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;

            // get a token for the Graph without triggering any user interaction (from the cache, via multi-resource refresh token, etc)
            ClientCredential clientcred = new ClientCredential(clientId, appKey);
            // initialize AuthenticationContext with the token cache of the currently signed in user, as kept in the app's database
            AuthenticationContext authenticationContext = new AuthenticationContext(aadInstance + tenantID, new ADALTokenCache(signedInUserID));
            AuthenticationResult authenticationResult = await authenticationContext.AcquireTokenSilentAsync(graphResourceID, clientcred, new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));
            return authenticationResult.AccessToken;
        }

    }
}
