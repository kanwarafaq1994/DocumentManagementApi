using DocumentManagement.Data.Common;
using DocumentManagement.Data.Security;
using DocumentManagement.Data.UnitsOfWork;
using LogicApi.ContextHandler;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogicApi.Test.AccountController
{
    public class AccountControllerTestBase
    {
        protected Mock<IUnitOfWork> _unitOfWork;
        protected Mock<AttachContext> _attachContext;
        protected Mock<IPasswordHasher> _passwordHasher;
        protected MemoryDbHelper _dbHelper;

        public AccountControllerTestBase()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _attachContext = new Mock<AttachContext>();
            _passwordHasher = new Mock<IPasswordHasher>();
            _dbHelper = new MemoryDbHelper();
        }

        protected LogicApi.Controllers.AccountController CreateAccountController(IUnitOfWork unitOfWork = null, IPasswordHasher passwordHasher = null, AttachContext attachContext = null)
        {
            var mockedAuthContext = CreateMockAuthContext();
            var mockedAttachContext = attachContext ?? CreateMockAttachContext(mockedAuthContext).Object;
            var accountController = new LogicApi.Controllers.AccountController(
                (unitOfWork == null) ? _unitOfWork.Object : unitOfWork,
                _passwordHasher.Object,
                mockedAttachContext
            );

            return accountController;
        }

        protected virtual Mock<IAuthContext> CreateMockAuthContext()
        {
            var mockAuthContext = new Mock<IAuthContext>();
            // Setup any necessary behavior for your mock here
            return mockAuthContext;
        }

        private Mock<AttachContext> CreateMockAttachContext(Mock<IAuthContext> mockAuthContext)
        {
            var appSettings = Options.Create(new AppSettings());
            var mockAttachContext = new Mock<AttachContext>(appSettings, mockAuthContext.Object);
            return mockAttachContext;
        }
    }

}
