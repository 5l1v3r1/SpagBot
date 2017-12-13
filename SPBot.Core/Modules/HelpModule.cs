using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SPBot.Core
{
    public class HelpModule : ModuleBase
    {
        [Command("Help", RunMode = RunMode.Async)]
        public async Task Help()
        {
            var DMToUserObject = await Context.User.GetOrCreateDMChannelAsync();
            await DMToUserObject.SendMessageAsync(Statics.HelpMessages());
        }
    }
}
