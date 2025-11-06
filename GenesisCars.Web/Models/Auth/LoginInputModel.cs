using System.ComponentModel.DataAnnotations;

namespace GenesisCars.Web.Models.Auth;

public class LoginInputModel
{
  [Required]
  [EmailAddress]
  public string Email { get; set; } = string.Empty;

  [Required]
  [Display(Name = "Last Name")]
  public string LastName { get; set; } = string.Empty;

  public string? ReturnUrl { get; set; }
}
