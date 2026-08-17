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
        // REGISTER - GET
        // =====================================================

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }


        // =====================================================
        // REGISTER - POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
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


            // =================================================
            // USER OLUŞTUR
            // =================================================

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


            // =================================================
            // USER ROLE
            // =================================================

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


            // =================================================
            // EMAIL CONFIRMATION TOKEN
            // =================================================

            var token =
                await _userManager.GenerateEmailConfirmationTokenAsync(
                    user);


            // =================================================
            // CONFIRMATION LINK
            // =================================================

            var confirmationLink = Url.Action(
                "ConfirmEmail",
                "Auth",
                new
                {
                    userId = user.Id,
                    token = token
                },
                Request.Scheme);


            // =================================================
            // EMAIL BODY
            // =================================================

            var emailBody =
                "<h2>Identity Mail</h2>" +

                "<p>Merhaba " +
                user.FirstName +
                ",</p>" +

                "<p>" +
                "Identity Mail hesabınızı oluşturduğunuz için " +
                "teşekkür ederiz." +
                "</p>" +

                "<p>" +
                "Hesabınızı kullanabilmek için email adresinizi " +
                "doğrulamanız gerekiyor." +
                "</p>" +

                "<p>" +

                "<a href=\"" +
                confirmationLink +
                "\"" +

                " style=\"" +
                "display:inline-block;" +
                "padding:12px 20px;" +
                "background-color:#0d6efd;" +
                "color:white;" +
                "text-decoration:none;" +
                "border-radius:6px;" +
                "\">" +

                "Email Adresimi Doğrula" +

                "</a>" +

                "</p>" +

                "<p>" +
                "Bu bağlantıya tıklayarak email adresinizi " +
                "doğrulayabilirsiniz." +
                "</p>" +

                "<p>" +
                "Eğer bu hesabı siz oluşturmadıysanız " +
                "bu emaili dikkate almayabilirsiniz." +
                "</p>" +

                "<p>Identity Mail</p>";


            // =================================================
            // EMAIL GÖNDER
            // =================================================

            await _emailService.SendEmailAsync(
                user.Email!,
                "Identity Mail - Email Doğrulama",
                emailBody);


            // Kullanıcıya direkt login yaptırmıyoruz.
            // Önce emailini doğrulaması gerekiyor.
            return RedirectToAction("EmailConfirmationPending");
        }
        [HttpGet]
        public IActionResult EmailConfirmationPending()
        {
            return View();
        }

        // =====================================================
        // CONFIRM EMAIL
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(
            int userId,
            string token)
        {
            // Kullanıcıyı bul
            var user = await _userManager.FindByIdAsync(
                userId.ToString());


            if (user == null)
            {
                return RedirectToAction("Login");
            }


            // Identity tokenı kontrol eder.
            var result =
                await _userManager.ConfirmEmailAsync(
                    user,
                    token);


            if (!result.Succeeded)
            {
                return View("EmailConfirmationFailed");
            }


            // Burada EmailConfirmed artık true olur.
            return View("EmailConfirmed");
        }


        // =====================================================
        // LOGIN - GET
        // =====================================================

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        // =====================================================
        // LOGIN - POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return View(loginDto);
            }


            // =================================================
            // USER BUL
            // =================================================

            var user = await _userManager.FindByEmailAsync(
                loginDto.Email);


            if (user == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Email veya şifre hatalı.");

                return View(loginDto);
            }


            // =================================================
            // IS ACTIVE KONTROLÜ
            // =================================================

            if (!user.IsActive)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Hesabınız pasif durumdadır.");

                return View(loginDto);
            }


            // =================================================
            // LOGIN
            // =================================================

            // EmailConfirmed kontrolünü Identity kendisi yapacak.
            //
            // Çünkü Program.cs içerisinde:
            //
            // options.SignIn.RequireConfirmedEmail = true;
            //
            // olarak ayarladık.

            var result =
                await _signInManager.PasswordSignInAsync(
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


            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains(Roles.Admin))
            {
                return RedirectToAction("Index", "Admin");
            }

            if (roles.Contains(Roles.User))
            {
                return RedirectToAction("Index", "Message");
            }

            await _signInManager.SignOutAsync();

            ModelState.AddModelError(
                string.Empty,
                "Kullanıcınıza atanmış geçerli bir rol bulunamadı.");

            return View(loginDto);
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

            if (user == null)
            {
                return RedirectToAction(
                    "ForgotPasswordConfirmation");
            }


            var token =
                await _userManager.GeneratePasswordResetTokenAsync(
                    user);


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
                "\"" +

                " style=\"" +
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
                "Eğer bu işlemi siz başlatmadıysanız, " +
                "bu maili dikkate almayabilirsiniz." +
                "</p>" +

                "<p>Identity Mail</p>";


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
        // CHANGE PASSWORD - GET
        // =====================================================

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }


        // =====================================================
        // CHANGE PASSWORD - POST
        // =====================================================

        [Authorize]
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

        [HttpGet]
        public IActionResult AccessDenied()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }

            if (User.IsInRole("User"))
            {
                return RedirectToAction("Index", "Message");
            }

            return RedirectToAction("Login", "Auth");
        }
    }
}