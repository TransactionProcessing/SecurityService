using SecurityService.BusinessLogic.Mfa;
using Shouldly;

namespace SecurityService.UnitTests.Mfa;

public sealed class RecoveryCodeGeneratorTests
{
    [Fact]
    public void Generate_returns_unique_codes_with_expected_shape()
    {
        var generator = new RecoveryCodeGenerator();

        var codes = generator.Generate(10);

        codes.Count.ShouldBe(10);
        codes.Distinct(StringComparer.Ordinal).Count().ShouldBe(10);
        codes.ShouldAllBe(code => code.Length == 16 && code.All(char.IsLetterOrDigit));
    }
}
