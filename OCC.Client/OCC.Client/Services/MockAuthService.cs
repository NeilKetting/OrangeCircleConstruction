using System.Linq;
using System.Threading.Tasks;
using OCC.Shared.Models;

namespace OCC.Client.Services
{
    public class MockAuthService : IAuthService
    {
        private readonly IRepository<User> _userRepository;
        private User? _currentUser;

        public MockAuthService(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        public User? CurrentUser => _currentUser;
        public bool IsAuthenticated => _currentUser != null;

        public async Task<bool> LoginAsync(string email, string password)
        {
            var users = await _userRepository.FindAsync(u => u.Email == email && u.Password == password);
            _currentUser = users.FirstOrDefault();
            return IsAuthenticated;
        }

        public async Task<bool> RegisterAsync(User user)
        {
            var existing = await _userRepository.FindAsync(u => u.Email == user.Email);
            if (existing.Any()) return false;

            await _userRepository.AddAsync(user);
            _currentUser = user;
            return true;
        }

        public Task LogoutAsync()
        {
            _currentUser = null;
            return Task.CompletedTask;
        }
    }
}
