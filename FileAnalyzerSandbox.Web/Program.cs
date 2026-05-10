using FileAnalyzerSandbox.Application.Services;
using FileAnalyzerSandbox.Domain.Interfaces;
using FileAnalyzerSandbox.Infrastructure.Data;
using FileAnalyzerSandbox.Infrastructure.Repositories;
using FileAnalyzerSandbox.Web.Components;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// DOCKER: HTTP на 8080, иначе используем launchSettings.json
// ============================================================
var isDocker = Environment.GetEnvironmentVariable("DOCKER_RUNNING") == "true";

string dbPath;
if (isDocker)
{
    dbPath = "/app/data/fileanalyzer.db";  // Docker: с сохранением
}
else
{
    dbPath = "fileanalyzer.db";             // Локально: простой файл
}

if (isDocker)
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(8080);
    });
}

// ============================================================
// ОСТАЛЬНЫЕ НАСТРОЙКИ
// ============================================================
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));
builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IFileAnalysisRepository, FileAnalysisRepository>();
builder.Services.AddScoped<IFileAnalysisService, FileAnalysisService>();
builder.Services.AddScoped<ISandboxService, SandboxService>();

builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddHealthChecks();
builder.Services.AddCors();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
}

// HTTPS редирект только НЕ в Docker
if (!isDocker)
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseAntiforgery();
app.MapHealthChecks("/health");
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

// Создание БД и тестового пользователя
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();

    var testUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    if (!context.Users.Any(u => u.Id == testUserId))
    {
        context.Users.Add(new FileAnalyzerSandbox.Domain.Entities.User
        {
            Id = testUserId,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "",
            CreatedAt = DateTime.Now
        });
        context.SaveChanges();
        Console.WriteLine("Test user created!");
    }
}

app.Run();