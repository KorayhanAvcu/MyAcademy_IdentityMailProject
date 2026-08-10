using IdentityMail.Web.Entities;

namespace IdentityMail.Web.Models
{
    public class SidebarViewModel
    {
        public AppUser User { get; set; }

        public bool IsAdmin { get; set; }

        public bool IsUser { get; set; }
        public int UnreadCount { get; set; }
    }
}
