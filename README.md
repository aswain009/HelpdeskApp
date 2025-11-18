# Helpdesk Tool (MVP)

A simple internal helpdesk tool to manage support tickets. Built with ASP.NET Core (net10.0), Razor Pages for a minimal UI, and Entity Framework Core with SQLite.

## Features

* Manage Users and Tickets
* Create, edit, view, and list tickets
* Assign/unassign a ticket to a user
* Filter tickets by status and assigned user
* Automatic `CreatedAt` / `UpdatedAt` timestamps
* REST API with Swagger in Development

## Tech Stack

* ASP.NET Core Web API + Razor Pages
* EF Core 9 with SQLite (default) or any EF-supported provider
* xUnit + FluentAssertions + WebApplicationFactory for tests

---

## Getting Started

### Prerequisites (all platforms)

* .NET SDK 10 installed (`dotnet --version` should show 10.x)

### 1. Navigate to the API project

From the solution root:

```bash
cd Helpdesk.Api
```

> On Windows, you can run this in **PowerShell**, **CMD**, or **Git Bash**.
> On macOS, run in **Terminal** or **iTerm**.
> The commands below are the same for both platforms.

### 2. Run the API + UI

```bash
dotnet run
```

This will start the app with the default configuration.

### 3. Run the app in Development mode (hot reload)

```bash
dotnet watch run
```

This enables hot reload while you’re editing code.

### 4. Trust HTTPS development certificate (Windows & macOS)

If you haven’t done this before on your machine, you may need to trust the HTTPS dev certificate so `https://localhost` works without browser warnings:

```bash
dotnet dev-certs https --trust
```

You might see a prompt from your OS asking you to confirm trust.

### 5. Default URLs

By default the app listens on:

* [https://localhost:5001](https://localhost:5001)  *(Swagger UI available in [Development](https://localhost:5001/swagger))*
* [http://localhost:5000](http://localhost:5000)

---

## Database

* Default connection string: `Data Source=helpdesk.db`
* On first run, the app creates and seeds sample **Users** and **Tickets**

---

## REST Endpoints (key)

* `GET /api/users`
* `GET /api/users/{id}`
* `GET /api/tickets?status={Open|InProgress|Closed|int}&assignedUserId={int}`
* `GET /api/tickets/{id}`
* `POST /api/tickets`

  * Body: `Ticket` (Id ignored)
* `PUT /api/tickets/{id}`

  * Body: `Ticket` (Id must match `{id}`)
* `DELETE /api/tickets/{id}`
* `POST /api/tickets/{id}/assign`

  * Body: `{ assignedUserId: number | null }`

---

## Razor Pages UI

* `/` → redirects to `/Tickets/Index`
* `/Tickets/Index` → list + filters
* `/Tickets/Create` → create new ticket
* `/Tickets/Details/{id}` → view details + assign/unassign
* `/Tickets/Edit/{id}` → edit title/description/status/assignee

> Client-side validation scripts are intentionally omitted for MVP; server-side validation via DataAnnotations is enforced.

---

## Tests

Run tests (all platforms):

```bash
dotnet test
```

### What’s covered

* API workflows: create ticket, change status to Closed, assign/unassign, filtering
* EF Core timestamps: `CreatedAt` and `UpdatedAt` behavior on add/update
* Razor PageModels: Edit and Details assignment handlers

---

## Design Notes

* Separation: Models, Data (DbContext + seed), Controllers, Razor Pages
* DI: `HelpdeskDbContext` injected everywhere
* Timestamps: maintained in `HelpdeskDbContext.SaveChangesAsync`
* Swagger: enabled only in Development

---

## Configuration

Override connection string via `appsettings.json` or environment variables:

* `ConnectionStrings__Default`

---

## Future Enhancements

* AuthN/AuthZ
* More validations and `ProblemDetails` error responses
* Deployment to Azure App Service and managed database
