using ClyvoCare.API.Data;
using ClyvoCare.API.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace ClyvoCare.IntegrationTests.Fixtures;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            services.RemoveAll(typeof(DbContextOptions));
            services.RemoveAll(typeof(AppDbContext));

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
            });
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();

        if (!db.Usuarios.Any())
        {
            db.Usuarios.AddRange(
                new Usuario { Id = 1, Nome = "Marina Oliveira", Email = "marina.tutor@email.com", Senha = "senha123" },
                new Usuario { Id = 2, Nome = "Rafael Costa", Email = "rafael.tutor@email.com", Senha = "senha123" }
            );
            db.Pets.AddRange(
                new Pet { Id = 1, Nome = "Thor", Especie = "Cachorro", DataNascimento = new DateTime(2022, 4, 15), UsuarioId = 1 },
                new Pet { Id = 2, Nome = "Luna", Especie = "Gato", DataNascimento = new DateTime(2021, 8, 3), UsuarioId = 2 }
            );
            db.LogsSaude.AddRange(
                new LogSaude { Id = 1, Peso = 28.40m, Temperatura = 38.50m, BatimentosCardiacos = 92, Observacoes = "Repouso noturno", PetId = 1, DataHora = DateTime.Now },
                new LogSaude { Id = 2, Peso = 4.10m, Temperatura = 38.90m, BatimentosCardiacos = 140, Observacoes = "Pos alimentacao", PetId = 2, DataHora = DateTime.Now }
            );
            db.SaveChanges();
        }

        return host;
    }
}
