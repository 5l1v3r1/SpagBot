using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using Discord.Audio;
using Discord.Commands;
using System.Net;
using System.Xml.Linq;

namespace SPBot.Core
{
    public class NSFWModule : ModuleBase
    {
        [Command("clearchat", RunMode = RunMode.Async)]
        public async Task ClearChat(int limit = 150)
        {
            var messages = await Context.Channel.GetMessagesAsync(limit).Flatten();
            foreach (IMessage Msg in messages) await Msg.DeleteAsync();
        }

        [Command("clearchatnsfw", RunMode = RunMode.Async)]
        public async Task ClearChatNSFW(int limit = 150)
        {
            var nsfwchan = await Context.Guild.GetTextChannelAsync(380861058690056192);
            var messages = await nsfwchan.GetMessagesAsync(limit).Flatten();
            foreach (IMessage Msg in messages) await Msg.DeleteAsync();
        }

        [Command("rule34", RunMode = RunMode.Async)]
        public async Task Rule34(string SearchQuery)
        {
            var NSFWChannel = (await Context.Guild.GetTextChannelsAsync()).Cast<IMessageChannel>().FirstOrDefault(x => x.Id == 380861058690056192);
            if (NSFWChannel == null)
            {
                NSFWChannel = Context.Channel;
            }
            string url = "https://rule34.xxx/index.php?page=dapi&s=post&q=index&tags=" + SearchQuery;
            using (WebClient WC = new WebClient())
            {
                string response = await WC.DownloadStringTaskAsync(new Uri(url));
                XDocument XDoc = XDocument.Parse(response);
                if (int.TryParse(XDoc.Root.Attribute("count").Value, out int count))
                {
                    if (count > 99) count = 99;
                    if(count > 0)
                    {
                        int rand = new Random().Next(0, count);
                        var RandomItem = XDoc.Root.Elements("post").ToArray()[rand];
                        string tags = RandomItem.Attribute("tags").Value;
                        string img = RandomItem.Attribute("file_url").Value;
                        string filename = img.Substring(img.LastIndexOf("/") + 2);
                        await WC.DownloadFileTaskAsync(img, filename);
                        await NSFWChannel.SendFileAsync(filename, tags);
                        if(NSFWChannel.Id != Context.Channel.Id)
                            await Context.Channel.SendMessageAsync("Posted to: " + NSFWChannel.Name);
                        System.IO.File.Delete(filename);
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync("No results found that match: '" + SearchQuery + "'");
                    }
                }

            }
        }
    }
}
