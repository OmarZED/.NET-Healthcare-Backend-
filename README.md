# .NET Healthcare Backend API

This repository contains the backend API service for a healthcare platform designed to facilitate communication and interaction between patients and doctors. Built with .NET [your .NET version, e.g., 8] and ASP.NET Core Web API.

## Core Features

*   **User Registration:** Separate registration endpoints for Patients and Doctors (`/api/auth/register-patient`, `/api/auth/register-doctor`).
*   **Authentication:** Secure login using email/password (`/api/auth/login`) with JWT Bearer token generation.
*   **Authorization:** Role-based access control (`Patient`, `Doctor` roles) using `[Authorize]` attributes.
*   **Profile Management:**
    *   Patients can view and update their own profile (`/api/patients/profile` - GET, PUT). Requires `Patient` role.
    *   Doctors can view and update their own profile (`/api/doctors/profile` - GET, PUT). Requires `Doctor` role.
*   **Doctor Discovery:**
    *   View a list of all available (verified) doctors (`/api/doctors/available` - GET). Requires authentication.
    *   View the detailed public profile of a specific doctor (`/api/doctors/{doctorId}` - GET). Requires authentication.
*   **(Planned)** Appointment Scheduling
*   **(Planned)** Secure Messaging between Patients and Doctors
*   **(Planned)** Admin functionalities (e.g., Doctor Verification)

## Technology Stack

*   **Framework:** .NET [your .NET version, e.g., 8] / ASP.NET Core Web API
*   **Authentication:** ASP.NET Core Identity
*   **Authorization:** JWT Bearer Tokens, Role-based Authorization (`[Authorize]`)
*   **Database ORM:** Entity Framework Core [your EF Core version, e.g., 8]
*   **Database Provider:** SQL Server (or specify yours if different)
*   **API Documentation:** Swagger / OpenAPI (Integrated)
*   **Dependency Injection:** Built-in .NET DI Container
*   **Logging:** Built-in .NET Logging (`ILogger`)

## Project Status

Actively under development. Core features implemented include authentication, profile management, and doctor discovery.

## Setup and Prerequisites

1.  **.NET SDK:** Install the required .NET SDK version ([e.g., .NET 8 SDK](https://dotnet.microsoft.com/download)).
2.  **Database:** A running instance of SQL Server (e.g., LocalDB installed with Visual Studio, a full SQL Server instance, or SQL Server running in Docker).
3.  **IDE/Editor:** Visual Studio 2022, VS Code (with C# Dev Kit extension), or JetBrains Rider.
4.  **Git:** For cloning the repository.
5.  **(Optional) Database Management Tool:** SQL Server Management Studio (SSMS) or Azure Data Studio to view/manage the database.

## Configuration

Configuration is managed via `appsettings.json` and environment-specific files (e.g., `appsettings.Development.json`).

Key sections to configure in `appsettings.json`:

1.  **`ConnectionStrings`:**
    ```json
    "ConnectionStrings": {
      "DefaultConnection": "Data Source=YOUR_SERVER_NAME;Initial Catalog=Medical;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False"
      // Or for SQL Authentication: "Data Source=YOUR_SERVER_NAME;Initial Catalog=Medical;User ID=your_sql_user;Password=your_sql_password;Encrypt=True;Trust Server Certificate=False;"
    }
    ```
    *   Update `Data Source` to your SQL Server instance name (e.g., `localhost`, `.\SQLEXPRESS`, `DESKTOP-QV6RRI3`).
    *   Ensure the `Medical` database can be created by EF Core Migrations or exists.
    *   Modify `Integrated Security`, `User ID`, `Password` based on your SQL Server authentication method.
    *   **Security Note:** For production, typically set `Encrypt=True` and `Trust Server Certificate=False`, ensuring your SQL Server has a valid certificate installed. `Trust Server Certificate=True` is often used for local development convenience but bypasses certificate validation.

2.  **`JWT` Settings:**
    ```json
    "JWT": {
      "Issuer": "https://yourdomain.com", // Replace with your identifier (e.g., your API's domain)
      "Audience": "https://yourapi.yourdomain.com", // Replace with your identifier (e.g., your API's domain or intended audience)
      "SigningKey": "!!REPLACE_THIS_WITH_A_VERY_STRONG_AND_SECRET_KEY_MIN_32_CHARS!!", // MUST be replaced with a strong, secret key
      "DurationInMinutes": 60 // Example: Token validity period
    }
    ```
    *   Replace placeholder `Issuer` and `Audience`. These should be consistent values checked during token validation.
    *   **CRITICAL SECURITY WARNING:** Replace the placeholder `SigningKey` with a **long, strong, randomly generated secret key**. Do **NOT** commit your actual `SigningKey` or production connection strings to source control. Use secure methods like:
        *   **User Secrets** (for local development)
        *   **Environment Variables**
        *   **Azure Key Vault** (or other cloud secret managers)

## Running the Project

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/OmarZED/.NET-Healthcare-Backend-.git
    cd .NET-Healthcare-Backend-
    ```
2.  **Configure `appsettings.json` (and User Secrets):**
    *   Update `ConnectionStrings` in `appsettings.json`.
    *   Configure the `JWT:SigningKey` using User Secrets locally:
        *   Right-click the project (`medical`) in Visual Studio -> Manage User Secrets, or run `dotnet user-secrets init` in the project directory.
        *   Add the key: `dotnet user-secrets set "JWT:SigningKey" "YOUR_SUPER_SECRET_RANDOM_KEY_HERE"`
    *   Set `JWT:Issuer` and `JWT:Audience` in `appsettings.json` or `appsettings.Development.json`.
3.  **Apply Database Migrations:** Ensure your database server is running. Open a terminal in the directory containing the `medical.csproj` file and run:
    ```bash
    dotnet ef database update
    ```
    This command creates the `Medical` database (if it doesn't exist) and applies the schema based on your EF Core migrations.
4.  **Run the application:**
    *   **Using .NET CLI:**
        ```bash
        dotnet run --project medical/medical.csproj
        # (Adjust path to your .csproj file if needed)
        ```
    *   **Using Visual Studio:** Open the solution (`.sln`) file and press F5 or click the Run button (often selects the `https` profile).
5.  **Access the API:** The application will start listening on specified ports (e.g., `https://localhost:7024`, `http://localhost:5136` - check the console output).
    *   The Swagger UI documentation is available at the HTTPS address + `/swagger` (e.g., `https://localhost:7024/swagger`).

## API Endpoints Summary

*   `/api/auth/register-patient` (POST): Register a new patient.
*   `/api/auth/register-doctor` (POST): Register a new doctor.
*   `/api/auth/login` (POST): Log in to get a JWT token.
*   `/api/patients/profile` (GET, PUT): Manage the logged-in patient's profile (Requires `Patient` role).
*   `/api/doctors/profile` (GET, PUT): Manage the logged-in doctor's profile (Requires `Doctor` role).
*   `/api/doctors/available` (GET): Get a list of all verified doctors (Requires authentication).
*   `/api/doctors/{doctorId}` (GET): Get details for a specific doctor (Requires authentication).

Refer to the Swagger UI (`/swagger`) hosted by the running application for detailed request/response models and interactive testing.