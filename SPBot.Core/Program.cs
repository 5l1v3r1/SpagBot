using Discord;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SPBot.Core
{
    class Program
    {
        private DiscordClass DiscordClass;
        private IServiceProvider Map;

        static void Main()
        {
            string FileName = @"C:\Users\Administrator\AppData\Local\Programs\Python\Python36-32\python.exe";
            string Args = "\"" + Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\audioserve.py" + "\"";
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                FileName = "python";
                Args = "\"" + Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/audioserve.py" + "\""; //forward slash!
            }
            System.Diagnostics.ProcessStartInfo StartInfo = new System.Diagnostics.ProcessStartInfo()
            {
                Arguments = Args,
                FileName = FileName,
            };
            System.Diagnostics.Process.Start(StartInfo);
            Program P = new Program();
        }

        private Program()
        {
            var DictionaryObject = new Dictionary<Discord.IVoiceChannel, AudioPlayer>();
            Dictionary<IGuild, ITextChannel> GuildBotchannels = new Dictionary<IGuild, ITextChannel>();
            Map = new ServiceCollection()
            .AddSingleton(DictionaryObject)
            .AddSingleton(GuildBotchannels)
            .BuildServiceProvider();
            DiscordClass = new DiscordClass(Map); 
            DiscordClass.MainAsync().GetAwaiter().GetResult();
        }

    }
}