
using System.ComponentModel.DataAnnotations;

namespace IdentityMail.Web.DTOs.UserDtos
{
    public class ResetPasswordDto
    {
        public string UserId { get; set; }

        public string Token { get; set; }

        [Required(ErrorMessage = "Yeni şifre zorunludur.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Yeni şifre tekrar zorunludur.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Şifreler birbiriyle uyuşmuyor.")]
        public string ConfirmPassword { get; set; }
    }
}

