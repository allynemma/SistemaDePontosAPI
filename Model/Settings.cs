using System.Text.Json.Serialization;

namespace SistemaDePontosAPI.Model;

public class Settings
{
    [JsonIgnore]
    public int Id { get; set; }
    public float Workday_Hours { get; set; }
    public float Overtime_Rate { get; set; }
}
