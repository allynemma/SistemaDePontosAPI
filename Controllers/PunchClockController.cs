using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaDePontosAPI.Model;
using System;
using System.Net.Http.Headers;

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
        public async Task<IActionResult> ResgistroDePonto([FromBody]PunchClockType punchClockType)
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

            var punchClock = new PunchClock
            {
                UserId = userId,
                Timestamp = DateTime.Now

            };

            await _context.PunchClocks.AddAsync(punchClock);
            await _context.SaveChangesAsync();

            var response = new
            {
                message = "Ponto registrado com sucesso",
                timestamp = punchClock.Timestamp
            };

            return CreatedAtAction(nameof(Historico), new { id = punchClock.Id }, response);
        }

        [HttpGet("history")]
        public async Task<IActionResult> Historico(int? id, DateTime? date)
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

            var token = HttpContext.Items["AuthToken"] as string;
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{token}");
            client.DefaultRequestHeaders.Add("Custom-Header", "valor");

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId");

            if (userIdClaim == null)
            {
                _logger.LogWarning("Tentativa de criar ponto sem usuário autenticado");
                return BadRequest("Usuário não autenticado");
            }

            int userId = int.Parse(userIdClaim.Value);

            var punchClocksQuery = _context.PunchClocks.AsQueryable();

            if (date.HasValue)
            {
                punchClocksQuery = punchClocksQuery.Where(p => p.Timestamp.Date == date.Value.Date);
            }

            var punchClocks = await punchClocksQuery
                .Where(p => p.UserId == userId)
                .Select(p => new
                {
                    p.Id,
                    p.UserId,
                    p.Timestamp,
                    p.PunchClockType
                })
                .ToListAsync();

            var response = new
            {
                date = date?.ToString("dd/MM/yyyy"),
                checkIns = punchClocks.Where(p => p.PunchClockType == PunchClockType.CheckIn).Select(p => p.Timestamp.TimeOfDay),
                checkOuts = punchClocks.Where(p => p.PunchClockType == PunchClockType.CheckOut).Select(p => p.Timestamp.TimeOfDay),
                hoursWorked = punchClocks.Where(p => p.PunchClockType == PunchClockType.CheckOut).Sum(p => p.Timestamp.Hour - 8)
            };

            return Ok(response);
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
            //punchClock.Users = punchClocks.Users;

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
