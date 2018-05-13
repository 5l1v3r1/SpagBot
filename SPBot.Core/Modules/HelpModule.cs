using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPBot.Core
{
    public class HelpModule : ModuleBase
    {
        private CommandService _service;

        public HelpModule(CommandService service)
        {
            _service = service;
        }

        [Command("Help", RunMode = RunMode.Async)]
        public async Task Help()
        {
            var DMToUserObject = await Context.User.GetOrCreateDMChannelAsync();
            string prefix = "+";
            var builder = new EmbedBuilder()
            {
                Color = new Color(114, 137, 218),
                Description = "These are the commands you can use"
            };

            foreach (var module in _service.Modules)
            {
                string description = null;
                foreach (var cmd in module.Commands)
                {
                    var result = await cmd.CheckPreconditionsAsync(Context);
                    if (result.IsSuccess)
                        description += $"{prefix}{cmd.Aliases.First()}\n";
                }

                if (!string.IsNullOrWhiteSpace(description))
                {
                    builder.AddField(x =>
                    {
                        x.Name = module.Name;
                        x.Value = description;
                        x.IsInline = false;
                    });
                }
            }
            await DMToUserObject.SendMessageAsync("",false, builder.Build());
        }
    }
}
