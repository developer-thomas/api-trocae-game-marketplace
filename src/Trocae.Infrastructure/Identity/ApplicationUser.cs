using Microsoft.AspNetCore.Identity;

namespace Trocae.Infrastructure.Identity;

/// <summary>
/// Conta da plataforma (ASP.NET Core Identity). Nunca armazena o CPF em texto
/// puro — só o hash HMAC-SHA256 com pepper de servidor (<see cref="CpfHash"/>).
/// Dados sensíveis (CPF, nome completo, data de nascimento, telefone, e-mail)
/// nunca devem ser expostos em perfis públicos.
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;

    public DateOnly BirthDate { get; set; }

    /// <summary>HMAC-SHA256(CPF, pepper) em hexadecimal — 64 caracteres.</summary>
    public string CpfHash { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
