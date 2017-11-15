using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using Discord.Audio;
using Discord.Commands;

namespace SPBot.Core
{
    public class DiscordClass : ModuleBase
    {
        private DiscordSocketClient Client;
        private CommandService Commands;
        private IServiceProvider Map;
        private Dictionary<IVoiceChannel, AudioPlayer> ChannelTrackList;
        private List<SocketGuild> ConnectedGuilds;


        public DiscordClass(IServiceProvider MyMap)
        {
            Client = new DiscordSocketClient();
            Commands = new CommandService();
            Map = MyMap;
            ChannelTrackList = (Dictionary<IVoiceChannel, AudioPlayer>)MyMap.GetService(typeof(Dictionary<IVoiceChannel, AudioPlayer>));
        }

        public async Task MainAsync()
        {
            Console.WriteLine("We bootin' up now!");
            if (System.IO.File.Exists("token.txt") == false)
            {
                Console.WriteLine("Please create a token.txt in the EXE directory with your bot token in, and try again.");
                Console.WriteLine("Press any key to exit.");
                Console.ReadLine();
                return;
            }
            Client.MessageReceived += Client_MessageReceived;
            Client.Connected += Client_Connected;
            Client.GuildAvailable += async (guild) => Console.WriteLine(guild.Name);
            Client.JoinedGuild += async (guild) => Console.WriteLine(guild.Name);
            System.Threading.Timer RoomTimer = new System.Threading.Timer(TimerCallback, null, 0, 60000);
            Console.CancelKeyPress += async (s, ev) => { await Client.LogoutAsync();
            };
            await Commands.AddModulesAsync(System.Reflection.Assembly.GetEntryAssembly());
            string token = System.IO.File.ReadAllText("token.txt");
            await Client.LoginAsync(TokenType.Bot, token);
            await Client.StartAsync();
            await Task.Delay(-1);
        }

        private void TimerCallback(object obj)
        {
            Console.WriteLine("Begin Minutely Cleanup");
            List<IVoiceChannel> ObjectsToRemove = new List<IVoiceChannel>();
            foreach(KeyValuePair<IVoiceChannel, AudioPlayer> Items in ChannelTrackList)
            {
                SocketGuildUser CurrentUser = Client.GetGuild(Items.Key.GuildId).CurrentUser;
                var users = ((SocketVoiceChannel)Items.Key).Users;
                if (users.Count() == 1 || users.Contains(CurrentUser) == false)
                {
                    Items.Value.Dispose();
                    ObjectsToRemove.Add(Items.Key);
                }
            }
            foreach(var ObjectToRemove in ObjectsToRemove)
            {
                ChannelTrackList.Remove(ObjectToRemove);
            }
        }

        private void Player_SendMessage_Raised(string Message, IMessageChannel x)
        {
            x.SendMessageAsync(Message);
        }

        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Client.LogoutAsync();
        }

        private async Task Client_Connected()
        {
            if(Statics.HasBooted == false)
            {
                Console.WriteLine("Connected to discord as: " + Client.CurrentUser.Username);
                Statics.HasBooted = true;
            }
            await Client.SetGameAsync("+help");
        }

        private async Task Client_MessageReceived(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            int prefix = 0;
            if (message.HasCharPrefix('+', ref prefix) && (message.Channel.Name.ToLower().Contains("bot") || message.Channel.Name.ToLower().Contains("control")))
            {
                var context = new CommandContext(Client, message);
                var result = await Commands.ExecuteAsync(context, prefix, Map);
                if (!result.IsSuccess)
                    await context.Channel.SendMessageAsync(result.ErrorReason);
            }
        }

        [Command("Help", RunMode = RunMode.Async)]
        public async Task Help()
        {
            var DMToUserObject = await Context.User.GetOrCreateDMChannelAsync();
            await DMToUserObject.SendMessageAsync(Statics.HelpMessages());
        }

        [Command("Dab", RunMode = RunMode.Async)]
        public async Task Dab()
        {
            await Context.Channel.SendMessageAsync("*dabs*");
        }

        [Command("Say", RunMode = RunMode.Async)]
        public async Task Say([Remainder]string Message)
        {
            //IUser Liam = await Context.Channel.GetUserAsync(128875352444370944);
            //await Context.Channel.SendMessageAsync("This feature doesn't currently work, thanks " + Liam.Mention);
            //return;
            IUserMessage Mess = await Context.Channel.SendMessageAsync(Message, true);
            await Mess.DeleteAsync();

        }

        [Command("Play", RunMode = RunMode.Async)]
        public async Task Play(string Text)
        {
            IVoiceChannel channel = (Context.Message.Author as IGuildUser).VoiceChannel;
            if (channel == null)
            {
                await Context.Channel.SendMessageAsync("Please join a voice channel before requesting tracks! :)");
                return;
            }
            if (ChannelTrackList.Keys.Contains(channel) == false)
            {
                AudioPlayer NewPlayer = new AudioPlayer()
                {
                    MessageClient = Context.Message.Channel
                };
                NewPlayer.SendMessage_Raised += Player_SendMessage_Raised;
                ChannelTrackList.Add(channel, NewPlayer);
            }
            AudioPlayer Player = ChannelTrackList[channel];
            string PlayAudio = await Player.DoPlayAsync(Text);
            if (PlayAudio != "")
            {
                if(PlayAudio == "Song download failed :(")
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
                    if(PlayAudio.Contains("Now Playing On SpagBot:"))
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
                NewPlayer.SendMessage_Raised += Player_SendMessage_Raised;
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
