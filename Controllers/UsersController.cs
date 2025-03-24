using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaDePontosAPI.Model;
using SistemaDePontosAPI.Services;

namespace SistemaDePontosAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly IUserService _userService;

        public UsersController(ILogger<UsersController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Users user)
        {
            if (user == null)
            {
                _logger.LogWarning("Tentativa de criar usu�rio com dados nulos");
                return BadRequest("Dados inv�lidos");
            }

            var createdUser = await _userService.Register(user);

            var response = new
            {
                createdUser.Id,
                message = "Usu�rio criado com sucesso"
            };

            return CreatedAtAction(nameof(Get), new { id = createdUser.Id }, response);
        }

        [AllowAnonymous]
        [HttpPost("auth/login")]
        public IActionResult Login(string email, string password)
        {
            if (email == null || password == null)
            {
                _logger.LogWarning("Tentativa de login com dados nulos");
                return BadRequest("Dados inv�lidos");
            }

            var userDb = _userService.Authenticate(email, password);
            if (userDb == null)
            {
                _logger.LogWarning($"Usu�rio com email {email} n�o encontrado.");
                return NotFound("Usu�rio n�o encontrado");
            }

            var token = _userService.GenerateJwtToken(userDb.Email, userDb.Id);

            HttpContext.Items["AuthToken"] = token;

            var response = new
            {
                token = token,
                userDb.Id
            };

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpGet(Name = "GetUsers")]
        public async Task<IActionResult> Get(int? id)
        {
            if (id.HasValue)
            {
                var user = await _userService.GetUserById(id.Value);
                if (user == null)
                {
                    _logger.LogWarning($"Usu�rio com id {id} n�o encontrado.");
                    return NotFound($"Usu�rio com id {id} n�o encontrado.");
                }
                return Ok(user);
            }

            var users = await _userService.GetAllUsers();
            return Ok(users);
        }

        [AllowAnonymous]
        [HttpPut("{id}", Name = "PutUsers")]
        public async Task<IActionResult> Put(int id, [FromBody] Users updateUser)
        {
            if (updateUser == null)
            {
                _logger.LogWarning("Tentativa de atualizar usu�rio com dados nulos");
                return BadRequest("Dados inv�lidos");
            }

            var user = await _userService.UpdateUser(id, updateUser);
            if (user == null)
            {
                _logger.LogWarning($"Usu�rio com id {id} n�o encontrado para atualiza��o.");
                return NotFound($"Usu�rio com id {id} n�o encontrado.");
            }

            return Ok(user);
        }

        [AllowAnonymous]
        [HttpDelete("{id}", Name = "DeleteUser")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _userService.DeleteUser(id);
            if (!result)
            {
                _logger.LogWarning($"Usu�rio com id {id} n�o encontrado para exclus�o.");
                return NotFound($"Usu�rio com id {id} n�o encontrado.");
            }

            return NoContent();
        }
    }
}
