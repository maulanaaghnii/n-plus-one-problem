using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace NPlusOneDotNet.Models
{
    public class Author
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<Post> Posts { get; set; } = new();
    }

    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int AuthorId { get; set; }
        public Author Author { get; set; } = null!;
    }

    public class AppDbContext : DbContext
    {
        public DbSet<Author> Authors => Set<Author>();
        public DbSet<Post> Posts => Set<Post>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=nplusone.db");
            // Log SQL queries to the console
            optionsBuilder.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
        }
    }
}
