using Discord.Commands;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Discord;
using Discord.WebSocket;
using EssentiBot.Common;
using System.Linq;
using Infrastructure;


namespace EssentiBot.Modules
{
    public class Fun : ModuleBase<SocketCommandContext>
    {
        private readonly Items _items;
        private readonly UserProfiles _userProfiles;

        public Fun(Items items, UserProfiles userProfiles)
        {
            _items = items;
            _userProfiles = userProfiles;
        }

        [Command("meme")]
        [Alias("reddit")]
        public async Task Meme(string subreddit = null)
        {
            await Context.Channel.TriggerTypingAsync();

            var client = new HttpClient();
            var result = await client.GetStringAsync($"https://reddit.com/r/{subreddit ?? "memes"}/random.json?limit=1");
            
            if (!result.StartsWith("["))
            {
                //await Context.Channel.SendMessageAsync("This subreddit does not exist!");
                await (Context.Channel as ISocketMessageChannel).SendErrorAsync("Error!", "This subreddit does not exist!");
                return;
            }
            JArray arr = JArray.Parse(result);
            JObject post = JObject.Parse(arr[0]["data"]["children"][0]["data"].ToString());

            if(post["over_18"].ToString() == "True" && !(Context.Channel as ITextChannel).IsNsfw)
            {
                await (Context.Channel as ISocketMessageChannel).SendErrorAsync("Error!", "The subreddit contains NSFW content, while this is a SFW channel!");
                return;
            }

            var builder = new EmbedBuilder()
                .WithImageUrl(post["url"].ToString())
                .WithColor(new Color(33, 176, 252))
                .WithTitle(post["title"].ToString())
                .WithUrl("https://reddit.com" + post["permalink"].ToString())
                .WithFooter($"🗨 {post["num_comments"]} ⬆️ {post["ups"]}");
            var embed = builder.Build();
            await Context.Channel.SendMessageAsync(null, false, embed);
        }

        [Command("joke")]
        [Summary("Get a random joke as a response")]
        public async Task Joke()
        {
            string[] responses = { "What do you call a dinosaur that is sleeping?\n\n**A dino-snore!**",
                "What did the left eye say to the right eye?\n\n**Between us, something smells!**",
                "When you look for something, why is it always in the last place you look?\n\n**Because when you find it, you stop looking.**",
                "Obi Wan: “Yoda, why did the Star Wars movies come out 4,5,6,1,2,3\n\nYoda: “In charge of scheduling I was”",
                "What did the 0 say to the 8?\n\n**Nice belt!**"
            };
            await ReplyAsync(responses[new Random().Next(0, responses.Count())]);
        }

        [Command("fishy")]
        [Summary("Fishing game")]
        [Alias("fish")]
        public async Task Fishy()
        {
            var inventory = await _items.GetInventoryByUserId(Context.User.Id);
            var profileCoins = await _userProfiles.GetEssentiCoinsAsync(Context.User.Id);

            if (profileCoins < 5)
            {
                await Context.Channel.SendErrorAsync("Error!", "You are poor!");
            }
            else
            {
                await _userProfiles.SubtractEssentiCoinsAsync(Context.User.Id, 5);

                if (inventory != null)
                {
                    var catchRate = new Random().Next(1, 100);
                    var catchChance = new Random().Next(1, 100);

                    if (catchChance >= 50)
                    {
                        //catch fish
                        if (catchRate <= 50)
                        {
                            //catch garbage fish
                            //add common fish to user inventory
                            await Context.Channel.EmbedBuild("Success!", "You just caught 🗑️!");                            
                        }
                        else if (catchRate >= 51 && catchRate <= 75)
                        {
                            //catch common fish
                            //add to user inventory
                            await Context.Channel.EmbedBuild("Success!", "You just caught 🐟!");
                        }
                        else if (catchRate >= 76 && catchRate <= 93)
                        {
                            //catch uncommon fish
                            //add to inventory
                            await Context.Channel.EmbedBuild("Success!", "You just caught 🐡!");
                        }
                        else
                        {
                            //catch rare fish
                            //add to inventory
                            await Context.Channel.EmbedBuild("Success!", "You just caught 🐠!");
                        }
                    }
                    else
                    {
                        await Context.Channel.SendErrorAsync("OOF", "You didn't catch anything. Better luck next time!");
                        //didnt catch anything
                    }
                }
                else
                {
                    await _items.AddInventory(Context.User.Id);
                }
            }
            
        }
        [Command("fishyinventory")]
        public async Task FishyInventory()
        {
            await DisplayFishyInventory();
        }

        private async Task DisplayFishyInventory()
        {
            var itemsList = await _items.GetItemsFromInventory();
            var inventoryEmbed = new EmbedBuilder();
            inventoryEmbed.WithTitle($"{Context.User.Username}'s inventory");

            var count = 1;
            foreach (var i in itemsList)
            {
                inventoryEmbed.AddField($"{i.ItemName} ", $",sell price: ``{i.SellPrice}``");
            }
        }

        //private async Task LeaderBoardDisplayAsync()
        //{
        //    var usersList = await _userProfiles.GetAllUserProfiles(Context.Guild.Id);
        //    var profileEmbed = new EmbedBuilder();
        //    profileEmbed.WithTitle("Top 10 most active people");
        //    var count = 1;
        //    foreach (var i in usersList)
        //    {
        //        var userName = Context.Guild.GetUser(i.UserId).Username;
        //        profileEmbed.AddField($"{count} place:", $"``{userName}`` Level: ``{i.Level}``");
        //        count++;
        //    }
        //    var embed = profileEmbed.Build();
        //    await Context.Channel.SendMessageAsync(null, false, embed);
        //}

    }
}
