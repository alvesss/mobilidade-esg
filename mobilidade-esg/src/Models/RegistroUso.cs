using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MobilidadeESG.Models;

public class RegistroUso
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int VeiculoEletricoId { get; set; }

    public VeiculoEletrico? VeiculoEletrico { get; set; }

    [Required]
    public int RotaTransporteId { get; set; }

    public RotaTransporte? RotaTransporte { get; set; }

    [Required]
    public DateTime DataHoraInicio { get; set; }

    public DateTime? DataHoraFim { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal DistanciaPercorridaKm { get; set; }

    [Column(TypeName = "decimal(10,4)")]
    public decimal Co2EvitadoKg { get; set; }

    [Column(TypeName = "decimal(10,4)")]
    public decimal EnergiaConsumidaKWh { get; set; }

    public int PontosGanhos { get; set; }

    [MaxLength(500)]
    public string? Observacoes { get; set; }

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
