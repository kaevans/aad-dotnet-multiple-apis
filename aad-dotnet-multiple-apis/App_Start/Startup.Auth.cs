using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IdentityModel.Claims;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Owin;
using aad_dotnet_multiple_apis.Models;

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

                    Notifications = new OpenIdConnectAuthenticationNotifications()
                    {
                        // If there is a code in the OpenID Connect response, redeem it for an access token and refresh token, and store those away.
                       AuthorizationCodeReceived = (context) => 
                       {
                           System.Diagnostics.Trace.WriteLine("OpenId Connect authorization code received");

                           var code = context.Code;
                           ClientCredential credential = new ClientCredential(AuthHelper.ClientId, AuthHelper.GetKey());
                           string signedInUserID = context.AuthenticationTicket.Identity.FindFirst(ClaimTypes.NameIdentifier).Value;

                           System.Diagnostics.Trace.WriteLine("SignedInUserID: " + signedInUserID);

                           AuthenticationContext authContext = new AuthenticationContext(AuthHelper.Authority, new ADALTokenCache(signedInUserID));
                           AuthenticationResult result = authContext.AcquireTokenByAuthorizationCodeAsync(
                               code, new Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path)), credential, AuthHelper.AzureADGraphResourceId).Result;

                           System.Diagnostics.Trace.WriteLine("Token received");

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
                                System.Diagnostics.Trace.WriteLine("Renewing consent");
                                notification.ProtocolMessage.Prompt = "consent";
                            }
                            return Task.FromResult(0);
                        }
                    }
                });
        }

        private static string EnsureTrailingSlash(string value)
        {
            if (value == null)
            {
                value = string.Empty;
            }

            if (!value.EndsWith("/", StringComparison.Ordinal))
            {
                return value + "/";
            }

            return value;
        }
    }
}
