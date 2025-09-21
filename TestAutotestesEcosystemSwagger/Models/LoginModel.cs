using System.ComponentModel.DataAnnotations;

namespace TestAutotestesEcosystemSwagger.Models
{
    /// <summary>
    /// Модель для учетных данных аутентификации
    /// </summary>
    public class LoginModel
    {
        /// <summary>
        /// Логин пользователя
        /// </summary>
        [Required(ErrorMessage = "Login is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Login must be between 3 and 50 characters")]
        public string Login { get; set; } = string.Empty;

        /// <summary>
        /// Пароль пользователя
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// URL для тестирования аутентификации
        /// </summary>
        [Required(ErrorMessage = "TestLoginUrl is required")]
        [Url(ErrorMessage = "TestLoginUrl must be a valid URL")]
        public string TestLoginUrl { get; set; } = "https://ai-ecosystem-test.janusww.com:9999/auth/login.html";
    }
}