using IdentityMail.Web.Enums;

namespace IdentityMail.Web.DTOs.AdminDtos
{
    public class MessageReportDetailDto
    {
        public int Id { get; set; }

        public int MessageId { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        public string ReporterName { get; set; }

        public string ReporterEmail { get; set; }

        public string SenderName { get; set; }

        public string SenderEmail { get; set; }

        public string Reason { get; set; }

        public string? Description { get; set; }

        public ReportStatus Status { get; set; }

        public DateTime CreatedDate { get; set; }

        public string? AdminNote { get; set; }

        public DateTime? ReviewedDate { get; set; }
    }
}
