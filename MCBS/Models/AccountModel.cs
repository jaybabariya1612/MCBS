using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCBS.Models
{
    public class AccountModel
    {
        [Key]
        public int AccountId { get; set; }

        [Required]
        [StringLength(100)]
        public string? AccountHolderName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(10)]
        public string? Gender { get; set; }

        [StringLength(18)]
        public string? AccountNumber { get; set; }

        [StringLength(11)]
        public string? IFSCCode { get; set; }

        [Required]
        [StringLength(100)]
        public string? BranchName { get; set; }

        [Required]
        [StringLength(20)]
        public string? AccountType { get; set; }  // Savings, Current, etc.

        [Required]
        [StringLength(15)]
        public string? MobileNumber { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(12)]
        public string? AadhaarNumber { get; set; }

        [StringLength(10)]
        public string? PANNumber { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(10)]
        public string? PostalCode { get; set; }

        [StringLength(100)]
        public string? NomineeName { get; set; }

        [StringLength(50)]
        public string? NomineeRelationship { get; set; }

        [StringLength(100)]
        public string? Occupation { get; set; }

        [StringLength(50)]
        public string? Nationality { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; } = 0.00m;

        public DateTime DateOfOpening { get; set; } = DateTime.Now;

        public DateTime? LastUpdated { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(200)]
        public string? AccountStatusReason { get; set; }

        public bool KYCStatus { get; set; } = false;
    }
}
