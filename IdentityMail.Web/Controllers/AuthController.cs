using IdentityMail.Web.Constants;
using IdentityMail.Web.DTOs.UserDtos;
using IdentityMail.Web.Entities;
using IdentityMail.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityMail.Web.Controllers
{
    public class AuthController(
        UserManager<AppUser> _userManager,
        SignInManager<AppUser> _signInManager,
        IEmailService _emailService) : Controller
    {
        // =====================================================
        // REGISTER
        // =====================================================

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(
            RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return View(registerDto);
            }

            if (registerDto.Password != registerDto.ConfirmPassword)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Şifreler birbiriyle uyumlu değil.");

                return View(registerDto);
            }

            var user = new AppUser
            {
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                UserName = registerDto.UserName,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(
                user,
                registerDto.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        error.Code,
                        error.Description);
                }

                return View(registerDto);
            }

            var roleResult = await _userManager.AddToRoleAsync(
                user,
                Roles.User);

            if (!roleResult.Succeeded)
            {
                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(
                        error.Code,
                        error.Description);
                }

                return View(registerDto);
            }

            return RedirectToAction("Login");
        }

        // =====================================================
        // LOGIN
        // =====================================================

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(
            LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return View(loginDto);
            }

            var user = await _userManager.FindByEmailAsync(
                loginDto.Email);

            if (user == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Bu email sistemde kayıtlı değil.");

                return View(loginDto);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user,
                loginDto.Password,
                false,
                false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Email veya şifre hatalı.");

                return View(loginDto);
            }

            return RedirectToAction("Index", "Message");
        }

        // =====================================================
        // LOGOUT
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Login");
        }

        // =====================================================
        // FORGOT PASSWORD - GET
        // =====================================================

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // =====================================================
        // FORGOT PASSWORD - POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(
            ForgotPasswordDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(
                model.Email);

            // Güvenlik nedeniyle kullanıcı bulunamadığında
            // yine confirmation sayfasına gönderiyoruz.
            if (user == null)
            {
                return RedirectToAction(
                    "ForgotPasswordConfirmation");
            }

            // Identity password reset token oluşturur.
            var token =
                await _userManager.GeneratePasswordResetTokenAsync(
                    user);

            // Token'ı doğrudan URL parametresi olarak gönderiyoruz.
            var resetLink = Url.Action(
                "ResetPassword",
                "Auth",
                new
                {
                    userId = user.Id,
                    token = token
                },
                Request.Scheme);

            var emailBody =
                "<h2>Identity Mail</h2>" +

                "<p>Merhaba,</p>" +

                "<p>" +
                "Şifrenizi sıfırlamak için aşağıdaki " +
                "butona tıklayın:" +
                "</p>" +

                "<p>" +

                "<a href=\"" +
                resetLink +
                "\" " +

                "style=\"" +
                "display:inline-block;" +
                "padding:10px 20px;" +
                "background-color:#0d6efd;" +
                "color:white;" +
                "text-decoration:none;" +
                "border-radius:6px;" +
                "\">" +

                "Şifremi Sıfırla" +

                "</a>" +

                "</p>" +

                "<p>" +
                "Bu bağlantı şifrenizi sıfırlamak için " +
                "kullanılacaktır." +
                "</p>" +

                "<p>" +
                "Eğer bu işlemi siz başlatmadıysanız, " +
                "bu maili dikkate almayabilirsiniz." +
                "</p>" +

                "<p>" +
                "Identity Mail" +
                "</p>";

            await _emailService.SendEmailAsync(
                user.Email!,
                "Identity Mail - Şifre Sıfırlama",
                emailBody);

            return RedirectToAction(
                "ForgotPasswordConfirmation");
        }

        // =====================================================
        // FORGOT PASSWORD CONFIRMATION
        // =====================================================

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        // =====================================================
        // RESET PASSWORD - GET
        // =====================================================

        [HttpGet]
        public IActionResult ResetPassword(
            string userId,
            string token)
        {
            if (string.IsNullOrEmpty(userId) ||
                string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login");
            }

            var model = new ResetPasswordDto
            {
                UserId = userId,
                Token = token
            };

            return View(model);
        }

        // =====================================================
        // RESET PASSWORD - POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(
            ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(
                model.UserId);

            if (user == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Geçersiz şifre sıfırlama isteği.");

                return View(model);
            }

            var result =
                await _userManager.ResetPasswordAsync(
                    user,
                    model.Token,
                    model.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View(model);
            }

            return RedirectToAction("Login");
        }

        // =====================================================
        // CHANGE PASSWORD
        // =====================================================

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(
            UserPasswordChangeDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var result =
                await _userManager.ChangePasswordAsync(
                    user,
                    model.CurrentPassword,
                    model.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);

            return RedirectToAction(
                "Index",
                "Message");
        }
    }
}

