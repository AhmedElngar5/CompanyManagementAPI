# Company Management API

An enterprise-grade ASP.NET Core Web API built with **.NET 10**, **Entity Framework Core 10**, and **Microsoft SQL Server**. This solution implements a robust REST API modeling an organization's Departments and Employees, demonstrating precise endpoint routing, route constraints, query string filtering, pagination, sorting, data validation, and relational integrity.

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Key Features](#2-key-features)
3. [Technology Stack](#3-technology-stack)
4. [Solution Architecture & Directory Structure](#4-solution-architecture--directory-structure)
5. [Database Schema & Entity Relationships](#5-database-schema--entity-relationships)
6. [Prerequisites](#6-prerequisites)
7. [SQL Server Configuration](#7-sql-server-configuration)
8. [Connection String Configuration](#8-connection-string-configuration)
9. [NuGet Package Dependencies](#9-nuget-package-dependencies)
10. [EF Core CLI Tools Installation](#10-ef-core-cli-tools-installation)
11. [EF Core Migration Commands](#11-ef-core-migration-commands)
12. [Database Creation & Update](#12-database-creation--update)
13. [How to Run the Application](#13-how-to-run-the-application)
14. [API Endpoints Reference](#14-api-endpoints-reference)
15. [Example Request and Response Bodies](#15-example-request-and-response-bodies)
16. [Postman Collection & Test Suite Instructions](#16-postman-collection--test-suite-instructions)
17. [Pagination & Sorting Usage](#17-pagination--sorting-usage)
18. [Validation Rules](#18-validation-rules)
19. [HTTP Status Codes](#19-http-status-codes)
20. [Technical Review Questions and Answers](#20-technical-review-questions-and-answers)

---

## 1. Project Overview

The **Company Management API** is a technical showcase of idiomatic ASP.NET Core Web API design patterns and Entity Framework Core capabilities. It models a common corporate hierarchy:
- A **Department** represents a business unit (e.g., Information Technology, Human Resources, Engineering).
- An **Employee** represents an individual team member tied directly to a specific department.

The application enforces strict relational constraints, eliminates circular JSON serialization references through Data Transfer Objects (DTOs), guarantees transactional consistency, and exposes a comprehensive set of RESTful endpoints adhering to HTTP standards.

---

## 2. Key Features

- **Exactly Two Dedicated Controllers**: Cleanly organized into `DepartmentsController` and `EmployeesController`.
- **Relational Integrity**: 1-to-Many relationship configured via EF Core Fluent API with `DeleteBehavior.Restrict` to protect departments with active staff from accidental deletion.
- **Advanced Attribute Routing**:
  - Base routes: `api/[controller]`
  - Integer route constraints: `{id:int}`
  - Alphabetic route constraints: `{name:alpha}`
  - Decimal route constraints: `{salary:decimal}`
  - Optional route parameters: `{name?}`
  - Nested resource routing: `api/departments/{departmentId:int}/employees/{employeeId:int}`
- **Rich Query String Filtering**:
  - Filter by `departmentId`, `minSalary`, and `maxSalary` dynamically using `[FromQuery]`.
  - Validation ensuring `minSalary <= maxSalary`.
- **Pagination & Sorting**:
  - Standard pagination with `pageNumber` and `pageSize` and rich pagination metadata (`totalPages`, `totalCount`, `hasNext`, `hasPrevious`).
  - Ascending and descending salary sorting via `sortBy=salary&sortOrder=asc|desc`.
- **Async/Await Across the Board**: Non-blocking database calls using EF Core asynchronous APIs (`ToListAsync`, `FirstOrDefaultAsync`, `AnyAsync`, `CountAsync`, `SaveChangesAsync`).
- **DTO Separation & Loop Prevention**: Flat and nested DTOs isolate internal EF models and eliminate cyclic graph errors.
- **Comprehensive Verification Suite**: Included Postman Collection (46 automated requests) and integration test script with 100% pass rate.

---

## 3. Technology Stack

- **Framework**: ASP.NET Core Web API
- **Target Framework**: .NET 10 (`net10.0`)
- **Language**: C# 13 / 14 preview
- **ORM**: Entity Framework Core 10 (`10.0.15`)
- **Database**: Microsoft SQL Server / LocalDB (SQL Server 2022)
- **Database Approach**: Code First with EF Core Migrations
- **API Testing**: Postman Collection v2.1 & PowerShell Automated Runner
- **IDE**: Visual Studio 2022 / 2026, Visual Studio Code, or JetBrains Rider

---

## 4. Solution Architecture & Directory Structure

```
CompanyManagementAPI/
│
├── CompanyManagementAPI.sln                 # Visual Studio standard solution
├── CompanyManagementAPI.slnx                # Modern XML solution
├── README.md                                # Comprehensive project documentation
├── .gitignore                               # Git ignore configuration
│
├── CompanyManagementAPI/                    # Web API Project
│   ├── Controllers/
│   │   ├── DepartmentsController.cs         # Department CRUD and nested employee endpoints
│   │   └── EmployeesController.cs           # Employee CRUD, search, filter, pagination, sorting
│   │
│   ├── Models/
│   │   ├── Department.cs                    # Department database entity
│   │   └── Employee.cs                      # Employee database entity
│   │
│   ├── DTOs/
│   │   ├── Department/
│   │   │   ├── CreateDepartmentDto.cs       # Payload for department creation
│   │   │   ├── UpdateDepartmentDto.cs       # Payload for department updates
│   │   │   └── DepartmentResponseDto.cs     # Outgoing department data with nested employee summaries
│   │   │
│   │   └── Employee/
│   │       ├── CreateEmployeeDto.cs         # Payload for employee creation
│   │       ├── UpdateEmployeeDto.cs         # Payload for employee updates
│   │       └── EmployeeResponseDto.cs       # Outgoing employee data with department name
│   │
│   ├── Data/
│   │   └── ApplicationDbContext.cs          # EF Core DbContext with DbSets and model configurations
│   │
│   ├── Configurations/
│   │   ├── DepartmentConfiguration.cs       # Fluent API mapping for Department
│   │   └── EmployeeConfiguration.cs         # Fluent API mapping for Employee
│   │
│   ├── Migrations/
│   │   ├── 20261119164633_InitialCreate.cs  # Initial database migration code
│   │   ├── 20261119164633_InitialCreate.Designer.cs
│   │   └── ApplicationDbContextModelSnapshot.cs
│   │
│   ├── Properties/
│   │   └── launchSettings.json              # Server ports (HTTP 5050, HTTPS 7211)
│   │
│   ├── appsettings.json                     # Production / default configuration
│   ├── appsettings.Development.json         # Development configuration with LocalDB
│   ├── Program.cs                           # App startup, DI configuration, middleware pipeline
│   └── CompanyManagementAPI.csproj          # .NET 10 project file with EF Core packages
│
└── Postman/
    ├── CompanyManagementAPI.postman_collection.json    # Complete 46-test Postman suite
    └── CompanyManagementAPI.postman_environment.json   # Postman environment variables
```

---

## 5. Database Schema & Entity Relationships

The domain features a **One-to-Many (1:N)** relationship between `Department` (principal) and `Employee` (dependent).

```
┌────────────────────────────────┐         ┌────────────────────────────────┐
│          Departments           │         │           Employees            │
├────────────────────────────────┤         ├────────────────────────────────┤
│ PK  Id          (int, identity)│ 1     * │ PK  Id          (int, identity)│
│     Name        (nvarchar 100) ├─────────┤     FullName    (nvarchar 150) │
│     Description (nvarchar 500) │         │     Email       (nvarchar 256) │ [UNIQUE INDEX]
│     Location    (nvarchar 200) │         │     Salary      (decimal 18,2) │
│     CreatedAt   (datetime2)    │         │     HireDate    (datetime2)    │
└────────────────────────────────┘         │ FK  DepartmentId(int)          │ [INDEX, ON DELETE RESTRICT]
                                           └────────────────────────────────┘
```

### Relational Constraints Enforced:
1. **Primary Keys**: Auto-incrementing 32-bit integers (`Id`).
2. **Foreign Key**: `DepartmentId` in `Employees` references `Id` in `Departments`.
3. **Unique Index**: An explicit unique index `IX_Employees_Email` prevents duplicate emails at the database layer.
4. **Delete Behavior (`DeleteBehavior.Restrict`)**: Prevents deleting a department if one or more employees reference it. Attempting to delete such a department will fail, and our controller explicitly intercepts this condition and returns an informative **HTTP 400 Bad Request** before any SQL error is thrown.
5. **Column Precision**: `Salary` is mapped to `decimal(18,2)` to guarantee financial accuracy without floating-point rounding errors.

---

## 6. Prerequisites

Ensure your development machine has the following installed:

- **.NET 10 SDK** (v10.0.100 or later)
  Verify via:
  ```bash
  dotnet --info
  ```
- **Microsoft SQL Server** (2019, 2022, or SQL Server Express / LocalDB).
  - SQL Server LocalDB is included with Visual Studio (`MSSQLLocalDB`).
- **Postman** (Desktop application or web agent) for executing API tests.

---

## 7. SQL Server Configuration

Ensure your SQL Server instance is running:

### For SQL Server LocalDB (Recommended for local dev):
```powershell
sqllocaldb start MSSQLLocalDB
sqllocaldb info MSSQLLocalDB
```

### For SQL Server Express / Developer:
Ensure the `MSSQLSERVER` or `SQLEXPRESS` service is running in Windows Services:
```powershell
Get-Service -Name "*SQL*"
```

---

## 8. Connection String Configuration

The connection string is configured in `appsettings.json` and overridden in `appsettings.Development.json`.

### Default / Production (`appsettings.json`):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CompanyManagementDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### Development (`appsettings.Development.json`):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CompanyManagementDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### Environment Variable Override:
You can override the connection string at runtime without modifying code:
```bash
# Windows PowerShell
$env:ConnectionStrings__DefaultConnection="Server=YOUR_SERVER;Database=CompanyManagementDB;User Id=sa;Password=YourStrongPassword!;TrustServerCertificate=True;"

# Linux / macOS Bash
export ConnectionStrings__DefaultConnection="Server=YOUR_SERVER;Database=CompanyManagementDB;User Id=sa;Password=YourStrongPassword!;TrustServerCertificate=True;"
```

---

## 9. NuGet Package Dependencies

The following packages are installed in `CompanyManagementAPI.csproj`:

| Package Name | Version | Purpose |
|---|---|---|
| `Microsoft.EntityFrameworkCore.SqlServer` | `10.0.15` | SQL Server database provider for EF Core |
| `Microsoft.EntityFrameworkCore.Tools` | `10.0.15` | Migrations and scaffolding command-line tools |
| `Microsoft.EntityFrameworkCore.Design` | `10.0.15` | Design-time shared components for EF Core tools |

To restore all packages manually:
```bash
dotnet restore
```

---

## 10. EF Core CLI Tools Installation

If `dotnet ef` is not installed globally on your machine, install it with:

```bash
dotnet tool install --global dotnet-ef
```

To update the tool to the latest version matching .NET 10:
```bash
dotnet tool update --global dotnet-ef
```

Verify installation:
```bash
dotnet ef --version
```

---

## 11. EF Core Migration Commands

The project already includes the initial migration `20261119164633_InitialCreate.cs`. If you make changes to entities and need to create a new migration:

### Add a New Migration:
```bash
cd CompanyManagementAPI
dotnet ef migrations add <MigrationName>
```

### Remove the Last Unapplied Migration:
```bash
dotnet ef migrations remove
```

### Generate SQL Script from Migrations:
```bash
dotnet ef migrations script
```

---

## 12. Database Creation & Update

To apply pending migrations and create the `CompanyManagementDB` database and tables:

```bash
cd CompanyManagementAPI
dotnet ef database update
```

This command will:
1. Connect to the configured SQL Server instance.
2. Create the `CompanyManagementDB` database if it does not already exist.
3. Create the `__EFMigrationsHistory` tracking table.
4. Execute DDL statements to create the `Departments` and `Employees` tables, foreign keys, indexes, and constraints.

---

## 13. How to Run the Application

### Option 1: Via .NET CLI
```bash
cd CompanyManagementAPI
dotnet run
```
The application will start listening on:
- HTTP: `http://localhost:5050`
- HTTPS: `https://localhost:7211`

### Option 2: Via Visual Studio
1. Open `CompanyManagementAPI.sln` or `CompanyManagementAPI.slnx`.
2. Set `CompanyManagementAPI` as the Startup Project.
3. Select the `http` profile (port 5050) or `https` profile (port 7211).
4. Press **F5** (Debug) or **Ctrl+F5** (Run without debugging).

---

## 14. API Endpoints Reference

### DepartmentsController (`api/departments`)

| HTTP Method | Route | Description | Status Codes |
|---|---|---|---|
| `GET` | `/api/departments` | Returns all departments including their employee lists | `200 OK` |
| `GET` | `/api/departments/{id:int}` | Returns a single department by ID with employees | `200 OK`, `404 Not Found` |
| `POST` | `/api/departments` | Creates a new department | `201 Created`, `400 Bad Request` |
| `PUT` | `/api/departments/{id:int}` | Updates an existing department | `204 No Content`, `400 Bad Request`, `404 Not Found` |
| `DELETE` | `/api/departments/{id:int}` | Deletes department (prohibited if employees exist) | `204 No Content`, `400 Bad Request`, `404 Not Found` |
| `GET` | `/api/departments/{departmentId:int}/employees` | Returns all employees in a specific department | `200 OK`, `404 Not Found` |
| `GET` | `/api/departments/{departmentId:int}/employees/{employeeId:int}` | Nested route: returns employee only if in department | `200 OK`, `404 Not Found` |

### EmployeesController (`api/employees`)

| HTTP Method | Route | Description | Status Codes |
|---|---|---|---|
| `GET` | `/api/employees` | Returns paginated employees with salary sorting | `200 OK` |
| `GET` | `/api/employees/{id:int}` | Returns employee by ID with department info | `200 OK`, `404 Not Found` |
| `POST` | `/api/employees` | Creates employee with full validation | `201 Created`, `400 Bad Request` |
| `PUT` | `/api/employees/{id:int}` | Updates employee (excluding self for email check) | `204 No Content`, `400 Bad Request`, `404 Not Found` |
| `DELETE` | `/api/employees/{id:int}` | Deletes employee | `204 No Content`, `404 Not Found` |
| `GET` | `/api/employees/department/{departmentId:int}` | Returns employees filtered by DepartmentId | `200 OK`, `404 Not Found` |
| `GET` | `/api/employees/salary/{salary:decimal}` | Exact salary lookup with decimal route constraint | `200 OK`, `404 Not Found` |
| `GET` | `/api/employees/search/{name?}` | Optional route parameter: searches by name or all | `200 OK` |
| `GET` | `/api/employees/search/by-name/{name:alpha}` | Search with alphabetic route constraint | `200 OK`, `404 Not Found` |
| `GET` | `/api/employees/filter` | Query string filtering (`departmentId`, `minSalary`, `maxSalary`) | `200 OK`, `400 Bad Request` |

---

## 15. Example Request and Response Bodies

### 1. Create Department
**Request:**
`POST /api/departments`
```json
{
  "name": "Information Technology",
  "description": "Software Engineering & Infrastructure",
  "location": "Building A, Floor 3"
}
```
**Response (201 Created):**
`Location: /api/departments/1`
```json
{
  "id": 1,
  "name": "Information Technology",
  "description": "Software Engineering & Infrastructure",
  "location": "Building A, Floor 3",
  "createdAt": "2026-11-19T17:00:00Z",
  "employeeCount": 0,
  "employees": []
}
```

### 2. Create Employee
**Request:**
`POST /api/employees`
```json
{
  "fullName": "Ahmed Hassan",
  "email": "ahmed.hassan@company.com",
  "salary": 8500.00,
  "hireDate": "2024-01-15T00:00:00",
  "departmentId": 1
}
```
**Response (201 Created):**
`Location: /api/employees/1`
```json
{
  "id": 1,
  "fullName": "Ahmed Hassan",
  "email": "ahmed.hassan@company.com",
  "salary": 8500.00,
  "hireDate": "2024-01-15T00:00:00",
  "departmentId": 1,
  "departmentName": "Information Technology"
}
```

### 3. Get Department with Employees
**Request:**
`GET /api/departments/1`
**Response (200 OK):**
```json
{
  "id": 1,
  "name": "Information Technology",
  "description": "Software Engineering & Infrastructure",
  "location": "Building A, Floor 3",
  "createdAt": "2026-11-19T17:00:00Z",
  "employeeCount": 1,
  "employees": [
    {
      "id": 1,
      "fullName": "Ahmed Hassan",
      "email": "ahmed.hassan@company.com",
      "salary": 8500.00,
      "hireDate": "2024-01-15T00:00:00"
    }
  ]
}
```

### 4. Delete Department with Employees (Blocked)
**Request:**
`DELETE /api/departments/1`
**Response (400 Bad Request):**
```json
{
  "message": "Cannot delete department 'Information Technology' because it still has 1 employee(s). Remove all employees first."
}
```

---

## 16. Postman Collection & Test Suite Instructions

A fully organized, pre-configured Postman test suite is provided in the `Postman/` folder:

- `Postman/CompanyManagementAPI.postman_collection.json`
- `Postman/CompanyManagementAPI.postman_environment.json`

### Import Instructions:
1. Open **Postman**.
2. Click **Import** (top left).
3. Select both files:
   - `CompanyManagementAPI.postman_collection.json`
   - `CompanyManagementAPI.postman_environment.json`
4. In Postman, switch the active environment to **Company Management API - Environment** (top right selector).

### Collection Organization:
The collection is structured into 8 test suites containing 46 automated requests:
1. `1. Departments` (Create, Read, Update, List, Department Employees)
2. `2. Employees` (Create multiple, Read, Update, List, Filter by Dept)
3. `3. Routing Tests` (Nested routes, Alpha constraint valid/invalid, Decimal constraint valid/invalid, Int constraint invalid)
4. `4. Filtering and Searching` (Search no-name, Search with name, Department filter, Salary range filter, Exact salary)
5. `5. Validation Tests` (Invalid Dept ID, Duplicate email, Invalid email syntax, Negative salary, Zero salary, Invalid salary range)
6. `6. Error Handling Tests` (Non-existent IDs for GET/DELETE, Prohibited department deletion)
7. `7. Pagination and Sorting` (Page 1, Page 2, Salary ASC, Salary DESC)
8. `8. Relationship Tests` (Eager loading verification, Cascading deletion protection verification, Clean teardown)

### Automated Workflow Execution:
Each request contains embedded JavaScript test assertions that:
- Verify HTTP status codes (`pm.response.to.have.status(...)`).
- Automatically extract returned resource IDs and populate environment variables (`pm.environment.set('departmentId', jsonData.id)`).
- Validate payload shapes and response headers.

To run the entire suite in sequence:
Click on the collection name in Postman -> Click **Run Collection** -> Click **Run Company Management API – Complete Test Suite**.

---

## 17. Pagination & Sorting Usage

### Pagination:
Use `pageNumber` (1-indexed) and `pageSize` query string parameters:
```
GET /api/employees?pageNumber=1&pageSize=2
```
**Response Format:**
```json
{
  "data": [
    {
      "id": 1,
      "fullName": "Ahmed Hassan",
      "email": "ahmed.hassan@company.com",
      "salary": 8500.00,
      "hireDate": "2024-01-15T00:00:00",
      "departmentId": 1,
      "departmentName": "Information Technology"
    },
    {
      "id": 2,
      "fullName": "Sara Ali",
      "email": "sara.ali@company.com",
      "salary": 12000.50,
      "hireDate": "2023-06-01T00:00:00",
      "departmentId": 1,
      "departmentName": "Information Technology"
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 2,
    "totalCount": 3,
    "totalPages": 2,
    "hasNext": true,
    "hasPrevious": false
  }
}
```

### Sorting:
Sort employees by salary in either ascending or descending order:
- **Ascending**: `GET /api/employees?sortBy=salary&sortOrder=asc`
- **Descending**: `GET /api/employees?sortBy=salary&sortOrder=desc`

Both pagination and sorting can be combined:
```
GET /api/employees?pageNumber=1&pageSize=5&sortBy=salary&sortOrder=desc
```

---

## 18. Validation Rules

| Field | Location | Validation Rule | Error Outcome |
|---|---|---|---|
| `Department.Name` | Body | Required, MaxLength(100) | `400 Bad Request` |
| `Department.Description` | Body | Optional, MaxLength(500) | `400 Bad Request` |
| `Department.Location` | Body | Optional, MaxLength(200) | `400 Bad Request` |
| `Employee.FullName` | Body | Required, MaxLength(150) | `400 Bad Request` |
| `Employee.Email` | Body | Required, Valid Email syntax, MaxLength(256), Unique across database | `400 Bad Request` |
| `Employee.Salary` | Body | Required, Greater than zero (`Range(0.01, Max)`) | `400 Bad Request` |
| `Employee.DepartmentId` | Body | Required, Must match an existing Department ID | `400 Bad Request` |
| `minSalary` vs `maxSalary` | Query | `minSalary` must not exceed `maxSalary` | `400 Bad Request` |
| Department Deletion | Route | Department must have 0 employees to be deleted | `400 Bad Request` |

---

## 19. HTTP Status Codes

| Code | Name | Scenario |
|---|---|---|
| `200` | OK | Successful GET operations returning records or filtered collections. |
| `201` | Created | Successful POST operations creating a Department or Employee, with `Location` header. |
| `204` | No Content | Successful PUT updates and DELETE removals. |
| `400` | Bad Request | Validation errors (negative salary, duplicate email, invalid department ID, minSalary > maxSalary, deleting department with employees). |
| `404` | Not Found | Requested department, employee, or nested resource does not exist, or route constraint does not match. |

---

## 20. Technical Review Questions and Answers

### Question 1: How do `[FromRoute]` and `[FromQuery]` differ, and when would you use each?

**Answer:**
`[FromRoute]` and `[FromQuery]` specify where the ASP.NET Core model binding subsystem binds parameter values from an incoming HTTP request:

- **`[FromRoute]`**:
  - **Source**: Binds values directly embedded in the URL path, matching named route template tokens (e.g., `/api/departments/{id:int}` binds `{id}` from the path).
  - **Usage**: Used to identify specific resources or resource hierarchies (canonical identity). Use route parameters when the parameter is indispensable to identifying which unique resource or nested sub-resource is being acted upon (e.g., `/api/departments/3/employees/10`).
- **`[FromQuery]`**:
  - **Source**: Binds values from key-value pairs appended after the `?` delimiter in the URL query string (e.g., `/api/employees/filter?departmentId=2&minSalary=5000`).
  - **Usage**: Used for optional criteria, modifiers, filters, search terms, pagination parameters (`pageNumber`, `pageSize`), and sorting orders (`sortBy`, `sortOrder`). The query string alters how resources are presented, searched, or shaped, rather than identifying the base resource itself.

---

### Question 2: Can a controller contain multiple GET actions? How does routing select the correct action?

**Answer:**
Yes, a controller can contain as many GET actions as needed. ASP.NET Core's endpoint routing engine selects the correct action through a deterministic candidate selection and scoring pipeline:

1. **HTTP Verb Matching**: Only actions adorned with `[HttpGet]` (or compatible verb attributes) are considered for a `GET` request.
2. **Path Segment Count & Literal Tokens**: Routes with literal path tokens take precedence over parameter tokens. For example, `api/employees/search` or `api/employees/filter` match before dynamic segments.
3. **Route Template Specificity**:
   - Explicit templates like `api/employees/{id:int}` require exactly one integer segment.
   - Distinct prefixes like `api/employees/department/{departmentId:int}` and `api/employees/salary/{salary:decimal}` match based on their intermediate literal segments (`department` vs `salary`).
4. **Route Constraints**: If two routes match the path shape, route constraints (e.g., `:int`, `:alpha`, `:decimal`) act as filters. Only actions whose constraints evaluate to true remain eligible candidates.
5. **Action Selection**: If exactly one candidate matches, that action is invoked. If multiple candidates have identical precedence and matching criteria, ASP.NET Core throws an `AmbiguousMatchException` at runtime.

---

### Question 3: How does `{id:int}` differ from `{id}`, and what happens when the constraint is not satisfied?

**Answer:**
- **Difference**:
  - `{id}` is an unconstrained route parameter that matches any string segment value (e.g., `"12"`, `"abc"`, `"5-3"`, `"true"`).
  - `{id:int}` applies the built-in `IntRouteConstraint`, which verifies that the incoming URL segment can be parsed as a valid 32-bit signed integer (`System.Int32`).
- **What happens when the constraint is not satisfied**:
  - Route constraints are evaluated during route matching **before** action invocation or model binding.
  - If a client requests `GET /api/employees/abc`, the `{id:int}` constraint evaluates to `false`.
  - The routing system excludes this endpoint candidate. If no other registered route pattern matches `/api/employees/abc`, ASP.NET Core returns **HTTP 404 Not Found**.
  - This prevents unnecessary controller instantiation, avoids type conversion exceptions during model binding, and avoids polluting controllers with manual parsing logic.

---

### Question 4: What makes two route templates ambiguous?

**Answer:**
Two route templates are ambiguous when the routing engine cannot deterministically decide which action has higher precedence for a given incoming request URL.

Ambiguity occurs when two endpoints in the same route tree share:
1. **The same HTTP verb** (e.g., both are `[HttpGet]`).
2. **The same number of path segments**.
3. **Equally specific route parameters** without distinguishing literals or non-overlapping constraints.

**Example of Ambiguity:**
```csharp
[HttpGet("{name}")]
public IActionResult GetByName(string name) { ... }

[HttpGet("{category}")]
public IActionResult GetByCategory(string category) { ... }
```
A request to `GET /api/products/gadgets` matches both templates identically. ASP.NET Core cannot infer which action the caller intended, resulting in a runtime `AmbiguousMatchException: The request matched multiple endpoints`.

To prevent ambiguity:
- Add differentiating literal path segments (e.g., `by-name/{name}` vs `by-category/{category}`).
- Apply mutually exclusive route constraints (e.g., `{id:int}` vs `{name:alpha}`).

---

### Question 5: When would you return `IActionResult` versus `ActionResult<T>`?

**Answer:**
- **`ActionResult<T>`**:
  - Introduced in ASP.NET Core 2.1 to combine the flexibility of returning any `IActionResult` (such as `NotFound()`, `BadRequest()`, `CreatedAtAction()`) with strong typing for the successful payload (`T`).
  - **When to use**: Recommended for operations that return a data payload on success (e.g., GET queries, POST resource creation). It enables compile-time type safety, improves OpenAPI/Swagger documentation by automatically discovering response types without requiring explicit `[ProducesResponseType]`, and provides cleaner syntax (`return Ok(dto)` or direct `return dto`).
- **`IActionResult`**:
  - Non-generic interface representing an HTTP response result.
  - **When to use**: Recommended when an action does not return a response payload body, such as:
    - `PUT` actions returning `204 NoContent()` or `404 NotFound()`.
    - `DELETE` actions returning `204 NoContent()` or `400 BadRequest()`.
    - Actions with polymorphic or heterogeneous response bodies where no single `T` exists.

---

### Question 6: Why are foreign keys and navigation properties both useful?

**Answer:**
Both serve distinct yet complementary roles across the database schema and application object graph:

1. **Foreign Keys (e.g., `Employee.DepartmentId`)**:
   - **Database Layer**: Enforce referential integrity in relational storage, ensuring child records cannot reference non-existent parent rows and controlling cascade/restrict deletion behaviors.
   - **Performance**: Enable creating, updating, or filtering dependent entities without having to load the entire parent object into memory. For example, assigning `employee.DepartmentId = 2` issues a lightweight SQL `UPDATE Employees SET DepartmentId = 2 WHERE Id = @id`.
2. **Navigation Properties (e.g., `Employee.Department` and `Department.Employees`)**:
   - **Object-Oriented Domain**: Represent relational associations as native C# object references and collections, allowing idiomatic traversal (e.g., `employee.Department.Name`).
   - **Query Shaping**: Enable EF Core to perform eager loading via `.Include(d => d.Employees)`, lazy loading, and projection queries via LINQ.
   - **Combining Both**: Exposing both a foreign key scalar and navigation reference provides maximum flexibility: lightweight scalar manipulation when ID is sufficient, and rich relational traversal when navigating object graphs.

---

### Question 7: How do `Include()` and `Select()` differ when shaping related data?

**Answer:**
Both methods load related data in Entity Framework Core, but they operate at different stages of the query lifecycle:

| Aspect | `Include()` | `Select()` (Projection) |
|---|---|---|
| **Mechanism** | Eager loading of full entity graphs | LINQ projection into DTOs or anonymous types |
| **Data Retrieved** | Fetches **all columns** of both the principal and dependent tables | Fetches **only the specific columns** requested in the projection |
| **Change Tracking** | Loaded entities are attached to the `ChangeTracker` (unless `AsNoTracking()` is used) | Projected DTOs are lightweight and **not tracked** by EF Core |
| **SQL Efficiency** | Generates `LEFT JOIN` retrieving entire rows; can incur network and memory overhead | Generates optimized SQL `SELECT` with only chosen columns |
| **Circular Reference Risk** | High if entities with bi-directional navigation are directly serialized to JSON | None; maps directly to clean, dedicated DTO contracts |
| **Best Use Case** | When related entities need to be updated, validated, or modified in business logic | Read-only API queries, reporting, and high-performance read endpoints |

---

### Question 8: Why should database calls generally use asynchronous EF Core methods?

**Answer:**
Database calls are I/O-bound operations that involve network latency, query execution on the SQL server, and data streaming.

1. **Thread Pool Starvation Prevention**:
   - In a synchronous call (e.g., `.ToList()`, `.SaveChanges()`), the ASP.NET Core request thread blocks and sits completely idle waiting for the database server to respond.
   - Under moderate to high concurrent traffic, the server's thread pool quickly exhausts available threads, causing incoming HTTP requests to be queued (increased latency, 503 errors, and server unresponsiveness).
2. **Scalability and Resource Efficiency**:
   - Asynchronous methods (e.g., `ToListAsync()`, `FirstOrDefaultAsync()`, `SaveChangesAsync()`) leverage non-blocking I/O completion ports (IOCP) via the `async/await` state machine.
   - While the database processes the query, the calling thread is released back to the thread pool to process other concurrent HTTP requests.
   - When the SQL server finishes streaming the result, a thread is taken from the pool to resume execution from the `await` statement.
3. **Conclusion**:
   - Asynchronous database calls do not make a single query execute faster, but they dramatically multiply the **throughput** and **concurrency** of the web application.

---

## License & Author
This project was implemented as a production-standard solution for the Senior ASP.NET Core Technical Challenge.
All tests, routes, validations, migrations, and architectural requirements have been verified and documented.
