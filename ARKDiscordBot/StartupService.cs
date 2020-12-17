using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using NLog;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace ARKDiscordBot
{
    internal class StartupService
    {
        private readonly Logger _log = LogManager.GetCurrentClassLogger();
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;
        private readonly IServiceProvider _services;

        public StartupService(DiscordSocketClient discord, CommandService commands, IConfigurationRoot config, IServiceProvider services)
        {
            _discord = discord;
            _commands = commands;
            _config = config;
            _services = services;
        }

        internal async Task StartAsync()
        {
#if DEBUG
            await _discord.LoginAsync(Discord.TokenType.Bot, _config["tokens:dev"]);
#endif
#if !DEBUG
            await _discord.LoginAsync(Discord.TokenType.Bot, _config["tokens:live"]);
#endif

            _log.Info("Bot is starting up..");
            await _discord.StartAsync();

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }
    }
}