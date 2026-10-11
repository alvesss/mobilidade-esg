using Microsoft.EntityFrameworkCore;
using MobilidadeESG.Data;
using MobilidadeESG.Models;
using MobilidadeESG.Repositories.Interfaces;
using MobilidadeESG.ViewModels;

namespace MobilidadeESG.Repositories;

public class VeiculoEletricoRepository : IVeiculoEletricoRepository
{
    private readonly AppDbContext _context;
    public VeiculoEletricoRepository(AppDbContext context) => _context = context;

    public async Task<PaginatedResult<VeiculoEletrico>> GetAllAsync(int pagina, int itensPorPagina, string? tipo = null, bool? ativo = null)
    {
        var query = _context.VeiculosEletricos.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(tipo))
            query = query.Where(v => v.TipoVeiculo == tipo);

        if (ativo.HasValue)
            query = query.Where(v => v.Ativo == ativo.Value);

        var total = await query.CountAsync();
        var dados = await query
            .OrderByDescending(v => v.DataCadastro)
            .Skip((pagina - 1) * itensPorPagina)
            .Take(itensPorPagina)
            .ToListAsync();

        return new PaginatedResult<VeiculoEletrico>
        {
            Data = dados,
            TotalItens = total,
            TotalPaginas = (int)Math.Ceiling(total / (double)itensPorPagina),
            PaginaAtual = pagina,
            ItensPorPagina = itensPorPagina
        };
    }

    public async Task<VeiculoEletrico?> GetByIdAsync(int id) =>
        await _context.VeiculosEletricos.Include(v => v.RegistrosUso).Include(v => v.Recompensa).FirstOrDefaultAsync(v => v.Id == id);

    public async Task<VeiculoEletrico?> GetByPlacaAsync(string placa) =>
        await _context.VeiculosEletricos.FirstOrDefaultAsync(v => v.Placa == placa.ToUpper());

    public async Task<VeiculoEletrico> CreateAsync(VeiculoEletrico veiculo)
    {
        _context.VeiculosEletricos.Add(veiculo);
        await _context.SaveChangesAsync();
        return veiculo;
    }

    public async Task<VeiculoEletrico> UpdateAsync(VeiculoEletrico veiculo)
    {
        _context.VeiculosEletricos.Update(veiculo);
        await _context.SaveChangesAsync();
        return veiculo;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var veiculo = await _context.VeiculosEletricos.FindAsync(id);
        if (veiculo == null) return false;
        veiculo.Ativo = false; // soft delete
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id) => await _context.VeiculosEletricos.AnyAsync(v => v.Id == id);
    public async Task<bool> PlacaExistsAsync(string placa, int? excludeId = null) =>
        await _context.VeiculosEletricos.AnyAsync(v => v.Placa == placa.ToUpper() && (!excludeId.HasValue || v.Id != excludeId.Value));
}

public class RotaTransporteRepository : IRotaTransporteRepository
{
    private readonly AppDbContext _context;
    public RotaTransporteRepository(AppDbContext context) => _context = context;

    public async Task<PaginatedResult<RotaTransporte>> GetAllAsync(int pagina, int itensPorPagina, string? cidade = null, string? tipo = null)
    {
        var query = _context.RotasTransporte.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(cidade))
            query = query.Where(r => r.Cidade.Contains(cidade));

        if (!string.IsNullOrEmpty(tipo))
            query = query.Where(r => r.TipoTransporte == tipo);

        var total = await query.CountAsync();
        var dados = await query
            .OrderBy(r => r.Cidade).ThenBy(r => r.Nome)
            .Skip((pagina - 1) * itensPorPagina)
            .Take(itensPorPagina)
            .Include(r => r.RegistrosUso)
            .ToListAsync();

        return new PaginatedResult<RotaTransporte>
        {
            Data = dados,
            TotalItens = total,
            TotalPaginas = (int)Math.Ceiling(total / (double)itensPorPagina),
            PaginaAtual = pagina,
            ItensPorPagina = itensPorPagina
        };
    }

    public async Task<RotaTransporte?> GetByIdAsync(int id) =>
        await _context.RotasTransporte.Include(r => r.RegistrosUso).FirstOrDefaultAsync(r => r.Id == id);

    public async Task<RotaTransporte> CreateAsync(RotaTransporte rota)
    {
        _context.RotasTransporte.Add(rota);
        await _context.SaveChangesAsync();
        return rota;
    }

    public async Task<RotaTransporte> UpdateAsync(RotaTransporte rota)
    {
        _context.RotasTransporte.Update(rota);
        await _context.SaveChangesAsync();
        return rota;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var rota = await _context.RotasTransporte.FindAsync(id);
        if (rota == null) return false;
        rota.Ativa = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id) => await _context.RotasTransporte.AnyAsync(r => r.Id == id);
}

public class RegistroUsoRepository : IRegistroUsoRepository
{
    private readonly AppDbContext _context;
    public RegistroUsoRepository(AppDbContext context) => _context = context;

    public async Task<PaginatedResult<RegistroUso>> GetAllAsync(int pagina, int itensPorPagina, int? veiculoId = null)
    {
        var query = _context.RegistrosUso
            .Include(r => r.VeiculoEletrico)
            .Include(r => r.RotaTransporte)
            .AsNoTracking().AsQueryable();

        if (veiculoId.HasValue)
            query = query.Where(r => r.VeiculoEletricoId == veiculoId.Value);

        var total = await query.CountAsync();
        var dados = await query
            .OrderByDescending(r => r.DataHoraInicio)
            .Skip((pagina - 1) * itensPorPagina)
            .Take(itensPorPagina)
            .ToListAsync();

        return new PaginatedResult<RegistroUso>
        {
            Data = dados,
            TotalItens = total,
            TotalPaginas = (int)Math.Ceiling(total / (double)itensPorPagina),
            PaginaAtual = pagina,
            ItensPorPagina = itensPorPagina
        };
    }

    public async Task<RegistroUso?> GetByIdAsync(int id) =>
        await _context.RegistrosUso.Include(r => r.VeiculoEletrico).Include(r => r.RotaTransporte).FirstOrDefaultAsync(r => r.Id == id);

    public async Task<RegistroUso> CreateAsync(RegistroUso registro)
    {
        _context.RegistrosUso.Add(registro);
        await _context.SaveChangesAsync();
        return registro;
    }

    public async Task<IEnumerable<RegistroUso>> GetByVeiculoIdAsync(int veiculoId) =>
        await _context.RegistrosUso.Where(r => r.VeiculoEletricoId == veiculoId).OrderByDescending(r => r.DataHoraInicio).ToListAsync();

    public async Task<IEnumerable<RegistroUso>> GetByPeriodoAsync(DateTime inicio, DateTime fim, string? cidade = null)
    {
        var query = _context.RegistrosUso
            .Include(r => r.VeiculoEletrico)
            .Include(r => r.RotaTransporte)
            .Where(r => r.DataHoraInicio >= inicio && r.DataHoraInicio <= fim)
            .AsQueryable();

        if (!string.IsNullOrEmpty(cidade))
            query = query.Where(r => r.RotaTransporte!.Cidade == cidade);

        return await query.ToListAsync();
    }
}

public class RecompensaRepository : IRecompensaRepository
{
    private readonly AppDbContext _context;
    public RecompensaRepository(AppDbContext context) => _context = context;

    public async Task<PaginatedResult<Recompensa>> GetAllAsync(int pagina, int itensPorPagina, string? nivel = null)
    {
        var query = _context.Recompensas
            .Include(r => r.VeiculoEletrico)
            .AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(nivel))
            query = query.Where(r => r.NivelRecompensa == nivel);

        var total = await query.CountAsync();
        var dados = await query
            .OrderByDescending(r => r.PontosAcumulados)
            .Skip((pagina - 1) * itensPorPagina)
            .Take(itensPorPagina)
            .ToListAsync();

        return new PaginatedResult<Recompensa>
        {
            Data = dados,
            TotalItens = total,
            TotalPaginas = (int)Math.Ceiling(total / (double)itensPorPagina),
            PaginaAtual = pagina,
            ItensPorPagina = itensPorPagina
        };
    }

    public async Task<Recompensa?> GetByIdAsync(int id) =>
        await _context.Recompensas.Include(r => r.VeiculoEletrico).Include(r => r.Resgates).FirstOrDefaultAsync(r => r.Id == id);

    public async Task<Recompensa?> GetByVeiculoIdAsync(int veiculoId) =>
        await _context.Recompensas.Include(r => r.VeiculoEletrico).Include(r => r.Resgates).FirstOrDefaultAsync(r => r.VeiculoEletricoId == veiculoId);

    public async Task<Recompensa> CreateAsync(Recompensa recompensa)
    {
        _context.Recompensas.Add(recompensa);
        await _context.SaveChangesAsync();
        return recompensa;
    }

    public async Task<Recompensa> UpdateAsync(Recompensa recompensa)
    {
        _context.Recompensas.Update(recompensa);
        await _context.SaveChangesAsync();
        return recompensa;
    }

    public async Task<ResgateBeneficio> AddResgateAsync(ResgateBeneficio resgate)
    {
        _context.ResgatesBeneficios.Add(resgate);
        await _context.SaveChangesAsync();
        return resgate;
    }
}

public class RelatorioRepository : IRelatorioRepository
{
    private readonly AppDbContext _context;
    public RelatorioRepository(AppDbContext context) => _context = context;

    public async Task<PaginatedResult<RelatorioImpacto>> GetAllAsync(int pagina, int itensPorPagina, string? cidade = null, int? ano = null)
    {
        var query = _context.RelatoriosImpacto.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(cidade))
            query = query.Where(r => r.Cidade.Contains(cidade));

        if (ano.HasValue)
            query = query.Where(r => r.Ano == ano.Value);

        var total = await query.CountAsync();
        var dados = await query
            .OrderByDescending(r => r.Ano).ThenByDescending(r => r.Mes)
            .Skip((pagina - 1) * itensPorPagina)
            .Take(itensPorPagina)
            .ToListAsync();

        return new PaginatedResult<RelatorioImpacto>
        {
            Data = dados,
            TotalItens = total,
            TotalPaginas = (int)Math.Ceiling(total / (double)itensPorPagina),
            PaginaAtual = pagina,
            ItensPorPagina = itensPorPagina
        };
    }

    public async Task<RelatorioImpacto?> GetByIdAsync(int id) => await _context.RelatoriosImpacto.FindAsync(id);

    public async Task<RelatorioImpacto?> GetByCidadeMesAnoAsync(string cidade, int mes, int ano) =>
        await _context.RelatoriosImpacto.FirstOrDefaultAsync(r => r.Cidade == cidade && r.Mes == mes && r.Ano == ano);

    public async Task<RelatorioImpacto> CreateAsync(RelatorioImpacto relatorio)
    {
        _context.RelatoriosImpacto.Add(relatorio);
        await _context.SaveChangesAsync();
        return relatorio;
    }

    public async Task<RelatorioImpacto> UpdateAsync(RelatorioImpacto relatorio)
    {
        _context.RelatoriosImpacto.Update(relatorio);
        await _context.SaveChangesAsync();
        return relatorio;
    }
}

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _context;
    public UsuarioRepository(AppDbContext context) => _context = context;

    public async Task<Usuario?> GetByEmailAsync(string email) =>
        await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email.ToLower() && u.Ativo);

    public async Task<Usuario?> GetByIdAsync(int id) => await _context.Usuarios.FindAsync(id);

    public async Task<Usuario> CreateAsync(Usuario usuario)
    {
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
        return usuario;
    }

    public async Task<bool> EmailExistsAsync(string email) =>
        await _context.Usuarios.AnyAsync(u => u.Email == email.ToLower());
}
