using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bibliotekarz.Data.Model;
using Microsoft.AspNetCore.Identity;

namespace Bibliotekarz.Data.Context;

/// <summary>
/// Klasa, która uzupełnia bazę danych niezbędnymi encjami. 
/// </summary>
public class Seeder
{
    public static void Seed(AppDbContext dbContext)
    {
        SeedRoles(dbContext);
        SeedUsers(dbContext);
    }

    private static void SeedUsers(AppDbContext dbContext)
    {
        // Dodaję standardowych użytkowników i przypisuję im odpowiednie role.
        if (!dbContext.Users.Any())
        {
            var passwordHasher = new PasswordHasher<ApplicationUser>();

            var adminUser = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                Email = "admin@example.com",
                NormalizedEmail = "ADMIN@EXAMPLE.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString("D"),
                FirstName = "Admin",
                LastName = "User"
            };
            adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "Qwertyuiop.1");

            var normalUser = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "user",
                NormalizedUserName = "USER",
                Email = "user@example.com",
                NormalizedEmail = "USER@EXAMPLE.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString("D"),
                FirstName = "Normal",
                LastName = "User"
            };
            normalUser.PasswordHash = passwordHasher.HashPassword(normalUser, "Qwertyuiop.1");

            dbContext.Users.AddRange(adminUser, normalUser);
            dbContext.SaveChanges();

            var roles = dbContext.Roles.ToList();
            dbContext.UserRoles.Add(new IdentityUserRole<string>()
            {
                UserId = normalUser.Id,
                RoleId = roles.First().Id
            });
            dbContext.UserRoles.Add(new IdentityUserRole<string>()
            {
                UserId = adminUser.Id,
                RoleId = roles.First(r => r.NormalizedName == "ADMIN").Id
            });

            dbContext.SaveChanges();
        }
    }

    private static void SeedRoles(AppDbContext dbContext)
    {
        if (!dbContext.Roles.Any())
        {
            List<IdentityRole> roles =
            [
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "User",
                    NormalizedName = "USER"
                },
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                }
            ];

            dbContext.Roles.AddRange(roles);
            dbContext.SaveChanges();
        }
    }
}