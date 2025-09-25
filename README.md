# Multi-Target .NET Library Solution

This solution demonstrates a C# class library that seamlessly integrates with both **.NET Framework 4.8** and **.NET 8**, providing MongoDB database connectivity and System.Web compatibility.

## Solution Structure

```
MultiTargetLibrary.sln
├── MultiTargetLibrary/                    # Main class library (multi-targeting)
│   ├── Configuration/
│   │   └── ConfigurationHelper.cs        # Configuration management
│   ├── Data/
│   │   ├── MongoDbContext.cs             # MongoDB context
│   │   └── MongoRepository.cs            # Generic repository pattern
│   ├── Services/
│   │   └── UserService.cs                # Sample service with CRUD operations
│   ├── Web/
│   │   └── HttpContextUtils.cs           # Cross-framework web utilities
│   └── LibraryManager.cs                 # Main library facade
├── TestConsoleApp.Net48/                 # .NET Framework 4.8 test app
└── TestConsoleApp.Net8/                  # .NET 8 test app
```

## Key Features

### 🎯 Multi-Targeting
- **Single codebase** that compiles for both .NET Framework 4.8 and .NET 8
- **Conditional compilation** for framework-specific code
- **Framework detection** at runtime

### 🗄️ MongoDB Integration
- **MongoDB.Driver** support for both frameworks
- **Generic repository pattern** for CRUD operations
- **Connection management** with proper disposal
- **Async/await** support throughout

### 🌐 System.Web Compatibility
- **HttpContext utilities** that work across frameworks
- **Session management** abstraction
- **Request information** extraction (URL, IP, User Agent, etc.)
- **Framework-agnostic** web operations

### 📝 Configuration Management
- **Microsoft.Extensions.Configuration** support
- **JSON configuration files**
- **Strongly-typed** configuration access
- **Default value** handling

### 📊 Logging Integration
- **Microsoft.Extensions.Logging** support
- **Structured logging** throughout the library
- **Null logger** fallbacks for optional scenarios

## Project Configuration

### MultiTargetLibrary.csproj Key Features:

```xml
<PropertyGroup>
  <TargetFrameworks>net48;net8.0</TargetFrameworks>
  <LangVersion>latest</LangVersion>
  <Nullable>enable</Nullable>
</PropertyGroup>

<!-- Framework-specific dependencies -->
<ItemGroup Condition="'$(TargetFramework)' == 'net48'">
  <Reference Include="System.Web" />
</ItemGroup>

<ItemGroup Condition="'$(TargetFramework)' == 'net8.0'">
  <FrameworkReference Include="Microsoft.AspNetCore.App" />
</ItemGroup>
```

## Usage Examples

### Basic Library Usage

```csharp
// Initialize library with configuration
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

using var libraryManager = new LibraryManager(configuration, logger);

// Test database connectivity
var isConnected = await libraryManager.TestAllServicesAsync();

// Use MongoDB operations
var user = await libraryManager.UserService.CreateUserAsync("John Doe", "john@example.com");
```

### .NET Framework 4.8 Usage

```csharp
// Traditional approach - direct instantiation
var config = ConfigurationManager.AppSettings;
var library = new LibraryManager(configuration);

// Web utilities work with System.Web.HttpContext
var currentUrl = LibraryManager.Web.GetCurrentUrl(HttpContext.Current);
```

### .NET 8 Usage with Dependency Injection

```csharp
// Register services in DI container
services.AddMultiTargetLibrary(services, configuration);

// Inject and use
public class MyController : ControllerBase
{
    private readonly LibraryManager _libraryManager;
    
    public MyController(LibraryManager libraryManager)
    {
        _libraryManager = libraryManager;
    }
    
    // Web utilities work with HttpContext from DI
    public IActionResult GetInfo()
    {
        var url = LibraryManager.Web.GetCurrentUrl(HttpContext);
        return Ok(url);
    }
}
```

## Configuration

### appsettings.json Example:

```json
{
  "ConnectionStrings": {
    "MongoDB": "mongodb://localhost:27017"
  },
  "MongoDB": {
    "DatabaseName": "YourDatabase"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

## Building and Running

### Prerequisites
- Visual Studio 2022 or later
- .NET Framework 4.8 Developer Pack
- .NET 8 SDK
- MongoDB server (local or remote)

### Build Commands

```powershell
# Build the entire solution
dotnet build

# Build for specific framework
dotnet build --framework net48
dotnet build --framework net8.0

# Run .NET Framework test app
dotnet run --project TestConsoleApp.Net48

# Run .NET 8 test app
dotnet run --project TestConsoleApp.Net8
```

## Framework-Specific Implementation Details

### Conditional Compilation
The library uses preprocessor directives to handle framework differences:

```csharp
#if NETFRAMEWORK
    // .NET Framework 4.8 specific code
    return System.Web.HttpContext.Current?.Request?.Url?.ToString();
#else
    // .NET 8 specific code
    return $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}";
#endif
```

### Dependency Management
- **Common dependencies**: MongoDB.Driver, Microsoft.Extensions.*
- **.NET Framework specific**: System.Web reference
- **.NET 8 specific**: Microsoft.AspNetCore.App framework reference

## Best Practices Implemented

1. **Dispose Pattern**: Proper resource cleanup
2. **Async/Await**: Throughout for better scalability
3. **Logging**: Comprehensive logging with structured messages
4. **Configuration**: Externalized settings with type safety
5. **Error Handling**: Proper exception handling and logging
6. **Separation of Concerns**: Clear architectural layers
7. **Dependency Injection**: Full DI support for .NET 8

## Extension Points

The library is designed to be extensible:

- **Add new repositories**: Extend `MongoRepository<T>`
- **Add new services**: Follow the `UserService` pattern
- **Add new web utilities**: Extend `HttpContextUtils`
- **Add new configuration**: Extend `ConfigurationHelper`

## License

This is a sample solution for educational and development purposes.
