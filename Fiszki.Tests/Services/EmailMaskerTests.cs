using FluentAssertions;
using Fiszki.Services.Services;
using Xunit;

namespace Fiszki.Tests.Services;

public class EmailMaskerTests
{
    [Theory]
    [InlineData("jan.kowalski@example.com", "ja***@example.com")]
    [InlineData("abc@x.pl", "ab***@x.pl")]
    public void Mask_KeepsFirstTwoCharactersAndDomain(string email, string expected)
    {
        EmailMasker.Mask(email).Should().Be(expected);
    }

    [Fact]
    public void Mask_TwoCharacterLocalPart_KeepsOnlyOneCharacter()
    {
        EmailMasker.Mask("ab@x.pl").Should().Be("a***@x.pl");
    }

    [Fact]
    public void Mask_OneCharacterLocalPart_ShowsNoLocalCharacters()
    {
        EmailMasker.Mask("a@x.pl").Should().Be("***@x.pl");
    }

    [Theory]
    [InlineData("")]
    public void Mask_EmptyInput_ReturnsMaskOnly(string email)
    {
        EmailMasker.Mask(email).Should().Be("***");
    }

    [Fact]
    public void Mask_NeverContainsFullLocalPart()
    {
        EmailMasker.Mask("jan.kowalski@example.com").Should().NotContain("kowalski");
    }
}
