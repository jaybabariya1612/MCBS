using System;
using System.ComponentModel.DataAnnotations;

namespace MCBS.Models
{
    public class ContactUsModel
    {
        public int ContactId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string? FullName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required]
        [Phone]
        [StringLength(15)]
        [Display(Name = "Mobile Number")]
        public string? MobileNumber { get; set; }

        [StringLength(200)]
        public string? Subject { get; set; }

        [Required]
        [Display(Name = "Message")]
        public string? Message { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.Now;
    }
}
