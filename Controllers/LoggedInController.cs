using Microsoft.AspNetCore.Mvc;

namespace SAMLHost.Controllers
{
    public class LoggedInController : Controller
    {
        [Route("LoggedIn/GoodJob")]
        public ActionResult GoodJob()
        {
            return View();
        }
    }
}