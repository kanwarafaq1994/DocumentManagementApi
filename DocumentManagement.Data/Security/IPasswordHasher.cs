namespace DocumentManagement.Data.Security
{
    public interface IPasswordHasher
    {
        string HashPassword(string plainTextPassword);
        bool VerifyPassword(string storedHashedData, string providedPassword);
    }

}
