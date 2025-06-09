using MCBS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace MCBS.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;

        public AccountController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public IActionResult Account()
        {
            // Check if session value "UserName" is null or empty
            string? userName = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Login", "Login");
            }
            return View();
        }

        private string GenerateRandomIFSC(string bankPrefix = "MCBS", int randomDigitsCount = 6)
        {
            var random = new Random();

            // 4 letter prefix (can customize)
            string prefix = bankPrefix.ToUpper();

            // Usually 1 char (often 0) after prefix - you can fix it or randomize
            char fifthChar = '0';

            // Generate random digits part
            string digits = "";
            for (int i = 0; i < randomDigitsCount; i++)
            {
                digits += random.Next(0, 10).ToString();
            }

            return prefix + fifthChar + digits; // e.g. BANK0123456
        }



        [HttpPost]
        public IActionResult Account(AccountModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string connectionString = _configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

            string generatedIFSC = GenerateRandomIFSC("BANK", 6);
            model.IFSCCode = generatedIFSC;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("sp_InsertIndianBankAccount_AutoAccountNumber", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@AccountHolderName", model.AccountHolderName);
                        cmd.Parameters.AddWithValue("@DateOfBirth", (object?)model.DateOfBirth ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Gender", (object?)model.Gender ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@IFSCCode", generatedIFSC);
                        cmd.Parameters.AddWithValue("@BranchName", model.BranchName);
                        cmd.Parameters.AddWithValue("@AccountType", model.AccountType);
                        cmd.Parameters.AddWithValue("@MobileNumber", model.MobileNumber);
                        cmd.Parameters.AddWithValue("@Email", (object?)model.Email ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@AadhaarNumber", (object?)model.AadhaarNumber ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@PANNumber", (object?)model.PANNumber ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Address", (object?)model.Address ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@City", (object?)model.City ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@State", (object?)model.State ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@PostalCode", (object?)model.PostalCode ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@NomineeName", (object?)model.NomineeName ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@NomineeRelationship", (object?)model.NomineeRelationship ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Occupation", (object?)model.Occupation ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Nationality", (object?)model.Nationality ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Balance", model.Balance);
                        cmd.Parameters.AddWithValue("@DateOfOpening", (object?)model.DateOfOpening ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@LastUpdated", (object?)model.LastUpdated ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@IsActive", model.IsActive);
                        cmd.Parameters.AddWithValue("@AccountStatusReason", (object?)model.AccountStatusReason ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@KYCStatus", model.KYCStatus);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                if (reader.FieldCount == 1 && reader["ErrorCode"] != null)
                                {
                                    string errorCode = reader["ErrorCode"].ToString()!;
                                    switch (errorCode)
                                    {
                                        case "MobileExists":
                                            ModelState.AddModelError("MobileNumber", "Mobile number already exists.");
                                            break;
                                        case "PANExists":
                                            ModelState.AddModelError("PANNumber", "PAN number already exists.");
                                            break;
                                        case "AadhaarExists":
                                            ModelState.AddModelError("AadhaarNumber", "Aadhaar number already exists.");
                                            break;
                                        default:
                                            ModelState.AddModelError("", "Unknown error occurred.");
                                            break;
                                    }
                                }
                                else
                                {
                                    int newAccountId = Convert.ToInt32(reader["NewAccountId"]);
                                    string? generatedAccountNumber = reader["GeneratedAccountNumber"] as string;

                                    TempData["SuccessMessage"] = $"Account created successfully! Your Account Number is {generatedAccountNumber ?? "N/A"} and IFSC Code is {generatedIFSC}.";
                                    return RedirectToAction("Account", "Account");
                                }
                            }
                            else
                            {
                                ModelState.AddModelError("", "Failed to create account. Please try again.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred: " + ex.Message);
            }

            return View(model);
        }



        public IActionResult AccountList()
        {
            List<AccountModel> accounts = new List<AccountModel>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM IndianBankAccount";
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    accounts.Add(new AccountModel
                    {
                        AccountId = Convert.ToInt32(reader["AccountId"]),
                        AccountHolderName = reader["AccountHolderName"].ToString(),
                        AccountNumber = reader["AccountNumber"].ToString(),
                        BranchName = reader["BranchName"].ToString(),
                        AccountType = reader["AccountType"].ToString(),
                        MobileNumber = reader["MobileNumber"].ToString(),
                        Balance = Convert.ToDecimal(reader["Balance"]),
                        IsActive = Convert.ToBoolean(reader["IsActive"]),
                        DateOfOpening = Convert.ToDateTime(reader["DateOfOpening"])
                    });
                }
                reader.Close();
            }

            return View(accounts);
        }

        public IActionResult AccountDetails(int id)
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

        // GET: Account/Edit/5
        public IActionResult AccountEdit(int id)
        {
            AccountModel account = new AccountModel();
            string connectionString = _configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM IndianBankAccount WHERE AccountId = @AccountId";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@AccountId", id);
                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();

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

        // POST: Account/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AccountEdit(AccountModel model)
        {
            if (ModelState.IsValid)
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = @"UPDATE IndianBankAccount SET
                            AccountHolderName = @AccountHolderName,
                            DateOfBirth = @DateOfBirth,
                            Gender = @Gender,
                            IFSCCode = @IFSCCode,
                            BranchName = @BranchName,
                            AccountType = @AccountType,
                            MobileNumber = @MobileNumber,
                            Email = @Email,
                            AadhaarNumber = @AadhaarNumber,
                            PANNumber = @PANNumber,
                            Address = @Address,
                            City = @City,
                            State = @State,
                            PostalCode = @PostalCode,
                            NomineeName = @NomineeName,
                            NomineeRelationship = @NomineeRelationship,
                            Occupation = @Occupation,
                            Nationality = @Nationality,
                            Balance = @Balance,
                            LastUpdated = GETDATE(),
                            IsActive = @IsActive,
                            AccountStatusReason = @AccountStatusReason,
                            KYCStatus = @KYCStatus
                            WHERE AccountId = @AccountId";

                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@AccountId", model.AccountId);
                    cmd.Parameters.AddWithValue("@AccountHolderName", model.AccountHolderName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@DateOfBirth", model.DateOfBirth ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Gender", model.Gender ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@IFSCCode", model.IFSCCode ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@BranchName", model.BranchName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@AccountType", model.AccountType ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@MobileNumber", model.MobileNumber ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", model.Email ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@AadhaarNumber", model.AadhaarNumber ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@PANNumber", model.PANNumber ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Address", model.Address ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@City", model.City ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@State", model.State ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@PostalCode", model.PostalCode ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@NomineeName", model.NomineeName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@NomineeRelationship", model.NomineeRelationship ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Occupation", model.Occupation ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Nationality", model.Nationality ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Balance", model.Balance);
                    cmd.Parameters.AddWithValue("@IsActive", model.IsActive);
                    cmd.Parameters.AddWithValue("@AccountStatusReason", model.AccountStatusReason ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@KYCStatus", model.KYCStatus);

                    connection.Open();
                    cmd.ExecuteNonQuery();
                }

                return RedirectToAction("AccountList", "Account");
            }

            return View(model);
        }


    }
}
