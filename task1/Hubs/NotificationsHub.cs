using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace task1.Hubs
{
    [Authorize]
    public class NotificationsHub : Hub
    {
        private const string NotificationsGroup = "notifications";

        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, NotificationsGroup);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, NotificationsGroup);
            await base.OnDisconnectedAsync(exception);
        }
    }
}