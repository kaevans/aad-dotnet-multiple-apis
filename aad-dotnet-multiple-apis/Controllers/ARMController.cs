using aad_dotnet_multiple_apis.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace aad_dotnet_multiple_apis.Controllers
{
    [Authorize]
    public class ARMController : Controller
    {
        // GET: ARM
        public async Task<ActionResult> Index()
        {
            var authHelper = new AuthHelper(new ADALTokenCache(AuthHelper.ClaimsSignedInUserID));

            var accessToken = await authHelper.GetTokenForApplication(AuthHelper.AzureManagementResourceId);
            List<Subscription> subscriptions = null;

            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
                client.BaseAddress = new Uri(AuthHelper.AzureManagementResourceId);

                var response = await client.GetAsync("/subscriptions?api-version=2016-06-01");
                var json = await response.Content.ReadAsStringAsync();
                subscriptions = JsonConvert.DeserializeObject<SubscriptionModel>(json).Subscriptions;
            }
            // if the above failed, the user needs to explicitly re-authenticate for the app to obtain the required token
            catch (AdalSilentTokenAcquisitionException ee)
            {
                AuthHelper.RefreshSession("/ARM");
            }
            // if the above failed, the user needs to explicitly re-authenticate for the app to obtain the required token
            catch (Exception oops)
            {
                ViewBag.Message = oops.Message;
                return View("Relogin");
            }

            return View("View", subscriptions);
        }
    }
}