using Scalar.AspNetCore;
using Markwell.Core.Brokers;
using Markwell.Core.Data;

var builder = WebApplication.CreateBuilder(args);

// Add storage broker (also serves as database context)
builder.Services.AddDbContext<StorageBroker>();
builder.Services.AddScoped<StorageBroker>();

// Add profile broker for domain operations
builder.Services.AddScoped<IProfileBroker, ProfileBroker>();

var app = builder.Build();

// Seed predefined roles on startup
using (var scope = app.Services.CreateScope())
{
    var storageBroker = scope.ServiceProvider.GetRequiredService<StorageBroker>();
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
