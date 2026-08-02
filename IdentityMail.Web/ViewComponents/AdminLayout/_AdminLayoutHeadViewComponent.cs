using Microsoft.AspNetCore.Mvc;

namespace IdentityMail.Web.ViewComponents.AdminLayout
{
    public class _AdminLayoutHeadViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
