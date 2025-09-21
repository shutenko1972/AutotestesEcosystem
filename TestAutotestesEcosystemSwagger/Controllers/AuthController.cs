using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace TestAutotestesEcosystemSwagger.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;

        public AuthController(ILogger<AuthController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Модель запроса для аутентификации
        /// </summary>
        public class LoginRequestModel
        {
            /// <summary>
            /// Логин пользователя
            /// </summary>
            [Required(ErrorMessage = "Логин обязателен")]
            public string Login { get; set; } = string.Empty;

            /// <summary>
            /// Пароль пользователя
            /// </summary>
            [Required(ErrorMessage = "Пароль обязателен")]
            public string Password { get; set; } = string.Empty;
        }

        /// <summary>
        /// Аутентификация пользователя
        /// </summary>
        /// <remarks>
        /// Выполняет вход пользователя в систему.
        /// </remarks>
        /// <param name="request">Модель с данными для аутентификации</param>
        /// <response code="302">Успешная аутентификация, перенаправление на главную страницу</response>
        /// <response code="400">Неверный формат запроса или отсутствуют обязательные поля</response>
        /// <response code="401">Неверные учетные данные</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Consumes("application/x-www-form-urlencoded")]
        public IActionResult Login([FromForm] LoginRequestModel request)
        {
            try
            {
                _logger.LogInformation("Login attempt for user: {Login}", request.Login);

                // Валидация модели
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToArray();

                    _logger.LogWarning("Validation failed: {Errors}", string.Join(", ", errors));
                    return BadRequest(new { errors });
                }

                // Проверка учетных данных
                if (request.Login != "v_shutenko" || request.Password != "8nEThznM")
                {
                    _logger.LogWarning("Invalid credentials for user: {Login}", request.Login);
                    return Unauthorized(new { error = "Неверные учетные данные" });
                }

                // Установка сессионной cookie
                Response.Cookies.Append("session-api", "94ca369fc76fcdde8f61c7785f0d0a9d", new CookieOptions
                {
                    Path = "/",
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddHours(8)
                });

                // Установка дополнительных cookies как в оригинальном запросе
                Response.Cookies.Append("_identity", "deleted", new CookieOptions
                {
                    Expires = DateTimeOffset.FromUnixTimeSeconds(0),
                    Path = "/",
                    HttpOnly = true,
                    SameSite = SameSiteMode.Lax
                });

                Response.Cookies.Append("_csrf-frontend", "6d32f5bf69ea1da23a4ce8178d1b15e4cae448740d90a06119adf6fa7bed937ba%3A2%3A%7Bi%3A0%3Bs%3A14%3A%22_csrf-frontend%22%3Bi%3A1%3Bs%3A32%3A%22e5LPYDiZpIASm3QnttIppoJC5Yi4G5je%22%3B%7D", new CookieOptions
                {
                    Path = "/",
                    HttpOnly = true,
                    SameSite = SameSiteMode.Lax
                });

                _logger.LogInformation("User {Login} authenticated successfully", request.Login);

                // Перенаправление на целевую страницу
                return Redirect("https://ai-ecosystem-test.janusww.com:9999/request/model.html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user: {Login}", request.Login);
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        /// <summary>
        /// Выход пользователя из системы
        /// </summary>
        /// <remarks>
        /// Выполняет logout пользователя, очищает сессионные cookies
        /// </remarks>
        /// <response code="200">Успешный выход из системы</response>
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Logout()
        {
            try
            {
                // Очистка cookies
                Response.Cookies.Delete("session-api");
                Response.Cookies.Delete("XSRF-TOKEN");
                Response.Cookies.Delete("_identity");
                Response.Cookies.Delete("_csrf-frontend");

                _logger.LogInformation("User logged out successfully");

                return Ok(new { message = "Успешный выход из системы" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, new { error = "Ошибка при выходе из системы" });
            }
        }
    }
}