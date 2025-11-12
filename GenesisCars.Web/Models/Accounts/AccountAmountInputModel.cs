using System.ComponentModel.DataAnnotations;

namespace GenesisCars.Web.Models.Accounts;

public class AccountAmountInputModel
{
  [Required]
  [Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "Amount must be greater than zero.")]
  public decimal Amount { get; set; }
}
