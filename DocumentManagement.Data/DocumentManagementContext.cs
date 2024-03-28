using DocumentManagement.Data.Common;
using DocumentManagement.Data.DataSeeding;
using DocumentManagement.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace DocumentManagement.Data
{
    public class DocumentManagementContext: DbContext, IDocumentManagementContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Document> Documents { get; set; }
        private List<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry> ChangedEntries => this.ChangeTracker.Entries().Where(x => x.State != EntityState.Unchanged).ToList();

        private string _connectionString;
        private readonly IAuthContext _authContext;

        public DocumentManagementContext(DbContextOptions<DocumentManagementContext> options, IAuthContext authContext) : base(options)
        {
            _authContext = authContext ?? new AuthContext();
        }
        public DocumentManagementContext() : base()
        {
            //This constructor is used for migrations only
            _connectionString = "server=localhost;database=DocumentManagement;Integrated Security=SSPI";
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!string.IsNullOrEmpty(_connectionString))
            {
                //This is used for migrations
                optionsBuilder.UseSqlServer(_connectionString, options => options.EnableRetryOnFailure()).EnableSensitiveDataLogging();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Seed();
        }

        public void Rollback()
        {
            foreach (var entry in ChangedEntries)
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                        entry.CurrentValues.SetValues(entry.OriginalValues);
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    case EntityState.Deleted:
                        entry.State = EntityState.Unchanged;
                        break;
                }
            }
        }

    }
}
