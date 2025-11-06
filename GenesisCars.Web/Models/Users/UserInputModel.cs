using System.ComponentModel.DataAnnotations;

namespace GenesisCars.Web.Models.Users;

public class UserInputModel
{
  [Required]
  [StringLength(100)]
  public string FirstName { get; set; } = string.Empty;

  [Required]
  [StringLength(100)]
  public string LastName { get; set; } = string.Empty;

  [Required]
  [EmailAddress]
  [StringLength(255)]
  public string Email { get; set; } = string.Empty;
}
