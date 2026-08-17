namespace IdentityMail.Web.DTOs.AdminDtos
{
    public class UserEditDto
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public bool IsActive { get; set; }

        public bool EmailConfirmed { get; set; }

        public string Role { get; set; }
    }
}
