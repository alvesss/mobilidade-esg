using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MobilidadeESG.Models;

public class VeiculoEletrico
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Modelo { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Fabricante { get; set; } = string.Empty;

    [Required]
    public int AnoFabricacao { get; set; }

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal AutonomiaKm { get; set; }

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal ConsumoKWhPorKm { get; set; }

    [Required, MaxLength(50)]
    public string TipoVeiculo { get; set; } = string.Empty; // Carro, Moto, Bicicleta, Ônibus, Patinete

    [Required, MaxLength(20)]
    public string Placa { get; set; } = string.Empty;

    [Required]
    public string ProprietarioNome { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    public bool Ativo { get; set; } = true;

    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalKmRodados { get; set; } = 0;

    // Relações
    public ICollection<RegistroUso> RegistrosUso { get; set; } = new List<RegistroUso>();
    public Recompensa? Recompensa { get; set; }
}
