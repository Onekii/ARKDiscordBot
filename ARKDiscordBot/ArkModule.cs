using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARKDiscordBot
{
    public class ArkModule : ModuleBase<SocketCommandContext>
    {
        Logger _log = LogManager.GetCurrentClassLogger();
        IConfigurationRoot _config;
        RCONManager _rconManager;
        Storage _storage;
        CommandHandler _commands;

        public ArkModule(IConfigurationRoot config, RCONManager _rcon, CommandHandler commands)
        {
            _config = config;
            _rconManager = _rcon;
            _storage = Storage.GetInstance();
            _commands = commands;
        }

        [Command("createmessage")]
        public async Task SendEmptyMessage()
        {
            if (_config["misc:deathCounterMessageId"] != "")
                return;

            await Context.Channel.SendMessageAsync("", false, Embeds.ServerStatus(new List<StatusClass>())).ConfigureAwait(false);
        }

        private bool ValidateWorldExists(string serverName)
        {
            var servers = _config.GetSection("servers");
            var server = servers.GetSection(serverName);

            return server.Exists();
        }

        [Command("startserver")]
        public async Task StartupServer(string serverName)
        {
            Embed e = null;
            IUser user = Context.User;

            if (ValidateWorldExists(serverName))
            {
                string startupCommand = _config[$"servers:{serverName}:startupCommand"];

                Process.Start(startupCommand);
                e = Embeds.ServerStartupRequested(serverName, user);
            }
            else
                e = Embeds.ServerNameDoesntExist(serverName, user);

            await Context.Channel.SendMessageAsync("", false, e).ConfigureAwait(false);
            await Context.Message.DeleteAsync().ConfigureAwait(false);
        }

        [Command("stopserver")]
        public async Task StopServer(string serverName)
        {
            Embed e = null;
            IUser user = Context.User;

            if (ValidateWorldExists(serverName))
            {
                await _rconManager.ShutdownServerAsync(serverName);
                e = Embeds.ServerStopRequested(serverName, user);
            }
            else
                e = Embeds.ServerNameDoesntExist(serverName, user);


            await Context.Channel.SendMessageAsync("", false, e).ConfigureAwait(false);
            await Context.Message.DeleteAsync().ConfigureAwait(false);
        }

        [Command("registerplayer")]
        public async Task RegisterPlayer(string steamId, [Remainder]string characterName)
        {
            IUser user = Context.User;

            if (_storage.Players.Any(plr => plr.DiscordId == user.Id))
            {
                await Context.Channel.SendMessageAsync("", false, Embeds.UserAlreadyExists(user)).ConfigureAwait(false);
                return;
            }

            _storage.Players.Add(new Player() { DiscordId = user.Id, CharacterName = characterName, SteamID = steamId });
            await Context.Channel.SendMessageAsync("", false, Embeds.NewCharacterRegistered(user, characterName));
        }

        [Command("createnewteam")]
        public async Task NewTeam (string teamName, [Remainder]string tribeName)
        {
            IUser user = Context.User;

            if (_storage.Teams.Any(x => x.Name == teamName))
            {
                await Context.Channel.SendMessageAsync("", false, Embeds.TeamAlreadyExists(teamName)).ConfigureAwait(false);
                return;
            }

            Team newTeam = new Team();
            newTeam.Id = _storage.Teams.Count + 1;
            newTeam.Name = teamName;
            newTeam.TribeName = tribeName;

            var role = await Context.Guild.CreateRoleAsync(teamName, null, null, false, true, null).ConfigureAwait(false);
            newTeam.RoleId = role.Id;

            _storage.Teams.Add(newTeam);

            await Context.Channel.SendMessageAsync("", false, Embeds.NewTeam(user, newTeam, Context.Guild));
        }

        [Command("registertribe")]
        public async Task RegisterTribe([Remainder]string tribeName)
        {
            IUser user = Context.User;
            Player plr = _storage.Players.Find(x => x.DiscordId == user.Id);
            if (plr is null)
            {
                await Context.Channel.SendMessageAsync("", false, Embeds.DiscordUserNoPlayer(user)).ConfigureAwait(false);
                return;
            }
            Team team = _storage.Teams.Find(x => x.Id == plr.TeamId);
            if (team is null)
            {
                await Context.Channel.SendMessageAsync("", false, Embeds.UserNotOnTeam(plr)).ConfigureAwait(false);
                return;
            }
            if (_storage.Teams.Any(team => team.TribeName == tribeName))
            {
                await Context.Channel.SendMessageAsync("", false, Embeds.TribeNameAlreadyExists(tribeName)).ConfigureAwait(false);
                return;
            }
            team.TribeName = tribeName;

            await Context.Channel.SendMessageAsync("", false, Embeds.NewTribe(user, team));
        }

        [Command("jointeam")]
        public async Task JoinTeam(int id)
        {
            IGuildUser user = (Context.User as IGuildUser);
            Player plr = _storage.Players.Find(x => x.DiscordId == user.Id);

            if (plr is null)
            {
                await Context.Channel.SendMessageAsync("", false, Embeds.DiscordUserNoPlayer(user)).ConfigureAwait(false);
                return;
            }
            Team team = _storage.Teams.Find(x => x.Id == id);
            if (team is null)
            {
                await Context.Channel.SendMessageAsync("", false, Embeds.UserNotOnTeam(plr)).ConfigureAwait(false);
                return;
            }

            IRole guildRole = Context.Guild.GetRole(team.RoleId);

            await user.AddRoleAsync(guildRole).ConfigureAwait(false);

            plr.TeamId = id;

            await Context.Channel.SendMessageAsync("", false, Embeds.PlayerJoinedTeam(user, team, Context.Guild));
        }

        [Command("openworld")]
        public async Task OpenWorld(string serverName)
        {
            IUser user = Context.User;
            Player plr = _storage.Players.Find(x => x.DiscordId == user.Id);
            if (plr is null)
            {
                await Context.Channel.SendMessageAsync("", false, Embeds.DiscordUserNoPlayer(user)).ConfigureAwait(false);
                return;
            }
            Team team = _storage.Teams.Find(x => x.Id == plr.TeamId);
            if (team is null)
            {
                await Context.Channel.SendMessageAsync("", false, Embeds.UserNotOnTeam(plr)).ConfigureAwait(false);
                return;
            }
            if (team.MinutesRemaining <= 0)
            {
                await Context.Channel.SendMessageAsync("", false, Embeds.TeamNoTimeLeft(team)).ConfigureAwait(false);
                return;
            }
            if (!string.IsNullOrEmpty(team.OpenMap))
            {
                await Context.Channel.SendMessageAsync("", false, Embeds.MapAlreadyOpen(team)).ConfigureAwait(false);
                return;
            }
                
            if (ValidateWorldExists(serverName))
            {
                StatusClass server = Globals.Servers.Find(x => x.Name == serverName);
                server.WhiteListTeam(team);
                team.OpenMap = server.MapName;
            }

            await Context.Channel.SendMessageAsync("", false, Embeds.TeamOpenedWorld(team)).ConfigureAwait(false);
            await _commands.UpdateHoursRemaining(Embeds.TeamCurrency(_storage.Teams));
        }

        [Command("closeworld")]
        public async Task CloseWorld()
        {
            IUser user = Context.User;
            Player plr = _storage.Players.Find(x => x.DiscordId == user.Id);
            if (plr is null)
            {
                await Context.Channel.SendMessageAsync("", false, Embeds.DiscordUserNoPlayer(user)).ConfigureAwait(false);

                return;
            }

            Team team = _storage.Teams.Find(x => x.Id == plr.TeamId);
            if (team is null)
            {
                await Context.Channel.SendMessageAsync("", false, Embeds.UserNotOnTeam(plr)).ConfigureAwait(false);
                return;
            }

            if (string.IsNullOrEmpty(team.OpenMap))
            {
                await Context.Channel.SendMessageAsync("", false, Embeds.NoMapOpen(team)).ConfigureAwait(false);
                return;
            }

            await Context.Channel.SendMessageAsync("", false, Embeds.TeamClosedWorld(team));

            StatusClass server = Globals.Servers.Find(x => x.MapName == team.OpenMap);
            server.RemoveTeamFromWhiteList(team);
            await server.KickAllTeamPlayers(team).ConfigureAwait(false);

            team.OpenMap = "";
            await _commands.UpdateHoursRemaining(Embeds.TeamCurrency(_storage.Teams));
        }
    }
}
