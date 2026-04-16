using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/health", () => new
{
    status = "healthy",
    timestamp = DateTimeOffset.UtcNow,
    version = "1.0.0"
})
.WithName("GetHealth")
.WithOpenApi();

app.MapScalarApiReference();

app.Run();
