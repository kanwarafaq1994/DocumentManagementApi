using DocumentManagement.Data.Common;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentManagement.Data.Models
{
    [Table("Users")]
    public class User : IEntity
    {

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(ModelConstants.StandardStringLength)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(ModelConstants.StandardStringLength)]
        public string LastName { get; set; }

        [Required]
        public int FailedLoginCount { get; set; }

        [Required]
        public bool IsActive { get; set; }

        [MaxLength(ModelConstants.StandardStringLength)]
        public string Email { get; set; }

        [MaxLength(ModelConstants.StandardStringLength)]
        public string PasswordHash { get; set; }

        [CanBeNull]
        public DateTime? LastActivity { get; set; }

        [CanBeNull]
        public DateTime? CreatedOn { get; set; }

        [CanBeNull]
        public int? CreatedBy { get; set; }

        [CanBeNull]
        public DateTime? EditedOn { get; set; }

        [CanBeNull]
        public int? EditedBy { get; set; }

        public bool IsAdmin { get; set; }

        public virtual List<Document> Documents { get; set; } = new List<Document>();

        public bool IsLoggedIn()
        {
            return LastActivity.HasValue && LastActivity >= StaticDateTimeProvider.InactiveTimeLimit;
        }
    }
}


