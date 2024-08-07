# ApacheCamelPOC

This project is a .NET Core 6 console application that demonstrates integration with an ERP system, Google Sheets, and Apache ActiveMQ. It simulates some of the functionalities of Apache Camel using .NET technologies.

## Project Structure

```
ApacheCamelPOC/
│
├── ApacheCamelPOC.csproj
├── Program.cs
├── credentials.json
└── appsettings.json
```

## Requirements

- .NET Core 6 SDK
- Apache ActiveMQ running on your local machine
- Google Cloud account to access Google Sheets API

## Setup

### 1. Create the Project

Create a new .NET Core console application:

```bash
dotnet new console -n ApacheCamelPOC
cd ApacheCamelPOC
```

### 2. Add NuGet Packages
```
dotnet add package Google.Apis.Sheets.v4
dotnet add package Google.Apis.Auth
dotnet add package Apache.NMS
dotnet add package Apache.NMS.ActiveMQ
dotnet add package Microsoft.Extensions.Configuration.Json
dotnet add package Microsoft.Extensions.Configuration.Binder

```

### 3. Add Config Files
- ```appsettings.json``` and ```credentials.json```

### 4. Running the application
- Ensure ActiveMQ is running: You must have Apache ActiveMQ installed and running locally on the default port 61616.
- ```dotnet run```


### Important Notes
Security: Ensure that sensitive information, such as API keys and secrets, is not exposed in your source code or version control. Use secure storage solutions for production environments.
Environment Variables: Consider using environment variables or a secure secrets manager for storing sensitive data in production environments.