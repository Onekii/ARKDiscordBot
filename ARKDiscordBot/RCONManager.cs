using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CoreRCON;
using System.Linq;
using System.Net;
using CoreRCON.PacketFormats;
using CoreRCON.Parsers.Standard;
using Discord;

namespace ARKDiscordBot
{
    public class RCONManager
    {
        private readonly Logger _log = LogManager.GetCurrentClassLogger();
        private readonly DiscordSocketClient _client;
        private readonly IConfigurationRoot _config;
        private readonly Timer _timer;
        private readonly CommandHandler _commands;
        private readonly Storage _storage;

        public RCONManager(DiscordSocketClient client, IConfigurationRoot config, CommandHandler commands)
        {
            _client = client;
            _config = config;
            Globals.Servers = new List<StatusClass>();
            LoadServerData();
            _commands = commands;
            _storage = Storage.GetInstance();

            _timer = new Timer(async _ =>
            {
                await CheckServersAvailable();
                await UpdateDeathCounters();
            },
            null, 
            TimeSpan.Zero,
            TimeSpan.FromSeconds(10));
        }

        private async Task UpdateDeathCounters()
        {
            await _commands.UpdateDeathCount(Embeds.DeathCounter(_storage.Players)).ConfigureAwait(false);
        }

        private void LoadServerData()
        {
            foreach (var server in _config.GetSection("servers").GetChildren())
            {
                string name = server.Key;
                string ip = server["ip"];
                string port = server["port"];
                string rconpassword = server["password"];
                string mapname = server["mapname"];

                StatusClass sc = new StatusClass(name, ip, port, rconpassword, mapname, this);
                Globals.Servers.Add(sc);
            }
        }

        private async Task CheckServersAvailable()
        {
            Globals.Servers.Where(x => !x.Connected).ToList().ForEach(x => x.Reconnect());

            await _commands.UpdateServerStatus(Embeds.ServerStatus(Globals.Servers)).ConfigureAwait(false);
        }

        internal async Task ShutdownServerAsync(string serverName)
        {
            StatusClass server = null;
            server = Globals.Servers.Find(x => x.Connected && x.Name == serverName);

            if (server is null)
                return;

            await server.SendSaveAndShutdownCommand().ConfigureAwait(false);
        }

        internal Player GetPlayer(string playerName)
        {
            return _storage.Players.Find(x => x.CharacterName.ToLower() == playerName.ToLower());
        }

        internal async Task SendKillFeed(Player plr, Player klr)
        {
            Embed e = Embeds.UserKill(plr, klr);
            await _commands.WriteKillToChannel(e).ConfigureAwait(false);
        }

        internal async Task SendKillFeed(string fullString, bool wasDino)
        {
            Embed e = Embeds.UserKill(fullString, wasDino);
            await _commands.WriteKillToChannel(e).ConfigureAwait(false);
        }

        internal Team GetTeam(Player plr)
        {
            return _storage.Teams.Find(x => x.Id == plr.TeamId);
        }

        internal List<Team> GetTeams()
        {
            return _storage.Teams;
        }
    }
}
