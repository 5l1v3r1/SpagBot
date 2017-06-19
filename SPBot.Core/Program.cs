using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;


namespace SPBot.Core
{
    class Program
    {
        private DiscordClass DiscordClass;
        private IServiceProvider Map;

        static void Main()
        {
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