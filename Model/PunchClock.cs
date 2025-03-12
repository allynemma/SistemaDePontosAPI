using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaDePontosAPI.Model;

public class PunchClock
{
    public int Id { get; set; }
    public int UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    public required Users Users { get; set; }
    public DateTime Timestamp { get; set; }

}
