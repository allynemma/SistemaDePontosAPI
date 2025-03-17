using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SistemaDePontosAPI.Model;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SistemaDePontosAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly Context _context;
        private readonly IConfiguration _configuration;

        public UsersController(ILogger<UsersController> logger, Context context, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _configuration = configuration;
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

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var response = new
            {
                user.Id,
                message = "Usuário criado com sucesso"
            };

            return CreatedAtAction(nameof(Get), new { id = user.Id }, response);
        }

        [AllowAnonymous]
        [HttpPost("auth/login")]
        public async Task<IActionResult> Login(string email, string senha)
        {
            if (email == null || senha == null)
            {
                _logger.LogWarning("Tentativa de login com dados nulos");
                return BadRequest("Dados inválidos");
            }
            var userDb = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.Password == senha);
            if (userDb == null)
            {
                _logger.LogWarning($"Usuário com email {email} não encontrado.");
                return NotFound("Usuário não encontrado");
            }
            var token = GenerateJwtToken(userDb.Email, userDb.Id);

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

        [AllowAnonymous]
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

        [AllowAnonymous]
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

        [HttpPost("generate-token")]
        private string GenerateJwtToken(string email, int userId)
        {
            var claims = new[]
             {
        new Claim(JwtRegisteredClaimNames.Sub, email),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim("UserId", userId.ToString()) // Adiciona o ID do usuário como uma claim
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("123456781234567812345678123456781234")); // Automatizar depois
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60), // Automatizar depois
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token); // Retornando a string do token
        }

    }
}
