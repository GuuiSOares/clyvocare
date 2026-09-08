using System.ComponentModel.DataAnnotations;

namespace ClyvoCare.API.DTOs
{
    public class LogSaudeCreateDTO
    {
        [Range(0.1, 200, ErrorMessage = "Peso inválido")]
        public decimal Peso { get; set; }

        [Range(30, 45, ErrorMessage = "Temperatura fora da faixa normal")]
        public decimal Temperatura { get; set; }

        public int BatimentosCardiacos { get; set; }

        public string Observacoes { get; set; } = string.Empty;

        [Required]
        public int PetId { get; set; }
    }
}