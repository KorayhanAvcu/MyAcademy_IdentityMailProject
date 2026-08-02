using IdentityMail.Web.DTOs.UserMessageDtos;
using IdentityMail.Web.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityMail.Web.Controllers
{
    public class UserController(UserManager<AppUser> _userManager) : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.FindByNameAsync(User.Identity!.Name);

            var userDatas = new EditProfileDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                ProfileImageUrl = user.ProfileImageUrl
            };

            return View(userDatas);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(EditProfileDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByNameAsync(User.Identity!.Name);

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.ProfileImageUrl = model.ProfileImageUrl;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return View(model);
            }

            return RedirectToAction("Edit");
        }
    }
}
