using MCBS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace MCBS.Controllers
{
    public class CustomerController : Controller
    {
        private readonly IConfiguration _configuration;

        public CustomerController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public IActionResult AccountDetail(int id)
        {
            AccountModel account = new AccountModel();
            string connectionString = _configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM IndianBankAccount WHERE AccountId = @id";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    account.AccountId = Convert.ToInt32(reader["AccountId"]);
                    account.AccountHolderName = reader["AccountHolderName"].ToString();
                    account.DateOfBirth = reader["DateOfBirth"] as DateTime?;
                    account.Gender = reader["Gender"].ToString();
                    account.AccountNumber = reader["AccountNumber"].ToString();
                    account.IFSCCode = reader["IFSCCode"].ToString();
                    account.BranchName = reader["BranchName"].ToString();
                    account.AccountType = reader["AccountType"].ToString();
                    account.MobileNumber = reader["MobileNumber"].ToString();
                    account.Email = reader["Email"].ToString();
                    account.AadhaarNumber = reader["AadhaarNumber"].ToString();
                    account.PANNumber = reader["PANNumber"].ToString();
                    account.Address = reader["Address"].ToString();
                    account.City = reader["City"].ToString();
                    account.State = reader["State"].ToString();
                    account.PostalCode = reader["PostalCode"].ToString();
                    account.NomineeName = reader["NomineeName"].ToString();
                    account.NomineeRelationship = reader["NomineeRelationship"].ToString();
                    account.Occupation = reader["Occupation"].ToString();
                    account.Nationality = reader["Nationality"].ToString();
                    account.Balance = Convert.ToDecimal(reader["Balance"]);
                    account.DateOfOpening = Convert.ToDateTime(reader["DateOfOpening"]);
                    account.LastUpdated = reader["LastUpdated"] as DateTime?;
                    account.IsActive = Convert.ToBoolean(reader["IsActive"]);
                    account.AccountStatusReason = reader["AccountStatusReason"].ToString();
                    account.KYCStatus = Convert.ToBoolean(reader["KYCStatus"]);
                }
                reader.Close();
            }

            return View(account);
        }


    }
}
