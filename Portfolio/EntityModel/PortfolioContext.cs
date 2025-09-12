using Microsoft.EntityFrameworkCore;

namespace Portfolio.EntityModel;

public partial class PortfolioContext : DbContext
{
    public PortfolioContext()
    {
    }

    public PortfolioContext(DbContextOptions<PortfolioContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Instrument> Instrument { get; set; }
    public virtual DbSet<InstrumentMonthlyDelta> InstrumentMonthlyDelta { get; set; }
    public virtual DbSet<Correlation> Correlation{ get; set; }
    public virtual DbSet<PriceDate> DateRange { get; set; }
    public virtual DbSet<Portfolio> Portfolio { get; set; }
    public virtual DbSet<PortfolioHolding> PortfolioHolding { get; set; }
    //public virtual DbSet<ProxyInstrument> ProxyInstrument { get; set; }
    public virtual DbSet<AdjustedReturn> AdjustedReturn { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Use connection string from configuration (appsettings.json or environment)
            optionsBuilder.UseSqlServer("Server=localhost;Database=Portfolio;Trusted_Connection=True;TrustServerCertificate=true;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InstrumentMonthlyDelta>().ToView("InstrumentMonthlyDelta");

        //modelBuilder.Entity<ProxyInstrument>()
        //        .HasOne(pi => pi.Proxy)
        //        .WithMany(i => i.Proxies)
        //        .HasForeignKey(pi => pi.InstrumentId)
        //        .OnDelete(DeleteBehavior.NoAction);
    }
}
