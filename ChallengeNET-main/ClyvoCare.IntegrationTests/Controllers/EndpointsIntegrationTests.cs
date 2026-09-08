using System.Net;
using System.Net.Http.Json;
using ClyvoCare.API.DTOs;
using ClyvoCare.API.Models;
using ClyvoCare.IntegrationTests.Fixtures;
using Xunit;

namespace ClyvoCare.IntegrationTests.Controllers;

public class EndpointsIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public EndpointsIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostUsuario_PayloadValido_RetornaCreated()
    {
        var usuarioDto = new UsuarioCreateDTO
        {
            Nome = "Tutor Integracao",
            Email = $"tutor_{Guid.NewGuid()}@teste.com",
            Senha = "senhaForte123"
        };

        var response = await _client.PostAsJsonAsync("/api/Usuarios", usuarioDto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CrudPet_FluxoCompleto_PersisteNoBanco()
    {
        var petDto = new PetCreateDTO
        {
            Nome = "Bidu",
            Especie = "Cachorro",
            DataNascimento = new DateTime(2023, 1, 10),
            UsuarioId = 1
        };

        var createResponse = await _client.PostAsJsonAsync("/api/Pets", petDto);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var criado = await createResponse.Content.ReadFromJsonAsync<Pet>();
        Assert.NotNull(criado);

        var getResponse = await _client.GetAsync($"/api/Pets/{criado!.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        criado.Nome = "Bidu Atualizado";
        var putResponse = await _client.PutAsJsonAsync($"/api/Pets/{criado.Id}", criado);
        Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);

        var deleteResponse = await _client.DeleteAsync($"/api/Pets/{criado.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task PostLogSaude_PayloadValido_RetornaCreated()
    {
        var logDto = new LogSaudeCreateDTO
        {
            PetId = 1,
            Peso = 10.5m,
            Temperatura = 38.8m,
            BatimentosCardiacos = 115,
            Observacoes = "Teste de integracao IoT"
        };

        var response = await _client.PostAsJsonAsync("/api/LogsSaude", logDto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
