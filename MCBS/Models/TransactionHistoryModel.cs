using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCBS.Models
{
    public class TransactionHistoryModel
    {
        public int TransactionId { get; set; }

        public int AccountId { get; set; }
        public string? AccountNumber { get; set; }
        public string? AccountHolderName { get; set; }  // logged-in user (owner of this account)

        public string? CounterpartyAccountNumber { get; set; }
        public string? CounterpartyAccountHolderName { get; set; }

        public string? TransactionType { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? Description { get; set; }
    }


}
