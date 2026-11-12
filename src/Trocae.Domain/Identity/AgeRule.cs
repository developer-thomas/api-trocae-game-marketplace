namespace Trocae.Domain.Identity;

/// <summary>
/// Regra de maioridade: a conta precisa ter 18 anos completos.
/// </summary>
public static class AgeRule
{
    public const int MinimumAge = 18;

    public static bool IsAdult(DateOnly birthDate, DateOnly today)
    {
        var age = today.Year - birthDate.Year;

        var birthdayAlreadyHappenedThisYear =
            today.Month > birthDate.Month ||
            (today.Month == birthDate.Month && today.Day >= birthDate.Day);

        if (!birthdayAlreadyHappenedThisYear)
        {
            age--;
        }

        return age >= MinimumAge;
    }
}
