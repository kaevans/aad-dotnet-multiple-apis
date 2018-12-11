using aad_dotnet_multiple_apis.Models;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace aad_dotnet_multiple_apis.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Reconsent()
        {
            //Add ability to renew consent, ability to test different scopes
            AuthHelper.RefreshSession("/Home", true);

            return View();
        }

        public async Task<ActionResult> Purge()
        {
            try
            {
                await ADALTokenCache.PurgeAsync();
                return RedirectToAction("Index");
            }
            catch
            {
                return View("Error");
            }
            
        }
    }
}