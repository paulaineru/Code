using System;
using System.Threading.Tasks;
using AuthenticationService.Services;
using AuthenticationService.Repository;
using SharedKernel.Models;
using Moq;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;
using System.Collections.Generic;

namespace AuthenticationService.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<AuthDbContext> _mockDbContext;
        private readonly UserService _userService;
        private readonly Mock<DbSet<User>> _mockUsersSet;

        public UserServiceTests()
        {
            _mockDbContext = new Mock<AuthDbContext>();
            _mockUsersSet = new Mock<DbSet<User>>();
            _userService = new UserService(_mockDbContext.Object);
        }

        [Fact]
        public async Task GetUserEmailByIdAsync_UserExists_ReturnsEmail()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var expectedEmail = "test@example.com";
            var user = new User { Id = userId, Email = expectedEmail };

            var mockSet = new Mock<DbSet<User>>();
            mockSet.Setup(x => x.FindAsync(userId))
                  .ReturnsAsync(user);

            _mockDbContext.Setup(x => x.Users)
                        .Returns(mockSet.Object);

            // Act
            var result = await _userService.GetUserEmailByIdAsync(userId);

            // Assert
            result.Should().Be(expectedEmail);
        }

        [Fact]
        public async Task GetUserEmailByIdAsync_UserNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var mockSet = new Mock<DbSet<User>>();
            mockSet.Setup(x => x.FindAsync(userId))
                  .ReturnsAsync((User)null);

            _mockDbContext.Setup(x => x.Users)
                        .Returns(mockSet.Object);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _userService.GetUserEmailByIdAsync(userId)
            );
        }

        [Fact]
        public async Task GetUserByUsernameAsync_UserExists_ReturnsUser()
        {
            // Arrange
            var username = "testuser";
            var expectedUser = new User { Username = username };

            var mockSet = new Mock<DbSet<User>>();
            var users = new List<User> { expectedUser }.AsQueryable();

            mockSet.As<IQueryable<User>>()
                  .Setup(m => m.Provider)
                  .Returns(users.Provider);
            mockSet.As<IQueryable<User>>()
                  .Setup(m => m.Expression)
                  .Returns(users.Expression);
            mockSet.As<IQueryable<User>>()
                  .Setup(m => m.ElementType)
                  .Returns(users.ElementType);
            mockSet.As<IQueryable<User>>()
                  .Setup(m => m.GetEnumerator())
                  .Returns(users.GetEnumerator());

            _mockDbContext.Setup(x => x.Users)
                        .Returns(mockSet.Object);

            // Act
            var result = await _userService.GetUserByUsernameAsync(username);

            // Assert
            result.Should().BeEquivalentTo(expectedUser);
        }

        [Fact]
        public async Task GetUserByUsernameAsync_UserNotFound_ReturnsNull()
        {
            // Arrange
            var username = "nonexistentuser";
            var mockSet = new Mock<DbSet<User>>();
            var users = new List<User>().AsQueryable();

            mockSet.As<IQueryable<User>>()
                  .Setup(m => m.Provider)
                  .Returns(users.Provider);
            mockSet.As<IQueryable<User>>()
                  .Setup(m => m.Expression)
                  .Returns(users.Expression);
            mockSet.As<IQueryable<User>>()
                  .Setup(m => m.ElementType)
                  .Returns(users.ElementType);
            mockSet.As<IQueryable<User>>()
                  .Setup(m => m.GetEnumerator())
                  .Returns(users.GetEnumerator());

            _mockDbContext.Setup(x => x.Users)
                        .Returns(mockSet.Object);

            // Act
            var result = await _userService.GetUserByUsernameAsync(username);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AddUserAsync_ValidUser_SavesChanges()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), Username = "testuser", Email = "test@example.com" };
            var mockSet = new Mock<DbSet<User>>();

            _mockDbContext.Setup(x => x.Users)
                        .Returns(mockSet.Object);
            _mockDbContext.Setup(x => x.SaveChangesAsync(default))
                        .ReturnsAsync(1);

            // Act
            await _userService.AddUserAsync(user);

            // Assert
            mockSet.Verify(x => x.AddAsync(user, default), Times.Once);
            _mockDbContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
        }
    }
} 