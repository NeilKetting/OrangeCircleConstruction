using OCC.Client.Services;
using OCC.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OCC.Client.Services
{
    public class MockProjectRepository : IRepository<Project>
    {
        private readonly List<Project> _projects;

        public MockProjectRepository()
        {
            _projects = new List<Project>
            {
                new Project
                {
                    Name = "Office Complex A",
                    Description = "New office build in Sandton.",
                    Location = "Sandton, JHB",
                    StartDate = DateTime.Now.AddMonths(-2),
                    EndDate = DateTime.Now.AddMonths(10),
                    Status = "Active"
                },
                new Project
                {
                    Name = "Residential Estate B",
                    Description = "Housing development phase 1.",
                    Location = "Pretoria East",
                    StartDate = DateTime.Now.AddMonths(-1),
                    EndDate = DateTime.Now.AddMonths(5),
                    Status = "Active"
                },
                new Project
                {
                    Name = "Mall Renovation C",
                    Description = "Revamping the food court and entrance.",
                    Location = "Rosebank",
                    StartDate = DateTime.Now.AddMonths(-5),
                    EndDate = DateTime.Now.AddDays(-10),
                    Status = "Completed"
                },
                 new Project
                {
                    Name = "Highway Bridge D",
                    Description = "Structural reinforcement of bridge.",
                    Location = "Midrand",
                    StartDate = DateTime.Now.AddMonths(1),
                    EndDate = DateTime.Now.AddMonths(12),
                    Status = "Active"
                },
                new Project
                {
                    Name = "Warehouse E",
                    Description = "Logistics center construction.",
                    Location = "Cape Town",
                    StartDate = DateTime.Now.AddMonths(-3),
                    EndDate = DateTime.Now.AddMonths(3),
                    Status = "OnHold"
                }
            };
        }

        public Task<IEnumerable<Project>> GetAllAsync() => Task.FromResult(_projects.AsEnumerable());

        public Task<Project?> GetByIdAsync(Guid id) => Task.FromResult(_projects.FirstOrDefault(p => p.Id == id));

        public Task<IEnumerable<Project>> FindAsync(Expression<Func<Project, bool>> predicate)
        {
            return Task.FromResult(_projects.AsQueryable().Where(predicate).AsEnumerable());
        }

        public Task AddAsync(Project entity)
        {
            _projects.Add(entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Project entity)
        {
            var existing = _projects.FirstOrDefault(p => p.Id == entity.Id);
            if (existing != null)
            {
                _projects.Remove(existing);
                _projects.Add(entity);
            }
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id)
        {
            var existing = _projects.FirstOrDefault(p => p.Id == id);
            if (existing != null)
            {
                _projects.Remove(existing);
            }
            return Task.CompletedTask;
        }
    }
}
