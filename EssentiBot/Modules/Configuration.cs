using Discord;
using Discord.Commands;
using EssentiBot.Utilities;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EssentiBot.Common;

namespace EssentiBot.Modules
{
    public class Configuration : ModuleBase<SocketCommandContext>
    {
        private readonly Servers _servers;
        private readonly Ranks _ranks;
        private readonly AutoRoles _autoRoles;
        private readonly ServerHelper _serverHelper;
        private readonly Items _items;

        public Configuration(Servers servers, Ranks ranks, AutoRoles autoRoles, ServerHelper serverHelper, Items items)
        {
            _servers = servers;
            _ranks = ranks;
            _autoRoles = autoRoles;
            _serverHelper = serverHelper;
            _items = items;
        }

        [Command("prefix", RunMode = RunMode.Async)]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        [Summary("View or change the prefix. Requires `Administrator` role.")]
        public async Task Prefix(string prefix = null)
        {
            if (prefix == null)
            {
                var guildPrefix = await _servers.GetGuildPrefix(Context.Guild.Id) ?? "//";
                await ReplyAsync($"The current prefix of this bot is `{guildPrefix}`.");
                return;
            }

            if (prefix.Length > 8)
            {
                await ReplyAsync("The length of the new prefix is too long!");
                return;
            }

            await _servers.ModifyGuildPrefix(Context.Guild.Id, prefix);
            await Context.Channel.SendSuccessAsync("Success!", $"The prefix has been changed to `{prefix}`.");
            await _serverHelper.SendLogAsync(Context.Guild, "Prefix adjusted", $"{Context.User.Mention} modified the prefix to `{prefix}`.");
        }

        [Command("ranks", RunMode = RunMode.Async)]
        [Summary("Lists all available ranks.\nIn order to add a rank, you can use the name or ID of the rank.")]
        public async Task Ranks()
        {
            var ranks = await _serverHelper.GetRanksAsync(Context.Guild);
            if (ranks.Count == 0)
            {
                await ReplyAsync("This server does not yet have any ranks!");
                return;
            }

            await Context.Channel.TriggerTypingAsync();

            string desctiption = "This message lists all available ranks.\nIn order to add a rank, you can use the name or ID of the rank.";
            foreach (var rank in ranks)
            {
                desctiption += $"\n{rank.Mention} ({rank.Id})";
            }

            await ReplyAsync(desctiption);
        }

        [Command("addrank", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [Summary("Add a role to the server ranks. Requires `Administrator` role.")]
        public async Task AddRank([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var ranks = await _serverHelper.GetRanksAsync(Context.Guild);

            var role = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (role == null)
            {
                await ReplyAsync("That role does not exist!");
                return;
            }

            if (role.Position > Context.Guild.CurrentUser.Hierarchy)
            {
                await ReplyAsync("That role has a higher position than the bot!");
                return;
            }

            if (ranks.Any(x => x.Id == role.Id))
            {
                await ReplyAsync("That role is already a rank!");
                return;
            }

            await _ranks.AddRankAsync(Context.Guild.Id, role.Id);
            await ReplyAsync($"The role {role.Mention} has been added to the ranks!");
        }

        [Command("delrank", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [Summary("Remove a role from the server ranks. Requires `Administrator` role")]
        public async Task DelRank([Remainder]string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var ranks = await _serverHelper.GetRanksAsync(Context.Guild);

            var role = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (role == null)
            {
                await ReplyAsync("That role does not exist!");
                return;
            }

            if (ranks.Any(x => x.Id != role.Id))
            {
                await ReplyAsync("This role is not a rank yet!");
                return;
            }

            await _ranks.RemoveRankAsync(Context.Guild.Id, role.Id);
            await ReplyAsync($"The role {role.Mention} has been removed from the ranks!");
        }


        [Command("autoroles", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("List all available autoroles on the server. Requires `Administrator` role.")]
        public async Task AutoRoles()
        {
            var autoRoles = await _serverHelper.GetAutoRolesAsync(Context.Guild);
            if (autoRoles.Count == 0)
            {
                await ReplyAsync("This server does not yet have any autoroles!");
                return;
            }

            await Context.Channel.TriggerTypingAsync();

            string desctiption = "This message lists all autoroles.\nIn order to remove an autorole, use the nme or ID";
            foreach (var autoRole in autoRoles)
            {
                desctiption += $"\n{autoRole.Mention} ({autoRole.Id})";
            }

            await ReplyAsync(desctiption);
        }

        [Command("addautorole", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [Summary("Add a role to the server autoroles. Requires `Administrator` role.")]
        public async Task AddAutoRole([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var autoRoles = await _serverHelper.GetAutoRolesAsync(Context.Guild);

            var role = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (role == null)
            {
                await ReplyAsync("That role does not exist!");
                return;
            }

            if (role.Position > Context.Guild.CurrentUser.Hierarchy)
            {
                await ReplyAsync("That role has a higher position than the bot!");
                return;
            }

            if (autoRoles.Any(x => x.Id == role.Id))
            {
                await ReplyAsync("That role is already an autorole!");
                return;
            }

            await _autoRoles.AddAutoRoleAsync(Context.Guild.Id, role.Id);
            await ReplyAsync($"The role {role.Mention} has been added to the autoroles!");
        }

        [Command("delautorole", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [Summary("Remove a role from the server autoroles. Requires `Administrator` role.")]
        public async Task DelAutoRole([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var autoRoles = await _serverHelper.GetAutoRolesAsync(Context.Guild);

            var role = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (role == null)
            {
                await ReplyAsync("That role does not exist!");
                return;
            }

            if (autoRoles.Any(x => x.Id != role.Id))
            {
                await ReplyAsync("This role is not an autorole yet!");
                return;
            }

            await _autoRoles.RemoveAutoRoleAsync(Context.Guild.Id, role.Id);
            await ReplyAsync($"The role {role.Mention} has been removed from the autoroles!");
        }

        [Command("welcome")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Set/Modify/Clear a welcome channel and background for the server." +
            " Requires `Administrator` role.")]
        public async Task Welcome(string option = null, string value = null)
        {
            if(option == null && value == null)
            {
                var fetchedChannelId = await _servers.GetWelcomeAsync(Context.Guild.Id);
                if(fetchedChannelId == 0)
                {
                    await Context.Channel.SendErrorAsync("Error!",
                        "There has not been set a welcome channel yet!");
                    return;
                }

                var fetchedChannel = Context.Guild.GetTextChannel(fetchedChannelId);
                if (fetchedChannel == null)
                {
                    await Context.Channel.SendErrorAsync("Error!",
                        "There has not been set a welcome channel yet!");
                    await _servers.ClearWelcomeAsync(Context.Guild.Id);
                    return;
                }

                var fetchedBackground = await _servers.GetBackgroundAsync(Context.Guild.Id);

                if (fetchedBackground != null)
                    await Context.Channel.SendSuccessAsync("Success!",
                        $"The channel used for the welcome module is {fetchedChannel.Mention}." +
                        $"\nThe background is set to {fetchedBackground}.");
                else
                    await Context.Channel.SendSuccessAsync("Success!",
                        $"The channel used for the welcome module is {fetchedChannel.Mention}.");

                return;
            }

            if(option == "channel" && value != null)
            {
                if(!MentionUtils.TryParseChannel(value, out ulong parsedId))
                {
                    await Context.Channel.SendErrorAsync("Error!", "Please pass in a valid channel!");
                    return;
                }

                var parsedChannel = Context.Guild.GetTextChannel(parsedId);
                if(parsedChannel == null)
                {
                    await Context.Channel.SendErrorAsync("Error!", "Please pass in a valid channel!");
                    return;
                }

                await _servers.ModifyWelcomeAsync(Context.Guild.Id, parsedId);
                await Context.Channel.SendSuccessAsync("Success!", $"Successfully modified the welcome channel to {parsedChannel.Mention}.");
                return;
            }

            if (option == "background" && value != null)
            {
                if(value == "clear")
                {
                    await _servers.ClearBckgroundAsync(Context.Guild.Id);
                    await Context.Channel.SendSuccessAsync("Success!", "Successfully cleared the background for this server.");
                }

                await _servers.ModifyBackgroundAsync(Context.Guild.Id, value);
                await Context.Channel.SendSuccessAsync("Success!", $"Successfully modified the background to {value}.");
                return;
            }

            if(option == "clear" && value == null)
            {
                await _servers.ClearWelcomeAsync(Context.Guild.Id);
                await Context.Channel.SendSuccessAsync("Success!", "Successfully cleared the welcome channel.");
                return;
            }

            await Context.Channel.SendErrorAsync("Error!", "You did not use this command properly.");
        }

        [Command("logs")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Set/Modify/Clear a logs channel for the server. Requires `Administrator` role.")]
        public async Task Logs(string value = null)
        {
            if (value == null)
            {
                var fetchedChannelId = await _servers.GetLogsAsync(Context.Guild.Id);
                if (fetchedChannelId == 0)
                {
                    await Context.Channel.SendErrorAsync("Error!", "There has not been set a logs channel yet!");
                    return;
                }

                var fetchedChannel = Context.Guild.GetTextChannel(fetchedChannelId);
                if (fetchedChannel == null)
                {
                    await Context.Channel.SendErrorAsync("Error!", "There has not been set a logs channel yet!");
                    await _servers.ClearLogsAsync(Context.Guild.Id);
                    return;
                }

                await Context.Channel.SendSuccessAsync("Success!", $"The channel used for the logs is set to {fetchedChannel.Mention}");

                return;
            }

            if (value != "clear")
            {
                if (!MentionUtils.TryParseChannel(value, out ulong parsedId))
                {
                    await Context.Channel.SendErrorAsync("Error!", "Please pass in a valid channel!");
                    return;
                }

                var parsedChannel = Context.Guild.GetTextChannel(parsedId);
                if (parsedChannel == null)
                {
                    await Context.Channel.SendErrorAsync("Error!", "Please pass in a valid channel!");
                    return;
                }

                await _servers.ModifyLogsAsync(Context.Guild.Id, parsedId);
                await Context.Channel.SendSuccessAsync("Success!", $"Successfully modified the logs channel to {parsedChannel.Mention}.");
                return;
            }

            if (value == "clear")
            {
                await _servers.ClearLogsAsync(Context.Guild.Id);
                await Context.Channel.SendSuccessAsync("Success!", "Successfully cleared the logs channel!");
                return;
            }

            await Context.Channel.SendErrorAsync("Error!", "You did not use this command properly.");
        }

        [Command("setlevelup")]
        [Alias("levelup")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Set/Modify/Clear a level up channel and background for the server. Requires `Administrator` role.")]
        public async Task LevelUp(string option = null, string value = null)
        {
            if (option == null && value == null)
            {
                var fetchedChannelId = await _servers.GetLevelUpChannelAsync(Context.Guild.Id);
                if (fetchedChannelId == 0)
                {
                    await Context.Channel.SendErrorAsync("Error!", "There has not been set a level up channel yet!");
                    return;
                }

                var fetchedChannel = Context.Guild.GetTextChannel(fetchedChannelId);
                if (fetchedChannel == null)
                {
                    await Context.Channel.SendErrorAsync("Error!", "There has not been set a level up channel yet!");
                    await _servers.ClearLevelUpChannelAsync(Context.Guild.Id);
                    return;
                }

                var fetchedLevelUp = await _servers.GetLevelUpAsync(Context.Guild.Id);

                if (fetchedLevelUp != null)
                    await Context.Channel.SendSuccessAsync("Success!", $"The channel used for the level up message is {fetchedChannel.Mention}.\nThe background is set to {fetchedLevelUp}.");
                else
                    await Context.Channel.SendSuccessAsync("Success!", $"The channel used for the level up message is {fetchedChannel.Mention}.");

                return;
            }

            if (option == "channel" && value != null)
            {
                if (!MentionUtils.TryParseChannel(value, out ulong parsedId))
                {
                    await Context.Channel.SendErrorAsync("Error!", "Please pass in a valid channel!");
                    return;
                }

                var parsedChannel = Context.Guild.GetTextChannel(parsedId);
                if (parsedChannel == null)
                {
                    await Context.Channel.SendErrorAsync("Error!", "Please pass in a valid channel!");
                    return;
                }

                await _servers.ModifyLevelUpChannelAsync(Context.Guild.Id, parsedId);
                await Context.Channel.SendSuccessAsync("Success!", $"Successfully modified the level up channel to {parsedChannel.Mention}.");
                return;
            }

            if (option == "background" && value != null)
            {
                if (value == "clear")
                {
                    await _servers.ClearLevelUpAsync(Context.Guild.Id);
                    await Context.Channel.SendSuccessAsync("Success!", "Successfully cleared the level up background for this server.");
                }

                await _servers.ModifyLevelUpAsync(Context.Guild.Id, value);
                await Context.Channel.SendSuccessAsync("Success!", $"Successfully modified the level up background to {value}.");
                return;
            }

            if (option == "clear" && value == null)
            {
                await _servers.ClearLevelUpChannelAsync(Context.Guild.Id);
                await Context.Channel.SendSuccessAsync("Success!", "Successfully cleared the level up channel.");
                return;
            }

            await Context.Channel.SendErrorAsync("Error!", "You did not use this command properly.");
        }

        [Command("additem")]
        [Alias("addi")]
        public async Task AddItemToDb(string itemName, ulong sellPrice, string rarity)
        {
            await Context.Channel.TriggerTypingAsync();

            await _items.AddItemAsync(itemName, sellPrice, rarity);

            await Context.Channel.SendSuccessAsync("Success!", $"Added {itemName} with sell price {sellPrice} and rarity {rarity}");
        }
    }

}
