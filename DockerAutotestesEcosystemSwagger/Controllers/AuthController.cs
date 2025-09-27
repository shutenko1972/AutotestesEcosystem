using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace DockerAutotestesEcosystemSwagger.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly ConcurrentDictionary<string, SessionInfo> _activeSessions;

        public AuthController(ILogger<AuthController> logger, 
                            ConcurrentDictionary<string, SessionInfo> activeSessions)
        {
            _logger = logger;
            _activeSessions = activeSessions;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 401)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public IActionResult Login([FromForm] string login, [FromForm] string password)
        {
            try
            {
                _logger.LogInformation("Login attempt for user: {Login}", login);

                // Валидация
                if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
                {
                    _logger.LogWarning("Validation failed: Login and password are required");
                    return BadRequest(new ErrorResponse { Error = "Логин и пароль обязательны" });
                }

                // Проверка учетных данных
                if (login != "v_shutenko" || password != "8nEThznM")
                {
                    _logger.LogWarning("Invalid credentials for user: {Login}", login);
                    return Unauthorized(new ErrorResponse { Error = "Неверные учетные данные" });
                }

                // Создание сессии
                var sessionToken = Guid.NewGuid().ToString();
                _activeSessions[sessionToken] = new SessionInfo
                {
                    UserLogin = login,
                    CreatedAt = DateTime.Now,
                    ExpiresAt = DateTime.Now.AddHours(1)
                };

                _logger.LogInformation("User {Login} authenticated successfully. Session created.", login);

                return Ok(new LoginResponse
                {
                    Message = "Успешная аутентификация",
                    RedirectUrl = "https://ai-ecosystem-test.janusww.com:9999/request/model.html",
                    SessionToken = sessionToken
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user: {Login}", login);
                return StatusCode(500, new ErrorResponse { Error = "Внутренняя ошибка сервера" });
            }
        }

        [HttpPost("logout")]
        [ProducesResponseType(typeof(LogoutResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 401)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public IActionResult Logout([FromForm] string sessionToken)
        {
            try
            {
                // Проверяем существование сессии
                if (!_activeSessions.ContainsKey(sessionToken))
                {
                    _logger.LogWarning("Logout attempt with invalid session token");
                    return Unauthorized(new ErrorResponse { Error = "Сессия недействительна" });
                }

                // Удаляем сессию
                var userLogin = _activeSessions[sessionToken].UserLogin;
                _activeSessions.TryRemove(sessionToken, out _);

                _logger.LogInformation("User {UserLogin} logged out successfully", userLogin);
                return Ok(new LogoutResponse { Message = "Успешный выход из системы" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, new ErrorResponse { Error = "Ошибка при выходе из системы" });
            }
        }

        [HttpPost("check-session")]
        [ProducesResponseType(typeof(SessionInfoResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 401)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public IActionResult CheckSession([FromForm] string sessionToken)
        {
            try
            {
                if (!_activeSessions.ContainsKey(sessionToken))
                {
                    return Unauthorized(new ErrorResponse { Error = "Сессия недействительна" });
                }

                var session = _activeSessions[sessionToken];

                // Проверяем не истекла ли сессия
                if (DateTime.Now > session.ExpiresAt)
                {
                    _activeSessions.TryRemove(sessionToken, out _);
                    return Unauthorized(new ErrorResponse { Error = "Сессия истекла" });
                }

                return Ok(new SessionInfoResponse
                {
                    Valid = true,
                    UserLogin = session.UserLogin,
                    ExpiresAt = session.ExpiresAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during session check");
                return StatusCode(500, new ErrorResponse { Error = "Ошибка при проверке сессии" });
            }
        }
    }
}