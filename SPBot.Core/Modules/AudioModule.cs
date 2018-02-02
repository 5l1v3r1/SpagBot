using Discord;
using Discord.Audio;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPBot.Core
{
    public class AudioModule : ModuleBase
    {
        private Dictionary<IVoiceChannel, AudioPlayer> ChannelTrackList;

        //Injected Constructor
        public AudioModule(Dictionary<IVoiceChannel, AudioPlayer> ChanneTrackListInjection = null)
        {
            ChannelTrackList = ChanneTrackListInjection;
        }

        [Command("Play", RunMode = RunMode.Async)]
        public async Task Play([Remainder]string Text)
        {
            IGuildUser SelectedUser = null;
            IVoiceChannel channel = (Context.Message.Author as IGuildUser).VoiceChannel;
            if (Context.Message.MentionedUserIds.Count() == 1)
            {
                ulong UserID = Context.Message.MentionedUserIds.First();
                SelectedUser = await Context.Guild.GetUserAsync(UserID);
                channel = SelectedUser?.VoiceChannel;
            }
            if (channel == null)
            {
                if (SelectedUser != null)
                {
                    await Context.Channel.SendMessageAsync(SelectedUser.Username + " isn't in a voice channel, baka!");
                }
                else
                {
                    await Context.Channel.SendMessageAsync("Please join a voice channel before requesting tracks! :)");
                }
                return;
            }
            if (ChannelTrackList.Keys.Contains(channel) == false)
            {
                AudioPlayer NewPlayer = new AudioPlayer()
                {
                    MessageClient = Context.Message.Channel
                };
                NewPlayer.SendMessage_Raised += (Message,x) => x.SendMessageAsync(Message);
                ChannelTrackList.Add(channel, NewPlayer);
            }
            AudioPlayer Player = ChannelTrackList[channel];
            string PlayAudio = await Player.DoPlayAsync(Text, (Context.Message.Author as IGuildUser).Nickname);
            if (PlayAudio != "")
            {
                if (PlayAudio == "Song download failed :(")
                {
                    await Context.Channel.SendMessageAsync(PlayAudio);
                    return;
                }
                var IsInVoiceChannel = await channel.GetUserAsync(Context.Client.CurrentUser.Id);
                if (IsInVoiceChannel == null || Player.DiscordOutStream == null)
                {
                    IAudioClient audioClient = await channel.ConnectAsync();
                    Player.EngageOutStream(audioClient);
                }
                VideoInfo Vid = null;
                if (PlayAudio == "MOVE")
                {
                    Vid = Player.GetNext();
                    PlayAudio = "Now Playing On SpagBot: " + Vid.Title;
                }
                else if (PlayAudio == "QUEUE")
                {
                    Vid = Player.Videos.Last();
                    PlayAudio = "Queued Up On SpagBot: " + Vid.Title;
                }
                if (Vid != null)
                {
                    Console.WriteLine(Context.Message.Author.Username + " has requested " + Vid.Title);
                    await Context.Message.DeleteAsync();
                    await Context.Channel.SendMessageAsync(PlayAudio);
                    if (PlayAudio.Contains("Now Playing On SpagBot:"))
                        await Player.PlayNext(true);
                }
            }
        }

        [Command("Repeat", RunMode = RunMode.Async)]
        public async Task Repeat()
        {
            IVoiceChannel channel = (Context.Message.Author as IGuildUser).VoiceChannel;
            if (ChannelTrackList.Keys.Contains(channel) == false)
            {
                return;
            }
            AudioPlayer Player = ChannelTrackList[channel];
            string Message = "";
            if (Player.ToggleRepeat())
            {
                Message = "Repeat has been enabled!";
            }
            else
            {
                Message = "Repeat has been disabled!";
            }
            await Context.Channel.SendMessageAsync(Message);
        }

        [Command("Radio", RunMode = RunMode.Async)]
        public async Task Radio()
        {
            IVoiceChannel channel = (Context.Message.Author as IGuildUser).VoiceChannel;
            if (channel == null)
            {
                await Context.Channel.SendMessageAsync("Please join a voice channel before requesting radio! :)");
                return;
            }
            if (ChannelTrackList.Keys.Contains(channel) == false)
            {
                AudioPlayer NewPlayer = new AudioPlayer()
                {
                    MessageClient = Context.Message.Channel
                };
                NewPlayer.SendMessage_Raised += (Message, x) => x.SendMessageAsync(Message);
                ChannelTrackList.Add(channel, NewPlayer);
            }
            AudioPlayer Player = ChannelTrackList[channel];
            var IsInVoiceChannel = await channel.GetUserAsync(Context.Client.CurrentUser.Id);
            if (IsInVoiceChannel == null || Player.DiscordOutStream == null)
            {
                IAudioClient audioClient = await channel.ConnectAsync();
                Player.EngageOutStream(audioClient);
            }
            await Player.PlayRadio();
        }

        [Command("Skip", RunMode = RunMode.Async)]
        public async Task Skip()
        {
            IVoiceChannel channel = (Context.Message.Author as IGuildUser).VoiceChannel;
            if (ChannelTrackList.Keys.Contains(channel) == false)
            {
                return;
            }
            AudioPlayer Player = ChannelTrackList[channel];
            VideoInfo Vid = Player.GetNext();
            if (Vid != null)
            {
                string NowPlaying = "";
                if (Vid is VideoInfo)
                {
                    NowPlaying = "Now Playing On SpagBot: " + ((VideoInfo)Vid).Title;
                }
                else
                {
                    NowPlaying = "Playing livestream on SpagBot!";
                }
                await Context.Channel.SendMessageAsync(NowPlaying);
            }
            await Player.PlayNext(true);
        }

        [Command("Clear", RunMode = RunMode.Async)]
        public Task Clear()
        {
            IVoiceChannel channel = (Context.Message.Author as IGuildUser).VoiceChannel;
            if (ChannelTrackList.Keys.Contains(channel) == false)
            {
                return Task.CompletedTask;
            }
            AudioPlayer Player = ChannelTrackList[channel];
            Player.ClearQueue();
            return Task.CompletedTask;
        }
    }
}
