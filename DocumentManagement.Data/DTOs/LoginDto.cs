using System.ComponentModel.DataAnnotations;

namespace DocumentManagement.Data.DTOs
{
    public class LoginDto
    {
        [Required]
        public string Password { get; set; }

        [Required]
        public string Email { get; set; }
    }
}
