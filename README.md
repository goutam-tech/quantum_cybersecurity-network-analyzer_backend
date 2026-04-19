# network_project

# Entity Framework Core
dotnet add package Microsoft.EntityFrameworkCore --version 8.0.0

# EF Core Design (for migrations)
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.0

# PostgreSQL provider
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 8.0.0

# Swagger UI
dotnet add package Swashbuckle.AspNetCore --version 6.5.0

dotnet restore


# Create the migration (skip if you already have the Migrations/ folder)
dotnet ef migrations add InitialCreate

# Apply migration → creates all 6 tables in PostgreSQL
dotnet ef database update

dotnet run