using System.ComponentModel.DataAnnotations;

namespace IdentityMail.Web.DTOs.UserDtos
{
    public class UserPasswordChangeDto
    {
        [Required(ErrorMessage = "Mevcut şifre zorunludur.")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "Yeni şifre zorunludur.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Yeni şifre tekrarı zorunludur.")]
        [Compare("NewPassword", ErrorMessage = "Yeni şifreler eşleşmiyor.")]
        [DataType(DataType.Password)]
        public string ConfirmNewPassword { get; set; }
    }
}
