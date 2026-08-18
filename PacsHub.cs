using Microsoft.AspNetCore.SignalR;

namespace AkıllıPacs
{
    public class PacsHub : Hub
    {
        public async Task SendEmergencyAlert(string patientName, string modality)
        {
            await Clients.All.SendAsync("ReceiveEmergencyAlert", patientName, modality);
        }
    }
}