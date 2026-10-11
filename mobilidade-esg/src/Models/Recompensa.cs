using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MobilidadeESG.Models;

public class Recompensa
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int VeiculoEletricoId { get; set; }

    public VeiculoEletrico? VeiculoEletrico { get; set; }

    [Required]
    public int PontosAcumulados { get; set; } = 0;

    [Required]
    public int PontosResgatados { get; set; } = 0;

    public int PontosSaldo => PontosAcumulados - PontosResgatados;

    [Required, MaxLength(50)]
    public string NivelRecompensa { get; set; } = "Bronze"; // Bronze, Prata, Ouro, Platina

    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalCo2EvitadoKg { get; set; } = 0;

    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalKmSustentaveis { get; set; } = 0;

    public DateTime UltimaAtualizacao { get; set; } = DateTime.UtcNow;

    public DateTime CriadaEm { get; set; } = DateTime.UtcNow;

    public ICollection<ResgateBeneficio> Resgates { get; set; } = new List<ResgateBeneficio>();
}

public class ResgateBeneficio
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int RecompensaId { get; set; }

    public Recompensa? Recompensa { get; set; }

    [Required, MaxLength(200)]
    public string DescricaoBeneficio { get; set; } = string.Empty;

    [Required]
    public int PontosUtilizados { get; set; }

    [Required, MaxLength(50)]
    public string TipoBeneficio { get; set; } = string.Empty; // Desconto, Crédito, Produto

    public DateTime DataResgate { get; set; } = DateTime.UtcNow;
}
