using ClyvoCare.API.Data;
using ClyvoCare.API.DTOs;
using ClyvoCare.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClyvoCare.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PetsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<PetsController> _logger;

    public PetsController(AppDbContext context, ILogger<PetsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Pet>>> GetPet()
    {
        _logger.LogInformation("Consultando listagem de pets.");
        return await _context.Pets.ToListAsync();
    }

    [HttpGet("{id}", Name = "GetPetById")]
    public async Task<ActionResult<Pet>> GetPet(int id)
    {
        var pet = await _context.Pets.FindAsync(id);
        if (pet == null)
        {
            _logger.LogWarning("Pet com Id {PetId} nao encontrado.", id);
            return NotFound();
        }

        return pet;
    }

    [HttpGet("especie/{especie}")]
    public async Task<ActionResult<IEnumerable<Pet>>> GetPorEspecie(string especie)
    {
        _logger.LogInformation("Buscando pets pela especie: {Especie}", especie);
        return await _context.Pets
            .Where(p => p.Especie.ToLower() == especie.ToLower())
            .ToListAsync();
    }

    [HttpGet("tutor/{usuarioId}")]
    public async Task<ActionResult<IEnumerable<Pet>>> GetPorTutor(int usuarioId)
    {
        _logger.LogInformation("Buscando pets associados ao tutor {UsuarioId}", usuarioId);
        return await _context.Pets
            .Where(p => p.UsuarioId == usuarioId)
            .ToListAsync();
    }

    [HttpGet("busca/{nome}")]
    public async Task<ActionResult<IEnumerable<Pet>>> GetPorNome(string nome)
    {
        _logger.LogInformation("Buscando pets por nome: {Nome}", nome);
        return await _context.Pets
            .Where(p => p.Nome.Contains(nome))
            .ToListAsync();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutPet(int id, Pet pet)
    {
        if (id != pet.Id)
        {
            _logger.LogWarning("Falha ao atualizar pet: Id da rota {RotaId} diverge do corpo {BodyId}.", id, pet.Id);
            return BadRequest();
        }

        var tutorExiste = await _context.Usuarios.AnyAsync(u => u.Id == pet.UsuarioId);
        if (!tutorExiste)
        {
            _logger.LogWarning("Falha ao atualizar pet: tutor {UsuarioId} nao encontrado.", pet.UsuarioId);
            return NotFound($"Tutor {pet.UsuarioId} nao encontrado.");
        }

        _context.Entry(pet).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Pet {PetId} atualizado com sucesso.", id);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Pets.Any(e => e.Id == id)) return NotFound();
            throw;
        }

        return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult<Pet>> PostPet(PetCreateDTO petDto)
    {
        var tutorExiste = await _context.Usuarios.AnyAsync(u => u.Id == petDto.UsuarioId);
        if (!tutorExiste)
        {
            _logger.LogWarning("Falha ao cadastrar pet: tutor {UsuarioId} nao encontrado.", petDto.UsuarioId);
            return NotFound($"Tutor {petDto.UsuarioId} nao encontrado.");
        }

        var pet = new Pet
        {
            Nome = petDto.Nome,
            Especie = petDto.Especie,
            DataNascimento = petDto.DataNascimento,
            UsuarioId = petDto.UsuarioId
        };

        _context.Pets.Add(pet);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Pet cadastrado com sucesso. Id: {PetId}, Tutor: {UsuarioId}", pet.Id, pet.UsuarioId);

        return CreatedAtRoute("GetPetById", new { id = pet.Id }, pet);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePet(int id)
    {
        var pet = await _context.Pets.FindAsync(id);
        if (pet == null)
        {
            _logger.LogWarning("Tentativa de exclusao falhou. Pet {PetId} nao encontrado.", id);
            return NotFound();
        }

        _context.Pets.Remove(pet);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Pet {PetId} removido com sucesso.", id);
        return NoContent();
    }
}
