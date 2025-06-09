using MCBS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Diagnostics;

namespace MCBS.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult About()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ContactUs()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ContactUs(ContactUsModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Check if session value "UserName" is null or empty
            string? userName = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("Login", "Login");
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"INSERT INTO BankContactUs (FullName, Email, MobileNumber, Subject, Message)
                                 VALUES (@FullName, @Email, @MobileNumber, @Subject, @Message)";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@FullName", model.FullName);
                cmd.Parameters.AddWithValue("@Email", model.Email);
                cmd.Parameters.AddWithValue("@MobileNumber", model.MobileNumber);
                cmd.Parameters.AddWithValue("@Subject", (object?)model.Subject ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Message", model.Message);

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();

                TempData["SuccessMessage"] = "Thank you for contacting us! We will get back to you shortly.";

                return RedirectToAction("ContactUs");
            }
        }

        [HttpGet]
        public IActionResult ContactList()
        {
            var contacts = new List<ContactUsModel>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM BankContactUs ORDER BY CreatedAt DESC";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        contacts.Add(new ContactUsModel
                        {
                            ContactId = Convert.ToInt32(reader["ContactId"]),
                            FullName = reader["FullName"].ToString(),
                            Email = reader["Email"].ToString(),
                            MobileNumber = reader["MobileNumber"].ToString(),
                            Subject = reader["Subject"].ToString(),
                            Message = reader["Message"].ToString()
                        });
                    }
                }
            }

            return View(contacts);
        }

        [HttpGet]
        public IActionResult EditContact(int id)
        {
            ContactUsModel model = new ContactUsModel();
            string connectionString = _configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM BankContactUs WHERE ContactId = @ContactId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ContactId", id);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        model.ContactId = Convert.ToInt32(reader["ContactId"]);
                        model.FullName = reader["FullName"].ToString();
                        model.Email = reader["Email"].ToString();
                        model.MobileNumber = reader["MobileNumber"].ToString();
                        model.Subject = reader["Subject"].ToString();
                        model.Message = reader["Message"].ToString();
                    }
                }
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult EditContact(ContactUsModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string connectionString = _configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"UPDATE BankContactUs 
                             SET FullName = @FullName, Email = @Email, MobileNumber = @MobileNumber, 
                                 Subject = @Subject, Message = @Message
                             WHERE ContactId = @ContactId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ContactId", model.ContactId);
                cmd.Parameters.AddWithValue("@FullName", model.FullName);
                cmd.Parameters.AddWithValue("@Email", model.Email);
                cmd.Parameters.AddWithValue("@MobileNumber", model.MobileNumber);
                cmd.Parameters.AddWithValue("@Subject", (object?)model.Subject ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Message", model.Message);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            TempData["SuccessMessage"] = "Contact message updated successfully.";
            return RedirectToAction("ContactList");
        }

        [HttpGet]
        public IActionResult ContactDetail(int id)
        {
            ContactUsModel model = new ContactUsModel();
            string connectionString = _configuration.GetConnectionString("DefaultConnection")
               ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM BankContactUs WHERE ContactId = @ContactId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ContactId", id);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        model.ContactId = Convert.ToInt32(reader["ContactId"]);
                        model.FullName = reader["FullName"].ToString();
                        model.Email = reader["Email"].ToString();
                        model.MobileNumber = reader["MobileNumber"].ToString();
                        model.Subject = reader["Subject"].ToString();
                        model.Message = reader["Message"].ToString();
                    }
                    else
                    {
                        return NotFound();
                    }
                }
            }

            return View(model);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
