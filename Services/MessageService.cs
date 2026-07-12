using Azure.Identity;
using Microsoft.Extensions.Azure;
using Azure.Messaging.ServiceBus;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace core8_rest_azure_service_bus.Services
{

    public class MessageService
    {
        private readonly ServiceBusClient _client;
        private readonly string _topicName;

        public MessageService(ServiceBusClient client, IConfiguration configuration)
        {
            _client = client;
            // Reads "topic.1" from your config
            _topicName = configuration["ConnectionStrings:CentralTopicName"] ?? "";
        }

        public async Task SendMessageAsync(string messageBody)
        {
            // Create the sender for your specific topic
            ServiceBusSender sender = _client.CreateSender(_topicName);
            
            ServiceBusMessage message = new ServiceBusMessage(messageBody);
            await sender.SendMessageAsync(message);
        }
    }

    
}