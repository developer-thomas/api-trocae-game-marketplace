namespace Trocae.Domain.Identity;

/// <summary>
/// Value object de CPF. Só existe uma instância válida depois de passar por
/// validação de formato e dígitos verificadores, sem qualquer chamada externa.
/// Nunca deve ser logado nem serializado para fora do backend em texto puro.
/// </summary>
public sealed class Cpf
{
    public string Digits { get; }

    private Cpf(string digits)
    {
        Digits = digits;
    }

    public static bool TryParse(string? input, out Cpf? cpf, out string? error)
    {
        cpf = null;

        if (string.IsNullOrWhiteSpace(input))
        {
            error = "CPF é obrigatório.";
            return false;
        }

        var digitsOnly = new string(input.Where(char.IsDigit).ToArray());

        if (digitsOnly.Length != 11)
        {
            error = "CPF deve ter 11 dígitos.";
            return false;
        }

        if (AllDigitsEqual(digitsOnly))
        {
            error = "CPF inválido.";
            return false;
        }

        if (!HasValidCheckDigits(digitsOnly))
        {
            error = "CPF inválido.";
            return false;
        }

        error = null;
        cpf = new Cpf(digitsOnly);
        return true;
    }

    private static bool AllDigitsEqual(string digits) => digits.Distinct().Count() == 1;

    private static bool HasValidCheckDigits(string digits)
    {
        var nums = digits.Select(c => c - '0').ToArray();

        var firstCheckDigit = CalculateCheckDigit(nums, 9);
        if (firstCheckDigit != nums[9])
        {
            return false;
        }

        var secondCheckDigit = CalculateCheckDigit(nums, 10);
        return secondCheckDigit == nums[10];
    }

    private static int CalculateCheckDigit(int[] digits, int count)
    {
        var weight = count + 1;
        var sum = 0;

        for (var i = 0; i < count; i++)
        {
            sum += digits[i] * weight;
            weight--;
        }

        var remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }

    public override string ToString() => "***.***.***-**";
}
