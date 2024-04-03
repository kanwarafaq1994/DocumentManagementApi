using DocumentManagement.Data.DTOs;
using DocumentManagement.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DocumentManagement.Data.Common.Extensions
{
    public static class DocumentHelper
    {
        private static string _fileServerRoot;

        // Set the file server root during application startup
        public static void InitializeFileServerRoot(string fileServerRoot)
        {
            _fileServerRoot = fileServerRoot;
        }

        public static DocumentDto ToDto(this Document document)
        {
            if (string.IsNullOrEmpty(_fileServerRoot))
            {
                throw new InvalidOperationException("FileServerRoot is not initialized. Make sure to call InitializeFileServerRoot method before using DocumentHelper.");
            }
            return new DocumentDto()
            {
                Id = document.Id,
                FileSize = document.FileSize,
                FileName = Path.GetFileName(document.FilePath),
                UploadTime = document.UploadTime,
                NumberOfDownloads = document.NumberOfDownloads,
                AbsoluteFilePath = Path.Combine(_fileServerRoot, document.FilePath),
                PreviewImagePath = document.PreviewImagePath,
                UserId = document.UserId,
            };
        }

        public static UserDto ToUserDocumentsDto(this User user)
        {
            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                UserName = $"{user.FirstName}{user.LastName}".ToLower(),
                IsActive = user.IsActive,
                Email = user.Email,
                LastEditedByUserId = user.EditedBy,
                Documents = user.Documents.Select(d => d.ToDto()).ToList()
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
