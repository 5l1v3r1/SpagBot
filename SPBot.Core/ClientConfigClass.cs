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
    public class ClientConfigClass
    {
        public static string ytauth = "";

        private DiscordSocketClient _Client;
        private CommandService _Commands;
        private IServiceProvider _Map;
        private Dictionary<IGuild, ITextChannel> _GuildBotchannels;
        private Dictionary<IVoiceChannel, AudioPlayer> _ChannelTrackList;
        private bool _FirstTimeConnection = true;

        public ClientConfigClass(IServiceProvider MyMap)
        {
            _Client = new DiscordSocketClient();
            _Commands = new CommandService();
            _Map = MyMap;
            _ChannelTrackList = (Dictionary<IVoiceChannel, AudioPlayer>)MyMap.GetService(typeof(Dictionary<IVoiceChannel, AudioPlayer>));
            _GuildBotchannels = (Dictionary<IGuild, ITextChannel>)MyMap.GetService(typeof(Dictionary<IGuild, ITextChannel>));
        }

        public async Task MainAsync()
        {
            _Client.MessageReceived += Client_MessageReceived;
            _Client.Connected += Client_Connected;
            _Client.GuildAvailable += async (guild) => await DoGuildJoining(guild);
            _Client.JoinedGuild += async (guild) => await DoGuildJoining(guild);
            await _Commands.AddModulesAsync(System.Reflection.Assembly.GetEntryAssembly());
            string[] tokens = System.IO.File.ReadAllLines("token.txt");
            string token = tokens[0];
            Console.WriteLine("Token in use: " + token);
            if(tokens.Count() > 1)
            {
                ytauth = tokens[1];
                Console.WriteLine("API Token Provided: " + ytauth);
            }
            Console.WriteLine("Token Obtained Successfully, Attempting To Log In.");
            await _Client.LoginAsync(TokenType.Bot, token);
            await _Client.StartAsync();
            Console.WriteLine("Successfully Logged In.");
            await Task.Delay(-1);
        }

        private async Task DoGuildJoining(SocketGuild guild)
        {
            if (_GuildBotchannels.Keys.Contains(guild) == false)
            {
                var TxtChan = guild.TextChannels.FirstOrDefault(x => x.Name.ToLower().Contains("bot") || x.Name.ToLower().Contains("control"));
                if (TxtChan != null)
                {
                    _GuildBotchannels.Add(guild, TxtChan);
                }
                else
                {
                    SocketTextChannel SelectedChan = guild.TextChannels.First();
                    _GuildBotchannels.Add(guild, SelectedChan);
                    await SelectedChan.SendMessageAsync("Nyaa~~ I'll be using this channel to listen to your commands!");
                }
                Console.WriteLine("Connected to: " + guild);
                var me = guild.GetUser(_Client.CurrentUser.Id);
                await me.ModifyAsync(x => x.Nickname = me.Username);
            }
        }

        private void TimerCallback(object obj)
        {
            List<IVoiceChannel> ObjectsToRemove = new List<IVoiceChannel>();
            foreach(KeyValuePair<IVoiceChannel, AudioPlayer> Items in _ChannelTrackList)
            {
                SocketGuildUser CurrentUser = _Client.GetGuild(Items.Key.GuildId).CurrentUser;
                var users = ((SocketVoiceChannel)Items.Key).Users;
                if (users.Count() == 1 || users.Contains(CurrentUser) == false)
                {
                    Items.Value.Dispose();
                    ObjectsToRemove.Add(Items.Key);
                }
            }
            foreach(var ObjectToRemove in ObjectsToRemove)
            {
                _ChannelTrackList.Remove(ObjectToRemove);
            }
        }

        private async Task Client_Connected()
        {
            if(_FirstTimeConnection)
            {
                Console.WriteLine("Connected to discord as: " + _Client.CurrentUser.Username);
                System.Threading.Timer RoomTimer = new System.Threading.Timer(TimerCallback, null, 0, 60000);
                _FirstTimeConnection = false;
            }
            await _Client.SetGameAsync("+help");
        }

        private async Task Client_MessageReceived(SocketMessage arg)
        {
            var UserMessage = arg as SocketUserMessage;
            var GuildChannel = arg.Channel as SocketGuildChannel;
            int prefix = 0;
            if (UserMessage.HasCharPrefix('+', ref prefix) && _GuildBotchannels[GuildChannel.Guild]?.Id == UserMessage.Channel.Id)
            {
                var context = new CommandContext(_Client, UserMessage);
                var result = await _Commands.ExecuteAsync(context, prefix, _Map);
                if (!result.IsSuccess)
                    await context.Channel.SendMessageAsync(result.ErrorReason);
            }
        }
    }
}
