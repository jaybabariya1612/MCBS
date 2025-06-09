using System.ComponentModel.DataAnnotations;

namespace MCBS.Models
{
    public class TransferRequestModel
    {
        [Required]
        public string? RecipientAccountNumber { get; set; }

        [Required]
        public string? RecipientMobileNumber { get; set; }

        [Required]
        public decimal Amount { get; set; }
    }
}
