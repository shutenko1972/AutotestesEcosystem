using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "🔐 Service API",
        Version = "1.0.0",
        Description = "API для аутентификации и проверки функций"
    });
});

builder.Services.AddSingleton<ConcurrentDictionary<string, SessionInfo>>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Модели данных
public class LoginRequest
{
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Message { get; set; } = string.Empty;
    public string RedirectUrl { get; set; } = string.Empty;
    public string SessionToken { get; set; } = string.Empty;
}

public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
}

public class LogoutResponse
{
    public string Message { get; set; } = string.Empty;
}

public class SessionInfoResponse
{
    public bool Valid { get; set; }
    public string UserLogin { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class SessionInfo
{
    public string UserLogin { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}