using ClyvoCare.API.Controllers;
using ClyvoCare.API.Data;
using ClyvoCare.API.DTOs;
using ClyvoCare.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClyvoCare.UnitTests.Controllers;

public class LogsSaudeControllerTests
{
    private readonly Mock<ILogger<LogsSaudeController>> _loggerMock = new();

    private AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static async Task SeedPetAsync(AppDbContext context)
    {
        context.Usuarios.Add(new Usuario { Id = 1, Nome = "Ana Tutor", Email = "ana@teste.com", Senha = "senha123" });
        context.Pets.Add(new Pet { Id = 1, Nome = "Thor", Especie = "Cachorro", DataNascimento = DateTime.Now.AddYears(-3), UsuarioId = 1 });
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task PostLogSaude_DadosValidos_RegistraMetricaERetornaCreated()
    {
        using var context = CreateDbContext();
        await SeedPetAsync(context);
        var controller = new LogsSaudeController(context, _loggerMock.Object);
        var dto = new LogSaudeCreateDTO
        {
            PetId = 1,
            Peso = 14.2m,
            Temperatura = 38.6m,
            BatimentosCardiacos = 105,
            Observacoes = "Coleta IoT automatica"
        };

        var result = await controller.PostLogSaude(dto);

        var createdResult = Assert.IsType<CreatedAtRouteResult>(result.Result);
        var log = Assert.IsType<LogSaude>(createdResult.Value);
        Assert.Equal(dto.PetId, log.PetId);
        Assert.Equal(dto.BatimentosCardiacos, log.BatimentosCardiacos);
    }

    [Fact]
    public async Task PutLogSaude_RegistroExistente_AtualizaComSucesso()
    {
        using var context = CreateDbContext();
        await SeedPetAsync(context);
        context.LogsSaude.Add(new LogSaude
        {
            Id = 1,
            PetId = 1,
            Peso = 12.0m,
            Temperatura = 38.0m,
            BatimentosCardiacos = 90,
            Observacoes = "Leitura inicial",
            DataHora = DateTime.Now
        });
        await context.SaveChangesAsync();

        var controller = new LogsSaudeController(context, _loggerMock.Object);
        var dto = new LogSaudeUpdateDTO
        {
            PetId = 1,
            Peso = 12.8m,
            Temperatura = 39.1m,
            BatimentosCardiacos = 130,
            Observacoes = "Febre detectada pelo sensor"
        };

        var result = await controller.PutLogSaude(1, dto);

        Assert.IsType<NoContentResult>(result);
        var atualizado = await context.LogsSaude.FindAsync(1);
        Assert.Equal(39.1m, atualizado!.Temperatura);
        Assert.Equal("Febre detectada pelo sensor", atualizado.Observacoes);
    }

    [Fact]
    public async Task DeleteLogSaude_RegistroExistente_RemoveComSucesso()
    {
        using var context = CreateDbContext();
        await SeedPetAsync(context);
        context.LogsSaude.Add(new LogSaude
        {
            Id = 1,
            PetId = 1,
            Peso = 10.0m,
            Temperatura = 38.2m,
            BatimentosCardiacos = 100,
            Observacoes = "Para exclusao",
            DataHora = DateTime.Now
        });
        await context.SaveChangesAsync();

        var controller = new LogsSaudeController(context, _loggerMock.Object);

        var result = await controller.DeleteLogSaude(1);

        Assert.IsType<NoContentResult>(result);
        Assert.Empty(context.LogsSaude);
    }
}
