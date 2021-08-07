using Discord;
using Discord.Commands;
using Discord.WebSocket;
using EssentiBot.Common;
using EssentiBot.Services;
using EssentiBot.Utilities;
using Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EssentiBot.Modules
{
    public class General : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<General> _logger;
        private readonly Images _images;
        private readonly ServerHelper _serverHelper;
        private readonly DiscordSocketClient _client;

        public General(ILogger<General> logger, Images images, ServerHelper serverHelper, DiscordSocketClient client)
        {
            _logger = logger;
            _images = images;
            _serverHelper = serverHelper;
            _client = client;
        }
                   
        [Command("ping")]
        [Summary("Get a reply with the current latency.")]
        public async Task PingAsync()
        {
            await Context.Channel.TriggerTypingAsync();

            await Context.Channel.SendSuccessAsync("Pong!", $"{_client.Latency} ms");
            //_logger.LogInformation($"{Context.User.Username} executed the ping command!");
            await _serverHelper.SendLogAsync(Context.Guild, "Command executed", $"{Context.User.Username} executed the ping command!");
        }

        [Command("echo")]
        [Summary("Makes the bot repeat your message.")]
        public async Task EchoAsync([Remainder] string text)
        {
            await Context.Channel.TriggerTypingAsync();

            await ReplyAsync(text);
            //_logger.LogInformation($"{Context.User.Username} executed the echo command!");
            await _serverHelper.SendLogAsync(Context.Guild, "Command executed", $"{Context.User.Username} executed the echo command!");
        }

        [Command("math")]
        [Summary("A simple math command.")]
        public async Task MathAsync([Remainder] string math)
        {
            await Context.Channel.TriggerTypingAsync();

            var dt = new DataTable();
            var result = dt.Compute(math, null);

            await Context.Channel.SendSuccessAsync("Success", $"The result was {result}.");
            //_logger.LogInformation($"{Context.User.Username} executed the math command!");
            await _serverHelper.SendLogAsync(Context.Guild, "Command executed", $"{Context.User.Username} executed the math command!");
        }

        [Command("info")]
        [Summary("Shows information about yourself or a mentioned user.")]
        public async Task Info(SocketGuildUser user = null)
        {
            await Context.Channel.TriggerTypingAsync();

            if (user == null)
            {
                var builder = new EmbedBuilder()
                    .WithThumbnailUrl(Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl())
                    .WithDescription("In this message you can see some information about yourself!")
                    .WithColor(new Color(33, 176, 252))
                    .AddField("User ID", Context.User.Id, true)
                    .AddField("Created at", Context.User.CreatedAt.ToString("dd/MM/yyyy"), true)
                    .AddField("Joined at", (Context.User as SocketGuildUser).JoinedAt.Value.ToString("dd/MM/yyyy"), true)
                    .AddField("Roles", string.Join(" ", (Context.User as SocketGuildUser).Roles.Select(x => x.Mention)))
                    .WithCurrentTimestamp();
                var embed = builder.Build();
                await Context.Channel.SendMessageAsync(null, false, embed);
            }
            else
            {
                var builder = new EmbedBuilder()
                    .WithThumbnailUrl(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
                    .WithDescription($"In this message you can see some information about a {user.Username}!")
                    .WithColor(new Color(33, 176, 252))
                    .AddField("User ID", user.Id, true)
                    .AddField("Created at", user.CreatedAt.ToString("dd/MM/yyyy"), true)
                    .AddField("Joined at", user.JoinedAt.Value.ToString("dd/MM/yyyy"), true)
                    .AddField("Roles", string.Join(" ", user.Roles.Select(x => x.Mention)))
                    .WithCurrentTimestamp();
                var embed = builder.Build();
                await Context.Channel.SendMessageAsync(null, false, embed);
            }

            await _serverHelper.SendLogAsync(Context.Guild, "Command executed", $"{Context.User.Username} executed the info command!");
        }       

        [Command("server")]
        [Summary("Shows information about the server.")]
        public async Task Server()
        {
            await Context.Channel.TriggerTypingAsync();

            var builder = new EmbedBuilder()
                .WithThumbnailUrl(Context.Guild.IconUrl)
                .WithDescription("In this message you can find some information about the current server.")
                .WithTitle($"{Context.Guild.Name} Information")
                .WithColor(new Color(33, 176, 252))
                .AddField("Created at", Context.Guild.CreatedAt.ToString("dd/MM/yyyy"), true)
                .AddField("Membercount", (Context.Guild as SocketGuild).MemberCount + " members", true)
                .AddField("Online users", (Context.Guild as SocketGuild).Users.Where(x => x.Status != UserStatus.Offline).Count() + " members", true);
            var embed = builder.Build();
            await Context.Channel.SendMessageAsync(null, false, embed);

            await _serverHelper.SendLogAsync(Context.Guild, "Command executed", $"{Context.User.Username} executed the server command!");
        }                

        [Command("rank", RunMode = RunMode.Async)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [Summary("Add or remove a rank from yourself.")]
        public async Task Rank([Remainder]string identifier)
        {
            await Context.Channel.TriggerTypingAsync();

            var ranks = await _serverHelper.GetRanksAsync(Context.Guild);

            IRole role;

            if (ulong.TryParse(identifier, out ulong roleId))
            {
                var roleById = Context.Guild.Roles.FirstOrDefault(x => x.Id == roleId);
                if(roleById == null)
                {
                    //await ReplyAsync("That role does not exist!");
                    await Context.Channel.SendErrorAsync("Error!", "That role does not exist!");
                    return;
                }

                role = roleById;
            }
            else
            {
                var roleByName = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, identifier, StringComparison.CurrentCultureIgnoreCase));
                if(roleByName == null)
                {
                    await Context.Channel.SendErrorAsync("Error!", "That role does not exist!");
                    return;
                }

                role = roleByName;
            }

            if (ranks.All(x => x.Id != role.Id))
            {
                await Context.Channel.SendErrorAsync("Error!", "That rank does not exist!");
                return;
            }

            if ((Context.User as SocketGuildUser).Roles.Any(x => x.Id == role.Id))
            {
                await (Context.User as SocketGuildUser).RemoveRoleAsync(role);
                await Context.Channel.SendSuccessAsync("Success!", $"Succesfully removed the rank {role.Mention} from you.");
                ranks = await _serverHelper.GetRanksAsync(Context.Guild);
                return;
            }

            await (Context.User as SocketGuildUser).AddRoleAsync(role);
            await Context.Channel.SendSuccessAsync("Success!", $"Succesfully added the rank {role.Mention} to you.");
            await _serverHelper.SendLogAsync(Context.Guild, "Command executed", $"{Context.User.Username} executed the rank command!");
            ranks = await _serverHelper.GetRanksAsync(Context.Guild);
        }

        
    }
}
