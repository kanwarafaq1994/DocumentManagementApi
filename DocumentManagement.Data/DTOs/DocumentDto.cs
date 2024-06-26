﻿using System;

namespace DocumentManagement.Data.DTOs
{
    public class DocumentDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string AbsoluteFilePath { get; set; }
        public string PreviewImagePath { get; set; }
        public string FileName { get; set; }
        public int NumberOfDownloads { get; set; }
        public long FileSize { get; set; }
        public DateTime UploadTime { get; set; }
    }

}
