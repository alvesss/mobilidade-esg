using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MobilidadeESG.Models;

public class RotaTransporte
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Origem { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Destino { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal DistanciaKm { get; set; }

    [Required, MaxLength(50)]
    public string TipoTransporte { get; set; } = string.Empty; // Ciclovias, Metrô, BRT, VLT, Bicicleta Pública

    [Required, MaxLength(100)]
    public string Cidade { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Estado { get; set; } = string.Empty;

    public bool Ativa { get; set; } = true;

    [Column(TypeName = "decimal(10,2)")]
    public decimal Co2EvitadoPorKmKg { get; set; } = 0.21m; // 210g/km média carro a gasolina

    public int PontosPorKm { get; set; } = 10;

    public DateTime CriadaEm { get; set; } = DateTime.UtcNow;

    // Relações
    public ICollection<RegistroUso> RegistrosUso { get; set; } = new List<RegistroUso>();
}
