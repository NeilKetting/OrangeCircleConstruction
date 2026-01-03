using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using OCC.Shared.Models;

namespace OCC.Client.Services
{
    public class MockStaffRepository : IRepository<StaffMember>
    {
        private readonly List<StaffMember> _staff = new()
        {
            new StaffMember { FirstName = "John", LastName = "Builder", Role = StaffRole.Builder, HourlyRate = 150 },
            new StaffMember { FirstName = "Sarah", LastName = "Tiler", Role = StaffRole.Tiler, HourlyRate = 180 },
            new StaffMember { FirstName = "Mike", LastName = "Painter", Role = StaffRole.Painter, HourlyRate = 130 },
            new StaffMember { FirstName = "Dave", LastName = "Electrician", Role = StaffRole.Electrician, HourlyRate = 250 },
            new StaffMember { FirstName = "Pete", LastName = "Plumber", Role = StaffRole.Plumber, HourlyRate = 220 }
        };

        public async Task<IEnumerable<StaffMember>> GetAllAsync()
        {
            return await Task.FromResult(_staff);
        }

        public async Task<StaffMember?> GetByIdAsync(Guid id)
        {
            return await Task.FromResult(_staff.FirstOrDefault(s => s.Id == id));
        }

        public async Task<IEnumerable<StaffMember>> FindAsync(Expression<Func<StaffMember, bool>> predicate)
        {
            return await Task.FromResult(_staff.AsQueryable().Where(predicate).ToList());
        }

        public async Task AddAsync(StaffMember entity)
        {
            _staff.Add(entity);
            await Task.CompletedTask;
        }

        public async Task UpdateAsync(StaffMember entity)
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
