using ConsoleApp1;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Register dependencies
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddHybridCache();

// Register service
builder.Services.AddSingleton<PostsService>();

using var app = builder.Build();

// Resolve and use the service
var postsService = app.Services.GetRequiredService<PostsService>();
var posts = await postsService.GetUserPostsAsync("1");

// Output results
Console.WriteLine($"Fetched {posts?.Count ?? 0} posts for user 1");
if (posts is not null)
{
    foreach (var p in posts)
    {
        Console.WriteLine($"[{p.Id}] {p.Title}");
    }
}

