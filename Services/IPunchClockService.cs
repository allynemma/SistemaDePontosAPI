using SistemaDePontosAPI.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SistemaDePontosAPI.Services
{
    public interface IPunchClockService
    {
        Task<PunchClock> RegisterPunchClock(int userId, PunchClockType punchClockType);
        Task<PunchClock> GetPunchClockById(int id);
        Task<IEnumerable<PunchClock>> GetPunchClocksByUserId(int userId, DateTime? dataInicio, DateTime? dataFim);
        Task<IEnumerable<PunchClock>> GetAllPunchClocks(int? userId, DateTime? dataInicio, DateTime? dataFim);
        Task<IEnumerable<PunchClock>> GetPunchClocksForReport(DateTime dataInicio, DateTime dataFim);
        Task<PunchClock> UpdatePunchClock(int id, PunchClock punchClock);
        Task<bool> DeletePunchClock(int id);
        bool HasCheckedInToday(int userId);
        bool HasCheckedOutToday(int userId);
    }
}

