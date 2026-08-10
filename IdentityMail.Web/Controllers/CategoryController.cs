using IdentityMail.Web.Context;
using IdentityMail.Web.DTOs.CategoryDtos;
using IdentityMail.Web.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityMail.Web.Controllers
{
    [Authorize]
    public class CategoryController(
        AppDbContext _context,
        UserManager<AppUser> _userManager) : Controller
    {
        // =========================
        // KATEGORİ LİSTESİ (Kişisel + Ortak)
        // =========================

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            var categories = await _context.Categories
                .Where(x => x.UserId == user.Id || x.UserId == null)
                .OrderBy(x => x.CategoryName)
                .ToListAsync();

            return View(categories);
        }


        // =========================
        // KATEGORİ EKLE
        // =========================

        [HttpGet]
        public IActionResult AddCategory()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(CategoryDto categoryDto)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
                return View(categoryDto);

            var exists = await _context.Categories
                .AnyAsync(x =>
                    (x.UserId == user.Id || x.UserId == null) &&
                    x.CategoryName.ToLower() == categoryDto.CategoryName.ToLower());

            if (exists)
            {
                ModelState.AddModelError(nameof(categoryDto.CategoryName), "Bu isimde bir kategori zaten var.");
                return View(categoryDto);
            }

            var category = new Category
            {
                CategoryName = categoryDto.CategoryName,
                UserId = user.Id // kullanıcı her zaman kendi kişisel kategorisini oluşturur
            };

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }


        // =========================
        // KATEGORİ GÜNCELLE
        // =========================

        [HttpGet]
        public async Task<IActionResult> UpdateCategory(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            // Sadece kendi kategorisini düzenleyebilir, ortak (UserId==null) kategoriyi düzenleyemez
            var category = await _context.Categories
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == user.Id);

            if (category == null)
                return NotFound();

            var updateCategoryDto = new UpdateCategoryDto
            {
                Id = category.Id,
                CategoryName = category.CategoryName
            };

            return View(updateCategoryDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCategory(UpdateCategoryDto updateCategoryDto)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
                return View(updateCategoryDto);

            var category = await _context.Categories
                .FirstOrDefaultAsync(x =>
                    x.Id == updateCategoryDto.Id &&
                    x.UserId == user.Id);

            if (category == null)
                return NotFound();

            var duplicateExists = await _context.Categories
                .AnyAsync(x =>
                    (x.UserId == user.Id || x.UserId == null) &&
                    x.Id != category.Id &&
                    x.CategoryName.ToLower() == updateCategoryDto.CategoryName.ToLower());

            if (duplicateExists)
            {
                ModelState.AddModelError(nameof(updateCategoryDto.CategoryName), "Bu isimde bir kategori zaten var.");
                return View(updateCategoryDto);
            }

            category.CategoryName = updateCategoryDto.CategoryName;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }


        // =========================
        // KATEGORİ SİL
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            // Sadece kendi kategorisini silebilir, ortak kategoriyi silemez
            var category = await _context.Categories
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == user.Id);

            if (category == null)
                return NotFound();

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}