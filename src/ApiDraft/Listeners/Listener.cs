using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;

namespace ApiDraft.Listeners
{
    public class Listener : IHostedService
    {
        private readonly ILogger<Listener> _logger;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly AppSettings _appSettings;
        private readonly ServiceBusProcessor _processor;
        public Listener(
            ILogger<Listener> logger,
            AppSettings appSettings,
            IAzureClientFactory<ServiceBusClient> serviceBusClientFactory)
        {
            _logger = logger;
            _serviceBusClient = serviceBusClientFactory.CreateClient(ServiceBuses.MainServiceBus);
            _processor = _serviceBusClient.CreateProcessor(appSettings.QueueName);
            _appSettings = appSettings;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("{MethodName} Listener starting.", nameof(StartAsync));
            _processor.ProcessMessageAsync += MessageHandler;
            _processor.ProcessErrorAsync += ErrorHandler;
   
            await _processor.StartProcessingAsync(cancellationToken);
        }

       public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("{MethodName} Listener stopping", nameof(StopAsync));

            await _processor.StopProcessingAsync(cancellationToken);

            await _serviceBusClient.DisposeAsync();
        }

        public async Task MessageHandler(ProcessMessageEventArgs args)
        {
            string rawBody = args.Message.Body.ToString();
            args.Message.ApplicationProperties.TryGetValue("EventType", out var rawValue);
            string eventType = rawValue.ToString();

            var node = JsonNode.Parse(rawBody);

            // put more logic here
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            _logger.LogError("{MethodName} {Message} Error while processing event - {exception}", nameof(ErrorHandler), args.ErrorSource.ToString(), args.Exception);

            return Task.CompletedTask;
        }
    }
}