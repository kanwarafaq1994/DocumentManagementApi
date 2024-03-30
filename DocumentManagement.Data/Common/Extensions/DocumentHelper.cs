using DocumentManagement.Data.DTOs;
using DocumentManagement.Data.Models;
using System.Collections.Generic;
using System.IO;

namespace DocumentManagement.Data.Common.Extensions
{
    public static class DocumentHelper
    {
        public static DocumentDto ToDto(this Document document)
        {
            return new DocumentDto()
            {
                Id = document.Id,
                FileSize = document.FileSize,
                FileName = Path.GetFileName(document.FilePath),
                UploadTime = document.UploadTime,
                UserId = document.UserId,
                IsExpired = document.IsExpired
            };
        }

        public static DocumentDto[] ToDtoArray(this IEnumerable<Document> documents)
        {
            var dtos = new List<DocumentDto>();

            foreach (var document in documents)
            {
                dtos.Add(document.ToDto());
            }

            return dtos.ToArray();
        }
    }
}
