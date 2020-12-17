using CoreRCON.Parsers.Standard;
using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using static ARKDiscordBot.RCONManager;

namespace ARKDiscordBot
{
    class Embeds
    {
        internal static Embed ServerStatus(List<StatusClass> servers)
        {
            var emb = new EmbedBuilder()
            {
                Color = new Color(3, 184, 0),
                Title = "ARK Server Status",
                Description = BuildDescription(servers),
                Footer = new EmbedFooterBuilder().WithText($"Last Updated: {DateTime.Now}")
            };

            string BuildDescription(List<StatusClass> servers)
            {
                string retVal = "";
                foreach (var server in servers)
                {
                    retVal += $"{server.Name} - {(server.Connected ? ":white_check_mark:" : ":x:")} - {server.MapName}" + Environment.NewLine;
                }
                return retVal;
            }

            return emb.Build();
        }

        internal static Embed TeamCurrency(List<Team> Teams)
        {
            var emb = new EmbedBuilder()
            {
                Color = new Color(3, 184, 0),
                Title = "External Map Hours",
                Description = BuildDescription(Teams),
                Footer = new EmbedFooterBuilder().WithText($"Last Updated: {DateTime.Now}")
            };
            return emb.Build();


            string BuildDescription(List<Team> Teams)
            {
                double minutesInHour = 60;
                string retVal = "";
                foreach (var team in Teams)
                {
                    retVal += $"{$"{team.Name} - External Hours Remaining: {team.MinutesRemaining / minutesInHour:0.##} ({team.MinutesRemaining} minutes) - On World: {(string.IsNullOrEmpty(team.OpenMap) ? "None" : team.OpenMap)}"}{Environment.NewLine}";
                }
                return retVal;
            }
        }

        internal static Embed DeathCounter(List<Player> players)
        {
            var emb = new EmbedBuilder()
            {
                Color = new Color(3, 184, 0),
                Title = "Death Counter",
                Description = BuildDescription(players),
                Footer = new EmbedFooterBuilder().WithText($"Last Updated: {DateTime.Now}")
            };
            return emb.Build();

            string BuildDescription(List<Player> players)
            {
                string retVal = "";
                foreach (var player in players)
                {
                    retVal += $"<@{player.DiscordId}> - {player.CharacterName} - **{player.DeathCount}**" + Environment.NewLine;
                }

                return retVal;
            }
        }

        internal static Embed ServerNameDoesntExist(string serverName, IUser user)
        {
            var emb = new EmbedBuilder()
            {
                Color = new Color(220, 20, 60),
                Title = "Command Incorrect",
                Description = $"{user.Mention}. {serverName} is not a valid server name. Please check your spelling.",
                Footer = new EmbedFooterBuilder().WithText($"Requested At: {DateTime.Now}")
            };

            return emb.Build();
        }

        internal static Embed MinutesRemaining(Team team, IGuild guild)
        {
            var emb = new EmbedBuilder()
            {
                Color = new Color(220, 20, 60),
                Title = "Timer Running Low",
                Description = $"{guild.GetRole(team.RoleId).Mention}. You only have {team.MinutesRemaining} minutes left on {team.OpenMap}!.",
                Footer = new EmbedFooterBuilder().WithText($"Sent At: {DateTime.Now}")
            };

            return emb.Build();
        }



        internal static Embed ServerStartupRequested(string serverName, IUser user)
        {
            var emb = new EmbedBuilder()
            {
                Color = new Color(3, 184, 0),
                Title = "ARK Server Startup Requested",
                Description = $"{user.Mention}. {serverName} startup command has been executed.",
                Footer = new EmbedFooterBuilder().WithText($"Requested At: {DateTime.Now}")
            };

            return emb.Build();
        }

        internal static Embed ServerStopRequested(string serverName, IUser user)
        {
            var emb = new EmbedBuilder()
            {
                Color = new Color(3, 184, 0),
                Title = "ARK Server Stop Requested",
                Description = $"{user.Mention}. {serverName} shutdown command has been executed.",
                Footer = new EmbedFooterBuilder().WithText($"Requested At: {DateTime.Now}")
            };

            return emb.Build();
        }

        internal static Embed UserKill(Player plr, Player klr)
        {
            var emb = new EmbedBuilder()
            {
                Color = new Color(220, 20, 60),
                Title = "Player was killed.",
                Description = $"KILLFEED: {klr.CharacterName}(<@{klr.DiscordId}>) killed {plr.CharacterName}(<@{plr.DiscordId}>).",
                Footer = new EmbedFooterBuilder().WithText($"Requested At: {DateTime.Now}")
            };

            return emb.Build();
        }

        internal static Embed TribeNameAlreadyExists(string tribeName)
        {
            var emb = new EmbedBuilder()
            {
                Color = new Color(220, 20, 60),
                Title = $"Team Name Already Exists",
                Description = $"A team with {tribeName} already exists. Could not assign tribe name.",
                Footer = new EmbedFooterBuilder().WithText($"Requested At: {DateTime.Now}")
            };

            return emb.Build();
        }

        internal static Embed UserKill(string chat, bool wasDino)
        {
            var emb = new EmbedBuilder()
            {
                Color = new Color(220, 20, 60),
                Title = $"{(wasDino ? "Dino" : "Player")} was killed.",
                Description = $"KILLFEED: {chat}.",
                Footer = new EmbedFooterBuilder().WithText($"Killed At: {DateTime.Now}")
            };

            return emb.Build();
        }

        internal static Embed NewCharacterRegistered(IUser user, string characterName)
        {
            var emb = new EmbedBuilder()
            {
                Color = new Color(3, 184, 0),
                Title = "New Character Registered",
                Description = $"{user.Mention}. Your character {characterName} was registered in the database.",
                Footer = new EmbedFooterBuilder().WithText($"Total Players Registered: {Storage.GetInstance().Players.Count}")
            };

            return emb.Build();
        }


        internal static Embed NewTeam(IUser user, Team newTeam, IGuild guild)
        {
            var emb = new EmbedBuilder()
            {
                Color = new Color(3, 184, 0),
                Title = "New Team Registered",
                Description = $"{user.Mention}. Your team (ID: {newTeam.Id}) - {newTeam.Name} was registered in the database.",
                Footer = new EmbedFooterBuilder().WithText($"Total Teams Registered: {Storage.GetInstance().Teams.Count}")
            };

            return emb.Build();
        }



        internal static Embed NewTribe(IUser user, Team team)
        {
            var emb = new EmbedBuilder()
            {
                Color = new Color(3, 184, 0),
                Title = "New Tribe Assigned",
                Description = $"{user.Mention}. Your team {team.Name} has been assigned the tribe name {team.TribeName}",
                Footer = new EmbedFooterBuilder().WithText($"Total Teams Registered: {Storage.GetInstance().Teams.Count}")
            };

            return emb.Build();
        }

        internal static Embed MapAlreadyOpen(Team team)
        {
            var emb = new EmbedBuilder()
            {
                Color = new Color(220, 20, 60),
                Title = $"Map Already Open",
                Description = $"{team.Name}, your team already has an open map. Your timer is running down.",
                Footer = new EmbedFooterBuilder().WithText($"Requested At: {DateTime.Now}")
            };

            return emb.Build();
        }

        internal static Embed PlayerJoinedTeam(IGuildUser user, Team team, IGuild guild)
        {
            var emb = new EmbedBuilder()
            {
                Color = new Color(3, 184, 0),
                Title = "Player Joined Team!",
                Description = $"{guild.GetRole(team.RoleId).Mention}. {user.Mention} has joined your team.",
                Footer = new EmbedFooterBuilder().WithText($"Total Teams Registered: {Storage.GetInstance().Teams.Count}")
            };

            return emb.Build();
        }

        internal static Embed TeamOpenedWorld(Team team)
        {
            var emb = new EmbedBuilder()
            {
                Color = new Color(3, 184, 0),
                Title = "Team Opened Map",
                Description = $"{team.Name} has opened {team.OpenMap}. Timer has begun!",
                Footer = new EmbedFooterBuilder().WithText($"Requested at: {DateTime.Now}")
            };

            return emb.Build();
        }

        internal static Embed TeamClosedWorld(Team team)
        {
            var emb = new EmbedBuilder()
            {
                Color = new Color(3, 184, 0),
                Title = "Team Closed Map",
                Description = $"{team.Name} has closed {team.OpenMap}. Timer has stopped!",
                Footer = new EmbedFooterBuilder().WithText($"Requested at: {DateTime.Now}")
            };

            return emb.Build();
        }

        internal static Embed DiscordUserNoPlayer(IUser user)
        {
            var emb = new EmbedBuilder()
            {
                Color = new Color(220, 20, 60),
                Title = $"No Character Registered",
                Description = $"{user.Mention} you do not have a character registered with the bot. Use !registerplayer <characterName>",
                Footer = new EmbedFooterBuilder().WithText($"Total Players Registered: {Storage.GetInstance().Players.Count}")
            };

            return emb.Build();
        }

        internal static Embed UserNotOnTeam(Player plr)
        {
            var emb = new EmbedBuilder()
            {
                Color = new Color(220, 20, 60),
                Title = $"User Not On Team",
                Description = $"<@{plr.DiscordId}> you are not assigned with a team. Use !jointeam <teamId>",
                Footer = new EmbedFooterBuilder().WithText($"Total Players Registered: {Storage.GetInstance().Teams.Count}")
            };

            return emb.Build();
        }

        internal static Embed NoMapOpen(Team team)
        {
            var emb = new EmbedBuilder()
            {
                Color = new Color(220, 20, 60),
                Title = $"No Map Open",
                Description = $"{team.Name} your team does not currently have a map open. Your timer is currently paused.",
                Footer = new EmbedFooterBuilder().WithText($"Requested At: {DateTime.Now}")
            };

            return emb.Build();
        }

        internal static Embed TeamNoTimeLeft(Team team)
        {
            var emb = new EmbedBuilder()
            {
                Color = new Color(220, 20, 60),
                Title = $"No Time Remaining",
                Description = $"{team.Name} your team does not have any time saved up. You can not open an additional world.",
                Footer = new EmbedFooterBuilder().WithText($"Requested At: {DateTime.Now}")
            };

            return emb.Build();
        }

        internal static Embed TeamAlreadyExists(string teamName)
        {
            var emb = new EmbedBuilder()
            {
                Color = new Color(220, 20, 60),
                Title = $"Team Name Already Exists",
                Description = $"A team with {teamName} already exists. Could not create team.",
                Footer = new EmbedFooterBuilder().WithText($"Requested At: {DateTime.Now}")
            };

            return emb.Build();
        }

        internal static Embed UserAlreadyExists(IUser user)
        {
            var emb = new EmbedBuilder()
            {
                Color = new Color(220, 20, 60),
                Title = $"User Already Registered",
                Description = $"{user.Mention}, you already have a character registered with the bot.",
                Footer = new EmbedFooterBuilder().WithText($"Requested At: {DateTime.Now}")
            };

            return emb.Build();
        }


    }
}
