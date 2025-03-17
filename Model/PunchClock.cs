using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaDePontosAPI.Model;

public class PunchClock
{
    [Key]
    public int Id { get; set; }

    [ForeignKey(nameof(Users))]
    public int UserId { get; set; }
    public PunchClockType PunchClockType{ get; set; }
    public DateTime Timestamp { get; set; }

}

public enum PunchClockType
{
    CheckIn,
    CheckOut
}