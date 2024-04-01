using Aspose.Words;
using DocumentManagement.Data.Common;
using DocumentManagement.Data.Exceptions;
using DocumentManagement.Data.Repositories;
using ImageMagick;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AsposeDocument = Aspose.Words.Document;
using Document = DocumentManagement.Data.Models.Document;
using SaveFormat = Aspose.Words.SaveFormat;

namespace DocumentManagement.Data.Services
{
    public class DocumentRepository : Repository<Document, DocumentManagementContext>, IDocumentRepository
    {
        private const long MaxSingleFileBytes = 5 * 1024 * 1024;
        private readonly string _rootPath;
        private readonly DocumentManagementContext _context;
        private readonly IAuthContext _authContext;

        public DocumentRepository(string rootPath, IAuthContext authContext, DocumentManagementContext context) : base(context)
        {
            _rootPath = rootPath;
            _context = context;
            _authContext = authContext;
        }

        public async override Task<List<Document>> GetAll()
        {
            return await _context.Documents.ToListAsync();
        }

        public override async Task<Document> Delete(int id)
        {
            var docToDelete = await _context.Documents.SingleAsync(d => d.Id == id);

            var fullPath = Path.Combine(_rootPath, docToDelete.FilePath);

            // Detach file from Application. If file Delete Fails it won't be visible
            _context.Documents.Remove(docToDelete);
            await _context.SaveChangesAsync();

            for (int i = 0; i < 5; i++)
            {
                try
                {
                    File.Delete(fullPath);
                    break;
                }
                catch (IOException)
                {
                    // In case somebody holds the file, try again in a second
                    await Task.Delay(1000);
                }
            }
            return docToDelete;
        }
        public async Task<Document> SaveFile(Stream fileStream, string fileName, bool overwriteFile = false)
        {
            string directory = "SealDocs";

            var relativePath = Path.Combine(directory, fileName);
            var fullPath = Path.Combine(_rootPath, relativePath);

            if (File.Exists(fullPath) && !overwriteFile)
            {
                throw new UserException(
                    "The file already exists in this folder. Please rename the file and try again");
            }

            var document = new Document()
            {
                FilePath = relativePath,
                FileSize = fileStream.Length,
                UploadTime = StaticDateTimeProvider.Now,
                UserId = _authContext.UserId

            };
            UploadAreaThrowHelper(document, fileStream);
            fileStream.Position = 0;

            Directory.CreateDirectory(Path.Combine(_rootPath, directory));

            try
            {
                await using (var file = new FileStream(fullPath, overwriteFile ? FileMode.Create : FileMode.CreateNew))
                {
                    await fileStream.CopyToAsync(file);
                }

                // Overwrite old entry with new Data if it changed
                var oldEntry = await _context.Documents.FirstOrDefaultAsync(x => x.FilePath == relativePath);
                if (oldEntry != null)
                {
                    oldEntry.FileSize = document.FileSize;
                    oldEntry.UploadTime = document.UploadTime;
                    await Update(oldEntry);
                    document = oldEntry;
                }
                else
                {
                    await Add(document);
                }

                await _context.SaveChangesAsync();
                return document;
            }
            catch (Exception ex)
            {
                // Delete partially uploaded, or files that are not known in DB.
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
                throw;
            }
        }

        public async Task<byte[]> GetContentByte(Document document)
        {
            var fullPath = Path.Combine(_rootPath, document.FilePath);
            return await File.ReadAllBytesAsync(fullPath);
        }

        private void UploadAreaThrowHelper(Document document, Stream fileStream)
        {
            // check file extension
            var whiteList = new List<string>() { "pdf", "jpg", "jpeg", "tiff", "tif", "png", "xls", "txt", "doc", "docx" };
            var extension = Path.GetExtension(document.FilePath)?.ToLower();
            if (string.IsNullOrEmpty(extension) || !whiteList.Contains(extension.Remove(0, 1)))
                throw new FileLoadException("Invalid file type");

            if (document.FileSize > MaxSingleFileBytes)
            {
                throw new UserException("The file could not be larger than 5 MB.");
            }

            // throw exception if file header is invalid

            MimeTypes.CheckContentType(fileStream);

        }
        public override Task<InfoDto> Validate(Document entity)
        {
            throw new System.NotSupportedException();
        }

        public async Task<List<Document>> GetUserDocuments(int userId)
        {
            return await _context.Documents.Where(u => u.UserId == userId).ToListAsync();
        }

        public async Task<Document> PublishDocument(int documentId)
        {
            var Publishdoc = await Get(documentId);

            if (Publishdoc == null || Publishdoc.IsDocumentShared)
            {
                throw new UserException("Document is not available or it is already shared");
            }

            Publishdoc.IsDocumentShared = true;
            Publishdoc.PublicDocumentUploadTime = StaticDateTimeProvider.Now;
            return Publishdoc;
        }

        public async Task UpdatePublishDocsExpiry()
        {
            var cutoffTime = StaticDateTimeProvider.Now.AddMinutes(-60);
            var publishedDocs = _context.Documents
                                       .Where(d => d.IsDocumentShared && d.PublicDocumentUploadTime < cutoffTime
                                        && !d.IsExpired)
                                       .ToList();

            if (publishedDocs.Count == 0)
            {
                return;
            }

            foreach (var doc in publishedDocs)
            {
                doc.IsExpired = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<Document>> GetPublishedDocument()
        {
            await UpdatePublishDocsExpiry();

            var publishedDocs = await _context.Documents
                                               .Where(d => d.IsDocumentShared && !d.IsExpired)
                                               .ToListAsync();

            return publishedDocs;
        }


        public Task<string> GeneratePreviewImage(string filePath, string originalFileName)
        {
            string previewImagePath = Path.Combine(Path.GetDirectoryName(filePath), "Previews", Path.GetFileNameWithoutExtension(originalFileName) + ".png");
            string fullFilePath = Path.Combine(_rootPath, filePath);
            string fullPreviewImagePath = Path.Combine(_rootPath, previewImagePath);

            Directory.CreateDirectory(Path.GetDirectoryName(fullPreviewImagePath)); // Ensure preview directory exists

            var extension = Path.GetExtension(originalFileName)?.ToLower();
            switch (extension)
            {
                case ".pdf":
                    ConvertPdfPageToImage(fullFilePath, fullPreviewImagePath);
                    break;

                case ".jpg":
                case ".jpeg":
                case ".png":
                    File.Copy(fullFilePath, fullPreviewImagePath, true);
                    break;

                case ".txt":
                    ConvertTextFileToImage(fullFilePath, fullPreviewImagePath);
                    break;

                case ".docx":
                case ".doc":
                    ConvertToImage(fullFilePath, fullPreviewImagePath);
                    break;
            }

            return Task.FromResult(fullPreviewImagePath);
        }

        private void ConvertPdfPageToImage(string pdfFilePath, string imageOutputPath)
        {
            using (var images = new MagickImageCollection())
            {
                // Settings for reading the first page of the PDF
                var settings = new MagickReadSettings { Density = new Density(300, 300), FrameIndex = 0, FrameCount = 1 };
                images.Read(pdfFilePath, settings);

                if (images.Count > 0)
                {
                    images[0].Write(imageOutputPath);
                }
            }
        }

        private void ConvertTextFileToImage(string textFilePath, string imageOutputPath)
        {
            var text = File.ReadAllLines(textFilePath).Take(20); // Adjust the number of lines as needed
            var readSettings = new MagickReadSettings { BackgroundColor = MagickColors.White, Width = 800, Height = 600 };
            using (var image = new MagickImage($"caption:{string.Join("\n", text)}", readSettings))
            {
                image.Format = MagickFormat.Png;
                image.Write(imageOutputPath);
            }
        }

        public void ConvertToImage(string inputFilePath, string outputImagePath)
        {
            AsposeDocument doc = new AsposeDocument(inputFilePath);
            AsposeDocument tempDoc = new AsposeDocument();

            Section tempSection = new Section(tempDoc);
            Body tempBody = new Body(tempDoc);

            tempSection.AppendChild(tempBody);
            tempDoc.AppendChild(tempSection);

            NodeImporter importer = new NodeImporter(doc, tempDoc, ImportFormatMode.KeepSourceFormatting);

            foreach (Paragraph para in doc.FirstSection.Body.Paragraphs)
            {
                Node clonedPara = importer.ImportNode(para, true);

                tempBody.AppendChild(clonedPara);

                if (para.ParagraphFormat.PageBreakBefore)
                    break; 
            }

            tempDoc.Save(outputImagePath, SaveFormat.Png);
        }
    }
}


