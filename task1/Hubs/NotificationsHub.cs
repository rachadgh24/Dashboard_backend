using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace task1.Hubs
{
    [Authorize]
    public class NotificationsHub : Hub
    {
        private const string AdminGroup = "admins";

        public override async Task OnConnectedAsync()
        {
            var role = Context.User?.FindFirst("role")?.Value
                       ?? Context.User?.FindFirst(ClaimTypes.Role)?.Value;

            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, AdminGroup);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, AdminGroup);
            await base.OnDisconnectedAsync(exception);
        }
    }
}