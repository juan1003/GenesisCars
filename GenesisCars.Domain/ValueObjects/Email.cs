using System.Net.Mail;
using GenesisCars.Domain.Exceptions;

namespace GenesisCars.Domain.ValueObjects;

public sealed class Email : IEquatable<Email>
{
  private Email(string value)
  {
    Value = value;
  }

  public string Value { get; }

  public static Email Create(string value)
  {
    if (string.IsNullOrWhiteSpace(value))
    {
      throw new DomainException("Email address is required.");
    }

    try
    {
      var address = new MailAddress(value.Trim());
      return new Email(address.Address);
    }
    catch (FormatException)
    {
      throw new DomainException("Email address is not valid.");
    }
  }

  public bool Equals(Email? other)
  {
    if (ReferenceEquals(null, other))
    {
      return false;
    }

    if (ReferenceEquals(this, other))
    {
      return true;
    }

    return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
  }

  public override bool Equals(object? obj) => Equals(obj as Email);

  public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Value);

  public override string ToString() => Value;

  public static implicit operator string(Email email) => email.Value;
}
