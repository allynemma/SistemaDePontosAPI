using Microsoft.EntityFrameworkCore;
using SistemaDePontosAPI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaDePontosAPI.Services
{
    public class PunchClockService : IPunchClockService
    {
        private readonly Context _context;

        public PunchClockService(Context context)
        {
            _context = context;
        }

        public async Task<PunchClock> RegisterPunchClock(int userId, PunchClockType punchClockType)
        {
            var punchClock = new PunchClock
            {
                UserId = userId,
                Timestamp = DateTime.Now,
                PunchClockType = punchClockType
            };

            _context.PunchClocks.Add(punchClock);
            await _context.SaveChangesAsync();

            return punchClock;
        }

        public async Task<PunchClock> GetPunchClockById(int id)
        {
            return await _context.PunchClocks.FindAsync(id);
        }

        public async Task<IEnumerable<PunchClock>> GetPunchClocksByUserId(int userId, DateTime? dataInicio, DateTime? dataFim)
        {
            var query = _context.PunchClocks.AsQueryable();

            if (dataInicio.HasValue && dataFim.HasValue)
            {
                query = query.Where(p => p.Timestamp.Date >= dataInicio.Value.Date && p.Timestamp.Date <= dataFim.Value.Date);
            }

            return await query.Where(p => p.UserId == userId).ToListAsync();
        }

        public async Task<IEnumerable<PunchClock>> GetAllPunchClocks(int? userId, DateTime? dataInicio, DateTime? dataFim)
        {
            var query = _context.PunchClocks.AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(p => p.UserId == userId);
            }

            if (dataInicio.HasValue)
            {
                query = query.Where(p => p.Timestamp.Date >= dataInicio.Value.Date);
            }

            if (dataFim.HasValue)
            {
                query = query.Where(p => p.Timestamp.Date <= dataFim.Value.Date);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<PunchClock>> GetPunchClocksForReport(DateTime dataInicio, DateTime dataFim)
        {
            return await _context.PunchClocks
                .Where(p => p.Timestamp.Date >= dataInicio.Date && p.Timestamp.Date <= dataFim.Date)
                .ToListAsync();
        }

        public async Task<PunchClock> UpdatePunchClock(int id, PunchClock punchClock)
        {
            var existingPunchClock = await _context.PunchClocks.FindAsync(id);
            if (existingPunchClock == null) return null;

            existingPunchClock.UserId = punchClock.UserId;
            existingPunchClock.Timestamp = punchClock.Timestamp;
            existingPunchClock.PunchClockType = punchClock.PunchClockType;

            _context.PunchClocks.Update(existingPunchClock);
            await _context.SaveChangesAsync();

            return existingPunchClock;
        }

        public async Task<bool> DeletePunchClock(int id)
        {
            var punchClock = await _context.PunchClocks.FindAsync(id);
            if (punchClock == null) return false;

            _context.PunchClocks.Remove(punchClock);
            await _context.SaveChangesAsync();

            return true;
        }

        public bool HasCheckedInToday(int userId)
        {
            var today = DateTime.Today;
            return _context.PunchClocks.Any(p => p.UserId == userId && p.Timestamp.Date == today && p.PunchClockType == PunchClockType.CheckIn);
        }

        public bool HasCheckedOutToday(int userId)
        {
            var today = DateTime.Today;
            return _context.PunchClocks.Any(p => p.UserId == userId && p.Timestamp.Date == today && p.PunchClockType == PunchClockType.CheckOut);
        }
    }
}

