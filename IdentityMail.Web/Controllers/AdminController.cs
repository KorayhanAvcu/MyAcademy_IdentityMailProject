using IdentityMail.Web.Context;
using IdentityMail.Web.DTOs.AdminDtos;
using IdentityMail.Web.Entities;
using IdentityMail.Web.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace IdentityMail.Web.Controllers
{
    // Sadece Admin rolündeki kullanıcılar erişebilir
    [Authorize(Roles = "Admin")]
    public class AdminController(
        UserManager<AppUser> _userManager,
        AppDbContext _context) : Controller
    {
        // User rolündeki kullanıcıların ID'lerini getirir
        private IQueryable<int> GetUserIds()
        {
            return _context.UserRoles
                .Where(x => x.RoleId == 2)
                .Select(x => x.UserId);
        }

        // Kategorilere göre mesaj istatistiklerini getirir
        private async Task<List<CategoryStatisticDto>> GetCategoryStatistics()
        {
            var userIds = GetUserIds();

            var totalMessageCount = await _context.UserMessages
                .CountAsync(x =>
                    x.IsDraft != true &&
                    x.CategoryId != null &&
                    userIds.Contains(x.SenderId));

            if (totalMessageCount == 0)
            {
                return new List<CategoryStatisticDto>();
            }

            var categories = await _context.UserMessages
                .Where(x =>
                    x.IsDraft != true &&
                    x.CategoryId != null &&
                    userIds.Contains(x.SenderId))
                .GroupBy(x => new
                {
                    x.CategoryId,
                    x.Category.CategoryName
                })
                .Select(g => new CategoryStatisticDto
                {
                    CategoryId = g.Key.CategoryId!.Value,
                    CategoryName = g.Key.CategoryName,
                    MessageCount = g.Count(),
                    Percentage = 0
                })
                .OrderByDescending(x => x.MessageCount)
                .ToListAsync();

            foreach (var category in categories)
            {
                category.Percentage =
                    (decimal)category.MessageCount /
                    totalMessageCount *
                    100;
            }

            return categories;
        }

        // En çok mesaj gönderen 5 kullanıcıyı getirir
        private async Task<List<TopSenderDto>> GetTopSenders()
        {
            var userIds = GetUserIds();

            var topSenders = await _context.UserMessages
                .Where(x =>
                    x.IsDraft != true &&
                    x.SenderId != 0 &&
                    userIds.Contains(x.SenderId))
                .GroupBy(x => new
                {
                    x.SenderId,
                    x.Sender.FirstName,
                    x.Sender.LastName,
                    x.Sender.Email,
                    x.Sender.ProfileImageUrl
                })
                .Select(g => new TopSenderDto
                {
                    UserId = g.Key.SenderId,
                    FullName =
                        g.Key.FirstName + " " +
                        g.Key.LastName,
                    Email = g.Key.Email,
                    ProfileImageUrl = g.Key.ProfileImageUrl,
                    SentMessageCount = g.Count()
                })
                .OrderByDescending(x => x.SentMessageCount)
                .Take(5)
                .ToListAsync();

            return topSenders;
        }

        // Admin dashboard ana sayfası
        public async Task<IActionResult> Index()
        {
            var userIds = GetUserIds();

            // Toplam kullanıcı sayısı
            var totalUserCount =
                await userIds.CountAsync();

            // Aktif kullanıcı sayısı
            var activeUserCount =
                await _context.Users
                    .Where(x =>
                        x.IsActive &&
                        userIds.Contains(x.Id))
                    .CountAsync();

            // Toplam gönderilen mesaj sayısı
            var totalMessageCount =
                await _context.UserMessages
                    .CountAsync(x =>
                        x.IsDraft != true &&
                        userIds.Contains(x.SenderId));

            // Bugün gönderilen mesaj sayısı
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var todayMessageCount =
                await _context.UserMessages
                    .CountAsync(x =>
                        x.SendDate >= today &&
                        x.SendDate < tomorrow &&
                        x.IsDraft != true &&
                        userIds.Contains(x.SenderId));

            // Okunmamış mesaj sayısı
            var unreadMessageCount =
                await _context.UserMessages
                    .CountAsync(x =>
                        x.IsRead == false &&
                        x.IsDraft != true &&
                        userIds.Contains(x.SenderId));

            // Çöpteki mesaj sayısı
            var deletedMessageCount =
                await _context.UserMessages
                    .CountAsync(x =>
                        x.IsDelete == true &&
                        x.IsDraft != true &&
                        userIds.Contains(x.SenderId));

            // Son 7 günlük mesaj grafiği
            var dailyMessages =
                await GetLast7DaysMessages();

            // Kategori istatistikleri
            var categories =
                await GetCategoryStatistics();

            // En çok mesaj gönderen kullanıcılar
            var topSenders =
                await GetTopSenders();

            // Dashboard DTO'sunu oluşturur
            var model = new DashboardDto
            {
                TotalUserCount = totalUserCount,
                ActiveUserCount = activeUserCount,
                TotalMessageCount = totalMessageCount,
                TodayMessageCount = todayMessageCount,
                UnreadMessageCount = unreadMessageCount,
                DeletedMessageCount = deletedMessageCount,
                DailyMessages = dailyMessages,
                Categories = categories,
                TopSenders = topSenders
            };

            return View(model);
        }

        // Son 7 gündeki mesaj sayılarını getirir
        private async Task<List<DailyMessageDto>> GetLast7DaysMessages()
        {
            var userIds = GetUserIds();

            var today = DateTime.Today;
            var startDate = today.AddDays(-6);
            var tomorrow = today.AddDays(1);

            var messages = await _context.UserMessages
                .Where(x =>
                    x.SendDate >= startDate &&
                    x.SendDate < tomorrow &&
                    x.IsDraft != true &&
                    userIds.Contains(x.SenderId))
                .ToListAsync();

            var result = Enumerable
                .Range(0, 7)
                .Select(i =>
                {
                    var date = startDate.AddDays(i);

                    return new DailyMessageDto
                    {
                        Label = date.DayOfWeek switch
                        {
                            DayOfWeek.Monday => "Pzt",
                            DayOfWeek.Tuesday => "Sal",
                            DayOfWeek.Wednesday => "Çar",
                            DayOfWeek.Thursday => "Per",
                            DayOfWeek.Friday => "Cum",
                            DayOfWeek.Saturday => "Cmt",
                            DayOfWeek.Sunday => "Paz",
                            _ => ""
                        },

                        Count = messages.Count(x =>
                            x.SendDate.Date == date.Date)
                    };
                })
                .ToList();

            return result;
        }

        // Bu ayın günlük mesaj sayılarını getirir
        private async Task<List<DailyMessageDto>> GetThisMonthMessages()
        {
            var userIds = GetUserIds();

            var today = DateTime.Today;

            var startDate = new DateTime(
                today.Year,
                today.Month,
                1);

            var endDate = startDate.AddMonths(1);

            var messages = await _context.UserMessages
                .Where(x =>
                    x.SendDate >= startDate &&
                    x.SendDate < endDate &&
                    x.IsDraft != true &&
                    userIds.Contains(x.SenderId))
                .ToListAsync();

            var daysInMonth =
                DateTime.DaysInMonth(
                    today.Year,
                    today.Month);

            var result = Enumerable
                .Range(1, daysInMonth)
                .Select(day =>
                {
                    var date = new DateTime(
                        today.Year,
                        today.Month,
                        day);

                    return new DailyMessageDto
                    {
                        Label = day.ToString(),

                        Count = messages.Count(x =>
                            x.SendDate.Date == date.Date)
                    };
                })
                .ToList();

            return result;
        }

        // Bu yılın aylık mesaj sayılarını getirir
        private async Task<List<DailyMessageDto>> GetThisYearMessages()
        {
            var userIds = GetUserIds();

            var today = DateTime.Today;

            var startDate = new DateTime(
                today.Year,
                1,
                1);

            var endDate = startDate.AddYears(1);

            var messages = await _context.UserMessages
                .Where(x =>
                    x.SendDate >= startDate &&
                    x.SendDate < endDate &&
                    x.IsDraft != true &&
                    userIds.Contains(x.SenderId))
                .ToListAsync();

            var result = Enumerable
                .Range(1, 12)
                .Select(month =>
                {
                    var monthName =
                        new DateTime(
                            today.Year,
                            month,
                            1)
                        .ToString(
                            "MMM",
                            new CultureInfo("tr-TR"));

                    var messageCount =
                        messages.Count(x =>
                            x.SendDate.Month == month);

                    return new DailyMessageDto
                    {
                        Label = monthName,
                        Count = messageCount
                    };
                })
                .ToList();

            return result;
        }

        // Grafik için seçilen tarih aralığını getirir
        [HttpPost]
        public async Task<IActionResult> MessageChart(
            MessageChartFilter filter)
        {
            List<DailyMessageDto> dailyMessages;

            switch (filter)
            {
                // Son 7 gün
                case MessageChartFilter.Last7Days:
                    dailyMessages =
                        await GetLast7DaysMessages();
                    break;

                // Bu ay
                case MessageChartFilter.ThisMonth:
                    dailyMessages =
                        await GetThisMonthMessages();
                    break;

                // Bu yıl
                case MessageChartFilter.ThisYear:
                    dailyMessages =
                        await GetThisYearMessages();
                    break;

                // Varsayılan: Son 7 gün
                default:
                    dailyMessages =
                        await GetLast7DaysMessages();
                    break;
            }

            return PartialView(
                "_DailyMessageChart",
                dailyMessages);
        }

        // Sistemdeki kullanıcıları listeler
        public async Task<IActionResult> UsersList()
        {
            var users = await _userManager.Users.ToListAsync();

            var userList = new List<UserListDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                userList.Add(new UserListDto
                {
                    Id = user.Id,
                    FullName =
                        user.FirstName + " " +
                        user.LastName,
                    Email = user.Email,
                    IsActive = user.IsActive,
                    EmailConfirmed = user.EmailConfirmed,
                    Roles = roles
                });
            }

            // Adminleri üstte gösterir
            userList = userList
                .OrderByDescending(x => x.Roles.Contains("Admin"))
                .ThenBy(x => x.FullName)
                .ToList();

            return View(userList);
        }

        // Kullanıcı düzenleme sayfası
        public async Task<IActionResult> UserEdit(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());

            var roles = await _userManager.GetRolesAsync(user);

            var model = new UserEditDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                Role = roles.FirstOrDefault()
            };

            return View(model);
        }

        // Kullanıcı bilgilerini ve rolünü günceller
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(UserEditDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id.ToString());

            if (user == null)
            {
                return NotFound();
            }

            // Kullanıcı bilgilerini günceller
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.UserName = model.Email;

            // Aktiflik durumunu günceller
            user.IsActive = model.IsActive;

            // Email onay durumunu günceller
            user.EmailConfirmed = model.EmailConfirmed;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }

            // Mevcut rolleri kaldırır
            var currentRoles = await _userManager.GetRolesAsync(user);

            if (currentRoles.Any())
            {
                var removeResult =
                    await _userManager.RemoveFromRolesAsync(
                        user,
                        currentRoles);

                if (!removeResult.Succeeded)
                {
                    foreach (var error in removeResult.Errors)
                    {
                        ModelState.AddModelError(
                            "",
                            error.Description);
                    }

                    return View(model);
                }
            }

            // Yeni rolü ekler
            if (!string.IsNullOrEmpty(model.Role))
            {
                var addResult =
                    await _userManager.AddToRoleAsync(
                        user,
                        model.Role);

                if (!addResult.Succeeded)
                {
                    foreach (var error in addResult.Errors)
                    {
                        ModelState.AddModelError(
                            "",
                            error.Description);
                    }

                    return View(model);
                }
            }

            return RedirectToAction("UsersList");
        }

        // Mesaj şikayetlerini listeler
        public async Task<IActionResult> MessageReports()
        {
            var reports = await _context.MessageReports
                .Include(x => x.Message)
                .Include(x => x.Reporter)
                .Include(x => x.ReviewedBy)
                .OrderBy(x => x.Status)
                .ThenByDescending(x => x.CreatedDate)
                .Select(x => new MessageReportDto
                {
                    Id = x.Id,
                    MessageId = x.MessageId,
                    Subject = x.Message.Subject,

                    ReporterName =
                        x.Reporter.FirstName + " " +
                        x.Reporter.LastName,

                    ReporterEmail = x.Reporter.Email,

                    SenderName =
                        x.Message.Sender.FirstName + " " +
                        x.Message.Sender.LastName,

                    Reason = x.Reason,
                    Description = x.Description,
                    Status = x.Status,
                    CreatedDate = x.CreatedDate,
                    AdminNote = x.AdminNote
                })
                .ToListAsync();

            return View(reports);
        }

        // Şikayet detayını getirir
        [HttpGet]
        public async Task<IActionResult> MessageReportDetail(int id)
        {
            var report = await _context.MessageReports
                .Include(x => x.Message)
                    .ThenInclude(x => x.Sender)
                .Include(x => x.Reporter)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (report == null)
                return NotFound();

            var model = new MessageReportDetailDto
            {
                Id = report.Id,
                MessageId = report.MessageId,
                Subject = report.Message.Subject,
                Body = report.Message.Body,

                ReporterName =
                    report.Reporter.FirstName + " " +
                    report.Reporter.LastName,

                ReporterEmail = report.Reporter.Email,

                SenderName =
                    report.Message.Sender.FirstName + " " +
                    report.Message.Sender.LastName,

                SenderEmail = report.Message.Sender.Email,

                Reason = report.Reason,
                Description = report.Description,
                Status = report.Status,
                CreatedDate = report.CreatedDate,
                AdminNote = report.AdminNote,
                ReviewedDate = report.ReviewedDate
            };

            return View(model);
        }

        // Şikayet durumunu günceller
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateMessageReport(
            int id,
            ReportStatus status,
            string? adminNote)
        {
            var report = await _context.MessageReports
                .FirstOrDefaultAsync(x => x.Id == id);

            if (report == null)
                return NotFound();

            var admin = await _userManager.GetUserAsync(User);

            if (admin == null)
                return RedirectToAction("Login", "Auth");

            report.Status = status;
            report.AdminNote = adminNote;
            report.ReviewedById = admin.Id;
            report.ReviewedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["ReportSuccess"] =
                "Şikayet durumu başarıyla güncellendi.";

            return RedirectToAction("MessageReports");
        }
    }
}