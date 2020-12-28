using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EssentiBot.Modules
{
    public class Interactive : InteractiveBase
    {
        private readonly CommandService _service;

        public Interactive(CommandService service)
        {
            _service = service;
        }

        // DeleteAfterAsync will send a message and asynchronously delete it after the timeout has popped
        // This method will not block.
        [Command("delete")]
        public async Task<RuntimeResult> Test_DeleteAfterAsync()
        {
            await ReplyAndDeleteAsync("I will be deleted in 10 seconds!", timeout: new TimeSpan(0, 0, 10));
            return Ok();
        }

        // NextMessageAsync will wait for the next message to come in over the gateway, given certain criteria
        // By default, this will be limited to messages from the source user in the source channel
        // This method will block the gateway, so it should be ran in async mode.
        [Command("next", RunMode = RunMode.Async)]
        public async Task Test_NextMessageAsync()
        {
            await ReplyAsync("What is 2+2?");
            var response = await NextMessageAsync(true, true, new TimeSpan(0, 0, 10));
            if (response != null)
            {
                if (response.Content == "4")
                    await ReplyAsync("Correct! The answer was 4.");
                else
                    await ReplyAsync("Wrong! the answer was 4.");
            }               
            else
                await ReplyAsync("You did not reply before the timeout!");
        }

        // PagedReplyAsync will send a paginated message to the channel
        // You can customize the paginator by creating a PaginatedMessage object
        // You can customize the criteria for the paginator as well, which defaults to restricting to the source user
        // This method will not block.
        [Command("paginator")]  
        [Summary("This will create a paginator")]
        public async Task Test_Paginator()
        {
            var pages = new[] { 
                "**Help**\n\n`//help` - Show the help command.", 
                "**Help**\n\n`//prefix` - View or change the prefix.", 
                "**Help**\n\n`//ping` - Get a reply with the current latency.",
                "**Help**\n\n`//ranks` - Lists all available ranks.\nIn order to add a rank, you can use the name or ID of the rank.",
                "**Help**\n\n`//addrank` - Add a role to the server ranks.",
                "**Help**\n\n`//delrank` - Remove a role from the server ranks.",
                "**Help**\n\n`//autorole` - List all available autoroles on the server.",
                "**Help**\n\n`//addautorole` - Add a role to the server autoroles.",
                "**Help**\n\n`//delautorole` - Remove a role from the server autoroles.",
                "**Help**\n\n`//welcome` - Set/Modify/Clear a welcome channel for the server.",
                "**Help**\n\n`//logs` - Set/Modify/Clear a logs channel for the server.",
                "**Help**\n\n`//purge (number of messges)` - Delete a set number of messages.",
                "**Help**\n\n`//rank` - Add or remove a rank from yourself.",
                "**Help**\n\n`//mute (duration in minutes)` - Mute someone for a given duration.",
                "**Help**\n\n`//unmute` - Unmute someone that has been muted.",
                "**Help**\n\n`//join` - Invite the bot to your voice channel.",
                "**Help**\n\n`//disconnect` - Remove the bot from your voice channel.",
                "**Help**\n\n`//play (song name or link)` - Play a song from YouTube by entering its name or a link.",
                "**Help**\n\n`//skip` - Skip the current track.",
                "**Help**\n\n`//pause` - Pause the current track.",
                "**Help**\n\n`//resume` - Resumes the current track.",
                "**Help**\n\n`//nowplaying` - Displays the current track that's being played.",
            };

            PaginatedMessage paginatedMessage = new PaginatedMessage()
            {
                Pages = pages,
                Options = new PaginatedAppearanceOptions()
                {
                    InformationText = "This is a list of all the commands and their descriptions."
                },
                Color = new Discord.Color(33, 176, 252),
                Title = "EssentiBot's commands"
            };
      
            await PagedReplyAsync(paginatedMessage);
        }

        [Command("help")]
        [Summary("Show the help command.")]
        public async Task Help()
        {
            List<string> Pages = new List<string>();

            foreach(var module in _service.Modules)
            {
                string page = $"**{module.Name}**\n\n";
                foreach(var command in module.Commands)
                {
                    page += $"`//{command.Name}` - {command.Summary ?? "No description provided."}\n\n";
                }
                Pages.Add(page);
            }

            await PagedReplyAsync(Pages);
        }
    }
}
