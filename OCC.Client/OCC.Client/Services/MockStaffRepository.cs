using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using OCC.Shared.Models;

namespace OCC.Client.Services
{
    public class MockStaffRepository : IRepository<Employee>
    {
        private readonly List<Employee> _staff = new()
        {
            new Employee { FirstName = "John", LastName = "Builder", Role = EmployeeRole.Builder, HourlyRate = 150 },
            new Employee { FirstName = "Sarah", LastName = "Tiler", Role = EmployeeRole.Tiler, HourlyRate = 180 },
            new Employee { FirstName = "Mike", LastName = "Painter", Role = EmployeeRole.Painter, HourlyRate = 130 },
            new Employee { FirstName = "Dave", LastName = "Electrician", Role = EmployeeRole.Electrician, HourlyRate = 250 },
            new Employee { FirstName = "Pete", LastName = "Plumber", Role = EmployeeRole.Plumber, HourlyRate = 220 }
        };

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            return await Task.FromResult(_staff);
        }

        public async Task<Employee?> GetByIdAsync(Guid id)
        {
            return await Task.FromResult(_staff.FirstOrDefault(s => s.Id == id));
        }

        public async Task<IEnumerable<Employee>> FindAsync(Expression<Func<Employee, bool>> predicate)
        {
            return await Task.FromResult(_staff.AsQueryable().Where(predicate).ToList());
        }

        public async Task AddAsync(Employee entity)
        {
            _staff.Add(entity);
            await Task.CompletedTask;
        }

        public async Task UpdateAsync(Employee entity)
        {
            var existing = _staff.FirstOrDefault(s => s.Id == entity.Id);
            if (existing != null)
            {
                var index = _staff.IndexOf(existing);
                _staff[index] = entity;
            }
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id)
        {
            var existing = _staff.FirstOrDefault(s => s.Id == id);
            if (existing != null)
            {
                _staff.Remove(existing);
            }
            await Task.CompletedTask;
        }
    }
}
