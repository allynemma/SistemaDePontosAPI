using Microsoft.AspNetCore.Authorization;
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

        public PunchClockController(Context context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPost(Name = "PostPunchClock")]
        public IActionResult ResgistroDePonto([FromBody] PunchClockType punchClockType)
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

            if (punchClockType == PunchClockType.CheckIn && HasCheckedInToday(userId))
            {
                _logger.LogWarning("Usuário já fez check-in hoje");
                return BadRequest("Usuário já fez check-in hoje");
            }

            if (punchClockType == PunchClockType.CheckOut && HasCheckedOutToday(userId))
            {
                _logger.LogWarning("Usuário já fez check-out hoje");
                return BadRequest("Usuário já fez check-out hoje");
            }

            var punchClock = new PunchClock
            {
                UserId = userId,
                Timestamp = DateTime.Now,
                PunchClockType = punchClockType
            };

            _context.PunchClocks.Add(punchClock);
            _context.SaveChanges();

            var response = new
            {
                message = "Ponto registrado com sucesso",
                timestamp = punchClock.Timestamp
            };

            return CreatedAtAction(nameof(Historico), new { id = punchClock.Id }, response);
        }

        [Authorize]
        [HttpGet("history")]
        public IActionResult Historico(int? id, DateTime? date)
        {
            if (id.HasValue)
            {
                var punchClock = _context.PunchClocks.Find(id);
                if (punchClock == null)
                {
                    _logger.LogWarning($"PunchClock com id {id} não encontrado.");
                    return NotFound($"PunchClock com id {id} não encontrado.");
                }
                return Ok(punchClock);
            }

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

            var punchClocks = punchClocksQuery
                .Where(p => p.UserId == userId)
                .Select(p => new
                {
                    p.Id,
                    p.UserId,
                    p.Timestamp,
                    p.PunchClockType
                })
                .ToList();

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
        public IActionResult Put(int id, [FromBody] PunchClock punchClocks)
        {
            var punchClock = _context.PunchClocks.Find(id);
            if (punchClock == null)
            {
                _logger.LogWarning($"Usuário com id {id} não encontrado para atualização.");
                return NotFound($"Usuário com id {id} não encontrado.");
            }

            punchClock.UserId = punchClocks.UserId;
            punchClock.Timestamp = punchClocks.Timestamp;
            punchClock.PunchClockType = punchClocks.PunchClockType;

            _context.PunchClocks.Update(punchClock);
            _context.SaveChanges();

            return Ok(punchClocks);
        }

        [HttpDelete("{id}", Name = "DeletePunchClocks")]
        public IActionResult Delete(int id)
        {
            var punchClocks = _context.PunchClocks.Find(id);
            if (punchClocks == null)
            {
                _logger.LogWarning($"PunchClock com id {id} não encontrado para exclusão.");
                return NotFound($"PunchClock com id {id} não encontrado.");
            }

            _context.PunchClocks.Remove(punchClocks);
            _context.SaveChanges();

            return NoContent();
        }
        private bool HasCheckedInToday(int userId)
        {
            var today = DateTime.Today;
            return _context.PunchClocks.Any(p => p.UserId == userId && p.Timestamp.Date == today && p.PunchClockType == PunchClockType.CheckIn);
        }

        private bool HasCheckedOutToday(int userId)
        {
            var today = DateTime.Today;
            return _context.PunchClocks.Any(p => p.UserId == userId && p.Timestamp.Date == today && p.PunchClockType == PunchClockType.CheckOut);
        }
    }
}
