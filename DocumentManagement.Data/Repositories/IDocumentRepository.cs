using DocumentManagement.Data.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DocumentManagement.Data.Repositories
{
    public interface IDocumentRepository : IRepository<Document>

    {
        Task<Document> SaveFile(Stream fileStream, string fileName,bool overwriteFile = false);
        Task<List<Document>> GetUserDocuments(int userId);
        Task<byte[]> GetContentByte(Document document);
    }

}
