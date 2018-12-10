using aad_dotnet_multiple_apis.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace aad_dotnet_multiple_apis.Controllers
{
    [Authorize]
    public class OnBehalfOfController : Controller
    {
        //Set as static readonly field to avoid TCP resource exhaustion.
        //See https://docs.microsoft.com/en-us/azure/architecture/antipatterns/improper-instantiation/
        private static readonly HttpClient client;

        static OnBehalfOfController()
        {
            client = new HttpClient();
        }

        // GET: OnBehalfOf
        public async Task<ActionResult> Index()
        {
            var authHelper = new AuthHelper(new ADALTokenCache(AuthHelper.ClaimsSignedInUserID));

            try
            {
                var accessToken = await authHelper.GetTokenForApplication(AuthHelper.CustomServiceResourceId);
                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var response = await client.GetAsync(AuthHelper.CustomServiceBaseAddress + "/api/MicrosoftGraph");

                string responseString = await response.Content.ReadAsStringAsync();


                var profile = JsonConvert.DeserializeObject<UserModel>(responseString);
                return View(profile);
            }
            // if the above failed, the user needs to explicitly re-authenticate for the app to obtain the required token
            catch (AdalSilentTokenAcquisitionException ee)
            {
                System.Diagnostics.Trace.TraceError("AdalSilentTokenAcquisitionException: " + ee.Message);
                AuthHelper.RefreshSession("/OnBehalfOf");
                return View("Relogin");
            }
            catch (Exception oops)
            {
                System.Diagnostics.Trace.TraceError("Exception: " + oops.Message);
                return View("Error");
            }

        }


        public ActionResult Reconsent()
        {
            //Add ability to renew consent, ability to test different scopes
            AuthHelper.RefreshSession("/OnBehalfOf", true);

            return View();
        }
    }
}