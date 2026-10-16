# Branch Stock Transfer System

A small full-stack application for viewing branch inventory and managing stock transfers. The API is built with ASP.NET Core and Entity Framework Core; the browser UI uses React and Vite.

## Requirements

- .NET 9 SDK
- Node.js and npm
- SQL Server running locally (the default connection uses Windows integrated authentication)

## Run the project

Open two terminals at the repository root.

1. Create or update the SQL Server database using the included Entity Framework migration:

   ```powershell
   dotnet tool install --global dotnet-ef --version 9.0.0
   dotnet ef database update --project .\StockTransferApi\StockTransferApi.csproj
   ```

   If `dotnet-ef` is already installed, skip the install command. The connection string is in `StockTransferApi/appsettings.json`; adjust it if your SQL Server instance differs.

2. Start the API in the first terminal:

   ```powershell
   dotnet run --project .\StockTransferApi\StockTransferApi.csproj
   ```

   The configured HTTP address is `http://localhost:5135`. In Development, Swagger is available at `http://localhost:5135/swagger`.

3. Install frontend dependencies and start Vite in the second terminal:

   ```powershell
   cd .\stock-transfer-web
   npm install
   npm run dev
   ```

4. Open the local URL printed by Vite (normally `http://localhost:5173`). The frontend API base URL is currently `http://localhost:5135/api`, configured in `stock-transfer-web/src/api/axiosInstance.js`.

## Run the tests

From the repository root:

```powershell
dotnet test .\StockTransferApi.Tests\StockTransferApi.Tests.csproj
```

The controller tests use a separate EF Core InMemory database for each test, so they do not require SQL Server. They cover insufficient-stock dispatch, rejecting a second dispatch, successful receive and stock update, rejecting receive before dispatch, and transfer creation validation.

## Main behavior

- View products and stock by branch.
- Create a transfer request with one or more transfer lines in the API (the current form submits one item at a time).
- Dispatch a requested transfer only when the source branch has sufficient stock.
- Receive a dispatched transfer and add its quantities to the destination branch.
- Cancel a transfer while it is still requested.
- Prevent transfers from using the same source and destination branch, non-positive quantities, and repeated dispatches.

## Assumptions

- A branch's stock is identified by the branch and product pair; the database enforces uniqueness for that pair.
- The included seed data is suitable for a local demonstration and is applied by the initial migration.
- The source branch is deducted at dispatch; the destination branch is credited at receive.
- The local development setup uses SQL Server with Windows authentication and allows the React development server through the API's current CORS policy.
- The UI is a single dashboard; the separate page files in the frontend are not the active app flow.

## Trade-offs

- The backend uses EF Core and controller endpoints to keep the project small and direct. More layers would add structure, but would be unnecessary for this assignment's size.
- The tests use EF Core InMemory for quick, isolated business-rule checks. This verifies controller logic, but does not exercise SQL Server behavior, migrations, or HTTP serialization end to end.
- The frontend is a single-page dashboard with state and API coordination in `App.jsx`; the UI pieces are split into focused components. This is simple to follow at this size, but API services and state hooks would help if it grows.
- The API URL and SQL Server connection string are local development settings rather than environment-based deployment configuration.

## Decisions

- UI Action State Management: Transfer actions are dynamically restricted based on the current status to maintain workflow integrity:

Requested: Users can either Dispatch or Cancel the transfer.

Dispatched: Cancellation is locked; the only available action is Receive (which also supports the optional partial/damaged goods recording flow).

Received / Cancelled: These are terminal states where no further actions can be performed.

- All business logic and transfer rules came from the assignment brief. Every design decision in this project was made by me according to those requirements, and I designed the database table structure myself.

- Role-Based Access & Branch Separation: In a real-world enterprise stock transfer system, dispatching and receiving operations are typically handled by users at two different branch locations with strict role-based access control. Since this assignment does not explicitly require multi-user authentication or role restrictions, the application is designed as a unified operations dashboard where these actions can be triggered sequentially.