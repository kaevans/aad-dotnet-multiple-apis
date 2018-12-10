using aad_dotnet_multiple_apis.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace aad_dotnet_multiple_apis.Controllers
{
    /// <summary>
    /// Demonstrates how to get an access token to the
    /// Azure Management API (ARM) and use the HttpClient
    /// class to make a restful call to retrieve the 
    /// current user's Azure subscription information.
    /// </summary>
    [Authorize]    
    public class ARMController : Controller
    {
        //Set as static readonly field to avoid TCP resource exhaustion.
        //See https://docs.microsoft.com/en-us/azure/architecture/antipatterns/improper-instantiation/
        private static readonly HttpClient client;

        static ARMController()
        {
            client = new HttpClient();
        }
        // GET: ARM
        public async Task<ActionResult> Index()
        {
            var authHelper = new AuthHelper(new ADALTokenCache(AuthHelper.ClaimsSignedInUserID));

            var accessToken = await authHelper.GetTokenForApplication(AuthHelper.AzureManagementResourceId);
            List<Subscription> subscriptions = null;

            try
            {
                //Set the Authorization header to use the access token
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);                

                var response = await client.GetAsync("https://management.azure.com/subscriptions?api-version=2016-06-01");
                var json = await response.Content.ReadAsStringAsync();
                subscriptions = JsonConvert.DeserializeObject<SubscriptionModel>(json).Subscriptions;

                return View(subscriptions);
            }
            // if the above failed, the user needs to explicitly re-authenticate for the app to obtain the required token
            catch (AdalSilentTokenAcquisitionException ee)
            {
                System.Diagnostics.Trace.TraceError("AdalSilentTokenAcquisitionException: " + ee.Message);
                AuthHelper.RefreshSession("/ARM");
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
            AuthHelper.RefreshSession("/ARM");
        }
    }    
}