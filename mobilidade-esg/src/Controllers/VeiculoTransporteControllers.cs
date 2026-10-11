using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobilidadeESG.Services.Interfaces;
using MobilidadeESG.ViewModels;

namespace MobilidadeESG.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class VeiculoEletricoController : ControllerBase
{
    private readonly IVeiculoEletricoService _service;
    private readonly ILogger<VeiculoEletricoController> _logger;

    public VeiculoEletricoController(IVeiculoEletricoService service, ILogger<VeiculoEletricoController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(PaginatedResult<VeiculoResponse>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pagina = 1,
        [FromQuery] int itensPorPagina = 10,
        [FromQuery] string? tipo = null,
        [FromQuery] bool? ativo = null)
    {
        if (pagina < 1) pagina = 1;
        if (itensPorPagina < 1 || itensPorPagina > 100) itensPorPagina = 10;

        var result = await _service.GetAllAsync(pagina, itensPorPagina, tipo, ativo);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(VeiculoResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var veiculo = await _service.GetByIdAsync(id);
        if (veiculo == null) return NotFound(new ApiError { Mensagem = $"Veículo com ID {id} não encontrado." });
        return Ok(veiculo);
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(VeiculoResponse), 201)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(ApiError), 409)]
    public async Task<IActionResult> Create([FromBody] VeiculoRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ApiError
            {
                Mensagem = "Dados inválidos.",
                Detalhes = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
            });

        try
        {
            var criado = await _service.CreateAsync(request);
            _logger.LogInformation("Veículo {Placa} cadastrado com sucesso.", criado.Placa);
            return CreatedAtAction(nameof(GetById), new { id = criado.Id }, criado);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ApiError { Mensagem = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(VeiculoResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] VeiculoRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ApiError
            {
                Mensagem = "Dados inválidos.",
                Detalhes = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
            });

        try
        {
            var atualizado = await _service.UpdateAsync(id, request);
            if (atualizado == null) return NotFound(new ApiError { Mensagem = $"Veículo com ID {id} não encontrado." });
            return Ok(atualizado);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ApiError { Mensagem = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Gestor")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var removido = await _service.DeleteAsync(id);
        if (!removido) return NotFound(new ApiError { Mensagem = $"Veículo com ID {id} não encontrado." });
        return NoContent();
    }
}

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TransportePublicoController : ControllerBase
{
    private readonly ITransportePublicoService _service;
    private readonly ILogger<TransportePublicoController> _logger;

    public TransportePublicoController(ITransportePublicoService service, ILogger<TransportePublicoController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("rotas")]
    [Authorize]
    [ProducesResponseType(typeof(PaginatedResult<RotaResponse>), 200)]
    public async Task<IActionResult> GetRotas(
        [FromQuery] int pagina = 1,
        [FromQuery] int itensPorPagina = 10,
        [FromQuery] string? cidade = null,
        [FromQuery] string? tipo = null)
    {
        if (pagina < 1) pagina = 1;
        if (itensPorPagina < 1 || itensPorPagina > 100) itensPorPagina = 10;

        var result = await _service.GetRotasAsync(pagina, itensPorPagina, cidade, tipo);
        return Ok(result);
    }

    [HttpGet("rotas/{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(RotaResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetRotaById(int id)
    {
        var rota = await _service.GetRotaByIdAsync(id);
        if (rota == null) return NotFound(new ApiError { Mensagem = $"Rota com ID {id} não encontrada." });
        return Ok(rota);
    }

    [HttpPost("rotas")]
    [Authorize(Roles = "Admin,Gestor")]
    [ProducesResponseType(typeof(RotaResponse), 201)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> CreateRota([FromBody] RotaRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ApiError
            {
                Mensagem = "Dados inválidos.",
                Detalhes = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
            });

        var criada = await _service.CreateRotaAsync(request);
        return CreatedAtAction(nameof(GetRotaById), new { id = criada.Id }, criada);
    }

    [HttpPut("rotas/{id:int}")]
    [Authorize(Roles = "Admin,Gestor")]
    [ProducesResponseType(typeof(RotaResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateRota(int id, [FromBody] RotaRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ApiError { Mensagem = "Dados inválidos." });

        var atualizada = await _service.UpdateRotaAsync(id, request);
        if (atualizada == null) return NotFound(new ApiError { Mensagem = $"Rota com ID {id} não encontrada." });
        return Ok(atualizada);
    }

    [HttpDelete("rotas/{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteRota(int id)
    {
        var removida = await _service.DeleteRotaAsync(id);
        if (!removida) return NotFound(new ApiError { Mensagem = $"Rota com ID {id} não encontrada." });
        return NoContent();
    }

    [HttpGet("registros")]
    [Authorize]
    [ProducesResponseType(typeof(PaginatedResult<RegistroUsoResponse>), 200)]
    public async Task<IActionResult> GetRegistros(
        [FromQuery] int pagina = 1,
        [FromQuery] int itensPorPagina = 10,
        [FromQuery] int? veiculoId = null)
    {
        if (pagina < 1) pagina = 1;
        if (itensPorPagina < 1 || itensPorPagina > 100) itensPorPagina = 10;

        var result = await _service.GetRegistrosAsync(pagina, itensPorPagina, veiculoId);
        return Ok(result);
    }

    [HttpPost("registros")]
    [Authorize]
    [ProducesResponseType(typeof(RegistroUsoResponse), 201)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> RegistrarUso([FromBody] RegistroUsoRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ApiError
            {
                Mensagem = "Dados inválidos.",
                Detalhes = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
            });

        try
        {
            var registro = await _service.RegistrarUsoAsync(request);
            _logger.LogInformation("Uso registrado para veículo {VeiculoId}, CO2 evitado: {Co2}kg", request.VeiculoEletricoId, registro.Co2EvitadoKg);
            return StatusCode(201, registro);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiError { Mensagem = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiError { Mensagem = ex.Message });
        }
    }
}
