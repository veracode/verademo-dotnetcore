using System.Data.Entity;

using Verademo.Models;

namespace Verademo.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() : base("Server=127.0.0.1,1433;Database=Verademo;User=sa;Password=SuperSecurePassw0rd!")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasKey(x => x.UserName);
        }

        public DbSet<User> Users { get; set; }
    }
}
