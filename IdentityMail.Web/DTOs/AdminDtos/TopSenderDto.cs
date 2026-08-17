namespace IdentityMail.Web.DTOs.AdminDtos
{
    public class TopSenderDto
    {
        public int UserId { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string? ProfileImageUrl { get; set; }

        public int SentMessageCount { get; set; }
    }
}
