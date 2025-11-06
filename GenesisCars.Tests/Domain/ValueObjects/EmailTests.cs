using GenesisCars.Domain.Exceptions;
using GenesisCars.Domain.ValueObjects;

namespace GenesisCars.Tests.Domain.ValueObjects;

public class EmailTests
{
  [Theory]
  [InlineData("test@example.com")]
  [InlineData("USER@Example.org")]
  public void Create_WithValidValue_ReturnsEmail(string address)
  {
    var email = Email.Create(address);

    Assert.Equal(address.Trim().ToLowerInvariant(), email.Value.ToLowerInvariant());
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  [InlineData("not-an-email")]
  public void Create_WithInvalidValue_ThrowsDomainException(string address)
  {
    Assert.Throws<DomainException>(() => Email.Create(address));
  }

  [Fact]
  public void Equals_IsCaseInsensitive()
  {
    var first = Email.Create("Test@Example.com");
    var second = Email.Create("test@example.com");

    Assert.Equal(first, second);
    Assert.True(first.Equals(second));
    Assert.Equal(first.GetHashCode(), second.GetHashCode());
  }
}
