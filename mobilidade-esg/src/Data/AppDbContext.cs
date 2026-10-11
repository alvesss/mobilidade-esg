using Microsoft.EntityFrameworkCore;
using MobilidadeESG.Models;

namespace MobilidadeESG.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<VeiculoEletrico> VeiculosEletricos { get; set; }
    public DbSet<RotaTransporte> RotasTransporte { get; set; }
    public DbSet<RegistroUso> RegistrosUso { get; set; }
    public DbSet<Recompensa> Recompensas { get; set; }
    public DbSet<ResgateBeneficio> ResgatesBeneficios { get; set; }
    public DbSet<RelatorioImpacto> RelatoriosImpacto { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // VeiculoEletrico
        modelBuilder.Entity<VeiculoEletrico>(e =>
        {
            e.HasIndex(v => v.Placa).IsUnique();
            e.HasIndex(v => v.Email);
            e.Property(v => v.AutonomiaKm).HasPrecision(10, 2);
            e.Property(v => v.ConsumoKWhPorKm).HasPrecision(10, 4);
            e.Property(v => v.TotalKmRodados).HasPrecision(12, 2);
        });

        // RotaTransporte
        modelBuilder.Entity<RotaTransporte>(e =>
        {
            e.HasIndex(r => new { r.Cidade, r.TipoTransporte });
            e.Property(r => r.DistanciaKm).HasPrecision(10, 2);
            e.Property(r => r.Co2EvitadoPorKmKg).HasPrecision(10, 4);
        });

        // RegistroUso
        modelBuilder.Entity<RegistroUso>(e =>
        {
            e.HasOne(r => r.VeiculoEletrico)
             .WithMany(v => v.RegistrosUso)
             .HasForeignKey(r => r.VeiculoEletricoId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(r => r.RotaTransporte)
             .WithMany(rt => rt.RegistrosUso)
             .HasForeignKey(r => r.RotaTransporteId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(r => r.DataHoraInicio);
            e.HasIndex(r => r.VeiculoEletricoId);
            e.Property(r => r.DistanciaPercorridaKm).HasPrecision(10, 2);
            e.Property(r => r.Co2EvitadoKg).HasPrecision(10, 4);
            e.Property(r => r.EnergiaConsumidaKWh).HasPrecision(10, 4);
        });

        // Recompensa
        modelBuilder.Entity<Recompensa>(e =>
        {
            e.HasOne(r => r.VeiculoEletrico)
             .WithOne(v => v.Recompensa)
             .HasForeignKey<Recompensa>(r => r.VeiculoEletricoId)
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(r => r.TotalCo2EvitadoKg).HasPrecision(14, 4);
            e.Property(r => r.TotalKmSustentaveis).HasPrecision(14, 2);
        });

        // ResgateBeneficio
        modelBuilder.Entity<ResgateBeneficio>(e =>
        {
            e.HasOne(r => r.Recompensa)
             .WithMany(rc => rc.Resgates)
             .HasForeignKey(r => r.RecompensaId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // RelatorioImpacto
        modelBuilder.Entity<RelatorioImpacto>(e =>
        {
            e.HasIndex(r => new { r.Cidade, r.Mes, r.Ano }).IsUnique();
            e.Property(r => r.TotalCo2EvitadoKg).HasPrecision(14, 4);
            e.Property(r => r.TotalKmSustentaveis).HasPrecision(14, 2);
            e.Property(r => r.EconomiaFinanceiraEstimadaReais).HasPrecision(14, 2);
        });

        // Usuario
        modelBuilder.Entity<Usuario>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
        });

        // Seed data inicial
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RotaTransporte>().HasData(
            new RotaTransporte { Id = 1, Nome = "Ciclovia Paulista", Origem = "Consolação", Destino = "Brigadeiro", DistanciaKm = 4.5m, TipoTransporte = "Ciclovia", Cidade = "São Paulo", Estado = "SP", Ativa = true, Co2EvitadoPorKmKg = 0.21m, PontosPorKm = 15, CriadaEm = new DateTime(2024, 1, 1) },
            new RotaTransporte { Id = 2, Nome = "BRT Expresso Tiradentes", Origem = "Terminal Tiradentes", Destino = "Santana", DistanciaKm = 12.3m, TipoTransporte = "BRT", Cidade = "São Paulo", Estado = "SP", Ativa = true, Co2EvitadoPorKmKg = 0.18m, PontosPorKm = 10, CriadaEm = new DateTime(2024, 1, 1) },
            new RotaTransporte { Id = 3, Nome = "VLT Carioca Centro", Origem = "Santos Dumont", Destino = "Praça XV", DistanciaKm = 6.7m, TipoTransporte = "VLT", Cidade = "Rio de Janeiro", Estado = "RJ", Ativa = true, Co2EvitadoPorKmKg = 0.19m, PontosPorKm = 12, CriadaEm = new DateTime(2024, 1, 1) }
        );

        modelBuilder.Entity<Usuario>().HasData(
            new Usuario { Id = 1, Nome = "Admin ESG", Email = "admin@mobilidade-esg.com", SenhaHash = "$2a$11$Ci80yEh7CyB5clSphK7tAOfE4IfT6Mf5Rd1Cztwx22XTWZG7uqFJa" /* hash fixo de "Admin@123": evita que o seed mude a cada build e quebre as migrations */, Perfil = "Admin", Ativo = true, CriadoEm = new DateTime(2024, 1, 1) }
        );
    }
}
