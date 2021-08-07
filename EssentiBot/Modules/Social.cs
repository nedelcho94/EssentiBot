using System;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using EssentiBot.Common;
using Infrastructure;
using System.Collections;
using System.Threading.Tasks;
using EssentiBot.Services;

namespace EssentiBot.Modules
{
    public class Social : ModuleBase<SocketCommandContext>
    {
        private readonly UserProfiles _userProfiles;

        public Social(UserProfiles userProfiles)
        {
            _userProfiles = userProfiles;
        }

        [Command("profile")]
        public async Task Profile([Remainder]SocketGuildUser user = null)
        {
            if(user != null)
            {
                await GetProfileToDisplayAsync(user);
            }
            else
            {
                await GetProfileToDisplayAsync(Context.Guild.GetUser(Context.User.Id));
            }
            
        }

        [Command("dailies")]
        [Alias("daily")]
        [Summary("Claim 200 daily EssentiCoins or give someone else daily EssentiCoins." +
            "\nGiving someone else gives them 50 extra coins.")]
        public async Task DailyEssentiCoins([Remainder] SocketGuildUser user = null)
        {
            if (user == null)
            {
                var userProfile = await _userProfiles.GetOrCreateUserProfile(Context.Guild.Id, Context.User.Id);
                if (!CommandHandler.cd.Contains(userProfile.UserId))
                {
                    await _userProfiles.ModifyEssentiCoinsAsync(userProfile.UserId, 200);
                    CommandHandler.cd.Add(userProfile.UserId);
                    await Context.Channel.SendSuccessAsync("Success!", 
                        "Successfully claimed 200 daily EssentiCoins!");
                }
                else
                {
                    await Context.Channel.SendErrorAsync("Error!", "You can't use that yet.");
                }
                
            }
            else
            {
                var chosenUserProfile = await _userProfiles.GetOrCreateUserProfile(Context.Guild.Id, user.Id);
                if (!CommandHandler.cd.Contains(chosenUserProfile.UserId))
                    await _userProfiles.ModifyEssentiCoinsAsync(chosenUserProfile.UserId, 250);
                await Context.Channel.SendSuccessAsync("Success!", $"Successfully given 250 daily EssentiCoins to {user.Username}!");
            }            
        }

        [Command("leaderboard")]
        [Alias("lb")]
        public async Task LeaderBoard()
        {
            await LeaderBoardDisplayAsync();
        }


        private async Task GetProfileToDisplayAsync(SocketGuildUser user)
        {    
             var profile = await _userProfiles.GetOrCreateUserProfile(Context.Guild.Id, user.Id);
             var profileEmbed = new EmbedBuilder()
                    .WithThumbnailUrl(user.GetAvatarUrl())
                    .AddField("User: ", user.Username, true)
                    .AddField("Level: ", profile.Level.ToString(), true)
                    .AddField("XP: ", profile.Exp.ToString(), true)
                    .AddField("EssentiCoins: ", profile.EssentiCoins.ToString(), false);
            var embed = profileEmbed.Build();
             await Context.Channel.SendMessageAsync(null, false, embed);
        }

        private async Task LeaderBoardDisplayAsync()
        {
            var usersList = await _userProfiles.GetAllUserProfiles(Context.Guild.Id);
            var profileEmbed = new EmbedBuilder();
            profileEmbed.WithTitle("Top 10 most active people");
            var count = 1;
            foreach (var i in usersList)
            {
                var userName = Context.Guild.GetUser(i.UserId).Username;
                profileEmbed.AddField($"{count} place:", $"``{userName}`` Level: ``{i.Level}``");
                count++;
            }
            var embed = profileEmbed.Build();
            await Context.Channel.SendMessageAsync(null, false, embed);
        }
    }
}
