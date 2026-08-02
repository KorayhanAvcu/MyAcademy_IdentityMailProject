using IdentityMail.Web.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityMail.Web.ViewComponents.AdminLayout
{
    public class _AdminLayoutTopnavbarViewComponent : ViewComponent
    {
        private readonly UserManager<AppUser> _userManager;

        public _AdminLayoutTopnavbarViewComponent(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            return View(user);
        }
    }
}