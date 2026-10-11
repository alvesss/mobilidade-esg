using System.ComponentModel.DataAnnotations;

namespace MobilidadeESG.ViewModels;

// ────────── Paginação ──────────
public class PaginatedResult<T>
{
    public IEnumerable<T> Data { get; set; } = [];
    public int TotalItens { get; set; }
    public int TotalPaginas { get; set; }
    public int PaginaAtual { get; set; }
    public int ItensPorPagina { get; set; }
    public bool TemProxima => PaginaAtual < TotalPaginas;
    public bool TemAnterior => PaginaAtual > 1;
}

// ────────── Veículo Elétrico ──────────
public class VeiculoRequest
{
    [Required(ErrorMessage = "Modelo é obrigatório")]
    [MaxLength(100)]
    public string Modelo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Fabricante é obrigatório")]
    [MaxLength(50)]
    public string Fabricante { get; set; } = string.Empty;

    [Required]
    [Range(2000, 2030, ErrorMessage = "Ano inválido")]
    public int AnoFabricacao { get; set; }

    [Required]
    [Range(1, 1000, ErrorMessage = "Autonomia deve ser entre 1 e 1000 km")]
    public decimal AutonomiaKm { get; set; }

    [Required]
    [Range(0.01, 1, ErrorMessage = "Consumo deve ser entre 0.01 e 1 kWh/km")]
    public decimal ConsumoKWhPorKm { get; set; }

    [Required]
    public string TipoVeiculo { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Placa { get; set; } = string.Empty;

    [Required]
    public string ProprietarioNome { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class VeiculoResponse
{
    public int Id { get; set; }
    public string Modelo { get; set; } = string.Empty;
    public string Fabricante { get; set; } = string.Empty;
    public int AnoFabricacao { get; set; }
    public decimal AutonomiaKm { get; set; }
    public decimal ConsumoKWhPorKm { get; set; }
    public string TipoVeiculo { get; set; } = string.Empty;
    public string Placa { get; set; } = string.Empty;
    public string ProprietarioNome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public decimal TotalKmRodados { get; set; }
    public DateTime DataCadastro { get; set; }
    public decimal Co2TotalEvitadoKg { get; set; }
}

// ────────── Rota Transporte ──────────
public class RotaRequest
{
    [Required] public string Nome { get; set; } = string.Empty;
    [Required] public string Origem { get; set; } = string.Empty;
    [Required] public string Destino { get; set; } = string.Empty;

    [Required, Range(0.1, 5000)]
    public decimal DistanciaKm { get; set; }

    [Required] public string TipoTransporte { get; set; } = string.Empty;
    [Required] public string Cidade { get; set; } = string.Empty;
    [Required] public string Estado { get; set; } = string.Empty;

    [Range(1, 100)] public int PontosPorKm { get; set; } = 10;
}

public class RotaResponse
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Origem { get; set; } = string.Empty;
    public string Destino { get; set; } = string.Empty;
    public decimal DistanciaKm { get; set; }
    public string TipoTransporte { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public bool Ativa { get; set; }
    public decimal Co2EvitadoPorKmKg { get; set; }
    public int PontosPorKm { get; set; }
    public int TotalViagens { get; set; }
}

// ────────── Registro de Uso ──────────
public class RegistroUsoRequest
{
    [Required] public int VeiculoEletricoId { get; set; }
    [Required] public int RotaTransporteId { get; set; }
    [Required] public DateTime DataHoraInicio { get; set; }
    public DateTime? DataHoraFim { get; set; }

    [Required, Range(0.1, 5000)]
    public decimal DistanciaPercorridaKm { get; set; }

    [MaxLength(500)] public string? Observacoes { get; set; }
}

public class RegistroUsoResponse
{
    public int Id { get; set; }
    public string VeiculoModelo { get; set; } = string.Empty;
    public string VeiculoPlaca { get; set; } = string.Empty;
    public string RotaNome { get; set; } = string.Empty;
    public string RotaTipo { get; set; } = string.Empty;
    public DateTime DataHoraInicio { get; set; }
    public DateTime? DataHoraFim { get; set; }
    public decimal DistanciaPercorridaKm { get; set; }
    public decimal Co2EvitadoKg { get; set; }
    public decimal EnergiaConsumidaKWh { get; set; }
    public int PontosGanhos { get; set; }
    public string? Observacoes { get; set; }
}

// ────────── Recompensa ──────────
public class RecompensaResponse
{
    public int Id { get; set; }
    public int VeiculoEletricoId { get; set; }
    public string ProprietarioNome { get; set; } = string.Empty;
    public int PontosAcumulados { get; set; }
    public int PontosResgatados { get; set; }
    public int PontosSaldo { get; set; }
    public string NivelRecompensa { get; set; } = string.Empty;
    public decimal TotalCo2EvitadoKg { get; set; }
    public decimal TotalKmSustentaveis { get; set; }
    public DateTime UltimaAtualizacao { get; set; }
}

public class ResgateRequest
{
    [Required] public int RecompensaId { get; set; }

    [Required, MaxLength(200)]
    public string DescricaoBeneficio { get; set; } = string.Empty;

    [Required, Range(1, 100000)]
    public int PontosUtilizados { get; set; }

    [Required] public string TipoBeneficio { get; set; } = string.Empty;
}

// ────────── Relatório ──────────
public class RelatorioRequest
{
    [Required] public string Cidade { get; set; } = string.Empty;
    [Required, Range(1, 12)] public int Mes { get; set; }
    [Required, Range(2020, 2030)] public int Ano { get; set; }
}

public class RelatorioResponse
{
    public int Id { get; set; }
    public string Cidade { get; set; } = string.Empty;
    public int Mes { get; set; }
    public int Ano { get; set; }
    public string Periodo => $"{Mes:00}/{Ano}";
    public decimal TotalCo2EvitadoKg { get; set; }
    public decimal TotalCo2EvitadoToneladas => TotalCo2EvitadoKg / 1000;
    public decimal TotalKmSustentaveis { get; set; }
    public int TotalViagens { get; set; }
    public int TotalVeiculosAtivos { get; set; }
    public decimal EconomiaEnergiaKWh { get; set; }
    public decimal EconomiaFinanceiraEstimadaReais { get; set; }
    public DateTime GeradoEm { get; set; }
    public string EquivalenteArvores => $"≈ {(TotalCo2EvitadoKg / 21.77m):N0} árvores plantadas";
}

// ────────── Auth ──────────
public class LoginRequest
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Senha { get; set; } = string.Empty;
}

public class RegisterRequest
{
    [Required] public string Nome { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, MinLength(6)] public string Senha { get; set; } = string.Empty;
    public string Perfil { get; set; } = "Usuario";
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Perfil { get; set; } = string.Empty;
    public DateTime Expiracao { get; set; }
}

// ────────── Erro padrão ──────────
public class ApiError
{
    public string Mensagem { get; set; } = string.Empty;
    public IEnumerable<string>? Detalhes { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
