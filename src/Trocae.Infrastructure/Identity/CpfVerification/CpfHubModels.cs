namespace Trocae.Infrastructure.Identity.CpfVerification;

public enum CpfHubLookupStatus
{
    Found,
    NotFound,
    TransientError
}

/// <summary>
/// Leitura normalizada da resposta do CPFHub. O campo <c>gender</c> da API
/// é propositalmente descartado e nunca chega a este tipo.
/// </summary>
public sealed record CpfHubLookupResult(
    CpfHubLookupStatus Status,
    string? NameUpper,
    DateOnly? BirthDate);
