using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace ARKDiscordBot
{
    class Program
    {
        private IConfigurationRoot _config = ConfigService.GetConfiguration();

        static void Main(string[] args) => new Program().StartAsync().GetAwaiter().GetResult();

        public async Task StartAsync()
        {
            var services = new ServiceCollection()
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig { LogLevel = Discord.LogSeverity.Info }))
                .AddSingleton(new CommandService(new CommandServiceConfig
                {
                    DefaultRunMode = RunMode.Async,
                    LogLevel = Discord.LogSeverity.Info
                }))
                .AddSingleton<StartupService>()
                .AddSingleton<CommandHandler>()
                .AddSingleton<LoggingService>()
                .AddSingleton<RCONManager>()
                .AddSingleton<ExternalHoursService>()
                .AddSingleton<DannyService>()
                .AddSingleton(_config);

            var provider = services.BuildServiceProvider();
            provider.GetRequiredService<LoggingService>();
            await provider.GetRequiredService<StartupService>().StartAsync();
            provider.GetRequiredService<CommandHandler>();
            provider.GetRequiredService<RCONManager>();
            provider.GetRequiredService<ExternalHoursService>();
            provider.GetRequiredService<DannyService>();
            await Task.Delay(-1);
        }
    }
}
