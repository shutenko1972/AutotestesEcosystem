using Microsoft.OpenApi.Models;
using System.Reflection;
using TestAutotestesEcosystemSwagger.Models;

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

    // Добавляем комментарии XML для Swagger
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);

    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // Сортируем эндпоинты: сначала системные, потом контроллеры
    options.OrderActionsBy(apiDesc =>
    {
        var path = apiDesc.RelativePath?.ToLower() ?? "";
        
        // Приоритет 1: Системные эндпоинты
        if (path == "health" || path == "") return "01";
        
        // Приоритет 2: Эндпоинты Auth
        if (path.StartsWith("api/auth")) return "02";
        
        // Остальные эндпоинты
        return "99";
    });

    // Группируем по контроллерам
    options.TagActionsBy(api =>
    {
        var controllerName = api.ActionDescriptor.RouteValues["controller"];
        return new[] { controllerName };
    });

    // Добавляем поддержку JWT Bearer
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\""
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Добавляем CORS для кросс-доменных запросов
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Регистрируем конфигурацию
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));

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
        options.DefaultModelsExpandDepth(-1);
        
        // Добавляем кастомные CSS для улучшения внешнего вида
        options.InjectStylesheet("/swagger-custom.css");
    });
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Ecosystem Autotests API v1");
        options.RoutePrefix = "api-docs";
    });
}

app.UseHttpsRedirection();

// Включаем CORS
app.UseCors("AllowAll");

// Добавляем middleware для логирования запросов
app.Use(async (context, next) =>
{
    Console.WriteLine($"Request: {context.Request.Method} {context.Request.Path}");
    await next();
    Console.WriteLine($"Response: {context.Response.StatusCode}");
});

app.UseAuthorization();

app.MapControllers();

// Добавляем health check endpoint (будет первым!)
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }))
   .WithTags("System")
   .WithName("HealthCheck")
   .WithDisplayName("01 - Health Check")
   .WithDescription("Проверка работоспособности приложения");

// Добавляем корневой endpoint с информацией об API (будет вторым!)
app.MapGet("/", () => Results.Redirect("/swagger"))
   .WithTags("System")
   .WithName("Root")
   .WithDisplayName("02 - Root Redirect")
   .WithDescription("Перенаправление на Swagger UI");

// Глобальная обработка исключений
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new
        {
            error = "An unexpected error occurred. Please try again later.",
            requestId = context.TraceIdentifier,
            timestamp = DateTime.UtcNow
        });
    });
});

// Получаем настройки из конфигурации
var apiSettings = app.Services.GetRequiredService<IConfiguration>().GetSection("ApiSettings").Get<ApiSettings>();

Console.WriteLine("Application starting...");
Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");
Console.WriteLine($"Base URL: {apiSettings?.BaseUrl ?? "Not configured"}");
Console.WriteLine($"Timeout: {apiSettings?.Timeout ?? 30} seconds");
Console.WriteLine("Swagger UI available at: /swagger");
Console.WriteLine("Health check available at: /health");
Console.WriteLine("Auth API available at: /api/auth");

app.Run();

// Класс для настроек API
public class ApiSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public int Timeout { get; set; } = 30;
}