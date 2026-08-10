using IdentityMail.Web.Constants;
using IdentityMail.Web.Context;
using IdentityMail.Web.Entities;
using IdentityMail.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityMail.Web.ViewComponents.AdminLayout
{
    public class _AdminLayoutSidebarViewComponent : ViewComponent
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;

        public _AdminLayoutSidebarViewComponent(UserManager<AppUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (user == null)
            {
                return Content(string.Empty);
            }

            var isAdmin = await _userManager.IsInRoleAsync(
                user,
                Roles.Admin);

            var isUser = await _userManager.IsInRoleAsync(
                user,
                Roles.User);

            var unreadCount = await _context.UserMessages
                .Where(x => x.ReceiverId == user.Id)
                .Where(x => x.IsRead == false)
                .Where(x => x.IsDelete == false)
                .Where(x => x.IsDraft != true)
                .CountAsync();
            var model = new SidebarViewModel
            {
                User = user,
                IsAdmin = isAdmin,
                IsUser = isUser,
                UnreadCount = unreadCount
            };

            return View(model);
        }
    }
}