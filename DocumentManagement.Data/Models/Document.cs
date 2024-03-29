using DocumentManagement.Data;
using DocumentManagement.Data.Common;
using JetBrains.Annotations;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentManagement.Data.Models
{
    public class Document : IEntity
    {

        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(UserDocument))]
        public int UserId { get; set; }

        public virtual User UserDocument { get; set; }

        [MaxLength(ModelConstants.MaxFilePath)]
        public string FilePath { get; set; }

        public long FileSize { get; set; }

        public DateTime UploadTime { get; set; }

        public string DocumentPublicLinkToken { get; set; }

        public int NumberOfDownloads { get; set; }

        public bool IsExpired { get; set; }
    }
}
