namespace IdentityMail.Web.DTOs.UserMessageDtos
{
    public class MessageFilterDto
    {
        // Gelen Kutusu'nda gönderen adı/email'i, Gönderilenler'de alıcı adı/email'i arar
        public string? SearchTerm { get; set; }

        public string? Subject { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public int? CategoryId { get; set; }

        public bool? IsRead { get; set; }
        public bool? IsImportant { get; set; }

        public string SortOrder { get; set; } = "desc"; // "asc" | "desc"

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
