using DocumentManagement.Data.Common;
using DocumentManagement.Data.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DocumentManagement.Data.Repositories
{
    public interface IDocumentRepository : IRepository<Document>

    {
        Task<(Document document, bool uploaded)> SaveFile(Stream fileStream, string fileName, bool overwriteFile = false);
        Task<User> GetUserDocuments(int userId);
        Task<List<Document>> GetPublishedDocument();
        Task UpdatePublishDocsExpiry();
        Task<Document> PublishDocument(int documentId);
        Task<string> GeneratePreviewImage(string filePath, string originalFileName);
        Task<byte[]> GetContentByte(Document document);
    }

}
