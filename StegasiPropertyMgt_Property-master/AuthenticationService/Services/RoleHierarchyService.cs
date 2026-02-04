using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Models;
using AuthenticationService.Repository;

namespace AuthenticationService.Services
{
    public interface IRoleHierarchyService
    {
        Task AddHierarchyAsync(Guid parentRoleId, Guid childRoleId, int hierarchyLevel = 1);
        Task RemoveHierarchyAsync(Guid parentRoleId, Guid childRoleId);
        Task<List<RoleHierarchy>> GetChildrenAsync(Guid parentRoleId);
        Task<List<RoleHierarchy>> GetParentsAsync(Guid childRoleId);
        Task<List<Guid>> GetAllParentRoleIdsAsync(Guid roleId);
        Task<List<Guid>> GetAllChildRoleIdsAsync(Guid roleId);
    }

    public class RoleHierarchyService : IRoleHierarchyService
    {
        private readonly IAuthDbContext _context;
        public RoleHierarchyService(IAuthDbContext context)
        {
            _context = context;
        }

        public async Task AddHierarchyAsync(Guid parentRoleId, Guid childRoleId, int hierarchyLevel = 1)
        {
            if (parentRoleId == childRoleId)
                throw new InvalidOperationException("A role cannot be its own parent or child.");

            var exists = await _context.RoleHierarchies.AnyAsync(rh => rh.ParentRoleId == parentRoleId && rh.ChildRoleId == childRoleId);
            if (exists)
                throw new InvalidOperationException("Hierarchy already exists.");

            var hierarchy = new RoleHierarchy
            {
                ParentRoleId = parentRoleId,
                ChildRoleId = childRoleId,
                HierarchyLevel = hierarchyLevel,
                CreatedAt = DateTime.UtcNow
            };
            _context.RoleHierarchies.Add(hierarchy);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveHierarchyAsync(Guid parentRoleId, Guid childRoleId)
        {
            var hierarchy = await _context.RoleHierarchies.FirstOrDefaultAsync(rh => rh.ParentRoleId == parentRoleId && rh.ChildRoleId == childRoleId);
            if (hierarchy != null)
            {
                _context.RoleHierarchies.Remove(hierarchy);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<RoleHierarchy>> GetChildrenAsync(Guid parentRoleId)
        {
            return await _context.RoleHierarchies.Where(rh => rh.ParentRoleId == parentRoleId).ToListAsync();
        }

        public async Task<List<RoleHierarchy>> GetParentsAsync(Guid childRoleId)
        {
            return await _context.RoleHierarchies.Where(rh => rh.ChildRoleId == childRoleId).ToListAsync();
        }

        public async Task<List<Guid>> GetAllParentRoleIdsAsync(Guid roleId)
        {
            var parents = new List<Guid>();
            var directParents = await _context.RoleHierarchies.Where(rh => rh.ChildRoleId == roleId).ToListAsync();
            foreach (var parent in directParents)
            {
                parents.Add(parent.ParentRoleId);
                parents.AddRange(await GetAllParentRoleIdsAsync(parent.ParentRoleId));
            }
            return parents.Distinct().ToList();
        }

        public async Task<List<Guid>> GetAllChildRoleIdsAsync(Guid roleId)
        {
            var children = new List<Guid>();
            var directChildren = await _context.RoleHierarchies.Where(rh => rh.ParentRoleId == roleId).ToListAsync();
            foreach (var child in directChildren)
            {
                children.Add(child.ChildRoleId);
                children.AddRange(await GetAllChildRoleIdsAsync(child.ChildRoleId));
            }
            return children.Distinct().ToList();
        }
    }
} 