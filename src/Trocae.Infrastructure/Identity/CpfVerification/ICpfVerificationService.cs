namespace Trocae.Infrastructure.Identity.CpfVerification;

public enum CpfVerificationOutcome
{
    Verified,
    NotFound,
    Mismatch
}

/// <summary>
/// Orquestra a verificação de CPF: cache por hash, no máximo uma consulta
/// real por CPF, e mock em desenvolvimento. Nunca recebe nem devolve o CPF
/// em texto puro fora deste módulo.
/// </summary>
public interface ICpfVerificationService
{
    Task<CpfVerificationOutcome> VerifyAsync(
        string cpfDigits,
        string cpfHash,
        string fullName,
        DateOnly birthDate,
        CancellationToken cancellationToken);
}
