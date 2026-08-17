namespace IdentityMail.Web.DTOs.AdminDtos
{
    public class DashboardDto
    {
        // Kartlar
        public int TotalUserCount { get; set; }
        public int ActiveUserCount { get; set; }
        public int TotalMessageCount { get; set; }
        public int TodayMessageCount { get; set; }
        public int UnreadMessageCount { get; set; }
        public int DeletedMessageCount { get; set; }

        // Grafikler
        public List<DailyMessageDto> DailyMessages { get; set; }

        public List<CategoryStatisticDto> Categories { get; set; }

        // En çok mesaj atanlar
        public List<TopSenderDto> TopSenders { get; set; }
    }
}
