using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;
using System.Threading.Tasks;
using System.Linq;
using Discord;

namespace SPBot.Core
{
    public class MiscModule : ModuleBase
    {
        [Command("Dab", RunMode = RunMode.Async)]
        public async Task Dab([Remainder]string Message = "")
        {
            if (Message.Trim() == "")
            {
                await Context.Channel.SendMessageAsync("*dabs*");
            }
            else if (Message.ToLower().Contains("on"))
            {
                var UserContextId = Context.Message.MentionedUserIds.FirstOrDefault();
                if (UserContextId > 0)
                {
                    if (UserContextId == 299586375752089600) UserContextId = Context.User.Id; // don't dab on self
                    await Context.Channel.SendMessageAsync("*Dabs on " + (await Context.Channel.GetUserAsync(UserContextId)).Mention + "*");
                }
                else if (Message.ToLower().Trim() == "on @everyone")
                {
                    await Context.Channel.SendMessageAsync("*Dabs on " + Context.Guild.EveryoneRole.Mention + "*");
                }
                else
                {
                    await Context.Channel.SendMessageAsync("Who am I dabbing on? :thinking:");
                }

            }
        }

        [Command("Say", RunMode = RunMode.Async)]
        public async Task Say([Remainder]string Message)
        {
            IUserMessage Mess = await Context.Channel.SendMessageAsync(Message, true);
            await Mess.DeleteAsync();

        }
    }
}
