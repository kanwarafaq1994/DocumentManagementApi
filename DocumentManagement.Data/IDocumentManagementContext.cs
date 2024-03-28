using DocumentManagement.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace DocumentManagement.Data
{
   public interface IDocumentManagementContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Document> Documents { get; set; }
        public void Rollback();
    }
}
