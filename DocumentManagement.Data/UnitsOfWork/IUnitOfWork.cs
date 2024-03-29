using DocumentManagement.Data.Repositories;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DocumentManagement.Data.UnitsOfWork
{
     public interface IUnitOfWork
    {
        public IUserRepository _userRepository { get; }
        public IDocumentRepository _documentRepository { get; }
        public Task<int> SaveChangesAsync();
        public IDbContextTransaction BeginTransaction();
    }
}
