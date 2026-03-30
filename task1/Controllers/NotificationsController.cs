using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using task1.Application.Interfaces;
using task1.Application.Models;
using task1.Models;

namespace task1.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // GET /notifications
        [Authorize(Policy = "ViewNotifications")]
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            if (!TryGetTenantId(out var tenantId))
                return Unauthorized(new ApiResponse<object> { Error = new ApiError { Code = "UNAUTHORIZED", Message = "Tenant context missing from token." } });

            var notifications = await _notificationService.GetAllAsync(tenantId);
            return Ok(new ApiResponse<List<NotificationModel>> { Data = notifications });
        }

        // DELETE /notifications/{id}
        [Authorize(Policy = "DeleteNotifications")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            if (!TryGetTenantId(out var tenantId))
                return Unauthorized(new ApiResponse<object> { Error = new ApiError { Code = "UNAUTHORIZED", Message = "Tenant context missing from token." } });

            var deleted = await _notificationService.DeleteAsync(tenantId, id);
            if (!deleted) return NotFound(new ApiResponse<object> { Error = new ApiError { Code = "NOT_FOUND", Message = "Notification not found." } });
            return Ok(new ApiResponse<object> { Data = null });
        }

        // DELETE /notifications (clear all)
        [Authorize(Policy = "DeleteNotifications")]
        [HttpDelete]
        public async Task<IActionResult> ClearNotifications()
        {
            if (!TryGetTenantId(out var tenantId))
                return Unauthorized(new ApiResponse<object> { Error = new ApiError { Code = "UNAUTHORIZED", Message = "Tenant context missing from token." } });

            await _notificationService.ClearAsync(tenantId);
            return Ok(new ApiResponse<object> { Data = null });
        }

        private bool TryGetTenantId(out Guid tenantId)
        {
            var tenantClaim = User.FindFirst("tenant_id")?.Value;
            return Guid.TryParse(tenantClaim, out tenantId) && tenantId != Guid.Empty;
        }
    }
}
