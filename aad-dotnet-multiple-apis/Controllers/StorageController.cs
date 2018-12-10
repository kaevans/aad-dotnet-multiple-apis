using aad_dotnet_multiple_apis.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Configuration;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace aad_dotnet_multiple_apis.Controllers
{
    [Authorize]
    public class StorageController : Controller
    {
        private async Task<CloudBlobClient> GetStorageClient()
        {
            var tokenHelper = new AuthHelper(new ADALTokenCache(AuthHelper.ClaimsSignedInUserID));

            var accessToken = await tokenHelper.GetTokenForApplication(AuthHelper.AzureStorageResourceId);

            // Use the access token to create the storage credentials.
            var tokenCredential = new TokenCredential(accessToken);
            StorageCredentials storageCredentials = new StorageCredentials(tokenCredential);

            string storageAccountUrl = ConfigurationManager.AppSettings["StorageAccountUrl"];
            var client = new CloudBlobClient(new Uri(storageAccountUrl), storageCredentials);

            return client;
        }

        // GET: Storage
        public async Task<ActionResult> Index()
        {            
            var items = new System.Collections.Generic.List<StorageModel>();

            try
            {
                var client = await GetStorageClient();
                
                var container = client.GetContainerReference("demo");

                //Don't do this in production due to overhead of additional call to check
                //Only good for demos to make sure the container exists
                await container.CreateIfNotExistsAsync();

                BlobContinuationToken blobContinuationToken = null;

                do
                {
                    var results = await container.ListBlobsSegmentedAsync(null, blobContinuationToken);

                    // Get the value of the continuation token returned by the listing call.
                    blobContinuationToken = results.ContinuationToken;

                    foreach (CloudBlockBlob item in results.Results)
                    {                        
                        items.Add(new StorageModel(item));
                    }

                } while (blobContinuationToken != null); // Loop while the continuation token is not null.

                return View(items);
            }
            // if the above failed, the user needs to explicitly re-authenticate for the app to obtain the required token
            catch (AdalSilentTokenAcquisitionException ee)
            {
                System.Diagnostics.Trace.TraceError("AdalSilentTokenAcquisitionException: " + ee.Message);
                AuthHelper.RefreshSession("/Storage");
                return View("Relogin");
            }
            catch (Exception oops)
            {
                System.Diagnostics.Trace.TraceError("Exception: " + oops.Message);
                return View("Error");
            }
        }

        //POST: Storage/Create
        [HttpPost]
        public async Task<ActionResult> Create()
        {


            try
            {
                var client = await GetStorageClient();

                var container = client.GetContainerReference("demo");

                //Don't do this in production due to overhead of additional call to check
                //Only good for demos to make sure the container exists
                await container.CreateIfNotExistsAsync();

                string userObjectID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;

                var fileName = string.Format("{0}-{1}.txt", userObjectID, System.DateTime.Now.Ticks);
                var blob = container.GetBlockBlobReference(fileName);

                StringBuilder sb = new StringBuilder();
                //Store all claim values about the current user into the file
                foreach (var claim in ClaimsPrincipal.Current.Claims)
                {
                    sb.AppendFormat("{0}:{1}", claim.Type, claim.Value);
                    sb.AppendLine();
                }

                using (var stream = new MemoryStream(Encoding.Default.GetBytes(sb.ToString()), false))
                {
                    blob.UploadFromStream(stream, null);
                }

                return RedirectToAction("Index");

            }
            // if the above failed, the user needs to explicitly re-authenticate for the app to obtain the required token
            catch (AdalSilentTokenAcquisitionException ee)
            {
                System.Diagnostics.Trace.TraceError("AdalSilentTokenAcquisitionException: " + ee.Message);
                AuthHelper.RefreshSession("/Storage");
                return View("Relogin");
            }
            catch (Exception oops)
            {
                System.Diagnostics.Trace.TraceError("Exception: " + oops.Message);
                return View("Error");
            }
        }



        // GET: Storage/Details/5
        public async Task<ActionResult> Details(string fileName)
        {
            var blobModel = new StorageModel();
            try
            {
                var tokenHelper = new AuthHelper(new ADALTokenCache(AuthHelper.ClaimsSignedInUserID));
                var client = await GetStorageClient();

                var container = client.GetContainerReference("demo");                
                var blob = container.GetBlockBlobReference(fileName);
                await blob.FetchAttributesAsync();

                blobModel = new StorageModel(blob);
                blobModel.Contents = await blob.DownloadTextAsync();

                return View(blobModel);
            }
            // if the above failed, the user needs to explicitly re-authenticate for the app to obtain the required token
            catch (AdalSilentTokenAcquisitionException ee)
            {
                System.Diagnostics.Trace.TraceError("AdalSilentTokenAcquisitionException: " + ee.Message);
                AuthHelper.RefreshSession("/ARM");
                return View("Relogin");
            }
            catch (Exception oops)
            {
                System.Diagnostics.Trace.TraceError("Exception: " + oops.Message);
                return View("Error");
            }

        }


        [HttpPost]
        public async Task<ActionResult> Delete(string fileName)
        {
            try
            {
                // TODO: Add delete logic here
                var tokenHelper = new AuthHelper(new ADALTokenCache(AuthHelper.ClaimsSignedInUserID));
                var client = await GetStorageClient();

                var container = client.GetContainerReference("demo");
                var blob = container.GetBlockBlobReference(fileName);
                await blob.DeleteAsync();
                return RedirectToAction("Index");
            }
            // if the above failed, the user needs to explicitly re-authenticate for the app to obtain the required token
            catch (AdalSilentTokenAcquisitionException ee)
            {
                System.Diagnostics.Trace.TraceError("AdalSilentTokenAcquisitionException: " + ee.Message);
                AuthHelper.RefreshSession("/Storage");
                return View("Relogin");
            }
            catch (Exception oops)
            {
                System.Diagnostics.Trace.TraceError("Exception: " + oops.Message);
                return View("Error");
            }
        }


        public void RefreshSession()
        {
            AuthHelper.RefreshSession("/Storage");
        }

    }
}