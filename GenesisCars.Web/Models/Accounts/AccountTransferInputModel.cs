using System.ComponentModel.DataAnnotations;

namespace GenesisCars.Web.Models.Accounts;

public class AccountTransferInputModel
{
  [Required]
  [Display(Name = "Recipient account")]
  public Guid RecipientAccountId { get; set; }

  [Required]
  [Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "Amount must be greater than zero.")]
  public decimal Amount { get; set; }
}
