using CsvHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaDePontosAPI.Model;
using SistemaDePontosAPI.Services;
using System.Globalization;
using SistemaDePontosAPI.Mensageria;

namespace SistemaDePontosAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PunchClockController : ControllerBase
    {
        private readonly ILogger<PunchClockController> _logger;
        private readonly IPunchClockService _punchClockService;
        private readonly KafkaProducer _kafkaProducer;

        public PunchClockController(ILogger<PunchClockController> logger, IPunchClockService punchClockService, KafkaProducer kafkaProducer)
        {
            _logger = logger;
            _punchClockService = punchClockService;
            _kafkaProducer = kafkaProducer;
        }

        [Authorize]
        [HttpPost(Name = "PostPunchClock")]
        public async Task<IActionResult> ResgistroDePonto([FromBody] PunchClockType punchClockType)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId");

            if (userIdClaim == null)
            {
                _logger.LogWarning("Tentativa de criar ponto sem usuário autenticado");
                return BadRequest("Usuário não autenticado");
            }

            if (punchClockType != PunchClockType.CheckIn && punchClockType != PunchClockType.CheckOut)
            {
                _logger.LogWarning("Tentativa de criar ponto sem falar se é check-in ou check-out");
                return BadRequest("Necessário ser check-in ou check-out");
            }

            int userId = int.Parse(userIdClaim.Value);

            if (punchClockType == PunchClockType.CheckIn && _punchClockService.HasCheckedInToday(userId))
            {
                _logger.LogWarning("Usuário já fez check-in hoje");
                return BadRequest("Usuário já fez check-in hoje");
            }

            if (punchClockType == PunchClockType.CheckOut && _punchClockService.HasCheckedOutToday(userId))
            {
                _logger.LogWarning("Usuário já fez check-out hoje");
                return BadRequest("Usuário já fez check-out hoje");
            }

            var punchClock = await _punchClockService.RegisterPunchClock(userId, punchClockType);

            var message = $"Ponto registrado para o usuário {userId} às {punchClock.Timestamp}";
            await _kafkaProducer.SendMessageAsync(message);

            var response = new
            {
                message = "Ponto registrado com sucesso",
                timestamp = punchClock.Timestamp
            };

            return CreatedAtAction(nameof(Historico), new { id = punchClock.Id }, response);
        }

        [Authorize]
        [HttpGet("history")]
        public async Task<IActionResult> Historico(int? id, DateTime? dataInicio, DateTime? dataFim)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId");
            if (userIdClaim == null)
            {
                _logger.LogWarning("Tentativa de criar ponto sem usuário autenticado");
                return BadRequest("Usuário não autenticado");
            }

            if (dataInicio.HasValue && dataFim.HasValue && dataInicio > dataFim)
            {
                _logger.LogWarning("Data de início não pode ser maior que a data final");
                return BadRequest("Data de início não pode ser maior que a data final");
            }

            int userId = int.Parse(userIdClaim.Value);

            if (id.HasValue)
            {
                var punchClock = await _punchClockService.GetPunchClockById(id.Value);
                if (punchClock == null)
                {
                    _logger.LogWarning($"PunchClock com id {id} não encontrado.");
                    return NotFound($"PunchClock com id {id} não encontrado.");
                }
                return Ok(punchClock);
            }

            var punchClocks = await _punchClockService.GetPunchClocksByUserId(userId, dataInicio, dataFim);

            var response = new
            {
                date = punchClocks,
                checkIns = punchClocks.Where(p => p.PunchClockType == PunchClockType.CheckIn).Select(p => p.Timestamp.TimeOfDay),
                checkOuts = punchClocks.Where(p => p.PunchClockType == PunchClockType.CheckOut).Select(p => p.Timestamp.TimeOfDay),
                hoursWorked = punchClocks.Where(p => p.PunchClockType == PunchClockType.CheckOut).Sum(p => p.Timestamp.Hour - 8)
            };

            return Ok(response);
        }

        [Authorize(Roles = "admin")]
        [HttpGet("admin/punch-clock")]
        public async Task<IActionResult> ListarPontos(int? userId, DateTime? dataInicio, DateTime? dataFim)
        {
            if (!User.IsInRole("admin"))
            {
                return Unauthorized();
            }

            if (dataInicio.HasValue && dataFim.HasValue && dataInicio > dataFim)
            {
                _logger.LogWarning("Data de início não pode ser maior que a data final");
                return BadRequest("Data de início não pode ser maior que a data final");
            }

            var punchClocks = await _punchClockService.GetAllPunchClocks(userId, dataInicio, dataFim);

            var response = new
            {
                employee = punchClocks.Select(p => p.UserId),
                date = punchClocks.Select(p => p.Timestamp.Date),
                checkIns = punchClocks.Where(p => p.PunchClockType == PunchClockType.CheckIn).Select(p => p.Timestamp.TimeOfDay),
                checkOuts = punchClocks.Where(p => p.PunchClockType == PunchClockType.CheckOut).Select(p => p.Timestamp.TimeOfDay),
                hoursWorked = punchClocks.Where(p => p.PunchClockType == PunchClockType.CheckOut).Sum(p => p.Timestamp.Hour - 8)
            };

            return Ok(response);
        }

        [Authorize(Roles = "admin")]
        [HttpGet("admin/report")]
        public async Task<IActionResult> GerarRelatorio(DateTime dataInicio, DateTime dataFim)
        {
            if (!User.IsInRole("admin"))
            {
                return Unauthorized();
            }

            if (dataInicio > dataFim)
            {
                _logger.LogWarning("Data de início não pode ser maior que a data final");
                return BadRequest("Data de início não pode ser maior que a data final");
            }

            var punchClocks = await _punchClockService.GetPunchClocksForReport(dataInicio, dataFim);

            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
            {
                csvWriter.WriteRecords(punchClocks);
                streamWriter.Flush();
                var result = memoryStream.ToArray();
                return File(result, "text/csv", "relatorio_pontos.csv");
            }
        }

        [HttpPut("{id}", Name = "PutPunchClocks")]
        public async Task<IActionResult> Put(int id, [FromBody] PunchClock punchClocks)
        {
            var updatedPunchClock = await _punchClockService.UpdatePunchClock(id, punchClocks);
            if (updatedPunchClock == null)
            {
                _logger.LogWarning($"Usuário com id {id} não encontrado para atualização.");
                return NotFound($"Usuário com id {id} não encontrado.");
            }

            return Ok(updatedPunchClock);
        }

        [HttpDelete("{id}", Name = "DeletePunchClocks")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _punchClockService.DeletePunchClock(id);
            if (!result)
            {
                _logger.LogWarning($"PunchClock com id {id} não encontrado para exclusão.");
                return NotFound($"PunchClock com id {id} não encontrado.");
            }

            return NoContent();
        }
    }
}

