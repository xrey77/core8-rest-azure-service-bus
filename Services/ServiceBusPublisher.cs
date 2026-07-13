// ServiceBusPublisher.cs
using Azure.Identity;
using Microsoft.Extensions.Azure;
using Azure.Messaging.ServiceBus;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace core8_rest_azure_service_bus.Services
{
    public interface IMessagePublisher
    {
        Task PublishAsync<T>(T messageBody, string subject);
    }

    public class ServiceBusPublisher : IMessagePublisher
    {
        private readonly ServiceBusSender _sender;
        private readonly ILogger<ServiceBusPublisher> _logger;

        // Inject the pre-configured sender straight from the DI container
        public ServiceBusPublisher(ServiceBusSender sender, ILogger<ServiceBusPublisher> logger)
        {
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task PublishAsync<T>(T messageBody, string subject)
        {
            if (messageBody == null)
            {
                throw new ArgumentNullException(nameof(messageBody));                
            }

            try
            {
                var jsonPayload = JsonSerializer.Serialize(messageBody);
                var message = new ServiceBusMessage(jsonPayload)
                {
                    Subject = subject,
                    ContentType = "application/json"
                };

                await _sender.SendMessageAsync(message);
                _logger.LogInformation("Message successfully published.");                
            }
            catch (ServiceBusException ex)
            {
                _logger.LogError(ex, "Service Bus error occurred while publishing message.");
                throw; // Rethrow or handle based on your retry policies
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while publishing message.");
                throw;
            }
        }
    }
}