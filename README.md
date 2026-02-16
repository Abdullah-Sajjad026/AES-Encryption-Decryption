# AES Encryption Project - Getting Started Guide

This is a comprehensive guide to setting up, running, and deploying the AES Encryption Project, which includes a .NET 8 Backend API and a React Frontend.

## Table of Contents

- [Prerequisites](#prerequisites)
- [Project Structure](#project-structure)
- [Backend Setup](#backend-setup)
- [Frontend Setup](#frontend-setup)
- [Running Locally](#running-locally)
- [Deployment Guide](#deployment-guide)

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/) (v16+)
- [PostgreSQL](https://www.postgresql.org/) (Local or Docker)
- [Git](https://git-scm.com/)

## Project Structure

```bash
AES-Encryption-Project/
├── src/
│   ├── AesProject.Api/          # Backend API (Presentation Layer)
│   ├── AesProject.Core/         # Domain Models, Interfaces (Core Layer)
│   ├── AesProject.Infrastructure/ # Data Access, Encryption Logic (Infrastructure Layer)
│   ├── AesProject.Tests/        # Unit Tests
│   └── client/                  # Frontend React App (Vite)
├── legacy_java/                 # Original Java source code
└── README.md
```

## Backend Setup

1.  **Navigate to the Backend Directory**:
    ```bash
    cd src/AesProject.Api
    ```

2.  **Configure Database**:
    Update the `DefaultConnection` string in `appsettings.json` to point to your PostgreSQL instance.
    ```json
    "ConnectionStrings": {
      "DefaultConnection": "Host=localhost;Database=AesProject;Username=postgres;Password=your_password"
    }
    ```

3.  **Configure Google Drive Integration** (Optional for local testing):
    Create OAuth 2.0 credentials in the Google Cloud Console and add them to `appsettings.json`.
    ```json
    "Google": {
      "ClientId": "YOUR_CLIENT_ID",
      "ClientSecret": "YOUR_CLIENT_SECRET",
      "CredentialsPath": "token_store"
    }
    ```

4.  **Apply Database Migrations**:
    Ensure the `dotnet-ef` tool is installed:
    ```bash
    dotnet tool install --global dotnet-ef
    ```
    Apply migrations:
    ```bash
    dotnet ef database update --project ../AesProject.Infrastructure --startup-project .
    ```

5.  **Run the API**:
    ```bash
    dotnet run
    ```
    The API will start on `http://localhost:5000` (or `https://localhost:5001`).

## Frontend Setup

1.  **Navigate to the Frontend Directory**:
    ```bash
    cd src/client
    ```

2.  **Install Dependencies**:
    ```bash
    npm install
    ```

3.  **Configure API Endpoint**:
    Update the `src/client/src/api/api.ts` file or create a `.env` file to set the API base URL if different from default (`http://localhost:5000/api/v1`).

4.  **Run Development Server**:
    ```bash
    npm run dev
    ```
    The frontend will start on `http://localhost:5173`.

## Running Locally

1.  Start PostgreSQL (e.g., via Docker):
    ```bash
    docker run --name aes-postgres -e POSTGRES_PASSWORD=your_password -p 5432:5432 -d postgres
    ```
2.  Start Backend (see [Backend Setup](#backend-setup)).
3.  Start Frontend (see [Frontend Setup](#frontend-setup)).
4.  Open `http://localhost:5173` in your browser.
5.  Register a new user, log in, and test file encryption/decryption.

## Deployment Guide

### Backend Deployment (Windows/Linux/Docker)

1.  **Publish**:
    ```bash
    dotnet publish src/AesProject.Api/AesProject.Api.csproj -c Release -o ./publish
    ```
2.  **Run**:
    Copy the `publish` folder to your server.
    Run using: `dotnet AesProject.Api.dll`.
    Use a reverse proxy like Nginx or IIS to forward traffic to the Kestrel server.

### Frontend Deployment

1.  **Build**:
    ```bash
    cd src/client
    npm run build
    ```
    This creates a `dist` folder containing static assets.

2.  **Serve**:
    - **Nginx/Apache**: Copy the contents of `dist` to your web server root (e.g., `/var/www/html`). Configure routing to redirect all requests to `index.html` (SPA routing).
    - **ASP.NET Core Static Files**: Copy the `dist` contents to `src/AesProject.Api/wwwroot`. Enable static file serving in `Program.cs`:
        ```csharp
        app.UseStaticFiles();
        app.MapFallbackToFile("index.html");
        ```

### Docker Compose (Recommended for Full Stack)

Create a `docker-compose.yml` in the root:

```yaml
version: '3.8'
services:
  db:
    image: postgres:latest
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: password
      POSTGRES_DB: AesProject
    ports:
      - "5432:5432"

  backend:
    build:
      context: .
      dockerfile: src/AesProject.Api/Dockerfile
    ports:
      - "5000:80"
    environment:
      ConnectionStrings__DefaultConnection: "Host=db;Database=AesProject;Username=postgres;Password=password"
    depends_on:
      - db

  frontend:
    build:
      context: src/client
      dockerfile: Dockerfile
    ports:
      - "80:80"
```
