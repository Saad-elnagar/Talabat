using Microsoft.AspNetCore.Identity;
using Talabat.Core.Entitys.Identity;

namespace Talabat.Reposatory.Identity
{
    public static class IdentityDbContextSeed  // ✅ static أحسن
    {
        public static async Task SeedUserAsync(UserManager<User> userManager)
        {
            if (!userManager.Users.Any())
            {
                var user = new User
                {
                    DisplayName = "SaadElnagar",
                    Email = "Saad@gmail.com",
                    UserName = "Saad",
                    PhoneNumber = "1234567890"
                };

                // ✅ Password يتوافق مع الـ Rules
                var result = await userManager.CreateAsync(user, "P@ssw0rd123");

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new Exception($"Failed to seed user: {errors}");
                }
            }
        }
    }
}