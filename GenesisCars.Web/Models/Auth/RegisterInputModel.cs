using System.ComponentModel.DataAnnotations;

namespace GenesisCars.Web.Models.Auth;

public class RegisterInputModel
{
  [Required]
  [StringLength(100)]
  [Display(Name = "First Name")]
  public string FirstName { get; set; } = string.Empty;

  [Required]
  [StringLength(100)]
  [Display(Name = "Last Name")]
  public string LastName { get; set; } = string.Empty;

  [Required]
  [EmailAddress]
  public string Email { get; set; } = string.Empty;

  public string? ReturnUrl { get; set; }
}
