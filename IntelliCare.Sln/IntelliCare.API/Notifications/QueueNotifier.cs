using IntelliCare.API.Hubs;
using IntelliCare.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace IntelliCare.API.Notifications
{
    public class QueueNotifier : IQueueNotifier
    {
        private readonly IHubContext<QueueHub> _hub;

        public QueueNotifier(IHubContext<QueueHub> hub)
        {
            _hub = hub;
        }

        public async Task NotifyQueueUpdateAsync(int doctorId)
        {
            await _hub.Clients.Group($"doctor-{doctorId}")
                .SendAsync("QueueUpdated", doctorId);
        }
    }
}
