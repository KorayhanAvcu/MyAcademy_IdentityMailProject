namespace IdentityMail.Web.DTOs.UserMessageDtos
{
    public class SendMailDto
    {
        public int? DraftId { get; set; }

        public int? Id { get; set; }

        public string? ReceiverMail { get; set; }

        public int? CategoryId { get; set; }

        public string? Subject { get; set; }

        public string? Body { get; set; }
        public int? ConversationId { get; set; }
    }
}
