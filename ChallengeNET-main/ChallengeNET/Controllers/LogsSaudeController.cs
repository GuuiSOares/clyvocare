using ClyvoCare.API.Data;
using ClyvoCare.API.Diagnostics;
using ClyvoCare.API.DTOs;
using ClyvoCare.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClyvoCare.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LogsSaudeController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<LogsSaudeController> _logger;

    public LogsSaudeController(AppDbContext context, ILogger<LogsSaudeController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LogSaude>>> GetLogSaude()
    {
        _logger.LogInformation("Consultando listagem de logs de saude.");
        return await _context.LogsSaude.OrderByDescending(l => l.DataHora).ToListAsync();
    }

    [HttpGet("{id}", Name = "GetLogSaudeById")]
    public async Task<ActionResult<LogSaude>> GetLogSaude(int id)
    {
        var log = await _context.LogsSaude.FindAsync(id);
        if (log == null)
        {
            _logger.LogWarning("Log de saude com Id {LogId} nao encontrado.", id);
            return NotFound();
        }

        return log;
    }

    [HttpGet("pet/{petId}", Name = "GetLogsPorPet")]
    public async Task<ActionResult<IEnumerable<LogSaude>>> GetLogsPorPet(int petId)
    {
        _logger.LogInformation("Consultando historico de saude para o PetId {PetId}", petId);
        return await _context.LogsSaude
            .Where(l => l.PetId == petId)
            .OrderByDescending(l => l.DataHora)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<LogSaude>> PostLogSaude([FromBody] LogSaudeCreateDTO dto)
    {
        using var activity = TelemetryConfig.ActivitySource.StartActivity("ProcessarRegistroLogSaude");

        var petExiste = await _context.Pets.AnyAsync(p => p.Id == dto.PetId);
        if (!petExiste)
        {
            _logger.LogWarning("Falha ao registrar log: Pet {PetId} nao encontrado.", dto.PetId);
            return NotFound($"Pet {dto.PetId} nao encontrado.");
        }

        var log = new LogSaude
        {
            Peso = dto.Peso,
            Temperatura = dto.Temperatura,
            BatimentosCardiacos = dto.BatimentosCardiacos,
            Observacoes = dto.Observacoes,
            PetId = dto.PetId,
            DataHora = DateTime.Now
        };

        _context.LogsSaude.Add(log);
        await _context.SaveChangesAsync();

        TelemetryConfig.LogsSaudeProcessadosCounter.Add(1);
        _logger.LogInformation("Log de saude registrado com sucesso. Id: {LogId}, PetId: {PetId}", log.Id, log.PetId);

        return CreatedAtRoute("GetLogSaudeById", new { id = log.Id }, log);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutLogSaude(int id, [FromBody] LogSaudeUpdateDTO dto)
    {
        var log = await _context.LogsSaude.FindAsync(id);
        if (log == null)
        {
            _logger.LogWarning("Falha ao atualizar log: Id {LogId} nao encontrado.", id);
            return NotFound();
        }

        var petExiste = await _context.Pets.AnyAsync(p => p.Id == dto.PetId);
        if (!petExiste)
        {
            _logger.LogWarning("Falha ao atualizar log: Pet {PetId} nao encontrado.", dto.PetId);
            return NotFound($"Pet {dto.PetId} nao encontrado.");
        }

        log.Peso = dto.Peso;
        log.Temperatura = dto.Temperatura;
        log.BatimentosCardiacos = dto.BatimentosCardiacos;
        log.Observacoes = dto.Observacoes;
        log.PetId = dto.PetId;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Log de saude {LogId} atualizado com sucesso.", id);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLogSaude(int id)
    {
        var log = await _context.LogsSaude.FindAsync(id);
        if (log == null)
        {
            _logger.LogWarning("Tentativa de exclusao falhou. Log {LogId} nao encontrado.", id);
            return NotFound();
        }

        _context.LogsSaude.Remove(log);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Log de saude {LogId} removido com sucesso.", id);
        return NoContent();
    }
}
