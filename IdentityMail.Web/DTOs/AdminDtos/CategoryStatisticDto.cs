namespace IdentityMail.Web.DTOs.AdminDtos
{
    public class CategoryStatisticDto
    {
        public int CategoryId { get; set; }

        public string CategoryName { get; set; }

        public int MessageCount { get; set; }

        public decimal Percentage { get; set; }
    }
}
