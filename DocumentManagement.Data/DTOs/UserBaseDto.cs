namespace DocumentManagement.Data.DTOs
{
    public class UserBaseDto
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public bool IsActive { get; set; }

        public string Email { get; set; }

        public int? LastEditedByUserId { get; set; }
    }
}
