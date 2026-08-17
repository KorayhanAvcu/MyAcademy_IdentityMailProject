namespace IdentityMail.Web.DTOs.AdminDtos
{
    public class UserListDto
    {
        public int Id { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public bool IsActive { get; set; }

        public bool EmailConfirmed { get; set; }

        public IList<string> Roles { get; set; }
    }
}
