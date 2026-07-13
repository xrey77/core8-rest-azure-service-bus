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

string serviceBusConnectionString = builder.Configuration.GetConnectionString("AzureServiceBus") 
    ?? throw new ArgumentNullException(nameof(builder.Configuration), "Unable to connect to AzureServiceBus.");
// Console.WriteLine(connectionString);

var topicName = builder.Configuration["ConnectionStrings:CentralTopicName"];
Console.WriteLine(topicName);
builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddServiceBusClient(serviceBusConnectionString);

    clientBuilder.AddClient<ServiceBusSender, ServiceBusClientOptions>((opts, provider) =>
    {
        var client = provider.GetRequiredService<ServiceBusClient>();
        return client.CreateSender(topicName);
    });
});

builder.Services.AddScoped<IMessagePublisher, ServiceBusPublisher>();



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
