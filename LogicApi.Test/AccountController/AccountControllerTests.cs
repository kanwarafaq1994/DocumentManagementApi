using DocumentManagement.Data.Common;
using DocumentManagement.Data.DTOs;
using DocumentManagement.Data.Models;
using DocumentManagement.Data.Repositories;
using DocumentManagement.Data.Security;
using DocumentManagement.Data.UnitsOfWork;
using LogicApi.ContextHandler;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace LogicApi.Test.AccountController
{
    public class AccountControllerTests : AccountControllerTestBase
    {
        [Fact]
        public async Task Login_should_return_UnAuthorized()
        {
            // Arrange
            var unitOfWork = new UnitOfWork(_dbHelper.GetContext(), Options.Create(new AppSettings()), CreateMockAuthContext().Object, _passwordHasher.Object);

            var mockUserRepository = new Mock<IUserRepository>();
            _unitOfWork.SetupGet(uow => uow.UserRepository).Returns(mockUserRepository.Object);

            var userController = CreateAccountController(unitOfWork);

            var user = new User
            {
                Id = 1,
                FirstName = "Test",
                LastName = "User",
                Email = "testuser1@gmail.com",
                IsActive = true,
                PasswordHash = "Uw7mRJuMS2SnOuiVRy4N6Q==:fp9FDMO1RWwYI520zNo82jIviCv8kdtabcifVxLhvzs=",
                LastActivity = StaticDateTimeProvider.Now,
                Documents = null
            };

            mockUserRepository.Setup(u => u.Add(It.IsAny<User>())).ReturnsAsync(user);

            // Act
            IActionResult result = await userController.Login(new LoginDto
            {
                Email = "email@email.com",
                Password = "Test123!"
            });

            // Assert
            AssertLoginUnauthorized(result);
        }

        private void AssertLoginUnauthorized(IActionResult result)
        {
            Assert.IsType<UnauthorizedObjectResult>(result);

            var objectResult = (ObjectResult)result;
            Assert.Equal(401, objectResult.StatusCode);

            // Assert response properties
            var responseDto = (LoginResponseDto)objectResult.Value;
            Assert.Equal("Invalid email address or password", responseDto.Message);
        }

        [Fact]
        public async Task Login_should_return_UnAuthorized_for_invalid_password()
        {
            // Arrange
            var unitOfWork = new UnitOfWork(_dbHelper.GetContext(), Options.Create(new AppSettings()), CreateMockAuthContext().Object, _passwordHasher.Object);
            var mockPasswordHasher = new Mock<IPasswordHasher>(); // Initialize the mockPasswordHasher

            var mockUserRepository = new Mock<IUserRepository>();

            _unitOfWork.SetupGet(uow => uow.UserRepository).Returns(mockUserRepository.Object);

            var userController = CreateAccountController(unitOfWork);

            var user = new User
            {
                Id = 1,
                FirstName = "Test",
                LastName = "User",
                Email = "testuser1@gmail.com",
                IsActive = true,
                PasswordHash = "Uw7mRJuMS2SnOuiVRy4N6Q==:fp9FDMO1RWwYI520zNo82jIviCv8kdtabcifVxLhvzs=",
                LastActivity = StaticDateTimeProvider.Now,
                Documents = null
            };

            mockUserRepository.Setup(u => u.GetUserByEmail(user.Email)).ReturnsAsync(user);
            mockPasswordHasher.Setup(ph => ph.VerifyPassword(user.PasswordHash, "InvalidPassword123!")).Returns(false);

            // Act
            IActionResult result = await userController.Login(new LoginDto
            {
                Email = user.Email,
                Password = "InvalidPassword123!"
            });

            // Assert
            AssertLoginUnauthorized(result);
        }

        [Fact]
        public async Task Login_should_return_UnAuthorized_for_inactive_user()
        {
            // Arrange
            var unitOfWork = new UnitOfWork(_dbHelper.GetContext(), Options.Create(new AppSettings()), CreateMockAuthContext().Object, _passwordHasher.Object);

            var mockUserRepository = new Mock<IUserRepository>();
            _unitOfWork.SetupGet(uow => uow.UserRepository).Returns(mockUserRepository.Object);

            var userController = CreateAccountController(unitOfWork);

            var inactiveUser = new User
            {
                Id = 1,
                FirstName = "Test",
                LastName = "User",
                Email = "inactiveuser@example.com",
                IsActive = false,
                PasswordHash = "hashedpassword", // Replace with actual hashed password
                LastActivity = StaticDateTimeProvider.Now,
                Documents = null
            };

            mockUserRepository.Setup(u => u.GetUserByEmail(inactiveUser.Email)).ReturnsAsync(inactiveUser);

            // Act
            IActionResult result = await userController.Login(new LoginDto
            {
                Email = inactiveUser.Email,
                Password = "ValidPassword123!"
            });

            // Assert
            AssertLoginUnauthorized(result);
        }

        [Fact]
        public async Task Login_should_return_UnAuthorized_for_nonexistent_user()
        {
            // Arrange
            var unitOfWork = new UnitOfWork(_dbHelper.GetContext(), Options.Create(new AppSettings()), CreateMockAuthContext().Object, _passwordHasher.Object);

            var mockUserRepository = new Mock<IUserRepository>();
            _unitOfWork.SetupGet(uow => uow.UserRepository).Returns(mockUserRepository.Object);

            var userController = CreateAccountController(unitOfWork);

            // No setup for GetUserByEmail(), meaning the user doesn't exist in the database

            // Act
            IActionResult result = await userController.Login(new LoginDto
            {
                Email = "nonexistentuser@example.com",
                Password = "NonExistentPassword123!"
            });

            // Assert
            AssertLoginUnauthorized(result);
        }
    }
}

