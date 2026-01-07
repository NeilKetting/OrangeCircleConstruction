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
            /* 
             * Previous check: if (context.Users.Any()) return; 
             * This prevents seeding if ANY user exists (e.g. invalid test users).
             * We will now explicitly ensure the Admin user exists.
             */

            var adminEmail = "neil@mdk.co.za";
            var adminUser = context.Users.FirstOrDefault(u => u.Email == adminEmail);

            if (adminUser == null)
            {
                adminUser = new User
                {
                    Email = adminEmail,
                    Password = hasher.HashPassword("pass"), // Hashed Password
                    FirstName = "Neil",
                    LastName = "Admin",
                    UserRole = UserRole.Admin,
                    IsApproved = true,
                    IsEmailVerified = true
                };
                context.Users.Add(adminUser);
                context.SaveChanges();
            }
            else
            {
                // Optional: Ensure password matches "pass" hash if debugging, 
                // but usually we don't overwrite passwords in prod. 
                // For this dev request: "Can we not seed me as a user created with password hash?"
                // Implicitly implies valid user should exist. 
                // If the user forgot their password (likely "pass" logic from failsafe), 
                // we could force reset it here, but that's aggressive.
                // Assuming existence is enough given the failsafe removal.
            }

            // Other default users if needed...
            if (context.Users.Count() > 1) return; // If more than just our admin, skip rest

            var users = new User[]
            {
                // ... add other seed users here if requested ...
            };
            // Note: The original returned early, so users array was never reached if Any() was true.
            // Keeping it simple.

            foreach (User u in users)
            {
                context.Users.Add(u);
            }
            context.SaveChanges();
        }
    }
}
