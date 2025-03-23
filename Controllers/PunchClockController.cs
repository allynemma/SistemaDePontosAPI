using CsvHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaDePontosAPI.Model;
using System;
using System.Globalization;
using System.Net.Http.Headers;

namespace SistemaDePontosAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        public IActionResult Historico(int? id, DateTime? dataInicio, DateTime? dataFim)
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

            if (dataInicio.HasValue && dataFim.HasValue)
            {
                if (dataInicio > dataFim)
                {
                    _logger.LogWarning("Data de início não pode ser maior que a data final");
                    return BadRequest("Data de início não pode ser maior que a data final");
                }
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId");

                if (userIdClaim == null)
                {
                    _logger.LogWarning("Tentativa de criar ponto sem usuário autenticado");
                    return BadRequest("Usuário não autenticado");
                }

                int userId = int.Parse(userIdClaim.Value);

                var punchClocksQuery = _context.PunchClocks.AsQueryable();
                punchClocksQuery = punchClocksQuery.Where(p => p.Timestamp.Date >= dataInicio.Value.Date && p.Timestamp.Date <= dataFim.Value.Date);


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
                    date = punchClocks.Where(p => p.Timestamp.Date >= dataInicio && p.Timestamp.Date <= dataFim),
                    checkIns = punchClocks.Where(p => p.PunchClockType == PunchClockType.CheckIn).Select(p => p.Timestamp.TimeOfDay),
                    checkOuts = punchClocks.Where(p => p.PunchClockType == PunchClockType.CheckOut).Select(p => p.Timestamp.TimeOfDay),
                    hoursWorked = punchClocks.Where(p => p.PunchClockType == PunchClockType.CheckOut).Sum(p => p.Timestamp.Hour - 8)
                };


                return Ok(response);
            }
            else
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId");
                if (userIdClaim == null)
                {
                    _logger.LogWarning("Tentativa de criar ponto sem usuário autenticado");
                    return BadRequest("Usuário não autenticado");
                }
                int userId = int.Parse(userIdClaim.Value);
                var punchClocksQuery = _context.PunchClocks.AsQueryable();
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
                    date = punchClocks,
                    checkIns = punchClocks.Where(p => p.PunchClockType == PunchClockType.CheckIn).Select(p => p.Timestamp.TimeOfDay),
                    checkOuts = punchClocks.Where(p => p.PunchClockType == PunchClockType.CheckOut).Select(p => p.Timestamp.TimeOfDay),
                    hoursWorked = punchClocks.Where(p => p.PunchClockType == PunchClockType.CheckOut).Sum(p => p.Timestamp.Hour - 8)
                };
                return Ok(response);
            }


        }
        [Authorize(Roles = "admin")]
        [HttpGet("admin/punch-clock")]
        public IActionResult ListarPontos(int ? userId, DateTime? dataInicio, DateTime? dataFim)
        {
            if (!dataInicio.HasValue && !dataFim.HasValue)
            {

                var punchClocks = _context.PunchClocks.Join
                    (
                        _context.Users,
                        p => p.UserId,
                        u => u.Id,
                        (p, u) => new
                        {
                            p.Id,
                            p.UserId,
                            p.Timestamp,
                            p.PunchClockType,
                            u.Name
                        }
                    ).ToList();
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
            else
            {
                if (dataInicio > dataFim)
                {
                    _logger.LogWarning("Data de início não pode ser maior que a data final");
                    return BadRequest("Data de início não pode ser maior que a data final");
                }
                if (!dataInicio.HasValue)
                {
                    var punchClocks = _context.PunchClocks
                        .Where(p => p.Timestamp.Date <= dataFim.Value.Date)
                        .Join
                    (
                        _context.Users,
                        p => p.UserId,
                        u => u.Id,
                        (p, u) => new
                        {
                            p.Id,
                            p.UserId,
                            p.Timestamp,
                            p.PunchClockType,
                            u.Name
                        }
                    ).ToList();
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
                if (!dataFim.HasValue)
                {
                    var punchClocks = _context.PunchClocks
                        .Where(p => p.Timestamp.Date >= dataInicio.Value.Date)
                        .Join
                    (
                        _context.Users,
                        p => p.UserId,
                        u => u.Id,
                        (p, u) => new
                        {
                            p.Id,
                            p.UserId,
                            p.Timestamp,
                            p.PunchClockType,
                            u.Name
                        }
                    ).ToList();
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
                else
                {
                        var punchClocks = _context.PunchClocks
                            .Where(p => p.Timestamp.Date <= dataFim.Value.Date && p.Timestamp.Date >= dataInicio.Value.Date)
                            .Join
                        (
                            _context.Users,
                            p => p.UserId,
                            u => u.Id,
                            (p, u) => new
                            {
                                p.Id,
                                p.UserId,
                                p.Timestamp,
                                p.PunchClockType,
                                u.Name
                            }
                        ).ToList();
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
            }
            
        }


        [Authorize (Roles = "admin")]
        [HttpGet("admin/report")]
        public IActionResult GerarRelatorio (DateTime dataInicio, DateTime dataFim)
        {
            if (dataInicio > dataFim)
            {
                _logger.LogWarning("Data de início não pode ser maior que a data final");
                return BadRequest("Data de início não pode ser maior que a data final");
            }
            var punchClocks = _context.PunchClocks
                .Where(p => p.Timestamp.Date >= dataInicio.Date && p.Timestamp.Date <= dataFim.Date)
                .Select(p => new
                {
                    p.Id,
                    p.UserId,
                    p.Timestamp,
                    p.PunchClockType
                })
                .ToList();

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
