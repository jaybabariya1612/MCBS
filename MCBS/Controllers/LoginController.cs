using MCBS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace MCBS.Controllers
{
    public class LoginController : Controller
    {
        private readonly IConfiguration _configuration;

        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login(LoginModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string connectionString = _configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

            string query = @"
        SELECT AccountId, AccountHolderName, Email, MobileNumber 
        FROM IndianBankAccount 
        WHERE AccountNumber = @AccountNumber AND CAST(DateOfBirth AS DATE) = @DateOfBirth";

            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@AccountNumber", model.AccountNumber ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@DateOfBirth", model.DateOfBirth?.Date ?? (object)DBNull.Value);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    string name = reader["AccountHolderName"]?.ToString() ?? "";
                    string email = reader["Email"]?.ToString() ?? "";
                    string accountId = reader["AccountId"]?.ToString() ?? "";

                    HttpContext.Session.SetString("UserName", name);
                    HttpContext.Session.SetString("Email", email);
                    HttpContext.Session.SetString("AccountId", accountId);

                    TempData["LoginMessage"] = "Login successful!";
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.Message = "Invalid Account Number or Date of Birth";
                    return View(model);
                }
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Check if email or mobile number already exists
                    string checkQuery = @"
                SELECT COUNT(*) 
                FROM tbl_Register 
                WHERE EmailID = @EmailID OR MobileNo = @MobileNo";

                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@EmailID", model.EmailID);
                        checkCmd.Parameters.AddWithValue("@MobileNo", model.MobileNo);
                        int count = (int)checkCmd.ExecuteScalar();

                        if (count > 0)
                        {
                            // Check specifically which field is duplicated
                            string detailQuery = @"
                        SELECT EmailID, MobileNo 
                        FROM tbl_Register 
                        WHERE EmailID = @EmailID OR MobileNo = @MobileNo";

                            using (SqlCommand detailCmd = new SqlCommand(detailQuery, conn))
                            {
                                detailCmd.Parameters.AddWithValue("@EmailID", model.EmailID);
                                detailCmd.Parameters.AddWithValue("@MobileNo", model.MobileNo);
                                using (SqlDataReader reader = detailCmd.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        string? existingEmail = reader["EmailID"].ToString();
                                        string? existingMobile = reader["MobileNo"].ToString();

                                        if (existingEmail == model.EmailID)
                                            ModelState.AddModelError("EmailID", "This email is already registered.");

                                        if (existingMobile == model.MobileNo)
                                            ModelState.AddModelError("MobileNo", "This mobile number is already registered.");
                                    }
                                }
                            }

                            return View(model);
                        }
                    }

                    // Insert new user
                    using (SqlCommand cmd = new SqlCommand("sp_InsertRegister", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@FirstName", model.FirstName);
                        cmd.Parameters.AddWithValue("@LastName", model.LastName);
                        cmd.Parameters.AddWithValue("@EmailID", model.EmailID);
                        cmd.Parameters.AddWithValue("@Password", model.Password);
                        cmd.Parameters.AddWithValue("@MobileNo", model.MobileNo);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        TempData["SuccessMessage"] = "Account created successfully! Please log in.";
                        return RedirectToAction("Login", "Login");
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while registering: " + ex.Message);
            }

            return View(model);
        }



        public IActionResult Logout()
        {
            // Clear session data
            HttpContext.Session.Clear();
            TempData["LogoutMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Login", "Login");
        }
    }
}
