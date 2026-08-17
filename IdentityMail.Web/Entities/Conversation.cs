namespace IdentityMail.Web.Entities
{
    public class Conversation
    {
        public int Id { get; set; }

        public string Subject { get; set; }

        public DateTime CreatedDate { get; set; }

        public List<UserMessage> Messages { get; set; } = new();
    }
}
