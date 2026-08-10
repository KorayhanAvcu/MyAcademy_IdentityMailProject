using IdentityMail.Web.Constants;
using IdentityMail.Web.Entities;
using Microsoft.AspNetCore.Identity;

namespace IdentityMail.Web.Data
{
    public static class IdentitySeeder
    {
        // Admin ve User rollerini oluştur
        public static async Task SeedRolesAsync(
            IServiceProvider serviceProvider)
        {
            var roleManager =
                serviceProvider.GetRequiredService<RoleManager<AppRole>>();

            // Admin rolü
            if (!await roleManager.RoleExistsAsync(Roles.Admin))
            {
                await roleManager.CreateAsync(new AppRole
                {
                    Name = Roles.Admin
                });
            }

            // User rolü
            if (!await roleManager.RoleExistsAsync(Roles.User))
            {
                await roleManager.CreateAsync(new AppRole
                {
                    Name = Roles.User
                });
            }
        }


        // Korayhan Avcu'yu Admin yap
        public static async Task SeedAdminAsync(
            IServiceProvider serviceProvider)
        {
            var userManager =
                serviceProvider.GetRequiredService<UserManager<AppUser>>();

            var adminEmail = "korayhan@gmail.com";

            var admin = await userManager.FindByEmailAsync(adminEmail);

            if (admin == null)
            {
                throw new Exception(
                    $"Admin yapılacak kullanıcı bulunamadı: {adminEmail}");
            }

            // Eğer Admin değilse Admin yap
            if (!await userManager.IsInRoleAsync(
                admin,
                Roles.Admin))
            {
                var result = await userManager.AddToRoleAsync(
                    admin,
                    Roles.Admin);

                if (!result.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        result.Errors.Select(x => x.Description));

                    throw new Exception(
                        $"Admin rolü verilemedi: {errors}");
                }
            }
        }


        // Rolü olmayan mevcut kullanıcıları User yap
        public static async Task SeedExistingUsersAsync(
            IServiceProvider serviceProvider)
        {
            var userManager =
                serviceProvider.GetRequiredService<UserManager<AppUser>>();

            var users = userManager.Users.ToList();

            foreach (var user in users)
            {
                // Korayhan Admin olduğu için User yapma
                if (user.Email == "korayhan@gmail.com")
                    continue;

                var roles = await userManager.GetRolesAsync(user);

                // Hiç rolü yoksa User yap
                if (!roles.Any())
                {
                    await userManager.AddToRoleAsync(
                        user,
                        Roles.User);
                }
            }
        }
    }
}