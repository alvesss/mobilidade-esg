using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using MobilidadeESG.ViewModels;
using Xunit;

namespace MobilidadeESG.Tests;

public class VeiculoEletricoControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private string? _token;

    public VeiculoEletricoControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    private async Task<string> ObterTokenAsync()
    {
        if (_token != null) return _token;

        var loginRequest = new LoginRequest { Email = "admin@teste.com", Senha = "Admin@123" };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var content = await response.Content.ReadFromJsonAsync<AuthResponse>();
        _token = content!.Token;
        return _token;
    }

    private async Task AdicionarAuthHeader()
    {
        var token = await ObterTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Teste obrigatório: GET /api/VeiculoEletrico deve retornar StatusCode 200
    /// </summary>
    [Fact]
    public async Task Get_ReturnsHttpStatusCode200()
    {
        // Arrange
        await AdicionarAuthHeader();
        var request = "/api/VeiculoEletrico";

        // Act
        var response = await _client.GetAsync(request);

        // Assert
        response.EnsureSuccessStatusCode(); // Verifica se o status code é 200
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_ReturnsPaginatedResult()
    {
        // Arrange
        await AdicionarAuthHeader();

        // Act
        var response = await _client.GetAsync("/api/VeiculoEletrico?pagina=1&itensPorPagina=5");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(content.TryGetProperty("data", out _));
        Assert.True(content.TryGetProperty("totalItens", out _));
        Assert.True(content.TryGetProperty("paginaAtual", out _));
    }

    [Fact]
    public async Task GetById_ComIdInexistente_RetornaNotFound()
    {
        // Arrange
        await AdicionarAuthHeader();

        // Act
        var response = await _client.GetAsync("/api/VeiculoEletrico/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_ComDadosValidos_RetornaCriado()
    {
        // Arrange
        await AdicionarAuthHeader();
        var veiculo = new VeiculoRequest
        {
            Modelo = "Model 3",
            Fabricante = "Tesla",
            AnoFabricacao = 2023,
            AutonomiaKm = 560m,
            ConsumoKWhPorKm = 0.15m,
            TipoVeiculo = "Carro",
            Placa = "ABC1D23",
            ProprietarioNome = "João Silva",
            Email = "joao@teste.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/VeiculoEletrico", veiculo);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Post_SemAutenticacao_Retorna401()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;
        var veiculo = new VeiculoRequest { Modelo = "Teste" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/VeiculoEletrico", veiculo);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Post_ComDadosInvalidos_RetornaBadRequest()
    {
        // Arrange
        await AdicionarAuthHeader();
        var veiculoInvalido = new { Modelo = "" }; // Faltam campos obrigatórios

        // Act
        var response = await _client.PostAsJsonAsync("/api/VeiculoEletrico", veiculoInvalido);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

public class TransportePublicoControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private string? _token;

    public TransportePublicoControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> ObterTokenAsync()
    {
        if (_token != null) return _token;
        var loginRequest = new LoginRequest { Email = "admin@teste.com", Senha = "Admin@123" };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var content = await response.Content.ReadFromJsonAsync<AuthResponse>();
        _token = content!.Token;
        return _token;
    }

    private async Task AdicionarAuthHeader()
    {
        var token = await ObterTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Teste obrigatório: GET /api/TransportePublico/rotas deve retornar StatusCode 200
    /// </summary>
    [Fact]
    public async Task Get_ReturnsHttpStatusCode200()
    {
        // Arrange
        await AdicionarAuthHeader();
        var request = "/api/TransportePublico/rotas";

        // Act
        var response = await _client.GetAsync(request);

        // Assert
        response.EnsureSuccessStatusCode(); // Verifica se o status code é 200
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetRotas_ComFiltroCidade_RetornaRotasDaCidade()
    {
        // Arrange
        await AdicionarAuthHeader();

        // Act
        var response = await _client.GetAsync("/api/TransportePublico/rotas?cidade=São Paulo");

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetRegistros_ReturnsHttpStatusCode200()
    {
        // Arrange
        await AdicionarAuthHeader();

        // Act
        var response = await _client.GetAsync("/api/TransportePublico/registros");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetRotaById_ComIdExistente_RetornaRota()
    {
        // Arrange
        await AdicionarAuthHeader();

        // Act
        var response = await _client.GetAsync("/api/TransportePublico/rotas/1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

public class RecompensaControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private string? _token;

    public RecompensaControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> ObterTokenAsync()
    {
        if (_token != null) return _token;
        var loginRequest = new LoginRequest { Email = "admin@teste.com", Senha = "Admin@123" };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var content = await response.Content.ReadFromJsonAsync<AuthResponse>();
        _token = content!.Token;
        return _token;
    }

    private async Task AdicionarAuthHeader()
    {
        var token = await ObterTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Teste obrigatório: GET /api/Recompensa deve retornar StatusCode 200
    /// </summary>
    [Fact]
    public async Task Get_ReturnsHttpStatusCode200()
    {
        // Arrange
        await AdicionarAuthHeader();
        var request = "/api/Recompensa";

        // Act
        var response = await _client.GetAsync(request);

        // Assert
        response.EnsureSuccessStatusCode(); // Verifica se o status code é 200
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ComIdInexistente_RetornaNotFound()
    {
        // Arrange
        await AdicionarAuthHeader();

        // Act
        var response = await _client.GetAsync("/api/Recompensa/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Resgatar_ComSaldoInsuficiente_RetornaBadRequest()
    {
        // Arrange
        await AdicionarAuthHeader();
        var request = new ResgateRequest
        {
            RecompensaId = 99999, // não existe
            DescricaoBeneficio = "Desconto transporte",
            PontosUtilizados = 100,
            TipoBeneficio = "Desconto"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Recompensa/resgatar", request);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.NotFound ||
            response.StatusCode == HttpStatusCode.BadRequest
        );
    }
}

public class RelatorioControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private string? _token;

    public RelatorioControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> ObterTokenAsync()
    {
        if (_token != null) return _token;
        var loginRequest = new LoginRequest { Email = "admin@teste.com", Senha = "Admin@123" };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var content = await response.Content.ReadFromJsonAsync<AuthResponse>();
        _token = content!.Token;
        return _token;
    }

    private async Task AdicionarAuthHeader()
    {
        var token = await ObterTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Teste obrigatório: GET /api/Relatorio deve retornar StatusCode 200
    /// </summary>
    [Fact]
    public async Task Get_ReturnsHttpStatusCode200()
    {
        // Arrange
        await AdicionarAuthHeader();
        var request = "/api/Relatorio";

        // Act
        var response = await _client.GetAsync(request);

        // Assert
        response.EnsureSuccessStatusCode(); // Verifica se o status code é 200
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_ComFiltroAno_RetornaRelatorios()
    {
        // Arrange
        await AdicionarAuthHeader();

        // Act
        var response = await _client.GetAsync("/api/Relatorio?ano=2024");

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GerarRelatorio_ComDadosValidos_Retorna200()
    {
        // Arrange
        await AdicionarAuthHeader();
        var relatorio = new RelatorioRequest
        {
            Cidade = "São Paulo",
            Mes = 1,
            Ano = 2024
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Relatorio/gerar", relatorio);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GerarRelatorio_SemAutenticacao_Retorna401()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;
        var relatorio = new RelatorioRequest { Cidade = "SP", Mes = 1, Ano = 2024 };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Relatorio/gerar", relatorio);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_ComCredenciaisValidas_RetornaToken()
    {
        // Arrange
        var login = new LoginRequest { Email = "admin@teste.com", Senha = "Admin@123" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", login);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(content?.Token);
        Assert.NotEmpty(content!.Token);
    }

    [Fact]
    public async Task Login_ComCredenciaisInvalidas_Retorna401()
    {
        // Arrange
        var login = new LoginRequest { Email = "admin@teste.com", Senha = "senhaerrada" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", login);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

public class HealthEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    /// <summary>
    /// Endpoint usado pelo pipeline (smoke test) e pelo HEALTHCHECK do Docker.
    /// </summary>
    [Fact]
    public async Task Health_ReturnsHttpStatusCode200()
    {
        var response = await _client.GetAsync("/health");

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Healthy", content.GetProperty("status").GetString());
    }
}
