using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaDePontosAPI.Model;

namespace SistemaDePontosAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SettingsController : ControllerBase
    {
        private readonly ILogger<SettingsController> _logger;
        private readonly Context _context;

        public SettingsController(ILogger<SettingsController> logger, Context context)
        {
            _logger = logger;
            _context = context;
        }

        [Authorize(Roles = "admin")]
        [HttpPost(Name = "PostSettings")]
        public IActionResult Post([FromBody] Settings settings)
        {
            if (!User.IsInRole("admin"))
            {
                return Unauthorized();
            }

            if (settings == null)
            {
                _logger.LogWarning("Tentativa de criar settings com dados nulos");
                return BadRequest("Dados inválidos");
            }

            settings.Id = 0;

            _context.Settings.Add(settings);
            _context.SaveChanges();

            return CreatedAtAction(nameof(Get), new { id = settings.Id }, settings);
        }

        [Authorize(Roles = "admin")]
        [HttpGet(Name = "GetSettings")]
        public IActionResult Get(int? id)
        {
            if (!User.IsInRole("admin"))
            {
                return Unauthorized();
            }
            if (id.HasValue)
            {
                var settings = _context.Settings.Find(id);
                if (settings == null)
                {
                    _logger.LogWarning($"Settings com id {id} não encontrado.");
                    return NotFound($"Settings com id {id} não encontrado.");
                }
                return Ok(settings);
            }

            var setting = _context.Settings.Select(index => new Settings
            {
                Id = index.Id,
                Overtime_Rate = index.Overtime_Rate,
                Workday_Hours = index.Workday_Hours
            }).ToArray();

            return Ok(setting);
        }

        [Authorize(Roles = "admin")]
        [HttpPut("{id}", Name = "PutSettings")]
        public IActionResult Put(int id, [FromBody] Settings settings)
        {
            if (!User.IsInRole("admin"))
            {
                return Unauthorized();
            }
            var setting = _context.Settings.Find(id);
            if (setting == null)
            {
                _logger.LogWarning($"Settings com id {id} não encontrado para atualização.");
                return NotFound($"Settings com id {id} não encontrado.");
            }

            setting.Overtime_Rate = settings.Overtime_Rate;
            setting.Workday_Hours = settings.Workday_Hours;

            _context.Settings.Update(setting);
            _context.SaveChanges();

            return Ok(setting);
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id}", Name = "DeleteSettings")]
        public IActionResult Delete(int id)
        {
            if (!User.IsInRole("admin"))
            {
                return Unauthorized();
            }
            var setting = _context.Settings.Find(id);
            if (setting == null)
            {
                _logger.LogWarning($"Settings com id {id} não encontrado para exclusão.");
                return NotFound($"Settings com id {id} não encontrado.");
            }

            _context.Settings.Remove(setting);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
