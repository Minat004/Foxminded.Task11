using Serilog;
using Telegram.Bot;
using TelegramBot.Core.Services;
using TelegramBot.Settings;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config.SetBasePath(Directory.GetCurrentDirectory());
        config.AddJsonFile("appsettings.json");
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        services.Configure<BotConfiguration>(context.Configuration.GetSection(BotConfiguration.Configuration));
        
        services.Configure<PrivatConfiguration>(context.Configuration.GetSection(PrivatConfiguration.Configuration));

        services.AddHttpClient<ExchangeArchiveService>((serviceProvider, client) =>
        {
            client.BaseAddress = new Uri(serviceProvider.GetConfiguration<PrivatConfiguration>().BaseUrl!);
        });
        
        services.AddHttpClient("telegram_bot_client")
            .AddTypedClient<ITelegramBotClient>((httpClient, serviceProvider) =>
            {
                var botConfig = serviceProvider.GetConfiguration<BotConfiguration>();
                TelegramBotClientOptions options = new(botConfig.ApiToken);
                return new TelegramBotClient(options, httpClient);
            });

        services.AddScoped<UpdateHandler>();
        services.AddScoped<ReceiverService>();

        services.AddHostedService<PollingService>();
    })
    .UseSerilog()
    .Build();

await host.RunAsync();