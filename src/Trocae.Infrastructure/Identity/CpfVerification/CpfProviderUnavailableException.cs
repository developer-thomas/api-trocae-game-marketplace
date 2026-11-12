namespace Trocae.Infrastructure.Identity.CpfVerification;

/// <summary>
/// O CPFHub respondeu 500/503 repetidamente e o resultado ainda não foi
/// gravado. O chamador (controller) deve devolver 503 ao cliente, sem
/// inventar um novo retry automático além do já feito aqui dentro.
/// </summary>
public sealed class CpfProviderUnavailableException : Exception
{
    public CpfProviderUnavailableException()
        : base("Serviço de verificação de CPF indisponível no momento.")
    {
    }
}
