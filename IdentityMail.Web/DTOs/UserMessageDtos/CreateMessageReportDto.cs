namespace IdentityMail.Web.DTOs.UserMessageDtos
{
    public class CreateMessageReportDto
    {
        public int MessageId { get; set; }

        public string Reason { get; set; }

        public string? Description { get; set; }
    }
}
