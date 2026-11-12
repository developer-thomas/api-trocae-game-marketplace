namespace Trocae.Infrastructure.Identity.CpfVerification;

/// <summary>
/// Cliente real da Consulta Simples do CPFHub.io. Só deve ser usado quando
/// existir uma chave real configurada (CpfHub:UseMock = false). Nunca usar
/// POST /cpf/realtime.
/// </summary>
public interface ICpfHubClient
{
    Task<CpfHubLookupResult> ConsultarAsync(string cpfDigits, CancellationToken cancellationToken);
}
