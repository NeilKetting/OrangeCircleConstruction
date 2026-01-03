using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using OCC.Shared.Models;

namespace OCC.Client.Services
{
    public class MockAttendanceRepository : IRepository<AttendanceRecord>
    {
        private readonly List<AttendanceRecord> _records = new();

        public async Task<IEnumerable<AttendanceRecord>> GetAllAsync()
        {
            return await Task.FromResult(_records);
        }

        public async Task<AttendanceRecord?> GetByIdAsync(Guid id)
        {
            return await Task.FromResult(_records.FirstOrDefault(r => r.Id == id));
        }

        public async Task<IEnumerable<AttendanceRecord>> FindAsync(Expression<Func<AttendanceRecord, bool>> predicate)
        {
            return await Task.FromResult(_records.AsQueryable().Where(predicate).ToList());
        }

        public async Task AddAsync(AttendanceRecord entity)
        {
            _records.Add(entity);
            await Task.CompletedTask;
        }

        public async Task UpdateAsync(AttendanceRecord entity)
        {
            var existing = _records.FirstOrDefault(r => r.Id == entity.Id);
            if (existing != null)
            {
                var index = _records.IndexOf(existing);
                _records[index] = entity;
            }
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id)
        {
            var existing = _records.FirstOrDefault(r => r.Id == id);
            if (existing != null)
            {
                _records.Remove(existing);
            }
            await Task.CompletedTask;
        }
    }
}
