using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace EFCoreMistake.Models
{
    public class Database : DbContext
    {
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Company> Companies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Company>().HasData(
                new { Id = 1, Name = "JetBrains" }
            );
            modelBuilder.Entity<Employee>().HasData(
                new {Id = 1, Name = "Khalid Abuhakmeh", CompanyId = 1}
            );
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=app.db");
        }
    }
    
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Employee> Employees { get; set; }
    }

    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CompanyId { get; set; }
        public Company Company { get; set; }
    }
}