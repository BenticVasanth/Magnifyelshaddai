# Magnify El-Shaddai

ASP.NET MVC application targeting .NET Framework 4.8.

## Overview

This repository contains a web application built with ASP.NET MVC and Entity Framework (EDMX model). The project includes a `Web.config` with connection string `ElshaddaiDBContext` and runtime/assembly redirects.

## Prerequisites

- Windows
- Visual Studio 2019 or later (with .NET Framework 4.8 SDK/targeting pack)
- .NET Framework 4.8 installed
- SQL Server instance for the application's database

## Setup

1. Clone the repository.
2. Open the solution in Visual Studio.
3. Restore NuGet packages (Visual Studio should do this automatically).
4. Update the connection string `ElshaddaiDBContext` in `Web.config` to point to your SQL Server instance and database.
   - The project uses an EDMX model; ensure the metadata paths and provider connection string are correct.
5. Build the solution and run the web project (F5).

## Notes

- The project is configured with large request size limits in `Web.config` (`maxAllowedContentLength` and `maxRequestLength`). Adjust if required.
- If you encounter assembly binding issues, verify the binding redirects in `Web.config` under `<runtime>`.

## Contributing

Contributions are welcome. Open issues or submit pull requests.

## License

Specify a license for the repository if needed.
