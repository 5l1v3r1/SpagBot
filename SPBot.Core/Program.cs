using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SPBot.Core
{
    class Program
    {
        private DiscordClass DiscordClass;
        private IServiceProvider Map;

        static void Main()
        {
            System.Diagnostics.ProcessStartInfo StartInfo = new System.Diagnostics.ProcessStartInfo()
            {
                Arguments = "\"" + Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\audioserve.py" + "\"",
                FileName = @"C:\Users\Administrator\AppData\Local\Programs\Python\Python36-32\python.exe",
            };
            System.Diagnostics.Process.Start(StartInfo);
            Program P = new Program();
        }

        private Program()
        {
            var DictionaryObject = new Dictionary<Discord.IVoiceChannel, AudioPlayer>();
            Map = new ServiceCollection()
            .AddSingleton(DictionaryObject)
            .BuildServiceProvider();
            DiscordClass = new DiscordClass(Map); 
            DiscordClass.MainAsync().GetAwaiter().GetResult();
        }

    }
}