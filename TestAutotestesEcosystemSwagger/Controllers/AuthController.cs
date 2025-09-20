using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TestAutotestesEcosystemSwagger.Models;

namespace TestAutotestesEcosystemSwagger.Controllers
{
    /// <summary>
    /// Контроллер для управления учетными данными аутентификации
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private static LoginModel _currentCredentials;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
        {
            _configuration = configuration;
            _logger = logger;
            
            if (_currentCredentials == null)
            {
                LoadCredentialsFromConfig();
            }
        }

        private void LoadCredentialsFromConfig()
        {
            _currentCredentials = new LoginModel
            {
                Login = _configuration["AuthSettings:DefaultLogin"] ?? "v_shutenko",
                Password = _configuration["AuthSettings:DefaultPassword"] ?? "8nEThznM",
                TestLoginUrl = _configuration["AuthSettings:DefaultTestLoginUrl"] ?? "https://ai-ecosystem-test.janusww.com:9999/auth/login.html"
            };

            _logger.LogInformation("Credentials loaded from configuration: Login={Login}, TestLoginUrl={TestLoginUrl}", 
                _currentCredentials.Login, _currentCredentials.TestLoginUrl);
        }

        /// <summary>
        /// Получить текущие учетные данные для тестов
        /// </summary>
        /// <returns>Текущие настройки учетных данных</returns>
        [HttpGet("credentials")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetCredentials()
        {
            _logger.LogDebug("GetCredentials requested");
            
            return Ok(new
            {
                _currentCredentials.Login,
                Password = "******",
                _currentCredentials.TestLoginUrl,
                Source = "Runtime memory (can be updated via POST)"
            });
        }

        /// <summary>
        /// Обновить учетные данные для тестов (в памяти приложения)
        /// </summary>
        /// <param name="model">Модель с новыми учетными данными</param>
        /// <returns>Результат обновления</returns>
        [HttpPost("credentials")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdateCredentials([FromBody] LoginModel model)
        {
            _logger.LogInformation("UpdateCredentials requested for login: {Login}", model.Login);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("UpdateCredentials validation failed");
                return BadRequest(ModelState);
            }

            _currentCredentials = model;
            
            _logger.LogInformation("Credentials updated successfully for login: {Login}", model.Login);

            return Ok(new
            {
                Message = "Credentials updated successfully in runtime memory",
                Login = model.Login,
                Password = "******",
                TestLoginUrl = model.TestLoginUrl,
                Note = "These settings are stored in memory and will reset on application restart"
            });
        }

        /// <summary>
        /// Получить учетные данные из конфигурации (только для чтения)
        /// </summary>
        /// <returns>Учетные данные из файла конфигурации</returns>
        [HttpGet("credentials/config")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetConfigCredentials()
        {
            _logger.LogDebug("GetConfigCredentials requested");
            
            var configCredentials = new
            {
                Login = _configuration["AuthSettings:DefaultLogin"] ?? "v_shutenko",
                Password = "******",
                TestLoginUrl = _configuration["AuthSettings:DefaultTestLoginUrl"] ?? "https://ai-ecosystem-test.janusww.com:9999/auth/login.html",
                Source = "Configuration file (appsettings.json)"
            };

            return Ok(configCredentials);
        }

        /// <summary>
        /// Сбросить учетные данные к значениям из конфигурации
        /// </summary>
        /// <returns>Результат сброса</returns>
        [HttpPost("credentials/reset")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult ResetCredentials()
        {
            _logger.LogInformation("ResetCredentials requested");
            
            LoadCredentialsFromConfig();

            return Ok(new
            {
                Message = "Credentials reset to configuration values",
                Login = _currentCredentials.Login,
                Password = "******",
                TestLoginUrl = _currentCredentials.TestLoginUrl,
                Source = "Configuration file (appsettings.json)"
            });
        }

        /// <summary>
        /// Валидировать учетные данные
        /// </summary>
        /// <param name="model">Учетные данные для валидации</param>
        /// <returns>Результат валидации</returns>
        [HttpPost("validate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult ValidateCredentials([FromBody] LoginModel model)
        {
            _logger.LogDebug("ValidateCredentials requested for login: {Login}", model.Login);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ValidateCredentials validation failed");
                return BadRequest(ModelState);
            }

            bool isValid = !string.IsNullOrEmpty(model.Login) && 
                          !string.IsNullOrEmpty(model.Password) && 
                          !string.IsNullOrEmpty(model.TestLoginUrl);

            _logger.LogInformation("Credentials validation result: {IsValid} for login: {Login}", isValid, model.Login);

            return Ok(new
            {
                Valid = isValid,
                Message = isValid ? "Credentials are valid" : "Credentials are invalid",
                Details = new
                {
                    HasLogin = !string.IsNullOrEmpty(model.Login),
                    HasPassword = !string.IsNullOrEmpty(model.Password),
                    HasTestLoginUrl = !string.IsNullOrEmpty(model.TestLoginUrl),
                    LoginLength = model.Login?.Length ?? 0,
                    PasswordLength = model.Password?.Length ?? 0
                }
            });
        }

        /// <summary>
        /// Валидировать текущие учетные данные (из памяти)
        /// </summary>
        /// <returns>Результат валидации текущих учетных данных</returns>
        [HttpGet("validate/current")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult ValidateCurrentCredentials()
        {
            _logger.LogDebug("ValidateCurrentCredentials requested");

            bool isValid = !string.IsNullOrEmpty(_currentCredentials.Login) && 
                          !string.IsNullOrEmpty(_currentCredentials.Password) && 
                          !string.IsNullOrEmpty(_currentCredentials.TestLoginUrl);

            _logger.LogInformation("Current credentials validation result: {IsValid} for login: {Login}", 
                isValid, _currentCredentials.Login);

            return Ok(new
            {
                Valid = isValid,
                Message = isValid ? "Current credentials are valid" : "Current credentials are invalid",
                Credentials = new
                {
                    Login = _currentCredentials.Login,
                    Password = "******",
                    TestLoginUrl = _currentCredentials.TestLoginUrl
                },
                Details = new
                {
                    HasLogin = !string.IsNullOrEmpty(_currentCredentials.Login),
                    HasPassword = !string.IsNullOrEmpty(_currentCredentials.Password),
                    HasTestLoginUrl = !string.IsNullOrEmpty(_currentCredentials.TestLoginUrl)
                }
            });
        }
    }
}