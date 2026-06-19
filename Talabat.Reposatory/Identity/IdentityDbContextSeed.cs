using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entitys.Identity;

namespace Talabat.Reposatory.Identity
{
    public class IdentityDbContextSeed
    {

        public static async Task SeedUserAsync(UserManager<User> userManager)
        {
            if (!userManager.Users.Any())
            {
                var user = new User()
                {
                    DisplayName = "SaadElnager",
                    Email = "Saad@gmail.com",
                    UserName = "Saad",
                    PhoneNumber= "1234567890",


                };
                await userManager.CreateAsync(user,"123");
            }

        }


    }
}
