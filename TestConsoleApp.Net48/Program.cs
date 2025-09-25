using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MultiTargetLibrary;
using MultiTargetLibrary.Services;

namespace TestConsoleApp.Net48
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== .NET Framework 4.8 Test Application ===");
            Console.WriteLine();

            // Set up configuration
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            // Set up logging
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole()
                       .SetMinimumLevel(LogLevel.Information);
            });

            var logger = loggerFactory.CreateLogger<Program>();

            try
            {
                // Initialize the library
                using var libraryManager = new LibraryManager(configuration, loggerFactory.CreateLogger<LibraryManager>());

                Console.WriteLine($"Library Info: {LibraryManager.GetLibraryInfo()}");
                Console.WriteLine();

                // Test database connection
                Console.WriteLine("Testing database connection...");
                var connectionTest = await libraryManager.TestAllServicesAsync();
                Console.WriteLine($"Connection test result: {(connectionTest ? "SUCCESS" : "FAILED")}");
                Console.WriteLine();

                if (connectionTest)
                {
                    // Test user operations
                    await TestUserOperations(libraryManager.UserService, logger);
                }

                // Test web utilities (simulated)
                TestWebUtilities();

                Console.WriteLine();
                Console.WriteLine("Test completed successfully!");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Application error occurred");
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static async Task TestUserOperations(UserService userService, ILogger logger)
        {
            try
            {
                Console.WriteLine("--- Testing User Operations ---");

                // Create a test user
                var testEmail = $"test.user.{DateTime.Now.Ticks}@example.com";
                var user = await userService.CreateUserAsync("Test User", testEmail);
                Console.WriteLine($"Created user: {user.Name} ({user.Email}) with ID: {user.Id}");

                // Retrieve the user
                var retrievedUser = await userService.GetUserByEmailAsync(testEmail);
                Console.WriteLine($"Retrieved user: {retrievedUser?.Name} - Active: {retrievedUser?.IsActive}");

                // Update user status
                if (retrievedUser != null)
                {
                    var updateResult = await userService.UpdateUserStatusAsync(retrievedUser.Id, false);
                    Console.WriteLine($"Updated user status: {updateResult}");
                }

                // Get user count
                var userCount = await userService.GetUserCountAsync();
                Console.WriteLine($"Total users in database: {userCount}");

                // Clean up - delete the test user
                if (retrievedUser != null)
                {
                    var deleteResult = await userService.DeleteUserAsync(retrievedUser.Id);
                    Console.WriteLine($"Deleted test user: {deleteResult}");
                }

                Console.WriteLine("User operations completed successfully!");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during user operations");
                Console.WriteLine($"User operations failed: {ex.Message}");
            }
        }

        private static void TestWebUtilities()
        {
            Console.WriteLine("--- Testing Web Utilities ---");
            
            // Note: In a real web application, these would have actual HTTP context
            Console.WriteLine($"Current Framework: {LibraryManager.GetCurrentFramework()}");
            Console.WriteLine("Library supports web context operations across frameworks");
            
            // Simulate web utility usage
            Console.WriteLine("Web utilities are available for HTTP context operations");
            Console.WriteLine("In .NET Framework: Uses System.Web.HttpContext");
            Console.WriteLine("In .NET 8: Uses Microsoft.AspNetCore.Http.HttpContext");
            
            // Test web utilities without HttpContext (they will handle null gracefully)
            var url = LibraryManager.Web.GetCurrentUrl();
            Console.WriteLine($"Current URL (null context): {url ?? "None (console app)"}");
            
            var userAgent = LibraryManager.Web.GetUserAgent();
            Console.WriteLine($"User Agent (null context): {userAgent ?? "None (console app)"}");
        }
    }
}
