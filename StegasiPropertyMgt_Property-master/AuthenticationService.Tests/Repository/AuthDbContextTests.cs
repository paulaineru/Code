using System;
using System.Threading.Tasks;
using AuthenticationService.Repository;
using SharedKernel.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;

namespace AuthenticationService.Tests.Repository
{
    public class AuthDbContextTests : IDisposable
    {
        private readonly AuthDbContext _context;
        private readonly DbContextOptions<AuthDbContext> _options;

        public AuthDbContextTests()
        {
            _options = new DbContextOptionsBuilder<AuthDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AuthDbContext(_options);
        }

        [Fact]
        public async Task AddUser_ShouldSaveUserToDatabase()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hashedpassword"
            };

            // Act
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Assert
            var savedUser = await _context.Users.FindAsync(user.Id);
            savedUser.Should().NotBeNull();
            savedUser.Username.Should().Be(user.Username);
            savedUser.Email.Should().Be(user.Email);
        }

        [Fact]
        public async Task FindUser_ShouldReturnNullForNonExistentUser()
        {
            // Arrange
            var nonExistentUserId = Guid.NewGuid();

            // Act
            var result = await _context.Users.FindAsync(nonExistentUserId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateUser_ShouldModifyExistingUser()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "originaluser",
                Email = "original@example.com",
                PasswordHash = "hashedpassword"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            user.Username = "updateduser";
            user.Email = "updated@example.com";
            await _context.SaveChangesAsync();

            // Assert
            var updatedUser = await _context.Users.FindAsync(user.Id);
            updatedUser.Should().NotBeNull();
            updatedUser.Username.Should().Be("updateduser");
            updatedUser.Email.Should().Be("updated@example.com");
        }

        [Fact]
        public async Task DeleteUser_ShouldRemoveUserFromDatabase()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "tobedeleted",
                Email = "delete@example.com",
                PasswordHash = "hashedpassword"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            // Assert
            var deletedUser = await _context.Users.FindAsync(user.Id);
            deletedUser.Should().BeNull();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
} 