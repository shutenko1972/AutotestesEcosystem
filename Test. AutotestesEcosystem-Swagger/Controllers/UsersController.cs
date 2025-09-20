using Microsoft.AspNetCore.Mvc;

namespace SwaggerApiProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private static List<User> _users = new()
        {
            new User { Id = 1, Name = "John Doe", Email = "john@example.com", Username = "johndoe" },
            new User { Id = 2, Name = "Jane Smith", Email = "jane@example.com", Username = "janesmith" }
        };

        /// <summary>
        /// Получить всех пользователей
        /// </summary>
        /// <returns>Список пользователей</returns>
        [HttpGet]
        public IActionResult GetUsers()
        {
            return Ok(_users);
        }

        /// <summary>
        /// Получить пользователя по ID
        /// </summary>
        /// <param name="id">ID пользователя</param>
        /// <returns>Данные пользователя</returns>
        [HttpGet("{id}")]
        public IActionResult GetUser(int id)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound(new { message = $"User with ID {id} not found" });

            return Ok(user);
        }

        /// <summary>
        /// Создать нового пользователя
        /// </summary>
        /// <param name="user">Данные пользователя</param>
        /// <returns>Созданный пользователь</returns>
        [HttpPost]
        public IActionResult CreateUser([FromBody] User user)
        {
            // Валидация
            if (string.IsNullOrEmpty(user.Name))
                return BadRequest(new { message = "Name is required" });

            if (string.IsNullOrEmpty(user.Email))
                return BadRequest(new { message = "Email is required" });

            if (!user.Email.Contains('@'))
                return BadRequest(new { message = "Invalid email format" });

            // Генерируем ID
            user.Id = _users.Count > 0 ? _users.Max(u => u.Id) + 1 : 1;

            // Если username не указан, генерируем из email
            if (string.IsNullOrEmpty(user.Username))
            {
                user.Username = user.Email.Split('@')[0];
            }

            _users.Add(user);

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        /// <summary>
        /// Обновить пользователя
        /// </summary>
        /// <param name="id">ID пользователя</param>
        /// <param name="user">Обновленные данные</param>
        /// <returns>Результат операции</returns>
        [HttpPut("{id}")]
        public IActionResult UpdateUser(int id, [FromBody] User user)
        {
            var existingUser = _users.FirstOrDefault(u => u.Id == id);
            if (existingUser == null)
                return NotFound(new { message = $"User with ID {id} not found" });

            existingUser.Name = user.Name;
            existingUser.Email = user.Email;
            existingUser.Username = user.Username;

            return Ok(existingUser);
        }

        /// <summary>
        /// Удалить пользователя
        /// </summary>
        /// <param name="id">ID пользователя</param>
        /// <returns>Результат операции</returns>
        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound(new { message = $"User with ID {id} not found" });

            _users.Remove(user);
            return Ok(new { message = $"User with ID {id} deleted successfully" });
        }
    }

    /// <summary>
    /// Модель пользователя
    /// </summary>
    public class User
    {
        /// <summary>
        /// ID пользователя
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Email пользователя
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Имя пользователя (никнейм)
        /// </summary>
        public string Username { get; set; } = string.Empty;
    }
}