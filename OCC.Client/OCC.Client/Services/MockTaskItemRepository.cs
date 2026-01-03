using OCC.Client.Services;
using OCC.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OCC.Client.Services
{
    public class MockTaskItemRepository : IRepository<TaskItem>
    {
        private readonly List<TaskItem> _tasks;

        public MockTaskItemRepository()
        {
            _tasks = new List<TaskItem>
            {
                new TaskItem
                {
                    Name = "Foundation Pouring",
                    Description = "Pour concrete for the main building foundation. Ensure proper curing time and temperature monitoring.",
                    PlanedStartDate = DateTime.Now.AddDays(-5),
                    PlanedDueDate = DateTime.Now.AddDays(2),
                    PlanedDurationHours = TimeSpan.FromHours(40),
                    ActualStartDate = DateTime.Now.AddDays(-4),
                    ProjectId = Guid.NewGuid()
                },
                new TaskItem
                {
                    Name = "Structural Framework",
                    Description = "Erect steel beams and columns according to structural engineering plans.",
                    PlanedStartDate = DateTime.Now.AddDays(3),
                    PlanedDueDate = DateTime.Now.AddDays(14),
                    PlanedDurationHours = TimeSpan.FromHours(120),
                    ProjectId = Guid.NewGuid()
                },
                new TaskItem
                {
                    Name = "Electrical Rough-in",
                    Description = "Install conduit and boxes for power and lighting circuits on the first floor.",
                    PlanedStartDate = DateTime.Now.AddDays(15),
                    PlanedDueDate = DateTime.Now.AddDays(20),
                    PlanedDurationHours = TimeSpan.FromHours(60),
                    ProjectId = Guid.NewGuid()
                },
                new TaskItem
                {
                    Name = "Plumbing Installation",
                    Description = "Install main water supply lines and drainage pipes for bathrooms and kitchen.",
                    PlanedStartDate = DateTime.Now.AddDays(18),
                    PlanedDueDate = DateTime.Now.AddDays(25),
                    PlanedDurationHours = TimeSpan.FromHours(80),
                    ProjectId = Guid.NewGuid()
                },
                new TaskItem
                {
                    Name = "Roofing Systems",
                    Description = "Install insulation layers and waterproof membrane for the flat roof section.",
                    PlanedStartDate = DateTime.Now.AddDays(26),
                    PlanedDueDate = DateTime.Now.AddDays(35),
                    PlanedDurationHours = TimeSpan.FromHours(90),
                    ProjectId = Guid.NewGuid()
                }
            };
        }

        public Task<IEnumerable<TaskItem>> GetAllAsync() => Task.FromResult(_tasks.AsEnumerable());

        public Task<TaskItem?> GetByIdAsync(Guid id) => Task.FromResult(_tasks.FirstOrDefault(t => t.Id == id));

        public Task<IEnumerable<TaskItem>> FindAsync(Expression<Func<TaskItem, bool>> predicate)
        {
            return Task.FromResult(_tasks.AsQueryable().Where(predicate).AsEnumerable());
        }

        public Task AddAsync(TaskItem entity)
        {
            _tasks.Add(entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(TaskItem entity)
        {
            var existing = _tasks.FirstOrDefault(t => t.Id == entity.Id);
            if (existing != null)
            {
                _tasks.Remove(existing);
                _tasks.Add(entity);
            }
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id)
        {
            var existing = _tasks.FirstOrDefault(t => t.Id == id);
            if (existing != null)
            {
                _tasks.Remove(existing);
            }
            return Task.CompletedTask;
        }
    }
}
