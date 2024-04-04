using DocumentManagement.Data.Common;
using DocumentManagement.Data.DTOs;
using DocumentManagement.Data.Models;
using DocumentManagement.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace LogicApi.Test.AccountController
{
    public class RegistrationTests : AccountControllerTestBase
    {
        [Fact]
        public async Task Registration_should_return_BadRequest_when_email_and_confirmEmail_do_not_match()
        {
            // Arrange
            var userController = CreateAccountController();
            var registrationDto = new RegistrationDto
            {
                Email = "test@example.com",
                ConfirmEmail = "mismatch@example.com", 
                FirstName = "Test",
                LastName = "User",
                Password = "ValidPassword123!"
            };

            // Act
            IActionResult result = await userController.Registration(registrationDto);

            // Assert
            AssertBadRequest(result, "Email address and confirm Email address must match.");
        }

        [Fact]
        public async Task Registration_should_return_BadRequest_when_user_data_invalid()
        {
            // Arrange
            var validationErrors = new List<string> { "Invalid email address format", "First name is required" };
            MockUserRepositoryWithValidationErrors(validationErrors);
            var userController = CreateAccountController();
            var registrationDto = new RegistrationDto
            {
                Email = "invalidemail",
                ConfirmEmail = "invalidemail",
                FirstName = "",
                LastName = "User",
                Password = "ValidPassword123!"
            };

            // Act
            IActionResult result = await userController.Registration(registrationDto);

            // Assert
            AssertBadRequestWithErrors(result, validationErrors);
        }

        [Fact]
        public async Task Registration_should_return_Ok_when_registration_successful()
        {
            // Arrange
            MockUserRepositoryWithNoValidationErrors();
            var userController = CreateAccountController();
            var registrationDto = new RegistrationDto
            {
                Email = "newuser@example.com",
                ConfirmEmail = "newuser@example.com",
                FirstName = "Test",
                LastName = "User",
                Password = "ValidPassword123!"
            };

            // Act
            IActionResult result = await userController.Registration(registrationDto);

            // Assert
            AssertOk(result, "Registration successful.");
        }

        private void MockUserRepositoryWithValidationErrors(List<string> errors)
        {
            var mockUserRepository = new Mock<IUserRepository>();
            mockUserRepository.Setup(u => u.Validate(It.IsAny<User>())).ReturnsAsync(new InfoDto { Message = errors });
            _unitOfWork.SetupGet(uow => uow.UserRepository).Returns(mockUserRepository.Object);
        }

        private void MockUserRepositoryWithNoValidationErrors()
        {
            var mockUserRepository = new Mock<IUserRepository>();
            mockUserRepository.Setup(u => u.Validate(It.IsAny<User>())).ReturnsAsync(new InfoDto());
            _unitOfWork.SetupGet(uow => uow.UserRepository).Returns(mockUserRepository.Object);
        }

        private void AssertBadRequest(IActionResult result, string errorMessage)
        {
            Assert.IsType<BadRequestObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(400, objectResult.StatusCode);
            var responseDto = (InfoDto)objectResult.Value;
            Assert.Equal(errorMessage, responseDto.Message[0]);
        }

        private void AssertBadRequestWithErrors(IActionResult result, List<string> errors)
        {
            Assert.IsType<BadRequestObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(400, objectResult.StatusCode);
            var responseDto = (List<string>)objectResult.Value;
            Assert.Equal(errors.Count, responseDto.Count);
            Assert.Equal(errors, responseDto);
        }

        private void AssertOk(IActionResult result, string successMessage)
        {
            Assert.IsType<OkObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(200, objectResult.StatusCode);
            var responseDto = (InfoDto)objectResult.Value;
            Assert.Equal(successMessage, responseDto.Message[0]);
        }
    }
}
