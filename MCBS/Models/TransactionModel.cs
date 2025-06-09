using System.ComponentModel.DataAnnotations;

namespace MCBS.Models
{
    public class TransactionModel
    {
        [Required]
        [Display(Name = "Receiver Account No. or Mobile No.")]
        public string? ReceiverIdentifier { get; set; }

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth (as PIN)")]
        public DateTime DobPin { get; set; }
    }
}
