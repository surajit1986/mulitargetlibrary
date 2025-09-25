using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Extensions.Logging;
using MultiTargetLibrary.Common;

namespace MultiTargetLibrary.Data
{
    /// <summary>
    /// Generic repository pattern for MongoDB operations
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public class MongoRepository<T> : IDisposable where T : class
    {
        private readonly MongoDbContext _context;
        private readonly IMongoCollection<T> _collection;
        private readonly ILogger<MongoRepository<T>> _logger;
        private bool _disposed = false;

        public MongoRepository(MongoDbContext context, string collectionName, ILogger<MongoRepository<T>>? logger = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _collection = _context.GetCollection<T>(collectionName);
            _logger = logger ?? new NullLogger<MongoRepository<T>>();
        }

        /// <summary>
        /// Get all documents from the collection
        /// </summary>
        public async Task<List<T>> GetAllAsync()
        {
            try
            {
                var result = await _collection.Find(_ => true).ToListAsync();
                _logger.LogInformation("Retrieved {Count} documents from collection", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all documents");
                throw;
            }
        }

        /// <summary>
        /// Get a document by filter
        /// </summary>
        public async Task<T?> GetByFilterAsync(FilterDefinition<T> filter)
        {
            try
            {
                var result = await _collection.Find(filter).FirstOrDefaultAsync();
                _logger.LogInformation("Retrieved document by filter");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document by filter");
                throw;
            }
        }

        /// <summary>
        /// Insert a single document
        /// </summary>
        public async Task InsertOneAsync(T document)
        {
            try
            {
                await _collection.InsertOneAsync(document);
                _logger.LogInformation("Inserted one document");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting document");
                throw;
            }
        }

        /// <summary>
        /// Insert multiple documents
        /// </summary>
        public async Task InsertManyAsync(IEnumerable<T> documents)
        {
            try
            {
                await _collection.InsertManyAsync(documents);
                _logger.LogInformation("Inserted multiple documents");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting multiple documents");
                throw;
            }
        }

        /// <summary>
        /// Update a document
        /// </summary>
        public async Task<bool> UpdateOneAsync(FilterDefinition<T> filter, UpdateDefinition<T> update)
        {
            try
            {
                var result = await _collection.UpdateOneAsync(filter, update);
                _logger.LogInformation("Updated document. Modified count: {ModifiedCount}", result.ModifiedCount);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating document");
                throw;
            }
        }

        /// <summary>
        /// Delete a document
        /// </summary>
        public async Task<bool> DeleteOneAsync(FilterDefinition<T> filter)
        {
            try
            {
                var result = await _collection.DeleteOneAsync(filter);
                _logger.LogInformation("Deleted document. Deleted count: {DeletedCount}", result.DeletedCount);
                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document");
                throw;
            }
        }

        /// <summary>
        /// Count documents matching filter
        /// </summary>
        public async Task<long> CountAsync(FilterDefinition<T> filter)
        {
            try
            {
                var count = await _collection.CountDocumentsAsync(filter);
                _logger.LogInformation("Counted {Count} documents", count);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting documents");
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
                _context?.Dispose();
                _logger.LogInformation("MongoDB repository disposed");
                _disposed = true;
            }
        }
    }
}
