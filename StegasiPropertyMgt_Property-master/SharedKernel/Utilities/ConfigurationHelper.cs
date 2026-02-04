// SharedKernel/Utilities/ConfigurationHelper.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using SharedKernel.Services;
using SharedKernel.Models;
using System;
using System.Threading.Tasks;

namespace SharedKernel.Utilities
{

    public static class ConfigurationHelper
    {
        private static readonly Lazy<string> _adminEmail = new(() =>
        {
            var config = GetConfiguration();
            return config["AdminEmail"] ?? throw new InvalidOperationException("AdminEmail not configured.");
        });

        public static string GetAdminEmailFromConfiguration()
        {
            return _adminEmail.Value;
        }
        /// <summary>
        /// Retrieves the email of the property manager for a given property.
        /// </summary>
        public static async Task<string> GetPropertyManagerEmailForProperty(IUserService userService, Guid propertyManagerId)
        {
            if (propertyManagerId == Guid.Empty)
            {
                throw new KeyNotFoundException("No property manager is assigned to this property.");
            }

            var user = await userService.GetUserByIdAsync(propertyManagerId);
            if (user == null)
                throw new KeyNotFoundException("Property manager not found.");
            return user.Email;
        }
        public static async Task<string> GetPropertyManagerEmailForLease(IUserService userService, Guid leaseId)
        {
            // Fetch property details using leaseId (assuming leaseId references a property)
            var property = await GetPropertyForLeaseAsync(leaseId);

            if (property == null || string.IsNullOrEmpty(property.PropertyManagerId))
            {
                throw new KeyNotFoundException("No property manager is assigned to this lease.");
            }

            var user = await userService.GetUserByIdAsync(Guid.Parse(property.PropertyManagerId));
            if (user == null)
                throw new KeyNotFoundException("Property manager not found.");
            return user.Email;
        }

        private static async Task<Property> GetPropertyForLeaseAsync(Guid leaseId)
        {
            // Implement logic to fetch property based on leaseId (e.g., via an HTTP call to PropertyManagementService)
            throw new NotImplementedException("GetPropertyForLeaseAsync must be implemented.");
        }
        
        public static async Task<bool> HasPropertyManagerAccess(Property property, string userId)
        {
            if (string.IsNullOrEmpty(property.PropertyManagerId))
                return false;

            return property.PropertyManagerId == userId;
        }

        public static async Task<bool> IsPropertyManager(string userId)
        {
            return !string.IsNullOrEmpty(userId);
        }

        public static async Task<bool> HasPropertyAccess(Property property, string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return false;

            if (await HasPropertyManagerAccess(property, userId))
                return true;

            return property.OwnerId.ToString() == userId;
        }

        public static async Task<bool> HasPropertyOwnerAccess(Property property, string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return false;

            return property.OwnerId.ToString() == userId;
        }

        private static IConfiguration GetConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            return builder.Build();
        }

        public static string GetConnectionString(string name)
        {
            var configuration = GetConfiguration();
            return configuration.GetConnectionString(name) ?? string.Empty;
        }

        public static string GetValue(string key)
        {
            var configuration = GetConfiguration();
            return configuration[key] ?? string.Empty;
        }
    }
}
