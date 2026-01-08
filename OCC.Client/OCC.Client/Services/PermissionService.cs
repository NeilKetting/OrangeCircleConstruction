using OCC.Client.Infrastructure;
using OCC.Shared.Models;

using OCC.Client.Services.Interfaces;
using OCC.Client.Services.Infrastructure; // If needed

namespace OCC.Client.Services
{
    /// <summary>
    /// Service responsible for determining user access and permissions based on their role.
    /// </summary>
    public class PermissionService : IPermissionService
    {
        private readonly IAuthService _authService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionService"/> class.
        /// </summary>
        /// <param name="authService">The authentication service to retrieve the current user.</param>
        public PermissionService(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Determines if the current user has access to a specific route or feature.
        /// </summary>
        /// <param name="route">The navigation route or feature key to check access for.</param>
        /// <returns>True if the user is authorized; otherwise, false.</returns>
        public bool CanAccess(string route)
        {
            var user = _authService.CurrentUser;
            if (user == null) return false;

            // Admin has access to everything
            if (user.UserRole == UserRole.Admin) return true;

            // Site Manager Access
            // Can access all core operational modules but restricted from User Management
            if (user.UserRole == UserRole.SiteManager)
            {
                return route switch
                {
                    "HealthSafety" => true,
                    "RollCall" => true,
                    "ClockOut" => true,
                    "LeaveApproval" => true,
                    "OvertimeRequest" => true,
                    "OvertimeApproval" => true,
                    "Teams" => true, 
                    "EmployeeManagement" => true,
                    _ => false
                };
            }

            // Office Staff Access
            if (user.UserRole == UserRole.Office)
            {
                return route switch
                {
                    "HealthSafety" => true,
                    "LeaveApproval" => true,
                    "OvertimeRequest" => true,
                    "OvertimeApproval" => true,
                    "EmployeeManagement" => true,
                    _ => false
                };
            }

            // Contractor/Guest Access
            // Restricted access to specific modules (Home, Projects, Time, Calendar)
            // No access to Staff or User Management
            if (user.UserRole == UserRole.ExternalContractor || user.UserRole == UserRole.Guest)
            {
                 return route switch
                {
                    NavigationRoutes.Home => true,
                    NavigationRoutes.Projects => true, 
                    NavigationRoutes.Time => true, 
                    NavigationRoutes.Calendar => true,
                    NavigationRoutes.Notifications => true,
                    NavigationRoutes.StaffManagement => false,
                    "UserManagement" => false,
                    "OvertimeRequest" => false,
                    _ => false
                };
            }

            return false;
        }
    }
}
