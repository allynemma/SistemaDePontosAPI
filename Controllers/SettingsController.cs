using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaDePontosAPI.Model;
using SistemaDePontosAPI.Services;
using SistemaDePontosAPI.Mensageria;
using System.Threading.Tasks;

namespace SistemaDePontosAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SettingsController : ControllerBase
    {
        private readonly ILogger<SettingsController> _logger;
        private readonly ISettingsService _settingsService;
        private readonly KafkaProducer _kafkaProducer;

        public SettingsController(ILogger<SettingsController> logger, ISettingsService settingsService, KafkaProducer kafkaProducer)
        {
            _logger = logger;
            _settingsService = settingsService;
            _kafkaProducer = kafkaProducer;
        }

        [Authorize(Roles = "admin")]
        [HttpPost(Name = "PostSettings")]
        public async Task<IActionResult> Post([FromBody] Settings settings)
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

            var mensagem = $"Settings criado com sucesso";
            await _kafkaProducer.SendMessageAsync(mensagem);

            var createdSettings = await _settingsService.CreateSettings(settings);

            return CreatedAtAction(nameof(Get), new { id = createdSettings.Id }, createdSettings);
        }

        [Authorize(Roles = "admin")]
        [HttpGet(Name = "GetSettings")]
        public async Task<IActionResult> Get(int? id)
        {
            if (!User.IsInRole("admin"))
            {
                return Unauthorized();
            }

            if (id.HasValue)
            {
                var settings = await _settingsService.GetSettingsById(id.Value);
                if (settings == null)
                {
                    _logger.LogWarning($"Settings com id {id} não encontrado.");
                    return NotFound($"Settings com id {id} não encontrado.");
                }
                return Ok(settings);
            }

            var allSettings = await _settingsService.GetAllSettings();
            return Ok(allSettings);
        }

        [Authorize(Roles = "admin")]
        [HttpPut("{id}", Name = "PutSettings")]
        public async Task<IActionResult> Put(int id, [FromBody] Settings settings)
        {
            if (!User.IsInRole("admin"))
            {
                return Unauthorized();
            }

            var updatedSettings = await _settingsService.UpdateSettings(id, settings);
            if (updatedSettings == null)
            {
                _logger.LogWarning($"Settings com id {id} não encontrado para atualização.");
                return NotFound($"Settings com id {id} não encontrado.");
            }

            var mensagem = $"Settings com id {id} atualizado com sucesso";
            await _kafkaProducer.SendMessageAsync(mensagem);

            return Ok(updatedSettings);
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id}", Name = "DeleteSettings")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!User.IsInRole("admin"))
            {
                return Unauthorized();
            }

            var result = await _settingsService.DeleteSettings(id);
            if (!result)
            {
                _logger.LogWarning($"Settings com id {id} não encontrado para exclusão.");
                return NotFound($"Settings com id {id} não encontrado.");
            }

            var mensagem = $"Settings com id {id} excluído com sucesso";
            await _kafkaProducer.SendMessageAsync(mensagem);

            return NoContent();
        }
    }
}
