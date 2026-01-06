using OCC.Client.Infrastructure;
using OCC.Shared.Models;
using System;

namespace OCC.Client.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IAuthService _authService;

        public PermissionService(IAuthService authService)
        {
            _authService = authService;
        }

        public bool CanAccess(string route)
        {
            var user = _authService.CurrentUser;
            if (user == null) return false;

            // Admin has access to everything
            if (user.UserRole == UserRole.Admin) return true;

            // Site Manager Access
            if (user.UserRole == UserRole.SiteManager)
            {
                return route switch
                {
                    NavigationRoutes.Home => true,
                    NavigationRoutes.Projects => true,
                    NavigationRoutes.StaffManagement => true,
                    NavigationRoutes.Time => true,
                    NavigationRoutes.Calendar => true,
                    NavigationRoutes.Notifications => true,
                    "UserManagement" => false, // No user management
                    _ => false
                };
            }

            // Contractor/Guest Access
            if (user.UserRole == UserRole.ExternalContractor || user.UserRole == UserRole.Guest)
            {
                 return route switch
                {
                    NavigationRoutes.Home => true,
                    NavigationRoutes.Projects => true, // Maybe read-only? Handled in view
                    NavigationRoutes.Time => true, // Can log time
                    NavigationRoutes.Calendar => true,
                    NavigationRoutes.Notifications => true,
                    NavigationRoutes.StaffManagement => false,
                    "UserManagement" => false,
                    _ => false
                };
            }

            return false;
        }
    }
}
