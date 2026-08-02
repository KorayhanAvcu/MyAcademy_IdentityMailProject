using IdentityMail.Web.Context;
using IdentityMail.Web.DTOs.UserMessageDtos;
using IdentityMail.Web.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityMail.Web.Controllers
{
    [Authorize]
    public class MessageController(UserManager<AppUser> _userManager, 
                                    AppDbContext _context) : Controller
    {
        public async Task<IActionResult> Index(int? isRead)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            ViewBag.fullName = user.FirstName + " " + user.LastName;


            var query = _context.UserMessages.Include(x => x.Sender)
                                             .Where(x => x.ReceiverId == user.Id)
                                             .Where(x => x.IsDelete == false);


            if (isRead.HasValue)
            {
                query = query.Where(x => x.IsRead == (isRead.Value == 1));
            }
            var messages = await query.OrderByDescending(x => x.SendDate)
                                      .ToListAsync();
            return View(messages);
        }
        public async Task<IActionResult> Important()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            ViewBag.fullName = user.FirstName + " " + user.LastName;


            var messages = await _context.UserMessages.Include(x => x.Sender)
                                             .Where(x => x.ReceiverId == user.Id)
                                             .Where(x => x.IsImportant == true)
                                             .Where(x => x.IsDelete == false)
                                             .OrderByDescending(x => x.SendDate).ToListAsync();
            return View(messages);
        }
        public async Task<IActionResult> SentMail()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            ViewBag.fullName = user.FirstName + " " + user.LastName;

            var messages = await _context.UserMessages.Include(x => x.Sender)
                                                      .Where(x => x.SenderId == user.Id)
                                                      .Where(x => x.IsDelete == false)
                                                      .ToListAsync();
            return View(messages);
        }

        public IActionResult SendMail()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendMail(SendMailDto sendMailDto)
        {
            var sender = await _userManager.FindByNameAsync(User.Identity.Name);
            var receiver = await _userManager.FindByEmailAsync(sendMailDto.ReceiverMail);
            if(receiver is null)
            {
                ModelState.AddModelError(string.Empty, "Girdiğiniz mail ile sistemde kayıtlı kullanıcı bulunamadı.");
                return View(sendMailDto);
            }
            var newMessage = new UserMessage
            {
                SendDate = DateTime.Now,
                ReceiverId = receiver.Id,
                SenderId = sender.Id,
                Subject = sendMailDto.Subject,
                Body = sendMailDto.Body
            };

            _context.UserMessages.Add(newMessage);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> MailDetail(int id)
        {
            var message = await _context.UserMessages.Include(x=>x.Sender).FirstOrDefaultAsync(x=>x.Id == id);
            message.IsRead = true;
            await _context.SaveChangesAsync();
            return View(message);
        }

        public async Task<IActionResult> isImportant(int id)
        {
            var message = await _context.UserMessages.Include(x => x.Sender).FirstOrDefaultAsync(x => x.Id == id);
            message.IsImportant = !message.IsImportant;
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> MoveToTrash(int id)
        {
            var message = await _context.UserMessages.Include(x => x.Sender).FirstOrDefaultAsync(x => x.Id == id);
            message.IsDelete = true;
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> RestoreMessage(int id)
        {
            var message = await _context.UserMessages.Include(x => x.Sender).FirstOrDefaultAsync(x => x.Id == id);
            message.IsDelete = false;
            await _context.SaveChangesAsync();
            return RedirectToAction("Trash");
        }

        

        public async Task<IActionResult> Trash()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var messages = await _context.UserMessages
                             .Include(x => x.Sender)
                             .Where(x =>
                                 (x.ReceiverId == user.Id || x.SenderId == user.Id) &&
                                 x.IsDelete)
                             .OrderByDescending(x => x.SendDate)
                             .ToListAsync();

            return View(messages);
        }
    }
}
