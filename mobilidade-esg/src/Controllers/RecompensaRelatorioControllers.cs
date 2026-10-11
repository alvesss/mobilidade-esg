using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobilidadeESG.Services.Interfaces;
using MobilidadeESG.ViewModels;

namespace MobilidadeESG.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class RecompensaController : ControllerBase
{
    private readonly IRecompensaService _service;

    public RecompensaController(IRecompensaService service) => _service = service;

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(PaginatedResult<RecompensaResponse>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pagina = 1,
        [FromQuery] int itensPorPagina = 10,
        [FromQuery] string? nivel = null)
    {
        if (pagina < 1) pagina = 1;
        if (itensPorPagina < 1 || itensPorPagina > 100) itensPorPagina = 10;

        var result = await _service.GetAllAsync(pagina, itensPorPagina, nivel);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(RecompensaResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var recompensa = await _service.GetByIdAsync(id);
        if (recompensa == null) return NotFound(new ApiError { Mensagem = $"Recompensa com ID {id} não encontrada." });
        return Ok(recompensa);
    }

    [HttpGet("veiculo/{veiculoId:int}")]
    [Authorize]
    [ProducesResponseType(typeof(RecompensaResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByVeiculoId(int veiculoId)
    {
        var recompensa = await _service.GetByVeiculoIdAsync(veiculoId);
        if (recompensa == null) return NotFound(new ApiError { Mensagem = $"Nenhuma carteira encontrada para o veículo {veiculoId}." });
        return Ok(recompensa);
    }

    [HttpPost("resgatar")]
    [Authorize]
    [ProducesResponseType(typeof(RecompensaResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(ApiError), 404)]
    public async Task<IActionResult> Resgatar([FromBody] ResgateRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ApiError
            {
                Mensagem = "Dados inválidos.",
                Detalhes = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
            });

        try
        {
            var resultado = await _service.ResgateAsync(request);
            return Ok(resultado);
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

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class RelatorioController : ControllerBase
{
    private readonly IRelatorioService _service;

    public RelatorioController(IRelatorioService service) => _service = service;

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(PaginatedResult<RelatorioResponse>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pagina = 1,
        [FromQuery] int itensPorPagina = 10,
        [FromQuery] string? cidade = null,
        [FromQuery] int? ano = null)
    {
        if (pagina < 1) pagina = 1;
        if (itensPorPagina < 1 || itensPorPagina > 100) itensPorPagina = 10;

        var result = await _service.GetAllAsync(pagina, itensPorPagina, cidade, ano);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(RelatorioResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var relatorio = await _service.GetByIdAsync(id);
        if (relatorio == null) return NotFound(new ApiError { Mensagem = $"Relatório com ID {id} não encontrado." });
        return Ok(relatorio);
    }

    [HttpPost("gerar")]
    [Authorize(Roles = "Admin,Gestor")]
    [ProducesResponseType(typeof(RelatorioResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 400)]
    public async Task<IActionResult> Gerar([FromBody] RelatorioRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ApiError
            {
                Mensagem = "Dados inválidos.",
                Detalhes = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
            });

        var relatorio = await _service.GerarRelatorioAsync(request);
        return Ok(relatorio);
    }
}

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _service;

    public AuthController(IAuthService service) => _service = service;

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    [ProducesResponseType(typeof(ApiError), 401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ApiError { Mensagem = "E-mail ou senha inválidos." });

        var result = await _service.LoginAsync(request);
        if (result == null)
            return Unauthorized(new ApiError { Mensagem = "Credenciais inválidas. Verifique e-mail e senha." });

        return Ok(result);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), 201)]
    [ProducesResponseType(typeof(ApiError), 400)]
    [ProducesResponseType(typeof(ApiError), 409)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ApiError
            {
                Mensagem = "Dados inválidos.",
                Detalhes = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
            });

        try
        {
            var result = await _service.RegisterAsync(request);
            return StatusCode(201, result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ApiError { Mensagem = ex.Message });
        }
    }
}
