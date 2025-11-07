using Microsoft.AspNetCore.SignalR;



namespace IntelliCare.API.Hubs
{
    public class QueueHub : Hub
    {
        public async Task NotifyQueueUpdate(int doctorId)
        {
            await Clients.Group($"doctor-{doctorId}").SendAsync("QueueUpdated", doctorId);
        }

        public override async Task OnConnectedAsync()
        {
            var doctorId = Context.GetHttpContext().Request.Query["doctorId"];
            await Groups.AddToGroupAsync(Context.ConnectionId, $"doctor-{doctorId}");
            await base.OnConnectedAsync();
        }
    }
}
