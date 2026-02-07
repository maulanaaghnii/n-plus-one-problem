using Microsoft.EntityFrameworkCore;
using NPlusOneDotNet.Models;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Seed Database
using (var scope = app.Services.CreateScope())
{
    using var db = new AppDbContext();
    db.Database.EnsureCreated();
    if (!db.Authors.Any())
    {
        var authors = new List<Author> { new Author { Name = "Alice" }, new Author { Name = "Bob" } };
        db.Authors.AddRange(authors);
        db.SaveChanges();

        var posts = Enumerable.Range(1, 100).Select(i => new Post
        {
            Title = $"Post #{i}",
            AuthorId = (i % 2) + 1
        });
        db.Posts.AddRange(posts);
        db.SaveChanges();
    }
}

app.MapGet("/bad", async () =>
{
    using var db = new AppDbContext();
    var stopwatch = Stopwatch.StartNew();

    // Query 1: Fetch all posts
    var posts = await db.Posts.ToListAsync();

    var results = new List<object>();
    foreach (var post in posts)
    {
        // Lazy Loading / Explicit Loading manual causing N+1
        // In EF Core, if not 'Included', 'Author' will be null
        await db.Entry(post).Reference(p => p.Author).LoadAsync(); 
        results.Add(new { post.Title, AuthorName = post.Author.Name });
    }

    stopwatch.Stop();
    return new { ElapsedMs = stopwatch.ElapsedMilliseconds, Count = results.Count };
});

app.MapGet("/good", async () =>
{
    using var db = new AppDbContext();
    var stopwatch = Stopwatch.StartNew();

    // 1 Query with JOIN using Include()
    var posts = await db.Posts
        .Include(p => p.Author)
        .ToListAsync();

    var results = posts.Select(p => new { p.Title, AuthorName = p.Author.Name }).ToList();

    stopwatch.Stop();
    return new { ElapsedMs = stopwatch.ElapsedMilliseconds, Count = results.Count };
});

app.Run();
