using ClyvoCare.API.Data;
using ClyvoCare.API.DTOs;
using ClyvoCare.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClyvoCare.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsuariosController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<UsuariosController> _logger;

    public UsuariosController(AppDbContext context, ILogger<UsuariosController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuario()
    {
        _logger.LogInformation("Consultando listagem geral de usuarios.");
        return await _context.Usuarios.ToListAsync();
    }

    [HttpGet("{id}", Name = "GetUsuarioById")]
    public async Task<ActionResult<Usuario>> GetUsuario(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null)
        {
            _logger.LogWarning("Usuario com Id {UsuarioId} nao foi encontrado.", id);
            return NotFound();
        }

        return usuario;
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutUsuario(int id, Usuario usuario)
    {
        if (id != usuario.Id)
        {
            _logger.LogWarning("Falha na atualizacao: Id da rota {RotaId} diverge do corpo {BodyId}.", id, usuario.Id);
            return BadRequest();
        }

        _context.Entry(usuario).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Usuario {UsuarioId} atualizado com sucesso.", id);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Usuarios.Any(e => e.Id == id)) return NotFound();
            throw;
        }

        return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult<Usuario>> PostUsuario(UsuarioCreateDTO usuarioDto)
    {
        var usuario = new Usuario
        {
            Nome = usuarioDto.Nome,
            Email = usuarioDto.Email,
            Senha = usuarioDto.Senha
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Usuario cadastrado com sucesso. Id: {UsuarioId}, Email: {Email}", usuario.Id, usuario.Email);

        return CreatedAtRoute("GetUsuarioById", new { id = usuario.Id }, usuario);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUsuario(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null)
        {
            _logger.LogWarning("Tentativa de exclusao falhou. Usuario {UsuarioId} nao encontrado.", id);
            return NotFound();
        }

        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Usuario {UsuarioId} removido com sucesso.", id);
        return NoContent();
    }
}