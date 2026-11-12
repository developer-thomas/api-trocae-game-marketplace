using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Trocae.Infrastructure.Persistence;

namespace Trocae.Infrastructure.Identity.CpfVerification;

/// <summary>
/// Implementação única usada em todos os ambientes. Em desenvolvimento
/// (CpfHub:UseMock = true, padrão), simula uma resposta "encontrado e
/// compatível" sem nenhuma chamada de rede — CPFHub fica simulado, como
/// decidido. Em produção, chama <see cref="ICpfHubClient"/> de verdade.
/// </summary>
public sealed class CpfVerificationService : ICpfVerificationService
{
    private const int MaxTransientRetries = 2;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromMilliseconds(500);

    private readonly TrocaeDbContext _db;
    private readonly ICpfHubClient _client;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CpfVerificationService> _logger;

    public CpfVerificationService(
        TrocaeDbContext db,
        ICpfHubClient client,
        IConfiguration configuration,
        ILogger<CpfVerificationService> logger)
    {
        _db = db;
        _client = client;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<CpfVerificationOutcome> VerifyAsync(
        string cpfDigits,
        string cpfHash,
        string fullName,
        DateOnly birthDate,
        CancellationToken cancellationToken)
    {
        var cached = await _db.CpfLookupCaches
            .SingleOrDefaultAsync(x => x.CpfHash == cpfHash, cancellationToken);

        if (cached is not null)
        {
            return ToOutcome(cached.Found, cached.NameMatched, cached.BirthDateMatched);
        }

        var useMock = _configuration.GetValue("CpfHub:UseMock", true);

        var lookup = useMock
            ? SimulateMockLookup(fullName, birthDate)
            : await ConsultarComRetryAsync(cpfDigits, cancellationToken);

        var found = lookup.Status == CpfHubLookupStatus.Found;
        var nameMatched = found && NormalizeName(fullName) == NormalizeName(lookup.NameUpper ?? string.Empty);
        var birthDateMatched = found && lookup.BirthDate == birthDate;

        // Grava o resultado ANTES de responder: um reenvio não deve gerar
        // outra cobrança no provedor, mesmo que o usuário corrija dados depois.
        _db.CpfLookupCaches.Add(new CpfLookupCache
        {
            CpfHash = cpfHash,
            Found = found,
            NameMatched = nameMatched,
            BirthDateMatched = birthDateMatched,
            CheckedAtUtc = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(cancellationToken);

        return ToOutcome(found, nameMatched, birthDateMatched);
    }

    private async Task<CpfHubLookupResult> ConsultarComRetryAsync(string cpfDigits, CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= MaxTransientRetries; attempt++)
        {
            var result = await _client.ConsultarAsync(cpfDigits, cancellationToken);

            if (result.Status != CpfHubLookupStatus.TransientError)
            {
                return result;
            }

            _logger.LogWarning("CPFHub instável (tentativa {Attempt}/{Max}).", attempt, MaxTransientRetries);

            if (attempt < MaxTransientRetries)
            {
                await Task.Delay(RetryDelay, cancellationToken);
            }
        }

        throw new CpfProviderUnavailableException();
    }

    private static CpfHubLookupResult SimulateMockLookup(string fullName, DateOnly birthDate) =>
        new(CpfHubLookupStatus.Found, NormalizeName(fullName), birthDate);

    private static string NormalizeName(string name)
    {
        var formD = name.Normalize(NormalizationForm.FormD);
        var withoutDiacritics = new string(formD
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .ToArray());

        return withoutDiacritics
            .Normalize(NormalizationForm.FormC)
            .Trim()
            .ToUpperInvariant();
    }

    private static CpfVerificationOutcome ToOutcome(bool found, bool nameMatched, bool birthDateMatched)
    {
        if (!found)
        {
            return CpfVerificationOutcome.NotFound;
        }

        return nameMatched && birthDateMatched
            ? CpfVerificationOutcome.Verified
            : CpfVerificationOutcome.Mismatch;
    }
}
