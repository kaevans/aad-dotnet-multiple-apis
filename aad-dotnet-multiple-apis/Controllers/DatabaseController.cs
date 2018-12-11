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
            
            
            try
            {
                var authHelper = new AuthHelper(new ADALTokenCache(AuthHelper.ClaimsSignedInUserID));
                var sqlConnectionString = ConfigurationManager.ConnectionStrings["DemoDB"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(sqlConnectionString))
                {
                    conn.AccessToken = await authHelper.GetTokenForApplication(AuthHelper.AzureSQLDatabaseResourceId);
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("SELECT SUSER_SNAME()", conn))
                    {
                        var result = cmd.ExecuteScalar();
                        ViewBag.DatabaseUserID = result.ToString();
                    }
                }
                return View();
            }
            // if the above failed, the user needs to explicitly re-authenticate for the app to obtain the required token
            catch (AdalSilentTokenAcquisitionException ee)
            {
                System.Diagnostics.Trace.TraceError("AdalSilentTokenAcquisitionException: " + ee.Message);
                AuthHelper.RefreshSession("/Database");
                return View("Relogin");
            }
            catch (Exception oops)
            {
                ViewBag.AdditionalInfo = "Make sure the current user is added to Azure SQL Database";
                System.Diagnostics.Trace.TraceError("Exception: " + oops.Message);
                return View("Error");
            }
        }
    }
}