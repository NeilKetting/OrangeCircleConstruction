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

            // Special Check for Wage Visibility
            if (route == "WageViewing")
            {
                // Only Admin can see wages
                return false;
            }

            // Office Staff Access
            if (user.UserRole == UserRole.Office)
            {
                return route switch
                {
                    "Home" => true, // Home is essential
                    "Time" => true, // Restore Time Access
                    "HealthSafety" => true,
                    "LeaveApproval" => true,
                    "OvertimeRequest" => true,
                    "OvertimeApproval" => true,
                    "Orders" => true,
                    // "EmployeeManagement" => false, // Now Restricted
                    _ => false
                };
            }

            // Site Manager Access
            if (user.UserRole == UserRole.SiteManager)
            {
                // Site Manager also loses EmployeeManagement based on user request
                return route switch
                {
                    "Home" => true, 
                    "Time" => true, // Restore Time Access
                    "HealthSafety" => true,
                    "RollCall" => true,
                    "ClockOut" => true,
                    "LeaveApproval" => true,
                    "OvertimeRequest" => true,
                    "OvertimeApproval" => true,
                    "Teams" => true, 
                    // "EmployeeManagement" => false, // Now Restricted
                    _ => false
                };
            }

            // Contractor/Guest Access
            // ... (No change needed, valid logic)
            if (user.UserRole == UserRole.ExternalContractor || user.UserRole == UserRole.Guest)
            {
                 return route switch
                {
                    NavigationRoutes.Home => true,
                    NavigationRoutes.Projects => true, 
                    NavigationRoutes.Time => true, 
                    NavigationRoutes.Calendar => true,
                    NavigationRoutes.Notifications => true,
                    _ => false
                };
            }

            return false;
        }
    }
}
