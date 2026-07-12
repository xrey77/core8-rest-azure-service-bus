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

        public ServiceBusPublisher(ServiceBusClient client, IConfiguration configuration, ILogger<ServiceBusPublisher> logger)
        {
            var topicName = configuration["ConnectionStrings:CentralTopicName"] 
                ?? throw new ArgumentNullException(nameof(configuration), "Central topic name is missing.");
            
            _sender = client.CreateSender(topicName);
            _logger = logger;
        }

        public async Task PublishAsync<T>(T messageBody, string subject)
        {
            if (messageBody == null)
            {
                // throw new ArgumentNullException(nameof(messageBody), "Cannot publish a null message body.");
                throw new ArgumentNullException(nameof(messageBody));                
            }

            try
            {
                var jsonPayload = JsonSerializer.Serialize(messageBody);
                var message = new ServiceBusMessage(jsonPayload)
                {
                    Subject = subject
                    // ContentType = "application/json"
                };

                // _logger.LogInformation("Sending message with subject {Subject} to Service Bus.", subject);
                
                await _sender.SendMessageAsync(message);
                _logger.LogInformation("Message successfully published to emulator.");                
            }
            catch (ServiceBusException ex)
            {
                // Validate specific Azure Service Bus errors using the Reason enum
                HandleServiceBusException(ex, subject);
                throw; // Rethrow or map to a custom domain exception depending on your architecture
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Serialization failed for message subject {Subject}.", subject);
                _logger.LogError(ex, "Failed to publish message to Azure Service Bus Emulator.");                
                throw;
            }
            // catch (Exception ex)
            // {
            //     _logger.LogError(ex, "An unexpected error occurred while publishing subject {Subject}.", subject);
            //     throw;
            // }
        }

        private void HandleServiceBusException(ServiceBusException ex, string subject)
        {
            switch (ex.Reason)
            {
                case ServiceBusFailureReason.MessageSizeExceeded:
                    _logger.LogError(ex, "Message payload size is too large for the topic. Subject: {Subject}", subject);
                    break;

                case ServiceBusFailureReason.ServiceTimeout:
                    _logger.LogWarning(ex, "Service Bus timed out. Consider implementing a retry policy. Subject: {Subject}", subject);
                    break;

                case ServiceBusFailureReason.QuotaExceeded:
                    _logger.LogCritical(ex, "Service Bus entity quota exceeded (e.g., topic is full). Subject: {Subject}", subject);
                    break;

                case ServiceBusFailureReason.ServiceCommunicationProblem:
                    _logger.LogError(ex, "Network connectivity issue between application and Service Bus. Subject: {Subject}", subject);
                    break;

                case ServiceBusFailureReason.MessagingEntityNotFound:
                    _logger.LogCritical(ex, "The specified Service Bus topic was not found. Subject: {Subject}", subject);
                    break;

                default:
                    _logger.LogError(ex, "Service Bus error occurred. Reason: {Reason}. Subject: {Subject}", ex.Reason, subject);
                    break;
            }
        }
    }
}


      // "AzureServiceBus": "Endpoint=sb://localhost:5672;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;",      
