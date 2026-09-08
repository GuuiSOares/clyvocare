using System.ComponentModel.DataAnnotations;

namespace ClyvoCare.API.DTOs
{
    public class PetCreateDTO
    {
        [Required(ErrorMessage = "O nome do pet é obrigatório.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "A espécie deve ser informada.")]
        public string Especie { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime DataNascimento { get; set; }

        public int UsuarioId { get; set; }
    }
}