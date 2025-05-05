# The-Quel Subdivision Management System

The-Quel is a comprehensive web application built with ASP.NET Core MVC for managing residential subdivisions. The system provides tools for property management, payment tracking, complaint handling, event management, and community engagement.

## Features

- **User Management**: Registration, authentication, and role-based authorization
- **Property Management**: Track lot information, ownership, and property details
- **Payment System**: Manage dues, fees, and payment tracking
- **Complaint System**: File and track complaints and community issues
- **Event Management**: Schedule and manage community events
- **Responsive UI**: Modern Bootstrap-based interface that works on all devices

## Technology Stack

- **ASP.NET Core MVC**: Web application framework
- **Entity Framework Core**: Data access and ORM
- **SQL Server**: Database
- **Bootstrap 5**: Frontend UI framework
- **Clean Architecture**: Separation of concerns and maintainability

## Project Structure

The project follows clean architecture principles with the following layers:

- **Core**: Domain entities, interfaces, and business logic
- **Data**: Database context, repositories, and data access
- **Services**: Business logic implementation
- **Presentation**: Controllers and views

## Getting Started

### Prerequisites

- .NET 6.0 SDK or higher
- SQL Server (or SQL Server Express)
- Visual Studio 2022 or Visual Studio Code

### Installation

1. Clone the repository
2. Open the solution in Visual Studio
3. Update the connection string in `appsettings.json` to point to your SQL Server
4. Run the following commands in Package Manager Console:
   ```
   update-database
   ```
5. Run the application

## Database Migration

The project uses Entity Framework Core Code-First approach. To create and apply migrations:

```
add-migration InitialCreate
update-database
```

## Security

The application implements:

- Authentication using ASP.NET Core Identity
- Role-based authorization (Admin, Staff, HomeOwner)
- CSRF protection
- Password hashing and security
- Input validation

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Contact

For questions or support, please contact:
- Email: info@the-quel.com 