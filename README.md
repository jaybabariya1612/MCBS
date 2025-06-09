Sure! Here's a full polished **README.md** for your **MCBS - Modern Core Banking System** project, with properly formatted sections including the fixed folder structure and everything else you provided:

````markdown
# 💼 MCBS - Modern Core Banking System

> A secure and feature-rich banking application developed using **ASP.NET Core MVC**, designed to manage customer accounts, perform transactions, and handle banking operations in a modern web interface.

---

## 🚀 Key Features

### 👤 Account Management
- Create bank accounts with auto-generated Account Number & IFSC
- Edit/update KYC info
- List accounts with search & filters
- Edit Account profile
- Prevent duplicate entries (Aadhaar, PAN, Mobile)

### 🔐 Customer Portal
- Secure login using Account Number + DOB as PIN
- View balance and transaction history
- Session-based authentication

### 💸 Transaction System
- Transfer money using Account No. or Mobile No.
- Full transaction history with filter
- Balance check & DOB confirmation for security
- Transaction rollback on failure

### 🛠 Admin Functionalities
- View all registered bank accounts
- Manage messages from contact form
- Handle customer registration & validation

---

## 🧰 Tech Stack

| Layer       | Technology                |
|-------------|---------------------------|
| Backend     | ASP.NET Core 6.0 MVC      |
| Frontend    | Razor Views, Bootstrap 5  |
| Database    | SQL Server (Stored Procedures) |
| Auth        | Session-based             |
| Tools Used  | Visual Studio, Git        |

---

## 🗂️ Folder Structure

```plaintext
MCBS/
│
├── Controllers/                # Contains all MVC controller logic
│   ├── AccountController.cs
│   ├── CustomerController.cs
│   ├── HomeController.cs
│   ├── LoginController.cs
│   └── TransactionController.cs
│
├── Models/                     # Data models for entities used throughout the application
│   ├── AccountModel.cs
│   ├── ContactUsModel.cs
│   ├── ErrorViewModel.cs
│   ├── LoginModel.cs
│   ├── RegisterModel.cs
│   ├── TransactionHistoryModel.cs
│   ├── TransactionModel.cs
│   └── TransferRequestModel.cs
│
├── Views/                      # Razor views grouped by feature
│   ├── Account/
│   ├── Customer/
│   ├── Home/
│   ├── Login/
│   ├── Transaction/
│   └── Shared/
│       ├── _ViewImports.cshtml
│       └── _ViewStart.cshtml
│
├── wwwroot/                    # Static files (CSS, JS, images)
│
├── appsettings.json            # Main configuration file
├── appsettings.Development.json
├── Program.cs                  # Application startup
├── MCBS.csproj                 # Project file
├── MCBS.sln                    # Visual Studio solution file
└── ...
````

---

## 🛠️ Setup Instructions

### Prerequisites

* ✅ .NET 6.0 SDK
* ✅ SQL Server
* ✅ Visual Studio 2022+

### Steps

1. **Clone the repository**:

   ```bash
   git clone https://github.com/jaybabariya1612/MCBS.git
   cd MCBS
   ```

2. **Configure Database**:

   * Update the connection string in `appsettings.json` to point to your SQL Server instance.
   * Run the provided SQL scripts to create and seed tables like `IndianBankAccount`, `TransactionHistories`, `BankContactUs`, `tbl_Register`, etc.

3. **Run the project**:

   ```bash
   dotnet run
   ```

4. **Open your browser and navigate to**:

   ```
   https://localhost:7159/
   ```

---

## 📌 Core Database Tables

| Table                  | Purpose                          |
| ---------------------- | -------------------------------- |
| `IndianBankAccount`    | Stores customer & account info   |
| `TransactionHistories` | Tracks debit/credit transactions |
| `BankContactUs`        | Stores contact form submissions  |
| `tbl_Register`         | Registration details             |

---

## 🧪 Usage Scenarios

### Create Account

> URL: `/Account/Account`
> Admin can add new accounts. System auto-generates account number & IFSC.

### Login & Dashboard

> URL: `/Login/Login`
> Customers log in using account number and DOB.

### Send Money

> URL: `/Transaction/SendMoney`
> Transfer funds securely, requires DOB validation.

### Contact Us

> URL: `/Home/ContactUs`
> Users submit queries; admins manage responses.

---

## 🔐 Security Highlights

* Secure session-based login
* DOB PIN verification for transactions
* Parameterized SQL queries to prevent injection
* Validations to avoid duplicate entries (Aadhaar, PAN, Mobile)
* Transaction rollback on failure to ensure data integrity

---

## 🧪 API Endpoints (MVC Actions)

| Route                    | Method | Purpose                 |
| ------------------------ | ------ | ----------------------- |
| `/Account/Account`       | POST   | Create new bank account |
| `/Login/Login`           | POST   | Authenticate user       |
| `/Transaction/SendMoney` | POST   | Perform a fund transfer |
| `/Home/ContactUs`        | POST   | Submit a contact form   |

---

## 📸 Screenshots

*You can showcase screenshots of:*

* ✨ Dashboard view
* 📊 Transaction page
* 📝 Account form
* 🔐 Login screen

*(Add your images in the repository and link here)*

---

## 📄 License

Licensed under the [MIT License](LICENSE)

---

## 🤝 Contributing

Contributions are welcome! If you plan significant changes, please open an issue to discuss before submitting a pull request.

---

## 📬 Contact

Created by **Jay Babariya**
📧 Email: `jaybabariya01@gmail.com`
🔗 [GitHub Profile](https://github.com/jaybabariya1612)

---
