using DocumentManagement.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DocumentManagement.Data.Repositories
{
    public interface IDocumentRepository : IRepository<Document>

    {
        Task<Document> SaveFile(Stream fileStream, string fileName,bool overwriteFile = false);
        Task<byte[]> GetContentByte(Document document);
    }

}
