using Microsoft.EntityFrameworkCore;
namespace SaurianSource.Models
{ 
    public class MyContext : DbContext 
    { 
        public MyContext(DbContextOptions options) : base(options) { }

        public DbSet<User> Users { get; set; }

        public DbSet<Thread> Threads { get; set; }

        public DbSet<Message> Messages { get; set; }
    }
}