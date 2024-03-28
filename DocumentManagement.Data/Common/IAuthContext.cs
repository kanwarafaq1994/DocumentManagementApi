using DocumentManagement.Data.Models;

namespace DocumentManagement.Data.Common
{
    public interface IAuthContext
    {
        bool IsAdmin { get; }
        string UserFirstName { get; }
        string UserLastName { get; }
        int UserId { get; }

        void Set(User user);
    }

}
