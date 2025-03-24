using Microsoft.EntityFrameworkCore;
using SistemaDePontosAPI.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SistemaDePontosAPI.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly Context _context;

        public SettingsService(Context context)
        {
            _context = context;
        }

        public async Task<Settings> CreateSettings(Settings settings)
        {
            settings.Id = 0;
            _context.Settings.Add(settings);
            await _context.SaveChangesAsync();
            return settings;
        }

        public async Task<Settings> GetSettingsById(int id)
        {
            return await _context.Settings.FindAsync(id);
        }

        public async Task<IEnumerable<Settings>> GetAllSettings()
        {
            return await _context.Settings.ToListAsync();
        }

        public async Task<Settings> UpdateSettings(int id, Settings settings)
        {
            var existingSettings = await _context.Settings.FindAsync(id);
            if (existingSettings == null) return null;

            existingSettings.Overtime_Rate = settings.Overtime_Rate;
            existingSettings.Workday_Hours = settings.Workday_Hours;

            _context.Settings.Update(existingSettings);
            await _context.SaveChangesAsync();

            return existingSettings;
        }

        public async Task<bool> DeleteSettings(int id)
        {
            var settings = await _context.Settings.FindAsync(id);
            if (settings == null) return false;

            _context.Settings.Remove(settings);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
