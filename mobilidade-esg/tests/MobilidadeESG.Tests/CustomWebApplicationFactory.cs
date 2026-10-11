using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using MobilidadeESG.Data;
using MobilidadeESG.Models;

namespace MobilidadeESG.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string AdminEmail = "admin@teste.com";
    public const string AdminSenha = "Admin@123";

    // Nome e "root" fixos por instância: todas as requisições do mesmo fixture
    // enxergam o MESMO banco em memória (antes, cada escopo gerava um Guid novo
    // e o login nunca encontrava o usuário de teste).
    private readonly string _dbName = "MobilidadeESGTest_" + Guid.NewGuid();
    private readonly InMemoryDatabaseRoot _dbRoot = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            // Remove o DbContext de SQL Server registrado no Program.cs
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_dbName, _dbRoot));

            using var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated(); // aplica também o HasData do AppDbContext
            SeedTestData(db);
        });
    }

    private static void SeedTestData(AppDbContext db)
    {
        // O HasData já cria rotas (Id 1..3) e o admin de produção (Id 1).
        // Aqui só garantimos o usuário de teste, com Id explícito para não colidir.
        if (!db.Usuarios.Any(u => u.Email == AdminEmail))
        {
            db.Usuarios.Add(new Usuario
            {
                Id = 100,
                Nome = "Admin Teste",
                Email = AdminEmail,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(AdminSenha),
                Perfil = "Admin",
                Ativo = true,
                CriadoEm = DateTime.UtcNow
            });
        }

        db.SaveChanges();
    }
}
