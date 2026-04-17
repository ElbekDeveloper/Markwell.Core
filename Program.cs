using Markwell.Core.Brokers;
using Markwell.Core.Data;
using Markwell.Core.Entities;
using Markwell.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

// Add DbContext with provider-agnostic configuration
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite("Data Source=:memory:"));
}
else
{
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString));
}

// Configure Identity
builder.Services.AddIdentity<User, Role>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.User.RequireUniqueEmail = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Register brokers
builder.Services.AddScoped<IdentityBroker>();
builder.Services.AddScoped<UserBroker>();
builder.Services.AddScoped<RoleBroker>();

// Register common services
builder.Services.AddScoped<PasswordValidationService>();
builder.Services.AddScoped<EmailVerificationService>();

// Register business services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<AuthenticationService>();

// Register orchestration service
builder.Services.AddScoped<ProfileManagementOrchestrationService>();

// Add logging
builder.Services.AddLogging();
builder.Services.AddOpenApi();

// Add controllers
builder.Services.AddControllers();

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

var app = builder.Build();

// Initialize database on startup (development only)
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();
    }
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Map controller routes
app.MapControllers();

app.MapGet("/health", () => new
{
    status = "healthy",
    timestamp = DateTimeOffset.UtcNow,
    version = "1.0.0"
})
.WithName("GetHealth");

app.MapOpenApi();
app.MapScalarApiReference();

app.Run();
