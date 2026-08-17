using IdentityMail.Web.Context;
using IdentityMail.Web.DTOs.UserMessageDtos;
using IdentityMail.Web.Entities;
using IdentityMail.Web.Enums;
using IdentityMail.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IdentityMail.Web.Controllers
{
    // Sadece User rolündeki kullanıcılar erişebilir.
    [Authorize(Roles = "User")]
    public class MessageController(
        UserManager<AppUser> _userManager,
        AppDbContext _context) : Controller
    {
        // Gelen kutusunu listeler ve filtreleme işlemlerini yapar.
        public async Task<IActionResult> Index(MessageFilterDto filter)
        {
            var user = await _userManager.GetUserAsync(User);

            // Kullanıcı bulunamazsa login sayfasına gönder.
            if (user == null)
                return RedirectToAction("Login", "Auth");

            // Sayfalama değerlerini varsayılan değerlere çek.
            if (filter.Page < 1) filter.Page = 1;
            if (filter.PageSize < 1) filter.PageSize = 10;

            // Kullanıcının gelen mesajlarını getir.
            var query = _context.UserMessages
                .Include(x => x.Sender)
                .Include(x => x.Category)
                .Where(x => x.ReceiverId == user.Id)
                .Where(x => x.IsDelete == false)
                .Where(x => x.IsDraft != true);

            // Gönderen adına veya email adresine göre ara.
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim().ToLower();

                query = query.Where(x =>
                    (x.Sender.FirstName + " " + x.Sender.LastName)
                        .ToLower()
                        .Contains(term) ||
                    x.Sender.Email.ToLower().Contains(term));
            }

            // Mesaj konusuna göre filtrele.
            if (!string.IsNullOrWhiteSpace(filter.Subject))
            {
                query = query.Where(x =>
                    x.Subject.Contains(filter.Subject));
            }

            // Başlangıç tarihine göre filtrele.
            if (filter.StartDate.HasValue)
            {
                query = query.Where(x =>
                    x.SendDate >= filter.StartDate.Value.Date);
            }

            // Bitiş tarihini de dahil ederek filtrele.
            if (filter.EndDate.HasValue)
            {
                var end = filter.EndDate.Value.Date.AddDays(1);

                query = query.Where(x =>
                    x.SendDate < end);
            }

            // Kategoriye göre filtrele.
            if (filter.CategoryId.HasValue)
            {
                query = query.Where(x =>
                    x.CategoryId == filter.CategoryId.Value);
            }

            // Okunmuş / okunmamış durumuna göre filtrele.
            if (filter.IsRead.HasValue)
            {
                query = query.Where(x =>
                    x.IsRead == filter.IsRead.Value);
            }

            // Önemli mesajlara göre filtrele.
            if (filter.IsImportant.HasValue)
            {
                query = query.Where(x =>
                    x.IsImportant == filter.IsImportant.Value);
            }

            // Mesajları tarihe göre sırala.
            query = filter.SortOrder == "asc"
                ? query.OrderBy(x => x.SendDate)
                : query.OrderByDescending(x => x.SendDate);

            // Toplam mesaj sayısını hesapla.
            var totalCount = await query.CountAsync();

            // İstenen sayfadaki mesajları getir.
            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            // Sayfalama sonucunu oluştur.
            var result = new PagedResult<UserMessage>
            {
                Items = items,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalCount = totalCount
            };

            // Kategorileri ve kullanıcı bilgilerini View'a gönder.
            await LoadCategories(user.Id);

            ViewBag.Filter = filter;
            ViewBag.fullName = $"{user.FirstName} {user.LastName}";

            return View(result);
        }


        // Önemli olarak işaretlenen mesajları listeler.
        public async Task<IActionResult> Important()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            var messages = await _context.UserMessages
                .Include(x => x.Sender)
                .Include(x => x.Category)
                .Where(x => x.ReceiverId == user.Id)
                .Where(x => x.IsImportant == true)
                .Where(x => x.IsDelete == false)
                .Where(x => x.IsDraft != true)
                .OrderByDescending(x => x.SendDate)
                .ToListAsync();

            return View(messages);
        }


        // Kullanıcının gönderdiği mesajları listeler.
        public async Task<IActionResult> SentMail(MessageFilterDto filter)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            // Sayfalama değerlerini kontrol et.
            if (filter.Page < 1) filter.Page = 1;
            if (filter.PageSize < 1) filter.PageSize = 10;

            // Kullanıcının gönderdiği mesajları getir.
            var query = _context.UserMessages
                .Include(x => x.Receiver)
                .Include(x => x.Category)
                .Where(x => x.SenderId == user.Id)
                .Where(x => x.IsDelete == false)
                .Where(x => x.IsDraft != true);

            // Alıcının adına veya email adresine göre ara.
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim().ToLower();

                query = query.Where(x =>
                    (x.Receiver.FirstName + " " + x.Receiver.LastName)
                        .ToLower()
                        .Contains(term) ||
                    x.Receiver.Email.ToLower().Contains(term));
            }

            // Konuya göre filtrele.
            if (!string.IsNullOrWhiteSpace(filter.Subject))
            {
                query = query.Where(x =>
                    x.Subject.Contains(filter.Subject));
            }

            // Tarih filtresi uygula.
            if (filter.StartDate.HasValue)
            {
                query = query.Where(x =>
                    x.SendDate >= filter.StartDate.Value.Date);
            }

            if (filter.EndDate.HasValue)
            {
                var end = filter.EndDate.Value.Date.AddDays(1);

                query = query.Where(x =>
                    x.SendDate < end);
            }

            // Kategoriye göre filtrele.
            if (filter.CategoryId.HasValue)
            {
                query = query.Where(x =>
                    x.CategoryId == filter.CategoryId.Value);
            }

            // Alıcının mesajı okuyup okumadığını filtrele.
            if (filter.IsRead.HasValue)
            {
                query = query.Where(x =>
                    x.IsRead == filter.IsRead.Value);
            }

            // Önemli mesaj filtresi uygula.
            if (filter.IsImportant.HasValue)
            {
                query = query.Where(x =>
                    x.IsImportant == filter.IsImportant.Value);
            }

            // Mesajları tarihe göre sırala.
            query = filter.SortOrder == "asc"
                ? query.OrderBy(x => x.SendDate)
                : query.OrderByDescending(x => x.SendDate);

            // Toplam mesaj sayısını bul.
            var totalCount = await query.CountAsync();

            // Sayfadaki mesajları getir.
            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var result = new PagedResult<UserMessage>
            {
                Items = items,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalCount = totalCount
            };

            await LoadCategories(user.Id);
            ViewBag.Filter = filter;

            return View(result);
        }


        // Yeni mail oluşturma sayfasını açar.
        [HttpGet]
        public async Task<IActionResult> SendMail()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            // Mail formunda kullanılacak kategorileri yükle.
            await LoadCategories(user.Id);

            return View(new SendMailDto());
        }


        // Yeni mail gönderir veya taslağı gönderir.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMail(SendMailDto sendMailDto)
        {
            var sender = await _userManager.GetUserAsync(User);

            if (sender == null)
                return RedirectToAction("Login", "Auth");

            // Form doğrulaması başarısızsa aynı sayfaya dön.
            if (!ModelState.IsValid)
            {
                await LoadCategories(sender.Id);
                return View(sendMailDto);
            }

            UserMessage? message = null;

            // ID varsa mevcut taslağı bul.
            if (sendMailDto.Id.HasValue)
            {
                message = await _context.UserMessages
                    .FirstOrDefaultAsync(x =>
                        x.Id == sendMailDto.Id.Value &&
                        x.SenderId == sender.Id &&
                        x.IsDraft == true);

                if (message == null)
                    return NotFound();
            }

            // Alıcı email adresinden kullanıcıyı bul.
            var receiver = await _userManager
                .FindByEmailAsync(sendMailDto.ReceiverMail!.Trim());

            // Alıcı sistemde yoksa hata göster.
            if (receiver == null)
            {
                ModelState.AddModelError(
                    nameof(sendMailDto.ReceiverMail),
                    "Girdiğiniz mail ile sistemde kayıtlı kullanıcı bulunamadı.");

                await LoadCategories(sender.Id);
                return View(sendMailDto);
            }

            // Yeni mesaj oluştur.
            if (message == null)
            {
                message = new UserMessage
                {
                    SenderId = sender.Id
                };

                _context.UserMessages.Add(message);
            }

            // Mesaj bilgilerini doldur.
            message.ReceiverId = receiver.Id;
            message.Subject = sendMailDto.Subject;
            message.Body = sendMailDto.Body;
            message.CategoryId = sendMailDto.CategoryId;
            message.SendDate = DateTime.Now;
            message.IsDraft = false;
            message.IsRead = false;
            message.IsImportant = false;
            message.IsDelete = false;

            await _context.SaveChangesAsync();

            // Mail gönderildikten sonra gelen kutusuna dön.
            return RedirectToAction("Index");
        }


        // Gelen mesaja cevap verme ekranını açar.
        [HttpGet]
        public async Task<IActionResult> Reply(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            // Cevap verilecek mesajı getir.
            var original = await _context.UserMessages
                .Include(x => x.Sender)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.ReceiverId == user.Id &&
                    x.IsDraft != true);

            if (original == null)
                return NotFound();

            // Cevap formunu orijinal mesaj bilgileriyle doldur.
            var model = new SendMailDto
            {
                ConversationId = original.ConversationId,

                ReceiverMail = original.Sender?.Email,

                Subject = original.Subject != null &&
                          original.Subject.StartsWith("Re:")
                    ? original.Subject
                    : $"Re: {original.Subject}",

                CategoryId = original.CategoryId,

                Body = $"\n\n---- Orijinal Mesaj ----\n" +
                       $"{original.Sender?.FirstName} " +
                       $"{original.Sender?.LastName} yazdı:\n" +
                       $"{original.Body}"
            };

            await LoadCategories(user.Id);

            return View("SendMail", model);
        }


        // Maili taslak olarak kaydeder veya mevcut taslağı günceller.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDraft(SendMailDto sendMailDto)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Json(new { success = false });

            UserMessage? draft = null;

            // ID varsa mevcut taslağı bul.
            if (sendMailDto.Id.HasValue)
            {
                draft = await _context.UserMessages
                    .FirstOrDefaultAsync(x =>
                        x.Id == sendMailDto.Id.Value &&
                        x.SenderId == user.Id &&
                        x.IsDraft == true);
            }

            // Taslak bulunamadıysa yeni taslak oluştur.
            if (draft == null)
            {
                draft = new UserMessage
                {
                    SenderId = user.Id,
                    SendDate = DateTime.Now,
                    IsDraft = true,
                    IsRead = false,
                    IsImportant = false,
                    IsDelete = false
                };

                _context.UserMessages.Add(draft);
            }

            // Taslak bilgilerini güncelle.
            draft.Subject = sendMailDto.Subject;
            draft.Body = sendMailDto.Body;
            draft.CategoryId = sendMailDto.CategoryId;

            // Alıcı girilmişse kullanıcıyı bul.
            if (!string.IsNullOrWhiteSpace(sendMailDto.ReceiverMail))
            {
                var receiver = await _userManager
                    .FindByEmailAsync(sendMailDto.ReceiverMail.Trim());

                if (receiver != null)
                    draft.ReceiverId = receiver.Id;
            }

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                id = draft.Id
            });
        }


        // Girilen email adresinin sistemde kayıtlı olup olmadığını kontrol eder.
        [HttpGet]
        public async Task<IActionResult> CheckReceiver(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Json(new { exists = false });

            var currentUser = await _userManager.GetUserAsync(User);

            // Email adresine göre kullanıcıyı bul.
            var receiver = await _userManager.FindByEmailAsync(email.Trim());

            if (receiver == null)
                return Json(new { exists = false });

            // Kullanıcının kendisine mail göndermesini kontrol et.
            if (currentUser != null && receiver.Id == currentUser.Id)
                return Json(new { exists = true, isSelf = true });

            return Json(new
            {
                exists = true,
                isSelf = false,
                name = $"{receiver.FirstName} {receiver.LastName}"
            });
        }


        // Kullanıcının taslak mesajlarını listeler.
        public async Task<IActionResult> Drafts()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            var drafts = await _context.UserMessages
                .Include(x => x.Receiver)
                .Where(x => x.SenderId == user.Id)
                .Where(x => x.IsDraft == true)
                .Where(x => x.IsDelete == false)
                .OrderByDescending(x => x.SendDate)
                .ToListAsync();

            return View(drafts);
        }


        // Seçilen taslağı düzenleme ekranında açar.
        [HttpGet]
        public async Task<IActionResult> EditDraft(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            // Taslağın kullanıcıya ait olduğunu kontrol et.
            var draft = await _context.UserMessages
                .Include(x => x.Receiver)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.SenderId == user.Id &&
                    x.IsDraft == true);

            if (draft == null)
                return NotFound();

            // Taslak bilgilerini forma aktar.
            var model = new SendMailDto
            {
                Id = draft.Id,
                ReceiverMail = draft.Receiver?.Email,
                Subject = draft.Subject,
                Body = draft.Body,
                CategoryId = draft.CategoryId
            };

            await LoadCategories(user.Id);

            return View("SendMail", model);
        }


        // Seçilen taslağı siler.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDraft(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            // Kullanıcıya ait taslağı bul.
            var draft = await _context.UserMessages
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.SenderId == user.Id &&
                    x.IsDraft == true);

            if (draft == null)
                return NotFound();

            _context.UserMessages.Remove(draft);

            await _context.SaveChangesAsync();

            return RedirectToAction("Drafts");
        }


        // Gelen mesajın detayını gösterir ve okundu olarak işaretler.
        public async Task<IActionResult> MailDetail(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            // Mesajın gerçekten kullanıcıya ait olduğunu kontrol et.
            var message = await _context.UserMessages
                .Include(x => x.Sender)
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.ReceiverId == user.Id &&
                    x.IsDraft != true);

            if (message == null)
                return NotFound();

            // Mesaj açıldığında okundu olarak işaretle.
            message.IsRead = true;

            await _context.SaveChangesAsync();

            return View(message);
        }


        // Gönderilen mesajın detayını gösterir.
        public async Task<IActionResult> SentMailDetail(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            // Mesajın kullanıcı tarafından gönderildiğini kontrol et.
            var message = await _context.UserMessages
                .Include(x => x.Sender)
                .Include(x => x.Receiver)
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.SenderId == user.Id &&
                    x.IsDraft != true);

            if (message == null)
                return NotFound();

            return View(message);
        }


        // Mesajı önemli olarak işaretler veya önemli durumdan çıkarır.
        public async Task<IActionResult> IsImportant(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            // Mesajın kullanıcıya ait olduğunu kontrol et.
            var message = await _context.UserMessages
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.ReceiverId == user.Id &&
                    x.IsDraft != true);

            if (message == null)
                return NotFound();

            // Mevcut önemli durumunun tersini al.
            message.IsImportant = !message.IsImportant;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }


        // Mesajı silmeden çöp kutusuna taşır.
        public async Task<IActionResult> MoveToTrash(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            // Mesajın gönderen veya alıcı olarak kullanıcıya ait olduğunu kontrol et.
            var message = await _context.UserMessages
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    (x.ReceiverId == user.Id || x.SenderId == user.Id) &&
                    x.IsDraft != true);

            if (message == null)
                return NotFound();

            // Soft delete uygula.
            message.IsDelete = true;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }


        // Çöp kutusundaki mesajı geri yükler.
        public async Task<IActionResult> RestoreMessage(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            // Kullanıcıya ait çöp mesajını bul.
            var message = await _context.UserMessages
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    (x.ReceiverId == user.Id || x.SenderId == user.Id));

            if (message == null)
                return NotFound();

            // Soft delete durumunu kaldır.
            message.IsDelete = false;

            await _context.SaveChangesAsync();

            return RedirectToAction("Trash");
        }


        // Kullanıcının çöp kutusundaki mesajları listeler.
        public async Task<IActionResult> Trash()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            var messages = await _context.UserMessages
                .Include(x => x.Sender)
                .Include(x => x.Receiver)
                .Where(x =>
                    (x.ReceiverId == user.Id || x.SenderId == user.Id) &&
                    x.IsDelete == true &&
                    x.IsDraft != true)
                .OrderByDescending(x => x.SendDate)
                .ToListAsync();

            return View(messages);
        }


        // Kullanıcıya ait ve genel kategorileri ViewBag'e yükler.
        private async Task LoadCategories(int userId)
        {
            ViewBag.Categories = await _context.Categories
                .Where(x => x.UserId == userId || x.UserId == null)
                .OrderBy(x => x.CategoryName)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.CategoryName
                })
                .ToListAsync();
        }


        // Mesaj şikayet formunu açar.
        [HttpGet]
        public async Task<IActionResult> Report(int id)
        {
            var userId = int.Parse(
                _userManager.GetUserId(User)!);

            // Şikayet edilecek mesajı getir.
            var message = await _context.UserMessages
                .Include(x => x.Sender)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.ReceiverId == userId &&
                    x.IsDraft != true &&
                    x.IsDelete != true);

            if (message == null)
                return NotFound();

            // Kullanıcı kendi mesajını şikayet edemez.
            if (message.SenderId == userId)
            {
                return BadRequest(
                    "Kendi mesajınızı şikayet edemezsiniz.");
            }

            // Aynı mesajın daha önce şikayet edilip edilmediğini kontrol et.
            var alreadyReported = await _context.MessageReports
                .AnyAsync(x =>
                    x.MessageId == id &&
                    x.ReporterId == userId);

            if (alreadyReported)
            {
                TempData["ReportError"] =
                    "Bu mesajı daha önce şikayet ettiniz.";

                return RedirectToAction(
                    "MailDetail",
                    new { id });
            }

            // Şikayet formu modelini oluştur.
            var model = new CreateMessageReportDto
            {
                MessageId = message.Id
            };

            ViewBag.Subject = message.Subject;

            ViewBag.SenderName =
                $"{message.Sender.FirstName} {message.Sender.LastName}";

            return View(model);
        }


        // Kullanıcının mesaj şikayetini kaydeder.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReportMessage(
            CreateMessageReportDto dto)
        {
            // Form doğrulaması başarısızsa hata mesajlarıyla geri dön.
            if (!ModelState.IsValid)
            {
                var messageForView = await _context.UserMessages
                    .Include(x => x.Sender)
                    .FirstOrDefaultAsync(x =>
                        x.Id == dto.MessageId);

                if (messageForView != null)
                {
                    ViewBag.Subject = messageForView.Subject;

                    ViewBag.SenderName =
                        $"{messageForView.Sender.FirstName} " +
                        $"{messageForView.Sender.LastName}";
                }

                return View("Report", dto);
            }

            var userId = int.Parse(
                _userManager.GetUserId(User)!);

            // Mesajın gerçekten bu kullanıcıya ait olduğunu kontrol et.
            var message = await _context.UserMessages
                .FirstOrDefaultAsync(x =>
                    x.Id == dto.MessageId &&
                    x.ReceiverId == userId &&
                    x.IsDraft != true &&
                    x.IsDelete != true);

            if (message == null)
                return NotFound();

            // Kullanıcı kendi mesajını şikayet edemez.
            if (message.SenderId == userId)
            {
                return BadRequest(
                    "Kendi mesajınızı şikayet edemezsiniz.");
            }

            // Aynı kullanıcı aynı mesajı tekrar şikayet edemez.
            var alreadyReported = await _context.MessageReports
                .AnyAsync(x =>
                    x.MessageId == dto.MessageId &&
                    x.ReporterId == userId);

            if (alreadyReported)
            {
                TempData["ReportError"] =
                    "Bu mesajı daha önce şikayet ettiniz.";

                return RedirectToAction(
                    "MailDetail",
                    new { id = dto.MessageId });
            }

            // Yeni şikayet kaydı oluştur.
            var report = new MessageReport
            {
                MessageId = dto.MessageId,
                ReporterId = userId,
                Reason = dto.Reason,
                Description = dto.Description,

                // Admin incelemesi bekleniyor.
                Status = ReportStatus.Pending,

                ReviewedById = null,
                AdminNote = null,

                CreatedDate = DateTime.Now,
                ReviewedDate = null
            };

            _context.MessageReports.Add(report);

            await _context.SaveChangesAsync();

            // Şikayet oluşturulduktan sonra gelen kutusuna dön.
            return RedirectToAction("Index");
        }
    }
}