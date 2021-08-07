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
    public class Moderation : ModuleBase<SocketCommandContext>
    {
        private readonly ServerHelper _serverHelper;

        public Moderation(ServerHelper serverHelper)
        {
            _serverHelper = serverHelper;
        }

        [Command("purge")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Summary("Delete a set number of messages. " +
            "Requires `Administrator` role.")]
        public async Task Purge(int amount)
        {
            var messages = await Context.Channel.GetMessagesAsync(amount + 1)
                .FlattenAsync();
            await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);

            var message = await Context.Channel
                .SendMessageAsync($"{messages.Count()} messages delete successfully!");
            await Task.Delay(2500);
            await message.DeleteAsync();
        }

        [Command("mute")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [Summary("Mute someone for a given duration. " +
            "Requires `Administrator` role.")]
        public async Task Mute(SocketGuildUser user, int minutes, [Remainder] string reason = null)
        {
            if (user.Hierarchy > Context.Guild.CurrentUser.Hierarchy)
            {
                await Context.Channel.SendErrorAsync("Invalid user!",
                    "That user has a higher position than the bot.");
                return;
            }

            var role = (Context.Guild as IGuild).Roles
                .FirstOrDefault(x => x.Name == "Muted!");
            if (role == null)
                role = await Context.Guild
                    .CreateRoleAsync("Muted!", new GuildPermissions(sendMessages: false), null, false, null);

            if (role.Position > Context.Guild.CurrentUser.Hierarchy)
            {
                await Context.Channel.SendErrorAsync("Invalid permissions!",
                    "The muted role has a higher position than the bot.");
                return;
            }

            if (user.Roles.Contains(role))
            {
                await Context.Channel.SendErrorAsync("Already muted!", 
                    "That user is already muted.");
                return;
            }

            await role.ModifyAsync(x => x
            .Position = Context.Guild.CurrentUser.Hierarchy);

            foreach (var channel in Context.Guild.TextChannels)
            {
                if (!channel.GetPermissionOverwrite(role).HasValue || 
                    channel.GetPermissionOverwrite(role).Value.SendMessages == PermValue.Allow)
                {
                    await channel.AddPermissionOverwriteAsync(role, 
                        new OverwritePermissions(sendMessages: PermValue.Deny));
                }
            }

            CommandHandler.Mutes
                .Add(new Mute { Guild = Context.Guild, User = user, 
                    End = DateTime.Now + TimeSpan.FromMinutes(minutes), Role = role });

            await user.AddRoleAsync(role);
            await Context.Channel.SendSuccessAsync($"Muted {user.Username}!",
                $"Duration: {minutes} minutes\nReason: {reason ?? "None"}");
            await _serverHelper.SendLogAsync(Context.Guild, "Command executed",
                $"{Context.User.Username} executed the mute command!");
        }

        [Command("unmute")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [Summary("Unmute someone that has been muted. Requires `Administrator` role.")]
        public async Task Unmute(SocketGuildUser user)
        {
            var role = (Context.Guild as IGuild).Roles
                .FirstOrDefault(x => x.Name == "Muted!");
            if (role == null)
            {
                await Context.Channel.SendErrorAsync("Not muted!",
                    "This person has not been muted yet.");
                return;
            }

            if (role.Position > Context.Guild.CurrentUser.Hierarchy)
            {
                await Context.Channel.SendErrorAsync("Invalid permissions!",
                    "The muted role has a higher position than the bot.");
                return;
            }

            if (!user.Roles.Contains(role))
            {
                await Context.Channel.SendErrorAsync("Not muted!",
                    "This person has not been muted yet.");
                return;
            }

            await user.RemoveRoleAsync(role);
            await Context.Channel.SendSuccessAsync($"Unmuted {user.Username}!",
                "Successfully unmuted the user.");
            await _serverHelper.SendLogAsync(Context.Guild, "Command executed",
                $"{Context.User.Username} executed the unmute command!");
        }
    }
}
