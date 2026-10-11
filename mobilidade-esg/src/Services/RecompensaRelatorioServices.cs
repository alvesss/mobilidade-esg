using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MobilidadeESG.Models;
using MobilidadeESG.Repositories.Interfaces;
using MobilidadeESG.Services.Interfaces;
using MobilidadeESG.ViewModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MobilidadeESG.Services;

public class RecompensaService : IRecompensaService
{
    private readonly IRecompensaRepository _repo;
    public RecompensaService(IRecompensaRepository repo) => _repo = repo;

    public async Task<PaginatedResult<RecompensaResponse>> GetAllAsync(int pagina, int itensPorPagina, string? nivel = null)
    {
        var result = await _repo.GetAllAsync(pagina, itensPorPagina, nivel);
        return new PaginatedResult<RecompensaResponse>
        {
            Data = result.Data.Select(MapToResponse),
            TotalItens = result.TotalItens,
            TotalPaginas = result.TotalPaginas,
            PaginaAtual = result.PaginaAtual,
            ItensPorPagina = result.ItensPorPagina
        };
    }

    public async Task<RecompensaResponse?> GetByIdAsync(int id)
    {
        var r = await _repo.GetByIdAsync(id);
        return r == null ? null : MapToResponse(r);
    }

    public async Task<RecompensaResponse?> GetByVeiculoIdAsync(int veiculoId)
    {
        var r = await _repo.GetByVeiculoIdAsync(veiculoId);
        return r == null ? null : MapToResponse(r);
    }

    public async Task<RecompensaResponse> ResgateAsync(ResgateRequest request)
    {
        var recompensa = await _repo.GetByIdAsync(request.RecompensaId)
            ?? throw new KeyNotFoundException("Carteira de recompensas não encontrada.");

        if (recompensa.PontosSaldo < request.PontosUtilizados)
            throw new InvalidOperationException($"Saldo insuficiente. Você tem {recompensa.PontosSaldo} pontos, mas tentou resgatar {request.PontosUtilizados}.");

        recompensa.PontosResgatados += request.PontosUtilizados;
        recompensa.UltimaAtualizacao = DateTime.UtcNow;
        await _repo.UpdateAsync(recompensa);

        await _repo.AddResgateAsync(new ResgateBeneficio
        {
            RecompensaId = request.RecompensaId,
            DescricaoBeneficio = request.DescricaoBeneficio,
            PontosUtilizados = request.PontosUtilizados,
            TipoBeneficio = request.TipoBeneficio
        });

        return MapToResponse(recompensa);
    }

    private static RecompensaResponse MapToResponse(Recompensa r) => new()
    {
        Id = r.Id,
        VeiculoEletricoId = r.VeiculoEletricoId,
        ProprietarioNome = r.VeiculoEletrico?.ProprietarioNome ?? "",
        PontosAcumulados = r.PontosAcumulados,
        PontosResgatados = r.PontosResgatados,
        PontosSaldo = r.PontosSaldo,
        NivelRecompensa = r.NivelRecompensa,
        TotalCo2EvitadoKg = r.TotalCo2EvitadoKg,
        TotalKmSustentaveis = r.TotalKmSustentaveis,
        UltimaAtualizacao = r.UltimaAtualizacao
    };
}

public class RelatorioService : IRelatorioService
{
    private readonly IRelatorioRepository _repo;
    private readonly IRegistroUsoRepository _registroRepo;
    private readonly IVeiculoEletricoRepository _veiculoRepo;

    public RelatorioService(IRelatorioRepository repo, IRegistroUsoRepository registroRepo, IVeiculoEletricoRepository veiculoRepo)
    {
        _repo = repo;
        _registroRepo = registroRepo;
        _veiculoRepo = veiculoRepo;
    }

    public async Task<PaginatedResult<RelatorioResponse>> GetAllAsync(int pagina, int itensPorPagina, string? cidade = null, int? ano = null)
    {
        var result = await _repo.GetAllAsync(pagina, itensPorPagina, cidade, ano);
        return new PaginatedResult<RelatorioResponse>
        {
            Data = result.Data.Select(MapToResponse),
            TotalItens = result.TotalItens,
            TotalPaginas = result.TotalPaginas,
            PaginaAtual = result.PaginaAtual,
            ItensPorPagina = result.ItensPorPagina
        };
    }

    public async Task<RelatorioResponse?> GetByIdAsync(int id)
    {
        var r = await _repo.GetByIdAsync(id);
        return r == null ? null : MapToResponse(r);
    }

    public async Task<RelatorioResponse> GerarRelatorioAsync(RelatorioRequest request)
    {
        // Verifica se já existe um relatório para esse período/cidade
        var existente = await _repo.GetByCidadeMesAnoAsync(request.Cidade, request.Mes, request.Ano);

        var inicio = new DateTime(request.Ano, request.Mes, 1, 0, 0, 0, DateTimeKind.Utc);
        var fim = inicio.AddMonths(1).AddSeconds(-1);

        var registros = (await _registroRepo.GetByPeriodoAsync(inicio, fim, request.Cidade)).ToList();
        var veiculos = await _veiculoRepo.GetAllAsync(1, 10000, null, true);

        var co2Total = registros.Sum(r => r.Co2EvitadoKg);
        var kmTotal = registros.Sum(r => r.DistanciaPercorridaKm);
        var energiaTotal = registros.Sum(r => r.EnergiaConsumidaKWh);

        // Estimativa: R$0,70 por kWh economizado vs gasolina a R$6,00/L (consumo médio 10km/L)
        var economiaCombustivel = kmTotal * (6.0m / 10.0m);
        var custoEnergia = energiaTotal * 0.70m;
        var economiaFinanceira = economiaCombustivel - custoEnergia;

        RelatorioImpacto relatorio;

        if (existente != null)
        {
            existente.TotalCo2EvitadoKg = co2Total;
            existente.TotalKmSustentaveis = kmTotal;
            existente.TotalViagens = registros.Count;
            existente.TotalVeiculosAtivos = veiculos.TotalItens;
            existente.EconomiaEnergiaKWh = energiaTotal;
            existente.EconomiaFinanceiraEstimadaReais = economiaFinanceira;
            existente.GeradoEm = DateTime.UtcNow;
            relatorio = await _repo.UpdateAsync(existente);
        }
        else
        {
            relatorio = await _repo.CreateAsync(new RelatorioImpacto
            {
                Cidade = request.Cidade,
                Mes = request.Mes,
                Ano = request.Ano,
                TotalCo2EvitadoKg = co2Total,
                TotalKmSustentaveis = kmTotal,
                TotalViagens = registros.Count,
                TotalVeiculosAtivos = veiculos.TotalItens,
                EconomiaEnergiaKWh = energiaTotal,
                EconomiaFinanceiraEstimadaReais = economiaFinanceira
            });
        }

        return MapToResponse(relatorio);
    }

    private static RelatorioResponse MapToResponse(RelatorioImpacto r) => new()
    {
        Id = r.Id,
        Cidade = r.Cidade,
        Mes = r.Mes,
        Ano = r.Ano,
        TotalCo2EvitadoKg = r.TotalCo2EvitadoKg,
        TotalKmSustentaveis = r.TotalKmSustentaveis,
        TotalViagens = r.TotalViagens,
        TotalVeiculosAtivos = r.TotalVeiculosAtivos,
        EconomiaEnergiaKWh = r.EconomiaEnergiaKWh,
        EconomiaFinanceiraEstimadaReais = r.EconomiaFinanceiraEstimadaReais,
        GeradoEm = r.GeradoEm
    };
}

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _repo;
    private readonly IConfiguration _config;

    public AuthService(IUsuarioRepository repo, IConfiguration config)
    {
        _repo = repo;
        _config = config;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var usuario = await _repo.GetByEmailAsync(request.Email);
        if (usuario == null || !BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash))
            return null;

        return GerarToken(usuario);
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _repo.EmailExistsAsync(request.Email))
            throw new InvalidOperationException("Este e-mail já está cadastrado.");

        var usuario = new Usuario
        {
            Nome = request.Nome,
            Email = request.Email.ToLower(),
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha),
            Perfil = request.Perfil == "Admin" ? "Usuario" : request.Perfil // Segurança: não permite criar admin via endpoint público
        };

        var criado = await _repo.CreateAsync(usuario);
        return GerarToken(criado);
    }

    private AuthResponse GerarToken(Usuario usuario)
    {
        var jwtSettings = _config.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiracao = DateTime.UtcNow.AddHours(int.Parse(jwtSettings["ExpirationHours"] ?? "8"));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Perfil)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: expiracao,
            signingCredentials: creds
        );

        return new AuthResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Nome = usuario.Nome,
            Email = usuario.Email,
            Perfil = usuario.Perfil,
            Expiracao = expiracao
        };
    }
}
