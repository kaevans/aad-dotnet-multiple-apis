using aad_dotnet_multiple_apis.Cache;
using aad_dotnet_multiple_apis.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace aad_dotnet_multiple_apis.Controllers
{
    [Authorize]
    public class DatabaseController : Controller
    {
        // GET: Database
        public async Task<ActionResult> Index()
        {
            var authHelper = new AuthHelper(new DbTokenCache(AuthHelper.ClaimsSignedInUserID));
            var sqlConnectionString = ConfigurationManager.ConnectionStrings["DemoDB"].ConnectionString;

            string databaseUserID = default(string);
            try
            {
                using (SqlConnection conn = new SqlConnection(sqlConnectionString))
                {
                    conn.AccessToken = await authHelper.GetTokenForApplication(AuthHelper.AzureSQLDatabaseResourceId);
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("SELECT SUSER_SNAME()", conn))
                    {
                        var result = cmd.ExecuteScalar();
                        databaseUserID = result.ToString();
                    }
                }
            }
            catch (AdalSilentTokenAcquisitionException ee)
            {
                AuthHelper.RefreshSession("/Database");
            }
            // if the above failed, the user needs to explicitly re-authenticate for the app to obtain the required token
            catch (Exception oops)
            {
                ViewBag.Message = oops.Message;
                return View("Relogin");
            }

            ViewBag.DatabaseUserID = databaseUserID;
            return View();


        }
    }
}