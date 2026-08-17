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
    [Authorize(Roles = "User")]
    public class MessageController(
        UserManager<AppUser> _userManager,
        AppDbContext _context) : Controller
    {
        // =========================
        // GELEN KUTUSU
        // =========================

        public async Task<IActionResult> Index(MessageFilterDto filter)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            if (filter.Page < 1) filter.Page = 1;
            if (filter.PageSize < 1) filter.PageSize = 10;

            var query = _context.UserMessages
                .Include(x => x.Sender)
                .Include(x => x.Category)
                .Where(x => x.ReceiverId == user.Id)
                .Where(x => x.IsDelete == false)
                .Where(x => x.IsDraft != true);

            // Gönderen adına / emailine arama
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim().ToLower();
                query = query.Where(x =>
                    (x.Sender.FirstName + " " + x.Sender.LastName).ToLower().Contains(term) ||
                    x.Sender.Email.ToLower().Contains(term));
            }

            // Konuya göre arama
            if (!string.IsNullOrWhiteSpace(filter.Subject))
            {
                query = query.Where(x => x.Subject.Contains(filter.Subject));
            }

            // Tarih aralığı
            if (filter.StartDate.HasValue)
            {
                query = query.Where(x => x.SendDate >= filter.StartDate.Value.Date);
            }

            if (filter.EndDate.HasValue)
            {
                var end = filter.EndDate.Value.Date.AddDays(1);
                query = query.Where(x => x.SendDate < end);
            }

            // Kategori
            if (filter.CategoryId.HasValue)
            {
                query = query.Where(x => x.CategoryId == filter.CategoryId.Value);
            }

            // Okundu / Okunmadı
            if (filter.IsRead.HasValue)
            {
                query = query.Where(x => x.IsRead == filter.IsRead.Value);
            }

            // Önemli
            if (filter.IsImportant.HasValue)
            {
                query = query.Where(x => x.IsImportant == filter.IsImportant.Value);
            }

            // Sıralama
            query = filter.SortOrder == "asc"
                ? query.OrderBy(x => x.SendDate)
                : query.OrderByDescending(x => x.SendDate);

            var totalCount = await query.CountAsync();

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
            ViewBag.fullName = $"{user.FirstName} {user.LastName}";

            return View(result);
        }


        // =========================
        // ÖNEMLİ MESAJLAR
        // =========================

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


        // =========================
        // GÖNDERİLENLER
        // =========================

        public async Task<IActionResult> SentMail(MessageFilterDto filter)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            if (filter.Page < 1) filter.Page = 1;
            if (filter.PageSize < 1) filter.PageSize = 10;

            var query = _context.UserMessages
                .Include(x => x.Receiver)
                .Include(x => x.Category)
                .Where(x => x.SenderId == user.Id)
                .Where(x => x.IsDelete == false)
                .Where(x => x.IsDraft != true);

            // Alıcı adına / emailine arama
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim().ToLower();
                query = query.Where(x =>
                    (x.Receiver.FirstName + " " + x.Receiver.LastName).ToLower().Contains(term) ||
                    x.Receiver.Email.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(filter.Subject))
            {
                query = query.Where(x => x.Subject.Contains(filter.Subject));
            }

            if (filter.StartDate.HasValue)
            {
                query = query.Where(x => x.SendDate >= filter.StartDate.Value.Date);
            }

            if (filter.EndDate.HasValue)
            {
                var end = filter.EndDate.Value.Date.AddDays(1);
                query = query.Where(x => x.SendDate < end);
            }

            if (filter.CategoryId.HasValue)
            {
                query = query.Where(x => x.CategoryId == filter.CategoryId.Value);
            }

            // Alıcının okuyup okumadığı
            if (filter.IsRead.HasValue)
            {
                query = query.Where(x => x.IsRead == filter.IsRead.Value);
            }

            if (filter.IsImportant.HasValue)
            {
                query = query.Where(x => x.IsImportant == filter.IsImportant.Value);
            }

            query = filter.SortOrder == "asc"
                ? query.OrderBy(x => x.SendDate)
                : query.OrderByDescending(x => x.SendDate);

            var totalCount = await query.CountAsync();

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


        // =========================
        // YENİ MAIL SAYFASI
        // =========================

        [HttpGet]
        public async Task<IActionResult> SendMail()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            await LoadCategories(user.Id);
            return View(new SendMailDto());
        }


        // =========================
        // MAIL GÖNDER
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMail(SendMailDto sendMailDto)
        {
            var sender = await _userManager.GetUserAsync(User);

            if (sender == null)
                return RedirectToAction("Login", "Auth");

            // 1) Data annotation kontrolleri en başta yapılır
            if (!ModelState.IsValid)
            {
                await LoadCategories(sender.Id);
                return View(sendMailDto);
            }

            // 2) Eğer mevcut bir taslaktan gönderiliyorsa taslağı bul
            UserMessage? message = null;

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

            // 3) Alıcı kontrolü (ModelState geçtiği için ReceiverMail artık boş olamaz)
            var receiver = await _userManager.FindByEmailAsync(sendMailDto.ReceiverMail!.Trim());

            if (receiver == null)
            {
                ModelState.AddModelError(
                    nameof(sendMailDto.ReceiverMail),
                    "Girdiğiniz mail ile sistemde kayıtlı kullanıcı bulunamadı.");

                await LoadCategories(sender.Id);
                return View(sendMailDto);
            }

            // 4) Yeni mesaj oluştur
            if (message == null)
            {
                message = new UserMessage { SenderId = sender.Id };
                _context.UserMessages.Add(message);
            }

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

            return RedirectToAction("Index");
        }


        // =========================
        // MESAJI YANITLA
        // =========================

        [HttpGet]
        public async Task<IActionResult> Reply(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            var original = await _context.UserMessages
                .Include(x => x.Sender)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.ReceiverId == user.Id &&
                    x.IsDraft != true);

            if (original == null)
                return NotFound();

            var model = new SendMailDto
            {
                // Aynı konuşmayı devam ettirebilmek için
                ConversationId = original.ConversationId,

                // Mesajı atan kişiye cevap ver
                ReceiverMail = original.Sender?.Email,

                // Konunun başına Re: ekle
                Subject = original.Subject != null &&
                          original.Subject.StartsWith("Re:")
                    ? original.Subject
                    : $"Re: {original.Subject}",

                CategoryId = original.CategoryId,

                // Orijinal mesajı göster
                Body = $"\n\n---- Orijinal Mesaj ----\n" +
                       $"{original.Sender?.FirstName} {original.Sender?.LastName} yazdı:\n" +
                       $"{original.Body}"
            };

            await LoadCategories(user.Id);

            return View("SendMail", model);
        }


        // =========================
        // TASLAK KAYDET
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDraft(SendMailDto sendMailDto)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Json(new { success = false });

            UserMessage? draft = null;

            if (sendMailDto.Id.HasValue)
            {
                draft = await _context.UserMessages
                    .FirstOrDefaultAsync(x =>
                        x.Id == sendMailDto.Id.Value &&
                        x.SenderId == user.Id &&
                        x.IsDraft == true);
            }

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

            draft.Subject = sendMailDto.Subject;
            draft.Body = sendMailDto.Body;
            draft.CategoryId = sendMailDto.CategoryId;

            if (!string.IsNullOrWhiteSpace(sendMailDto.ReceiverMail))
            {
                var receiver = await _userManager.FindByEmailAsync(sendMailDto.ReceiverMail.Trim());

                if (receiver != null)
                    draft.ReceiverId = receiver.Id;
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, id = draft.Id });
        }


        // =========================
        // ALICI EMAIL DOĞRULA
        // =========================

        [HttpGet]
        public async Task<IActionResult> CheckReceiver(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Json(new { exists = false });

            var currentUser = await _userManager.GetUserAsync(User);
            var receiver = await _userManager.FindByEmailAsync(email.Trim());

            if (receiver == null)
                return Json(new { exists = false });

            if (currentUser != null && receiver.Id == currentUser.Id)
                return Json(new { exists = true, isSelf = true });

            return Json(new
            {
                exists = true,
                isSelf = false,
                name = $"{receiver.FirstName} {receiver.LastName}"
            });
        }


        // =========================
        // TASLAKLAR
        // =========================

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


        // =========================
        // TASLAK DÜZENLE
        // =========================

        [HttpGet]
        public async Task<IActionResult> EditDraft(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            var draft = await _context.UserMessages
                .Include(x => x.Receiver)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.SenderId == user.Id &&
                    x.IsDraft == true);

            if (draft == null)
                return NotFound();

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


        // =========================
        // TASLAK SİL
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDraft(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Auth");

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


        // =========================
        // MAIL DETAY
        // =========================

        public async Task<IActionResult> MailDetail(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            var message = await _context.UserMessages
                .Include(x => x.Sender)
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.ReceiverId == user.Id &&
                    x.IsDraft != true);

            if (message == null)
                return NotFound();

            message.IsRead = true;
            await _context.SaveChangesAsync();

            return View(message);
        }


        // =========================
        // GÖNDERİLEN MAIL DETAY
        // =========================

        public async Task<IActionResult> SentMailDetail(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Auth");

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


        // =========================
        // ÖNEMLİ YAP / ÇIKAR
        // =========================

        public async Task<IActionResult> IsImportant(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            var message = await _context.UserMessages
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.ReceiverId == user.Id &&
                    x.IsDraft != true);

            if (message == null)
                return NotFound();

            message.IsImportant = !message.IsImportant;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }


        // =========================
        // ÇÖPE TAŞI
        // =========================

        public async Task<IActionResult> MoveToTrash(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            var message = await _context.UserMessages
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    (x.ReceiverId == user.Id || x.SenderId == user.Id) &&
                    x.IsDraft != true);

            if (message == null)
                return NotFound();

            message.IsDelete = true;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }


        // =========================
        // ÇÖPTEN GERİ AL
        // =========================

        public async Task<IActionResult> RestoreMessage(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            var message = await _context.UserMessages
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    (x.ReceiverId == user.Id || x.SenderId == user.Id));

            if (message == null)
                return NotFound();

            message.IsDelete = false;
            await _context.SaveChangesAsync();

            return RedirectToAction("Trash");
        }


        // =========================
        // ÇÖP KUTUSU
        // =========================

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


        // =========================
        // KATEGORİLERİ YÜKLE (KULLANICIYA ÖZEL)
        // =========================

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

        
        [HttpGet]
        public async Task<IActionResult> Report(int id)
        {
            var userId = int.Parse(
                _userManager.GetUserId(User)!);

            var message = await _context.UserMessages
                .Include(x => x.Sender)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.ReceiverId == userId &&
                    x.IsDraft != true &&
                    x.IsDelete != true);

            if (message == null)
            {
                return NotFound();
            }

            // Kullanıcı kendi mesajına şikayet edemesin
            if (message.SenderId == userId)
            {
                return BadRequest("Kendi mesajınızı şikayet edemezsiniz.");
            }

            // Aynı kullanıcı aynı mesajı tekrar şikayet edemesin
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

            var model = new CreateMessageReportDto
            {
                MessageId = message.Id
            };

            ViewBag.Subject = message.Subject;
            ViewBag.SenderName =
                $"{message.Sender.FirstName} {message.Sender.LastName}";

            return View(model);
        }
        // =====================================================
        // ŞİKAYET OLUŞTUR
        // =====================================================

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReportMessage(
            CreateMessageReportDto dto)
        {
            if (!ModelState.IsValid)
            {
                var messageForView = await _context.UserMessages
                    .Include(x => x.Sender)
                    .FirstOrDefaultAsync(x => x.Id == dto.MessageId);

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

            // Mesaj gerçekten bu kullanıcıya mı ait?
            var message = await _context.UserMessages
                .FirstOrDefaultAsync(x =>
                    x.Id == dto.MessageId &&
                    x.ReceiverId == userId &&
                    x.IsDraft != true &&
                    x.IsDelete != true);

            if (message == null)
            {
                return NotFound();
            }

            // Kendi mesajını şikayet edemez
            if (message.SenderId == userId)
            {
                return BadRequest(
                    "Kendi mesajınızı şikayet edemezsiniz.");
            }

            // Daha önce şikayet edilmiş mi?
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

            var report = new MessageReport
            {
                MessageId = dto.MessageId,

                ReporterId = userId,

                Reason = dto.Reason,

                Description = dto.Description,

                Status = ReportStatus.Pending,

                ReviewedById = null,

                AdminNote = null,

                CreatedDate = DateTime.Now,

                ReviewedDate = null
            };

            _context.MessageReports.Add(report);

            await _context.SaveChangesAsync();

            TempData["ReportSuccess"] =
                "Mesajınız başarıyla şikayet edildi. Yönetici inceleyecektir.";

            return RedirectToAction("Index");
        }
    }
}