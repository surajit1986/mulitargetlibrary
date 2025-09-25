using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MultiTargetLibrary.Common;
using System;

namespace MultiTargetLibrary.Configuration
{
    /// <summary>
    /// Configuration helper that works across both .NET Framework and .NET 8
    /// </summary>
    public class ConfigurationHelper
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConfigurationHelper> _logger;

        public ConfigurationHelper(IConfiguration configuration, ILogger<ConfigurationHelper>? logger = null)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? new NullLogger<ConfigurationHelper>();
        }

        /// <summary>
        /// Get MongoDB connection string from configuration
        /// </summary>
        public string GetMongoConnectionString(string? keyName = null)
        {
            var key = keyName ?? "ConnectionStrings:MongoDB";
            var connectionString = _configuration[key];
            
            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogWarning("MongoDB connection string not found for key: {Key}", key);
                throw new InvalidOperationException($"MongoDB connection string not found for key: {key}");
            }

            _logger.LogInformation("Retrieved MongoDB connection string for key: {Key}", key);
            return connectionString;
        }

        /// <summary>
        /// Get MongoDB database name from configuration
        /// </summary>
        public string GetMongoDatabaseName(string? keyName = null)
        {
            var key = keyName ?? "MongoDB:DatabaseName";
            var databaseName = _configuration[key];
            
            if (string.IsNullOrEmpty(databaseName))
            {
                _logger.LogWarning("MongoDB database name not found for key: {Key}", key);
                throw new InvalidOperationException($"MongoDB database name not found for key: {key}");
            }

            _logger.LogInformation("Retrieved MongoDB database name for key: {Key}", key);
            return databaseName;
        }
        
    }


}
