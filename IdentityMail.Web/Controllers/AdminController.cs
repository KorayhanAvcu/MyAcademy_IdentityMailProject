using Microsoft.AspNetCore.Mvc;

namespace IdentityMail.Web.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
