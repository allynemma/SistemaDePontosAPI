using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaDePontosAPI.Model;

namespace SistemaDePontosAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PunchClockController : ControllerBase
    {
        private readonly ILogger<PunchClockController> _logger;
        private readonly Context _context;

        public PunchClockController(ILogger<PunchClockController> logger, Context context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpPost(Name = "PostPunchClock")]
        public async Task<IActionResult> Post([FromBody] PunchClock punchClock)
        {
            if (punchClock == null)
            {
                _logger.LogWarning("Tentativa de criar punchClock com dados nulos");
                return BadRequest("Dados inválidos");
            }

            await _context.PunchClocks.AddAsync(punchClock);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = punchClock.Id }, punchClock);
        }

        [HttpGet(Name = "GetPunchClock")]
        public async Task<IActionResult> Get(int? id)
        {
            if (id.HasValue)
            {
                var punchClock = await _context.PunchClocks.FindAsync(id);
                if (punchClock == null)
                {
                    _logger.LogWarning($"PunchClock com id {id} não encontrado.");
                    return NotFound($"PunchClock com id {id} não encontrado.");
                }
                return Ok(punchClock);
            }
            var punchClocks = _context.PunchClocks.Select(index => new PunchClock
            {
                Id = index.Id,
                UserId = index.UserId,
                Timestamp = index.Timestamp,
                Users = index.Users,
            }).ToArray();
            return Ok(punchClocks);
        }

        [HttpPut("{id}", Name = "PutPunchClocks")]
        public async Task<IActionResult> Put(int id, [FromBody] PunchClock punchClocks)
        {
            var punchClock = await _context.PunchClocks.FindAsync(id);
            if (punchClock == null)
            {
                _logger.LogWarning($"Usuário com id {id} não encontrado para atualização.");
                return NotFound($"Usuário com id {id} não encontrado.");
            }

            punchClock.Id = punchClocks.Id;
            punchClock.UserId = punchClocks.UserId;
            punchClock.Timestamp = punchClocks.Timestamp;
            punchClock.Users = punchClocks.Users;

            _context.PunchClocks.Update(punchClock);
            await _context.SaveChangesAsync();

            return Ok(punchClocks);
        }

        [HttpDelete("{id}", Name = "DeletePunchClocks")]
        public async Task<IActionResult> Delete(int id)
        {
            var punchClocks = await _context.PunchClocks.FindAsync(id);
            if (punchClocks == null)
            {
                _logger.LogWarning($"PunchClock com id {id} não encontrado para exclusão.");
                return NotFound($"PunchClock com id {id} não encontrado.");
            }

            _context.PunchClocks.Remove(punchClocks);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
