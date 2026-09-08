using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClyvoCare.API.Models
{
    [Table("TB_CC_PET")]
    public class Pet
    {
        [Key]
        [Column("ID_PET")]
        public int Id { get; set; }

        [Required]
        [Column("NOME_PET")]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [Column("ESPECIE")]
        [StringLength(50)]
        public string Especie { get; set; } = string.Empty;

        [Required]
        [Column("DT_NASCIMENTO")]
        public DateTime DataNascimento { get; set; }

        [Required]
        [Column("ID_USUARIO")]
        public int UsuarioId { get; set; }
    }
}