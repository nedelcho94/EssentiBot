using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using EssentiBot.Common;
using EssentiBot.Utilities;
using Infrastructure;
using Microsoft.Extensions.Configuration;
using Victoria;
using Victoria.EventArgs;

namespace EssentiBot.Services
{
    public class CommandHandler : InitializedService
    {
        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _service;
        private readonly IConfiguration _config;
        private readonly Servers _servers;
        private readonly UserProfiles _userProfiles;
        private readonly ServerHelper _serverHelper;
        private readonly Images _images;
        public static List<Mute> Mutes = new List<Mute>();
        private readonly LavaNode _lavaNode;
        private readonly Random rand = new Random();
        public static List<ulong> cd = new List<ulong>();
        private readonly EssentiBotContext _context;
        private readonly DateTime coolDown = DateTime.ParseExact("12:00 AM", "h:mm tt", CultureInfo.InvariantCulture);

        public CommandHandler(DiscordSocketClient client, CommandService service, IConfiguration config, IServiceProvider provider, Servers servers, ServerHelper serverHelper, Images images, LavaNode lavaNode, UserProfiles userProfiles, EssentiBotContext context)
        {
            _provider = provider;
            _client = client;
            _service = service;
            _config = config;
            _servers = servers;
            _serverHelper = serverHelper;
            _images = images;
            _lavaNode = lavaNode;
            _userProfiles = userProfiles;
            _context = context;
        }
        // Pass the events we are working with to the modules
        public override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            _client.MessageReceived += OnMessageReceived;
            _client.UserJoined += OnUserJoined;
            _client.Ready += OnReadyAsync;
            _lavaNode.OnTrackEnded += OnTrackEnded;
            _client.ReactionAdded += OnReactionAsync;
            _client.JoinedGuild += OnJoinedGuild;
            _client.LeftGuild += OnLeftGuild;

            var newTask = new Task(async () => await MuteHandler());
            newTask.Start();

            var cdTask = new Task(async () => await DailyCdHandler());
            cdTask.Start();

            _service.CommandExecuted += OnCommandExecuted;
            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }

        private async Task OnTrackEnded(TrackEndedEventArgs args)
        {
            if (!args.Reason.ShouldPlayNext())
            {
                return;
            }

            var player = args.Player;
            if (!player.Queue.TryDequeue(out var queueable))
            {
                await (player.TextChannel as ISocketMessageChannel).SendSuccessAsync("Queue completed!", "Please add more tracks to rock n' roll!");
                await _client.SetGameAsync("use //help to see my commands", null, ActivityType.Playing);
                return;
            }

            if (!(queueable is LavaTrack track))
            {
                await (player.TextChannel as ISocketMessageChannel).SendErrorAsync("Error!","Next item in queue is not a track!");
                return;
            }

            await args.Player.PlayAsync(track);
            await (args.Player.TextChannel as ISocketMessageChannel).SendSuccessAsync("Finished playing current song!",
                $"{args.Reason}: `{args.Track.Title}`\n\nNow playing: `{track.Title}`");
            await _client.SetGameAsync($"{args.Track.Title}", null, ActivityType.Listening);
        }

        private async Task OnLeftGuild(SocketGuild arg)
        {
            await _client.SetGameAsync($"over {_client.Guilds.Count} servers", null, ActivityType.Watching);
        }

        private async Task OnJoinedGuild(SocketGuild arg)
        {
            await _client.SetGameAsync($"over {_client.Guilds.Count} servers", null, ActivityType.Watching);
        }

        private async Task OnReactionAsync(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if (arg3.MessageId != 792104075118379029) return;

            if (arg3.Emote.Name != "✅") return;

            var channel = await (arg2 as ITextChannel).Guild.GetTextChannelAsync(790276024326684672);
            await channel.SendMessageAsync($"{arg3.User.Value.Mention} replied with the ✅");
        }

        private async Task OnReadyAsync()
        {
            if (!_lavaNode.IsConnected)
            {
               await _lavaNode.ConnectAsync();
            }
            await _client.SetGameAsync("use //help to see my commands", null, ActivityType.Playing);
        }

        private async Task MuteHandler()
        {
            List<Mute> Remove = new List<Mute>();
            foreach(var mute in Mutes)
            {
                if (DateTime.Now < mute.End)
                    continue;

                var guild = _client.GetGuild(mute.Guild.Id);

                if (guild.GetRole(mute.Role.Id) == null)
                {
                    Remove.Add(mute);
                    continue;
                }

                var role = guild.GetRole(mute.Role.Id);

                if(guild.GetUser(mute.User.Id) == null)
                {
                    Remove.Add(mute);
                    continue;
                }

                var user = guild.GetUser(mute.User.Id);

                if(role.Position > guild.CurrentUser.Hierarchy)
                {
                    Remove.Add(mute);
                    continue;
                }

                await user.RemoveRoleAsync(mute.Role);
                Remove.Add(mute);
            }

            Mutes = Mutes.Except(Remove).ToList();

            await Task.Delay(1 * 60 * 1000);
            await MuteHandler();
        }

        private async Task OnUserJoined(SocketGuildUser arg)
        { 
            var newTask = new Task(async () => await HandleUserJoined(arg));
            newTask.Start();
        }

        private async Task HandleUserJoined(SocketGuildUser arg)
        {
            var roles = await _serverHelper.GetAutoRolesAsync(arg.Guild);
            if (roles.Count > 0)         
                await arg.AddRolesAsync(roles);

            var channelId = await _servers.GetWelcomeAsync(arg.Guild.Id);
            if (channelId == 0)
                return;

            var channel = arg.Guild.GetTextChannel(channelId);
            if(channel == null)
            {
                await _servers.ClearWelcomeAsync(arg.Guild.Id);
                return;
            }

            var background = await _servers.GetBackgroundAsync(arg.Guild.Id);
            string path = await _images.CreateImageAsync(arg, background);

            await channel.SendFileAsync(path, null);
            System.IO.File.Delete(path);
            await _serverHelper.SendLogAsync(arg.Guild, "User joined", $"{arg.Username} has joined the server!");
        }

        private async Task OnMessageReceived(SocketMessage arg)
        {
            if (!(arg is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            var argPos = 0;
            var guild = message.Channel as SocketGuildChannel;
            var socketUser = message.Author as SocketGuildUser;
            var prefix = await _servers.GetGuildPrefix(guild.Guild.Id) ?? "//";

            var server = await _context.Servers
                .FindAsync(guild.Guild.Id);

            if (server == null)
            {
                _context.Add(new Server { ServerId = guild.Guild.Id, Prefix = prefix });
            }
               
            if (!message.HasStringPrefix(prefix, ref argPos) && !message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                var discordUser = await _userProfiles.GetOrCreateUserProfile(guild.Guild.Id, message.Author.Id);
                await _userProfiles.ModifyExpAsync(message.Author.Id, (ulong)rand.Next(1, 10));

                var expToNextLevel = (ulong)(5 * Math.Pow(discordUser.Level, 2) + 50 * discordUser.Level + 100);

                if (discordUser.Exp >= expToNextLevel)
                {
                    await _userProfiles.ModifyLevelAsync(discordUser.UserId, discordUser.Level += 1);
                    discordUser.Exp -= expToNextLevel;

                    var channelId = await _servers.GetLevelUpChannelAsync(guild.Guild.Id);
                    if (channelId == 0)
                        return;

                    var channel = guild.Guild.GetTextChannel(channelId);
                    if (channel == null)
                    {
                        await _servers.ClearLevelUpChannelAsync(guild.Guild.Id);
                        return;
                    }
                    
                    var levelUpBackground = await _servers.GetLevelUpAsync(guild.Guild.Id);
                    string path = await _images.CreateLevelUpImageAsync(socketUser, discordUser, levelUpBackground);

                    await channel.SendFileAsync(path, null);
                    System.IO.File.Delete(path);
                }

                return;
            }

            var context = new SocketCommandContext(_client, message);
            await _service.ExecuteAsync(context, argPos, _provider);
        }

        private async Task OnCommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (command.IsSpecified && !result.IsSuccess) await (context.Channel as ISocketMessageChannel).SendErrorAsync("Error!", result.ErrorReason);
        }

        private async Task DailyCdHandler()
        {
            var currentTime = DateTime.Now.ToShortTimeString();
            if (DateTime.Compare(coolDown, DateTime.ParseExact(currentTime, "h:mm tt", CultureInfo.InvariantCulture)) == 0)
            {
                cd.Clear();
                Console.WriteLine("cooldown cleared");
            }   
            await Task.Delay(1 * 60 * 1000);
            await DailyCdHandler();
        }
    }
}
