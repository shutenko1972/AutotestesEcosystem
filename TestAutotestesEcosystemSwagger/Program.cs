using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "AI Ecosystem Autotests API",
        Description = "ASP.NET Core Web API для управления тестами авторизации AI Ecosystem",
        TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact
        {
            Name = "Development Team",
            Email = "dev@example.com",
            Url = new Uri("https://example.com/contact")
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Явно указываем серверы для Swagger на правильном порту
    options.AddServer(new OpenApiServer { Url = "https://localhost:60363" });
    options.AddServer(new OpenApiServer { Url = "http://localhost:5214" });

    // Добавляем комментарии XML для Swagger
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);

    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Правильная настройка CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Ecosystem Autotests API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "AI Ecosystem Autotests API Documentation";
    });
}

app.UseHttpsRedirection();

// Включаем CORS ДО UseAuthorization и MapControllers
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();