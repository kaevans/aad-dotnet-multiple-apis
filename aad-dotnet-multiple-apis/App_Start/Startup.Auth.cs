using aad_dotnet_multiple_apis.Cache;
using aad_dotnet_multiple_apis.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.IdentityModel.Claims;
using System.Threading.Tasks;
using System.Web;

namespace aad_dotnet_multiple_apis
{
    public partial class Startup
    {


        public void ConfigureAuth(IAppBuilder app)
        {
            ApplicationDbContext db = new ApplicationDbContext();

            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    ClientId = AuthHelper.ClientId,
                    Authority = AuthHelper.Authority,
                    PostLogoutRedirectUri = AuthHelper.PostLogoutRedirectUri,
                    //RedirectUri required if multiple reply URLs are configured in AAD
                    RedirectUri = AuthHelper.ReplyUrl,

                    Notifications = new OpenIdConnectAuthenticationNotifications()
                    {
                        // If there is a code in the OpenID Connect response, redeem it for an access token and refresh token, and store those away.
                       AuthorizationCodeReceived = (context) => 
                       {
                           var code = context.Code;
                           ClientCredential credential = new ClientCredential(AuthHelper.ClientId, AuthHelper.GetKey());
                           var signedInUserID = context.AuthenticationTicket.Identity.FindFirst(ClaimTypes.NameIdentifier).Value;

                           AuthenticationContext authContext = new AuthenticationContext(AuthHelper.Authority, new DbTokenCache(signedInUserID));
                           AuthenticationResult result = authContext.AcquireTokenByAuthorizationCodeAsync(
                               code, new Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path)), credential, AuthHelper.AzureADGraphResourceId).Result;

                           return Task.FromResult(0);
                       },
                        RedirectToIdentityProvider = (notification) =>
                        {
                            //If a controller added the "RenewConsent" setting, request
                            //the middleware to prompt again for consent. For ADAL, this means adding
                            //the ?prompt=consent querystring to the request to login.microsoftonline.com
                            var consent = notification.OwinContext.Get<string>("RenewConsent");

                            if (consent == "RenewConsent")
                            {
                                notification.ProtocolMessage.Prompt = "consent";
                            }
                            return Task.FromResult(0);
                        }
                    }
                });
        }
    }
}
