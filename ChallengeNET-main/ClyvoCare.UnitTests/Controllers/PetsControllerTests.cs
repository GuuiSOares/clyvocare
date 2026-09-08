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

public class PetsControllerTests
{
    private readonly Mock<ILogger<PetsController>> _loggerMock = new();

    private AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task PostPet_DadosValidos_SalvaComSucessoERetornaCreated()
    {
        using var context = CreateDbContext();
        context.Usuarios.Add(new Usuario { Id = 1, Nome = "Carlos Tutor", Email = "carlos@teste.com", Senha = "senha123" });
        await context.SaveChangesAsync();

        var controller = new PetsController(context, _loggerMock.Object);
        var dto = new PetCreateDTO
        {
            Nome = "Rex",
            Especie = "Cachorro",
            DataNascimento = DateTime.Now.AddYears(-2),
            UsuarioId = 1
        };

        var result = await controller.PostPet(dto);

        var createdResult = Assert.IsType<CreatedAtRouteResult>(result.Result);
        var petSalvo = Assert.IsType<Pet>(createdResult.Value);
        Assert.Equal("Rex", petSalvo.Nome);
        Assert.Equal(1, petSalvo.UsuarioId);
    }

    [Fact]
    public async Task GetPorEspecie_FiltroValido_RetornaPetsDaEspecie()
    {
        using var context = CreateDbContext();
        context.Usuarios.Add(new Usuario { Id = 1, Nome = "Carlos Tutor", Email = "carlos@teste.com", Senha = "senha123" });
        context.Pets.AddRange(
            new Pet { Id = 1, Nome = "Mingau", Especie = "Gato", UsuarioId = 1 },
            new Pet { Id = 2, Nome = "Bob", Especie = "Cachorro", UsuarioId = 1 }
        );
        await context.SaveChangesAsync();

        var controller = new PetsController(context, _loggerMock.Object);

        var result = await controller.GetPorEspecie("Gato");

        var pets = Assert.IsAssignableFrom<IEnumerable<Pet>>(result.Value).ToList();
        Assert.Single(pets);
        Assert.Equal("Mingau", pets[0].Nome);
    }
}
