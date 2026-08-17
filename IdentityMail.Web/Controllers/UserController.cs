using IdentityMail.Web.DTOs.UserMessageDtos;
using IdentityMail.Web.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityMail.Web.Controllers
{
    // Kullanıcı profil işlemlerini yönetir.
    public class UserController(UserManager<AppUser> _userManager) : Controller
    {
        // Kullanıcı ana sayfasını gösterir.
        public IActionResult Index()
        {
            return View();
        }

        // Profil düzenleme sayfasını açar.
        public async Task<IActionResult> Edit()
        {
            // Giriş yapan kullanıcıyı bul.
            var user = await _userManager.FindByNameAsync(User.Identity!.Name);

            // Kullanıcı bilgilerini DTO'ya aktar.
            var userDatas = new EditProfileDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                ProfileImageUrl = user.ProfileImageUrl
            };

            return View(userDatas);
        }

        // Profil bilgilerini günceller.
        [HttpPost]
        public async Task<IActionResult> Edit(EditProfileDto model)
        {
            // Form doğrulaması başarısızsa sayfaya geri dön.
            if (!ModelState.IsValid)
                return View(model);

            // Giriş yapan kullanıcıyı bul.
            var user = await _userManager.FindByNameAsync(User.Identity!.Name);

            // Yeni profil bilgilerini kullanıcıya aktar.
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.ProfileImageUrl = model.ProfileImageUrl;

            // Kullanıcı bilgilerini veritabanında güncelle.
            var result = await _userManager.UpdateAsync(user);

            // Güncelleme başarısızsa hataları göster.
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return View(model);
            }

            // Güncelleme başarılıysa profil sayfasına dön.
            return RedirectToAction("Edit");
        }
    }
}