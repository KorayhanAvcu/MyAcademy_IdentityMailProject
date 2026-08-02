using System.ComponentModel.DataAnnotations;

namespace IdentityMail.Web.DTOs.UserMessageDtos
{
    public class EditProfileDto
    {
        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        public string LastName { get; set; }
        public string? ProfileImageUrl { get; set; }
    }
}
