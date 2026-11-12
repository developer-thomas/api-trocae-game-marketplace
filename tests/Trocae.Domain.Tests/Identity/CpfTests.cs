using Trocae.Domain.Identity;
using Xunit;

namespace Trocae.Domain.Tests.Identity;

public class CpfTests
{
    [Theory]
    [InlineData("111.444.777-35")]
    [InlineData("11144477735")]
    public void TryParse_DeveAceitarCpfValido(string input)
    {
        var ok = Cpf.TryParse(input, out var cpf, out var error);

        Assert.True(ok);
        Assert.NotNull(cpf);
        Assert.Equal("11144477735", cpf!.Digits);
        Assert.Null(error);
    }

    [Theory]
    [InlineData("11144477736")] // dígito verificador errado
    [InlineData("11111111111")] // todos os dígitos iguais
    [InlineData("123456789")]   // tamanho errado
    [InlineData("")]
    [InlineData(null)]
    public void TryParse_DeveRejeitarCpfInvalido(string? input)
    {
        var ok = Cpf.TryParse(input, out var cpf, out var error);

        Assert.False(ok);
        Assert.Null(cpf);
        Assert.NotNull(error);
    }

    [Fact]
    public void ToString_NuncaExpoeOCpfEmTextoPuro()
    {
        Cpf.TryParse("11144477735", out var cpf, out _);

        Assert.DoesNotContain("1", cpf!.ToString());
    }
}
