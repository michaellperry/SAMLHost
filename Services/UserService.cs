using SAMLHost.Models;

namespace SAMLHost.Services
{
    public class UserService
    {
        private List<User> _users = new List<User>();

        public User Get(string username)
        {
            var existingUser = _users.FirstOrDefault(u => u.Username == username);
            if (existingUser != null)
            {
                return existingUser;
            }
            else
            {
                var newUser = new User { Username = username };
                _users.Add(newUser);
                return newUser;
            }
        }
    }
}