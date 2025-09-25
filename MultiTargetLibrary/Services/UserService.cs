using MultiTargetLibrary.Data;
using MultiTargetLibrary.Configuration;
using MultiTargetLibrary.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MultiTargetLibrary.Services
{
    /// <summary>
    /// Sample service that demonstrates MongoDB operations across both frameworks
    /// </summary>
    public class UserService : IDisposable
    {
        private readonly MongoDbContext _dbContext;
        private readonly MongoRepository<User> _userRepository;
        private readonly ILogger<UserService> _logger;
        private bool _disposed = false;

        public UserService(ConfigurationHelper configHelper, ILogger<UserService>? logger = null)
        {
            _logger = logger ?? new NullLogger<UserService>();
            
            try
            {
                var connectionString = configHelper.GetMongoConnectionString();
                var databaseName = configHelper.GetMongoDatabaseName();
                
                _dbContext = new MongoDbContext(connectionString, databaseName, 
                    logger as ILogger<MongoDbContext>);
                _userRepository = new MongoRepository<User>(_dbContext, "users", 
                    logger as ILogger<MongoRepository<User>>);
                
                _logger.LogInformation("UserService initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize UserService");
                throw;
            }
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        public async Task<User> CreateUserAsync(string name, string email)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Name cannot be null or empty", nameof(name));
            
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException("Email cannot be null or empty", nameof(email));

            var user = new User
            {
                Id = ObjectId.GenerateNewId(),
                Name = name,
                Email = email,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _userRepository.InsertOneAsync(user);
            _logger.LogInformation("Created user with ID: {UserId}", user.Id);
            
            return user;
        }

        /// <summary>
        /// Get user by email
        /// </summary>
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException("Email cannot be null or empty", nameof(email));

            var filter = Builders<User>.Filter.Eq(u => u.Email, email);
            var user = await _userRepository.GetByFilterAsync(filter);
            
            _logger.LogInformation("Retrieved user by email: {Email}, Found: {Found}", email, user != null);
            return user;
        }

        /// <summary>
        /// Update user status
        /// </summary>
        public async Task<bool> UpdateUserStatusAsync(ObjectId userId, bool isActive)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var update = Builders<User>.Update
                .Set(u => u.IsActive, isActive)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _userRepository.UpdateOneAsync(filter, update);
            _logger.LogInformation("Updated user status for ID: {UserId}, Success: {Success}", userId, result);
            
            return result;
        }

        /// <summary>
        /// Delete user by ID
        /// </summary>
        public async Task<bool> DeleteUserAsync(ObjectId userId)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var result = await _userRepository.DeleteOneAsync(filter);
            
            _logger.LogInformation("Deleted user with ID: {UserId}, Success: {Success}", userId, result);
            return result;
        }

        /// <summary>
        /// Get total user count
        /// </summary>
        public async Task<long> GetUserCountAsync()
        {
            var filter = Builders<User>.Filter.Empty;
            var count = await _userRepository.CountAsync(filter);
            
            _logger.LogInformation("Total user count: {Count}", count);
            return count;
        }

        /// <summary>
        /// Test database connectivity
        /// </summary>
        public async Task<bool> TestDatabaseConnectionAsync()
        {
            return await _dbContext.TestConnectionAsync();
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
                _userRepository?.Dispose();
                _dbContext?.Dispose();
                _logger.LogInformation("UserService disposed");
                _disposed = true;
            }
        }
    }

    /// <summary>
    /// User entity model
    /// </summary>
    public class User
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
