using System.ComponentModel.DataAnnotations;

namespace GenesisCars.Web.Models.Accounts;

public class AccountInputModel
{
  [Required]
  [StringLength(200)]
  [Display(Name = "Owner name")]
  public string OwnerName { get; set; } = string.Empty;

  [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "Initial balance cannot be negative.")]
  [Display(Name = "Initial balance")]
  public decimal InitialBalance { get; set; }
}
