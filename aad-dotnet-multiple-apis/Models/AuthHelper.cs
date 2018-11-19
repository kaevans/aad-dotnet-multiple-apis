using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;
using System;
using System.Configuration;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace aad_dotnet_multiple_apis.Models
{
    public class AuthHelper
    {

        public static string ClientId = ConfigurationManager.AppSettings["ida:ClientId"];        
        public static string AadInstance = EnsureTrailingSlash(ConfigurationManager.AppSettings["ida:AADInstance"]);
        public static string TenantId = ConfigurationManager.AppSettings["ida:TenantId"];
        public static string ReplyUrl = ConfigurationManager.AppSettings["ida:ReplyUrl"];
        public static string PostLogoutRedirectUri = ConfigurationManager.AppSettings["ida:PostLogoutRedirectUri"];

        public static readonly string Authority = AadInstance + TenantId;

        // Resource ID of the Azure AD Graph API.  
        public static string AzureADGraphResourceId = "https://graph.windows.net";

        // Resource ID of the Microsoft Graph API
        public static string MicrosoftGraphResourceId = "https://graph.microsoft.com";

        // Resource ID of the Azure Storage API.  
        public static string AzureStorageResourceId = "https://storage.azure.com/";

        // Resource ID of the Azure SQL Database API.  
        public static string AzureSQLDatabaseResourceId = "https://database.windows.net/";

        // Resource ID of the Azure Management API.  
        public static string AzureManagementResourceId = "https://management.azure.com/";

        private TokenCache _tokenCache;
        public AuthHelper(TokenCache cacheImpl)
        {
            _tokenCache = cacheImpl;
        }

        public static string ClaimsSignedInUserID
        {
            get { return ClaimsPrincipal.Current.FindFirst(System.IdentityModel.Claims.ClaimTypes.NameIdentifier).Value; }
        }





        public async Task<string> GetTokenForApplication(string resourceId, string key)
        {
            // get a token for the Graph without triggering any user interaction (from the cache, via multi-resource refresh token, etc)

            var userObjectId = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value; 
            ClientCredential clientcred = new ClientCredential(ClientId, key);
            // initialize AuthenticationContext with the token cache of the currently signed in user
            AuthenticationContext authenticationContext = new AuthenticationContext(Authority, _tokenCache);
            AuthenticationResult authenticationResult = await authenticationContext.AcquireTokenSilentAsync(resourceId, clientcred, new UserIdentifier(userObjectId, UserIdentifierType.UniqueId));
            return authenticationResult.AccessToken;

        }

        public async Task<string> GetTokenForApplication(string resourceId)
        {
            string key = GetKey();
            string token = await GetTokenForApplication(resourceId, key);
            return token;
        }



        public static void RefreshSession(string redirectUri, bool reconsent)
        {
            if (reconsent)
            {
                HttpContext.Current.GetOwinContext().Set("RenewConsent", "RenewConsent");
            }
            RefreshSession(redirectUri);
        }

        public static void RefreshSession(string redirectUri)
        {
            HttpContext.Current.GetOwinContext().Authentication.Challenge(
                new AuthenticationProperties { RedirectUri = redirectUri },
                OpenIdConnectAuthenticationDefaults.AuthenticationType);

        }

        /// <summary>
        /// Get client secret key from application state. Key is retrieved
        /// from KeyVault in Application_Start via the KeyConfig class
        /// </summary>
        /// <returns>Application secret</returns>
        public static string GetKey()
        {
            var keyObj = HttpContext.Current.Application["ida:ClientSecret"];
            if (null == keyObj)
            {
                KeyConfig.RegisterKeys();
            }
            string key = HttpContext.Current.Application["ida:ClientSecret"].ToString();
            return key;
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