using MobilidadeESG.Models;
using MobilidadeESG.Repositories.Interfaces;
using MobilidadeESG.Services.Interfaces;
using MobilidadeESG.ViewModels;

namespace MobilidadeESG.Services;

public class VeiculoEletricoService : IVeiculoEletricoService
{
    private readonly IVeiculoEletricoRepository _repo;
    private readonly IRecompensaRepository _recompensaRepo;

    public VeiculoEletricoService(IVeiculoEletricoRepository repo, IRecompensaRepository recompensaRepo)
    {
        _repo = repo;
        _recompensaRepo = recompensaRepo;
    }

    public async Task<PaginatedResult<VeiculoResponse>> GetAllAsync(int pagina, int itensPorPagina, string? tipo = null, bool? ativo = null)
    {
        var result = await _repo.GetAllAsync(pagina, itensPorPagina, tipo, ativo);
        return new PaginatedResult<VeiculoResponse>
        {
            Data = result.Data.Select(MapToResponse),
            TotalItens = result.TotalItens,
            TotalPaginas = result.TotalPaginas,
            PaginaAtual = result.PaginaAtual,
            ItensPorPagina = result.ItensPorPagina
        };
    }

    public async Task<VeiculoResponse?> GetByIdAsync(int id)
    {
        var veiculo = await _repo.GetByIdAsync(id);
        return veiculo == null ? null : MapToResponse(veiculo);
    }

    public async Task<VeiculoResponse> CreateAsync(VeiculoRequest request)
    {
        if (await _repo.PlacaExistsAsync(request.Placa))
            throw new InvalidOperationException($"Já existe um veículo cadastrado com a placa {request.Placa}.");

        var veiculo = new VeiculoEletrico
        {
            Modelo = request.Modelo,
            Fabricante = request.Fabricante,
            AnoFabricacao = request.AnoFabricacao,
            AutonomiaKm = request.AutonomiaKm,
            ConsumoKWhPorKm = request.ConsumoKWhPorKm,
            TipoVeiculo = request.TipoVeiculo,
            Placa = request.Placa.ToUpper(),
            ProprietarioNome = request.ProprietarioNome,
            Email = request.Email.ToLower(),
            DataCadastro = DateTime.UtcNow,
            Ativo = true
        };

        var criado = await _repo.CreateAsync(veiculo);

        // Cria carteira de recompensas automaticamente
        await _recompensaRepo.CreateAsync(new Recompensa
        {
            VeiculoEletricoId = criado.Id,
            PontosAcumulados = 0,
            PontosResgatados = 0,
            NivelRecompensa = "Bronze"
        });

        return MapToResponse(criado);
    }

    public async Task<VeiculoResponse?> UpdateAsync(int id, VeiculoRequest request)
    {
        var veiculo = await _repo.GetByIdAsync(id);
        if (veiculo == null) return null;

        if (await _repo.PlacaExistsAsync(request.Placa, id))
            throw new InvalidOperationException($"A placa {request.Placa} já está em uso por outro veículo.");

        veiculo.Modelo = request.Modelo;
        veiculo.Fabricante = request.Fabricante;
        veiculo.AnoFabricacao = request.AnoFabricacao;
        veiculo.AutonomiaKm = request.AutonomiaKm;
        veiculo.ConsumoKWhPorKm = request.ConsumoKWhPorKm;
        veiculo.TipoVeiculo = request.TipoVeiculo;
        veiculo.Placa = request.Placa.ToUpper();
        veiculo.ProprietarioNome = request.ProprietarioNome;
        veiculo.Email = request.Email.ToLower();

        var atualizado = await _repo.UpdateAsync(veiculo);
        return MapToResponse(atualizado);
    }

    public async Task<bool> DeleteAsync(int id) => await _repo.DeleteAsync(id);

    private static VeiculoResponse MapToResponse(VeiculoEletrico v) => new()
    {
        Id = v.Id,
        Modelo = v.Modelo,
        Fabricante = v.Fabricante,
        AnoFabricacao = v.AnoFabricacao,
        AutonomiaKm = v.AutonomiaKm,
        ConsumoKWhPorKm = v.ConsumoKWhPorKm,
        TipoVeiculo = v.TipoVeiculo,
        Placa = v.Placa,
        ProprietarioNome = v.ProprietarioNome,
        Email = v.Email,
        Ativo = v.Ativo,
        TotalKmRodados = v.TotalKmRodados,
        DataCadastro = v.DataCadastro,
        Co2TotalEvitadoKg = v.RegistrosUso?.Sum(r => r.Co2EvitadoKg) ?? 0
    };
}

public class TransportePublicoService : ITransportePublicoService
{
    private readonly IRotaTransporteRepository _rotaRepo;
    private readonly IRegistroUsoRepository _registroRepo;
    private readonly IVeiculoEletricoRepository _veiculoRepo;
    private readonly IRecompensaRepository _recompensaRepo;

    public TransportePublicoService(
        IRotaTransporteRepository rotaRepo,
        IRegistroUsoRepository registroRepo,
        IVeiculoEletricoRepository veiculoRepo,
        IRecompensaRepository recompensaRepo)
    {
        _rotaRepo = rotaRepo;
        _registroRepo = registroRepo;
        _veiculoRepo = veiculoRepo;
        _recompensaRepo = recompensaRepo;
    }

    public async Task<PaginatedResult<RotaResponse>> GetRotasAsync(int pagina, int itensPorPagina, string? cidade = null, string? tipo = null)
    {
        var result = await _rotaRepo.GetAllAsync(pagina, itensPorPagina, cidade, tipo);
        return new PaginatedResult<RotaResponse>
        {
            Data = result.Data.Select(MapRotaToResponse),
            TotalItens = result.TotalItens,
            TotalPaginas = result.TotalPaginas,
            PaginaAtual = result.PaginaAtual,
            ItensPorPagina = result.ItensPorPagina
        };
    }

    public async Task<RotaResponse?> GetRotaByIdAsync(int id)
    {
        var rota = await _rotaRepo.GetByIdAsync(id);
        return rota == null ? null : MapRotaToResponse(rota);
    }

    public async Task<RotaResponse> CreateRotaAsync(RotaRequest request)
    {
        var rota = new RotaTransporte
        {
            Nome = request.Nome,
            Origem = request.Origem,
            Destino = request.Destino,
            DistanciaKm = request.DistanciaKm,
            TipoTransporte = request.TipoTransporte,
            Cidade = request.Cidade,
            Estado = request.Estado,
            PontosPorKm = request.PontosPorKm,
            Co2EvitadoPorKmKg = 0.21m
        };
        var criada = await _rotaRepo.CreateAsync(rota);
        return MapRotaToResponse(criada);
    }

    public async Task<RotaResponse?> UpdateRotaAsync(int id, RotaRequest request)
    {
        var rota = await _rotaRepo.GetByIdAsync(id);
        if (rota == null) return null;

        rota.Nome = request.Nome;
        rota.Origem = request.Origem;
        rota.Destino = request.Destino;
        rota.DistanciaKm = request.DistanciaKm;
        rota.TipoTransporte = request.TipoTransporte;
        rota.Cidade = request.Cidade;
        rota.Estado = request.Estado;
        rota.PontosPorKm = request.PontosPorKm;

        var atualizada = await _rotaRepo.UpdateAsync(rota);
        return MapRotaToResponse(atualizada);
    }

    public async Task<bool> DeleteRotaAsync(int id) => await _rotaRepo.DeleteAsync(id);

    public async Task<PaginatedResult<RegistroUsoResponse>> GetRegistrosAsync(int pagina, int itensPorPagina, int? veiculoId = null)
    {
        var result = await _registroRepo.GetAllAsync(pagina, itensPorPagina, veiculoId);
        return new PaginatedResult<RegistroUsoResponse>
        {
            Data = result.Data.Select(MapRegistroToResponse),
            TotalItens = result.TotalItens,
            TotalPaginas = result.TotalPaginas,
            PaginaAtual = result.PaginaAtual,
            ItensPorPagina = result.ItensPorPagina
        };
    }

    public async Task<RegistroUsoResponse> RegistrarUsoAsync(RegistroUsoRequest request)
    {
        var veiculo = await _veiculoRepo.GetByIdAsync(request.VeiculoEletricoId)
            ?? throw new KeyNotFoundException("Veículo não encontrado.");

        var rota = await _rotaRepo.GetByIdAsync(request.RotaTransporteId)
            ?? throw new KeyNotFoundException("Rota não encontrada.");

        if (!rota.Ativa)
            throw new InvalidOperationException("Esta rota está inativa.");

        // Calcula impacto
        var co2Evitado = rota.Co2EvitadoPorKmKg * request.DistanciaPercorridaKm;
        var energiaConsumida = veiculo.ConsumoKWhPorKm * request.DistanciaPercorridaKm;
        var pontos = (int)(rota.PontosPorKm * request.DistanciaPercorridaKm);

        var registro = new RegistroUso
        {
            VeiculoEletricoId = request.VeiculoEletricoId,
            RotaTransporteId = request.RotaTransporteId,
            DataHoraInicio = request.DataHoraInicio,
            DataHoraFim = request.DataHoraFim,
            DistanciaPercorridaKm = request.DistanciaPercorridaKm,
            Co2EvitadoKg = co2Evitado,
            EnergiaConsumidaKWh = energiaConsumida,
            PontosGanhos = pontos,
            Observacoes = request.Observacoes
        };

        var criado = await _registroRepo.CreateAsync(registro);

        // Atualiza km total do veículo
        veiculo.TotalKmRodados += request.DistanciaPercorridaKm;
        await _veiculoRepo.UpdateAsync(veiculo);

        // Atualiza recompensas
        var recompensa = await _recompensaRepo.GetByVeiculoIdAsync(request.VeiculoEletricoId);
        if (recompensa != null)
        {
            recompensa.PontosAcumulados += pontos;
            recompensa.TotalCo2EvitadoKg += co2Evitado;
            recompensa.TotalKmSustentaveis += request.DistanciaPercorridaKm;
            recompensa.NivelRecompensa = CalcularNivel(recompensa.PontosAcumulados);
            recompensa.UltimaAtualizacao = DateTime.UtcNow;
            await _recompensaRepo.UpdateAsync(recompensa);
        }

        criado.VeiculoEletrico = veiculo;
        criado.RotaTransporte = rota;
        return MapRegistroToResponse(criado);
    }

    private static string CalcularNivel(int pontos) => pontos switch
    {
        >= 10000 => "Platina",
        >= 5000 => "Ouro",
        >= 1000 => "Prata",
        _ => "Bronze"
    };

    private static RotaResponse MapRotaToResponse(RotaTransporte r) => new()
    {
        Id = r.Id,
        Nome = r.Nome,
        Origem = r.Origem,
        Destino = r.Destino,
        DistanciaKm = r.DistanciaKm,
        TipoTransporte = r.TipoTransporte,
        Cidade = r.Cidade,
        Estado = r.Estado,
        Ativa = r.Ativa,
        Co2EvitadoPorKmKg = r.Co2EvitadoPorKmKg,
        PontosPorKm = r.PontosPorKm,
        TotalViagens = r.RegistrosUso?.Count ?? 0
    };

    private static RegistroUsoResponse MapRegistroToResponse(RegistroUso r) => new()
    {
        Id = r.Id,
        VeiculoModelo = r.VeiculoEletrico?.Modelo ?? "",
        VeiculoPlaca = r.VeiculoEletrico?.Placa ?? "",
        RotaNome = r.RotaTransporte?.Nome ?? "",
        RotaTipo = r.RotaTransporte?.TipoTransporte ?? "",
        DataHoraInicio = r.DataHoraInicio,
        DataHoraFim = r.DataHoraFim,
        DistanciaPercorridaKm = r.DistanciaPercorridaKm,
        Co2EvitadoKg = r.Co2EvitadoKg,
        EnergiaConsumidaKWh = r.EnergiaConsumidaKWh,
        PontosGanhos = r.PontosGanhos,
        Observacoes = r.Observacoes
    };
}
