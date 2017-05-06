using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Beautiplayer;
using Discord;
using System.Diagnostics;
using Discord.Audio;
using Discord.Commands;

namespace SPBot
{
    public class DiscordClass : ModuleBase
    {
        private AudioPlayer Player;
        private DiscordSocketClient Client;
        private CommandService Commands;
        private DependencyMap Map;

        public DiscordClass(DependencyMap MyMap)
        {
            Client = new DiscordSocketClient();
            Player = MyMap.Get<AudioPlayer>();
            Commands = new CommandService();
            Map = MyMap;
        }

        public async Task MainAsync()
        {
            if(System.IO.File.Exists("token.txt") == false)
            {
                Console.WriteLine("Please create a token.txt in the EXE directory with your bot token in, and try again.");
                Console.WriteLine("Press any key to exit.");
                Console.ReadLine();
                return;
            }
            Client.MessageReceived += Client_MessageReceived;
            Player.SendMessage_Raised += Player_SendMessage_Raised;
            Client.Connected += Client_Connected;
            Client.GuildAvailable += Client_GuildAvailable;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            await Commands.AddModulesAsync(System.Reflection.Assembly.GetEntryAssembly());
            string token = System.IO.File.ReadAllText("token.txt");
            await Client.LoginAsync(TokenType.Bot, token);
            await Client.StartAsync();
            await Task.Delay(-1);
        }

        private void Player_SendMessage_Raised(string Message)
        {
            IMessageChannel x = Client.GetChannel(284040882796101632) as IMessageChannel;
            x.SendMessageAsync(Message);
        }

        private async Task Client_GuildAvailable(SocketGuild arg)
        {
            if(Statics.HasBooted == false)
            {
                Statics.HasBooted = true;
                await arg.TextChannels.Where(x => x.Name.ToLower().Contains("bot")).First().SendMessageAsync("I'm All Fired Up!");
            }
        }

        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Client.LogoutAsync();
        }

        private async Task Client_Connected()
        {
            await Client.SetGameAsync("+help");
        }

        private async Task Client_MessageReceived(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            int prefix = 0;
            if (message.HasCharPrefix('+', ref prefix) && message.Channel.Name.ToLower().Contains("bot"))
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
            var DMToUserObject = await Context.User.CreateDMChannelAsync();
            await DMToUserObject.SendMessageAsync(Statics.HelpMessages());
        }

        [Command("Play", RunMode = RunMode.Async)]
        public async Task Play(string Text)
        {
            string PlayAudio = Player.DoPlay(Text);
            if (PlayAudio != "")
            {
                IVoiceChannel channel = (Context.Message.Author as IGuildUser).VoiceChannel;
                var IsInVoiceChannel = await channel.GetUserAsync(Context.Client.CurrentUser.Id);
                if (IsInVoiceChannel == null || Player.DiscordOutStream == null)
                {
                    IAudioClient audioClient = await channel.ConnectAsync();
                    Player.EngageOutStream(audioClient);
                }
                if (PlayAudio == "MOVE")
                {
                    Object Vid = await Player.GetNext();
                    if(Vid is YoutubeExtractor.VideoInfo)
                    {
                        PlayAudio = "Now Playing On SpagBot: " + ((YoutubeExtractor.VideoInfo)Vid).Title;
                    }
                    else
                    {
                        PlayAudio = "Playing livestream on SpagBot!";
                    }
                    
                    await Context.Channel.SendMessageAsync(PlayAudio);
                    await Player.PlayNext();
                    return;
                }
                else if (PlayAudio == "QUEUE")
                {
                    object Vid = Player.Videos.Last();
                    if (Vid is YoutubeExtractor.VideoInfo)
                    {
                        PlayAudio = "Queued Up On SpagBot: " + ((YoutubeExtractor.VideoInfo)Vid).Title;
                    }
                    else
                    {
                        PlayAudio = "Stream Queued on SpagBot!";
                    }
                    
                }
                await Context.Channel.SendMessageAsync(PlayAudio);
            }
        }

        [Command("Skip", RunMode = RunMode.Async)]
        public async Task Skip()
        {
            object Vid = await Player.GetNext();
            if (Vid != null)
            {
                string NowPlaying = "";
                if (Vid is YoutubeExtractor.VideoInfo)
                {
                    NowPlaying = "Now Playing On SpagBot: " + ((YoutubeExtractor.VideoInfo)Vid).Title;
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
            Player.ClearQueue();
            return null;
        }
    }
}
