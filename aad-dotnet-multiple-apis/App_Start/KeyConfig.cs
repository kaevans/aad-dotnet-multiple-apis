using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;

namespace aad_dotnet_multiple_apis
{
    public class KeyConfig
    {
        public static void RegisterKeys()
        {
            string deployType = ConfigurationManager.AppSettings["deployType"];
            switch (deployType)
            {
                case "local":
                    //Use this when debugging locally with ida:ClientSecret in web.config
                    UseConfig();
                    break;
                case "vm":
                    //The following works for Azure VMs
                    UseVMMSIUrl();
                    break;
                default:
                    //The following works for both Azure App Service and Azure VMs
                    UseAzureServiceTokenProvider();
                    break;
            }

        }

        private static void UseVMMSIUrl()
        {
            //Use MSI to contact Key Vault
            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(
               async (authority, resource, scope) =>
               {
                   var request = (HttpWebRequest)WebRequest.Create("http://169.254.169.254/metadata/identity/oauth2/token?api-version=2018-02-01&resource=" + resource);
                   request.Headers["Metadata"] = "true";
                   request.Method = "GET";

                   // Call /token endpoint
                   var response = (HttpWebResponse)await request.GetResponseAsync();
                   System.Diagnostics.Debug.WriteLine("Got token response from MSI");

                   // Pipe response Stream to a StreamReader, and extract access token
                   var streamResponse = new StreamReader(response.GetResponseStream());
                   var stringResponse = streamResponse.ReadToEnd();
                   var j = new JavaScriptSerializer();
                   Dictionary<string, string> list = (Dictionary<string, string>)j.Deserialize(stringResponse, typeof(Dictionary<string, string>));
                   string accessToken = list["access_token"];

                   return accessToken;
               }));

            var keyVaultUrl = ConfigurationManager.AppSettings["KeyVaultUrl"];
            var secret = keyVaultClient.GetSecretAsync(keyVaultUrl, "multiple-apis-client-secret").GetAwaiter().GetResult();

            HttpContext.Current.Application["ida:ClientSecret"] = secret.Value;
            System.Diagnostics.Debug.WriteLine("Client secret stored in Application state from KeyVault");
        }

        private static void UseAzureServiceTokenProvider()
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var kv = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

            var keyVaultUrl = ConfigurationManager.AppSettings["KeyVaultUrl"];

            var secret = kv.GetSecretAsync(keyVaultUrl, "multiple-apis-client-secret").GetAwaiter().GetResult();

            HttpContext.Current.Application["ida:ClientSecret"] = secret.Value;
            System.Diagnostics.Debug.WriteLine("Client secret stored in Application state from KeyVault");
        }

        private static void UseConfig()
        {
            //Fallback to using the key from configuration 
            string key = ConfigurationManager.AppSettings["ida:ClientSecret"];
            HttpContext.Current.Application["ida:ClientSecret"] = key;
            System.Diagnostics.Debug.WriteLine("Client secret stored in Application state from configuration file");
        }
    }
}