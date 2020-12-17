using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace ARKDiscordBot
{
    class ExternalHoursService
    {
        private readonly Logger _log = LogManager.GetCurrentClassLogger();
        private readonly DiscordSocketClient _client;
        private readonly IConfigurationRoot _config;
        private readonly Timer _timer;
        private readonly CommandHandler _commands;
        private readonly Storage _storage;

        public ExternalHoursService(DiscordSocketClient client, IConfigurationRoot config, CommandHandler commands)
        {
            _client = client;
            _config = config;
            _commands = commands;
            _storage = Storage.GetInstance();

            _timer = new Timer(async _ =>
            {
                await CheckExternalHours();
            },
            null,
            TimeSpan.FromMinutes(1),
            TimeSpan.FromMinutes(1));
        }

        private async Task CheckExternalHours()
        {
            _storage.Teams.ForEach(async x =>
            {
                if (!string.IsNullOrEmpty(x.OpenMap))
                {
                    x.MinutesRemaining--;

                    if (x.MinutesRemaining == 30.0d || x.MinutesRemaining == 15.0d || x.MinutesRemaining == 0.0d)
                    {
                        await _commands.SendMinutesRemaining(x).ConfigureAwait(false);
                    }
                }
            });
            _storage.Save();

            await _commands.UpdateHoursRemaining(Embeds.TeamCurrency(_storage.Teams)).ConfigureAwait(false);
        }
    }
}
