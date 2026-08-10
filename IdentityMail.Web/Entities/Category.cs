using System.ComponentModel.DataAnnotations;

namespace IdentityMail.Web.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string CategoryName { get; set; }
        public int? UserId { get; set; }
        public AppUser? User { get; set; }

    }
}
