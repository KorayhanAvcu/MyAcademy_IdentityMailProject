using IdentityMail.Web.Enums;

namespace IdentityMail.Web.Entities
{
    public class MessageReport
    {
        public int Id { get; set; }

        // Şikayet edilen mesaj
        public int MessageId { get; set; }
        public UserMessage Message { get; set; }

        // Şikayet eden kullanıcı
        public int ReporterId { get; set; }
        public AppUser Reporter { get; set; }

        // Şikayet nedeni
        public string Reason { get; set; }

        // Kullanıcının açıklaması
        public string? Description { get; set; }

        // Bekliyor / İnceleniyor / Çözüldü / Reddedildi
        public ReportStatus Status { get; set; }

        // Admin'in yazdığı not
        public string? AdminNote { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? ReviewedDate { get; set; }

        // Şikayeti inceleyen admin
        public int? ReviewedById { get; set; }
        public AppUser? ReviewedBy { get; set; }
    }
}
