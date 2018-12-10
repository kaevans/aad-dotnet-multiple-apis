using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using System.Configuration;
using System.Web;

namespace aad_dotnet_webapi_onbehalfof
{
    public class KeyConfig
    {
        /// <summary>
        /// Uses Managed Service Identity to obtain a token from Azure Key Vault.
        /// To debug locally in Visual Studio 2017, go to 
        /// Tools / Options / Azure Service Authentication and sign in as an 
        /// account that has permission to Get a secret from the Key Vault.
        /// </summary>
        public static void RegisterKeys()
        {
            System.Diagnostics.Trace.WriteLine("UseAzureServiceTokenProvider");
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var kv = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

            var keyVaultUrl = ConfigurationManager.AppSettings["KeyVaultUrl"];

            var secret = kv.GetSecretAsync(keyVaultUrl, "webapi-onbehalfof-client-secret").GetAwaiter().GetResult();

            HttpContext.Current.Application["ida:ClientSecret"] = secret.Value;

            System.Diagnostics.Trace.WriteLine("Secret retrieved");
        }

    }
}