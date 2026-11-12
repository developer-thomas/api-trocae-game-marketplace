using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Trocae.Infrastructure.Identity.CpfVerification;

/// <summary>
/// Implementação real do cliente do CPFHub.io — Consulta Simples.
/// GET https://api.cpfhub.io/cpf/{11 dígitos}, header x-api-key.
/// Não é chamada em desenvolvimento (CpfHub:UseMock = true por padrão).
/// Não repete automaticamente em timeout: quem decide retry (só em 500/503,
/// com espera, e só se o resultado ainda não foi gravado) é
/// <see cref="CpfVerificationService"/>.
/// </summary>
public sealed class CpfHubClient : ICpfHubClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CpfHubClient> _logger;

    public CpfHubClient(HttpClient httpClient, IConfiguration configuration, ILogger<CpfHubClient> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<CpfHubLookupResult> ConsultarAsync(string cpfDigits, CancellationToken cancellationToken)
    {
        var baseUrl = _configuration["CpfHub:BaseUrl"] ?? "https://api.cpfhub.io";
        var apiKey = _configuration["CpfHub:ApiKey"];

        using var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/cpf/{cpfDigits}");
        request.Headers.Add("x-api-key", apiKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            // 404 é gratuito — deve ser cacheado pelo chamador, não repetir.
            return new CpfHubLookupResult(CpfHubLookupStatus.NotFound, null, null);
        }

        if (response.StatusCode is HttpStatusCode.InternalServerError or HttpStatusCode.ServiceUnavailable)
        {
            _logger.LogWarning("CPFHub retornou {StatusCode}; elegível a retry com espera.", response.StatusCode);
            return new CpfHubLookupResult(CpfHubLookupStatus.TransientError, null, null);
        }

        response.EnsureSuccessStatusCode();

        // Formato provisório — ajustar quando a documentação completa da
        // Consulta Simples estiver disponível. "gender" é ignorado de propósito.
        var payload = await response.Content.ReadFromJsonAsync<CpfHubResponseDto>(cancellationToken: cancellationToken);
        if (payload is null)
        {
            return new CpfHubLookupResult(CpfHubLookupStatus.NotFound, null, null);
        }

        DateOnly? birthDate = payload.Ano is > 0 && payload.Mes is > 0 and <= 12 && payload.Dia is > 0 and <= 31
            ? new DateOnly(payload.Ano.Value, payload.Mes.Value, payload.Dia.Value)
            : null;

        return new CpfHubLookupResult(CpfHubLookupStatus.Found, payload.NameUpper, birthDate);
    }

    private sealed class CpfHubResponseDto
    {
        public string? NameUpper { get; set; }
        public int? Dia { get; set; }
        public int? Mes { get; set; }
        public int? Ano { get; set; }
        // "gender" existe na resposta real, mas é propositalmente omitido aqui.
    }
}
