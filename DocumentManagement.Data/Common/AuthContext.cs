using DocumentManagement.Data.Models;

namespace DocumentManagement.Data.Common
{
    public class AuthContext : IAuthContext
    {
        public int UserId { get; private set; }
        public bool IsAdmin { get; private set; }
        public string UserFirstName { get; private set; }
        public string UserLastName { get; private set; }

        public void Set(User user)
        {
            UserId = user.Id;
            UserFirstName = user.FirstName;
            UserLastName = user.LastName;
            IsAdmin = user.IsAdmin;
        }
    }

}
