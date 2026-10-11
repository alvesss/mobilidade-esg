using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MobilidadeESG.Models;

public class RelatorioImpacto
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Cidade { get; set; } = string.Empty;

    [Required]
    public int Mes { get; set; }

    [Required]
    public int Ano { get; set; }

    [Column(TypeName = "decimal(14,4)")]
    public decimal TotalCo2EvitadoKg { get; set; }

    [Column(TypeName = "decimal(14,2)")]
    public decimal TotalKmSustentaveis { get; set; }

    public int TotalViagens { get; set; }

    public int TotalVeiculosAtivos { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal EconomiaEnergiaKWh { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal EconomiaFinanceiraEstimadaReais { get; set; }

    public DateTime GeradoEm { get; set; } = DateTime.UtcNow;
}
