using MultiTargetLibrary.Configuration;
using MultiTargetLibrary.Data;
using MultiTargetLibrary.Services;
using MultiTargetLibrary.Web;
using MultiTargetLibrary.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

#if NETFRAMEWORK
using System.Web;
#else
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
#endif

namespace MultiTargetLibrary
{
    /// <summary>
    /// Main facade class for the multi-target library
    /// Provides unified access to all library functionality
    /// </summary>
    public class LibraryManager : IDisposable
    {
        private readonly ConfigurationHelper _configHelper;
        private readonly ILogger<LibraryManager> _logger;
        private UserService? _userService;
        private bool _disposed = false;

        public LibraryManager(IConfiguration configuration, ILogger<LibraryManager>? logger = null)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? new NullLogger<LibraryManager>();
            _configHelper = new ConfigurationHelper(configuration, logger as ILogger<ConfigurationHelper>);
            
            _logger.LogInformation("LibraryManager initialized for framework: {Framework}", GetCurrentFramework());
        }

        public IConfiguration Configuration { get; }

        /// <summary>
        /// Get the user service instance (lazy initialization)
        /// </summary>
        public UserService UserService
        {
            get
            {
                if (_userService == null)
                {
                    _userService = new UserService(_configHelper, _logger as ILogger<UserService>);
                }
                return _userService;
            }
        }

        /// <summary>
        /// Test all connections and services
        /// </summary>
        public async Task<bool> TestAllServicesAsync()
        {
            try
            {
                _logger.LogInformation("Testing all services...");
                
                // Test MongoDB connection
                var dbConnectionTest = await UserService.TestDatabaseConnectionAsync();
                if (!dbConnectionTest)
                {
                    _logger.LogError("Database connection test failed");
                    return false;
                }

                _logger.LogInformation("All service tests passed");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service tests failed");
                return false;
            }
        }

        /// <summary>
        /// Get HTTP context utilities
        /// </summary>
        public static class Web
        {
            public static string? GetCurrentUrl(object? httpContext = null)
            {
                return HttpContextUtils.GetRequestUrl(httpContext);
            }

            public static string? GetUserAgent(object? httpContext = null)
            {
                return HttpContextUtils.GetUserAgent(httpContext);
            }

            public static string? GetClientIP(object? httpContext = null)
            {
                return HttpContextUtils.GetClientIPAddress(httpContext);
            }

            public static bool IsSecure(object? httpContext = null)
            {
                return HttpContextUtils.IsHttps(httpContext);
            }

            public static T? GetSessionValue<T>(string key, object? httpContext = null)
            {
                return HttpContextUtils.GetSessionValue<T>(key, httpContext);
            }

            public static void SetSessionValue<T>(string key, T value, object? httpContext = null)
            {
                HttpContextUtils.SetSessionValue(key, value, httpContext);
            }
        }

        /// <summary>
        /// Get the current .NET framework version
        /// </summary>
        public static string GetCurrentFramework()
        {
#if NETFRAMEWORK
            return ".NET Framework 4.8";
#else
            return ".NET 8.0";
#endif
        }

        /// <summary>
        /// Get library version and info
        /// </summary>
        public static string GetLibraryInfo()
        {
            var framework = GetCurrentFramework();
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
            return $"MultiTargetLibrary v{version} running on {framework}";
        }

#if NET8_0_OR_GREATER
        /// <summary>
        /// Extension method to register library services in DI container (.NET 8 only)
        /// </summary>
        public static IServiceCollection AddMultiTargetLibrary(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(configuration);
            services.AddSingleton<ConfigurationHelper>();
            services.AddScoped<LibraryManager>();
            services.AddScoped<UserService>();
            services.AddHttpContextAccessor();
            
            return services;
        }
#endif

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _userService?.Dispose();
                _logger.LogInformation("LibraryManager disposed");
                _disposed = true;
            }
        }
    }
}
