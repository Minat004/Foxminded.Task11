using TelegramBot;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config.SetBasePath(Directory.GetCurrentDirectory());
        config.AddJsonFile("appsettings.json");
        config.AddEnvironmentVariables();
    })
    .ConfigureServices(services =>
    {
        services.AddTransient<App>();
    })
    .Build();

var app = host.Services.GetRequiredService<App>();

await app.RunAsync();