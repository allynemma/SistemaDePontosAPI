using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaDePontosAPI.Model;

namespace SistemaDePontosAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly Context _context;

        public UsersController(ILogger<UsersController> logger, Context context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpPost(Name = "PostUsers")]
        public async Task<IActionResult> Post([FromBody] Users user)
        {
            if (user == null)
            {
                _logger.LogWarning("Tentativa de criar usuário com dados nulos");
                return BadRequest("Dados inválidos");
            }

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
        }

        [HttpGet(Name = "GetUsers")]
        public async Task<IActionResult> Get(int? id)
        {
            if (id.HasValue)
            {
                var users = await _context.Users.FindAsync(id);
                if (users == null)
                {
                    _logger.LogWarning($"Usuário com id {id} não encontrado.");
                    return NotFound($"Usuário com id {id} não encontrado.");
                }
                return Ok(users);
            }
            var user = _context.Users.Select(index => new Users
            {
                Id = index.Id,
                Name = index.Name,
                Email = index.Email,
                Password = index.Password,
                Role = index.Role,
            }).ToArray();
            return Ok (user);
        }

        [HttpPut("{id}", Name = "PutUsers")]
        public async Task<IActionResult> Put(int id, [FromBody] Users updateUser)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                _logger.LogWarning($"Usuário com id {id} não encontrado para atualização.");
                return NotFound($"Usuário com id {id} não encontrado.");
            }

            user.Name = updateUser.Name;
            user.Password = updateUser.Password;
            user.Email = updateUser.Email;
            user.Role = updateUser.Role;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(user);
        }

        [HttpDelete("{id}", Name = "DeleteUser")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                _logger.LogWarning($"Usuário com id {id} não encontrado para exclusão.");
                return NotFound($"Usuário com id {id} não encontrado.");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
