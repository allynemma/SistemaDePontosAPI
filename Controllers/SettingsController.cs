using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        [HttpPost(Name = "PostSettings")]
        public async Task<IActionResult> Post([FromBody] Settings settings)
        {
            if (settings == null)
            {
                _logger.LogWarning("Tentativa de criar settings com dados nulos");
                return BadRequest("Dados inválidos");
            }

            settings.Id = 0;

            await _context.Settings.AddAsync(settings);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = settings.Id }, settings);
        }

        [HttpGet(Name = "GetSettings")]
        public async Task<IActionResult> Get(int? id)
        {
            if (id.HasValue)
            {
                var settings = await _context.Settings.FindAsync(id);
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

        [HttpPut("{id}", Name = "PutSettings")]
        public async Task<IActionResult> Put(int id, [FromBody] Settings settings)
        {
            var setting = await _context.Settings.FindAsync(id);
            if (setting == null)
            {
                _logger.LogWarning($"Settings com id {id} não encontrado para atualização.");
                return NotFound($"Settings com id {id} não encontrado.");
            }

            setting.Overtime_Rate = settings.Overtime_Rate;
            setting.Workday_Hours = settings.Workday_Hours;

            _context.Settings.Update(setting);
            await _context.SaveChangesAsync();

            return Ok(setting);
        }

        [HttpDelete("{id}", Name = "DeleteSettings")]
        public async Task<IActionResult> Delete(int id)
        {
            var setting = await _context.Settings.FindAsync(id);
            if (setting == null)
            {
                _logger.LogWarning($"Settings com id {id} não encontrado para exclusão.");
                return NotFound($"Settings com id {id} não encontrado.");
            }

            _context.Settings.Remove(setting);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
