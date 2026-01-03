using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using OCC.Shared.Models;

namespace OCC.Client.Services
{
    public class MockUserRepository : IRepository<User>
    {
        private readonly List<User> _users = new()
        {
            new User { Id = Guid.NewGuid(), Email = "a", Password = "a", FirstName = "Admin", LastName = "User", Phone = "011 111 222-3333", UserRole = UserRole.Admin },
            new User { Id = Guid.NewGuid(), Email = "admin@occ.com", Password = "admin", FirstName = "Admin", LastName = "User", Phone = "011 111 222-3333", UserRole = UserRole.Admin },
            new User { Id = Guid.NewGuid(), Email = "vernono@occ.com", Password = "pass", FirstName = "Vernon", LastName = "Steenberg", Phone = "082 000-0000", UserRole = UserRole.SiteManager },
            new User { Id = Guid.NewGuid(), Email = "neil@origize63.co.za", Password = "pass", FirstName = "Neil", LastName = "Ketting", Phone = "082 747-8618", UserRole = UserRole.Guest },
            new User { Id = Guid.NewGuid(), Email = "helga@mdk.co.za", Password = "pass", FirstName = "Helga", LastName = "Ketting", Phone = "082 305-7656", UserRole = UserRole.SiteManager },
        };

        public Task<IEnumerable<User>> GetAllAsync() => Task.FromResult(_users.AsEnumerable());

        public Task<User?> GetByIdAsync(Guid id) => Task.FromResult(_users.FirstOrDefault(u => u.Id == id));

        public Task<IEnumerable<User>> FindAsync(Expression<Func<User, bool>> predicate)
        {
            return Task.FromResult(_users.AsQueryable().Where(predicate).AsEnumerable());
        }

        public Task AddAsync(User entity)
        {
            _users.Add(entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(User entity)
        {
            var existing = _users.FirstOrDefault(u => u.Id == entity.Id);
            if (existing != null)
            {
                _users.Remove(existing);
                _users.Add(entity);
            }
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id)
        {
            var existing = _users.FirstOrDefault(u => u.Id == id);
            if (existing != null) _users.Remove(existing);
            return Task.CompletedTask;
        }
    }
}