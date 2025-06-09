using MCBS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;

namespace MCBS.Controllers
{
    public class TransactionController : Controller
    {
        private readonly IConfiguration _configuration;

        public TransactionController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult SendMoney()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SendMoney(TransactionModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var senderAccountIdStr = HttpContext.Session.GetString("AccountId");
            if (string.IsNullOrEmpty(senderAccountIdStr))
            {
                TempData["ErrorMessage"] = "User session expired. Please log in again.";
                return RedirectToAction("Login", "Login");
            }

            int senderAccountId = Convert.ToInt32(senderAccountIdStr);
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction sqlTxn = conn.BeginTransaction())
                {
                    try
                    {
                        // Get sender account details
                        SqlCommand getSenderCmd = new SqlCommand("SELECT AccountNumber, Balance, DateOfBirth FROM IndianBankAccount WHERE AccountId = @SenderId", conn, sqlTxn);
                        getSenderCmd.Parameters.AddWithValue("@SenderId", senderAccountId);
                        SqlDataReader senderReader = getSenderCmd.ExecuteReader();

                        if (!senderReader.Read())
                        {
                            ModelState.AddModelError("", "Sender account not found.");
                            return View(model);
                        }

                        string senderAccountNumber = senderReader["AccountNumber"].ToString();
                        decimal senderBalance = Convert.ToDecimal(senderReader["Balance"]);
                        DateTime DateOfBirth = Convert.ToDateTime(senderReader["DateOfBirth"]);
                        senderReader.Close();

                        // Validate DOB (used as PIN)
                        if (DateOfBirth.Date != model.DobPin.Date)
                        {
                            ModelState.AddModelError("", "Invalid DateOfBirth PIN.");
                            return View(model);
                        }

                        if (senderBalance < model.Amount)
                        {
                            ModelState.AddModelError("", "Insufficient balance.");
                            return View(model);
                        }

                        // Determine whether input is mobile or account number
                        bool isMobile = Regex.IsMatch(model.ReceiverIdentifier, @"^\d{10}$");
                        SqlCommand getReceiverCmd = new SqlCommand(@"
                    SELECT AccountId, AccountNumber FROM IndianBankAccount 
                    WHERE " + (isMobile ? "MobileNumber = @Receiver" : "AccountNumber = @Receiver"), conn, sqlTxn);
                        getReceiverCmd.Parameters.AddWithValue("@Receiver", model.ReceiverIdentifier);
                        SqlDataReader receiverReader = getReceiverCmd.ExecuteReader();

                        if (!receiverReader.Read())
                        {
                            ModelState.AddModelError("", "Receiver account not found.");
                            return View(model);
                        }

                        int receiverAccountId = Convert.ToInt32(receiverReader["AccountId"]);
                        string receiverAccountNumber = receiverReader["AccountNumber"].ToString();
                        receiverReader.Close();

                        // Debit sender
                        SqlCommand debitCmd = new SqlCommand("UPDATE IndianBankAccount SET Balance = Balance - @Amount WHERE AccountId = @SenderId", conn, sqlTxn);
                        debitCmd.Parameters.AddWithValue("@Amount", model.Amount);
                        debitCmd.Parameters.AddWithValue("@SenderId", senderAccountId);
                        debitCmd.ExecuteNonQuery();

                        // Credit receiver
                        SqlCommand creditCmd = new SqlCommand("UPDATE IndianBankAccount SET Balance = Balance + @Amount WHERE AccountId = @ReceiverId", conn, sqlTxn);
                        creditCmd.Parameters.AddWithValue("@Amount", model.Amount);
                        creditCmd.Parameters.AddWithValue("@ReceiverId", receiverAccountId);
                        creditCmd.ExecuteNonQuery();

                        // Transaction log - sender
                        SqlCommand senderTxnCmd = new SqlCommand(@"
                    INSERT INTO TransactionHistories (AccountId, TransactionType, Amount, Description)
                    VALUES (@AccountId, 'Debit', @Amount, @Description)", conn, sqlTxn);
                        senderTxnCmd.Parameters.AddWithValue("@AccountId", senderAccountId);
                        senderTxnCmd.Parameters.AddWithValue("@Amount", model.Amount);
                        senderTxnCmd.Parameters.AddWithValue("@Description", $"Sent to A/C {receiverAccountNumber}");
                        senderTxnCmd.ExecuteNonQuery();

                        // Transaction log - receiver
                        SqlCommand receiverTxnCmd = new SqlCommand(@"
                    INSERT INTO TransactionHistories (AccountId, TransactionType, Amount, Description)
                    VALUES (@AccountId, 'Credit', @Amount, @Description)", conn, sqlTxn);
                        receiverTxnCmd.Parameters.AddWithValue("@AccountId", receiverAccountId);
                        receiverTxnCmd.Parameters.AddWithValue("@Amount", model.Amount);
                        receiverTxnCmd.Parameters.AddWithValue("@Description", $"Received from A/C {senderAccountNumber}");
                        receiverTxnCmd.ExecuteNonQuery();

                        sqlTxn.Commit();
                        TempData["SuccessMessage"] = "Transaction successful.";
                        return RedirectToAction("SendMoney");
                    }
                    catch (Exception ex)
                    {
                        sqlTxn.Rollback();
                        TempData["ErrorMessage"] = "Transaction failed: " + ex.Message;
                        return View(model);
                    }
                }
            }
        }
        public IActionResult TransactionHistory(DateTime? fromDate, DateTime? toDate)
        {
            var accountIdStr = HttpContext.Session.GetString("AccountId");
            if (string.IsNullOrEmpty(accountIdStr))
            {
                TempData["ErrorMessage"] = "User session expired. Please log in again.";
                return RedirectToAction("Login", "Login");
            }

            int accountId = Convert.ToInt32(accountIdStr);
            var transactions = new List<TransactionHistoryModel>();

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // SQL to extract counterparty account number from Description and get both names
                string sql = @"
            WITH ExtractedCounterparty AS (
                SELECT 
                    t.TransactionId,
                    t.AccountId,
                    a.AccountNumber,
                    a.AccountHolderName,
                    t.TransactionType,
                    t.Amount,
                    t.TransactionDate,
                    t.Description,

                    -- Extract counterparty account number depending on Debit or Credit
                    CASE 
                        WHEN t.TransactionType = 'Debit' THEN
                            LTRIM(RTRIM(SUBSTRING(t.Description, CHARINDEX('Sent to A/C', t.Description) + LEN('Sent to A/C '), 20)))
                        WHEN t.TransactionType = 'Credit' THEN
                            LTRIM(RTRIM(SUBSTRING(t.Description, CHARINDEX('Received from A/C', t.Description) + LEN('Received from A/C '), 20)))
                        ELSE NULL
                    END AS CounterpartyAccountNumber
                FROM TransactionHistories t
                JOIN IndianBankAccount a ON t.AccountId = a.AccountId
                WHERE t.AccountId = @accountId
                  AND (@fromDate IS NULL OR t.TransactionDate >= @fromDate)
                  AND (@toDate IS NULL OR t.TransactionDate <= @toDate)
            )

            SELECT 
                e.TransactionId,
                e.AccountId,
                e.AccountNumber,
                e.AccountHolderName,
                e.TransactionType,
                e.Amount,
                e.TransactionDate,
                e.Description,
                e.CounterpartyAccountNumber,
                ic.AccountHolderName AS CounterpartyAccountHolderName
            FROM ExtractedCounterparty e
            LEFT JOIN IndianBankAccount ic ON e.CounterpartyAccountNumber = ic.AccountNumber
            ORDER BY e.TransactionDate DESC;
        ";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@accountId", accountId);
                    cmd.Parameters.AddWithValue("@fromDate", (object)fromDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@toDate", (object)toDate ?? DBNull.Value);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            transactions.Add(new TransactionHistoryModel
                            {
                                TransactionId = (int)reader["TransactionId"],
                                AccountId = (int)reader["AccountId"],
                                AccountNumber = reader["AccountNumber"].ToString(),
                                AccountHolderName = reader["AccountHolderName"].ToString(),
                                TransactionType = reader["TransactionType"].ToString(),
                                Amount = (decimal)reader["Amount"],
                                TransactionDate = (DateTime)reader["TransactionDate"],
                                Description = reader["Description"].ToString(),
                                CounterpartyAccountNumber = reader["CounterpartyAccountNumber"]?.ToString(),
                                CounterpartyAccountHolderName = reader["CounterpartyAccountHolderName"]?.ToString() ?? "Unknown"
                            });
                        }
                    }
                }
            }

            return View(transactions);
        }


    }
}
