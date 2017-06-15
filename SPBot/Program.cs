using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SPBot
{
    class Program
    {
        private DiscordClass DiscordClass;
        private DependencyMap Map;

        static void Main()
        {
            Program P = new Program();
        }

        private Program()
        {
            Map = new DependencyMap();
            Map.Add(new Dictionary<Discord.IVoiceChannel, AudioPlayer>());
            DiscordClass = new DiscordClass(Map); 
            DiscordClass.MainAsync().GetAwaiter().GetResult();
        }

    }
}