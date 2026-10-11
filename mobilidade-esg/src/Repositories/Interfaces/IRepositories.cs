using MobilidadeESG.Models;
using MobilidadeESG.ViewModels;

namespace MobilidadeESG.Repositories.Interfaces;

public interface IVeiculoEletricoRepository
{
    Task<PaginatedResult<VeiculoEletrico>> GetAllAsync(int pagina, int itensPorPagina, string? tipo = null, bool? ativo = null);
    Task<VeiculoEletrico?> GetByIdAsync(int id);
    Task<VeiculoEletrico?> GetByPlacaAsync(string placa);
    Task<VeiculoEletrico> CreateAsync(VeiculoEletrico veiculo);
    Task<VeiculoEletrico> UpdateAsync(VeiculoEletrico veiculo);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> PlacaExistsAsync(string placa, int? excludeId = null);
}

public interface IRotaTransporteRepository
{
    Task<PaginatedResult<RotaTransporte>> GetAllAsync(int pagina, int itensPorPagina, string? cidade = null, string? tipo = null);
    Task<RotaTransporte?> GetByIdAsync(int id);
    Task<RotaTransporte> CreateAsync(RotaTransporte rota);
    Task<RotaTransporte> UpdateAsync(RotaTransporte rota);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}

public interface IRegistroUsoRepository
{
    Task<PaginatedResult<RegistroUso>> GetAllAsync(int pagina, int itensPorPagina, int? veiculoId = null);
    Task<RegistroUso?> GetByIdAsync(int id);
    Task<RegistroUso> CreateAsync(RegistroUso registro);
    Task<IEnumerable<RegistroUso>> GetByVeiculoIdAsync(int veiculoId);
    Task<IEnumerable<RegistroUso>> GetByPeriodoAsync(DateTime inicio, DateTime fim, string? cidade = null);
}

public interface IRecompensaRepository
{
    Task<PaginatedResult<Recompensa>> GetAllAsync(int pagina, int itensPorPagina, string? nivel = null);
    Task<Recompensa?> GetByIdAsync(int id);
    Task<Recompensa?> GetByVeiculoIdAsync(int veiculoId);
    Task<Recompensa> CreateAsync(Recompensa recompensa);
    Task<Recompensa> UpdateAsync(Recompensa recompensa);
    Task<ResgateBeneficio> AddResgateAsync(ResgateBeneficio resgate);
}

public interface IRelatorioRepository
{
    Task<PaginatedResult<RelatorioImpacto>> GetAllAsync(int pagina, int itensPorPagina, string? cidade = null, int? ano = null);
    Task<RelatorioImpacto?> GetByIdAsync(int id);
    Task<RelatorioImpacto?> GetByCidadeMesAnoAsync(string cidade, int mes, int ano);
    Task<RelatorioImpacto> CreateAsync(RelatorioImpacto relatorio);
    Task<RelatorioImpacto> UpdateAsync(RelatorioImpacto relatorio);
}

public interface IUsuarioRepository
{
    Task<Usuario?> GetByEmailAsync(string email);
    Task<Usuario?> GetByIdAsync(int id);
    Task<Usuario> CreateAsync(Usuario usuario);
    Task<bool> EmailExistsAsync(string email);
}
