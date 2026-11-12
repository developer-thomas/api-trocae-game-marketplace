using System.Security.Cryptography;
using System.Text;

namespace Trocae.Infrastructure.Identity;

/// <summary>
/// Gera o identificador único e não reversível do CPF, usado para checar
/// duplicidade de conta e como chave do cache de consulta ao CPFHub.
/// Perder o pepper significa perder a capacidade de localizar contas já
/// gravadas por CPF — ele deve vir de variável de ambiente em produção.
/// </summary>
public static class CpfHasher
{
    public static string Hash(string cpfDigits, string pepper)
    {
        if (string.IsNullOrEmpty(pepper))
        {
            throw new InvalidOperationException("Cpf:Pepper não configurado.");
        }

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(pepper));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(cpfDigits));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
