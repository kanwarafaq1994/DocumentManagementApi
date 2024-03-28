using DocumentManagement.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace DocumentManagement.Data.DataSeeding
{
    public static class ModelBuilderExtensions
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            var password = "$2a$10$TWb.9jrYBS5psxE7mlMCeeBS44tdDXUntu.RVAKlbu8BZCwMmv.ly"; // LPARentenbank2021!

            AddUser(modelBuilder, 1, "Kanwar", "Afaq", password, "test@test.de",true, 0 , true, new DateTime(2024, 4, 1), 1);
            AddUser(modelBuilder, 2, "Test", "User1", password, "test1@test.de", false, 0, true, new DateTime(2024, 4, 1), 1);
            AddUser(modelBuilder, 3, "Test", "User2", password, "test2@test.de", false, 0, true, new DateTime(2024, 4, 1), 1);
        }

        private static void AddUser(ModelBuilder modelBuilder, int id,
           string firstName, string lastName, string password, string email, bool isAdmin,
           int failedLoginCount, bool isActive, DateTime createdOn, int createdBy)
        {

            modelBuilder.Entity<User>(u =>
            {
                u.HasData(new
                {
                    Id = id,
                    FirstName = firstName,
                    LastName = lastName,
                    PasswordHash = password,
                    FailedLoginCount = failedLoginCount,
                    Email = email,
                    IsActive = isActive,
                    IsAdmin = isAdmin,
                    CreatedOn = createdOn,
                    CreatedBy = createdBy,
                });
            });
        }
    }
}
