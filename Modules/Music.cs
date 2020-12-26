using Discord;
using Discord.Commands;
using EssentiBot.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;

namespace EssentiBot.Modules
{
    public class Music : ModuleBase<SocketCommandContext>
    {
        private readonly LavaNode _lavaNode;

        public Music(LavaNode lavaNode)
        {
            _lavaNode = lavaNode;
        }

        [Command("join", RunMode = RunMode.Async)]
        public async Task JoinAsync()
        {
            if (_lavaNode.HasPlayer(Context.Guild))
            {
                //await ReplyAsync("I'm already connected to a voice channel!");
                await Context.Channel.SendErrorAsync("Already connected!", "I'm already connected to a voice channel!");
                return;
            }

            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                //await ReplyAsync("You must be connected to a voice channel!");
                await Context.Channel.SendErrorAsync("Error!", "You must be connected to a voice channel!");
                return;
            }

            try
            {
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
                await Context.Channel.SendSuccessAsync("Joined!", $"Joined `{voiceState.VoiceChannel.Name}`");
            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
            }
        }

        [Command("disconnect", RunMode = RunMode.Async)]
        public async Task DisconnectAsync()
        {
            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                //await ReplyAsync("I'm already connected to a voice channel!");
                await Context.Channel.SendErrorAsync("Error!", "I'm not connected to a voice channel!");
                return;
            }

            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                //await ReplyAsync("You must be connected to a voice channel!");
                await Context.Channel.SendErrorAsync("Error!", "You must be connected to a voice channel!");
                return;
            }

            try
            {
                await _lavaNode.LeaveAsync(voiceState.VoiceChannel);
                await Context.Channel.SendSuccessAsync("Disconnected!", $"Disconnected from `{voiceState.VoiceChannel.Name}`");
            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
            }
        }

        [Command("play", RunMode = RunMode.Async)]
        [Alias("p")]
        public async Task PlayAsync([Remainder]string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                //await ReplyAsync("Please provide search terms.");
                await Context.Channel.SendErrorAsync("No search terms!", "Please provide search terms!");
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await Context.Channel.SendErrorAsync("Error!", "I'm not connected to a voice channel!");
                return;
            }

            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await Context.Channel.SendErrorAsync("Error!", "You must be connected to a voice channel!");
                return;
            }

            if (!query.StartsWith("http"))
            {


                var searchResponse = await _lavaNode.SearchYouTubeAsync(query);
                if (searchResponse.LoadStatus == LoadStatus.LoadFailed ||
                    searchResponse.LoadStatus == LoadStatus.NoMatches)
                {
                    await Context.Channel.SendErrorAsync("Error!", $"I wasn't able to find anything for `{query}`.");
                    return;
                }

                var player = _lavaNode.GetPlayer(Context.Guild);

                if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
                {
                    var track = searchResponse.Tracks[0];
                    player.Queue.Enqueue(track);
                    //await ReplyAsync($"Enqueued: {track.Title}");
                    await Context.Channel.SendSuccessAsync("Queued!", $"Enqueued: `{track.Title}`");
                }
                else
                {
                    var track = searchResponse.Tracks[0];

                    await player.PlayAsync(track);
                    //var artwork = await track.FetchArtworkAsync();
                    await Context.Channel.SendSuccessAsync("Playing!", $"Now Playing: `{track.Title}`");
                }
            }
            else
            {
                var searchResponse = await _lavaNode.SearchAsync(query);
                if (searchResponse.LoadStatus == LoadStatus.LoadFailed ||
                    searchResponse.LoadStatus == LoadStatus.NoMatches)
                {
                    await Context.Channel.SendErrorAsync("Error!", $"I wasn't able to find anything for `{query}`.");
                    return;
                }

                var player = _lavaNode.GetPlayer(Context.Guild);

                if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
                {
                    var track = searchResponse.Tracks[0];
                    player.Queue.Enqueue(track);
                    //await ReplyAsync($"Enqueued: {track.Title}");
                    await Context.Channel.SendSuccessAsync("Queued!", $"Enqueued: `{track.Title}`");
                }
                else
                {
                    var track = searchResponse.Tracks[0];

                    await player.PlayAsync(track);
                    //var artwork = await track.FetchArtworkAsync();
                    await Context.Channel.SendSuccessAsync("Playing!", $"Now Playing: `{track.Title}`");
                }
            }
        }
       
        [Command("skip", RunMode = RunMode.Async)]
        public async Task Skip()
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                //await ReplyAsync("You must be connected to a voice channel!");
                await Context.Channel.SendErrorAsync("Error!", "You must be connected to a voice channel!");
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                //await ReplyAsync("I'm not connected to a voice channel.");
                await Context.Channel.SendErrorAsync("Error!", "I'm not connected to a voice channel!");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);
            if (voiceState.VoiceChannel != player.VoiceChannel)
            {
                await Context.Channel.SendErrorAsync("Error!", "You must be in the same voice channel as me!");
                return;
            }

            if(player.Queue.Count == 0)
            {
                await Context.Channel.SendErrorAsync("Empty Queue!", "There are no more songs in the queue!");
                return;
            }

            await player.SkipAsync();
            await Context.Channel.SendSuccessAsync("Skipped!", $"Now playing: `{player.Track.Title}`!");
        }

        [Command("pause", RunMode = RunMode.Async)]
        public async Task Pause()
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                //await ReplyAsync("You must be connected to a voice channel!");
                await Context.Channel.SendErrorAsync("Error!", "You must be connected to a voice channel!");
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                //await ReplyAsync("I'm not connected to a voice channel.");
                await Context.Channel.SendErrorAsync("Error!", "I'm not connected to a voice channel!");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);
            if (voiceState.VoiceChannel != player.VoiceChannel)
            {
                await Context.Channel.SendErrorAsync("Error!", "You must be in the same voice channel as me!");
                return;
            }
            
            if(player.PlayerState == PlayerState.Paused || player.PlayerState == PlayerState.Stopped)
            {
                await Context.Channel.SendErrorAsync("Error!", "The music is already paused!");
                return;
            }

            await player.PauseAsync();
            await Context.Channel.SendSuccessAsync("Paused!", $"Paused the track: `{player.Track.Title}`!");
        }

        [Command("resume", RunMode = RunMode.Async)]
        public async Task Resume()
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                //await ReplyAsync("You must be connected to a voice channel!");
                await Context.Channel.SendErrorAsync("Error!", "You must be connected to a voice channel!");
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                //await ReplyAsync("I'm not connected to a voice channel.");
                await Context.Channel.SendErrorAsync("Error!", "I'm not connected to a voice channel!");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);
            if (voiceState.VoiceChannel != player.VoiceChannel)
            {
                await Context.Channel.SendErrorAsync("Error!", "You must be in the same voice channel as me!");
                return;
            }

            if (player.PlayerState == PlayerState.Playing)
            {
                await Context.Channel.SendErrorAsync("Error!", "The music is already playing!");
                return;
            }

            await player.ResumeAsync();
            await Context.Channel.SendSuccessAsync("Resumed!", $"Resumed the track: `{player.Track.Title}`!");
        }

        [Command("nowplaying", RunMode = RunMode.Async)]
        [Alias("np")]
        public async Task NowPlaying()
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                //await ReplyAsync("You must be connected to a voice channel!");
                await Context.Channel.SendErrorAsync("Error!", "You must be connected to a voice channel!");
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                //await ReplyAsync("I'm not connected to a voice channel.");
                await Context.Channel.SendErrorAsync("Error!", "I'm not connected to a voice channel!");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);
            if (voiceState.VoiceChannel != player.VoiceChannel)
            {
                await Context.Channel.SendErrorAsync("Error!", "You must be in the same voice channel as me!");
                return;
            }

            await Context.Channel.SendSuccessAsync("Now playing:", $"`{player.Track.Title}`");
        }
    }
}
