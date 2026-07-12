// Program.cs
using Azure.Identity;
using Microsoft.Extensions.Azure;
using Azure.Messaging.ServiceBus;

using core8_rest_azure_service_bus.Services;

var builder = WebApplication.CreateBuilder(args);

// 3. Register your custom publisher (ensure this comes AFTER the client registration)
builder.Services.AddSingleton<IMessagePublisher, ServiceBusPublisher>();


// 1. Register Services via Extension Methods
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddCustomCors();
builder.Services.AddCustomAuthentication(builder.Configuration);

// var options = new ServiceBusClientOptions
// {
//     TransportType = ServiceBusTransportType.AmqpWebSockets
// };

// string connectionString = builder.Configuration.GetConnectionString("AzureServiceBus")
//                 ?? throw new ArgumentNullException(nameof(builder.Configuration), "Central topic name is missing.");

// var client = new ServiceBusClient(connectionString, options);


string connectionString = builder.Configuration.GetConnectionString("AzureServiceBus") 
    ?? throw new ArgumentNullException(nameof(builder.Configuration), "Unable to connect to AzureServiceBus.");
    // ?? builder.Configuration["ConnectionStrings:AzureServiceBus"];

// 2. Register ServiceBusClient with emulator options
// builder.Services.AddSingleton(sp =>
// {
//     var options = new ServiceBusClientOptions
//     {
//         // Must use WebSockets to connect to the local emulator container
//         TransportType = ServiceBusTransportType.AmqpWebSockets
//     };

//     return new ServiceBusClient(connectionString, options);
// });


// var clientOptions = new ServiceBusClientOptions
// {
//     TransportType = ServiceBusTransportType.AmqpWebSockets
// };

// // Fixes CS1061 by using the correct root-level callback property
// clientOptions.CertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => 
// {
//     // For local emulator development, bypass certificate validation check
//     return true; 
// };

builder.Services.AddAzureClients(clientBuilder =>
{
    // Fetch your connection string
    var connectionString = builder.Configuration.GetConnectionString("AzureServiceBus");

    clientBuilder.AddServiceBusClient(connectionString)
        .ConfigureOptions(options =>
        {
            // The emulator only supports TCP, not WebSockets
            options.TransportType = ServiceBusTransportType.AmqpTcp; 
            
            // Bypass SSL certificate check for local development
            options.CustomEndpointAddress = new Uri("sb://localhost"); 
        });
});





var app = builder.Build();

// 2. Configure HTTP Request Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// CORS must sit before Authentication/Authorization
app.UseCors(); 

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
