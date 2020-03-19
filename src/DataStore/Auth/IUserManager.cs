using System.Security.Claims;

namespace DataStore.Auth
{
    public interface IUserManager
    {
        Claim[] GetUser(string userName, string password); 
    }
}