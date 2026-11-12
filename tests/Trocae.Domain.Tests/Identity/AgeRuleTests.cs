using Trocae.Domain.Identity;
using Xunit;

namespace Trocae.Domain.Tests.Identity;

public class AgeRuleTests
{
    [Fact]
    public void IsAdult_DeveSerVerdadeiroQuandoFezAniversarioHoje()
    {
        var today = new DateOnly(2026, 11, 12);
        var birthDate = new DateOnly(2008, 11, 12); // 18 anos completos hoje

        Assert.True(AgeRule.IsAdult(birthDate, today));
    }

    [Fact]
    public void IsAdult_DeveSerFalsoQuandoFaltaUmDiaParaCompletar18()
    {
        var today = new DateOnly(2026, 11, 12);
        var birthDate = new DateOnly(2008, 11, 13); // completa 18 anos amanhã

        Assert.False(AgeRule.IsAdult(birthDate, today));
    }

    [Fact]
    public void IsAdult_DeveSerVerdadeiroParaMaiorDeIdadeSobra()
    {
        var today = new DateOnly(2026, 11, 12);
        var birthDate = new DateOnly(1990, 1, 1);

        Assert.True(AgeRule.IsAdult(birthDate, today));
    }
}
