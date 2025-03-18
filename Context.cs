using Microsoft.EntityFrameworkCore;

namespace SistemaDePontosAPI;

public class Context : DbContext
{
    public Context ()  { } //cria a instancia do dbcontext

    public Context (DbContextOptions<Context> options) : base (options) { }
    //

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Data Source=DESKTOP-HDNU4UN;Initial Catalog=Estudos;Integrated Security=True;Encrypt=True;Trust Server Certificate=True");
        }
    }

    public virtual DbSet<Model.Users> Users { get; set; }
    public virtual DbSet<Model.PunchClock> PunchClocks { get; set; }
    public virtual DbSet<Model.Settings> Settings { get; set; }


}
