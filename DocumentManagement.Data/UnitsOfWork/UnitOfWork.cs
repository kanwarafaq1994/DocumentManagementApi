using DocumentManagement.Data.Common;
using DocumentManagement.Data.Repositories;
using DocumentManagement.Data.Security;
using DocumentManagement.Data.Services;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace DocumentManagement.Data.UnitsOfWork
{
    public class UnitOfWork : IUnitOfWork
    {

        private readonly DocumentManagementContext _context;
        private readonly IOptions<AppSettings> _appSettings;
        private readonly IAuthContext _authContext;
        private readonly IPasswordHasher _passwordHasher;

        public IUserRepository UserRepository { get; }
        public IDocumentRepository DocumentRepository { get; }
        public IAuthContext AuthContext => _authContext;

        public UnitOfWork(DocumentManagementContext context,
                          IOptions<AppSettings> appSettings,
                          IAuthContext authContext,
                          IPasswordHasher passwordHasher)
        {
            _context = context;
            _appSettings = appSettings;
            _authContext = authContext;
            _passwordHasher = passwordHasher;

            UserRepository = new UserRepository(context, _passwordHasher, _appSettings);
            DocumentRepository = new DocumentRepository(appSettings.Value.FileServerRoot, authContext, context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public IDbContextTransaction BeginTransaction()
        {
            return _context.Database.BeginTransaction();
        }
    }
}
