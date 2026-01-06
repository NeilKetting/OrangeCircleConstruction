using OCC.API.Data;
using OCC.Shared.Models;

namespace OCC.API.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            // Prepare DB (Apply Migrations)
            context.Database.EnsureCreated();

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
                    Password = "pass", // Plain text as per AuthController
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
