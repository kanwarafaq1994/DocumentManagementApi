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

        private DocumentManagementContext _context;

        private readonly IOptions<AppSettings> _appSettings;

        private readonly IPasswordHasher _passwordHasher;

        public IUserRepository userRepository { get; }

        public IDocumentRepository documentRepository { get; }

        public IAuthContext AuthContext { get; }
        public UnitOfWork(DocumentManagementContext context,
            IOptions<AppSettings> appSettings, IPasswordHasher passwordHasher, 
            IUserRepository userRepository, IAuthContext authContext)
        {
            _context = context;
            _appSettings = appSettings;
            AuthContext = authContext;
            this.userRepository = userRepository;
            documentRepository = new DocumentRepository(_appSettings.Value.FileServerRoot,AuthContext, _context);
            _passwordHasher = passwordHasher;
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
