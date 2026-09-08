using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClyvoCare.API.Models
{
    [Table("TB_CC_LOG_SAUDE")]
    public class LogSaude
    {
        [Key]
        [Column("ID_LOG")]
        public int Id { get; set; }

        [Column("PESO", TypeName = "NUMBER(10,2)")]
        public decimal Peso { get; set; }

        [Column("TEMPERATURA", TypeName = "NUMBER(10,2)")]
        public decimal Temperatura { get; set; }

        [Column("BATIMENTOS_CARDIACOS")]
        public int BatimentosCardiacos { get; set; }

        [Column("DATA_HORA")]
        public DateTime DataHora { get; set; }

        [Column("OBSERVACOES")]
        [StringLength(500)]
        public string Observacoes { get; set; } = string.Empty;

        [Column("ID_PET")]
        public int PetId { get; set; }
    }
}
