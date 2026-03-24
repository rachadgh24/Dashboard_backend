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
            var notifications = await _notificationService.GetAllAsync();
            return Ok(new ApiResponse<List<NotificationModel>> { Data = notifications });
        }

        // DELETE /notifications/{id}
        [Authorize(Policy = "DeleteNotifications")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var deleted = await _notificationService.DeleteAsync(id);
            if (!deleted) return NotFound(new ApiResponse<object> { Error = new ApiError { Code = "NOT_FOUND", Message = "Notification not found." } });
            return Ok(new ApiResponse<object> { Data = null });
        }

        // DELETE /notifications (clear all)
        [Authorize(Policy = "DeleteNotifications")]
        [HttpDelete]
        public async Task<IActionResult> ClearNotifications()
        {
            await _notificationService.ClearAsync();
            return Ok(new ApiResponse<object> { Data = null });
        }
    }
}
