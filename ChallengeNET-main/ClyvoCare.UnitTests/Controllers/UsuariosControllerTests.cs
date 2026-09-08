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

public class UsuariosControllerTests
{
	private readonly Mock<ILogger<UsuariosController>> _loggerMock = new();

	private AppDbContext CreateDbContext()
	{
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
			.Options;
		return new AppDbContext(options);
	}

	[Fact]
	public async Task PostUsuario_DadosValidos_RetornaCreatedAtAction()
	{
		// Arrange
		using var context = CreateDbContext();
		var controller = new UsuariosController(context, _loggerMock.Object);
		var dto = new UsuarioCreateDTO
		{
			Nome = "Carlos Alberto",
			Email = "carlos@teste.com",
			Senha = "senhaSegura123"
		};

		// Act
		var result = await controller.PostUsuario(dto);

		// Assert
		var createdResult = Assert.IsType<CreatedAtRouteResult>(result.Result);
		var usuarioCriado = Assert.IsType<Usuario>(createdResult.Value);
		Assert.Equal(dto.Email, usuarioCriado.Email);
		Assert.Equal(dto.Nome, usuarioCriado.Nome);
	}

	[Fact]
	public async Task GetUsuario_IdInexistente_RetornaNotFound()
	{
		// Arrange
		using var context = CreateDbContext();
		var controller = new UsuariosController(context, _loggerMock.Object);

		// Act
		var result = await controller.GetUsuario(999);

		// Assert
		Assert.IsType<NotFoundResult>(result.Result);
	}
}