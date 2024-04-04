using DocumentManagement.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DocumentManagement.Test.Common
{
    public static class MemoryDbHelper
    {
        public static DocumentManagementContext GetContext()
        {
            DbContextOptions<DocumentManagementContext> options;
            var builder = new DbContextOptionsBuilder<DocumentManagementContext>();
            builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
            options = builder.Options;

            DocumentManagementContext documentManagementContext = new DocumentManagementContext(options, null);
            documentManagementContext.Database.EnsureDeleted();
            documentManagementContext.Database.EnsureCreated();

            documentManagementContext.SaveChanges();

            return documentManagementContext;
        }
    }
}
