using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Markwell.Core.Brokers;
using Markwell.Core.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<StorageBroker>((sp, options) =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("DefaultConnection") ?? "Data Source=markwell.db";

    if (builder.Environment.IsProduction())
        options.UseNpgsql(connectionString);
    else
        options.UseSqlite(connectionString);
});

builder.Services.AddScoped<IProfileBroker, ProfileBroker>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var storageBroker = scope.ServiceProvider.GetRequiredService<StorageBroker>();
    await storageBroker.Database.EnsureCreatedAsync();
    await RoleSeeder.SeedRolesAsync(storageBroker);
}

app.UseHttpsRedirection();

app.MapGet("/health", () => new
{
    status = "healthy",
    timestamp = DateTimeOffset.UtcNow,
    version = "1.0.0"
})
.WithName("GetHealth");

app.MapScalarApiReference();

app.Run();
