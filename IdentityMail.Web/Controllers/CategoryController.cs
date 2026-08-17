using IdentityMail.Web.Context;
using IdentityMail.Web.DTOs.CategoryDtos;
using IdentityMail.Web.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityMail.Web.Controllers
{
    // Sadece User rolündeki kullanıcılar erişebilir
    [Authorize(Roles = "User")]
    public class CategoryController(
        AppDbContext _context,
        UserManager<AppUser> _userManager) : Controller
    {
        // Kişisel ve ortak kategorileri listeler
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            // Kullanıcı bulunamazsa login sayfasına gönder
            if (user == null)
                return RedirectToAction("Login", "Auth");

            // Kullanıcının kendi kategorilerini ve ortak kategorileri getir
            var categories = await _context.Categories
                .Where(x => x.UserId == user.Id || x.UserId == null)
                .OrderBy(x => x.CategoryName)
                .ToListAsync();

            return View(categories);
        }

        // Kategori ekleme sayfasını açar
        [HttpGet]
        public IActionResult AddCategory()
        {
            return View();
        }

        // Yeni kategori ekler
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(CategoryDto categoryDto)
        {
            var user = await _userManager.GetUserAsync(User);

            // Kullanıcı bulunamazsa login sayfasına gönder
            if (user == null)
                return RedirectToAction("Login", "Auth");

            // Form doğrulaması başarısızsa sayfaya geri dön
            if (!ModelState.IsValid)
                return View(categoryDto);

            // Aynı isimde kişisel veya ortak kategori var mı kontrol et
            var exists = await _context.Categories
                .AnyAsync(x =>
                    (x.UserId == user.Id || x.UserId == null) &&
                    x.CategoryName.ToLower() == categoryDto.CategoryName.ToLower());

            if (exists)
            {
                ModelState.AddModelError(
                    nameof(categoryDto.CategoryName),
                    "Bu isimde bir kategori zaten var.");

                return View(categoryDto);
            }

            // Yeni kişisel kategori oluştur
            var category = new Category
            {
                CategoryName = categoryDto.CategoryName,
                UserId = user.Id
            };

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // Kategori güncelleme sayfasını açar
        [HttpGet]
        public async Task<IActionResult> UpdateCategory(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            // Kullanıcı bulunamazsa login sayfasına gönder
            if (user == null)
                return RedirectToAction("Login", "Auth");

            // Sadece kullanıcının kendi kategorisini getir
            var category = await _context.Categories
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == user.Id);

            // Kategori bulunamazsa 404 döndür
            if (category == null)
                return NotFound();

            // Entity bilgisini DTO'ya aktar
            var updateCategoryDto = new UpdateCategoryDto
            {
                Id = category.Id,
                CategoryName = category.CategoryName
            };

            return View(updateCategoryDto);
        }

        // Kategoriyi günceller
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCategory(UpdateCategoryDto updateCategoryDto)
        {
            var user = await _userManager.GetUserAsync(User);

            // Kullanıcı bulunamazsa login sayfasına gönder
            if (user == null)
                return RedirectToAction("Login", "Auth");

            // Form doğrulaması başarısızsa sayfaya geri dön
            if (!ModelState.IsValid)
                return View(updateCategoryDto);

            // Sadece kullanıcının kendi kategorisini bul
            var category = await _context.Categories
                .FirstOrDefaultAsync(x =>
                    x.Id == updateCategoryDto.Id &&
                    x.UserId == user.Id);

            // Kategori bulunamazsa 404 döndür
            if (category == null)
                return NotFound();

            // Aynı isimde başka bir kategori var mı kontrol et
            var duplicateExists = await _context.Categories
                .AnyAsync(x =>
                    (x.UserId == user.Id || x.UserId == null) &&
                    x.Id != category.Id &&
                    x.CategoryName.ToLower() == updateCategoryDto.CategoryName.ToLower());

            if (duplicateExists)
            {
                ModelState.AddModelError(
                    nameof(updateCategoryDto.CategoryName),
                    "Bu isimde bir kategori zaten var.");

                return View(updateCategoryDto);
            }

            // Kategori adını güncelle
            category.CategoryName = updateCategoryDto.CategoryName;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // Kategori siler
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            // Kullanıcı bulunamazsa login sayfasına gönder
            if (user == null)
                return RedirectToAction("Login", "Auth");

            // Sadece kullanıcının kendi kategorisini getir
            var category = await _context.Categories
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == user.Id);

            // Kategori bulunamazsa 404 döndür
            if (category == null)
                return NotFound();

            // Kategoriyi sil
            _context.Categories.Remove(category);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}