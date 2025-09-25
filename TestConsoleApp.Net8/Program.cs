using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MultiTargetLibrary;
using MultiTargetLibrary.Services;
using System;
using System.Threading.Tasks;

namespace TestConsoleApp.Net8
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== .NET 8 Test Application ===");
            Console.WriteLine();

            // Create host builder with dependency injection
            var host = CreateHostBuilder(args).Build();

            try
            {
                // Get services from DI container
                var logger = host.Services.GetRequiredService<ILogger<Program>>();
                var libraryManager = host.Services.GetRequiredService<LibraryManager>();

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
                    var userService = host.Services.GetRequiredService<UserService>();
                    await TestUserOperations(userService, logger);
                }

                // Test web utilities (simulated)
                TestWebUtilities();

                Console.WriteLine();
                Console.WriteLine("Test completed successfully!");
            }
            catch (Exception ex)
            {
                var logger = host.Services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Application error occurred");
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // Register the multi-target library services
                    LibraryManager.AddMultiTargetLibrary(services, hostContext.Configuration);
                });

        private static async Task TestUserOperations(UserService userService, ILogger logger)
        {
            try
            {
                Console.WriteLine("--- Testing User Operations ---");

                // Create a test user
                var testEmail = $"test.user.{DateTime.Now.Ticks}@example.com";
                var user = await userService.CreateUserAsync("Test User .NET 8", testEmail);
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
            
            Console.WriteLine($"Current Framework: {LibraryManager.GetCurrentFramework()}");
            Console.WriteLine("Web utilities are available for HTTP context operations");
            Console.WriteLine("In .NET Framework: Uses System.Web.HttpContext");
            Console.WriteLine("In .NET 8: Uses Microsoft.AspNetCore.Http.HttpContext");
            
            // Demonstrate web utility methods (they would work with actual HTTP context)
            Console.WriteLine("Available web methods:");
            Console.WriteLine("- GetCurrentUrl()");
            // Test web utilities without HttpContext (they will handle null gracefully)
            var url = LibraryManager.Web.GetCurrentUrl();
            Console.WriteLine($"Current URL (null context): {url ?? "None (console app)"}");
            Console.WriteLine("- GetUserAgent()");
            var userAgent = LibraryManager.Web.GetUserAgent();
            Console.WriteLine($"User Agent (null context): {userAgent ?? "None (console app)"}");

        }
    }
}
