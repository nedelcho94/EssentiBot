using Discord;
using Discord.Commands;
using Discord.WebSocket;
using EssentiBot.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;

namespace EssentiBot.Modules
{
    public class Music : ModuleBase<SocketCommandContext>
    {
        private readonly LavaNode _lavaNode;
        private readonly DiscordSocketClient _client;

        public Music(LavaNode lavaNode, DiscordSocketClient client)
        {
            _lavaNode = lavaNode;
            _client = client;
        }

        [Command("join", RunMode = RunMode.Async)]
        [Summary("Invites the bot to your voice channel.")]
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
        [Summary("Removes the bot from your voice channel.")]
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
        [Summary("Play a song from YouTube by entering its name or link.")]
        public async Task PlayAsync([Remainder]string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
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

            if (!searchQuery.StartsWith("http"))
            {
                var searchResponse = await _lavaNode.SearchYouTubeAsync(searchQuery);
                if (searchResponse.LoadStatus == LoadStatus.LoadFailed ||
                    searchResponse.LoadStatus == LoadStatus.NoMatches)
                {
                    await Context.Channel.SendErrorAsync("Error!", $"I wasn't able to find anything for `{searchQuery}`.");
                    return;
                }


                var player = _lavaNode.GetPlayer(Context.Guild);

                if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
                {
                    if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
                    {
                        foreach (var track in searchResponse.Tracks)
                        {
                            player.Queue.Enqueue(track);
                        }

                        //await ReplyAsync($"Enqueued {searchResponse.Tracks.Count} tracks.");
                        await Context.Channel.SendSuccessAsync("Queued!", $"Enqueued {searchResponse.Tracks.Count} tracks.");
                    }
                    else
                    {
                        var track = searchResponse.Tracks[0];
                        player.Queue.Enqueue(track);
                        await Context.Channel.SendSuccessAsync("Queued!", $"Enqueued: `{track.Title}`");
                    }
                }
                else
                {
                    var track = searchResponse.Tracks[0];
                    if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
                    {
                        for (var i = 0; i < searchResponse.Tracks.Count; i++)
                        {
                            if (i == 0)
                            {
                                await player.PlayAsync(track);
                                await Context.Channel.SendSuccessAsync("Playing!", $"Now Playing: `{track.Title}`");
                                await _client.SetGameAsync($"{track.Title}", null, ActivityType.Listening);
                            }
                            else
                            {
                                player.Queue.Enqueue(searchResponse.Tracks[i]);
                            }
                        }
                        await Context.Channel.SendSuccessAsync("Queued!", $"Enqueued {searchResponse.Tracks.Count} tracks.");
                    }
                    else
                    {
                        await player.PlayAsync(track);
                        await Context.Channel.SendSuccessAsync("Playing!", $"Now Playing: `{track.Title}`");
                        await _client.SetGameAsync($"{track.Title}", null, ActivityType.Listening);
                    }
                }
                
            }
            else
            {
                var queries = searchQuery.Split(' ');
                foreach (var query in queries)
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
                        if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
                        {
                            foreach (var track in searchResponse.Tracks)
                            {
                                player.Queue.Enqueue(track);
                            }

                            //await ReplyAsync($"Enqueued {searchResponse.Tracks.Count} tracks.");
                            await Context.Channel.SendSuccessAsync("Queued!", $"Enqueued {searchResponse.Tracks.Count} tracks.");
                        }
                        else
                        {
                            var track = searchResponse.Tracks[0];
                            player.Queue.Enqueue(track);
                            await Context.Channel.SendSuccessAsync("Queued!", $"Enqueued: `{track.Title}`");
                        }
                    }
                    else
                    {
                        var track = searchResponse.Tracks[0];
                        if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
                        {
                            for (var i = 0; i < searchResponse.Tracks.Count; i++)
                            {
                                if (i == 0)
                                {
                                    await player.PlayAsync(track);
                                    await Context.Channel.SendSuccessAsync("Playing!", $"Now Playing: `{track.Title}`");
                                    await _client.SetGameAsync($"{track.Title}", null, ActivityType.Listening);
                                }
                                else
                                {
                                    player.Queue.Enqueue(searchResponse.Tracks[i]);
                                }
                            }
                            await Context.Channel.SendSuccessAsync("Queued!", $"Enqueued {searchResponse.Tracks.Count} tracks.");
                        }
                        else
                        {
                            await player.PlayAsync(track);
                            await Context.Channel.SendSuccessAsync("Playing!", $"Now Playing: `{track.Title}`");
                            await _client.SetGameAsync($"{track.Title}", null, ActivityType.Listening);
                        }
                    }
                }
            }
        }
       
        [Command("skip", RunMode = RunMode.Async)]
        [Summary("Skip the current track.")]
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
            await _client.SetGameAsync($"{player.Track.Title}", null, ActivityType.Listening);
        }

        [Command("pause", RunMode = RunMode.Async)]
        [Summary("Pause the current track.")]
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
        [Summary("Resumes the current track.")]
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
            await _client.SetGameAsync($"{player.Track.Title}", null, ActivityType.Listening);
        }

        [Command("nowplaying", RunMode = RunMode.Async)]
        [Alias("np")]
        [Summary("Displays the current track that's being played.")]
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
            await _client.SetGameAsync($"{player.Track.Title}", null, ActivityType.Listening);
        }

        [Command("volume")]
        public async Task Volume(ushort vol)
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

            if(vol > 150 || vol <= 2)
            {
                await Context.Channel.SendErrorAsync("Error!", "Please use a number between 2 - 150");
                return;
            }
            await player.UpdateVolumeAsync(vol);
            await Context.Channel.SendSuccessAsync("Success!", $"Volume set to {vol}!");
        }
    }
}
