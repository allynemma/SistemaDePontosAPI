using SistemaDePontosAPI.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SistemaDePontosAPI.Services
{
    public interface ISettingsService
    {
        Task<Settings> CreateSettings(Settings settings);
        Task<Settings> GetSettingsById(int id);
        Task<IEnumerable<Settings>> GetAllSettings();
        Task<Settings> UpdateSettings(int id, Settings settings);
        Task<bool> DeleteSettings(int id);
    }
}
