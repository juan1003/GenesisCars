using GenesisCars.Domain.Exceptions;
using GenesisCars.Domain.ValueObjects;

namespace GenesisCars.Domain.Entities;

public class User
{
  private User()
  {
  }

  private User(Guid id, string firstName, string lastName, Email email)
  {
    Id = id;
    UpdateNames(firstName, lastName);
    Email = email;
    CreatedAtUtc = DateTime.UtcNow;
    UpdatedAtUtc = CreatedAtUtc;
  }

  public Guid Id { get; private set; }

  public string FirstName { get; private set; } = string.Empty;

  public string LastName { get; private set; } = string.Empty;

  public Email Email { get; private set; } = null!;

  public DateTime CreatedAtUtc { get; private set; }

  public DateTime UpdatedAtUtc { get; private set; }

  public static User Create(string firstName, string lastName, Email email)
  {
    return new User(Guid.NewGuid(), firstName, lastName, email);
  }

  public void Update(string firstName, string lastName, Email email)
  {
    UpdateNames(firstName, lastName);
    Email = email ?? throw new DomainException("Email address is required.");
    UpdatedAtUtc = DateTime.UtcNow;
  }

  private void UpdateNames(string firstName, string lastName)
  {
    if (string.IsNullOrWhiteSpace(firstName))
    {
      throw new DomainException("First name is required.");
    }

    if (string.IsNullOrWhiteSpace(lastName))
    {
      throw new DomainException("Last name is required.");
    }

    FirstName = firstName.Trim();
    LastName = lastName.Trim();
  }
}
