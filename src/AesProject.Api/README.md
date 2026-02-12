# AES Encryption Project - Backend API

This is a .NET 8 Web API project that provides secure file encryption (AES-GCM), decryption, and Google Drive integration.

## Features

- **Secure Encryption**: Uses AES-GCM (Galois/Counter Mode) with 128/192/256-bit keys. Keys and IVs are generated uniquely for each file and stored securely in a PostgreSQL database.
- **Authentication**: Built-in user management using ASP.NET Core Identity (cookies).
- **Google Drive Integration**: Upload encrypted files directly to the user's Google Drive. Uses OAuth 2.0 for secure authorization.
- **Zip Slip Protection**: Securely handles zip file extraction to prevent path traversal attacks.
- **Layered Architecture**: Organized into API (Presentation), Core (Domain/Interfaces), and Infrastructure (Data/Services).

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL](https://www.postgresql.org/) (Running locally or via Docker)

## Configuration

Configuration is managed via `appsettings.json` (or User Secrets for local development).

### `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=AesProject;Username=postgres;Password=yourpassword"
  },
  "Google": {
    "ClientId": "YOUR_GOOGLE_CLIENT_ID",
    "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET",
    "CredentialsPath": "token_store" // Directory to store user tokens
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

## Running the Application

1.  **Database Setup**: ensure PostgreSQL is running and update the `ConnectionStrings:DefaultConnection` in `appsettings.json`.
2.  **Apply Migrations**:
    ```bash
    dotnet ef database update --project ../AesProject.Infrastructure --startup-project .
    ```
3.  **Run API**:
    ```bash
    dotnet run
    ```
    The API will start on `http://localhost:5000` (HTTP) or configured HTTPS port.
    Swagger UI is available at `/swagger/index.html` in Development environment.

## Project Structure

- **AesProject.Api**: ASP.NET Core Web API controllers, configuration, and entry point.
- **AesProject.Core**: Domain entities (`AesEncryptionMetaInfo`), interfaces (`IAesEncryptionService`, `IGoogleDriveService`), and DTOs.
- **AesProject.Infrastructure**: Implementation of services (Encryption, File, Google Drive), EF Core `ApplicationDbContext`, and Migrations.
- **AesProject.Tests**: xUnit tests for core services.

## Key Services

- **AesEncryptionService**: Handles AES-GCM encryption/decryption. Stores metadata (Key, IV, Tag Length) in DB linked by a unique ID.
- **FileService**: Manages temporary file creation, zipping, and secure unzipping.
- **GoogleDriveService**: Manages OAuth flow and file uploads using the Google Drive API v3.
