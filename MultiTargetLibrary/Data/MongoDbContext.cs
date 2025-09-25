using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MultiTargetLibrary.Common;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MultiTargetLibrary.Data
{
    /// <summary>
    /// MongoDB context that works across .NET Framework 4.8 and .NET 8
    /// </summary>
    public class MongoDbContext : IDisposable
    {
        private readonly IMongoClient _client;
        private readonly IMongoDatabase _database;
        private readonly ILogger<MongoDbContext> _logger;
        private bool _disposed = false;

        public MongoDbContext(string connectionString, string databaseName, ILogger<MongoDbContext>? logger = null)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));
            
            if (string.IsNullOrEmpty(databaseName))
                throw new ArgumentException("Database name cannot be null or empty", nameof(databaseName));

            _logger = logger ?? new NullLogger<MongoDbContext>();
            
            try
            {
                var settings = MongoClientSettings.FromConnectionString(connectionString);
                settings.ServerApi = new ServerApi(ServerApiVersion.V1);
                
                _client = new MongoClient(settings);
                _database = _client.GetDatabase(databaseName);
                
                _logger.LogInformation("MongoDB connection established for database: {DatabaseName}", databaseName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to establish MongoDB connection");
                throw;
            }
        }

        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            if (string.IsNullOrEmpty(collectionName))
                throw new ArgumentException("Collection name cannot be null or empty", nameof(collectionName));

            return _database.GetCollection<T>(collectionName);
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                await _database.RunCommandAsync((Command<MongoDB.Bson.BsonDocument>)"{ping:1}");
                _logger.LogInformation("MongoDB connection test successful");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MongoDB connection test failed");
                return false;
            }
        }

        public async Task<List<string>> GetCollectionNamesAsync()
        {
            try
            {
                var collections = await _database.ListCollectionNamesAsync();
                return await collections.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve collection names");
                throw;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                // MongoDB driver handles client disposal automatically
                _logger.LogInformation("MongoDB context disposed");
                _disposed = true;
            }
        }
    }


}
