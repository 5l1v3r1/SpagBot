using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;
using System.Threading.Tasks;
using System.Linq;
using Discord;

namespace SPBot.Core
{
    public class UrbanModule : ModuleBase
    {
        [Command("urban", RunMode = RunMode.Async)]
        public async Task urban([Remainder]string urbanstr)
        {
            using (System.Net.WebClient WebClient = new System.Net.WebClient())
            {
                string Resp = await WebClient.DownloadStringTaskAsync("http://api.urbandictionary.com/v0/define?term=" + urbanstr);
                Newtonsoft.Json.Linq.JToken JsonContent = Newtonsoft.Json.Linq.JToken.Parse(Resp);
                JsonContent = JsonContent.SelectToken("list.[0].definition");
                if (JsonContent != null)
                    await Context.Channel.SendMessageAsync(JsonContent.ToString());
                else
                    await Context.Channel.SendMessageAsync("No definition found for: " + urbanstr);

            }
        }
    }
}
