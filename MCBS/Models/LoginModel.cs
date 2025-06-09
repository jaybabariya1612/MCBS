using System.ComponentModel.DataAnnotations;

namespace MCBS.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Account Number is required")]
        public string? AccountNumber { get; set; }

        [Required(ErrorMessage = "Date of Birth is required")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }
    }
}
