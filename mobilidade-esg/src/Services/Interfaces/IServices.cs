using MobilidadeESG.ViewModels;

namespace MobilidadeESG.Services.Interfaces;

public interface IVeiculoEletricoService
{
    Task<PaginatedResult<VeiculoResponse>> GetAllAsync(int pagina, int itensPorPagina, string? tipo = null, bool? ativo = null);
    Task<VeiculoResponse?> GetByIdAsync(int id);
    Task<VeiculoResponse> CreateAsync(VeiculoRequest request);
    Task<VeiculoResponse?> UpdateAsync(int id, VeiculoRequest request);
    Task<bool> DeleteAsync(int id);
}

public interface ITransportePublicoService
{
    Task<PaginatedResult<RotaResponse>> GetRotasAsync(int pagina, int itensPorPagina, string? cidade = null, string? tipo = null);
    Task<RotaResponse?> GetRotaByIdAsync(int id);
    Task<RotaResponse> CreateRotaAsync(RotaRequest request);
    Task<RotaResponse?> UpdateRotaAsync(int id, RotaRequest request);
    Task<bool> DeleteRotaAsync(int id);
    Task<PaginatedResult<RegistroUsoResponse>> GetRegistrosAsync(int pagina, int itensPorPagina, int? veiculoId = null);
    Task<RegistroUsoResponse> RegistrarUsoAsync(RegistroUsoRequest request);
}

public interface IRecompensaService
{
    Task<PaginatedResult<RecompensaResponse>> GetAllAsync(int pagina, int itensPorPagina, string? nivel = null);
    Task<RecompensaResponse?> GetByIdAsync(int id);
    Task<RecompensaResponse?> GetByVeiculoIdAsync(int veiculoId);
    Task<RecompensaResponse> ResgateAsync(ResgateRequest request);
}

public interface IRelatorioService
{
    Task<PaginatedResult<RelatorioResponse>> GetAllAsync(int pagina, int itensPorPagina, string? cidade = null, int? ano = null);
    Task<RelatorioResponse?> GetByIdAsync(int id);
    Task<RelatorioResponse> GerarRelatorioAsync(RelatorioRequest request);
}

public interface IAuthService
{
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
}
