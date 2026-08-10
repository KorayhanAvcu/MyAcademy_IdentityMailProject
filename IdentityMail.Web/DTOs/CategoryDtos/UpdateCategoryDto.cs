using System.ComponentModel.DataAnnotations;

namespace IdentityMail.Web.DTOs.CategoryDtos
{
    public class UpdateCategoryDto
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Kategory alanı zorunludur.")]
        public string CategoryName { get; set; }
    }
}
