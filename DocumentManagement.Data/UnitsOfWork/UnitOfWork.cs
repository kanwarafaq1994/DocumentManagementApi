using DocumentManagement.Data.Common;
using DocumentManagement.Data.Repositories;
using DocumentManagement.Data.Security;
using DocumentManagement.Data.Services;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DocumentManagement.Data.UnitsOfWork
{
    public class UnitOfWork : IUnitOfWork
    {

        private DocumentManagementContext _context;

        private readonly IOptions<AppSettings> _appSettings;

        private readonly IPasswordHasher _passwordHasher;

        public IUserRepository _userRepository { get; }

        public IDocumentRepository _documentRepository { get; }

        public IAuthContext AuthContext { get; }
        public UnitOfWork(DocumentManagementContext context,
            IOptions<AppSettings> appSettings, IPasswordHasher passwordHasher, 
            IUserRepository userRepository, IAuthContext authContext)
        {
            _context = context;
            _appSettings = appSettings;
            AuthContext = authContext;
            _userRepository = userRepository;
            _documentRepository = new DocumentRepository(_appSettings.Value.FileServerRoot, _context);
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
