using Microsoft.AspNet.SignalR;

namespace cohort.Realtime
{
    public class Notification : Hub
    {
        public void Send(string message)
        {
            Clients.All.addMessage(message);
        }
    }
}