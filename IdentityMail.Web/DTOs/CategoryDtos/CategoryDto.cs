using System.ComponentModel.DataAnnotations;

namespace IdentityMail.Web.DTOs.CategoryDtos
{
    public class CategoryDto
    {
        
        [Required(ErrorMessage = "Kategory alanı zorunludur.")]
        public string CategoryName { get; set; }
    }
}
