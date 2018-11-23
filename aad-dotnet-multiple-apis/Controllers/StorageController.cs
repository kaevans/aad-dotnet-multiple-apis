using aad_dotnet_multiple_apis.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace aad_dotnet_multiple_apis.Controllers
{
    [Authorize]
    public class StorageController : Controller
    {

        // GET: Storage
        public async Task<ActionResult> Index()
        {
            var tokenHelper = new AuthHelper(new ADALTokenCache(AuthHelper.ClaimsSignedInUserID));


            var items = new System.Collections.Generic.List<StorageModel>();

            try
            {
                var accessToken = await tokenHelper.GetTokenForApplication(AuthHelper.AzureStorageResourceId);
                // Use the access token to create the storage credentials.
                var tokenCredential = new TokenCredential(accessToken);
                StorageCredentials storageCredentials = new StorageCredentials(tokenCredential);

                string storageAccountUrl = ConfigurationManager.AppSettings["StorageAccountUrl"];
                var client = new CloudBlobClient(new Uri(storageAccountUrl), storageCredentials);

                var container = client.GetContainerReference("demo");
                //Don't do this in production due to overhead of additional call to check
                //Only good for demos to make sure the container exists
                await container.CreateIfNotExistsAsync();

                var fileName = Guid.NewGuid().ToString() + ".txt";
                var blob = container.GetBlockBlobReference(fileName);

                using (var stream = new MemoryStream(Encoding.Default.GetBytes("Hello world"), false))
                {
                    blob.UploadFromStream(stream, null);
                }

                BlobContinuationToken blobContinuationToken = null;

                do
                {
                    var results = await container.ListBlobsSegmentedAsync(null, blobContinuationToken);

                    // Get the value of the continuation token returned by the listing call.
                    blobContinuationToken = results.ContinuationToken;

                    foreach (IListBlobItem item in results.Results)
                    {
                        items.Add(new StorageModel(item));
                    }

                } while (blobContinuationToken != null); // Loop while the continuation token is not null.
            }
            // if the above failed, the user needs to explicitly re-authenticate for the app to obtain the required token
            catch (AdalSilentTokenAcquisitionException ee)
            {
                AuthHelper.RefreshSession("/Storage");
            }
            // if the above failed, the user needs to explicitly re-authenticate for the app to obtain the required token
            catch (Exception oops)
            {
                ViewBag.Message = oops.Message;
                return View("Relogin");
            }

            return View("IndexView", items);
        }

    }
}