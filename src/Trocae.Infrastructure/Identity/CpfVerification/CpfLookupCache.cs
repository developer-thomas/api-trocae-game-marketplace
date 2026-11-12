namespace Trocae.Infrastructure.Identity.CpfVerification;

/// <summary>
/// Resultado já consultado de um CPF no CPFHub, indexado pelo hash (nunca o
/// CPF em texto puro). Cada CPF só deve ser consultado uma vez: antes de
/// chamar o provedor de novo, esta tabela precisa ser checada.
/// Guarda só os booleanos de comparação — nunca o nome/data oficiais
/// retornados pelo provedor, nem o campo gender.
/// </summary>
public sealed class CpfLookupCache
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string CpfHash { get; set; } = string.Empty;

    /// <summary>true = CPFHub encontrou registro (custou 1 crédito); false = 404 (grátis).</summary>
    public bool Found { get; set; }

    public bool NameMatched { get; set; }

    public bool BirthDateMatched { get; set; }

    public DateTime CheckedAtUtc { get; set; } = DateTime.UtcNow;
}
