using GenesisCars.Domain.Entities;
using GenesisCars.Domain.Exceptions;
using GenesisCars.Domain.ValueObjects;

namespace GenesisCars.Tests.Domain.Entities;

public class UserTests
{
  [Fact]
  public void Create_WithValidData_Succeeds()
  {
    var email = Email.Create("user@example.com");
    var user = User.Create("Jane", "Doe", email);

    Assert.Equal("Jane", user.FirstName);
    Assert.Equal("Doe", user.LastName);
    Assert.Equal(email, user.Email);
    Assert.NotEqual(default, user.Id);
  }

  [Theory]
  [InlineData("")]
  [InlineData(" ")]
  public void Create_WithInvalidFirstName_ThrowsDomainException(string firstName)
  {
    var email = Email.Create("user@example.com");
    Assert.Throws<DomainException>(() => User.Create(firstName, "Doe", email));
  }

  [Theory]
  [InlineData("")]
  [InlineData(" ")]
  public void Create_WithInvalidLastName_ThrowsDomainException(string lastName)
  {
    var email = Email.Create("user@example.com");
    Assert.Throws<DomainException>(() => User.Create("Jane", lastName, email));
  }

  [Fact]
  public void Update_WithNewValues_UpdatesProperties()
  {
    var user = User.Create("Jane", "Doe", Email.Create("user@example.com"));
    var newEmail = Email.Create("updated@example.com");

    user.Update("Janet", "Smith", newEmail);

    Assert.Equal("Janet", user.FirstName);
    Assert.Equal("Smith", user.LastName);
    Assert.Equal(newEmail, user.Email);
  }
}
