using System.Security.Claims;

namespace DataStore.Auth
{
    public class UserManager : IUserManager
    {
        public Claim[] GetUser(string userName, string password)
        {
            // Dummy user manager for now...
            if (userName == "Andrew" && password == "test")
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, userName),
                    new Claim("Admin", "")
                };
                return claims;
            }

            if (userName == "James" && password == "test")
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, userName),                    
                };
                return claims;
            }

            return null;
        }
    }
}