using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClyvoCare.API.Models
{
    [Table("TB_CC_USUARIO")]
    public class Usuario
    {
        [Key]
        [Column("ID_USUARIO")]
        public int Id { get; set; }

        [Required]
        [Column("NOME_USUARIO")]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [Column("EMAIL")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Column("SENHA")]
        [StringLength(100)]
        public string Senha { get; set; } = string.Empty;
    }
}
