using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ARKDiscordBot
{
    class DannyService
    {
        private readonly Logger _log = LogManager.GetCurrentClassLogger();
        private readonly Timer _timer;
        private readonly CommandHandler _commands;

        public DannyService(CommandHandler commands)
        {
            _commands = commands;

            _timer = new Timer(async _ =>
            {
                await DannyTimer();
            },
            null,
            TimeSpan.Zero,
            TimeSpan.FromMinutes(1));
        }

        private async Task DannyTimer()
        {
            if (DateTime.Now.Minute == 0)
            {
                DateTime dt = new DateTime(2020, 12, 19, 00, 00, 00);

                await _commands.TellDannyToDoWork(dt.Subtract(DateTime.Now.AddSeconds(60 - DateTime.Now.Second)).TotalMinutes / 60).ConfigureAwait(false);
            }
        }
    }
}
