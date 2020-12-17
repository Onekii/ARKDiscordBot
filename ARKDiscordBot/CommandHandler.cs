using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ARKDiscordBot
{
    public class CommandHandler
    {
        private readonly Logger _log = LogManager.GetCurrentClassLogger();
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;
        private readonly IServiceProvider _provider;
        private IMessage _message;
        private IMessage _hoursMessage;
        private IMessage _deathCountMessage;
        private IGuild _guild;
        private ITextChannel _logChannel;

        public CommandHandler(DiscordSocketClient discord, CommandService commands, IConfigurationRoot config, IServiceProvider services)
        {
            _client = discord;
            _commands = commands;
            _config = config;
            _provider = services;

            _client.MessageReceived += OnMessageReceieved;
            _client.GuildAvailable += _client_GuildAvailable;
        }

        private async Task _client_GuildAvailable(SocketGuild arg)
        {
            await InitStatusMessage().ConfigureAwait(false);
        }

        private async Task InitStatusMessage()
        {
            _guild = _client.GetGuild(ulong.Parse(_config["misc:guildId"]));
            var channel = await _guild.GetTextChannelAsync(ulong.Parse(_config["misc:channelId"]));
            _message = await channel.GetMessageAsync(ulong.Parse(_config["misc:statusMessageId"])).ConfigureAwait(false);
            _hoursMessage = await channel.GetMessageAsync(ulong.Parse(_config["misc:hoursRemainingMessageId"])).ConfigureAwait(false);
            _deathCountMessage = await channel.GetMessageAsync(ulong.Parse(_config["misc:deathCounterMessageId"])).ConfigureAwait(false);
            _logChannel = channel;

            await ClearChannel(channel);
        }

        internal async Task SendMinutesRemaining(Team team)
        {
            await _logChannel.SendMessageAsync("", false, Embeds.MinutesRemaining(team, _guild)).ConfigureAwait(false);
        }

        private async Task OnMessageReceieved(SocketMessage message)
        {
            var msg = message as SocketUserMessage;

            if (msg == null)
                return;

            if (msg.Author == _client.CurrentUser)
                return;

            if (msg.Author.Id == 519860567070998528)
            {
                await msg.Channel.SendMessageAsync("https://media.tenor.com/images/f6270f443c732df289e0b0d0240a9741/tenor.gif").ConfigureAwait(false);
            }

            var context = new SocketCommandContext(_client, msg);

            int argPos = 0;
            if (msg.HasStringPrefix(_config["prefix"], ref argPos) || msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                _log.Info($"{msg.Author.Username} (in {msg?.Channel?.Name}/{context?.Guild?.Name}) is trying to execute: " + msg.Content);
                var result = await _commands.ExecuteAsync(context, argPos, _provider);

                if (!result.IsSuccess)
                {
                    _log.Info(result.ToString());
                    await msg.DeleteAsync().ConfigureAwait(false);
                }
            }
            else
            {
                if (msg.Channel == _message.Channel && msg.Author.Id != _client.CurrentUser.Id)
                {
                    _log.Info($"Message Deleted in Bot Channel: {msg.Content} by {msg.Author.Username}");
                    await msg.DeleteAsync().ConfigureAwait(false);
                }
            }
        }

        internal async Task UpdateServerStatus(Discord.Embed embed)
        {
            if (_message is null)
                return;

            await (_message as IUserMessage).ModifyAsync(x => x.Embed = embed).ConfigureAwait(false);
        }

        internal async Task UpdateHoursRemaining(Discord.Embed embed)
        {
            if (_hoursMessage is null)
                return;

            await (_hoursMessage as IUserMessage).ModifyAsync(x => x.Embed = embed).ConfigureAwait(false);
        }

        internal async Task UpdateDeathCount(Discord.Embed embed)
        {
            if (_deathCountMessage is null)
                return;

            await (_deathCountMessage as IUserMessage).ModifyAsync(x => x.Embed = embed).ConfigureAwait(false);
        }

        internal async Task WriteKillToChannel(Embed e)
        {
            ITextChannel channel = await _guild.GetTextChannelAsync(787503967491981344).ConfigureAwait(false);

            await channel.SendMessageAsync("", false, e).ConfigureAwait(false);
        }

        internal async Task TellDannyToDoWork(double hoursLeft)
        {
            ITextChannel channel = await _guild.GetTextChannelAsync(630693266337169440);

            await channel.SendMessageAsync($"<@!519860567070998528> you have {hoursLeft} hours before your deadline.").ConfigureAwait(false);
        }

        internal async Task ClearChannel(ITextChannel c)
        {
            var messages = await c.GetMessagesAsync().FlattenAsync();
            List<IMessage> messagesToDelete = new List<IMessage>();
            foreach (var message in messages)
            {
                if (message.Id != _message.Id && message.Id != _hoursMessage.Id && message.Id != _deathCountMessage.Id)
                {
                    messagesToDelete.Add(message);
                }
            }
            await c.DeleteMessagesAsync(messagesToDelete).ConfigureAwait(false);
        }
    }
}