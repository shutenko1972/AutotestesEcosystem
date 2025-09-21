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
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
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

                _logger.LogInformation("User {Login} authenticated successfully", request.Login);

                return Ok(new
                {
                    message = "Успешная аутентификация",
                    redirectUrl = "https://ai-ecosystem-test.janusww.com:9999/request/model.html",
                    sessionToken = "94ca369fc76fcdde8f61c7785f0d0a9d"
                });
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
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Logout()
        {
            try
            {
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