using aad_dotnet_multiple_apis.Cache;
using aad_dotnet_multiple_apis.Models;
using Microsoft.Graph;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace aad_dotnet_multiple_apis.Controllers
{
    [Authorize]
    public class MicrosoftGraphController : Controller
    {
        // GET: MicrosoftGraph
        public async Task<ActionResult> Index()
        {

            var ret = new List<UserModel>();

            try
            {
                var authHelper = new AuthHelper(new DbTokenCache(AuthHelper.ClaimsSignedInUserID));


                var graphServiceClient = new GraphServiceClient(new DelegateAuthenticationProvider(async (requestMessage) =>
                {
                    var accessToken = await authHelper.GetTokenForApplication(AuthHelper.MicrosoftGraphResourceId);
                    requestMessage
                        .Headers
                        .Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                }));

                var users = await graphServiceClient.Users.Request().GetAsync();
                foreach (var user in users)
                {
                    ret.Add(new UserModel
                    {
                        Id = user.Id,
                        DisplayName = user.DisplayName,
                        UserPrincipalName = user.UserPrincipalName,
                        OnPremisesDomainName = user.OnPremisesDomainName,
                        OnPremisesImmutableId = user.OnPremisesImmutableId,
                        OnPremisesSamAccountName = user.OnPremisesSamAccountName,
                        OnPremisesUserPrincipalName = user.OnPremisesUserPrincipalName
                    });
                }
            }
            // if the above failed, the user needs to explicitly re-authenticate for the app to obtain the required token
            catch (AdalSilentTokenAcquisitionException ee)
            {
                AuthHelper.RefreshSession("/MicrosoftGraph");
            }
            // if the above failed, the user needs to explicitly re-authenticate for the app to obtain the required token
            catch (Exception oops)
            {
                ViewBag.Message = oops.Message;
                return View("Relogin");
            }

            return View("View", ret);
        }

        public async Task<ActionResult> Me()
        {

            var ret = new UserModel();

            try
            {
                var authHelper = new AuthHelper(new DbTokenCache(AuthHelper.ClaimsSignedInUserID));
                var accessToken = await authHelper.GetTokenForApplication(AuthHelper.MicrosoftGraphResourceId);

                var graphServiceClient = new GraphServiceClient("https://graph.microsoft.com/beta", new DelegateAuthenticationProvider((requestMessage) =>
                {
                    requestMessage
                        .Headers
                        .Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                    return Task.FromResult(0);

                }));


                var user = await graphServiceClient.Me.Request().GetAsync();
                ret = new UserModel
                {
                    Id = user.Id,
                    DisplayName = user.DisplayName,
                    UserPrincipalName = user.UserPrincipalName,
                    OnPremisesDomainName = user.OnPremisesDomainName,
                    OnPremisesImmutableId = user.OnPremisesImmutableId,
                    OnPremisesSamAccountName = user.OnPremisesSamAccountName,
                    OnPremisesUserPrincipalName = user.OnPremisesUserPrincipalName
                };

            }
            // if the above failed, the user needs to explicitly re-authenticate for the app to obtain the required token
            catch (AdalSilentTokenAcquisitionException ee)
            {
                AuthHelper.RefreshSession("/MicrosoftGraph");
            }
            // if the above failed, the user needs to explicitly re-authenticate for the app to obtain the required token
            catch (Exception oops)
            {
                ViewBag.Message = oops.Message;
                return View("Relogin");
            }

            return View("Me", ret);
        }

        public ActionResult Reconsent()
        {
            //Add ability to renew consent, ability to test different scopes
            AuthHelper.RefreshSession("/MicrosoftGraph", true);

            return View();
        }
    }
}