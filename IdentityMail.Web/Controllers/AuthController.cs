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
        // Kayıt sayfasını açar
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }


        // Yeni kullanıcı kaydı oluşturur
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            RegisterDto registerDto)
        {
            // Form doğrulaması
            if (!ModelState.IsValid)
            {
                return View(registerDto);
            }

            // Şifrelerin eşleşmesini kontrol eder
            if (registerDto.Password != registerDto.ConfirmPassword)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Şifreler birbiriyle uyumlu değil.");

                return View(registerDto);
            }


            // Yeni kullanıcı oluşturur
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


            // Kullanıcı oluşturma hatalarını gösterir
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


            // Kullanıcıya User rolünü verir
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


            // Email doğrulama tokenı oluşturur
            var token =
                await _userManager.GenerateEmailConfirmationTokenAsync(
                    user);


            // Email doğrulama linkini oluşturur
            var confirmationLink = Url.Action(
                "ConfirmEmail",
                "Auth",
                new
                {
                    userId = user.Id,
                    token = token
                },
                Request.Scheme);


            // Doğrulama email içeriğini oluşturur
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


            // Doğrulama emailini gönderir
            await _emailService.SendEmailAsync(
                user.Email!,
                "Identity Mail - Email Doğrulama",
                emailBody);


            // Email doğrulama bekleme sayfasına gönderir
            return RedirectToAction("EmailConfirmationPending");
        }


        // Email doğrulama bekleme sayfasını açar
        [HttpGet]
        public IActionResult EmailConfirmationPending()
        {
            return View();
        }


        // Email doğrulama işlemini gerçekleştirir
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(
            int userId,
            string token)
        {
            // Kullanıcıyı bulur
            var user = await _userManager.FindByIdAsync(
                userId.ToString());


            if (user == null)
            {
                return RedirectToAction("Login");
            }


            // Email doğrulama tokenını kontrol eder
            var result =
                await _userManager.ConfirmEmailAsync(
                    user,
                    token);


            if (!result.Succeeded)
            {
                return View("EmailConfirmationFailed");
            }


            // Email doğrulaması başarılı
            return View("EmailConfirmed");
        }


        // Login sayfasını açar
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        // Kullanıcı giriş işlemini gerçekleştirir
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginDto loginDto)
        {
            // Form doğrulaması
            if (!ModelState.IsValid)
            {
                return View(loginDto);
            }


            // Email adresine göre kullanıcıyı bulur
            var user = await _userManager.FindByEmailAsync(
                loginDto.Email);


            if (user == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Email veya şifre hatalı.");

                return View(loginDto);
            }


            // Hesabın aktif olup olmadığını kontrol eder
            if (!user.IsActive)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Hesabınız pasif durumdadır.");

                return View(loginDto);
            }


            // Kullanıcı adı ve şifre ile giriş yapar
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


            // Kullanıcının rolünü kontrol eder
            var roles = await _userManager.GetRolesAsync(user);


            // Admin kullanıcıyı Admin paneline gönderir
            if (roles.Contains(Roles.Admin))
            {
                return RedirectToAction("Index", "Admin");
            }


            // User kullanıcıyı mesaj paneline gönderir
            if (roles.Contains(Roles.User))
            {
                return RedirectToAction("Index", "Message");
            }


            // Geçersiz rol varsa oturumu kapatır
            await _signInManager.SignOutAsync();

            ModelState.AddModelError(
                string.Empty,
                "Kullanıcınıza atanmış geçerli bir rol bulunamadı.");

            return View(loginDto);
        }


        // Kullanıcının oturumunu kapatır
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Login");
        }


        // Şifremi unuttum sayfasını açar
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }


        // Şifre sıfırlama emaili gönderir
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(
            ForgotPasswordDto model)
        {
            // Form doğrulaması
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // Email adresine göre kullanıcıyı bulur
            var user = await _userManager.FindByEmailAsync(
                model.Email);


            // Kullanıcı bulunamazsa bilgi vermeden devam eder
            if (user == null)
            {
                return RedirectToAction(
                    "ForgotPasswordConfirmation");
            }


            // Şifre sıfırlama tokenı oluşturur
            var token =
                await _userManager.GeneratePasswordResetTokenAsync(
                    user);


            // Şifre sıfırlama linkini oluşturur
            var resetLink = Url.Action(
                "ResetPassword",
                "Auth",
                new
                {
                    userId = user.Id,
                    token = token
                },
                Request.Scheme);


            // Şifre sıfırlama email içeriğini oluşturur
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


            // Şifre sıfırlama emailini gönderir
            await _emailService.SendEmailAsync(
                user.Email!,
                "Identity Mail - Şifre Sıfırlama",
                emailBody);


            return RedirectToAction(
                "ForgotPasswordConfirmation");
        }


        // Şifre sıfırlama sonucu sayfasını açar
        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }


        // Şifre sıfırlama sayfasını açar
        [HttpGet]
        public IActionResult ResetPassword(
            string userId,
            string token)
        {
            // Gerekli bilgiler var mı kontrol eder
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


        // Yeni şifreyi kaydeder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(
            ResetPasswordDto model)
        {
            // Form doğrulaması
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // Kullanıcıyı bulur
            var user = await _userManager.FindByIdAsync(
                model.UserId);


            if (user == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Geçersiz şifre sıfırlama isteği.");

                return View(model);
            }


            // Tokenı kontrol ederek yeni şifreyi kaydeder
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


        // Şifre değiştirme sayfasını açar
        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }


        // Giriş yapan kullanıcının şifresini değiştirir
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(
            UserPasswordChangeDto model)
        {
            // Form doğrulaması
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // Giriş yapan kullanıcıyı bulur
            var user = await _userManager.GetUserAsync(User);


            if (user == null)
            {
                return RedirectToAction("Login");
            }


            // Mevcut şifreyi kontrol ederek yeni şifreyi kaydeder
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


            // Yeni şifre ile oturumu yeniler
            await _signInManager.RefreshSignInAsync(user);


            return RedirectToAction(
                "Index",
                "Message");
        }


        // Yetkisiz erişimde kullanıcıyı kendi paneline yönlendirir
        [HttpGet]
        public IActionResult AccessDenied()
        {
            // Admin kullanıcıyı Admin paneline gönderir
            if (User.IsInRole(Roles.Admin))
            {
                return RedirectToAction(
                    "Index",
                    "Admin");
            }


            // User kullanıcıyı mesaj paneline gönderir
            if (User.IsInRole(Roles.User))
            {
                return RedirectToAction(
                    "Index",
                    "Message");
            }


            // Rolü olmayan kullanıcıyı Login sayfasına gönderir
            return RedirectToAction(
                "Login",
                "Auth");
        }
    }
}