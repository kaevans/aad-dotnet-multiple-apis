using Microsoft.Azure.KeyVault;
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
            string key = ConfigurationManager.AppSettings["ida:ClientSecret"];

            if (string.IsNullOrEmpty(key))
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
            else
            {
                //Fallback to using the key from configuration 
                HttpContext.Current.Application["ida:ClientSecret"] = key;
                System.Diagnostics.Debug.WriteLine("Client secret stored in Application state from configuration file");
            }
        }
    }
}