using aad_dotnet_webapi_onbehalfof.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace aad_dotnet_webapi_onbehalfof.Controllers
{
    [Authorize]
    public class MicrosoftGraphController : ApiController
    {
        //
        // The Client ID is used by the application to uniquely identify itself to Azure AD.
        // The App Key is a credential used by the application to authenticate to Azure AD.
        // The Tenant is the name of the Azure AD tenant in which this application is registered.
        // The AAD Instance is the instance of Azure, for example public Azure or Azure China.
        // The Authority is the sign-in URL of the tenant.
        //
        private static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static string tenant = ConfigurationManager.AppSettings["ida:Tenant"];
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientID"];
        

        //
        // To authenticate to the Graph API, the app needs to know the Grah API's App ID URI.
        // To contact the Me endpoint on the Graph API we need the URL as well.
        //
        private static string graphResourceId = "https://graph.microsoft.com";
        
        private const string TenantIdClaimType = "http://schemas.microsoft.com/identity/claims/tenantid";

        
        

        // Error Constants
        const String SERVICE_UNAVAILABLE = "temporarily_unavailable";

        //Set as static readonly field to avoid TCP resource exhaustion.
        //See https://docs.microsoft.com/en-us/azure/architecture/antipatterns/improper-instantiation/
        private static readonly HttpClient client = new HttpClient();


        // POST api/microsoftgraph
        public async Task<UserProfile> Get()
        {
            var scopeClaim = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/scope");
            if (scopeClaim == null || !scopeClaim.Value.Contains("user_impersonation"))
            {
                throw new HttpResponseException(new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, ReasonPhrase = "The Scope claim does not contain 'user_impersonation' or scope claim not found" });
            }

            //
            // Call the Graph API On Behalf Of the user who called the To Do list web API.
            //            
            UserProfile profile = new UserProfile();
            profile = await CallGraphAPIOnBehalfOfUser();
            return profile;            
        }

        private static async Task<UserProfile> CallGraphAPIOnBehalfOfUser()
        {
            UserProfile profile = null;


            //
            // Call the Beta endpoint of the Graph API and retrieve the user's profile.
            //
            
            string requestUrl = "https://graph.microsoft.com/beta/me?$select=Id,DisplayName,GivenName,Surname,UserPrincipalName,OnPremisesDomainName,OnPremisesImmutableId,OnPremisesSamAccountName,OnPremisesUserPrincipalName";

            
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetAccessToken());
            var response = await client.SendAsync(request);

            //
            // Return the user's profile.
            //
            if (response.IsSuccessStatusCode)
            {
                string responseString = await response.Content.ReadAsStringAsync();
                profile = JsonConvert.DeserializeObject<UserProfile>(responseString);
                return (profile);
            }

            // An unexpected error occurred calling the Graph API.  Return a null profile.
            return (null);
        }

        private static async Task<string> GetAccessToken()
        {
            string accessToken = null;
            AuthenticationResult result = null;

            //
            // Use ADAL to get a token On Behalf Of the current user.  To do this we will need:
            //      The Resource ID of the service we want to call.
            //      The current user's access token, from the current request's authorization header.
            //      The credentials of this application.
            //      The username (UPN or email) of the user calling the API
            //
            ClientCredential clientCred = new ClientCredential(clientId, HttpContext.Current.Application["ida:ClientSecret"].ToString());
            string userName = ClaimsPrincipal.Current.FindFirst(ClaimTypes.Upn) != null ? ClaimsPrincipal.Current.FindFirst(ClaimTypes.Upn).Value : ClaimsPrincipal.Current.FindFirst(ClaimTypes.Email).Value;

            
            string userAccessToken = ClaimsPrincipal.Current.Identities.First().BootstrapContext as string;

            UserAssertion userAssertion = new UserAssertion(userAccessToken, "urn:ietf:params:oauth:grant-type:jwt-bearer", userName);

            string authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant);
            string userId = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
            AuthenticationContext authContext = new AuthenticationContext(authority, new ADALTokenCache(userId));

            // In the case of a transient error, retry once after 1 second, then abandon.
            // Retrying is optional.  It may be better, for your application, to return an error immediately to the user and have the user initiate the retry.
            bool retry = false;
            int retryCount = 0;

            do
            {
                retry = false;
                try
                {
                    result = await authContext.AcquireTokenAsync(graphResourceId, clientCred, userAssertion);
                    accessToken = result.AccessToken;
                }
                catch (AdalException ex)
                {
                    if (ex.ErrorCode == SERVICE_UNAVAILABLE)
                    {
                        // Transient error, OK to retry.
                        retry = true;
                        retryCount++;
                        await Task.Delay(1000);
                    }
                    else
                    {
                        //Not transient.
                        throw;
                    }
                }
            } while ((retry == true) && (retryCount < 2));

            return accessToken;
        }
    }
}
