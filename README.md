# Greenix Enterprise Grievance Platform

Greenix is an enterprise-grade Civic Complaint Management and Grievance Resolution API built on **.NET 9** following the principles of **Clean Architecture** (Onion Architecture). It provides city administrators, government officers, field workers, and citizens with coordinated workflows, location-aware queries, secure file uploads, KPI metrics reporting, and automated alerts.

---

## Architecture & Project Structure

The solution (`Greenix.sln`) is divided into 6 distinct layers following Clean Architecture guidelines:

1. **Greenix.Domain**: Represents the core business layer. Defines auditing abstractions, enums (`ComplaintStatus`, `PriorityLevel`, `ImageType`), and domain entities (`User`, `Department`, `Category`, `Complaint`, `ComplaintImage`, `WorkerAssignment`, `ComplaintStatusHistory`, `Feedback`).
2. **Greenix.Application**: Orchestrates application logic and use cases. Contains DTOs, query/command service contracts, custom service implementations (`ComplaintService`, `DashboardService`, `ReportService`), and registration extensions.
3. **Greenix.Infrastructure**: Provides core technological implementations. Houses token signing services (`TokenService`), geospatial calculation engines (`GeospatialService`), file managers (`LocalFileStorageService`), SMTP email dispatchers (`EmailService`), and IAM identity management (`IdentityService`).
4. **Greenix.Persistence**: Connects to storage systems. Defines `ApplicationDbContext`, entity Fluent API mappings, automatic audit auditing interceptors, and default database initializations.
5. **Greenix.Shared**: Shares utility constants (`Roles`), responses wrappers (`BaseResponse`), and shared model classes.
6. **Greenix.API**: The presentation entry point. Mapped with REST controllers, Serilog file logging pipelines, Swagger JWT authorize configurations, health checks (`/health`), and versioned routes.
7. **Greenix.Application.UnitTests**: The automated testing suite containing xUnit and Moq implementations.

---

## Prerequisites

To run this application locally, ensure you have the following installed:
- **.NET 9.0 SDK** or later
- **Visual Studio 2022** (v17.10+) or **VS Code**
- **Microsoft SQL Server** (local `SQLEXPRESS` instance or Docker container)
- **Docker Desktop** (optional, for containerized deployment)

---

## Getting Started Locally

### 1. Database Connection Configuration
By default, the application is configured to connect to a local SQL Server Express instance:
- **Server**: `localhost\SQLEXPRESS`
- **Database**: `GreenixAPI`
- **Connection String** (in `Greenix.API/appsettings.json`):
  `"Server=localhost\\SQLEXPRESS;Database=GreenixAPI;Trusted_Connection=True;TrustServerCertificate=True;"`

Ensure SQL Express is running on your machine before starting.

### 2. Restore Dependencies
Navigate to the root directory and restore NuGet packages:
```bash
dotnet restore
```

### 3. Run EF Core Migrations
Apply database migrations to generate tables and seed initial department/category master data:
```bash
dotnet ef database update --project Greenix.Persistence --startup-project Greenix.API
```
*(If you do not have the EF tool installed, run `dotnet tool install --global dotnet-ef` first).*

### 4. Build and Run the API
Run the project using the .NET CLI:
```bash
dotnet run --project Greenix.API
```
Or open `Greenix.sln` in Visual Studio and click **Start / Debug (F5)**.

Access the API documentation at:
- **Swagger UI**: `http://localhost:5000/swagger` or `https://localhost:5001/swagger`

---

## Running the Unit Tests

The test suite checks coordinate boundaries, department limits, and resolving proof parameters. Run the test suite:
```bash
dotnet test
```

---

## Containerized Deployment (Docker Compose)

Greenix comes with a ready-to-run multi-container Docker Compose file that spins up both the API and a MS SQL Server instance:

1. **Build and start the containers**:
   ```bash
   docker-compose up --build -d
   ```
2. **Access the application**:
   - The API will be available at: `http://localhost:8080`
   - The Database will be running on port `1433` inside the container network.
3. **Environment Overrides**:
   - Connection strings are overridden inside `docker-compose.yml` using `ConnectionStrings__DefaultConnection`.

---

## Frontend Setup (React + Vite + TypeScript)

The solution includes a responsive, premium React client under the `Greenix.Client` directory.

### 1. Install dependencies
Navigate to the client directory and install the package dependencies:
```bash
cd Greenix.Client
npm install
```

### 2. Run the development server
Start the React dev application:
```bash
npm run dev
```
The client will start at `http://localhost:3000`.

### 3. API Proxy Integration
All requests starting with `/api` and `/uploads` are automatically proxied to the backend server at `http://localhost:5000` via the settings in `vite.config.ts`.

