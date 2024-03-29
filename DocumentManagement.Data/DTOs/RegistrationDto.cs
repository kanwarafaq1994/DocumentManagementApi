namespace DocumentManagement.Data.DTOs
{
    public class RegistrationDto: UserBaseDto
    {
        public string ConfirmEmail { get; set; }

        public string ConfirmPassword { get; set; }
    }
}
