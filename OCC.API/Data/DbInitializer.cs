using OCC.API.Data;
using OCC.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace OCC.API.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context, OCC.API.Services.PasswordHasher hasher)
        {
            // Prepare DB (Apply Migrations)
            context.Database.Migrate();

            // Look for any users.
            if (context.Users.Any())
            {
                return;   // DB has been seeded
            }

            var users = new User[]
            {
                new User
                {
                    Email = "neil@mdk.co.za",
                    Password = hasher.HashPassword("pass"), // Hashed Password
                    FirstName = "Neil",
                    LastName = "Admin",
                    UserRole = UserRole.Admin,
                    IsApproved = true,
                    IsEmailVerified = true
                }
            };

            foreach (User u in users)
            {
                context.Users.Add(u);
            }
            context.SaveChanges();
        }
    }
}
