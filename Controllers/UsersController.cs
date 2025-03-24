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
                _logger.LogWarning("Tentativa de criar usuário com dados nulos");
                return BadRequest("Dados inválidos");
            }

            var createdUser = await _userService.Register(user);

            var response = new
            {
                createdUser.Id,
                message = "Usuário criado com sucesso"
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
                return BadRequest("Dados inválidos");
            }

            var userDb = _userService.Authenticate(email, password);
            if (userDb == null)
            {
                _logger.LogWarning($"Usuário com email {email} não encontrado.");
                return NotFound("Usuário não encontrado");
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
                    _logger.LogWarning($"Usuário com id {id} não encontrado.");
                    return NotFound($"Usuário com id {id} não encontrado.");
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
                _logger.LogWarning("Tentativa de atualizar usuário com dados nulos");
                return BadRequest("Dados inválidos");
            }

            var user = await _userService.UpdateUser(id, updateUser);
            if (user == null)
            {
                _logger.LogWarning($"Usuário com id {id} não encontrado para atualização.");
                return NotFound($"Usuário com id {id} não encontrado.");
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
                _logger.LogWarning($"Usuário com id {id} não encontrado para exclusão.");
                return NotFound($"Usuário com id {id} não encontrado.");
            }

            return NoContent();
        }
    }
}
